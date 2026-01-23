using System;
using System.Collections.Generic;
using System.IO;
using AirMigrations;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using SimpleJSON;

public class AirGameMigration
{
	public class AdditionalGameStateData
	{
		public DateTime Date { get; set; }

		public Guid? OwnerId { get; set; }
	}

	private Dictionary<string, byte> playerTribeIndex = new Dictionary<string, byte>();

	private Dictionary<string, byte> playerIdIndex = new Dictionary<string, byte>();

	private Dictionary<string, int> onlineIdToPlayerIndex = new Dictionary<string, int>();

	private Dictionary<WorldCoordinates, byte> playerCapitals = new Dictionary<WorldCoordinates, byte>();

	private Dictionary<WorldCoordinates, string[]> tileExplorers = new Dictionary<WorldCoordinates, string[]>();

	private Dictionary<byte, string[]> knownPlayers = new Dictionary<byte, string[]>();

	private Dictionary<string, Dictionary<string, int>> playerOpinions = new Dictionary<string, Dictionary<string, int>>();

	private Dictionary<string, string> mixedTribeMapping = new Dictionary<string, string>();

	private Dictionary<string, Guid> legacyPlayerMapping;

	private List<byte> erroneousPlayer = new List<byte>();

	private GameState gameState;

	private Version appVersion;

	public bool MigrateGameData(byte[] gameData, int targetGameVersion, Dictionary<string, Guid> legacyPlayerMapping, GameMetadata gameMetadata, out GameState parsedGameState, out AdditionalGameStateData additionalData)
	{
		parsedGameState = null;
		additionalData = null;
		if (gameData == null)
		{
			return false;
		}
		this.legacyPlayerMapping = legacyPlayerMapping;
		try
		{
			using (MemoryStream input = new MemoryStream(gameData))
			{
				ByteArrayReader byteArrayReader = new ByteArrayReader(input);
				gameState = new GameState();
				gameState.Version = targetGameVersion;
				gameState.Settings = new GameSettings();
				gameState.Map = new MapData(0, 0);
				additionalData = new AdditionalGameStateData();
				short num = byteArrayReader.ReadInt16();
				gameState.Settings.GameName = byteArrayReader.ReadString();
				gameState.CurrentState = ParseGameState((num < 5) ? 1 : byteArrayReader.ReadInt16());
				gameState.Settings.GameType = ParseGameType((num < 5) ? 1 : byteArrayReader.ReadInt16());
				if (gameState.Settings.GameType == GameType.PassAndPlay && gameState.CurrentState == GameState.State.Lobby)
				{
					Log.Info("[VersionManager] Unstarted Pass & Play game, skipping migration", Array.Empty<object>());
					return false;
				}
				if (gameState.Settings.GameType == GameType.SinglePlayer && gameState.CurrentState == GameState.State.Lobby)
				{
					gameState.CurrentState = GameState.State.Started;
				}
				int num2 = byteArrayReader.ReadInt16();
				if (num >= 5)
				{
					byteArrayReader.ReadInt16();
				}
				gameState.PlayerStates = new List<PlayerState>();
				Dictionary<string, Player> dictionary = new Dictionary<string, Player>();
				if (gameMetadata != null && gameMetadata.Players != null)
				{
					foreach (Player value in gameMetadata.Players.Values)
					{
						dictionary.Add(value.OnlineId, value);
					}
				}
				for (int i = 0; i < num2; i++)
				{
					PlayerState playerState = new PlayerState();
					string text = byteArrayReader.ReadString();
					playerState.UserName = byteArrayReader.ReadString();
					string text2 = byteArrayReader.ReadString();
					byte id = ((text2 == "Nature") ? byte.MaxValue : ((byte)(i + 1)));
					if (gameState.Settings.GameType == GameType.Multiplayer)
					{
						playerState.tribe = ParseTribeString(text2);
						playerState.tribeMix = TribeData.Type.None;
						if (playerState.tribe != TribeData.Type.None)
						{
							playerState.hasChosenTribe = true;
						}
					}
					if (gameState.CurrentState == GameState.State.Lobby && playerState.tribe != TribeData.Type.Nature)
					{
						playerState.Currency = 5;
					}
					string text3 = ((num >= 6) ? byteArrayReader.ReadString() : text);
					playerState.AccountId = GetGuidForPlayer(text3);
					string text4 = ((num >= 6) ? byteArrayReader.ReadString() : "");
					if (dictionary.ContainsKey(text3))
					{
						Player player = dictionary[text3];
						playerState.AutoPlay = player.IsAi;
					}
					else
					{
						playerState.AutoPlay = text4 == "ai";
					}
					playerState.handicap = ((num < 6) ? 1 : byteArrayReader.ReadInt32());
					playerState.Id = id;
					gameState.PlayerStates.Add(playerState);
					if (!string.IsNullOrEmpty(text3))
					{
						onlineIdToPlayerIndex.Add(text3, i);
					}
				}
				if (gameState.Settings.GameType == GameType.Multiplayer)
				{
					GameStateUtils.AddNaturePlayer(gameState);
				}
				foreach (PlayerState playerState3 in gameState.PlayerStates)
				{
					if (!playerState3.AutoPlay && playerState3.AccountId.HasValue)
					{
						additionalData.OwnerId = playerState3.AccountId;
						break;
					}
				}
				string text5 = byteArrayReader.ReadString();
				gameState.CurrentTurn = (uint)Math.Max((short)0, byteArrayReader.ReadInt16());
				gameState.Settings.BaseGameMode = ParseGameModeString(byteArrayReader.ReadString());
				gameState.Settings.OpponentCount = gameState.PlayerCount - 1;
				gameState.CurrentPlayerIndex = (byte)onlineIdToPlayerIndex[text5];
				long num3 = byteArrayReader.ReadInt32();
				num3 *= 1000;
				additionalData.Date = DateTimeOffset.FromUnixTimeMilliseconds(num3).UtcDateTime;
				int mapSize = byteArrayReader.ReadInt32();
				gameState.Settings.MapSize = mapSize;
				gameState.Settings.mapPreset = MapPreset.Continents;
				Version.TryParse((num >= 7) ? byteArrayReader.ReadString() : "0.0", out appVersion);
				int num4 = byteArrayReader.ReadInt16();
				byte[][] array = new byte[num4][];
				int num5 = -1;
				int num6 = -1;
				for (int j = 0; j < num4; j++)
				{
					byteArrayReader.ReadString();
					int count = byteArrayReader.ReadInt32();
					array[j] = byteArrayReader.ReadBytes(count);
					int moveCountFromGameStateData = GetMoveCountFromGameStateData(array[j]);
					if (moveCountFromGameStateData > num5)
					{
						num6 = j;
						num5 = moveCountFromGameStateData;
					}
				}
				if (num6 > -1)
				{
					MigrateGameState(text5, array[num6]);
				}
			}
			if (gameState.CurrentState == GameState.State.Lobby && gameState.TryGetPlayer(gameState.CurrentPlayer, out var playerState2) && playerState2.AutoPlay)
			{
				GameStateUtils.AssignTribe(gameState, playerState2, null);
			}
			if (gameState.CurrentState == GameState.State.Started && gameState.Settings.GameType == GameType.Multiplayer && gameState.CurrentTurn != 0)
			{
				gameState.ActionStack.Add(new StartTurnAction(gameState.CurrentPlayer));
				ActionManagerUtils.PerformAllQueuedActions(gameState);
			}
			parsedGameState = gameState;
			return true;
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to migrate with error {0}", new object[1] { ex });
			return false;
		}
	}

	public int GetMoveCountFromGameStateData(byte[] data)
	{
		try
		{
			int result = -1;
			using (MemoryStream input = new MemoryStream(data))
			{
				ByteArrayReader byteArrayReader = new ByteArrayReader(input);
				byteArrayReader.ReadInt16();
				string[] stringCache = byteArrayReader.ReadStringArray();
				int num = byteArrayReader.ReadInt16();
				for (int i = 0; i < num; i++)
				{
					byteArrayReader.ReadInt16();
					byteArrayReader.ReadInt16();
					byteArrayReader.ReadInt16();
					byteArrayReader.ReadBoolean();
					byteArrayReader.ReadInt16();
					GetStringArrayFromCache(byteArrayReader, stringCache);
					byteArrayReader.ReadInt16();
					GetStringArrayFromCache(byteArrayReader, stringCache);
					GetStringArrayFromCache(byteArrayReader, stringCache);
					GetStringArrayFromCache(byteArrayReader, stringCache);
					byteArrayReader.ReadInt16();
					byteArrayReader.ReadInt16();
					byteArrayReader.ReadInt16();
					byteArrayReader.ReadInt16();
					GetStringArrayFromCache(byteArrayReader, stringCache);
					GetStringArrayFromCache(byteArrayReader, stringCache);
					byteArrayReader.ReadBoolean();
					byteArrayReader.ReadInt16();
					byteArrayReader.ReadBoolean();
					byteArrayReader.ReadInt16();
					byteArrayReader.ReadInt16();
					byteArrayReader.ReadInt16();
					byteArrayReader.ReadInt16();
					byteArrayReader.ReadInt16();
					GetStringArrayFromCache(byteArrayReader, stringCache);
					GetStringArrayFromCache(byteArrayReader, stringCache);
					int num2 = byteArrayReader.ReadInt16();
					for (int j = 0; j < num2; j++)
					{
						byteArrayReader.ReadInt16();
						byteArrayReader.ReadInt16();
					}
				}
				byteArrayReader.ReadInt16();
				result = byteArrayReader.ReadInt16();
			}
			return result;
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to migrate with error {0}", new object[1] { ex });
			return -1;
		}
	}

	public bool MigrateGameState(string playerId, byte[] data)
	{
		try
		{
			using (MemoryStream input = new MemoryStream(data))
			{
				ByteArrayReader byteArrayReader = new ByteArrayReader(input);
				int num = byteArrayReader.ReadInt16();
				string[] stringCache = byteArrayReader.ReadStringArray();
				int num2 = byteArrayReader.ReadInt16();
				for (int i = 0; i < num2; i++)
				{
					PlayerState playerState = gameState.PlayerStates[i];
					GetStringFromCache(byteArrayReader, stringCache);
					GetStringFromCache(byteArrayReader, stringCache);
					playerState.Currency = byteArrayReader.ReadInt16();
					byteArrayReader.ReadBoolean();
					byteArrayReader.ReadInt16();
					playerState.builtUniqueImprovements = ParseUniqueBuildings(GetStringArrayFromCache(byteArrayReader, stringCache));
					playerState.handicap = byteArrayReader.ReadInt16();
					playerState.availableTech = ParseTech(GetStringArrayFromCache(byteArrayReader, stringCache));
					GetStringArrayFromCache(byteArrayReader, stringCache);
					GetStringArrayFromCache(byteArrayReader, stringCache);
					playerState.kills = (uint)byteArrayReader.ReadInt16();
					playerState.wipeOuts = (uint)byteArrayReader.ReadInt16();
					int pacifistTurns = byteArrayReader.ReadInt16();
					byteArrayReader.ReadInt16();
					knownPlayers[playerState.Id] = GetStringArrayFromCache(byteArrayReader, stringCache);
					GetStringArrayFromCache(byteArrayReader, stringCache);
					byteArrayReader.ReadBoolean();
					playerState.casualities = (uint)byteArrayReader.ReadInt16();
					byteArrayReader.ReadBoolean();
					byteArrayReader.ReadInt16();
					string stringFromCache = GetStringFromCache(byteArrayReader, stringCache);
					if (stringFromCache.Contains("&"))
					{
						string[] array = stringFromCache.Split('&');
						playerState.tribeMix = ParseTribeString(array[0]);
						playerState.tribe = ParseTribeString(array[1]);
						string tribeName = GetTribeName(array[1]);
						string tribeName2 = GetTribeName(array[0]);
						string key = $"{tribeName2.Substring(0, 4)}{tribeName.Substring(tribeName.Length - 3)}";
						mixedTribeMapping.Add(key, playerId);
					}
					else
					{
						playerState.tribe = ParseTribeString(stringFromCache);
						playerState.tribeMix = TribeData.Type.None;
					}
					if (playerState.tribe != TribeData.Type.None)
					{
						playerState.hasChosenTribe = true;
					}
					if (playerState.tribe == TribeData.Type.Aquarion && playerState.availableTech.Contains(TechData.Type.Riding))
					{
						playerState.availableTech.Remove(TechData.Type.Riding);
						playerState.availableTech.Add(TechData.Type.Riding2);
					}
					WorldCoordinates worldCoordinates = new WorldCoordinates(byteArrayReader.ReadInt16(), byteArrayReader.ReadInt16());
					if (worldCoordinates == WorldCoordinates.NULL_COORDINATES && playerState.Id != byte.MaxValue)
					{
						erroneousPlayer.Add(playerState.Id);
					}
					string text = ((num >= 5) ? GetStringFromCache(byteArrayReader, stringCache) : stringFromCache);
					if (IsValidPlayer(playerState.Id))
					{
						playerTribeIndex.Add(SimplifyTribeString(stringFromCache), playerState.Id);
						if (string.IsNullOrEmpty(text))
						{
							playerIdIndex.Add(SimplifyTribeString(stringFromCache), playerState.Id);
						}
						else
						{
							playerIdIndex.Add(text, playerState.Id);
						}
						playerCapitals.Add(worldCoordinates, playerState.Id);
					}
					GetStringArrayFromCache(byteArrayReader, stringCache);
					string[] stringArrayFromCache = GetStringArrayFromCache(byteArrayReader, stringCache);
					ParseTasks(playerState, stringArrayFromCache, pacifistTurns);
					int num3 = byteArrayReader.ReadInt16();
					for (int j = 0; j < num3; j++)
					{
						string stringFromCache2 = GetStringFromCache(byteArrayReader, stringCache);
						int value = byteArrayReader.ReadInt16();
						if (IsValidPlayer(playerState.Id))
						{
							if (playerOpinions.ContainsKey(stringFromCache2))
							{
								playerOpinions[stringFromCache2].Add(stringFromCache, value);
								continue;
							}
							Dictionary<string, int> dictionary = new Dictionary<string, int>();
							dictionary.Add(stringFromCache, value);
							playerOpinions.Add(stringFromCache2, dictionary);
						}
					}
				}
				MigrateWorldState(byteArrayReader, stringCache);
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to migrate with error {0}", new object[1] { ex });
			return false;
		}
	}

	public WorldCoordinates ReadFlippedWorldCoordinate(ByteArrayReader reader, ushort mapSize)
	{
		int num = reader.ReadInt16();
		int num2 = reader.ReadInt16();
		return new WorldCoordinates(mapSize - num2 - 1, mapSize - num - 1);
	}

	public WorldCoordinates FlipCoordinate(WorldCoordinates coordinates, ushort mapSize)
	{
		return new WorldCoordinates(mapSize - coordinates.Y - 1, mapSize - coordinates.X - 1);
	}

	public bool MigrateWorldState(ByteArrayReader reader, string[] stringCache)
	{
		try
		{
			gameState.CurrentTurn = (uint)reader.ReadInt16();
			reader.ReadInt16();
			gameState.LastProcessedCommand = 0;
			reader.ReadInt16();
			GetStringFromCache(reader, stringCache);
			reader.ReadInt16();
			ushort num = (ushort)reader.ReadInt16();
			ushort num2 = (ushort)reader.ReadInt16();
			gameState.Map = new MapData(num, num2);
			for (int i = 0; i < num * num2; i++)
			{
				WorldCoordinates worldCoordinates = new WorldCoordinates(i % num, i / num);
				WorldCoordinates coordinates = FlipCoordinate(worldCoordinates, num);
				int num3 = coordinates.ToIndex(num);
				TileData tileData = new TileData();
				tileData.coordinates = coordinates;
				tileData.altitude = reader.ReadInt16();
				tileData.terrain = ParseTerrainString(GetStringFromCache(reader, stringCache));
				tileData.HasRoad = reader.ReadBoolean();
				bool connectedToCapital = reader.ReadBoolean();
				reader.ReadBoolean();
				tileData.climate = reader.ReadInt16();
				tileExplorers.Add(tileData.coordinates, GetStringArrayFromCache(reader, stringCache));
				tileData.unit = MigrateUnitState(reader, tileData, num, stringCache);
				tileData.resource = MigrateResourceState(reader, tileData, stringCache);
				tileData.improvement = MigrateBuildingState(reader, tileData, connectedToCapital, stringCache);
				if (reader.ReadBoolean())
				{
					tileData.rulingCityCoordinates = ReadFlippedWorldCoordinate(reader, num);
				}
				if (playerCapitals.TryGetValue(worldCoordinates, out var value))
				{
					tileData.capitalOf = value;
					for (int j = 0; j < gameState.PlayerStates.Count; j++)
					{
						if (gameState.PlayerStates[j].Id == value)
						{
							gameState.PlayerStates[j].startTile = tileData.coordinates;
						}
					}
				}
				gameState.Map.Tiles[num3] = tileData;
			}
			Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
			for (int k = 0; k < gameState.Map.Tiles.Length; k++)
			{
				TileData tileData2 = gameState.Map.Tiles[k];
				TileData tile = gameState.Map.GetTile(tileData2.rulingCityCoordinates);
				if (tile != null)
				{
					tileData2.owner = tile.owner;
				}
				tileData2.explorers = ParseTileExplorers(tileExplorers[tileData2.coordinates]);
				if (tileData2.explorers == null || tileData2.explorers.Count <= 0)
				{
					continue;
				}
				for (int l = 0; l < tileData2.explorers.Count; l++)
				{
					if (dictionary.TryGetValue(tileData2.explorers[l], out var value2))
					{
						value2++;
					}
					else
					{
						dictionary.Add(tileData2.explorers[l], 1);
					}
					dictionary[tileData2.explorers[l]] = value2;
				}
			}
			UpdateConnectedCities(gameState);
			for (int m = 0; m < gameState.PlayerStates.Count; m++)
			{
				PlayerState playerState = gameState.PlayerStates[m];
				if (!IsValidPlayer(playerState.Id))
				{
					continue;
				}
				if (dictionary.ContainsKey(playerState.Id) && playerState.tasks != null && playerState.tasks.Count > 0)
				{
					foreach (TaskBase task in playerState.tasks)
					{
						if (task is ExplorerTask explorerTask)
						{
							explorerTask.ExploredTiles = dictionary[playerState.Id];
						}
					}
				}
				playerState.knownPlayers = ParseKnownTribes(knownPlayers[playerState.Id]);
				foreach (KeyValuePair<string, Dictionary<string, int>> playerOpinion in playerOpinions)
				{
					if (!IsValidPlayer(playerState.Id) || !playerIdIndex.ContainsKey(playerOpinion.Key) || playerIdIndex[playerOpinion.Key] != playerState.Id)
					{
						continue;
					}
					foreach (KeyValuePair<string, int> item in playerOpinion.Value)
					{
						if (playerIdIndex.ContainsKey(item.Key))
						{
							byte key = playerIdIndex[item.Key];
							playerState.aggressions.Add(key, item.Value);
						}
					}
				}
				foreach (PlayerState playerState2 in gameState.PlayerStates)
				{
					if (playerState2.Id != playerState.Id && !playerState.aggressions.ContainsKey(playerState2.Id))
					{
						playerState.aggressions.Add(playerState2.Id, 0);
					}
				}
				if (playerState.Id != byte.MaxValue)
				{
					CheckTasks(gameState, playerState);
					playerState.score = (uint)playerState.GetScore(gameState).totalScore;
					playerState.cities = playerState.CountCities(gameState);
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to migrate with error {0}", new object[1] { ex });
			return false;
		}
	}

	private UnitState MigrateUnitState(ByteArrayReader reader, TileData tile, ushort mapSize, string[] stringCache)
	{
		try
		{
			if (!reader.ReadBoolean())
			{
				return null;
			}
			UnitState unitState = new UnitState();
			unitState.id = gameState.GetNextUnitId();
			unitState.coordinates = tile.coordinates;
			string text = GetStringFromCache(reader, stringCache);
			short result = -1;
			if (text.Contains(":"))
			{
				string[] array = text.Split(':');
				text = array[0];
				short.TryParse(array[1], out result);
			}
			unitState.type = ParseUnitType(text);
			unitState.health = (ushort)(reader.ReadInt16() * 10);
			reader.ReadInt16();
			unitState.moved = reader.ReadBoolean();
			unitState.attacked = reader.ReadBoolean();
			int degrees = reader.ReadInt16();
			unitState.direction = ParseDirectionFromRotation(degrees);
			GridDirection direction = unitState.direction;
			if ((uint)direction <= 3u)
			{
				unitState.flipped = true;
			}
			else
			{
				unitState.flipped = false;
			}
			int num = (ushort)reader.ReadInt16();
			unitState.xp = (ushort)reader.ReadInt16();
			unitState.promotionLevel = (ushort)((unitState.xp >= 3 && num == 0) ? 1u : 0u);
			if (gameState.GameLogicData.TryGetData(unitState.type, out var data) && data.promotionLimit == 0)
			{
				unitState.promotionLevel = 0;
			}
			string stringFromCache = GetStringFromCache(reader, stringCache);
			UnitState unitState2 = null;
			if (!string.IsNullOrEmpty(stringFromCache))
			{
				unitState2 = new UnitState();
			}
			unitState.createdTurn = (ushort)reader.ReadInt16();
			short num2 = (short)(reader.ReadInt16() + 1);
			if (result == -1)
			{
				result = num2;
			}
			unitState.style = result;
			unitState.owner = GetPlayerIdFromStyle(gameState, num2);
			if (reader.ReadBoolean())
			{
				unitState.home = ReadFlippedWorldCoordinate(reader, mapSize);
			}
			if (appVersion >= Version.Parse("3.1"))
			{
				ParseExtraData(unitState, reader.ReadString());
			}
			if (unitState2 != null)
			{
				unitState2.type = ParseUnitType(stringFromCache);
				unitState2.id = gameState.GetNextUnitId();
				unitState2.style = unitState.style;
				unitState2.owner = unitState.owner;
				unitState2.home = unitState.home;
				unitState2.passengerUnit = unitState;
				unitState2.coordinates = unitState.coordinates;
				unitState2.health = unitState.health;
				unitState2.moved = unitState.moved;
				unitState2.attacked = unitState.attacked;
				unitState2.direction = unitState.direction;
				unitState2.promotionLevel = 0;
				unitState2.xp = 0;
				return unitState2;
			}
			return unitState;
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to migrate unit on tile {0} with error {1}", new object[2] { tile.coordinates, ex });
			return null;
		}
	}

	private ResourceState MigrateResourceState(ByteArrayReader reader, TileData tile, string[] stringCache)
	{
		try
		{
			if (!reader.ReadBoolean())
			{
				return null;
			}
			return new ResourceState
			{
				type = ParseResourceType(GetStringFromCache(reader, stringCache))
			};
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to migrate resource on tile {0} with error {1}", new object[2] { tile.coordinates, ex });
			return null;
		}
	}

	private ImprovementState MigrateBuildingState(ByteArrayReader reader, TileData tile, bool connectedToCapital, string[] stringCache)
	{
		try
		{
			if (!reader.ReadBoolean())
			{
				return null;
			}
			ImprovementState improvementState = new ImprovementState();
			improvementState.type = ParseBuildingType(GetStringFromCache(reader, stringCache));
			improvementState.name = GetStringFromCache(reader, stringCache);
			if (improvementState.name == "City")
			{
				improvementState.name = null;
			}
			improvementState.level = (ushort)reader.ReadInt16();
			if (improvementState.IsMonument())
			{
				improvementState.level = (ushort)Math.Max(0, improvementState.level - 1);
			}
			improvementState.founded = (ushort)(reader.ReadInt16() - 1);
			improvementState.founder = GetPlayerIdFromStyle(gameState, reader.ReadInt16());
			GetStringFromCache(reader, stringCache);
			improvementState.xp = reader.ReadInt16();
			improvementState.population = reader.ReadInt16();
			improvementState.rewards = ParseRewards(GetStringArrayFromCache(reader, stringCache));
			GetStringArrayFromCache(reader, stringCache);
			improvementState.production = (ushort)reader.ReadInt16();
			reader.ReadBoolean();
			improvementState.baseScore = (ushort)reader.ReadInt16();
			improvementState.borderSize = (ushort)reader.ReadInt16();
			reader.ReadInt16();
			improvementState.upgrade = (ushort)reader.ReadInt16();
			if (reader.ReadBoolean())
			{
				tile.owner = GetPlayerIdFromStyle(gameState, reader.ReadInt16() + 1);
			}
			return improvementState;
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to migrate building on tile {0} with error {1}", new object[2] { tile.coordinates, ex });
			return null;
		}
	}

	private bool MigrateActions(ByteArrayReader reader, int version)
	{
		try
		{
			string[] stringCache = reader.ReadStringArray();
			if (version >= 5)
			{
				reader.ReadInt16();
			}
			reader.ReadInt16();
			int num = reader.ReadInt32();
			if (num == -1)
			{
				return true;
			}
			int num2 = 24;
			int num3 = num / num2;
			for (int i = 0; i < num3; i++)
			{
				string stringFromCache = GetStringFromCache(reader, stringCache);
				Log.Verbose("Action {0}: {1}", new object[2] { i, stringFromCache });
				reader.ReadInt16();
				reader.ReadInt16();
				reader.ReadInt16();
				reader.ReadInt16();
				reader.ReadInt16();
				reader.ReadInt16();
				reader.ReadInt16();
				GetStringFromCache(reader, stringCache);
				switch (reader.ReadInt16())
				{
				case 1:
					reader.ReadInt16();
					reader.ReadInt16();
					break;
				case 2:
					GetStringFromCache(reader, stringCache);
					reader.ReadInt16();
					break;
				case 3:
					reader.ReadInt16();
					reader.ReadInt16();
					break;
				case 4:
					GetStringFromCache(reader, stringCache);
					reader.ReadInt16();
					break;
				case 5:
					GetStringFromCache(reader, stringCache);
					reader.ReadInt16();
					break;
				case 6:
					GetStringFromCache(reader, stringCache);
					reader.ReadInt16();
					break;
				case 7:
					GetStringFromCache(reader, stringCache);
					reader.ReadInt16();
					break;
				default:
					reader.ReadInt16();
					reader.ReadInt16();
					break;
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to migrate with error {0}", new object[1] { ex });
			return false;
		}
	}

	private static TerrainData.Type ParseTerrainString(string terrain)
	{
		return terrain switch
		{
			"water" => TerrainData.Type.Water, 
			"ocean" => TerrainData.Type.Ocean, 
			"field" => TerrainData.Type.Field, 
			"forest" => TerrainData.Type.Forest, 
			"mountain" => TerrainData.Type.Mountain, 
			"ice" => TerrainData.Type.Ice, 
			_ => TerrainData.Type.None, 
		};
	}

	private static List<TechData.Type> ParseTech(string[] availableTech)
	{
		if (availableTech == null || availableTech.Length == 0)
		{
			return new List<TechData.Type>();
		}
		List<TechData.Type> list = new List<TechData.Type>(availableTech.Length);
		for (int i = 0; i < availableTech.Length; i++)
		{
			switch (availableTech[i].ToLower())
			{
			case "basic":
				list.Add(TechData.Type.Basic);
				break;
			case "riding":
				list.Add(TechData.Type.Riding);
				break;
			case "free spirit":
				list.Add(TechData.Type.FreeSpirit);
				break;
			case "chivalry":
				list.Add(TechData.Type.Chivalry);
				break;
			case "roads":
				list.Add(TechData.Type.Roads);
				break;
			case "trade":
				list.Add(TechData.Type.Trade);
				break;
			case "organization":
				list.Add(TechData.Type.Organization);
				break;
			case "shields":
				list.Add(TechData.Type.Shields);
				break;
			case "farming":
				list.Add(TechData.Type.Farming);
				break;
			case "construction":
				list.Add(TechData.Type.Construction);
				break;
			case "fishing":
				list.Add(TechData.Type.Fishing);
				break;
			case "whaling":
				list.Add(TechData.Type.Whaling);
				break;
			case "aquatism":
				list.Add(TechData.Type.Aquatism);
				break;
			case "sailing":
				list.Add(TechData.Type.Sailing);
				break;
			case "navigation":
				list.Add(TechData.Type.Navigation);
				break;
			case "hunting":
				list.Add(TechData.Type.Hunting);
				break;
			case "forestry":
				list.Add(TechData.Type.Forestry);
				break;
			case "mathematics":
				list.Add(TechData.Type.Mathematics);
				break;
			case "archery":
				list.Add(TechData.Type.Archery);
				break;
			case "spiritualism":
				list.Add(TechData.Type.Spiritualism);
				break;
			case "climbing":
				list.Add(TechData.Type.Climbing);
				break;
			case "meditation":
				list.Add(TechData.Type.Meditation);
				break;
			case "philosophy":
				list.Add(TechData.Type.Philosophy);
				break;
			case "mining":
				list.Add(TechData.Type.Mining);
				break;
			case "smithery":
				list.Add(TechData.Type.Smithery);
				break;
			case "riding2":
				list.Add(TechData.Type.Riding2);
				break;
			case "free diving":
				list.Add(TechData.Type.FreeDiving);
				break;
			case "spearing":
				list.Add(TechData.Type.Spearing);
				break;
			case "forest magic":
				list.Add(TechData.Type.ForestMagic);
				break;
			case "water magic":
				list.Add(TechData.Type.WaterMagic);
				break;
			case "frostwork":
				list.Add(TechData.Type.Frostwork);
				break;
			case "polar warfare":
				list.Add(TechData.Type.PolarWarfare);
				break;
			case "polarism":
				list.Add(TechData.Type.Polarism);
				break;
			default:
				Log.Warning("Unknown tech: {0}", new object[1] { availableTech[i] });
				break;
			}
		}
		return list;
	}

	private static UnitData.Type ParseUnitType(string unitString)
	{
		switch (unitString)
		{
		case "scout":
			return UnitData.Type.Scout;
		case "warrior":
			return UnitData.Type.Warrior;
		case "rider":
			return UnitData.Type.Rider;
		case "knight":
			return UnitData.Type.Knight;
		case "defender":
			return UnitData.Type.Defender;
		case "ship":
			return UnitData.Type.Ship;
		case "battleship":
			return UnitData.Type.Battleship;
		case "catapult":
			return UnitData.Type.Catapult;
		case "archer":
			return UnitData.Type.Archer;
		case "priest":
			return UnitData.Type.MindBender;
		case "swordman":
			return UnitData.Type.Swordsman;
		case "giant":
			return UnitData.Type.Giant;
		case "bunny":
			return UnitData.Type.Bunny;
		case "boat":
			return UnitData.Type.Boat;
		case "polytaur":
			return UnitData.Type.Polytaur;
		case "seamonster":
			return UnitData.Type.Navalon;
		case "egg":
			return UnitData.Type.DragonEgg;
		case "dragon":
			return UnitData.Type.BabyDragon;
		case "dragon_large":
			return UnitData.Type.FireDragon;
		case "amphibian":
			return UnitData.Type.Amphibian;
		case "tridention":
			return UnitData.Type.Tridention;
		case "icemaker":
			return UnitData.Type.Mooni;
		case "battlesled":
			return UnitData.Type.BattleSled;
		case "fortress":
			return UnitData.Type.IceFortress;
		case "ice archer":
			return UnitData.Type.IceArcher;
		case "crab":
			return UnitData.Type.Crab;
		case "wendy":
			return UnitData.Type.Gaami;
		default:
			Log.Warning("Unable to parse unit type: {0}", new object[1] { unitString });
			return UnitData.Type.None;
		}
	}

	private static ResourceData.Type ParseResourceType(string type)
	{
		return type switch
		{
			"fruit" => ResourceData.Type.Fruit, 
			"crop" => ResourceData.Type.Crop, 
			"fish" => ResourceData.Type.Fish, 
			"whale" => ResourceData.Type.Whale, 
			"game" => ResourceData.Type.Game, 
			"metal" => ResourceData.Type.Metal, 
			_ => ResourceData.Type.None, 
		};
	}

	private static ImprovementData.Type ParseBuildingType(string type)
	{
		return type switch
		{
			"City" => ImprovementData.Type.City, 
			"ruin" => ImprovementData.Type.Ruin, 
			"Monument1" => ImprovementData.Type.Monument1, 
			"Monument2" => ImprovementData.Type.Monument2, 
			"Monument3" => ImprovementData.Type.Monument3, 
			"Monument4" => ImprovementData.Type.Monument4, 
			"Monument5" => ImprovementData.Type.Monument5, 
			"Monument6" => ImprovementData.Type.Monument6, 
			"Monument7" => ImprovementData.Type.Monument7, 
			"Temple" => ImprovementData.Type.Temple, 
			"Burn Forest" => ImprovementData.Type.BurnForest, 
			"Road" => ImprovementData.Type.Road, 
			"Customs House" => ImprovementData.Type.CustomsHouse, 
			"Gather" => ImprovementData.Type.HarvestFruit, 
			"Farm" => ImprovementData.Type.Farm, 
			"Windmill" => ImprovementData.Type.Windmill, 
			"Fishing" => ImprovementData.Type.Fishing, 
			"Whale Hunting" => ImprovementData.Type.WhaleHunting, 
			"Water Temple" => ImprovementData.Type.WaterTemple, 
			"Port" => ImprovementData.Type.Port, 
			"Hunting" => ImprovementData.Type.Hunting, 
			"Clear Forest" => ImprovementData.Type.ClearForest, 
			"Lumber Hut" => ImprovementData.Type.LumberHut, 
			"Sawmill" => ImprovementData.Type.Sawmill, 
			"Grow Forest" => ImprovementData.Type.GrowForest, 
			"Forest Temple" => ImprovementData.Type.ForestTemple, 
			"Mountain Temple" => ImprovementData.Type.MountainTemple, 
			"Mine" => ImprovementData.Type.Mine, 
			"Forge" => ImprovementData.Type.Forge, 
			"enchant" => ImprovementData.Type.EnchantAnimal, 
			"enchant_whale" => ImprovementData.Type.EnchantWhale, 
			"sanctuary" => ImprovementData.Type.Sanctuary, 
			"ice_bank" => ImprovementData.Type.IceBank, 
			"iceport" => ImprovementData.Type.Outpost, 
			"Ice Temple" => ImprovementData.Type.IceTemple, 
			_ => ImprovementData.Type.None, 
		};
	}

	private static GridDirection ParseDirectionFromRotation(int degrees)
	{
		GridDirection[] obj = new GridDirection[8]
		{
			GridDirection.S,
			GridDirection.SW,
			GridDirection.W,
			GridDirection.NW,
			GridDirection.N,
			GridDirection.NE,
			GridDirection.E,
			GridDirection.SE
		};
		int num = (int)Math.Round((float)degrees / 45f) % 8;
		return obj[num];
	}

	private static List<ImprovementData.Type> ParseUniqueBuildings(string[] uniqueBuildings)
	{
		if (uniqueBuildings == null || uniqueBuildings.Length == 0)
		{
			return new List<ImprovementData.Type>();
		}
		List<ImprovementData.Type> list = new List<ImprovementData.Type>();
		for (int i = 0; i < uniqueBuildings.Length; i++)
		{
			ImprovementData.Type type = ParseBuildingType(uniqueBuildings[i]);
			if (type != ImprovementData.Type.None)
			{
				list.Add(type);
			}
		}
		return list;
	}

	private static void ParseTasks(PlayerState playerState, string[] activeTasks, int pacifistTurns = 0)
	{
		if (activeTasks == null || activeTasks.Length == 0)
		{
			return;
		}
		for (int i = 0; i < activeTasks.Length; i++)
		{
			TaskBase taskBase = null;
			switch (activeTasks[i])
			{
			case "pacifist":
				taskBase = new PacifistTask(pacifistTurns);
				break;
			case "genius":
				taskBase = new GeniusTask();
				break;
			case "network":
				taskBase = new NetworkTask();
				break;
			case "wealth":
				taskBase = new WealthTask();
				break;
			case "killer":
				taskBase = new KillerTask((int)playerState.kills);
				break;
			case "metropolis":
				taskBase = new MetropolisTask();
				break;
			case "explorer":
				taskBase = new ExplorerTask();
				break;
			default:
				Log.Warning("Unknown task: {0}", new object[1] { activeTasks[i] });
				break;
			}
			if (taskBase != null)
			{
				taskBase.EnableTask();
				playerState.tasks.Add(taskBase);
			}
		}
	}

	private static void CheckTasks(GameState gameState, PlayerState playerState)
	{
		if (playerState.tasks != null && playerState.tasks.Count != 0)
		{
			for (int i = 0; i < playerState.tasks.Count; i++)
			{
				playerState.tasks[i].IsTaskCompleted(gameState, playerState);
			}
		}
	}

	private static List<CityReward> ParseRewards(string[] rewardStrings)
	{
		if (rewardStrings == null || rewardStrings.Length == 0)
		{
			return null;
		}
		List<CityReward> list = new List<CityReward>();
		for (int i = 0; i < rewardStrings.Length; i++)
		{
			switch (rewardStrings[i])
			{
			case "city wall":
				list.Add(CityReward.CityWall);
				break;
			case "park":
				list.Add(CityReward.Park);
				break;
			case "workshop":
				list.Add(CityReward.Workshop);
				break;
			case "explorer":
				list.Add(CityReward.Explorer);
				break;
			case "border growth":
				list.Add(CityReward.BorderGrowth);
				break;
			case "super unit":
				list.Add(CityReward.SuperUnit);
				break;
			case "resources":
				list.Add(CityReward.Resources);
				break;
			case "population growth":
				list.Add(CityReward.PopulationGrowth);
				break;
			}
		}
		return list;
	}

	private List<byte> ParseKnownTribes(string[] knownTribes)
	{
		if (knownTribes == null || knownTribes.Length == 0)
		{
			return new List<byte>();
		}
		List<byte> list = new List<byte>(knownTribes.Length);
		for (int i = 0; i < knownTribes.Length; i++)
		{
			if (playerIdIndex.ContainsKey(knownTribes[i]))
			{
				byte item = playerIdIndex[knownTribes[i]];
				list.Add(item);
			}
		}
		return list;
	}

	private static void ParseExtraData(UnitState unitState, string extraDataString)
	{
		if (string.IsNullOrEmpty(extraDataString))
		{
			return;
		}
		JSONNode.Enumerator enumerator = JSON.Parse(extraDataString).GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, JSONNode> current = enumerator.Current;
			if (!(current.Key == "effects"))
			{
				continue;
			}
			JSONArray asArray = current.Value.AsArray;
			for (int i = 0; i < asArray.Count; i++)
			{
				if (asArray[i].Value == "frozen")
				{
					unitState.AddEffect(UnitEffect.Frozen);
					continue;
				}
				Log.Warning("Unable to parse UnitEffect: {0}", new object[1] { asArray[i].ToString() });
			}
		}
	}

	private List<byte> ParseTileExplorers(string[] explorers)
	{
		if (explorers == null || explorers.Length == 0)
		{
			return null;
		}
		List<byte> list = new List<byte>();
		for (int i = 0; i < explorers.Length; i++)
		{
			string text = explorers[i].ToLower();
			if (mixedTribeMapping.TryGetValue(text, out var value))
			{
				text = value;
			}
			byte item = playerTribeIndex[SimplifyTribeString(text)];
			list.Add(item);
		}
		return list;
	}

	private static TribeData.Type ParseTribeString(string tribeString)
	{
		switch (tribeString)
		{
		case "Xin-xi":
		case "xin-xi":
			return TribeData.Type.Xinxi;
		case "Imperius":
		case "imperius":
			return TribeData.Type.Imperius;
		case "Bardur":
		case "bardur":
			return TribeData.Type.Bardur;
		case "Oumaji":
		case "oumaji":
			return TribeData.Type.Oumaji;
		case "Kickoo":
		case "kickoo":
			return TribeData.Type.Kickoo;
		case "Hoodrick":
		case "hoodrick":
			return TribeData.Type.Hoodrick;
		case "Luxidoor":
		case "luxidoor":
			return TribeData.Type.Luxidoor;
		case "Vengir":
		case "vengir":
			return TribeData.Type.Vengir;
		case "Zebasi":
		case "zebasi":
			return TribeData.Type.Zebasi;
		case "Ai-Mo":
		case "ai-mo":
		case "AiMo":
		case "aimo":
			return TribeData.Type.Aimo;
		case "Aquarion":
		case "aquarion":
			return TribeData.Type.Aquarion;
		case "Quetzali":
		case "quetzali":
			return TribeData.Type.Quetzali;
		case "∑∫ỹriȱŋ":
		case "Elyrion":
		case "elyrion":
			return TribeData.Type.Elyrion;
		case "Yădakk":
		case "Yadakk":
		case "yădakk":
		case "yadakk":
			return TribeData.Type.Yadakk;
		case "Polaris":
		case "polaris":
			return TribeData.Type.Polaris;
		case "Nature":
		case "nature":
			return TribeData.Type.Nature;
		default:
			Log.Warning("Unable to parse tribe: {0}", new object[1] { tribeString });
			return TribeData.Type.None;
		}
	}

	private static string GetTribeName(string tribeName)
	{
		return tribeName switch
		{
			"aimo" => "ai-mo", 
			"yadakk" => "yădakk", 
			"elyrion" => "∑∫ỹriȱŋ", 
			_ => tribeName, 
		};
	}

	private static string SimplifyTribeString(string tribeString)
	{
		tribeString = tribeString.ToLower();
		return tribeString switch
		{
			"ai-mo" => "aimo", 
			"∑∫ỹriȱŋ" => "elyrion", 
			"yădakk" => "yadakk", 
			_ => tribeString, 
		};
	}

	private static TribeData.Type ParseTribeFromStyle(int style)
	{
		switch (style)
		{
		case 0:
			return TribeData.Type.None;
		case 1:
			return TribeData.Type.Xinxi;
		case 2:
			return TribeData.Type.Imperius;
		case 3:
			return TribeData.Type.Bardur;
		case 4:
			return TribeData.Type.Oumaji;
		case 5:
			return TribeData.Type.Kickoo;
		case 6:
			return TribeData.Type.Hoodrick;
		case 7:
			return TribeData.Type.Luxidoor;
		case 8:
			return TribeData.Type.Vengir;
		case 9:
			return TribeData.Type.Zebasi;
		case 10:
			return TribeData.Type.Aimo;
		case 11:
			return TribeData.Type.Aquarion;
		case 12:
			return TribeData.Type.Quetzali;
		case 13:
			return TribeData.Type.Elyrion;
		case 14:
			return TribeData.Type.Yadakk;
		case 15:
			return TribeData.Type.Polaris;
		default:
			Log.Warning("Unable to parse tribe from style: {0}", new object[1] { style });
			return TribeData.Type.None;
		}
	}

	private static GameMode ParseGameModeString(string gameMode)
	{
		return gameMode switch
		{
			"perfection_mode" => GameMode.Perfection, 
			"domination_mode" => GameMode.Domination, 
			"score_mode" => GameMode.Glory, 
			"capital_mode" => GameMode.Might, 
			_ => GameMode.None, 
		};
	}

	private static GameType ParseGameType(int gameType)
	{
		return gameType switch
		{
			0 => GameType.SinglePlayer, 
			1 => GameType.PassAndPlay, 
			2 => GameType.Multiplayer, 
			_ => GameType.PassAndPlay, 
		};
	}

	private static GameState.State ParseGameState(int gameState)
	{
		return gameState switch
		{
			0 => GameState.State.Lobby, 
			1 => GameState.State.Started, 
			2 => GameState.State.Ended, 
			3 => GameState.State.Lobby, 
			_ => GameState.State.Unknown, 
		};
	}

	private static byte GetPlayerIdFromStyle(GameState gameState, int style)
	{
		for (int i = 0; i < gameState.PlayerStates.Count; i++)
		{
			PlayerState playerState = gameState.PlayerStates[i];
			if (playerState.GetTribeStyle(gameState) == style)
			{
				return playerState.Id;
			}
		}
		return 0;
	}

	private Guid GetGuidForPlayer(string onlineId)
	{
		if (legacyPlayerMapping != null && legacyPlayerMapping.TryGetValue(onlineId, out var value))
		{
			return value;
		}
		return Guid.Empty;
	}

	private bool IsValidPlayer(byte playerId)
	{
		return !erroneousPlayer.Contains(playerId);
	}

	private static void UpdateConnectedCities(GameState gameState)
	{
		List<TileData> changedTiles = new List<TileData>();
		gameState.UpdateRoutes(changedTiles);
		List<TileData> list = new List<TileData>();
		List<TileData> list2 = new List<TileData>();
		for (int i = 0; i < gameState.PlayerStates.Count; i++)
		{
			PlayerState playerState = gameState.PlayerStates[i];
			list.Clear();
			list2.Clear();
			gameState.Map.GetPlayerCityTiles(playerState.Id, list);
			gameState.FindConnectedCities(playerState.Id, list, list2);
			foreach (TileData item in list)
			{
				bool flag = list2.Contains(item);
				if (!flag)
				{
					item.improvement.connectedToCapitalOfPlayer = 0;
				}
				else if (flag)
				{
					item.improvement.connectedToCapitalOfPlayer = playerState.Id;
				}
			}
		}
	}

	private static string GetStringFromCache(ByteArrayReader reader, string[] stringCache)
	{
		return stringCache[reader.ReadInt16()];
	}

	private static string[] GetStringArrayFromCache(ByteArrayReader reader, string[] stringCache)
	{
		int num = reader.ReadInt16();
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = stringCache[reader.ReadInt16()];
		}
		return array;
	}
}
