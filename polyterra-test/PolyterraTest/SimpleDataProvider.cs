using System;
using System.IO;

/// <summary>
/// Simple implementation of IPolytopiaDataProvider for testing
/// </summary>
public class SimpleDataProvider : IPolytopiaDataProvider
{
    private readonly string _gameDataJson;

    public SimpleDataProvider(string gameDataJson)
    {
        _gameDataJson = gameDataJson;
    }

    public string LoadGameLogicData(int version)
    {
        // Return the same game data for all versions
        return _gameDataJson;
    }

    public string LoadAvatarData(int version)
    {
        // Return minimal avatar data
        return "{}";
    }
}
