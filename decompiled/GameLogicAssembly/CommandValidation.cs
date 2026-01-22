using Polytopia.Data;

public static class CommandValidation
{
	public static bool CanAfford(this PlayerState playerState, int cost)
	{
		return playerState.Currency >= cost;
	}

	public static bool CanAfford(this PlayerState playerState, UnitData unitData)
	{
		return playerState.Currency >= unitData.cost;
	}

	public static bool CanAfford(this PlayerState playerState, ImprovementData improvementData)
	{
		return playerState.Currency >= improvementData.cost;
	}

	public static bool CanAfford(this PlayerState player, GameState gameState, TechData techData)
	{
		return player.Currency >= gameState.GameLogicData.GetTechPrice(techData, player, gameState);
	}

	public static bool HasUnit(GameState state, WorldCoordinates coordinates)
	{
		return state.Map.GetTile(coordinates).unit != null;
	}

	public static bool HasUnitTerrain(GameState state, WorldCoordinates coordinates, UnitData unit)
	{
		if (unit.movementTerrain.Count > 0)
		{
			foreach (TileData tileNeighbor in state.Map.GetTileNeighbors(coordinates))
			{
				if (unit.movementTerrain.Contains(tileNeighbor.terrain))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool HasCity(GameState state, WorldCoordinates coordinates)
	{
		return state.Map.GetTile(coordinates).HasImprovement(ImprovementData.Type.City);
	}

	public static bool CanCitySupportUnit(GameState state, WorldCoordinates coordinates)
	{
		TileData tile = state.Map.GetTile(coordinates);
		TileData tile2 = state.Map.GetTile(tile.rulingCityCoordinates);
		if (tile2 == null || tile2.improvement == null || tile2.improvement.type != ImprovementData.Type.City)
		{
			return false;
		}
		return state.Map.GetCityUnitCount(tile2.coordinates) < tile2.improvement.level + 1;
	}

	public static bool CanTrainUnit(this PlayerState player, GameState state, UnitData.Type unitType)
	{
		if (state.GameLogicData.TryGetData(unitType, out var data) && state.GameLogicData.GetUnlockedUnits(player, state, shouldIncludeHidden: false).Contains(data))
		{
			return true;
		}
		return false;
	}

	public static bool ContainsOpponentCityOrUnit(this TileData tile, byte playerId, byte opponentId)
	{
		if (tile.HasOpponentCity(playerId) && tile.owner == opponentId)
		{
			return true;
		}
		if (tile.HasOpponentUnit(playerId) && tile.unit.owner == opponentId)
		{
			return true;
		}
		return false;
	}

	public static bool HasTempleWithinRange(GameState gameState, WorldCoordinates coordinates, int range)
	{
		foreach (TileData item in gameState.Map.GetArea(coordinates, range, allowDiagonal: true))
		{
			if (item.HasImprovement(ImprovementData.Type.Temple) || item.HasImprovement(ImprovementData.Type.IceTemple) || item.HasImprovement(ImprovementData.Type.WaterTemple) || item.HasImprovement(ImprovementData.Type.ForestTemple) || item.HasImprovement(ImprovementData.Type.MountainTemple))
			{
				return true;
			}
		}
		return false;
	}
}
