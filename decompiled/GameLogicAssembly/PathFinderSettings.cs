using System.Collections.Generic;
using Polytopia.Data;

public struct PathFinderSettings
{
	public IEnumerable<TerrainData> allowedTerrain;

	public UnitState unit;

	public UnitData unitData;

	public PlayerState playerState;

	public GameState gameState;

	public bool isRequiredToUsePortToGoIntoWater;

	public bool shouldAllowOccupiedTiles;

	public bool shouldAllowEnemyTiles;

	public bool shouldAllowAlliesTiles;

	public bool shouldFollowTransportPaths;

	public bool shouldAllowOldRoutes;

	public int version;

	public static PathFinderSettings CreateDefault(PlayerState playerState, IEnumerable<TerrainData> allowedTerrain, int version, GameState gameState)
	{
		return new PathFinderSettings
		{
			version = version,
			playerState = playerState,
			allowedTerrain = allowedTerrain,
			gameState = gameState,
			shouldAllowOccupiedTiles = true,
			isRequiredToUsePortToGoIntoWater = true,
			shouldAllowEnemyTiles = false,
			shouldFollowTransportPaths = true
		};
	}

	public static PathFinderSettings CreateRouter(PlayerState playerState, IEnumerable<TerrainData> allowedTerrain, int version, GameState gameState)
	{
		return new PathFinderSettings
		{
			version = version,
			playerState = playerState,
			allowedTerrain = allowedTerrain,
			gameState = gameState,
			shouldAllowOccupiedTiles = true,
			isRequiredToUsePortToGoIntoWater = true,
			shouldAllowEnemyTiles = false,
			shouldAllowAlliesTiles = (version >= 60),
			shouldFollowTransportPaths = true,
			shouldAllowOldRoutes = true
		};
	}

	public static PathFinderSettings CreateForUnit(UnitState unit, GameState gameState)
	{
		gameState.GameLogicData.TryGetData(unit.type, out var data);
		gameState.TryGetPlayer(unit.owner, out var playerState);
		return new PathFinderSettings
		{
			version = gameState.Version,
			playerState = playerState,
			allowedTerrain = unit.GetAllowedTerrain(gameState),
			unit = unit,
			unitData = data,
			gameState = gameState,
			isRequiredToUsePortToGoIntoWater = (!data.IsAquatic() && !data.HasAbility(UnitAbility.Type.Fly)),
			shouldAllowOccupiedTiles = false,
			shouldAllowEnemyTiles = true
		};
	}

	public static PathFinderSettings CreateForScout(PlayerState playerState, List<TerrainData> allowedTerrain, GameState gameState)
	{
		return new PathFinderSettings
		{
			version = gameState.Version,
			playerState = playerState,
			allowedTerrain = allowedTerrain,
			gameState = gameState,
			isRequiredToUsePortToGoIntoWater = false,
			shouldAllowOccupiedTiles = true,
			shouldAllowEnemyTiles = true
		};
	}
}
