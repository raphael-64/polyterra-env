using System;
using System.Collections.Generic;
using System.Diagnostics;
using Polytopia.Data;
using PolytopiaBackendBase.Game;

public class MapGenerator
{
	public enum MapGenerationType
	{
		Default,
		Tutorial
	}

	private struct StartResourceData
	{
		public int playerId;

		public int minResourcesCount;

		public ResourceData resource;

		public TerrainData.Type terrain;
	}

	public const string LOG_PREFIX = "<color=#639ad8>[MapGenerator]</color>";

	public const int MINIMUM_DOMAIN_SIZE = 3;

	public const int DESIRED_DOMAIN_SIZE = 5;

	public const int DEFAULT_CLIMATE = 2;

	public Random random;

	public MapGenerationType mapGenerationType;

	public void Generate(GameState state, MapGeneratorSettings settings, Action onComplete = null)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		stopwatch.Start();
		int i = 0;
		float num = float.MaxValue;
		int num2 = -1;
		int num3 = -1;
		for (; i < settings.equalityIterations; i++)
		{
			num3 = new Random().Next();
			GenerateInternal(num3, state, settings);
			float inequality = GetInequality(state);
			Log.Verbose("Map inequality is {2}/{3} Attempt {0}/{1}", new object[4] { i, settings.equalityIterations, inequality, settings.equalityLimit });
			if (inequality < num)
			{
				num = inequality;
				num2 = num3;
			}
			if (inequality < settings.equalityLimit)
			{
				num2 = num3;
				break;
			}
		}
		if (num3 != num2)
		{
			GenerateInternal(num2, state, settings);
		}
		_ = state.Map;
		for (int j = 0; j < state.PlayerStates.Count; j++)
		{
			PlayerState playerState = state.PlayerStates[j];
			if (playerState.Id == byte.MaxValue)
			{
				playerState.score = 0u;
			}
			else
			{
				playerState.score = (uint)playerState.GetScore(state).totalScore;
			}
		}
		stopwatch.Stop();
		Log.Verbose("{0} Equal map generated in {1} iterations, {2} seconds. Inequality rating is {3}", new object[4]
		{
			"<color=#639ad8>[MapGenerator]</color>",
			i,
			(float)stopwatch.ElapsedMilliseconds / 1000f,
			num
		});
		onComplete?.Invoke();
	}

	public void GenerateWithSeed(int seed, GameState state, MapGeneratorSettings settings, Action onComplete = null)
	{
		GenerateInternal(seed, state, settings);
		onComplete?.Invoke();
	}

	private MapData GenerateInternal(int seed, GameState state, MapGeneratorSettings settings)
	{
		Log.Verbose("Generating map with random seed {0}", new object[1] { seed });
		random = new Random(seed);
		Stopwatch stopwatch = Stopwatch.StartNew();
		stopwatch.Start();
		MapData map = state.Map;
		if (state.Settings.RulesGameMode == GameMode.Tutorial)
		{
			mapGenerationType = MapGenerationType.Tutorial;
		}
		else
		{
			mapGenerationType = MapGenerationType.Default;
		}
		Log.Info("{0} Generating world of size {1}x{2}...", new object[3] { "<color=#639ad8>[MapGenerator]</color>", map.Width, map.Height });
		int num = 0;
		for (int i = 0; i < map.Height; i++)
		{
			for (int j = 0; j < map.Width; j++)
			{
				map.Tiles[num++] = new TileData
				{
					coordinates = new WorldCoordinates(j, i),
					terrain = TerrainData.Type.Water,
					climate = 0,
					altitude = -1,
					improvement = null,
					resource = null,
					unit = null,
					owner = 0
				};
			}
		}
		List<int> list = new List<int>();
		Log.Verbose("{0} Generating player capitals..", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		List<int> list2 = GeneratePlayerCapitals(map.Width, state.PlayerCount);
		int num2 = (int)((float)(int)map.Width * 0.33f);
		int num3 = num2 * num2 - list2.Count;
		int num4 = (int)(settings.postTerrainCityDensity * (float)num3);
		Log.Verbose("{0} Will attempt to add another {1} cities apart from capitals, max {2}", new object[3] { "<color=#639ad8>[MapGenerator]</color>", num4, num3 });
		int num5 = Math.Max(settings.minSuburbCount, Math.Min(settings.maxSuburbCount, num4 / state.PlayerCount));
		Log.Verbose("{0} Generating {1} player suburbs..", new object[2] { "<color=#639ad8>[MapGenerator]</color>", num5 });
		List<int> list3 = GenerateSuburbs(state, list2, num5, list);
		Log.Verbose("{0} Generating player capitals..", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		List<int> list4 = new List<int>(list2);
		list4.AddRange(list3);
		int num6 = (int)((float)(num3 - list3.Count) * settings.preTerrainCityDensity);
		GeneratePreTerrainCities(map, list4, num6);
		list.AddRange(list4);
		List<int> list5 = new List<int>(list);
		list5.AddRange(GetCapitalNeighborIndices(list2, map.Width));
		Log.Verbose("{0} Generating land noise...", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		float[] noise = GenerateNoise(map.Width, map.Height, list5, settings.wetness);
		Log.Verbose("{0} Smoothing land...", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		SmoothNoise(map.Width, map.Height, noise, list, settings.smoothIterations, settings.surroundingSpaceValue);
		Log.Verbose("{0} Generating terrain from noise data...", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		SetTerrainFromNoise(map, state, noise, list4, list2, list, settings.wetness, settings.shallowPercentOfWater);
		Log.Verbose("{0} Generating post terrain cities...", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		int maxCityCount = num4 - num6;
		AddPostTerrainCities(map, maxCityCount);
		Log.Verbose("{0} Expanding climates...", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		AddClimates(map, state.PlayerStates, state.Version, settings, list);
		Log.Verbose("{0} Adding Terrain...", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		AddTerrain(map, state.PlayerStates, state.Version, settings, list);
		Log.Verbose("{0} Converting water into ocean...", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		MakeOcean(map, state.GameLogicData, settings.shallowPercentOfWater == 0f);
		Log.Verbose("{0} Adding resources...", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		AddResources(map, state, settings.richness);
		Log.Verbose("{0} Deciding start resources and terrain", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		List<StartResourceData> startResourcesForPlayers = GetStartResourcesForPlayers(state, settings);
		for (int k = 0; k < 10; k++)
		{
			Log.Verbose("{0} Preparing start areas...", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
			bool flag = PrepareStartAreas(map, state, settings, startResourcesForPlayers, list);
			Log.Verbose("{0} Converting water into ocean again after start areas...", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
			MakeOcean(map, state.GameLogicData, settings.shallowPercentOfWater == 0f);
			if (!flag)
			{
				break;
			}
		}
		Log.Verbose("{0} Adding ruins...", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		AddRuins(map, map.Tiles.Length / 40);
		Log.Verbose("{0} Preparing alien climates...", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
		PrepareAlienClimates(state);
		stopwatch.Stop();
		Log.Verbose("{0} Generated a {1}x{2} world in {3} seconds", new object[4]
		{
			"<color=#639ad8>[MapGenerator]</color>",
			map.Width,
			map.Height,
			(float)stopwatch.ElapsedMilliseconds / 1000f
		});
		return map;
	}

	public static float GetCityRating(TileData tileData, GameState gameState, out Dictionary<WorldCoordinates, int> tiles)
	{
		int maxDistance = gameState.Map.Width / 2;
		int walkableTiles = 0;
		Dictionary<WorldCoordinates, int> visitedTiles = new Dictionary<WorldCoordinates, int>();
		List<TileData> tilesToCheck = new List<TileData>();
		gameState.TryGetPlayer(tileData.owner, out var playerState);
		gameState.GameLogicData.TryGetData(playerState.tribe, out var tribeData);
		int num = 0;
		visitedTiles.Add(tileData.coordinates, num);
		float result = 100f + rateTile(tileData, num) + (float)walkableTiles / 4f;
		tiles = visitedTiles;
		return result;
		float rateNextTile()
		{
			if (tilesToCheck.Count > 0)
			{
				TileData tileData2 = tilesToCheck[0];
				tilesToCheck.RemoveAt(0);
				visitedTiles.TryGetValue(tileData2.coordinates, out var value);
				return rateTile(tileData2, value);
			}
			return 0f;
		}
		float rateTile(TileData tile, int distance)
		{
			float num2 = 0f;
			if (distance > 0)
			{
				walkableTiles++;
				if (tile.HasImprovement(ImprovementData.Type.Ruin))
				{
					num2 += 10f / (float)distance;
				}
				if (tile.HasImprovement(ImprovementData.Type.City))
				{
					gameState.TryGetPlayer(tile.owner, out var _);
					num2 = ((tile.owner != 0) ? (num2 - 50f / (float)distance) : (num2 + (100f + (float)(30 / distance))));
				}
			}
			if (distance < maxDistance)
			{
				foreach (TileData tileNeighbor in gameState.Map.GetTileNeighbors(tile.coordinates))
				{
					if (!visitedTiles.ContainsKey(tileNeighbor.coordinates))
					{
						int value = distance + 1;
						if (!tileNeighbor.CanBeAccessedWithUnlockedTech(gameState, tribeData.startingTech))
						{
							value = 0;
							visitedTiles.Add(tileNeighbor.coordinates, value);
						}
						else
						{
							visitedTiles.Add(tileNeighbor.coordinates, value);
							tilesToCheck.Add(tileNeighbor);
						}
					}
				}
			}
			return num2 + rateNextTile();
		}
	}

	private float GetInequality(GameState gameState)
	{
		float num = float.MinValue;
		float num2 = float.MaxValue;
		foreach (PlayerState playerState in gameState.PlayerStates)
		{
			TileData tile = gameState.Map.GetTile(playerState.startTile);
			if (tile != null)
			{
				Dictionary<WorldCoordinates, int> tiles;
				float cityRating = GetCityRating(tile, gameState, out tiles);
				if (cityRating < num2)
				{
					num2 = cityRating;
				}
				if (cityRating > num)
				{
					num = cityRating;
				}
			}
		}
		if (num2 == 0f)
		{
			num2 = 0.001f;
		}
		float num3 = num / num2;
		Log.Verbose("Map Inequality {0}, highest {1}, lowest {2}", new object[3] { num3, num, num2 });
		return num3;
	}

	private List<int> GetCapitalNeighborIndices(List<int> capitals, int mapWidth)
	{
		List<int> list = new List<int>();
		foreach (int capital in capitals)
		{
			list.Add(capital + 1);
			list.Add(capital - 1);
			list.Add(capital + mapWidth);
			list.Add(capital - mapWidth);
		}
		return list;
	}

	private float[] GenerateNoise(int width, int height, List<int> landIndices, float wetness)
	{
		float[] array = new float[width * height];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (landIndices.Contains(i) ? (wetness + random.Value() * (1f - wetness)) : random.Value());
		}
		return array;
	}

	private void SetTerrainFromNoise(MapData map, GameState state, float[] noise, List<int> cityTileIndices, List<int> capitalTileIndices, List<int> landTileIndices, float wetness, float percentShallows)
	{
		float[] array = new float[noise.Length];
		noise.CopyTo(array, 0);
		Array.Sort(array);
		int num = (int)Math.Round(wetness * (float)(noise.Length - 1));
		float num2 = array[num];
		int num3 = (int)Math.Round((1f - percentShallows) * (float)num);
		float num4 = array[num3];
		for (int i = 0; i < map.Tiles.Length; i++)
		{
			TileData tileData = map.Tiles[i];
			if (cityTileIndices.Contains(i))
			{
				int num5 = capitalTileIndices.IndexOf(i);
				if (num5 >= 0)
				{
					SetTileAsCapital(state, state.PlayerStates[num5], tileData);
				}
				else
				{
					SetTileAsCity(tileData);
				}
				continue;
			}
			float num6 = noise[i];
			if (num6 < num4)
			{
				tileData.altitude = -2;
				tileData.terrain = TerrainData.Type.Ocean;
			}
			else if (num6 < num2)
			{
				tileData.altitude = -1;
				tileData.terrain = TerrainData.Type.Water;
			}
			else
			{
				tileData.altitude = 1;
				tileData.terrain = TerrainData.Type.Field;
			}
		}
		foreach (int landTileIndex in landTileIndices)
		{
			TileData obj = map.Tiles[landTileIndex];
			obj.altitude = 1;
			obj.terrain = TerrainData.Type.Field;
		}
	}

	private List<int> GeneratePlayerCapitals(int width, int playerCount)
	{
		int num = (int)Math.Ceiling(Math.Sqrt(playerCount));
		int num2 = width / num;
		if (num2 < 3)
		{
			throw new Exception("Domain size " + num2 + " is too small to allow for an isolated capital for all " + playerCount + " players");
		}
		int val = width - num2 * num;
		int num3 = num * num;
		List<int> list = new List<int>(num3);
		for (int i = 0; i < num3; i++)
		{
			list.Add(i);
		}
		int[] probabilities = new int[width * width];
		for (int j = 1; j < num; j++)
		{
			for (int k = 1; k < num; k++)
			{
				int num4 = Math.Min(val, Math.Max(1, Math.Min(val, k) - 1));
				int num5 = Math.Min(val, Math.Max(1, Math.Min(val, j) - 1));
				int num6 = k * num2 + num4;
				int num7 = j * num2 + num5;
				AddDistanceToProbabilityTable(probabilities, width, new WorldCoordinates(num6 - 1, num7 - 1));
			}
		}
		List<int> list2 = new List<int>(playerCount);
		for (int l = 0; l < playerCount; l++)
		{
			int index = random.Range(0, list.Count);
			int index2 = list[index];
			list.RemoveAt(index);
			WorldCoordinates worldCoordinates = WorldCoordinates.FromIndex(index2, num);
			int num8 = Math.Min(val, Math.Max(1, Math.Min(val, worldCoordinates.X) - 1));
			int num9 = Math.Min(val, Math.Max(1, Math.Min(val, worldCoordinates.Y) - 1));
			int num10 = worldCoordinates.X * num2 + num8;
			int num11 = worldCoordinates.Y * num2 + num9;
			int num12 = ((num2 == 3) ? 1 : 2);
			int num13 = ((num2 <= 5) ? 1 : 2);
			int startX = Math.Max(num12, num10 + num13);
			int endX = Math.Min(width - num12, num10 + num2 - num13);
			int startY = Math.Max(num12, num11 + num13);
			int endY = Math.Min(width - num12, num11 + num2 - num13);
			int max = CalculateProbabilityInRange(probabilities, width, startX, endX, startY, endY);
			int value = random.Range(0, max);
			int num14 = IndexForProbabilityValueInRange(probabilities, width, value, startX, endX, startY, endY);
			WorldCoordinates worldCoordinates2 = WorldCoordinates.FromIndex(num14, width);
			Log.Verbose("{0} Adding capital at {1}, {2} for player {3}", new object[4] { "<color=#639ad8>[MapGenerator]</color>", worldCoordinates2.X, worldCoordinates2.Y, l });
			list2.Add(num14);
		}
		return list2;
	}

	private void AddDistanceToProbabilityTable(int[] probabilities, int width, WorldCoordinates coordinates)
	{
		for (int i = 0; i < probabilities.Length; i++)
		{
			int num = MapDataExtensions.ChebyshevDistance(WorldCoordinates.FromIndex(i, width), coordinates);
			int num2 = Math.Max(1, num - 1);
			int num3 = num2 * num2 * num2 * num2;
			int num4 = probabilities[i];
			probabilities[i] = ((num4 == 0) ? num3 : Math.Min(num4, num3));
		}
	}

	private int CalculateProbabilityInRange(int[] probabilities, int width, int startX, int endX, int startY, int endY)
	{
		int num = 0;
		for (int i = startY; i < endY; i++)
		{
			for (int j = startX; j < endX; j++)
			{
				int num2 = WorldCoordinates.ToIndex(j, i, width);
				num += Math.Max(1, probabilities[num2]);
			}
		}
		return num;
	}

	private int IndexForProbabilityValueInRange(int[] probabilities, int width, int value, int startX, int endX, int startY, int endY)
	{
		int num = 0;
		for (int i = startY; i < endY; i++)
		{
			for (int j = startX; j < endX; j++)
			{
				int num2 = WorldCoordinates.ToIndex(j, i, width);
				num += Math.Max(1, probabilities[num2]);
				if (value < num)
				{
					return num2;
				}
			}
		}
		throw new Exception($"Could not find index for value {value}");
	}

	private void SetTileAsCapital(GameState gameState, PlayerState playerState, TileData tile)
	{
		if (tile == null || !gameState.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			return;
		}
		playerState.startTile = tile.coordinates;
		playerState.cities = 0;
		Log.Verbose("{0} Set StartTile of player: {1} to: {2}", new object[3] { "<color=#639ad8>[MapGenerator]</color>", playerState.Id, playerState.startTile });
		tile.terrain = TerrainData.Type.Field;
		tile.climate = data.climate;
		tile.skinType = playerState.skinType;
		tile.altitude = 1;
		tile.resource = null;
		tile.unit = null;
		tile.improvement = new ImprovementState
		{
			type = ImprovementData.Type.City,
			founded = 0,
			level = 1,
			borderSize = 1,
			production = 1
		};
		if (gameState.Version <= 15)
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(gameState, tile.coordinates, data.language);
		}
		else
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(gameState, tile.coordinates, data);
		}
		tile.capitalOf = playerState.Id;
		if (data.bonus == TribeData.BonusEnum.CityLevel3)
		{
			if (gameState.Version < 45)
			{
				tile.improvement.level = 3;
				tile.improvement.AddReward(CityReward.CityWall);
			}
			else
			{
				tile.improvement.level = 2;
			}
		}
		if (data.bonus == TribeData.BonusEnum.Spores)
		{
			foreach (TileData item in gameState.Map.GetArea(tile.coordinates, 1, allowDiagonal: true, includeCenter: false))
			{
				if (item.terrain == TerrainData.Type.Field)
				{
					ImprovementState improvement = new ImprovementState
					{
						type = ImprovementData.Type.Fungi,
						level = 0,
						xp = 0,
						founded = (ushort)gameState.CurrentTurn,
						founder = item.owner
					};
					item.improvement = improvement;
					break;
				}
			}
		}
		ActionUtils.RuleArea(gameState, playerState, tile, shouldUseActions: false);
		ActionUtils.ExploreFromTile(gameState, playerState, tile, 2, shouldUseActions: false);
	}

	private bool IsWithinRangeOfIndices(WorldCoordinates coordinates, List<int> indices, int width, int range = 2)
	{
		foreach (int index in indices)
		{
			WorldCoordinates worldCoordinates = WorldCoordinates.FromIndex(index, width);
			WorldCoordinates worldCoordinates2 = coordinates - worldCoordinates;
			if (Math.Abs(worldCoordinates2.X) <= range && Math.Abs(worldCoordinates2.Y) <= range)
			{
				return true;
			}
		}
		return false;
	}

	private void AddLandIndicesBetweenTiles(WorldCoordinates fromCoordinate, int toIndex, List<int> landIndices, int width)
	{
		WorldCoordinates worldCoordinates = WorldCoordinates.FromIndex(toIndex, width);
		int num = 1000;
		WorldCoordinates worldCoordinates2 = fromCoordinate;
		while (num-- > 0 && !(worldCoordinates2 == worldCoordinates))
		{
			int item = worldCoordinates2.ToIndex(width);
			if (!landIndices.Contains(item))
			{
				landIndices.Add(item);
			}
			WorldCoordinates worldCoordinates3 = worldCoordinates - worldCoordinates2;
			int num2 = Math.Abs(worldCoordinates3.X);
			int num3 = Math.Abs(worldCoordinates3.Y);
			int num4 = Math.Sign(worldCoordinates3.X);
			int num5 = Math.Sign(worldCoordinates3.Y);
			if (num2 + num3 == 1)
			{
				worldCoordinates2 += worldCoordinates3;
				break;
			}
			int num6 = random.Range(1, 4);
			if ((num6 & 1) > 0)
			{
				worldCoordinates2.X = Math.Min(width - 1, Math.Max(0, worldCoordinates2.X + ((num4 != 0) ? num4 : ((random.Value() > 0.5f) ? 1 : (-1)))));
			}
			if ((num6 & 2) > 0)
			{
				worldCoordinates2.Y = Math.Min(width - 1, Math.Max(0, worldCoordinates2.Y + ((num5 != 0) ? num5 : ((random.Value() > 0.5f) ? 1 : (-1)))));
			}
		}
	}

	private List<int> GenerateSuburbs(GameState state, List<int> capitalIndices, int suburbCount, List<int> landIndices)
	{
		MapData map = state.Map;
		List<int> list = new List<int>();
		if (suburbCount == 0)
		{
			Log.Verbose("Created 0 suburbs", Array.Empty<object>());
			return list;
		}
		List<int>[] array = new List<int>[state.PlayerCount];
		List<int> list2 = new List<int>();
		for (int i = 0; i < suburbCount; i++)
		{
			for (int j = 0; j < state.PlayerCount; j++)
			{
				if (array[j] == null)
				{
					array[j] = new List<int>(suburbCount);
				}
				List<int> list3 = array[j];
				WorldCoordinates worldCoordinates = WorldCoordinates.FromIndex(capitalIndices[j], map.Width);
				for (int k = 3; (float)k < (float)(int)map.Width * 0.6f; k++)
				{
					list2.Clear();
					int num = worldCoordinates.Y - k;
					int num2 = worldCoordinates.Y + k;
					for (int l = num; l <= num2 && l < map.Height - 1; l++)
					{
						if (l < 1)
						{
							continue;
						}
						int num3 = worldCoordinates.X - k;
						int num4 = worldCoordinates.X + k;
						for (int m = num3; m <= num4 && m < map.Width - 1; m++)
						{
							if (m >= 1 && (l == num || l == num2 || m == num3 || m == num4))
							{
								WorldCoordinates coordinates = new WorldCoordinates(m, l);
								if (!IsWithinRangeOfIndices(coordinates, capitalIndices, map.Width) && !IsWithinRangeOfIndices(coordinates, list, map.Width) && !IsWithinRangeOfIndices(coordinates, list3, map.Width))
								{
									list2.Add(coordinates.ToIndex(map.Width));
								}
							}
						}
					}
					if (list2.Count > 0)
					{
						int num5 = list2[random.Range(0, list2.Count)];
						list3.Add(num5);
						list.Add(num5);
						state.GameLogicData.TryGetData(state.PlayerStates[j].tribe, out var data);
						Log.Verbose("{0} Added suburb for {1} : {4} at {2}, distance {3}", new object[5]
						{
							"<color=#639ad8>[MapGenerator]</color>",
							j,
							WorldCoordinates.FromIndex(num5, map.Width),
							k,
							data.type
						});
						RemoveIndicesNearIndex(list2, num5, map.Width, 2);
						break;
					}
				}
			}
		}
		for (int n = 0; n < state.PlayerCount; n++)
		{
			List<int> list4 = array[n];
			int num6 = capitalIndices[n];
			WorldCoordinates worldCoordinates2 = WorldCoordinates.FromIndex(num6, map.Width);
			if (list4.Count != suburbCount)
			{
				Log.Verbose("Only managed to make {0} of {1} unique suburbs for player {2}, connecting to other players suburbs and/or capitals", new object[3] { list4.Count, suburbCount, n });
				_ = list4.Count;
				for (int num7 = 0; num7 < suburbCount; num7++)
				{
					int num8 = map.Width;
					int num9 = -1;
					foreach (int item in list)
					{
						if (num6 != item && !list4.Contains(item))
						{
							WorldCoordinates to = WorldCoordinates.FromIndex(item, map.Width);
							int num10 = MapDataExtensions.ChebyshevDistance(worldCoordinates2, to);
							if (num10 < num8)
							{
								num8 = num10;
								num9 = item;
							}
						}
					}
					foreach (int capitalIndex in capitalIndices)
					{
						if (num6 != capitalIndex && !list4.Contains(capitalIndex))
						{
							WorldCoordinates to2 = WorldCoordinates.FromIndex(capitalIndex, map.Width);
							int num11 = MapDataExtensions.ChebyshevDistance(worldCoordinates2, to2);
							if (num11 < num8)
							{
								num8 = num11;
								num9 = capitalIndex;
							}
						}
					}
					if (num9 >= 0)
					{
						list4.Add(num9);
						continue;
					}
					Log.Error("Only managed to find a total of {0} of {1} suburbs for player {2}", new object[3] { list4.Count, suburbCount, n });
					break;
				}
			}
			WorldCoordinates worldCoordinates3 = worldCoordinates2;
			foreach (int item2 in list4)
			{
				worldCoordinates3 += WorldCoordinates.FromIndex(item2, map.Width);
			}
			WorldCoordinates fromCoordinate = worldCoordinates3 / (list4.Count + 1);
			AddLandIndicesBetweenTiles(fromCoordinate, num6, landIndices, map.Width);
			foreach (int item3 in list4)
			{
				AddLandIndicesBetweenTiles(fromCoordinate, item3, landIndices, map.Width);
			}
		}
		return list;
	}

	private List<int> GeneratePreTerrainCities(MapData map, List<int> cities, int cityCount)
	{
		List<int> list = new List<int>((map.Width - 2) * (map.Height - 2));
		for (int i = 1; i < map.Height - 1; i++)
		{
			for (int j = 1; j < map.Width - 1; j++)
			{
				int item = WorldCoordinates.ToIndex(j, i, map.Width);
				list.Add(item);
			}
		}
		foreach (int city in cities)
		{
			RemoveIndicesNearIndex(list, city, map.Width, 2);
		}
		for (int k = 0; k < cityCount; k++)
		{
			if (list.Count <= 0)
			{
				break;
			}
			int num = list[random.Range(0, list.Count)];
			cities.Add(num);
			Log.Verbose("{0} Added city at {1}", new object[2]
			{
				"<color=#639ad8>[MapGenerator]</color>",
				WorldCoordinates.FromIndex(num, map.Width)
			});
			RemoveIndicesNearIndex(list, num, map.Width, 2);
		}
		return cities;
	}

	private void RemoveIndicesNearIndex(List<int> indices, int index, int width, int range)
	{
		WorldCoordinates worldCoordinates = WorldCoordinates.FromIndex(index, width);
		for (int i = worldCoordinates.Y - range; i <= worldCoordinates.Y + range; i++)
		{
			for (int j = worldCoordinates.X - range; j <= worldCoordinates.X + range; j++)
			{
				int item = WorldCoordinates.ToIndex(j, i, width);
				int num = indices.BinarySearch(item);
				if (num >= 0)
				{
					indices.RemoveAt(num);
				}
			}
		}
	}

	private void SetTileAsCity(TileData tile)
	{
		if (tile != null)
		{
			tile.owner = 0;
			tile.terrain = TerrainData.Type.Field;
			tile.resource = null;
			tile.improvement = new ImprovementState
			{
				type = ImprovementData.Type.City,
				level = 1,
				borderSize = 1,
				production = 1,
				founded = 0
			};
		}
	}

	private void AddPostTerrainCities(MapData map, int maxCityCount = 200)
	{
		List<int> allGoodSpotIndices = GetAllGoodSpotIndices(map);
		for (int i = 0; i < maxCityCount; i++)
		{
			if (allGoodSpotIndices.Count <= 0)
			{
				break;
			}
			int num = allGoodSpotIndices[random.Range(0, allGoodSpotIndices.Count)];
			TileData tileData = map.Tiles[num];
			SetTileAsCity(tileData);
			WorldCoordinates coordinates = tileData.coordinates;
			for (int j = coordinates.Y - 2; j <= coordinates.Y + 2; j++)
			{
				for (int k = coordinates.X - 2; k <= coordinates.X + 2; k++)
				{
					int indexUnsafe = map.GetIndexUnsafe(k, j);
					allGoodSpotIndices.Remove(indexUnsafe);
				}
			}
		}
	}

	private List<int> GetAllGoodSpotIndices(MapData map)
	{
		List<int> list = new List<int>();
		List<TileData> list2 = new List<TileData>(25);
		for (int i = 0; i < map.Tiles.Length; i++)
		{
			TileData tileData = map.Tiles[i];
			if (tileData.terrain == TerrainData.Type.Mountain || tileData.terrain == TerrainData.Type.Water || tileData.terrain == TerrainData.Type.Ocean)
			{
				continue;
			}
			map.GetAreaPreallocated(list2, tileData.coordinates, 1, allowDiagonal: false, includeCenter: false);
			if (list2.Count < 4)
			{
				continue;
			}
			map.GetAreaPreallocated(list2, tileData.coordinates, 2, allowDiagonal: true);
			bool flag = true;
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j].improvement != null)
				{
					flag = false;
				}
			}
			if (flag)
			{
				list.Add(i);
			}
		}
		return list;
	}

	private void AddResources(MapData map, GameState gameState, float richness = 1f)
	{
		List<ResourceData> resources = gameState.GameLogicData.GetAllResources();
		HashSet<TileData> hashSet = new HashSet<TileData>();
		float num = richness / 3f;
		Dictionary<ResourceData.Type, int> currentResources = new Dictionary<ResourceData.Type, int>();
		List<TileData> list = new List<TileData>();
		for (int i = 0; i < map.Tiles.Length; i++)
		{
			TileData tileData = map.Tiles[i];
			if (!tileData.HasImprovement(ImprovementData.Type.City))
			{
				continue;
			}
			List<TileData> tileNeighbors = map.GetTileNeighbors(tileData.coordinates);
			currentResources.Clear();
			for (int j = 0; j < tileNeighbors.Count; j++)
			{
				TileData tileData2 = tileNeighbors[j];
				if (random.Value() < richness && !hashSet.Contains(tileData2) && tileData2.resource == null)
				{
					if (mapGenerationType == MapGenerationType.Tutorial)
					{
						AddResourcesInTutorial(tileData2);
					}
					else
					{
						AddResource(tileData2, resources, map);
					}
				}
				hashSet.Add(tileData2);
			}
			list.Add(tileData);
		}
		List<TileData> list2 = new List<TileData>(25);
		foreach (TileData item in list)
		{
			map.GetAreaPreallocated(list2, item.coordinates, 2, allowDiagonal: true, includeCenter: false);
			for (int k = 0; k < list2.Count; k++)
			{
				TileData tileData3 = list2[k];
				if (tileData3.improvement == null && random.Value() < num && !hashSet.Contains(list2[k]) && tileData3.resource == null)
				{
					AddResource(tileData3, resources, map);
				}
				hashSet.Add(tileData3);
			}
		}
		void AddResourcesInTutorial(TileData neighborTile)
		{
			List<ResourceData> list3 = new List<ResourceData>(resources);
			int num2 = 2;
			currentResources.TryGetValue(ResourceData.Type.Fruit, out var value);
			if (value >= num2)
			{
				int num3 = list3.FindIndex((ResourceData x) => x.type == ResourceData.Type.Fruit);
				if (num3 > 0)
				{
					list3.RemoveAt(num3);
				}
			}
			ResourceData resourceData = AddResource(neighborTile, list3, map);
			if (resourceData != null)
			{
				if (currentResources.ContainsKey(resourceData.type))
				{
					int num4 = currentResources[resourceData.type];
					currentResources[resourceData.type] = num4 + 1;
				}
				else
				{
					currentResources.Add(resourceData.type, 1);
				}
			}
		}
	}

	private ResourceData AddResource(TileData tile, List<ResourceData> resources, MapData map)
	{
		if (tile.improvement != null)
		{
			Log.Error("Something is wrong! Trying to add resources to tile {0}", new object[1] { tile.coordinates });
			return null;
		}
		if (tile.continent == null)
		{
			Log.Error("Something is wrong, tile {0} has no continent", new object[1] { tile.coordinates });
			return null;
		}
		List<ResourceData> list = new List<ResourceData>();
		foreach (ResourceData resource in resources)
		{
			float modifier = tile.continent.GetModifier(resource.type);
			if (modifier == 0f || random.Value() > modifier)
			{
				continue;
			}
			if (resource.type == ResourceData.Type.Whale)
			{
				bool flag = false;
				foreach (TileData tileNeighbor in map.GetTileNeighbors(tile.coordinates))
				{
					if (tileNeighbor.HasResource(ResourceData.Type.Whale))
					{
						flag = true;
						break;
					}
					if (tileNeighbor.HasImprovement(ImprovementData.Type.City))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			if (resource.resourceTerrainRequirements == null || resource.resourceTerrainRequirements.Count == 0)
			{
				list.Add(resource);
			}
			else if (resource.resourceTerrainRequirements != null && resource.resourceTerrainRequirements.Contains(tile.terrain))
			{
				list.Add(resource);
			}
		}
		if (list.Count > 0)
		{
			ResourceData resourceData = list[random.Range(0, list.Count)];
			tile.improvement = null;
			tile.resource = new ResourceState
			{
				type = resourceData.type
			};
			return resourceData;
		}
		return null;
	}

	private void AddRuins(MapData map, int amount)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (num < amount && num3++ < map.Tiles.Length)
		{
			TileData tileData = map.Tiles[random.Range(map.Tiles.Length)];
			if ((tileData.terrain == TerrainData.Type.Mountain || !(random.Value() < 0.5f)) && tileData.terrain != TerrainData.Type.Forest && tileData.improvement == null && !IsNearBuilding(map, tileData, 2) && tileData.terrain != TerrainData.Type.Water && (!tileData.IsWater || !((float)num2 > (float)amount / 3f)))
			{
				tileData.improvement = new ImprovementState
				{
					type = ImprovementData.Type.Ruin,
					borderSize = 0,
					level = 1,
					production = 1,
					founded = 0
				};
				num++;
				if (tileData.IsWater)
				{
					num2++;
				}
			}
		}
	}

	private ResourceData GetResourceForTech(TechData tech, TribeData tribe, GameState state)
	{
		foreach (ImprovementData improvementUnlock in tech.improvementUnlocks)
		{
			ImprovementData improvementData = state.GameLogicData.GetOverride(improvementUnlock, tribe);
			foreach (TerrainRequirements terrainRequirement in improvementData.terrainRequirements)
			{
				if (terrainRequirement.resource != null && terrainRequirement.resource.type != ResourceData.Type.None)
				{
					Log.Verbose("{0} used {1}, and so needs {2}", new object[3]
					{
						tribe.displayName,
						improvementData.displayName,
						terrainRequirement.resource.type
					});
					return terrainRequirement.resource;
				}
			}
		}
		return null;
	}

	private TileData FindLandTileToConvert(MapData map, WorldCoordinates capitalTile, List<int> landTileIndices)
	{
		for (int i = -1; i < 2; i += 2)
		{
			TileData tile = map.GetTile(capitalTile + new WorldCoordinates(i, 0));
			int indexUnsafe = map.GetIndexUnsafe(tile.coordinates);
			if (landTileIndices.Contains(indexUnsafe))
			{
				TileData tile2 = map.GetTile(capitalTile + new WorldCoordinates(i * 2, -1));
				TileData tile3 = map.GetTile(capitalTile + new WorldCoordinates(i * 2, 0));
				TileData tile4 = map.GetTile(capitalTile + new WorldCoordinates(i * 2, 1));
				TileData tile5 = map.GetTile(capitalTile + new WorldCoordinates(i, -1));
				TileData tile6 = map.GetTile(capitalTile + new WorldCoordinates(i, 1));
				if ((tile2.IsWater || !tile5.IsWater) && (tile4.IsWater || !tile6.IsWater) && (tile3.IsWater || !tile5.IsWater || !tile6.IsWater))
				{
					return tile;
				}
			}
		}
		for (int j = -1; j < 2; j += 2)
		{
			TileData tile7 = map.GetTile(capitalTile + new WorldCoordinates(0, j));
			int indexUnsafe2 = map.GetIndexUnsafe(tile7.coordinates);
			if (landTileIndices.Contains(indexUnsafe2))
			{
				TileData tile8 = map.GetTile(capitalTile + new WorldCoordinates(-1, j * 2));
				TileData tile9 = map.GetTile(capitalTile + new WorldCoordinates(0, j * 2));
				TileData tile10 = map.GetTile(capitalTile + new WorldCoordinates(1, j * 2));
				TileData tile11 = map.GetTile(capitalTile + new WorldCoordinates(-1, j));
				TileData tile12 = map.GetTile(capitalTile + new WorldCoordinates(1, j));
				if ((tile8.IsWater || !tile11.IsWater) && (tile10.IsWater || !tile12.IsWater) && (tile9.IsWater || !tile11.IsWater || !tile12.IsWater))
				{
					return tile7;
				}
			}
		}
		return null;
	}

	private List<StartResourceData> GetStartResourcesForPlayers(GameState state, MapGeneratorSettings settings)
	{
		List<StartResourceData> list = new List<StartResourceData>();
		for (int i = 0; i < state.PlayerStates.Count; i++)
		{
			PlayerState playerState = state.PlayerStates[i];
			if (playerState.Id == byte.MaxValue)
			{
				continue;
			}
			ResourceData resourceData = null;
			TerrainData.Type terrain = TerrainData.Type.Field;
			if (!PolytopiaDataManager.GetGameLogicData(VersionManager.GetGameLogicDataVersionFromGameVersion(state.Version)).TryGetData(playerState.tribe, out var data))
			{
				Log.Error("{0} Could not get Tribe for player: {1}", new object[2] { "<color=#639ad8>[MapGenerator]</color>", playerState });
			}
			if (data.startingResource.Count > 0)
			{
				resourceData = data.startingResource[0];
			}
			if (resourceData == null)
			{
				foreach (TechData item in data.startingTech)
				{
					resourceData = GetResourceForTech(item, data, state);
				}
			}
			List<ResourceData> list2 = new List<ResourceData>();
			if (resourceData == null)
			{
				PolytopiaDataManager.GetGameLogicData(VersionManager.GetGameLogicDataVersionFromGameVersion(state.Version)).TryGetData(TechData.Type.Basic, out var data2);
				foreach (TechData techUnlock in data2.techUnlocks)
				{
					ResourceData resourceForTech = GetResourceForTech(techUnlock, data, state);
					if (resourceForTech != null)
					{
						if (settings.wetness <= 0f && resourceForTech.resourceTerrainRequirements != null && resourceForTech.resourceTerrainRequirements.Count > 0 && resourceForTech.resourceTerrainRequirements.Contains(TerrainData.Type.Water))
						{
							Log.Verbose("{0} Blocked use of {1} because wetness is 0f", new object[2] { "<color=#639ad8>[MapGenerator]</color>", resourceForTech.type });
						}
						else
						{
							list2.Add(resourceForTech);
						}
					}
				}
				resourceData = list2[random.Range(0, list2.Count)];
			}
			Log.Verbose("{0} Possible starting Resources {1} for Tribe {2}", new object[3] { "<color=#639ad8>[MapGenerator]</color>", list2.Count, playerState.tribe });
			if (resourceData.resourceTerrainRequirements != null && resourceData.resourceTerrainRequirements.Count > 0)
			{
				terrain = resourceData.resourceTerrainRequirements[random.Range(resourceData.resourceTerrainRequirements.Count)].type;
			}
			if (mapGenerationType == MapGenerationType.Tutorial)
			{
				ResourceData resourceData2 = state.GameLogicData.AllResourceData[ResourceData.Type.Game];
				TerrainData.Type terrain2 = TerrainData.Type.Forest;
				int count = resourceData2.resourceTerrainRequirements.Count;
				if (count > 0)
				{
					terrain2 = resourceData2.resourceTerrainRequirements[random.Range(count)].type;
				}
				list.Add(new StartResourceData
				{
					resource = resourceData2,
					terrain = terrain2,
					playerId = playerState.Id,
					minResourcesCount = 3
				});
			}
			list.Add(new StartResourceData
			{
				resource = resourceData,
				terrain = terrain,
				playerId = playerState.Id,
				minResourcesCount = 2
			});
		}
		return list;
	}

	private bool PrepareStartAreas(MapData map, GameState state, MapGeneratorSettings settings, List<StartResourceData> startResources, List<int> landTileIndices)
	{
		bool result = false;
		List<TileData> list = new List<TileData>(9);
		for (int i = 0; i < startResources.Count; i++)
		{
			StartResourceData startResource = startResources[i];
			PlayerState playerState = state.PlayerStates.Find((PlayerState x) => x.Id == startResource.playerId);
			if (playerState.Id == byte.MaxValue)
			{
				continue;
			}
			ResourceData resource = startResource.resource;
			TerrainData.Type terrain = startResource.terrain;
			int minResourcesCount = startResource.minResourcesCount;
			if (resource == null)
			{
				continue;
			}
			if (!PolytopiaDataManager.GetGameLogicData(VersionManager.GetGameLogicDataVersionFromGameVersion(state.Version)).TryGetData(playerState.tribe, out var data))
			{
				Log.Error("{0} Could not get Tribe for player: {1}", new object[2] { "<color=#639ad8>[MapGenerator]</color>", playerState });
			}
			map.GetAreaPreallocated(list, playerState.startTile, 1, allowDiagonal: true);
			for (int num = 0; num < list.Count; num++)
			{
				TileData tileData = list[num];
				tileData.climate = data.climate;
				tileData.skinType = playerState.skinType;
				if (tileData.coordinates == playerState.startTile)
				{
					list.RemoveAt(num--);
					continue;
				}
				int indexUnsafe = map.GetIndexUnsafe(tileData.coordinates);
				if ((terrain == TerrainData.Type.Water || terrain == TerrainData.Type.Ocean) && landTileIndices != null && landTileIndices.Contains(indexUnsafe))
				{
					list.RemoveAt(num--);
				}
			}
			if (list.Count < minResourcesCount && (terrain == TerrainData.Type.Water || terrain == TerrainData.Type.Ocean))
			{
				for (int num2 = list.Count; num2 < minResourcesCount; num2++)
				{
					TileData tileData2 = FindLandTileToConvert(map, playerState.startTile, landTileIndices);
					if (tileData2 == null)
					{
						break;
					}
					list.Add(tileData2);
					int indexUnsafe2 = map.GetIndexUnsafe(tileData2.coordinates);
					landTileIndices.Remove(indexUnsafe2);
				}
			}
			if (list.Count < minResourcesCount)
			{
				Log.Error("{0} Failed to find minimum number of tiles for start resources", new object[1] { "<color=#639ad8>[MapGenerator]</color>" });
			}
			int num3 = 0;
			for (int num4 = list.Count - 1; num4 >= 0; num4--)
			{
				TileData tileData3 = list[num4];
				if (tileData3.resource != null && tileData3.resource.type == resource.type)
				{
					num3++;
					list.RemoveAt(num4);
				}
				else if (mapGenerationType == MapGenerationType.Tutorial && tileData3.resource != null && ShouldRemoveCandidate(tileData3.resource.type, playerState.Id))
				{
					list.RemoveAt(num4);
				}
			}
			for (; num3 < minResourcesCount; num3++)
			{
				if (list.Count <= 0)
				{
					break;
				}
				result = true;
				int index = random.Range(list.Count);
				TileData tileData4 = list[index];
				Log.Verbose("{0} Adding starting resource: {1} to {2}", new object[3] { "<color=#639ad8>[MapGenerator]</color>", resource.type, tileData4.coordinates });
				tileData4.climate = data.climate;
				tileData4.skinType = playerState.skinType;
				tileData4.resource = new ResourceState
				{
					type = resource.type
				};
				tileData4.terrain = terrain;
				switch (tileData4.terrain)
				{
				case TerrainData.Type.Water:
					tileData4.altitude = -1;
					break;
				case TerrainData.Type.Ocean:
					tileData4.altitude = -2;
					break;
				case TerrainData.Type.Field:
				case TerrainData.Type.Forest:
					tileData4.altitude = 1;
					break;
				case TerrainData.Type.Mountain:
					tileData4.altitude = 2;
					break;
				}
				list.RemoveAt(index);
			}
		}
		return result;
		bool ShouldRemoveCandidate(ResourceData.Type type, int playerId)
		{
			for (int j = 0; j < startResources.Count; j++)
			{
				if (startResources[j].playerId == playerId && startResources[j].resource.type == type)
				{
					return true;
				}
			}
			return false;
		}
	}

	private void PrepareAlienClimates(GameState gameState)
	{
		foreach (PlayerState playerState in gameState.PlayerStates)
		{
			if (!gameState.GameLogicData.TryGetData(playerState.tribe, out var data) || !data.HasAbility(TribeAbility.Type.AlienClimate) || playerState.tribe != TribeData.Type.Polaris)
			{
				continue;
			}
			foreach (TileData item in gameState.Map.GetArea(playerState.startTile, 1, allowDiagonal: true))
			{
				if (item.IsWater)
				{
					item.terrain = TerrainData.Type.Ice;
				}
			}
		}
	}

	private void SmoothNoise(int width, int height, float[] noise, List<int> landIndices, int iterations, float surroundingSpaceValue)
	{
		float[] array = new float[noise.Length];
		List<WorldCoordinates> list = new List<WorldCoordinates>(9);
		for (int i = 0; i < iterations; i++)
		{
			for (int j = 0; j < array.Length; j++)
			{
				WorldCoordinates center = WorldCoordinates.FromIndex(j, width);
				WorldCoordinates.GetAreaPreallocated(list, center, 1);
				float num = 0f;
				foreach (WorldCoordinates item in list)
				{
					if (!item.IsValid(width, height))
					{
						num += surroundingSpaceValue;
						continue;
					}
					int num2 = item.ToIndex(width);
					num += noise[num2];
				}
				num += (float)(9 - list.Count) * surroundingSpaceValue;
				array[j] = num / 9f;
			}
			for (int k = 0; k < landIndices.Count; k++)
			{
				int num3 = landIndices[k];
				array[num3] = noise[num3];
			}
			array.CopyTo(noise, 0);
		}
	}

	private void MakeOcean(MapData map, GameLogicData gameLogicData, bool shouldConvertShallows = true)
	{
		List<TileData> list = new List<TileData>(9);
		for (int i = 0; i < map.Tiles.Length; i++)
		{
			TileData tileData = map.Tiles[i];
			if (!tileData.IsWater)
			{
				continue;
			}
			map.GetAreaPreallocated(list, tileData.coordinates, 1, allowDiagonal: false, includeCenter: false);
			bool flag = false;
			foreach (TileData item in list)
			{
				if (!item.IsWater)
				{
					flag = true;
				}
			}
			if (flag)
			{
				tileData.terrain = TerrainData.Type.Water;
				tileData.altitude = -1;
			}
			else if (shouldConvertShallows)
			{
				tileData.terrain = TerrainData.Type.Ocean;
				tileData.altitude = -2;
			}
			if (tileData.resource != null)
			{
				gameLogicData.TryGetData(tileData.resource.type, out var data);
				if (data.resourceTerrainRequirements != null && !data.resourceTerrainRequirements.Contains(tileData.terrain))
				{
					tileData.resource = null;
				}
			}
		}
	}

	private void AddClimates(MapData map, List<PlayerState> playerStates, int version, MapGeneratorSettings settings, List<int> landTileIndices = null)
	{
		int num = playerStates.Count - 1;
		map.Continents = new WorldContinent[num];
		bool flag = true;
		foreach (PlayerState playerState in playerStates)
		{
			if (playerState.Id != byte.MaxValue && PolytopiaDataManager.GetGameLogicData(VersionManager.GetGameLogicDataVersionFromGameVersion(version)).TryGetData(playerState.tribe, out var data) && !data.HasAbility(TribeAbility.Type.AlienClimate))
			{
				flag = false;
				break;
			}
		}
		for (int i = 0; i < num; i++)
		{
			WorldContinent worldContinent = new WorldContinent(playerStates[i], version, flag);
			if (settings.wetness == 0f)
			{
				worldContinent.SetModifier(TerrainData.Type.Water, 0f);
				worldContinent.SetModifier(TerrainData.Type.Ocean, 0f);
				worldContinent.SetModifier(ResourceData.Type.Fish, 0f);
				worldContinent.SetModifier(ResourceData.Type.Whale, 0f);
			}
			map.Continents[i] = worldContinent;
		}
		int num2 = map.Tiles.Length;
		int num3 = num2 * 10;
		List<TileData> list = new List<TileData>(9);
		int num4 = num;
		while (num4 < num2 && num3 > 0)
		{
			for (int j = 0; j < num; j++)
			{
				if (num4 >= num2)
				{
					break;
				}
				if (num3 <= 0)
				{
					break;
				}
				WorldContinent worldContinent2 = map.Continents[j];
				if (worldContinent2.Tiles.Count == 0 || (worldContinent2.hasAlienClimate && !flag))
				{
					num3--;
					continue;
				}
				TileData tile = map.GetTile(worldContinent2.Tiles[random.Range(0, worldContinent2.Tiles.Count)]);
				List<TileData> list2 = new List<TileData>();
				map.GetAreaPreallocated(list, tile.coordinates, 1, allowDiagonal: true, includeCenter: false);
				for (int k = 0; k < list.Count; k++)
				{
					TileData tileData = list[k];
					if (tileData.climate == 0 && tileData.altitude > -1)
					{
						list2.Add(tileData);
					}
				}
				if (list2.Count == 0)
				{
					for (int l = 0; l < list.Count; l++)
					{
						TileData tileData2 = list[l];
						if (tileData2.climate == 0)
						{
							list2.Add(tileData2);
						}
					}
				}
				if (list2.Count == 0)
				{
					worldContinent2.Tiles.Remove(tile.coordinates);
					j--;
					num3--;
					continue;
				}
				TileData tileData3 = list2[random.Range(0, list2.Count)];
				tileData3.climate = worldContinent2.Climate;
				tileData3.skinType = worldContinent2.SkinType;
				tileData3.continent = worldContinent2;
				if (tileData3.altitude > -1)
				{
					tileData3.altitude = 1;
					tileData3.terrain = TerrainData.Type.Field;
					tileData3.resource = null;
				}
				num4++;
				worldContinent2.Tiles.Add(tileData3.coordinates);
			}
		}
	}

	private void patchContinents(MapData map)
	{
		WorldContinent[] continents = map.Continents;
		foreach (WorldContinent obj in continents)
		{
			obj.Tiles.Clear();
			obj.LandTileCount = 0;
		}
		TileData[] tiles = map.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (tileData.continent == null)
			{
				Log.Verbose("tile {0} does not belong to a continent", new object[1] { tileData.coordinates });
				continue;
			}
			tileData.continent.Tiles.Add(tileData.coordinates);
			if (!tileData.IsWater && tileData.improvement == null)
			{
				tileData.continent.LandTileCount++;
			}
		}
	}

	private void AddTerrain(MapData map, List<PlayerState> playerStates, int version, MapGeneratorSettings settings, List<int> landTileIndices = null)
	{
		patchContinents(map);
		int num = 0;
		WorldContinent[] continents = map.Continents;
		foreach (WorldContinent worldContinent in continents)
		{
			Log.Verbose("Continent {0} ({1})", new object[2]
			{
				num++,
				worldContinent.LandTileCount
			});
			float num2 = worldContinent.GetModifier(TerrainData.Type.Field) + worldContinent.GetModifier(TerrainData.Type.Forest) + worldContinent.GetModifier(TerrainData.Type.Mountain);
			float num3 = worldContinent.GetModifier(TerrainData.Type.Field) / num2 * (float)worldContinent.LandTileCount;
			float num4 = worldContinent.GetModifier(TerrainData.Type.Mountain) / num2 * (float)worldContinent.LandTileCount;
			float num5 = worldContinent.GetModifier(TerrainData.Type.Forest) / num2 * (float)worldContinent.LandTileCount;
			List<TerrainData.Type> list = new List<TerrainData.Type>();
			List<TerrainData.Type> list2 = new List<TerrainData.Type>();
			for (int j = 0; (float)j < num3; j++)
			{
				list2.Add(TerrainData.Type.Field);
			}
			for (int k = 0; (float)k < num4; k++)
			{
				list2.Add(TerrainData.Type.Mountain);
			}
			for (int l = 0; (float)l < num5; l++)
			{
				list2.Add(TerrainData.Type.Forest);
			}
			foreach (WorldCoordinates tile2 in worldContinent.Tiles)
			{
				TileData tile = map.GetTile(tile2);
				if (!tile.IsWater && tile.improvement == null)
				{
					tile.altitude = 1;
					tile.resource = null;
					if (list2.Count == 0)
					{
						break;
					}
					int index = random.Range(0, list2.Count);
					TerrainData.Type terrain = list2[index];
					list2.RemoveAt(index);
					tile.terrain = terrain;
					if (tile.terrain == TerrainData.Type.Mountain)
					{
						tile.altitude = 2;
					}
				}
				list.Add(tile.terrain);
			}
		}
	}

	public bool GetRandomIsolatedTile(MapData map, out WorldCoordinates coordinates)
	{
		int num = 0;
		float num2 = 0f;
		coordinates = default(WorldCoordinates);
		bool flag = false;
		while (num++ < 200)
		{
			TileData tile = map.GetTile(new WorldCoordinates(random.Range(2, map.Width - 4), random.Range(2, map.Height - 4)));
			if (tile != null && !tile.IsWater)
			{
				float num3 = DistanceSqrToClosestCity(map, tile.coordinates);
				if (!flag || num3 > num2)
				{
					num2 = num3;
					coordinates = tile.coordinates;
					flag = true;
				}
			}
		}
		return flag;
	}

	public float DistanceSqrToClosestCity(MapData map, WorldCoordinates from)
	{
		float num = 1000f;
		TileData[] tiles = map.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (tileData.improvement != null)
			{
				float num2 = (from - tileData.coordinates).SqrMagnitude;
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public bool IsNearBuilding(MapData map, TileData tile, int radius)
	{
		foreach (TileData item in map.GetArea(tile.coordinates, radius, allowDiagonal: true))
		{
			if (item.improvement != null)
			{
				MapDataExtensions.ChebyshevDistance(tile.coordinates, item.coordinates);
				if (item.HasImprovement(ImprovementData.Type.Ruin))
				{
					return true;
				}
				if (item.HasImprovement(ImprovementData.Type.City) && item.owner != 0)
				{
					return true;
				}
			}
		}
		return false;
	}
}
