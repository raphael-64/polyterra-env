using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Polytopia.Data;
using PolytopiaBackendBase.Game;

/// <summary>
/// Bridge between C# game logic and Python PettingZoo environment
/// Communicates via JSON over stdin/stdout
/// </summary>
public class PolyterraEnvBridge
{
    private GameState gameState;
    private ActionManager actionManager;
    private int maxTurns;
    private int currentSeed;

    public PolyterraEnvBridge(int maxTurns = 100)
    {
        this.maxTurns = maxTurns;
    }

    /// <summary>
    /// Process JSON command and return JSON response
    /// </summary>
    public string ProcessCommand(string jsonCommand)
    {
        try
        {
            var request = JsonSerializer.Deserialize<Request>(jsonCommand);

            return request.command switch
            {
                "reset" => HandleReset(request),
                "step" => HandleStep(request),
                "observe" => HandleObserve(request),
                "get_state" => HandleGetState(request),
                "close" => HandleClose(),
                _ => JsonSerializer.Serialize(new Response { success = false, error = $"Unknown command: {request.command}" })
            };
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new Response
            {
                success = false,
                error = $"Error processing command: {ex.Message}\n{ex.StackTrace}"
            });
        }
    }

    private string HandleReset(Request request)
    {
        int numPlayers = request.num_players ?? 4;
        int seed = request.seed ?? new Random().Next();
        string gameMode = request.game_mode ?? "perfection";

        currentSeed = seed;

        // Initialize game data
        GameDataLoader.InitializeDataManager();

        // Create game state
        gameState = new GameState();
        gameState.Seed = seed;
        gameState.Version = 100;

        // Set game mode
        var mode = gameMode.ToLower() == "domination"
            ? GameMode.Domination
            : GameMode.Perfection;

        gameState.Settings = new GameSettings
        {
            Difficulty = GameSettings.Difficulties.Normal,
            BaseGameMode = mode,
            RulesGameMode = mode
        };

        if (mode == GameMode.Domination)
        {
            gameState.Settings.OpponentCount = numPlayers - 1;
        }

        // Create player states
        gameState.PlayerStates = new List<PlayerState>();

        // IMPORTANT: Add real players FIRST (indices 0-3) because MapGenerator expects
        // PlayerStates[0..PlayerCount-1] to be real players when assigning capitals
        var tribes = new[] { TribeData.Type.Imperius, TribeData.Type.Bardur,
                            TribeData.Type.Oumaji, TribeData.Type.Xinxi };

        for (byte i = 1; i <= numPlayers; i++)
        {
            gameState.PlayerStates.Add(new PlayerState
            {
                Id = i,
                UserName = $"player_{i-1}",  // 0-indexed for Python
                tribe = tribes[(i - 1) % tribes.Length],
                AutoPlay = false,  // Controlled by RL agents
                hasChosenTribe = true,
                handicap = 1,
                Currency = 0,
                aiState = new AIState()
            });
        }

        // Add Nature player LAST (after real players)
        gameState.PlayerStates.Add(new PlayerState
        {
            Id = byte.MaxValue,
            UserName = "Nature",
            tribe = TribeData.Type.Nature,
            AutoPlay = false,
            hasChosenTribe = true
        });

        // Generate map
        int mapSize = gameState.Settings.MapSize;
        gameState.Map = new MapData((ushort)mapSize, (ushort)mapSize);

        var mapSettings = new MapGeneratorSettings
        {
            wetness = 0.55f,
            richness = 1.0f,
            equalityIterations = 5,
            equalityLimit = 0.15f
        };

        var mapGenerator = new MapGenerator();
        mapGenerator.GenerateWithSeed(seed, gameState, mapSettings);

        GameStateUtils.SetPlayerColors(gameState);

        // Initialize action manager
        actionManager = new ActionManager(gameState);

        // Start match
        gameState.CurrentState = GameState.State.Lobby;
        var startCommand = new StartMatchCommand(gameState.PlayerStates[0].Id);  // First real player
        actionManager.ExecuteCommand(startCommand, out _);

        gameState.CurrentState = GameState.State.Started;
        gameState.CurrentTurn = 1;
        gameState.CurrentPlayerIndex = 1;  // First real player (index 1, skip Nature at 0)

        // Return initial observations
        return JsonSerializer.Serialize(new Response
        {
            success = true,
            observations = GetAllObservations(),
            agents = GetActiveAgents(),
            agent_selection = GetCurrentAgent(),
            rewards = GetRewards(),
            terminations = GetTerminations(),
            truncations = GetTruncations(),
            info = new Dictionary<string, object>
            {
                ["seed"] = seed,
                ["map_size"] = mapSize,
                ["num_players"] = numPlayers
            }
        });
    }

    private string HandleStep(Request request)
    {
        if (gameState == null)
        {
            return JsonSerializer.Serialize(new Response
            {
                success = false,
                error = "Game not initialized. Call reset first."
            });
        }

        var playerIndex = gameState.CurrentPlayerIndex;
        var player = gameState.PlayerStates[playerIndex];

        // Parse and execute command
        CommandBase command;

        if (request.action_type == "end_turn")
        {
            command = new EndTurnCommand(player.Id);
        }
        else
        {
            // For now, just support end_turn
            // TODO: Implement full action parsing
            command = new EndTurnCommand(player.Id);
        }

        bool success = actionManager.ExecuteCommand(command, out string error);

        if (!success)
        {
            return JsonSerializer.Serialize(new Response
            {
                success = false,
                error = error
            });
        }

        // Check if game ended
        bool gameEnded = IsGameOver();

        // Get rewards (delta in score)
        var rewards = GetRewards();

        // NOTE: Don't manually advance player - EndTurnAction already calls state.EndPlayerTurn()

        return JsonSerializer.Serialize(new Response
        {
            success = true,
            observations = GetAllObservations(),
            rewards = rewards,
            terminations = GetTerminations(),
            truncations = GetTruncations(),
            agents = GetActiveAgents(),
            agent_selection = GetCurrentAgent(),
            info = new Dictionary<string, object>
            {
                ["current_turn"] = gameState.CurrentTurn
            }
        });
    }

    private string HandleObserve(Request request)
    {
        if (gameState == null)
        {
            return JsonSerializer.Serialize(new Response
            {
                success = false,
                error = "Game not initialized"
            });
        }

        string agent = request.agent ?? GetCurrentAgent();
        var observation = GetObservation(agent);

        return JsonSerializer.Serialize(new Response
        {
            success = true,
            observation = observation
        });
    }

    private string HandleGetState(Request request)
    {
        return JsonSerializer.Serialize(new Response
        {
            success = true,
            state = new Dictionary<string, object>
            {
                ["current_turn"] = gameState?.CurrentTurn ?? 0,
                ["current_player"] = gameState?.CurrentPlayerIndex ?? 0,
                ["game_state"] = gameState?.CurrentState.ToString() ?? "Unknown",
                ["agents"] = GetActiveAgents()
            }
        });
    }

    private string HandleClose()
    {
        gameState = null;
        actionManager = null;

        return JsonSerializer.Serialize(new Response { success = true });
    }

    private Dictionary<string, object> GetObservation(string agentName)
    {
        // Extract player index from agent name (e.g., "player_0" -> 0)
        // Since we reordered PlayerStates to have real players first, the index matches directly
        int playerIdx = int.Parse(agentName.Split('_')[1]);
        var player = gameState.PlayerStates[playerIdx];

        // Get available techs
        var available_techs = new List<string>();
        if (player.availableTech != null)
        {
            foreach (var tech in player.availableTech)
            {
                available_techs.Add(tech.ToString());
            }
        }

        // Create observation
        var obs = new Dictionary<string, object>
        {
            ["turn"] = gameState.CurrentTurn,
            ["current_player_idx"] = gameState.CurrentPlayerIndex - 1, // 0-indexed

            // Player info
            ["player_id"] = player.Id,
            ["currency"] = player.Currency,
            ["score"] = player.score,
            ["tribe"] = player.tribe.ToString(),
            ["available_techs"] = available_techs,
            ["cities"] = player.cities,
            ["kills"] = player.kills,

            // Map dimensions
            ["map_width"] = gameState.Map.Width,
            ["map_height"] = gameState.Map.Height,

            // Full tile data
            ["tiles"] = GetTileObservations(player)
        };

        return obs;
    }

    private List<Dictionary<string, object>> GetTileObservations(PlayerState player)
    {
        var tiles = new List<Dictionary<string, object>>();

        for (int i = 0; i < gameState.Map.Tiles.Length; i++)
        {
            var tile = gameState.Map.Tiles[i];

            // Check if player can see this tile
            bool visible = tile.explorers != null && tile.explorers.Contains(player.Id);

            var tileData = new Dictionary<string, object>
            {
                ["x"] = tile.coordinates.X,
                ["y"] = tile.coordinates.Y,
                ["terrain"] = tile.terrain.ToString(),
                ["owner"] = tile.owner,
                ["visible"] = visible,
                ["resource"] = tile.resource?.type.ToString() ?? "None",
                ["has_resource"] = tile.resource != null && tile.resource.type != ResourceData.Type.None
            };

            // Unit data
            if (tile.unit != null && visible)
            {
                tileData["unit"] = new Dictionary<string, object>
                {
                    ["type"] = tile.unit.type.ToString(),
                    ["owner"] = tile.unit.owner,
                    ["health"] = tile.unit.health,
                    ["promotion_level"] = tile.unit.promotionLevel,
                    ["has_moved"] = tile.unit.moved || tile.unit.attacked
                };
            }

            // Improvement data
            if (tile.improvement != null && visible)
            {
                var impData = new Dictionary<string, object>
                {
                    ["type"] = tile.improvement.type.ToString(),
                    ["level"] = tile.improvement.level,
                    ["owner"] = tile.improvement.owner
                };

                // City-specific data
                if (tile.improvement.type == ImprovementData.Type.City)
                {
                    impData["population"] = tile.improvement.population;
                    impData["production"] = tile.improvement.production;
                    impData["is_capital"] = tile.improvement.founder == tile.improvement.owner && tile.improvement.founded == 1;
                    impData["border_size"] = tile.improvement.borderSize;
                    impData["name"] = tile.improvement.name ?? "";
                }

                tileData["improvement"] = impData;
            }

            tiles.Add(tileData);
        }

        return tiles;
    }

    private Dictionary<string, object> GetAllObservations()
    {
        var observations = new Dictionary<string, object>();

        // Real players are at indices 0 to PlayerCount-1
        for (int i = 0; i < gameState.PlayerCount; i++)
        {
            var player = gameState.PlayerStates[i];
            string agentName = $"player_{player.Id - 1}";
            observations[agentName] = GetObservation(agentName);
        }

        return observations;
    }

    private Dictionary<string, double> GetRewards()
    {
        var rewards = new Dictionary<string, double>();

        // Real players are at indices 0 to PlayerCount-1
        for (int i = 0; i < gameState.PlayerCount; i++)
        {
            var player = gameState.PlayerStates[i];
            string agentName = $"player_{player.Id - 1}";
            rewards[agentName] = player.score;
        }

        return rewards;
    }

    private Dictionary<string, bool> GetTerminations()
    {
        var terminations = new Dictionary<string, bool>();

        // Real players are at indices 0 to PlayerCount-1
        for (int i = 0; i < gameState.PlayerCount; i++)
        {
            var player = gameState.PlayerStates[i];
            string agentName = $"player_{player.Id - 1}";
            bool eliminated = player.wipedAtCommand >= 0 || player.resignedAtCommandIndex >= 0;
            terminations[agentName] = eliminated || gameState.CurrentState == GameState.State.Ended;
        }

        return terminations;
    }

    private Dictionary<string, bool> GetTruncations()
    {
        var truncations = new Dictionary<string, bool>();
        bool truncated = gameState.CurrentTurn >= maxTurns;

        // Real players are at indices 0 to PlayerCount-1
        for (int i = 0; i < gameState.PlayerCount; i++)
        {
            var player = gameState.PlayerStates[i];
            string agentName = $"player_{player.Id - 1}";
            truncations[agentName] = truncated;
        }

        return truncations;
    }

    private List<string> GetActiveAgents()
    {
        var agents = new List<string>();

        // Real players are at indices 0 to PlayerCount-1
        for (int i = 0; i < gameState.PlayerCount; i++)
        {
            var player = gameState.PlayerStates[i];
            if (player.wipedAtCommand >= 0 || player.resignedAtCommandIndex >= 0) continue;

            agents.Add($"player_{player.Id - 1}");
        }

        return agents;
    }

    private string GetCurrentAgent()
    {
        // CurrentPlayerIndex now points directly to real players (indices 0-3)
        // or to Nature at the end (index PlayerCount)
        int playerIndex = gameState.CurrentPlayerIndex;

        // If out of bounds or pointing to Nature, wrap to first active player
        if (playerIndex < 0 || playerIndex >= gameState.PlayerCount)
        {
            // Find first active player
            for (int i = 0; i < gameState.PlayerCount; i++)
            {
                var player = gameState.PlayerStates[i];
                if (player.wipedAtCommand < 0 && player.resignedAtCommandIndex < 0)
                {
                    return $"player_{player.Id - 1}";
                }
            }
            return "";
        }

        // Convert player Id to agent name (Id 1-4 -> player_0-3)
        var currentPlayer = gameState.PlayerStates[playerIndex];
        return $"player_{currentPlayer.Id - 1}";
    }

    private bool IsGameOver()
    {
        if (gameState.CurrentState == GameState.State.Ended)
            return true;

        if (gameState.CurrentTurn >= maxTurns)
            return true;

        // Count active players
        int activePlayers = 0;
        foreach (var player in gameState.PlayerStates)
        {
            if (player.Id != byte.MaxValue &&
                player.wipedAtCommand < 0 &&
                player.resignedAtCommandIndex < 0)
            {
                activePlayers++;
            }
        }

        return activePlayers <= 1;
    }

    private void AdvanceToNextPlayer()
    {
        // Find next active player
        int startIdx = gameState.CurrentPlayerIndex;

        do
        {
            gameState.CurrentPlayerIndex++;

            // Wrap around
            if (gameState.CurrentPlayerIndex >= gameState.PlayerStates.Count)
            {
                gameState.CurrentPlayerIndex = 1; // Skip Nature (0)
                gameState.CurrentTurn++;
            }

            var player = gameState.PlayerStates[gameState.CurrentPlayerIndex];

            // Check if player is still active
            if (player.wipedAtCommand < 0 && player.resignedAtCommandIndex < 0)
            {
                break;
            }

        } while (gameState.CurrentPlayerIndex != startIdx);
    }

    // JSON structures
    public class Request
    {
        public string command { get; set; }
        public int? seed { get; set; }
        public int? num_players { get; set; }
        public string game_mode { get; set; }
        public string agent { get; set; }
        public string action_type { get; set; }
        public Dictionary<string, object> action_params { get; set; }
    }

    public class Response
    {
        public bool success { get; set; }
        public string error { get; set; }
        public Dictionary<string, object> observation { get; set; }
        public Dictionary<string, object> observations { get; set; }
        public Dictionary<string, double> rewards { get; set; }
        public Dictionary<string, bool> terminations { get; set; }
        public Dictionary<string, bool> truncations { get; set; }
        public List<string> agents { get; set; }
        public string agent_selection { get; set; }
        public Dictionary<string, object> info { get; set; }
        public Dictionary<string, object> state { get; set; }
    }
}
