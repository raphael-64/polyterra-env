using System;
using System.Collections.Generic;
using System.Linq;
using Polytopia.Data;
using PolytopiaBackendBase.Game;

/// <summary>
/// Headless game runner for Polytopia
/// </summary>
public class GameRunner
{
    private GameState gameState;
    private ActionManager actionManager;
    private int maxTurns;
    private bool verbose;

    public GameState GameState => gameState;

    public GameRunner(int maxTurns = 100, bool verbose = false)
    {
        this.maxTurns = maxTurns;
        this.verbose = verbose;
    }

    /// <summary>
    /// Initialize a new game
    /// </summary>
    public void InitializeGame(int numPlayers, GameMode gameMode = GameMode.Domination, int seed = -1)
    {
        if (verbose) Console.WriteLine($"\n=== Initializing Game ===");
        if (verbose) Console.WriteLine($"Players: {numPlayers}, Mode: {gameMode}");

        // Initialize game data
        GameDataLoader.InitializeDataManager();

        // Create game state
        gameState = new GameState();
        if (seed >= 0)
        {
            gameState.Seed = seed;
        }

        // Set version to latest (100+ based on decompiled code)
        gameState.Version = 100;

        // Initialize settings based on game mode
        gameState.Settings = new GameSettings
        {
            Difficulty = GameSettings.Difficulties.Normal,
            BaseGameMode = gameMode,
            RulesGameMode = gameMode
        };

        // For Domination mode, set opponent count (Perfection doesn't need it)
        if (gameMode == GameMode.Domination)
        {
            gameState.Settings.OpponentCount = numPlayers - 1;
        }

        // Create player states (index 0 is Nature/neutral player)
        gameState.PlayerStates = new List<PlayerState>();

        // Add nature player (ID 255)
        var naturePlayer = new PlayerState
        {
            Id = byte.MaxValue,
            UserName = "Nature",
            tribe = TribeData.Type.Nature,
            AutoPlay = false,
            hasChosenTribe = true
        };
        gameState.PlayerStates.Add(naturePlayer);

        // Add human/AI players
        // Only use tribes that exist in gamedata.json
        var availableTribes = new List<TribeData.Type>
        {
            TribeData.Type.Imperius,
            TribeData.Type.Bardur,
            TribeData.Type.Oumaji,
            TribeData.Type.Xinxi
        };

        for (byte i = 1; i <= numPlayers; i++)
        {
            var tribe = availableTribes[(i - 1) % availableTribes.Count];
            var player = new PlayerState
            {
                Id = i,
                UserName = $"Player {i}",
                tribe = tribe,
                AutoPlay = true, // All players are AI
                hasChosenTribe = true,
                handicap = 1,
                Currency = 0,
                aiState = new AIState()
            };
            gameState.PlayerStates.Add(player);
        }

        // Initialize map with size (MapSize is determined by GameMode)
        int mapSize = gameState.Settings.MapSize;
        if (verbose) Console.WriteLine($"Generating map (size: {mapSize}x{mapSize})...");
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

        // Set player colors and names
        GameStateUtils.SetPlayerColors(gameState);

        // Initialize action manager
        actionManager = new ActionManager(gameState);

        // Set game state to lobby
        gameState.CurrentState = GameState.State.Lobby;
        gameState.CurrentPlayerIndex = 1; // Start with first real player

        if (verbose) Console.WriteLine($"✓ Game initialized with {numPlayers} players");
    }

    /// <summary>
    /// Start the game match
    /// </summary>
    public void StartMatch()
    {
        if (verbose) Console.WriteLine("\n=== Starting Match ===");

        var startCommand = new StartMatchCommand(gameState.PlayerStates[1].Id);
        if (!actionManager.ExecuteCommand(startCommand, out string error))
        {
            throw new Exception($"Failed to start match: {error}");
        }

        gameState.CurrentState = GameState.State.Started;
        gameState.CurrentTurn = 1;

        if (verbose) Console.WriteLine("✓ Match started");
    }

    /// <summary>
    /// Run the game until completion or max turns
    /// </summary>
    public GameResult RunGame()
    {
        if (verbose) Console.WriteLine($"\n=== Running Game (Max {maxTurns} turns) ===\n");

        int turnCount = 0;
        while (!IsGameOver() && turnCount < maxTurns)
        {
            turnCount++;
            RunTurn();
        }

        return GetGameResult();
    }

    /// <summary>
    /// Run a single turn for all players
    /// </summary>
    public void RunTurn()
    {
        if (verbose) Console.WriteLine($"--- Turn {gameState.CurrentTurn} ---");

        for (int playerIndex = 1; playerIndex < gameState.PlayerStates.Count; playerIndex++)
        {
            var player = gameState.PlayerStates[playerIndex];

            // Skip eliminated players
            if (player.wipedAtCommand >= 0 || player.resignedAtCommandIndex >= 0)
            {
                continue;
            }

            gameState.CurrentPlayerIndex = (byte)playerIndex;

            if (verbose) Console.WriteLine($"  Player {player.Id} ({player.tribe}) - Score: {player.score}");

            // Get AI move
            var command = AI.GetMove(gameState, player);

            // Execute command
            if (!actionManager.ExecuteCommand(command, out string error))
            {
                Console.WriteLine($"  ERROR executing command: {error}");
            }

            // Check if it's end turn
            if (command is EndTurnCommand || command is EndMatchCommand)
            {
                if (verbose && !(command is EndMatchCommand))
                {
                    var cityCount = gameState.Map.Tiles.Count(t =>
                        t.improvement?.type == ImprovementData.Type.City && t.owner == player.Id);
                    var unitCount = gameState.Map.Tiles.Count(t =>
                        t.unit != null && t.unit.owner == player.Id);
                    Console.WriteLine($"    Cities: {cityCount}, Units: {unitCount}, Stars: {player.Currency}");
                }
            }
        }

        // Advance turn
        gameState.CurrentTurn++;
    }

    /// <summary>
    /// Check if game is over
    /// </summary>
    public bool IsGameOver()
    {
        if (gameState.CurrentState == GameState.State.Ended)
        {
            return true;
        }

        // Count active players
        int activePlayers = 0;
        foreach (var player in gameState.PlayerStates)
        {
            if (player.Id != byte.MaxValue && player.wipedAtCommand < 0 && player.resignedAtCommandIndex < 0)
            {
                activePlayers++;
            }
        }

        return activePlayers <= 1;
    }

    /// <summary>
    /// Get final game result
    /// </summary>
    public GameResult GetGameResult()
    {
        var result = new GameResult
        {
            TotalTurns = (int)gameState.CurrentTurn,
            PlayerResults = new List<PlayerResult>()
        };

        var sortedPlayers = gameState.GetPlayersSortedByRank();

        foreach (var player in sortedPlayers)
        {
            if (player.Id == byte.MaxValue) continue; // Skip nature

            var playerResult = new PlayerResult
            {
                PlayerId = player.Id,
                PlayerName = player.UserName,
                Tribe = player.tribe,
                FinalScore = player.score,
                Rank = result.PlayerResults.Count + 1,
                Eliminated = player.wipedAtCommand >= 0
            };

            result.PlayerResults.Add(playerResult);
        }

        if (result.PlayerResults.Count > 0)
        {
            result.WinnerId = result.PlayerResults[0].PlayerId;
            result.WinnerName = result.PlayerResults[0].PlayerName;
        }

        return result;
    }

    /// <summary>
    /// Print game summary
    /// </summary>
    public void PrintGameSummary()
    {
        var result = GetGameResult();

        Console.WriteLine("\n=== GAME OVER ===");
        Console.WriteLine($"Total Turns: {result.TotalTurns}");
        Console.WriteLine($"Winner: {result.WinnerName} (Player {result.WinnerId})");
        Console.WriteLine("\nFinal Rankings:");

        foreach (var pr in result.PlayerResults)
        {
            string status = pr.Eliminated ? " [ELIMINATED]" : "";
            Console.WriteLine($"  {pr.Rank}. {pr.PlayerName} ({pr.Tribe}) - Score: {pr.FinalScore}{status}");
        }
    }
}

/// <summary>
/// Game result data
/// </summary>
public class GameResult
{
    public int TotalTurns { get; set; }
    public byte WinnerId { get; set; }
    public string WinnerName { get; set; }
    public List<PlayerResult> PlayerResults { get; set; }
}

/// <summary>
/// Individual player result
/// </summary>
public class PlayerResult
{
    public byte PlayerId { get; set; }
    public string PlayerName { get; set; }
    public TribeData.Type Tribe { get; set; }
    public uint FinalScore { get; set; }
    public int Rank { get; set; }
    public bool Eliminated { get; set; }
}
