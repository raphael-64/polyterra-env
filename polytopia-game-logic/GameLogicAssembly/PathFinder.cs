using System;
using System.Collections.Generic;
using System.Linq;
using Polytopia.Data;

public static class PathFinder
{
	public static List<WorldCoordinates> GetMoveOptions(this GameState gameState, WorldCoordinates start, int maxCost, UnitState unit)
	{
		if (unit != null && unit.HasFollower() && gameState.TryGetUnit(unit.follower, out var unit2) && unit2.GetMovement(gameState) < maxCost)
		{
			maxCost = unit2.GetMovement(gameState);
		}
		int num = maxCost * 10;
		Queue<WorldCoordinates> queue = new Queue<WorldCoordinates>();
		queue.Enqueue(start);
		List<WorldCoordinates> list = new List<WorldCoordinates>();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		dictionary[gameState.Map.GetIndexUnsafe(start)] = 0;
		if (unit.HasLeader())
		{
			return list;
		}
		PathFinderSettings settings = PathFinderSettings.CreateForUnit(unit, gameState);
		int num2 = 1000;
		while (queue.Count > 0 && num2-- > 0)
		{
			TileData tile = gameState.Map.GetTile(queue.Dequeue());
			WorldCoordinates coordinates = tile.coordinates;
			int num3 = dictionary[gameState.Map.GetIndexUnsafe(coordinates)];
			foreach (TileData item in ValidNeighbors(gameState.Map, coordinates, settings))
			{
				int num4 = num3 + item.GetMovementCost(gameState.Map, tile, settings);
				int indexUnsafe = gameState.Map.GetIndexUnsafe(item.coordinates);
				int value;
				bool flag = dictionary.TryGetValue(indexUnsafe, out value);
				if (!flag || value > num4)
				{
					dictionary[indexUnsafe] = num4;
					if (!flag && IsAllowedToFinishOnTile(settings, item))
					{
						list.Add(item.coordinates);
					}
					if (num4 < num)
					{
						queue.Enqueue(item.coordinates);
					}
				}
			}
		}
		return list;
	}

	private static bool IsAllowedToFinishOnTile(PathFinderSettings settings, TileData tile)
	{
		UnitState unit = tile.GetUnit(settings.gameState, settings.playerState.Id);
		if (!settings.shouldAllowOccupiedTiles && unit != null)
		{
			return false;
		}
		if (tile.HasImprovement(ImprovementData.Type.City))
		{
			if (settings.unit != null && settings.unit.HasAbility(UnitAbility.Type.Infiltrate, settings.gameState) && tile.HasOpponentCity(settings.playerState.Id))
			{
				Log.Verbose("[felix] Infiltrators are not allowed to enter cities", Array.Empty<object>());
				return false;
			}
			if (settings.playerState.HasPeaceWith(tile.owner) || settings.playerState.HasBrokenPeaceWith(tile.owner))
			{
				return false;
			}
		}
		return true;
	}

	public static List<WorldCoordinates> GetPath(this GameState gameState, WorldCoordinates start, WorldCoordinates destination, int maxCost, UnitState unit)
	{
		PathFinderSettings settings = PathFinderSettings.CreateForUnit(unit, gameState);
		return gameState.Map.GetPath(start, destination, maxCost, settings);
	}

	public static List<WorldCoordinates> GetPath(this MapData map, WorldCoordinates start, WorldCoordinates destination, int maxCost, PathFinderSettings settings)
	{
		int num = maxCost * 10;
		if (start.Equals(destination))
		{
			return new List<WorldCoordinates>(1) { start };
		}
		TileData tile = map.GetTile(destination);
		if (!IsAllowedToFinishOnTile(settings, tile))
		{
			return null;
		}
		MinHeap<WorldCoordinates>.Node node = new MinHeap<WorldCoordinates>.Node(start, MapDataExtensions.ManhattanDistance(start, destination));
		MinHeap<WorldCoordinates> open = new MinHeap<WorldCoordinates>();
		open.Push(node);
		int[] costSoFar = new int[map.Tiles.Length];
		WorldCoordinates[] cameFrom = new WorldCoordinates[map.Tiles.Length];
		int num2 = 1000;
		while (open.HasNext() && num2 > 0)
		{
			WorldCoordinates data = open.Pop().Data;
			if (data.Equals(destination))
			{
				return ReconstructPath(map, start, destination, cameFrom);
			}
			Step(map, ref open, ref cameFrom, ref costSoFar, data, destination, num, settings);
			num2--;
		}
		return null;
	}

	private static void Step(MapData map, ref MinHeap<WorldCoordinates> open, ref WorldCoordinates[] cameFrom, ref int[] costSoFar, WorldCoordinates current, WorldCoordinates target, float multipliedMaxCost, PathFinderSettings settings)
	{
		int num = costSoFar[map.GetIndexUnsafe(current)];
		TileData tile = map.GetTile(current);
		int indexUnsafe = map.GetIndexUnsafe(current);
		if ((float)costSoFar[indexUnsafe] >= multipliedMaxCost)
		{
			return;
		}
		foreach (TileData item in ValidNeighbors(map, current, settings))
		{
			int movementCost = item.GetMovementCost(map, tile, settings);
			int num2 = num + movementCost;
			int indexUnsafe2 = map.GetIndexUnsafe(item.coordinates);
			int num3 = costSoFar[indexUnsafe2];
			if (num3 <= 0 || num2 < num3)
			{
				costSoFar[indexUnsafe2] = num2;
				cameFrom[indexUnsafe2] = current;
				float expectedCost = ((settings.version < 10) ? ((float)(num2 + MapDataExtensions.ManhattanDistance(item.coordinates, target) * 10)) : ((float)num2 + WorldCoordinates.Distance(item.coordinates, target)));
				open.Push(new MinHeap<WorldCoordinates>.Node(item.coordinates, expectedCost));
			}
		}
	}

	private static IEnumerable<TileData> ValidNeighbors(MapData map, WorldCoordinates coordinates, PathFinderSettings settings)
	{
		TileData tile = map.GetTile(coordinates);
		List<TileData> list = new List<TileData>(8);
		if (settings.allowedTerrain == null || settings.allowedTerrain.Count() == 0)
		{
			return list;
		}
		List<TileData> tileNeighbors = map.GetTileNeighbors(coordinates);
		for (int i = 0; i < tileNeighbors.Count; i++)
		{
			TileData tileData = tileNeighbors[i];
			if (((!settings.shouldAllowAlliesTiles || !settings.playerState.HasPeaceWith(tileData.owner)) && !settings.shouldAllowEnemyTiles && PlayerState.AreDifferentPlayers(tileData.owner, settings.playerState.Id)) || !tileData.GetExplored(settings.playerState.Id))
			{
				continue;
			}
			UnitState unit = tileData.GetUnit(settings.gameState, settings.playerState.Id);
			if (!settings.shouldAllowOccupiedTiles && unit != null && unit.owner != settings.playerState.Id && !settings.playerState.HasPeaceWith(unit.owner))
			{
				if (settings.gameState.Version < 83)
				{
					if (settings.unit == null || !settings.unit.HasEffect(UnitEffect.Invisible))
					{
						continue;
					}
				}
				else if (settings.unit == null || !settings.unit.HasAbility(UnitAbility.Type.Hide, settings.gameState))
				{
					continue;
				}
			}
			if (settings.unit != null && settings.unit.HasAbility(UnitAbility.Type.Disloyal, settings.gameState) && tileData.owner != tile.owner)
			{
				continue;
			}
			if (tileData.improvement != null && settings.gameState.GameLogicData.TryGetData(tileData.improvement.type, out var data) && data != null && data.HasAbility(ImprovementAbility.Type.Bridge))
			{
				list.Add(tileData);
			}
			else
			{
				if ((settings.unit != null && tileData.IsWater && (!tileData.HasImprovement(ImprovementData.Type.Port) || tileData.owner != settings.playerState.Id) && !settings.unitData.HasAbility(UnitAbility.Type.Swim) && !settings.unitData.HasAbility(UnitAbility.Type.Fly)) || (settings.isRequiredToUsePortToGoIntoWater && !tile.IsWater && tileData.IsWater && (!tileData.HasImprovement(ImprovementData.Type.Port) || tileData.owner != settings.playerState.Id)) || (settings.unit != null && settings.unit.HasFollower() && (settings.version < 44 || tileData.IsWater) && tileData.HasImprovement(ImprovementData.Type.Port)))
				{
					continue;
				}
				foreach (TerrainData item in settings.allowedTerrain)
				{
					if (settings.version >= 40)
					{
						if (item.type == tileData.terrain || tileData.HasImprovement(ImprovementData.Type.City))
						{
							list.Add(tileData);
							break;
						}
					}
					else if (settings.version > 23)
					{
						if (item.type == tileData.terrain || (tileData.HasImprovement(ImprovementData.Type.City) && tileData.owner != 0))
						{
							list.Add(tileData);
							break;
						}
					}
					else if (item.type == tileData.terrain || tileData.HasImprovement(ImprovementData.Type.City))
					{
						list.Add(tileData);
						break;
					}
				}
			}
		}
		return list;
	}

	private static List<WorldCoordinates> ReconstructPath(MapData map, WorldCoordinates start, WorldCoordinates destination, WorldCoordinates[] cameFrom)
	{
		List<WorldCoordinates> list = new List<WorldCoordinates> { destination };
		WorldCoordinates worldCoordinates = destination;
		do
		{
			worldCoordinates = cameFrom[map.GetIndexUnsafe(worldCoordinates)];
			list.Add(worldCoordinates);
		}
		while (!worldCoordinates.Equals(start));
		return list;
	}
}
