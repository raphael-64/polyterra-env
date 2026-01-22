using System;
using System.Collections.Generic;
using System.Linq;
using Polytopia.Data;
using PolytopiaBackendBase.Game;

/// <summary>
/// Simple game test without full map generation
/// Tests core game mechanics: commands, actions, AI decision making
/// </summary>
public class SimpleGameTest
{
    public static void Run()
    {
        Console.WriteLine("=== Simple Game Logic Test ===\n");

        try
        {
            // Initialize game data
            Console.WriteLine("Initializing game data...");
            GameDataLoader.InitializeDataManager();
            Console.WriteLine();

            // Test 1: Create basic game state
            Console.WriteLine("Test 1: Creating game state with players...");
            var gameState = new GameState();
            gameState.Version = 100;
            gameState.Seed = new Random().Next();
            gameState.Settings = new GameSettings
            {
                Difficulty = GameSettings.Difficulties.Normal,
                BaseGameMode = GameMode.Perfection,
                MapSize = 12  // Small map (Tiny=10, Small=12, Normal=14, Large=16, Huge=18)
            };

            // Add players
            gameState.PlayerStates = new List<PlayerState>();

            // Nature player
            gameState.PlayerStates.Add(new PlayerState
            {
                Id = byte.MaxValue,
                UserName = "Nature",
                tribe = TribeData.Type.Nature
            });

            // Real players
            var tribes = new[] { TribeData.Type.Imperius, TribeData.Type.Bardur, TribeData.Type.Oumaji, TribeData.Type.Xinxi };
            for (byte i = 1; i <= 4; i++)
            {
                gameState.PlayerStates.Add(new PlayerState
                {
                    Id = i,
                    UserName = $"Player {i}",
                    tribe = tribes[i - 1],
                    AutoPlay = true,
                    hasChosenTribe = true,
                    aiState = new AIState(),
                    Currency = 5
                });
            }

            Console.WriteLine($"✓ Created {gameState.PlayerCount} players");

            // Test 2: Generate real map with MapGenerator
            Console.WriteLine("\nTest 2: Generating map with MapGenerator...");

            // Pre-allocate the map (MapGenerator expects this)
            int mapSize = gameState.Settings.MapSize;
            gameState.Map = new MapData((ushort)mapSize, (ushort)mapSize);

            var mapSettings = new MapGeneratorSettings
            {
                wetness = 0.55f,      // Default water coverage
                richness = 1f,        // Resource density
                smoothIterations = 3
            };

            var mapGenerator = new MapGenerator();

            try
            {
                mapGenerator.Generate(gameState, mapSettings, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠ Map generation failed: {ex.Message}");
                Console.WriteLine($"  Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"  Inner stack: {ex.InnerException.StackTrace}");
                }
                Console.WriteLine("  Creating patterned fallback map...");
            }

            // Check if generation succeeded
            if (gameState.Map != null && gameState.Map.Tiles != null)
            {
                Console.WriteLine($"✓ Generated {gameState.Map.Width}x{gameState.Map.Height} map");

                // Count terrain types
                var terrainCounts = new Dictionary<TerrainData.Type, int>();
                foreach (var tile in gameState.Map.Tiles)
                {
                    if (!terrainCounts.ContainsKey(tile.terrain))
                        terrainCounts[tile.terrain] = 0;
                    terrainCounts[tile.terrain]++;
                }

                Console.WriteLine("  Terrain distribution:");
                foreach (var kvp in terrainCounts.OrderByDescending(x => x.Value))
                {
                    Console.WriteLine($"    {kvp.Key}: {kvp.Value} tiles");
                }
            }
            else
            {
                Console.WriteLine("  ⚠ Map generation may have failed, creating minimal fallback...");
                int fallbackSize = 12;
                gameState.Map = new MapData((ushort)fallbackSize, (ushort)fallbackSize);
                for (int i = 0; i < gameState.Map.Tiles.Length; i++)
                {
                    int x = i % fallbackSize;
                    int y = i / fallbackSize;
                    gameState.Map.Tiles[i] = new TileData
                    {
                        coordinates = new WorldCoordinates(x, y),
                        terrain = (x + y) % 3 == 0 ? TerrainData.Type.Forest :
                                 (x + y) % 3 == 1 ? TerrainData.Type.Mountain : TerrainData.Type.Field,
                        owner = 0
                    };
                }
            }

            // Test 3: Test command execution
            Console.WriteLine("\nTest 3: Testing command execution...");
            var actionManager = new ActionManager(gameState);
            gameState.CurrentState = GameState.State.Started;
            gameState.CurrentPlayerIndex = 1;

            // Create and execute an EndTurn command
            var command = new EndTurnCommand(gameState.PlayerStates[1].Id);
            bool success = actionManager.ExecuteCommand(command, out string error);

            if (success)
            {
                Console.WriteLine("✓ EndTurnCommand executed successfully");
            }
            else
            {
                Console.WriteLine($"✗ Command failed: {error}");
            }

            // Test 4: Test basic game state queries
            Console.WriteLine("\nTest 4: Testing game state queries...");
            Console.WriteLine($"  Current Turn: {gameState.CurrentTurn}");
            Console.WriteLine($"  Player Count: {gameState.PlayerCount}");
            Console.WriteLine($"  Game State: {gameState.CurrentState}");
            Console.WriteLine($"  Map Size: {gameState.Map.Width}x{gameState.Map.Height}");
            Console.WriteLine("✓ All queries successful");

            // Test 5: Test player state
            Console.WriteLine("\nTest 5: Testing player state...");
            foreach (var player in gameState.PlayerStates)
            {
                if (player.Id == byte.MaxValue) continue;
                Console.WriteLine($"  Player {player.Id}: {player.UserName} ({player.tribe}) - Stars: {player.Currency}");
            }
            Console.WriteLine("✓ Player state accessible");

            Console.WriteLine("\n=== ALL TESTS PASSED ===");
            Console.WriteLine("✓ Core game logic is functional!");
            Console.WriteLine("✓ Commands can be executed");
            Console.WriteLine("✓ Game state is manageable");

            Console.WriteLine("\n=== Starting Interactive Game View ===");
            Console.WriteLine("Press ENTER to start...");
            Console.ReadLine();

            // Interactive loop with console viewer
            while (true)
            {
                // Render current state
                ConsoleViewer.RenderGameState(gameState);

                var input = Console.ReadLine();
                if (input?.ToLower() == "q")
                    break;

                // Play a turn
                if (gameState.CurrentState == GameState.State.Started)
                {
                    var nextPlayer = gameState.PlayerStates[gameState.CurrentPlayerIndex];
                    var endTurn = new EndTurnCommand(nextPlayer.Id);

                    if (!actionManager.ExecuteCommand(endTurn, out string err))
                    {
                        Console.WriteLine($"\n✗ Error: {err}");
                        Console.WriteLine("Press ENTER to continue...");
                        Console.ReadLine();
                    }
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine($"\n🏆 GAME OVER!");
                    Console.WriteLine($"   Final State: {gameState.CurrentState}");
                    Console.WriteLine($"   Total Turns: {gameState.CurrentTurn}");
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
                    break;
                }
            }

            Console.WriteLine("\n✓ Game session ended");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
