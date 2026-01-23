using System;
using System.Collections.Generic;
using Polytopia.Data;

public static class TileDataExtensions
{
	public static int CalculateWork(this TileData tile, GameState gameState)
	{
		if (tile.improvement == null)
		{
			return 0;
		}
		return tile.CalculateWork(gameState, tile.improvement.level);
	}

	public static int CalculateWork(this TileData tile, GameState gameState, PlayerState playerState, int improvementLevel)
	{
		gameState.TryGetPlayer(tile.owner, out var playerState2);
		if (tile.owner == playerState.Id)
		{
			return tile.CalculateWork(gameState, improvementLevel);
		}
		if (tile.owner != 0 && tile.capitalOf == tile.owner)
		{
			if (tile.HasOpponentUnit(tile.owner))
			{
				return 0;
			}
			return playerState2.GetIncomeFromEmbassy(playerState, gameState);
		}
		return 0;
	}

	public static int CalculateWork(this TileData tile, GameState gameState, int improvementLevel)
	{
		if (tile.improvement != null && gameState.GameLogicData.TryGetData(tile.improvement.type, out var data))
		{
			if (data.type == ImprovementData.Type.City && tile.owner != 0)
			{
				int num = 0;
				if (tile.unit != null && tile.unit.owner != tile.owner)
				{
					return num;
				}
				num += tile.improvement.production;
				num += improvementLevel - 1;
				if (!gameState.TryGetPlayer(tile.owner, out var playerState))
				{
					return 0;
				}
				if (playerState.startTile == tile.coordinates)
				{
					num++;
					num += playerState.handicap - 1;
					foreach (PlayerState playerState2 in gameState.PlayerStates)
					{
						num += playerState.GetIncomeFromEmbassy(playerState2, gameState);
					}
				}
				if (tile.improvement.xp < 0)
				{
					num += tile.improvement.xp;
				}
				return Math.Max(num, 0);
			}
			if (data.work > 0 && data.adjacencyImprovements != null && data.adjacencyImprovements.Count > 0)
			{
				return Math.Max(data.work * improvementLevel, 0);
			}
			return data.work;
		}
		return 0;
	}

	public static int CalculateRawProduction(this TileData tile, GameState gameState)
	{
		int num = 0;
		if (tile.improvement != null && gameState.GameLogicData.TryGetData(tile.improvement.type, out var data))
		{
			if (data.type == ImprovementData.Type.City && tile.owner != 0)
			{
				num += tile.improvement.production;
				num += tile.improvement.level - 1;
				if (!gameState.TryGetPlayer(tile.owner, out var playerState))
				{
					return 0;
				}
				if (playerState.startTile == tile.coordinates)
				{
					num++;
				}
			}
			if (data.work > 0 && data.adjacencyImprovements != null && data.adjacencyImprovements.Count > 0)
			{
				return Math.Max(data.work * tile.improvement.level, 0);
			}
		}
		return num;
	}

	public static List<TileData> GetHealOptions(this TileData tileState, byte playerId, GameState state, bool includeCenter = false)
	{
		List<TileData> area = state.Map.GetArea(tileState.coordinates, 1, allowDiagonal: true, includeCenter);
		for (int i = 0; i < area.Count; i++)
		{
			TileData tileData = area[i];
			if (tileData.unit == null || tileData.unit.owner != playerId || (!tileData.unit.IsDamaged(state) && !tileData.unit.HasEffect(UnitEffect.Poisoned)))
			{
				area.RemoveAt(i--);
			}
		}
		return area;
	}
}
