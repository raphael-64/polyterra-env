// Check if running as environment server
if (args.Length > 0 && args[0] == "--env-server")
{
    RunEnvServer();
    return 0;
}

Console.WriteLine("=== Polyterra Headless Game Runner ===\n");

try
{
    // Parse command-line arguments
    // Usage: dotnet run [mode] [players] [seed]
    // Example: dotnet run perfection 4
    // Example: dotnet run domination 4 12345

    var gameMode = PolytopiaBackendBase.Game.GameMode.Perfection; // Default
    int numPlayers = 4;
    int maxTurns = 100;
    int seed = new Random().Next();
    bool verbose = true;

    if (args.Length > 0)
    {
        string modeArg = args[0].ToLower();
        if (modeArg == "domination" || modeArg == "dom")
        {
            gameMode = PolytopiaBackendBase.Game.GameMode.Domination;
        }
        else if (modeArg == "perfection" || modeArg == "perf")
        {
            gameMode = PolytopiaBackendBase.Game.GameMode.Perfection;
        }
    }

    if (args.Length > 1 && int.TryParse(args[1], out int playerCount))
    {
        numPlayers = playerCount;
    }

    if (args.Length > 2 && int.TryParse(args[2], out int seedArg))
    {
        seed = seedArg;
    }

    string mapSizeInfo = gameMode == PolytopiaBackendBase.Game.GameMode.Perfection
        ? "16x16 (Perfection mode)"
        : "16x16 (4 players in Domination mode)";

    Console.WriteLine($"Configuration:");
    Console.WriteLine($"  Players: {numPlayers}");
    Console.WriteLine($"  Game Mode: {gameMode}");
    Console.WriteLine($"  Map Size: {mapSizeInfo}");
    Console.WriteLine($"  Max Turns: {maxTurns}");
    Console.WriteLine($"  Seed: {seed}");
    Console.WriteLine();
    Console.WriteLine($"💡 Tip: Run with arguments to customize:");
    Console.WriteLine($"   dotnet run perfection 4");
    Console.WriteLine($"   dotnet run domination 4 12345");
    Console.WriteLine();

    // Create and initialize game runner
    var gameRunner = new GameRunner(maxTurns, verbose);

    // Initialize game with 4 players and selected mode
    gameRunner.InitializeGame(numPlayers, gameMode, seed);

    // Start the match
    gameRunner.StartMatch();

    // Display initial game state
    Console.WriteLine("\n=== Initial Game State ===");
    ConsoleViewer.RenderGameState(gameRunner.GameState);
    Console.WriteLine("\nPress ENTER to start automated game...");
    Console.ReadLine();

    // Run the game
    var result = gameRunner.RunGame();

    // Print final summary
    gameRunner.PrintGameSummary();

    Console.WriteLine("\n=== HEADLESS GAME COMPLETE ===");
    Console.WriteLine("✓ Map generated successfully");
    Console.WriteLine("✓ 4 players initialized");
    Console.WriteLine("✓ Game ran to completion");
    Console.WriteLine($"✓ Total turns played: {result.TotalTurns}");

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ ERROR: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"\nInner Exception: {ex.InnerException.Message}");
        Console.WriteLine($"Inner Stack: {ex.InnerException.StackTrace}");
    }
    return 1;
}

static void RunEnvServer()
{
    var bridge = new PolyterraEnvBridge(maxTurns: 100);

    Console.Error.WriteLine("Polyterra Environment Server started");
    Console.Error.WriteLine("Ready to accept JSON commands on stdin");

    while (true)
    {
        try
        {
            string line = Console.ReadLine();

            if (string.IsNullOrEmpty(line))
            {
                break;
            }

            // Process command
            string response = bridge.ProcessCommand(line);

            // Write response to stdout
            Console.WriteLine(response);
            Console.Out.Flush();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            break;
        }
    }

    Console.Error.WriteLine("Environment Server shutting down");
}
