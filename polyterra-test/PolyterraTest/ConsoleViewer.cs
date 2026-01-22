using System;
using Polytopia.Data;

/// <summary>
/// Console-based game state viewer with ASCII art
/// </summary>
public static class ConsoleViewer
{
    public static void RenderGameState(GameState gameState)
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine($"  🎮 POLYTERRA - Turn {gameState.CurrentTurn}");
        Console.WriteLine($"  State: {gameState.CurrentState} | Players: {gameState.PlayerCount}");
        Console.WriteLine("═══════════════════════════════════════════════════════\n");

        // Show players
        Console.WriteLine("👥 PLAYERS:");
        foreach (var player in gameState.PlayerStates)
        {
            if (player.Id == byte.MaxValue) continue;

            var isCurrentPlayer = gameState.CurrentPlayerIndex == gameState.PlayerStates.IndexOf(player);
            var marker = isCurrentPlayer ? " ⬅️" : "  ";

            Console.WriteLine($"{marker} Player {player.Id}: {player.UserName} ({player.tribe}) - ⭐{player.Currency} stars");
        }

        // Show map
        Console.WriteLine($"\n🗺️  MAP ({gameState.Map.Width}x{gameState.Map.Height}):");
        Console.WriteLine();

        var map = gameState.Map;
        int width = map.Width;
        int height = map.Height;

        // Header
        Console.Write("   ");
        for (int x = 0; x < width; x++)
        {
            Console.Write($" {x} ");
        }
        Console.WriteLine();

        for (int y = 0; y < height; y++)
        {
            Console.Write($"{y,2} ");
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index < map.Tiles.Length)
                {
                    var tile = map.Tiles[index];
                    string symbol = tile.terrain switch
                    {
                        TerrainData.Type.Field => " · ",
                        TerrainData.Type.Forest => " T ",
                        TerrainData.Type.Mountain => " ^ ",
                        TerrainData.Type.Water => " ~ ",
                        TerrainData.Type.Ocean => " ≈ ",
                        _ => " ? "
                    };

                    Console.Write(symbol);
                }
            }
            Console.WriteLine();
        }

        Console.WriteLine("\n📖 Legend: · = Field  T = Forest  ^ = Mountain  ~ = Water  ≈ = Ocean");
        Console.WriteLine("\n💡 Press ENTER to advance turn, or 'q' to quit");
    }
}
