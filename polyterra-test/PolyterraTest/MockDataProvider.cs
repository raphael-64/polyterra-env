using System;
using System.Collections.Generic;
using Polytopia.Data;

/// <summary>
/// Mock data provider that creates minimal game data programmatically
/// In production, this would load from JSON files
/// </summary>
public class MockDataProvider : IPolytopiaDataProvider
{
    public string LoadGameLogicData(int version)
    {
        // Return minimal game logic data JSON
        // This is a simplified version - real game has much more data
        return @"{
            ""tribes"": [],
            ""units"": [],
            ""improvements"": [],
            ""technologies"": [],
            ""resources"": [],
            ""terrains"": []
        }";
    }

    public string LoadAvatarData(int version)
    {
        return @"{""avatars"":[]}";
    }
}
