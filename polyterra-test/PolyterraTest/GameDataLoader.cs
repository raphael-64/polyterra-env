using System;
using System.IO;
using Polytopia.Data;

/// <summary>
/// Utility to load game data from JSON file
/// </summary>
public static class GameDataLoader
{
    private static GameLogicData _cachedData = null;

    /// <summary>
    /// Load game data from JSON file
    /// </summary>
    public static GameLogicData LoadGameData(string jsonPath = null)
    {
        // Return cached data if already loaded
        if (_cachedData != null)
        {
            return _cachedData;
        }

        // Determine JSON path
        if (string.IsNullOrEmpty(jsonPath))
        {
            // Default to gamedata.json in same directory as executable
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            jsonPath = Path.Combine(exeDir, "gamedata.json");
        }

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"Game data file not found: {jsonPath}");
        }

        Console.Error.WriteLine($"Loading game data from: {jsonPath}");

        // Load and parse JSON
        string jsonContent = File.ReadAllText(jsonPath);
        var gameData = new GameLogicData();
        gameData.Parse(jsonContent);

        // Set up the data provider with the loaded JSON
        PolytopiaDataManager.provider = new SimpleDataProvider(jsonContent);

        // Cache for future use
        _cachedData = gameData;

        Console.Error.WriteLine($"✓ Loaded game data:");
        Console.Error.WriteLine($"  Tribes: {gameData.tribes.Count}");
        Console.Error.WriteLine($"  Units: {gameData.units.Count}");
        Console.Error.WriteLine($"  Tech: {gameData.tech.Count}");
        Console.Error.WriteLine($"  Improvements: {gameData.improvements.Count}");
        Console.Error.WriteLine($"  Terrains: {gameData.terrains.Count}");
        Console.Error.WriteLine($"  Resources: {gameData.resources.Count}");

        return gameData;
    }

    /// <summary>
    /// Initialize PolytopiaDataManager with game data
    /// </summary>
    public static void InitializeDataManager(string jsonPath = null)
    {
        var gameData = LoadGameData(jsonPath);
        PolytopiaDataManager.currentVersion = gameData;
        Console.Error.WriteLine("✓ PolytopiaDataManager initialized");
    }
}
