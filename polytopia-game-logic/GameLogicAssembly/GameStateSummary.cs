using System;
using System.Collections.Generic;
using System.IO;
using Polytopia.Data;
using PolytopiaBackendBase.Game;

public class GameStateSummary : IBinarySerializable
{
	public class GamePlayerSummary : IBinarySerializable
	{
		public byte Id { get; set; }

		public Guid? PolytopiaId { get; set; }

		public string UserName { get; set; }

		public TribeData.Type TribeType { get; set; }

		public SkinType SkinType { get; set; }

		public bool AutoPlay { get; set; }

		public bool IsDead { get; set; }

		public bool HasChosenTribe { get; set; }

		public int Handicap { get; set; }

		public void Serialize(BinaryWriter writer, int version)
		{
			using MemoryStream memoryStream = new MemoryStream();
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(Id);
			binaryWriter.Write(PolytopiaId.ToString());
			binaryWriter.Write(UserName ?? "");
			binaryWriter.Write((byte)TribeType);
			binaryWriter.Write(AutoPlay);
			binaryWriter.Write(HasChosenTribe);
			binaryWriter.Write(Handicap);
			if (version >= 20)
			{
				binaryWriter.Write(IsDead);
			}
			if (version >= 86)
			{
				binaryWriter.Write((int)SkinType);
			}
			writer.Write((int)memoryStream.Length);
			memoryStream.WriteTo(writer.BaseStream);
		}

		public void Deserialize(BinaryReader reader, int version)
		{
			int num = reader.ReadInt32();
			long position = reader.BaseStream.Position;
			Id = reader.ReadByte();
			string g = reader.ReadString();
			PolytopiaId = new Guid(g);
			UserName = reader.ReadString();
			TribeType = (TribeData.Type)reader.ReadByte();
			AutoPlay = reader.ReadBoolean();
			HasChosenTribe = reader.ReadBoolean();
			Handicap = reader.ReadInt32();
			if (version >= 20)
			{
				IsDead = reader.ReadBoolean();
			}
			if (version >= 86)
			{
				SkinType = (SkinType)reader.ReadInt32();
			}
			reader.BaseStream.Position = position + num;
		}
	}

	public string GameName { get; set; }

	public uint CurrentTurn { get; set; }

	public byte CurrentPlayer { get; set; }

	public ushort LastProcessedCommand { get; set; }

	public ushort MapWidth { get; set; }

	public ushort MapHeight { get; set; }

	public MapPreset MapPreset { get; set; }

	public GameMode GameMode { get; set; }

	public GameType GameType { get; set; }

	public GameRules Rules { get; set; }

	public bool IsAutoSkipEnabled { get; set; }

	public List<GamePlayerSummary> PlayerSummaries { get; set; }

	public void Serialize(BinaryWriter writer, int version)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(GameName ?? "");
		binaryWriter.Write(CurrentTurn);
		binaryWriter.Write(CurrentPlayer);
		binaryWriter.Write(LastProcessedCommand);
		binaryWriter.Write(MapWidth);
		binaryWriter.Write(MapHeight);
		binaryWriter.Write((byte)GameMode);
		binaryWriter.Write((byte)GameType);
		ushort num = (ushort)(PlayerSummaries?.Count ?? 0);
		binaryWriter.Write(num);
		for (int i = 0; i < num; i++)
		{
			PlayerSummaries[i].Serialize(binaryWriter, version);
		}
		if (version >= 11)
		{
			Rules.Serialize(binaryWriter, version);
		}
		if (version >= 24)
		{
			binaryWriter.Write((byte)MapPreset);
		}
		if (version >= 70)
		{
			binaryWriter.Write(IsAutoSkipEnabled);
		}
		writer.Write((int)memoryStream.Length);
		memoryStream.WriteTo(writer.BaseStream);
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		int num = reader.ReadInt32();
		long position = reader.BaseStream.Position;
		GameName = reader.ReadString();
		CurrentTurn = reader.ReadUInt32();
		CurrentPlayer = reader.ReadByte();
		LastProcessedCommand = reader.ReadUInt16();
		MapWidth = reader.ReadUInt16();
		MapHeight = reader.ReadUInt16();
		GameMode = (GameMode)reader.ReadByte();
		GameType = (GameType)reader.ReadByte();
		ushort num2 = reader.ReadUInt16();
		PlayerSummaries = new List<GamePlayerSummary>();
		for (int i = 0; i < num2; i++)
		{
			GamePlayerSummary gamePlayerSummary = new GamePlayerSummary();
			gamePlayerSummary.Deserialize(reader, version);
			PlayerSummaries.Add(gamePlayerSummary);
		}
		if (version >= 11)
		{
			Rules = new GameRules();
			Rules.Deserialize(reader, version);
		}
		if (version >= 24)
		{
			MapPreset = (MapPreset)reader.ReadByte();
		}
		if (version >= 70)
		{
			IsAutoSkipEnabled = reader.ReadBoolean();
		}
		reader.BaseStream.Position = position + num;
	}

	public static bool FromByteArray(byte[] data, out GameStateSummary summary)
	{
		summary = null;
		return false;
	}

	public static bool FromGameStateByteArray(byte[] stateData, out GameStateSummary summary, out GameState gameState)
	{
		if (!SerializationHelpers.FromByteArray<GameState>(stateData, out gameState, out var version))
		{
			summary = null;
			return false;
		}
		summary = new GameStateSummary
		{
			GameName = (gameState.Settings?.GameName ?? "No Name"),
			CurrentPlayer = gameState.CurrentPlayer,
			MapWidth = (ushort)(gameState.Settings?.MapSize ?? 0),
			MapHeight = (ushort)(gameState.Settings?.MapSize ?? 0),
			MapPreset = (gameState.Settings?.mapPreset ?? MapPreset.None),
			GameMode = (gameState.Settings?.BaseGameMode ?? GameMode.Custom),
			GameType = (gameState.Settings?.GameType ?? GameType.Multiplayer),
			CurrentTurn = gameState.CurrentTurn,
			LastProcessedCommand = gameState.LastProcessedCommand
		};
		summary.PlayerSummaries = new List<GamePlayerSummary>(gameState.PlayerStates.Count);
		foreach (PlayerState playerState in gameState.PlayerStates)
		{
			GamePlayerSummary gamePlayerSummary = new GamePlayerSummary
			{
				Id = playerState.Id,
				PolytopiaId = playerState.AccountId,
				UserName = playerState.UserName,
				TribeType = playerState.tribe,
				SkinType = playerState.skinType,
				AutoPlay = playerState.AutoPlay,
				HasChosenTribe = playerState.hasChosenTribe,
				Handicap = playerState.handicap
			};
			if (gameState.Version >= 20)
			{
				gamePlayerSummary.IsDead = gameState.CurrentState != GameState.State.Unknown && gameState.CurrentState != GameState.State.Lobby && !playerState.IsAlive(gameState);
			}
			summary.PlayerSummaries.Add(gamePlayerSummary);
		}
		if (version >= 11)
		{
			summary.Rules = gameState.Settings?.rules ?? new GameRules(summary.GameMode);
		}
		if (version >= 70)
		{
			summary.IsAutoSkipEnabled = gameState.Settings?.IsAutoSkipEnabled ?? false;
		}
		return true;
	}

	public static byte[] FromGameStateByteArray(byte[] stateData)
	{
		if (!FromGameStateByteArray(stateData, out var summary, out var gameState))
		{
			return null;
		}
		return SerializationHelpers.ToByteArray(summary, gameState.Version);
	}
}
