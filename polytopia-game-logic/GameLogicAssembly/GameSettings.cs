using System;
using System.Collections.Generic;
using System.IO;
using Polytopia.Data;
using PolytopiaBackendBase.Game;

public class GameSettings : IBinarySerializable
{
	public enum Difficulties
	{
		Easy,
		Normal,
		Hard,
		Crazy,
		Frozen
	}

	public const int DEFAULT_MATCHMAKING_MAPSIZE = 0;

	public const int DEFAULT_MATCHMAKING_OPPONENT_COUNT = 1;

	public const MapPreset DEFAULT_MATCHMAKING_MAP_PRESET = MapPreset.None;

	protected string gameName = "No Name";

	protected int opponentCount;

	protected Difficulties difficulty;

	protected GameMode baseGameMode = GameMode.Perfection;

	protected GameType gameType;

	protected int mapSize;

	public List<TribeData.Type> disabledTribes = new List<TribeData.Type>();

	private List<TribeData.Type> unlockedTribes = new List<TribeData.Type>();

	protected Dictionary<TribeData.Type, SkinType> selectedSkins = new Dictionary<TribeData.Type, SkinType>();

	public Dictionary<Guid, PlayerData> players = new Dictionary<Guid, PlayerData>();

	public MapPreset mapPreset;

	private GameMode rulesGameMode = GameMode.Perfection;

	public GameRules rules = new GameRules();

	public int TimeLimit { get; set; }

	public float TimeBonusPerPopulation { get; set; }

	public float TimeBonusPerCity { get; set; }

	public float BaseTimeSeconds { get; set; }

	public bool UseDynamicTimers { get; set; }

	public bool UseTimeBanks { get; set; }

	public bool LiveGamePreset { get; set; }

	public bool IsAutoSkipEnabled { get; set; }

	public GameMode BaseGameMode
	{
		get
		{
			return baseGameMode;
		}
		set
		{
			baseGameMode = value;
			if (baseGameMode != GameMode.Custom)
			{
				RulesGameMode = value;
			}
		}
	}

	public GameMode RulesGameMode
	{
		get
		{
			return rulesGameMode;
		}
		set
		{
			rulesGameMode = value;
			rules.LoadPreset(rulesGameMode);
		}
	}

	public GameType GameType
	{
		get
		{
			return gameType;
		}
		set
		{
			gameType = value;
		}
	}

	public string GameName
	{
		get
		{
			return gameName;
		}
		set
		{
			gameName = value;
		}
	}

	public Difficulties Difficulty
	{
		get
		{
			return difficulty;
		}
		set
		{
			difficulty = value;
		}
	}

	public int Handicap => HandicapFromDifficulty(Difficulty);

	public int MapSize
	{
		get
		{
			switch (baseGameMode)
			{
			case GameMode.Perfection:
				return 16;
			case GameMode.Domination:
				if (opponentCount <= 1)
				{
					return 11;
				}
				if (opponentCount <= 2)
				{
					return 14;
				}
				if (opponentCount <= 3)
				{
					return 16;
				}
				return 18;
			case GameMode.Tutorial:
				return 14;
			default:
				return mapSize;
			}
		}
		set
		{
			mapSize = value;
		}
	}

	public float DifficultyBonusMultiplier => ScoreSheet.GetDifficultyBonusMultiplier(difficulty, OpponentCount);

	public int OpponentCount
	{
		get
		{
			return opponentCount;
		}
		set
		{
			opponentCount = value;
		}
	}

	public PlayerData[] Players
	{
		get
		{
			int count = players.Count;
			PlayerData[] array = new PlayerData[count];
			int[] array2 = new int[count];
			int num = 0;
			foreach (PlayerData value in players.Values)
			{
				PlayerData playerData = (array[num] = value);
				if (playerData.type == PlayerData.Type.Player)
				{
					array2[num] = 0;
				}
				else if (playerData.type == PlayerData.Type.Bot)
				{
					array2[num] = count + (num + 1);
				}
				else
				{
					array2[num] = num + 1;
				}
				num++;
			}
			Array.Sort(array2, array);
			return array;
		}
	}

	public PlayerData[] AlivePlayers
	{
		get
		{
			PlayerData[] array = Players;
			int num = array.Length;
			List<PlayerData> list = new List<PlayerData>(num);
			for (int i = 0; i < num; i++)
			{
				PlayerData playerData = array[i];
				if (playerData.type != PlayerData.Type.Bot && playerData.type != PlayerData.Type.None)
				{
					list.Add(playerData);
				}
			}
			return list.ToArray();
		}
	}

	public MapGeneratorSettings GetMapGeneratorSettings()
	{
		return MapGeneratorSettings.CreateFromPreset(mapPreset);
	}

	public void SetLiveModePreset()
	{
		LiveGamePreset = true;
		BaseTimeSeconds = 8f;
		TimeBonusPerCity = 12f;
		TimeBonusPerPopulation = 1f;
		UseTimeBanks = true;
		UseDynamicTimers = true;
		IsAutoSkipEnabled = true;
	}

	public void AddPlayer(PlayerData data)
	{
		if (!players.ContainsKey(data.profile.id))
		{
			players.Add(data.profile.id, data);
		}
		OpponentCount = players.Count;
	}

	public void RemovePlayer(PlayerData data)
	{
		RemovePlayer(data.profile.id);
	}

	public void RemovePlayer(Guid id)
	{
		if (players.ContainsKey(id))
		{
			players.Remove(id);
		}
		OpponentCount = players.Count;
	}

	public void ClearPlayers()
	{
		players.Clear();
		OpponentCount = players.Count;
	}

	public PlayerData GetPlayer(Guid id)
	{
		if (players.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public bool HavePlayer(PlayerData data)
	{
		return HavePlayer(data.profile.id);
	}

	public bool HavePlayer(Guid id)
	{
		return players.ContainsKey(id);
	}

	public List<Guid> GetPlayerIds()
	{
		List<Guid> list = new List<Guid>();
		foreach (Guid key in players.Keys)
		{
			list.Add(key);
		}
		return list;
	}

	public void SetSelectedSkin(TribeData.Type tribeType, SkinType skinType)
	{
		selectedSkins[tribeType] = skinType;
	}

	public SkinType GetSelectedSkin(TribeData.Type tribeType)
	{
		if (selectedSkins.TryGetValue(tribeType, out var value) && !value.IsLocked())
		{
			return value;
		}
		return SkinType.Default;
	}

	public void DisableTribe(TribeData.Type tribeType)
	{
		if (!disabledTribes.Contains(tribeType))
		{
			disabledTribes.Add(tribeType);
		}
	}

	public void EnableTribe(TribeData.Type tribeType)
	{
		if (disabledTribes.Contains(tribeType))
		{
			disabledTribes.Remove(tribeType);
		}
	}

	public bool IsTribeEnabled(TribeData.Type tribeType)
	{
		if (tribeType == TribeData.Type.None || tribeType == TribeData.Type.Nature)
		{
			return false;
		}
		return !disabledTribes.Contains(tribeType);
	}

	public void SetUnlockedTribes(List<TribeData.Type> unlockedTribes)
	{
		this.unlockedTribes = unlockedTribes;
	}

	public bool IsTribeLocked(TribeData.Type tribeType)
	{
		return !unlockedTribes.Contains(tribeType);
	}

	public bool IsAnyTribeUnlocked()
	{
		if (unlockedTribes != null)
		{
			return unlockedTribes.Count > 0;
		}
		return false;
	}

	public void ApplyLobbySettings(LobbyGameViewModel lobbyGameViewModel)
	{
		LiveGamePreset = !lobbyGameViewModel.IsPersistent;
		GameType = GameType.Multiplayer;
		GameName = lobbyGameViewModel.Name;
		MapSize = lobbyGameViewModel.MapSize;
		mapPreset = lobbyGameViewModel.MapPreset;
		BaseGameMode = lobbyGameViewModel.GameMode;
		if (lobbyGameViewModel.ScoreLimit != -1)
		{
			rules.ScoreLimit = lobbyGameViewModel.ScoreLimit;
		}
		disabledTribes = new List<TribeData.Type>();
		for (int i = 0; i < lobbyGameViewModel.DisabledTribes.Count; i++)
		{
			disabledTribes.Add((TribeData.Type)lobbyGameViewModel.DisabledTribes[i]);
		}
	}

	public void HardReset()
	{
		opponentCount = 0;
		Difficulty = Difficulties.Easy;
		baseGameMode = GameMode.Perfection;
		GameType = GameType.SinglePlayer;
		mapSize = 0;
		rulesGameMode = GameMode.Perfection;
		mapPreset = MapPreset.None;
		disabledTribes.Clear();
		SoftReset();
	}

	public void SoftReset()
	{
		GameName = PolyLanguage.MakeGameName();
		players.Clear();
		if (gameType != GameType.SinglePlayer && gameType != GameType.Matchmaking)
		{
			opponentCount = 0;
		}
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		if (version < 9)
		{
			Serialize8(writer, version);
		}
		else if (version < 70)
		{
			Serialize9(writer, version);
		}
		else
		{
			SerializeDefault(writer, version);
		}
	}

	public void SerializeDefault(BinaryWriter writer, int version)
	{
		rules.Serialize(writer, version);
		writer.Write((byte)baseGameMode);
		writer.Write((byte)rulesGameMode);
		writer.Write(GameName ?? "");
		writer.Write(MapSize);
		writer.Write((ushort)((disabledTribes != null) ? ((uint)disabledTribes.Count) : 0u));
		if (disabledTribes != null)
		{
			for (int i = 0; i < disabledTribes.Count; i++)
			{
				writer.Write((ushort)disabledTribes[i]);
			}
		}
		writer.Write((ushort)((unlockedTribes != null) ? ((uint)unlockedTribes.Count) : 0u));
		if (unlockedTribes != null)
		{
			for (int j = 0; j < unlockedTribes.Count; j++)
			{
				writer.Write((ushort)unlockedTribes[j]);
			}
		}
		writer.Write((ushort)Difficulty);
		writer.Write(OpponentCount);
		writer.Write((ushort)GameType);
		writer.Write((byte)mapPreset);
		writer.Write(TimeLimit);
		writer.Write(TimeBonusPerCity);
		writer.Write(TimeBonusPerPopulation);
		writer.Write(BaseTimeSeconds);
		writer.Write(UseDynamicTimers);
		writer.Write(UseTimeBanks);
		writer.Write(LiveGamePreset);
		writer.Write(IsAutoSkipEnabled);
		if (version >= 86)
		{
			SaveSkins();
		}
		void SaveSkins()
		{
			writer.Write(selectedSkins.Count);
			foreach (KeyValuePair<TribeData.Type, SkinType> selectedSkin in selectedSkins)
			{
				writer.Write((ushort)selectedSkin.Key);
				writer.Write((ushort)selectedSkin.Value);
			}
		}
	}

	public void Serialize9(BinaryWriter writer, int version)
	{
		rules.Serialize(writer, version);
		writer.Write((byte)baseGameMode);
		writer.Write((byte)rulesGameMode);
		writer.Write(GameName ?? "");
		writer.Write(MapSize);
		writer.Write((ushort)((disabledTribes != null) ? ((uint)disabledTribes.Count) : 0u));
		if (disabledTribes != null)
		{
			for (int i = 0; i < disabledTribes.Count; i++)
			{
				writer.Write((ushort)disabledTribes[i]);
			}
		}
		writer.Write((ushort)((unlockedTribes != null) ? ((uint)unlockedTribes.Count) : 0u));
		if (unlockedTribes != null)
		{
			for (int j = 0; j < unlockedTribes.Count; j++)
			{
				writer.Write((ushort)unlockedTribes[j]);
			}
		}
		writer.Write((ushort)Difficulty);
		writer.Write(OpponentCount);
		writer.Write((ushort)GameType);
		writer.Write((byte)mapPreset);
		writer.Write(TimeLimit);
	}

	public void Serialize8(BinaryWriter writer, int version)
	{
		rules.Serialize(writer, version);
		writer.Write((byte)baseGameMode);
		writer.Write((byte)rulesGameMode);
		writer.Write(GameName ?? "");
		writer.Write(MapSize);
		writer.Write((ushort)((disabledTribes != null) ? ((uint)disabledTribes.Count) : 0u));
		if (disabledTribes != null)
		{
			for (int i = 0; i < disabledTribes.Count; i++)
			{
				writer.Write((ushort)disabledTribes[i]);
			}
		}
		writer.Write((ushort)((unlockedTribes != null) ? ((uint)unlockedTribes.Count) : 0u));
		if (unlockedTribes != null)
		{
			for (int j = 0; j < unlockedTribes.Count; j++)
			{
				writer.Write((ushort)unlockedTribes[j]);
			}
		}
		writer.Write((ushort)Difficulty);
		writer.Write(OpponentCount);
		writer.Write((ushort)GameType);
		writer.Write((byte)mapPreset);
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		if (version < 9)
		{
			Deserialize8(reader, version);
		}
		else if (version < 70)
		{
			Deserialize9(reader, version);
		}
		else
		{
			DeserializeDefault(reader, version);
		}
	}

	public void DeserializeDefault(BinaryReader reader, int version)
	{
		rules.Deserialize(reader, version);
		baseGameMode = (GameMode)reader.ReadByte();
		rulesGameMode = (GameMode)reader.ReadByte();
		GameName = reader.ReadString();
		MapSize = reader.ReadInt32();
		ushort num = reader.ReadUInt16();
		if (disabledTribes == null)
		{
			disabledTribes = new List<TribeData.Type>(num);
		}
		disabledTribes.Clear();
		for (int i = 0; i < num; i++)
		{
			disabledTribes.Add((TribeData.Type)reader.ReadUInt16());
		}
		ushort num2 = reader.ReadUInt16();
		if (unlockedTribes == null)
		{
			unlockedTribes = new List<TribeData.Type>(num2);
		}
		unlockedTribes.Clear();
		for (int j = 0; j < num2; j++)
		{
			unlockedTribes.Add((TribeData.Type)reader.ReadUInt16());
		}
		Difficulty = (Difficulties)reader.ReadUInt16();
		OpponentCount = reader.ReadInt32();
		GameType = (GameType)reader.ReadUInt16();
		mapPreset = (MapPreset)reader.ReadByte();
		TimeLimit = reader.ReadInt32();
		TimeBonusPerCity = reader.ReadSingle();
		TimeBonusPerPopulation = reader.ReadSingle();
		BaseTimeSeconds = reader.ReadSingle();
		UseDynamicTimers = reader.ReadBoolean();
		UseTimeBanks = reader.ReadBoolean();
		LiveGamePreset = reader.ReadBoolean();
		IsAutoSkipEnabled = reader.ReadBoolean();
		if (version >= 86)
		{
			LoadSkins();
		}
		void LoadSkins()
		{
			selectedSkins.Clear();
			int num3 = reader.ReadInt32();
			for (int k = 0; k < num3; k++)
			{
				TribeData.Type key = (TribeData.Type)reader.ReadInt16();
				SkinType value = (SkinType)reader.ReadInt16();
				selectedSkins.Add(key, value);
			}
		}
	}

	public void Deserialize9(BinaryReader reader, int version)
	{
		rules.Deserialize(reader, version);
		baseGameMode = (GameMode)reader.ReadByte();
		rulesGameMode = (GameMode)reader.ReadByte();
		GameName = reader.ReadString();
		MapSize = reader.ReadInt32();
		ushort num = reader.ReadUInt16();
		if (disabledTribes == null)
		{
			disabledTribes = new List<TribeData.Type>(num);
		}
		disabledTribes.Clear();
		for (int i = 0; i < num; i++)
		{
			disabledTribes.Add((TribeData.Type)reader.ReadUInt16());
		}
		ushort num2 = reader.ReadUInt16();
		if (unlockedTribes == null)
		{
			unlockedTribes = new List<TribeData.Type>(num2);
		}
		unlockedTribes.Clear();
		for (int j = 0; j < num2; j++)
		{
			unlockedTribes.Add((TribeData.Type)reader.ReadUInt16());
		}
		Difficulty = (Difficulties)reader.ReadUInt16();
		OpponentCount = reader.ReadInt32();
		GameType = (GameType)reader.ReadUInt16();
		mapPreset = (MapPreset)reader.ReadByte();
		TimeLimit = reader.ReadInt32();
	}

	public void Deserialize8(BinaryReader reader, int version)
	{
		rules.Deserialize(reader, version);
		baseGameMode = (GameMode)reader.ReadByte();
		rulesGameMode = (GameMode)reader.ReadByte();
		GameName = reader.ReadString();
		MapSize = reader.ReadInt32();
		ushort num = reader.ReadUInt16();
		if (disabledTribes == null)
		{
			disabledTribes = new List<TribeData.Type>(num);
		}
		disabledTribes.Clear();
		for (int i = 0; i < num; i++)
		{
			disabledTribes.Add((TribeData.Type)reader.ReadUInt16());
		}
		ushort num2 = reader.ReadUInt16();
		if (unlockedTribes == null)
		{
			unlockedTribes = new List<TribeData.Type>(num2);
		}
		unlockedTribes.Clear();
		for (int j = 0; j < num2; j++)
		{
			unlockedTribes.Add((TribeData.Type)reader.ReadUInt16());
		}
		Difficulty = (Difficulties)reader.ReadUInt16();
		OpponentCount = reader.ReadInt32();
		GameType = (GameType)reader.ReadUInt16();
		mapPreset = (MapPreset)reader.ReadByte();
	}

	public static int HandicapFromDifficulty(Difficulties difficulty)
	{
		return difficulty switch
		{
			Difficulties.Frozen => -1, 
			Difficulties.Easy => 0, 
			Difficulties.Normal => 1, 
			Difficulties.Hard => 2, 
			Difficulties.Crazy => 4, 
			_ => 0, 
		};
	}

	public static Difficulties DifficultyFromHandicap(int handicap)
	{
		return handicap switch
		{
			-1 => Difficulties.Frozen, 
			0 => Difficulties.Easy, 
			1 => Difficulties.Normal, 
			2 => Difficulties.Hard, 
			4 => Difficulties.Crazy, 
			_ => Difficulties.Normal, 
		};
	}

	public void ReplaceUserId(Guid oldUserId, Guid newUserId)
	{
		if (players.TryGetValue(oldUserId, out var value))
		{
			players.Remove(oldUserId);
			players.Add(newUserId, value);
		}
	}
}
