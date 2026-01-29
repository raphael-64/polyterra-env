using System;
using System.Collections.Generic;
using System.IO;
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

        // Add Nature player LAST (required because PlayerCount = PlayerStates.Count - 1)
        // Nature represents neutral entities but isn't a turn-taking player
        gameState.PlayerStates.Add(new PlayerState
        {
            Id = byte.MaxValue,
            UserName = "Nature",
            tribe = TribeData.Type.Nature,
            AutoPlay = true,
            hasChosenTribe = true
        });

        // Generate map with deterministic seed (like LocalClient/HotseatClient)
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

        // Spawn starting units for each player (like LocalClient/HotseatClient do at lines 163-166)
        for (int i = 0; i < numPlayers; i++)
        {
            var player = gameState.PlayerStates[i];
            if (gameState.GameLogicData.TryGetData(player.tribe, out var tribeData) &&
                gameState.GameLogicData.TryGetData(tribeData.startingUnit.type, out var unitData))
            {
                var capitalTile = gameState.Map.GetTile(player.startTile);
                var unit = ActionUtils.TrainUnitScored(gameState, player, capitalTile, unitData);
                // Make sure unit can act on first turn
                unit.attacked = false;
                unit.moved = false;

                // Explore tiles around the starting unit (like TrainAction does)
                ActionUtils.ExploreFromTile(gameState, player, capitalTile, unitData.GetSightRange(), shouldUseActions: false);
            }
        }

        // Process all queued exploration actions
        ActionManagerUtils.PerformAllQueuedActions(gameState);

        // Initialize action manager
        actionManager = new ActionManager(gameState);

        // Start match
        gameState.CurrentState = GameState.State.Lobby;
        var startCommand = new StartMatchCommand(gameState.PlayerStates[0].Id);  // First real player
        actionManager.ExecuteCommand(startCommand, out _);

        gameState.CurrentState = GameState.State.Started;
        gameState.CurrentPlayerIndex = 0;  // First real player at index 0

        // Add StartTurnAction for player_0 so they get currency like all other players
        gameState.ActionStack.Add(new StartTurnAction(gameState.PlayerStates[0].Id));
        ActionManagerUtils.PerformAllQueuedActions(gameState);

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

        // Skip Nature if we land on it (Nature is always last and should never take turns)
        // This happens when last real player ends turn
        while (gameState.CurrentPlayerIndex < gameState.PlayerStates.Count &&
               gameState.PlayerStates[gameState.CurrentPlayerIndex].Id == byte.MaxValue)
        {
            File.AppendAllText("/tmp/polyterra-debug.log", $"[HandleStep] Skipping Nature at index {gameState.CurrentPlayerIndex}, turn {gameState.CurrentTurn}\n");
            gameState.EndPlayerTurn();
            File.AppendAllText("/tmp/polyterra-debug.log", $"[HandleStep] After skipping Nature: index {gameState.CurrentPlayerIndex}, turn {gameState.CurrentTurn}\n");
        }

        var playerIndex = gameState.CurrentPlayerIndex;
        var player = gameState.PlayerStates[playerIndex];

        File.AppendAllText("/tmp/polyterra-debug.log", $"[HandleStep] Before action: Player {player.Id}, Index {playerIndex}, Turn {gameState.CurrentTurn}, Currency {player.Currency}\n");

        // Parse and execute command based on action_type
        CommandBase command;

        try
        {
            command = ParseAction(request, player);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new Response
            {
                success = false,
                error = $"Failed to parse action: {ex.Message}"
            });
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

        File.AppendAllText("/tmp/polyterra-debug.log", $"[HandleStep] After action: Player {gameState.PlayerStates[gameState.CurrentPlayerIndex].Id}, Index {gameState.CurrentPlayerIndex}, Turn {gameState.CurrentTurn}\n");

        // If we land on Nature after command execution, skip it
        // This happens when a player ends turn and Nature is next
        bool skippedNature = false;
        while (gameState.CurrentPlayerIndex < gameState.PlayerStates.Count &&
               gameState.PlayerStates[gameState.CurrentPlayerIndex].Id == byte.MaxValue)
        {
            File.AppendAllText("/tmp/polyterra-debug.log", $"[HandleStep] Skipping Nature after action at index {gameState.CurrentPlayerIndex}, turn {gameState.CurrentTurn}\n");
            gameState.EndPlayerTurn();
            File.AppendAllText("/tmp/polyterra-debug.log", $"[HandleStep] After skipping Nature: index {gameState.CurrentPlayerIndex}, turn {gameState.CurrentTurn}\n");
            skippedNature = true;
        }

        // When we skip Nature, EndTurnAction added StartTurnAction for Nature (which we skipped)
        // so we need to add StartTurnAction for the current player and process it
        if (skippedNature)
        {
            gameState.ActionStack.Add(new StartTurnAction(gameState.CurrentPlayer));
            ActionManagerUtils.PerformAllQueuedActions(gameState);
            File.AppendAllText("/tmp/polyterra-debug.log", $"[HandleStep] Added StartTurnAction for player {gameState.CurrentPlayer} after skipping Nature\n");
        }

        // Check if game ended
        bool gameEnded = IsGameOver();

        // Get rewards (delta in score)
        var rewards = GetRewards();

        // NOTE: Don't manually advance player - EndTurnAction already calls state.EndPlayerTurn()

        var currentAgent = GetCurrentAgent();

        return JsonSerializer.Serialize(new Response
        {
            success = true,
            observations = GetAllObservations(),
            rewards = rewards,
            terminations = GetTerminations(),
            truncations = GetTruncations(),
            agents = GetActiveAgents(),
            agent_selection = currentAgent,
            info = new Dictionary<string, object>
            {
                ["current_turn"] = gameState.CurrentTurn
            }
        });
    }

    private CommandBase ParseAction(Request request, PlayerState player)
    {
        var actionType = request.action_type?.ToLower() ?? "end_turn";
        var actionParams = request.action_params ?? new Dictionary<string, object>();

        switch (actionType)
        {
            case "end_turn":
                return new EndTurnCommand(player.Id);

            case "move":
            {
                // Get unit from source coordinates
                int fromX = GetIntParam(actionParams, "from_x");
                int fromY = GetIntParam(actionParams, "from_y");
                int toX = GetIntParam(actionParams, "to_x");
                int toY = GetIntParam(actionParams, "to_y");

                var fromCoords = new WorldCoordinates((ushort)fromX, (ushort)fromY);
                var fromTile = gameState.Map.GetTile(fromCoords);
                if (fromTile?.unit == null)
                {
                    throw new Exception($"No unit at ({fromX}, {fromY})");
                }

                var target = new WorldCoordinates((ushort)toX, (ushort)toY);
                return new MoveCommand(player.Id, fromTile.unit, target);
            }

            case "attack":
            {
                // Get unit from source coordinates
                int fromX = GetIntParam(actionParams, "from_x");
                int fromY = GetIntParam(actionParams, "from_y");
                int targetX = GetIntParam(actionParams, "target_x");
                int targetY = GetIntParam(actionParams, "target_y");

                var fromCoords = new WorldCoordinates((ushort)fromX, (ushort)fromY);
                var fromTile = gameState.Map.GetTile(fromCoords);
                if (fromTile?.unit == null)
                {
                    throw new Exception($"No unit at ({fromX}, {fromY})");
                }

                var target = new WorldCoordinates((ushort)targetX, (ushort)targetY);
                return new AttackCommand(player.Id, fromTile.unit, target);
            }

            case "build":
            {
                int x = GetIntParam(actionParams, "x");
                int y = GetIntParam(actionParams, "y");
                string improvementTypeStr = GetStringParam(actionParams, "improvement_type");

                if (!Enum.TryParse<ImprovementData.Type>(improvementTypeStr, true, out var improvementType))
                {
                    throw new Exception($"Invalid improvement type: {improvementTypeStr}");
                }

                var coords = new WorldCoordinates((ushort)x, (ushort)y);
                return new BuildCommand(player.Id, improvementType, coords);
            }

            case "train":
            {
                int x = GetIntParam(actionParams, "city_x");
                int y = GetIntParam(actionParams, "city_y");
                string unitTypeStr = GetStringParam(actionParams, "unit_type");

                if (!Enum.TryParse<UnitData.Type>(unitTypeStr, true, out var unitType))
                {
                    throw new Exception($"Invalid unit type: {unitTypeStr}");
                }

                var coords = new WorldCoordinates((ushort)x, (ushort)y);
                return new TrainCommand(player.Id, unitType, coords);
            }

            case "research":
            {
                string techTypeStr = GetStringParam(actionParams, "tech_type");

                if (!Enum.TryParse<TechData.Type>(techTypeStr, true, out var techType))
                {
                    throw new Exception($"Invalid tech type: {techTypeStr}");
                }

                return new ResearchCommand(player.Id, techType);
            }

            case "promote":
            {
                int x = GetIntParam(actionParams, "x");
                int y = GetIntParam(actionParams, "y");
                var coords = new WorldCoordinates((ushort)x, (ushort)y);
                return new PromoteCommand(player.Id, coords);
            }

            case "recover":
            {
                int x = GetIntParam(actionParams, "x");
                int y = GetIntParam(actionParams, "y");
                var coords = new WorldCoordinates((ushort)x, (ushort)y);
                return new RecoverCommand(player.Id, coords);
            }

            default:
                throw new Exception($"Unknown action type: {actionType}");
        }
    }

    private int GetIntParam(Dictionary<string, object> actionParams, string key)
    {
        if (!actionParams.ContainsKey(key))
        {
            throw new Exception($"Missing required parameter: {key}");
        }

        var value = actionParams[key];
        if (value is JsonElement jsonElement)
        {
            return jsonElement.GetInt32();
        }
        return Convert.ToInt32(value);
    }

    private string GetStringParam(Dictionary<string, object> actionParams, string key)
    {
        if (!actionParams.ContainsKey(key))
        {
            throw new Exception($"Missing required parameter: {key}");
        }

        var value = actionParams[key];
        if (value is JsonElement jsonElement)
        {
            return jsonElement.GetString();
        }
        return value.ToString();
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
            ["current_player_idx"] = gameState.CurrentPlayerIndex,  // Already 0-indexed (real players at indices 0-3)

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
            ["tiles"] = GetTileObservations(player),

            // Valid actions for the current player
            ["valid_actions"] = GetValidActions(player)
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
            // In Polytopia: explorers list contains player IDs who have discovered this tile
            bool explored = tile.explorers != null && tile.explorers.Contains(player.Id);
            // For now, visible = explored (could be refined for fog of war vs current vision)
            bool visible = explored;

            var tileData = new Dictionary<string, object>
            {
                ["x"] = tile.coordinates.X,
                ["y"] = tile.coordinates.Y,
                ["terrain"] = tile.terrain.ToString(),
                ["owner"] = tile.owner,
                ["explored"] = explored,
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

        // Real players are at indices 0 to PlayerCount-1 (PlayerCount excludes Nature)
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

        // Real players are at indices 0 to PlayerCount-1 (PlayerCount excludes Nature)
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

        // Real players are at indices 0 to PlayerCount-1 (PlayerCount excludes Nature)
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

        // Real players are at indices 0 to PlayerCount-1 (PlayerCount excludes Nature)
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

        // Real players are at indices 0 to PlayerCount-1 (PlayerCount excludes Nature)
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
        // CurrentPlayerIndex points to real players (0 to PlayerCount-1) or Nature (PlayerCount)
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

    private Dictionary<string, object> GetValidActions(PlayerState player)
    {
        // Return information about valid actions for the current player
        // This includes: valid unit moves, attacks, builds, etc.

        var validActions = new Dictionary<string, object>();
        var validMoves = new List<Dictionary<string, object>>();
        var validAttacks = new List<Dictionary<string, object>>();
        var validBuilds = new List<Dictionary<string, object>>();
        var validTrains = new List<Dictionary<string, object>>();

        // Check if player can end turn (almost always valid)
        var endTurnCmd = new EndTurnCommand(player.Id);
        validActions["can_end_turn"] = endTurnCmd.IsValid(gameState);

        // Check valid research options using game logic's GetUnlockableTech
        var validTechs = new List<string>();
        var unlockableTechs = gameState.GameLogicData.GetUnlockableTech(player);
        if (unlockableTechs != null)
        {
            foreach (var tech in unlockableTechs)
            {
                var researchCmd = new ResearchCommand(player.Id, tech.type);
                if (researchCmd.IsValid(gameState))
                {
                    validTechs.Add(tech.type.ToString());
                }
            }
        }
        validActions["valid_research"] = validTechs;

        // Iterate through all tiles to find valid actions
        for (int i = 0; i < gameState.Map.Tiles.Length; i++)
        {
            var tile = gameState.Map.Tiles[i];

            // Check unit actions if this tile has a player's unit
            if (tile.unit != null && tile.unit.owner == player.Id)
            {
                var unit = tile.unit;

                // Get unit data for range/movement info
                if (!gameState.GameLogicData.TryGetData(unit.type, out var unitData))
                {
                    continue;
                }

                // Use game engine's GetMovementOptions for efficient move checking
                var movement = unit.GetMovement(gameState);
                var movementOptions = unit.GetMovementOptions(gameState, movement);

                foreach (var targetCoords in movementOptions)
                {
                    var moveCmd = new MoveCommand(player.Id, unit, targetCoords);
                    if (moveCmd.IsValid(gameState))
                    {
                        validMoves.Add(new Dictionary<string, object>
                        {
                            ["unit_id"] = unit.id,
                            ["from_x"] = tile.coordinates.X,
                            ["from_y"] = tile.coordinates.Y,
                            ["to_x"] = targetCoords.X,
                            ["to_y"] = targetCoords.Y
                        });
                    }
                }

                // Use game engine's GetAttackOptions for efficient attack checking
                var attackOptions = unit.GetAttackOptions(gameState, unitData.GetRange());
                foreach (var targetCoords in attackOptions)
                {
                    var attackCmd = new AttackCommand(player.Id, unit, targetCoords);
                    if (attackCmd.IsValid(gameState))
                    {
                        validAttacks.Add(new Dictionary<string, object>
                        {
                            ["unit_id"] = unit.id,
                            ["from_x"] = tile.coordinates.X,
                            ["from_y"] = tile.coordinates.Y,
                            ["target_x"] = targetCoords.X,
                            ["target_y"] = targetCoords.Y
                        });
                    }
                }

                // Check if unit can recover
                var recoverCmd = new RecoverCommand(player.Id, tile.coordinates);
                if (recoverCmd.IsValid(gameState))
                {
                    validActions[$"can_recover_{unit.id}"] = true;
                }

                // Check if unit can promote
                var promoteCmd = new PromoteCommand(player.Id, tile.coordinates);
                if (promoteCmd.IsValid(gameState))
                {
                    validActions[$"can_promote_{unit.id}"] = true;
                }
            }

            // Check build/train actions if this tile has a city owned by player
            if (tile.owner == player.Id &&
                tile.improvement != null &&
                tile.improvement.type == Polytopia.Data.ImprovementData.Type.City)
            {
                // Check valid unit training using game logic's GetUnlockedUnits
                var unlockedUnits = gameState.GameLogicData.GetUnlockedUnits(player, gameState, false);
                if (unlockedUnits != null)
                {
                    foreach (var unitData in unlockedUnits)
                    {
                        var trainCmd = new TrainCommand(player.Id, unitData.type, tile.coordinates);
                        if (trainCmd.IsValid(gameState))
                        {
                            validTrains.Add(new Dictionary<string, object>
                            {
                                ["city_x"] = tile.coordinates.X,
                                ["city_y"] = tile.coordinates.Y,
                                ["unit_type"] = unitData.type.ToString()
                            });
                        }
                    }
                }

                // Check valid building constructions using game logic's GetUnlockedImprovements
                var unlockedImprovements = gameState.GameLogicData.GetUnlockedImprovements(player);
                if (unlockedImprovements != null)
                {
                    // Check each surrounding tile for valid builds
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int targetX = tile.coordinates.X + dx;
                            int targetY = tile.coordinates.Y + dy;

                            if (targetX >= 0 && targetX < gameState.Map.Width &&
                                targetY >= 0 && targetY < gameState.Map.Height)
                            {
                                var targetCoords = new WorldCoordinates((ushort)targetX, (ushort)targetY);

                                foreach (var improvementData in unlockedImprovements)
                                {
                                    var buildCmd = new BuildCommand(player.Id, improvementData.type, targetCoords);
                                    if (buildCmd.IsValid(gameState))
                                    {
                                        validBuilds.Add(new Dictionary<string, object>
                                        {
                                            ["x"] = targetX,
                                            ["y"] = targetY,
                                            ["improvement_type"] = improvementData.type.ToString()
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        validActions["valid_moves"] = validMoves;
        validActions["valid_attacks"] = validAttacks;
        validActions["valid_builds"] = validBuilds;
        validActions["valid_trains"] = validTrains;

        return validActions;
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
