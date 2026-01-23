using System;
using System.Collections.Generic;
using Polytopia.Data;

public static class MapDataExtensions
{
	public static int GetTileIndex(this MapData map, WorldCoordinates coordinates)
	{
		if (map == null)
		{
			return -1;
		}
		if (coordinates.X < 0 || coordinates.X >= map.Width || coordinates.Y < 0 || coordinates.Y >= map.Height)
		{
			return -1;
		}
		int num = coordinates.X + coordinates.Y * map.Width;
		if (num < 0 || num >= map.Tiles.Length)
		{
			return -1;
		}
		return num;
	}

	public static TileData GetTile(this MapData map, WorldCoordinates coordinates)
	{
		int tileIndex = map.GetTileIndex(coordinates);
		if (tileIndex == -1)
		{
			return null;
		}
		return map.Tiles[tileIndex];
	}

	public static int GetMultiplierImprovementsForImprovementOnTile(this MapData map, ImprovementData improvement, TileData tile)
	{
		int num = 0;
		foreach (TileData tileNeighbor in map.GetTileNeighbors(tile.coordinates))
		{
			if (tileNeighbor.improvement == null)
			{
				continue;
			}
			foreach (AdjacencyImprovements adjacencyImprovement in improvement.adjacencyImprovements)
			{
				if (adjacencyImprovement.improvement != null && adjacencyImprovement.improvement.type == tileNeighbor.improvement.type)
				{
					num++;
				}
			}
		}
		return num;
	}

	public static List<TileData> GetArea(this MapData map, WorldCoordinates center, int radius, bool allowDiagonal, bool includeCenter = true)
	{
		int num = radius * 2 + 1;
		List<TileData> list = new List<TileData>(num * num);
		map.GetAreaPreallocated(list, center, radius, allowDiagonal, includeCenter);
		return list;
	}

	public static void GetAreaPreallocated(this MapData map, List<TileData> area, WorldCoordinates center, int radius, bool allowDiagonal, bool includeCenter = true)
	{
		area.Clear();
		int i = -radius;
		int j = -radius;
		for (; i <= radius; i++)
		{
			for (; j <= radius; j++)
			{
				if ((i != 0 || j != 0 || includeCenter) && (allowDiagonal || Math.Abs(i) + Math.Abs(j) <= radius))
				{
					WorldCoordinates worldCoordinates = center + new WorldCoordinates(i, j);
					if (worldCoordinates.X >= 0 && worldCoordinates.X < map.Width && worldCoordinates.Y >= 0 && worldCoordinates.Y < map.Height)
					{
						int num = worldCoordinates.X + worldCoordinates.Y * map.Width;
						area.Add(map.Tiles[num]);
					}
				}
			}
			j = -radius;
		}
	}

	public static List<TileData> GetAreaFiltered(this MapData map, WorldCoordinates center, int radius, IEnumerable<TerrainData> filter, bool allowDiagonal, bool includeCenter = true)
	{
		List<TileData> list = new List<TileData>();
		int i = -radius;
		int j = -radius;
		for (; i <= radius; i++)
		{
			for (; j <= radius; j++)
			{
				if ((i == 0 && j == 0 && !includeCenter) || !(Math.Abs(i) + Math.Abs(j) <= radius || allowDiagonal))
				{
					continue;
				}
				WorldCoordinates worldCoordinates = center + new WorldCoordinates(i, j);
				if (worldCoordinates.X < 0 || worldCoordinates.X >= map.Width || worldCoordinates.Y < 0 || worldCoordinates.Y >= map.Height)
				{
					continue;
				}
				int num = worldCoordinates.X + worldCoordinates.Y * map.Width;
				TerrainData.Type terrain = map.Tiles[num].terrain;
				foreach (TerrainData item in filter)
				{
					if (item.type == terrain)
					{
						list.Add(map.Tiles[num]);
					}
				}
			}
			j = -radius;
		}
		return list;
	}

	public static TileData[] GetAreaSorted(this MapData map, WorldCoordinates center, int radius, bool allowDiagonal, bool includeCenter = true)
	{
		List<TileData> list = new List<TileData>();
		List<float> list2 = new List<float>();
		int i = -radius;
		int j = -radius;
		for (; i <= radius; i++)
		{
			for (; j <= radius; j++)
			{
				if ((i != 0 || j != 0 || includeCenter) && (Math.Abs(i) + Math.Abs(j) <= radius || allowDiagonal))
				{
					WorldCoordinates worldCoordinates = center + new WorldCoordinates(i, j);
					if (worldCoordinates.X >= 0 && worldCoordinates.X < map.Width && worldCoordinates.Y >= 0 && worldCoordinates.Y < map.Height)
					{
						int num = worldCoordinates.X + worldCoordinates.Y * map.Width;
						list.Add(map.Tiles[num]);
						list2.Add(Math.Abs(i) + Math.Abs(j));
					}
				}
			}
			j = -radius;
		}
		TileData[] array = list.ToArray();
		Array.Sort(list2.ToArray(), array);
		return array;
	}

	public static List<TileData> GetTileNeighbors(this MapData map, WorldCoordinates tile)
	{
		return map.GetArea(tile, 1, allowDiagonal: true, includeCenter: false);
	}

	public static TileData[] GetTileNeighborsSorted(this MapData map, WorldCoordinates tile)
	{
		return map.GetAreaSorted(tile, 1, allowDiagonal: true, includeCenter: false);
	}

	public static int GetIndexUnsafe(this MapData map, WorldCoordinates coordinates)
	{
		return coordinates.ToIndex(map.Width);
	}

	public static int GetIndexUnsafe(this MapData map, int x, int y)
	{
		return WorldCoordinates.ToIndex(x, y, map.Width);
	}

	public static string GenerateCityName(GameState state, WorldCoordinates coordinates, string language)
	{
		string text = string.Empty;
		string[] array = language.Split(',');
		int num = state.RandomHash.Range(0, int.MaxValue, coordinates.X, coordinates.Y);
		int num2 = 3 + num % 5;
		while (text.Length < num2)
		{
			num = state.RandomHash.Range(0, int.MaxValue, num);
			text += array[num % array.Length];
			if (text.Substring(0, 1) == " " || text.Substring(0, 1) == "-")
			{
				text = string.Empty;
			}
		}
		return char.ToUpper(text[0]) + text.Substring(1);
	}

	public static string GenerateCityName(GameState state, WorldCoordinates coordinates, TribeData tribeData)
	{
		int seed = state.RandomHash.Range(0, int.MaxValue, coordinates.X, coordinates.Y);
		return PolyLanguage.MakeWord(tribeData, 0f, seed);
	}

	public static WorldCoordinates ClosestCity(this MapData data, WorldCoordinates origin, byte owner)
	{
		int num = int.MaxValue;
		int num2 = -1;
		for (int i = 0; i < data.Tiles.Length; i++)
		{
			TileData tileData = data.Tiles[i];
			if (tileData.improvement != null && tileData.improvement.type == ImprovementData.Type.City && tileData.owner == owner)
			{
				int sqrMagnitude = (tileData.coordinates - origin).SqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					num2 = i;
				}
			}
		}
		if (num2 == -1)
		{
			return new WorldCoordinates(-1, -1);
		}
		return data.Tiles[num2].coordinates;
	}

	public static ushort GetMinimumMapSize(int players)
	{
		return (ushort)((int)Math.Ceiling(Math.Sqrt(players)) * 3);
	}

	public static int GetMaximumOpponentCountForMapSize(int mapSize)
	{
		if (mapSize == 0)
		{
			return 15;
		}
		return (int)Math.Pow(mapSize / 3, 2.0) - 1;
	}

	public static int ManhattanDistance(WorldCoordinates from, WorldCoordinates to)
	{
		int num = Math.Abs(from.X - to.X);
		int num2 = Math.Abs(from.Y - to.Y);
		return num + num2;
	}

	public static int ChebyshevDistance(WorldCoordinates from, WorldCoordinates to)
	{
		WorldCoordinates worldCoordinates = from - to;
		return Math.Max(Math.Abs(worldCoordinates.X), Math.Abs(worldCoordinates.Y));
	}

	public static int GetCityPotential(this GameState gameState, TileData tileData, PlayerState player)
	{
		ImprovementState improvement = tileData.improvement;
		if (improvement == null || improvement.type != ImprovementData.Type.City)
		{
			return 0;
		}
		int num = improvement.level * 10;
		List<TileData> area = gameState.Map.GetArea(tileData.coordinates, improvement.borderSize, allowDiagonal: true);
		for (int i = 0; i < area.Count; i++)
		{
			TileData tileData2 = area[i];
			if (tileData2.GetResource(gameState, player.Id) != null && tileData2.improvement == null)
			{
				num += 10;
			}
		}
		return num;
	}

	public static int GetCityUnitCount(this MapData mapData, WorldCoordinates cityCoordinates)
	{
		int num = 0;
		TileData[] tiles = mapData.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (tileData.unit != null && tileData.unit.home == cityCoordinates)
			{
				num++;
			}
		}
		return num;
	}

	public static void GetPlayerUnits(this MapData mapData, byte playerId, List<UnitState> units)
	{
		if (units == null)
		{
			units = new List<UnitState>();
		}
		units.Clear();
		TileData[] tiles = mapData.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (tileData.unit != null && tileData.unit.owner == playerId)
			{
				units.Add(tileData.unit);
			}
		}
	}

	public static int CountPlayerUnitsWithinOpponentBorders(this MapData mapData, byte playerId, byte opponentId)
	{
		int num = 0;
		TileData[] tiles = mapData.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (tileData.owner == opponentId && tileData.unit != null && tileData.unit.owner == playerId)
			{
				num++;
			}
		}
		return num;
	}

	private static void ResetRoutes(this MapData mapData, List<TileData> routeTiles)
	{
		TileData[] tiles = mapData.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (tileData.hasRoute)
			{
				tileData.hasRoute = false;
				tileData.hadRoute = true;
				routeTiles.Add(tileData);
			}
		}
	}

	public static void UpdateRoutes(this GameState gameState, List<TileData> changedTiles)
	{
		List<TileData> list = new List<TileData>();
		gameState.Map.ResetRoutes(list);
		List<TileData> list2 = new List<TileData>(20);
		List<TileData> list3 = new List<TileData>();
		List<TileData> list4 = new List<TileData>();
		List<TerrainData> allowedTerrain = new List<TerrainData>(3);
		foreach (PlayerState playerState in gameState.PlayerStates)
		{
			if (playerState.Id == byte.MaxValue)
			{
				continue;
			}
			gameState.Map.GetPlayerEmpireTiles(playerState.Id, list2);
			list3.Clear();
			list4.Clear();
			List<TerrainData> unlockedMovements = gameState.GameLogicData.GetUnlockedMovements(playerState);
			foreach (TileData item in list2)
			{
				if (item.improvement != null && gameState.GameLogicData.TryGetData(item.improvement.type, out var data))
				{
					if (data.IsRouteOpener())
					{
						list3.Add(item);
					}
					if (data.type == ImprovementData.Type.City)
					{
						list4.Add(item);
					}
				}
			}
			for (int i = 0; i < list3.Count; i++)
			{
				TileData tileData = list3[i];
				if (!gameState.GameLogicData.TryGetData(tileData.improvement.type, out var data2))
				{
					continue;
				}
				for (int j = i + 1; j < list3.Count; j++)
				{
					TileData destinationRouter = list3[j];
					FindPath(tileData, destinationRouter, unlockedMovements, playerState);
				}
				if (gameState.Version == 41 && ChebyshevDistance(tileData.coordinates, tileData.rulingCityCoordinates) == 1)
				{
					FindPath(tileData, gameState.Map.GetTile(tileData.rulingCityCoordinates), unlockedMovements, playerState);
				}
				if (!data2.HasAbility(ImprovementAbility.Type.Network))
				{
					continue;
				}
				ushort num = 0;
				for (int k = 0; k < list4.Count; k++)
				{
					TileData destinationRouter2 = list4[k];
					if (FindPath(tileData, destinationRouter2, unlockedMovements, playerState))
					{
						num++;
					}
				}
				if (gameState.Version < 50)
				{
					tileData.improvement.level = num;
				}
			}
		}
		foreach (TileData item2 in list)
		{
			if (!item2.hasRoute && !changedTiles.Contains(item2))
			{
				changedTiles.Add(item2);
			}
			item2.hadRoute = false;
		}
		bool FindPath(TileData originRouter, TileData tileData2, List<TerrainData> playerAllowedTerrain, PlayerState player)
		{
			if (!gameState.GameLogicData.TryGetData(originRouter.improvement.type, out var data3))
			{
				return false;
			}
			if (!gameState.GameLogicData.TryGetData(tileData2.improvement.type, out var data4))
			{
				return false;
			}
			if (gameState.Version < 40)
			{
				int num2 = 5;
				if ((originRouter.coordinates - tileData2.coordinates).SqrMagnitude > num2 * num2)
				{
					return false;
				}
			}
			else if (ChebyshevDistance(originRouter.coordinates, tileData2.coordinates) > data3.range)
			{
				return false;
			}
			allowedTerrain.Clear();
			foreach (TerrainData route in data3.routes)
			{
				if ((data4.routes.Contains(route) && playerAllowedTerrain.Contains(route)) || data4.type == ImprovementData.Type.City)
				{
					allowedTerrain.Add(route);
				}
			}
			if (allowedTerrain.Count <= 0)
			{
				return false;
			}
			PathFinderSettings settings = PathFinderSettings.CreateRouter(player, allowedTerrain, gameState.Version, gameState);
			List<WorldCoordinates> path = gameState.Map.GetPath(originRouter.coordinates, tileData2.coordinates, data3.range, settings);
			if (path != null && path.Count > 0)
			{
				foreach (WorldCoordinates item3 in path)
				{
					TileData tile = gameState.Map.GetTile(item3);
					tile.hasRoute = true;
					if (!changedTiles.Contains(tile))
					{
						changedTiles.Add(tile);
					}
				}
				return true;
			}
			return false;
		}
	}

	public static void FindConnectedCities(this GameState gameState, byte playerId, List<TileData> cityTiles, List<TileData> connectedCities)
	{
		if (playerId == byte.MaxValue || !gameState.TryGetPlayer(playerId, out var playerState))
		{
			return;
		}
		Stack<TileData> stack = new Stack<TileData>();
		HashSet<TileData> hashSet = new HashSet<TileData>();
		TileData tile = gameState.Map.GetTile(playerState.startTile);
		if (tile.owner == playerId)
		{
			stack.Push(tile);
		}
		int num = 10000;
		while (stack.Count > 0 && num-- > 0)
		{
			TileData tileData = stack.Pop();
			hashSet.Add(tileData);
			if (tileData.HasImprovement(ImprovementData.Type.City) && tileData != tile)
			{
				connectedCities.Add(tileData);
			}
			foreach (TileData tileNeighbor in gameState.Map.GetTileNeighbors(tileData.coordinates))
			{
				bool flag = tileNeighbor.owner != playerId && tileNeighbor.owner != 0 && !playerState.HasPeaceWith(tileNeighbor.owner);
				if (tileData.HasMatchingTransportPath(tileNeighbor, gameState) && !flag && !hashSet.Contains(tileNeighbor))
				{
					stack.Push(tileNeighbor);
				}
			}
		}
		if (num == 0)
		{
			Log.Error("Too many iterations while looking for connected cities", Array.Empty<object>());
		}
	}

	public static int CountCities(GameState state)
	{
		int num = 0;
		TileData[] tiles = state.Map.Tiles;
		int num2 = tiles.Length;
		for (int i = 0; i < num2; i++)
		{
			TileData tileData = tiles[i];
			if (tileData.improvement != null && tileData.improvement.type == ImprovementData.Type.City)
			{
				num++;
			}
		}
		return num;
	}

	public static void GetPlayerCityTiles(this MapData mapData, byte playerId, List<TileData> cityTiles)
	{
		if (cityTiles == null)
		{
			cityTiles = new List<TileData>();
		}
		cityTiles.Clear();
		TileData[] tiles = mapData.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (tileData.owner == playerId && tileData.HasImprovement(ImprovementData.Type.City))
			{
				cityTiles.Add(tileData);
			}
		}
	}

	public static void GetPlayerEmpireTiles(this MapData mapData, byte playerId, List<TileData> empireTiles)
	{
		if (empireTiles == null)
		{
			empireTiles = new List<TileData>();
		}
		empireTiles.Clear();
		TileData[] tiles = mapData.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (tileData.owner == playerId)
			{
				empireTiles.Add(tileData);
			}
		}
	}

	public static void GetPlayerVisibleTiles(this MapData mapData, byte playerId, List<TileData> visibleTiles)
	{
		if (visibleTiles == null)
		{
			visibleTiles = new List<TileData>();
		}
		visibleTiles.Clear();
		TileData[] tiles = mapData.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (tileData.GetExplored(playerId))
			{
				visibleTiles.Add(tileData);
			}
		}
	}

	public static int GetNumberOfFrozenTilesOwnedByPlayer(this GameState gameState, byte playerId)
	{
		int num = 0;
		TileData[] tiles = gameState.Map.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (tileData.owner == playerId && tileData.IsFrozen(gameState))
			{
				num++;
			}
		}
		return num;
	}

	public static int CountIceTiles(this GameState gameState)
	{
		int num = 0;
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			if (gameState.Map.Tiles[i].IsFrozen(gameState))
			{
				num++;
			}
		}
		return num;
	}

	public static bool CanSupportMoreUnits(this TileData cityTile, GameState gameState)
	{
		return gameState.Map.GetCityUnitCount(cityTile.coordinates) < cityTile.improvement.level + 1;
	}
}
