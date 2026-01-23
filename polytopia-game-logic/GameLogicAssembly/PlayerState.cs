using System;
using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class PlayerState
{
	public const byte NO_PLAYER_ID = 0;

	public const byte NATURE_PLAYER_ID = byte.MaxValue;

	public const byte DISPLAY_USERNAME_LENGTH = 25;

	public WorldCoordinates startTile;

	public TribeData.Type tribe;

	public TribeData.Type tribeMix;

	public bool hasChosenTribe;

	public int handicap = 1;

	public int resignedTurn = -1;

	public int resignedAtCommandIndex = -1;

	public int wipedAtCommand = -1;

	public List<TechData.Type> availableTech = new List<TechData.Type>();

	public List<TaskBase> tasks = new List<TaskBase>();

	public Dictionary<byte, int> aggressions = new Dictionary<byte, int>();

	public List<byte> knownPlayers = new List<byte>();

	public List<ImprovementData.Type> builtUniqueImprovements = new List<ImprovementData.Type>();

	public Dictionary<byte, DiplomacyRelation> relations = new Dictionary<byte, DiplomacyRelation>();

	public List<DiplomacyMessage> messages = new List<DiplomacyMessage>();

	public SkinType skinType;

	private int currency;

	public uint score;

	public uint endScore;

	public int cities;

	public uint kills;

	public uint casualities;

	public uint wipeOuts;

	public byte killerId;

	public uint killedTurn;

	public int colorOverride = -1;

	public AIState aiState;

	public List<TechData> unlockedTechCache = new List<TechData>(10);

	public bool blockTrainUnits;

	public OpinionManager opinions = new OpinionManager();

	public byte Id { get; set; }

	public string UserName { get; set; }

	public string DisplayUserName
	{
		get
		{
			int length = Math.Min(25, UserName.Length);
			return UserName.Substring(0, length);
		}
	}

	public Guid? AccountId { get; set; }

	public bool AutoPlay { get; set; }

	public int Currency
	{
		get
		{
			return currency;
		}
		set
		{
			if (value < 0)
			{
				Log.Error($"Tried to set a negative currency for ID #{Id}, {UserName}({tribe})!", Array.Empty<object>());
			}
			currency = Math.Max(value, 0);
		}
	}

	public bool haveColorOverride => colorOverride >= 0;

	public int GetStupidity()
	{
		if (AutoPlay)
		{
			return Math.Max(0, 2 - handicap);
		}
		return 0;
	}

	public static bool AreDifferentPlayers(byte id, byte otherId)
	{
		if (id != otherId && id != 0)
		{
			return otherId != 0;
		}
		return false;
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		writer.Write(Id);
		writer.Write(UserName);
		writer.Write(AccountId.ToString());
		writer.Write(AutoPlay);
		startTile.Serialize(writer, version);
		writer.Write((ushort)tribe);
		writer.Write(hasChosenTribe);
		writer.Write(handicap);
		writer.Write((ushort)((aggressions != null) ? ((uint)aggressions.Count) : 0u));
		foreach (KeyValuePair<byte, int> aggression in aggressions)
		{
			writer.Write(aggression.Key);
			writer.Write(aggression.Value);
		}
		writer.Write(currency);
		writer.Write(score);
		writer.Write(endScore);
		writer.Write((ushort)cities);
		writer.Write((ushort)((availableTech != null) ? ((uint)availableTech.Count) : 0u));
		if (availableTech != null)
		{
			for (int i = 0; i < availableTech.Count; i++)
			{
				writer.Write((ushort)availableTech[i]);
			}
		}
		writer.Write((ushort)((knownPlayers != null) ? ((uint)knownPlayers.Count) : 0u));
		if (knownPlayers != null)
		{
			for (int j = 0; j < knownPlayers.Count; j++)
			{
				writer.Write(knownPlayers[j]);
			}
		}
		ushort num = (ushort)((tasks != null) ? ((uint)tasks.Count) : 0u);
		writer.Write(num);
		for (int k = 0; k < num; k++)
		{
			TaskBase.SerializeTask(tasks[k], writer, version);
		}
		writer.Write(kills);
		writer.Write(casualities);
		writer.Write(wipeOuts);
		writer.Write(colorOverride);
		writer.Write((byte)tribeMix);
		if (version < 21)
		{
			return;
		}
		writer.Write((ushort)((builtUniqueImprovements != null) ? ((uint)builtUniqueImprovements.Count) : 0u));
		if (builtUniqueImprovements != null)
		{
			for (int l = 0; l < builtUniqueImprovements.Count; l++)
			{
				writer.Write((short)builtUniqueImprovements[l]);
			}
		}
		if (version < 60)
		{
			return;
		}
		writer.Write((ushort)relations.Count);
		foreach (KeyValuePair<byte, DiplomacyRelation> relation in relations)
		{
			writer.Write(relation.Key);
			relation.Value.Serialize(writer, version);
		}
		writer.Write((ushort)messages.Count);
		foreach (DiplomacyMessage message in messages)
		{
			message.Serialize(writer, version);
		}
		writer.Write(killerId);
		writer.Write(killedTurn);
		if (version < 70)
		{
			return;
		}
		writer.Write(resignedAtCommandIndex);
		writer.Write(wipedAtCommand);
		if (version >= 86)
		{
			writer.Write((ushort)skinType);
			if (version >= 93)
			{
				writer.Write(resignedTurn);
			}
		}
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		Id = reader.ReadByte();
		UserName = reader.ReadString();
		AccountId = Guid.Parse(reader.ReadString());
		AutoPlay = reader.ReadBoolean();
		startTile = new WorldCoordinates(reader, version);
		tribe = (TribeData.Type)reader.ReadUInt16();
		hasChosenTribe = reader.ReadBoolean();
		handicap = reader.ReadInt32();
		int num = reader.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			byte key = reader.ReadByte();
			aggressions[key] = reader.ReadInt32();
		}
		currency = reader.ReadInt32();
		score = reader.ReadUInt32();
		endScore = reader.ReadUInt32();
		cities = reader.ReadUInt16();
		int num2 = reader.ReadUInt16();
		if (availableTech == null || availableTech.Count < num2)
		{
			availableTech = new List<TechData.Type>(num2);
		}
		for (int j = 0; j < num2; j++)
		{
			if (j < availableTech.Count)
			{
				availableTech[j] = (TechData.Type)reader.ReadUInt16();
			}
			else
			{
				availableTech.Add((TechData.Type)reader.ReadUInt16());
			}
		}
		int num3 = reader.ReadUInt16();
		if (knownPlayers == null || knownPlayers.Count < num3)
		{
			knownPlayers = new List<byte>();
		}
		for (int k = 0; k < num3; k++)
		{
			if (k < knownPlayers.Count)
			{
				knownPlayers[k] = reader.ReadByte();
			}
			else
			{
				knownPlayers.Add(reader.ReadByte());
			}
		}
		ushort num4 = reader.ReadUInt16();
		if (tasks == null)
		{
			tasks = new List<TaskBase>(num4);
		}
		else
		{
			tasks.Clear();
		}
		for (int l = 0; l < num4; l++)
		{
			tasks.Add(TaskBase.DeserializeTask(reader, version));
		}
		kills = reader.ReadUInt32();
		casualities = reader.ReadUInt32();
		wipeOuts = reader.ReadUInt32();
		colorOverride = reader.ReadInt32();
		tribeMix = (TribeData.Type)reader.ReadByte();
		if (version < 21)
		{
			return;
		}
		ushort num5 = reader.ReadUInt16();
		if (builtUniqueImprovements == null)
		{
			builtUniqueImprovements = new List<ImprovementData.Type>(num5);
		}
		else
		{
			builtUniqueImprovements.Clear();
		}
		for (int m = 0; m < num5; m++)
		{
			builtUniqueImprovements.Add((ImprovementData.Type)reader.ReadInt16());
		}
		if (version < 60)
		{
			return;
		}
		relations.Clear();
		ushort num6 = reader.ReadUInt16();
		for (int n = 0; n < num6; n++)
		{
			byte key2 = reader.ReadByte();
			DiplomacyRelation diplomacyRelation = new DiplomacyRelation();
			diplomacyRelation.Deserialize(reader, version);
			relations[key2] = diplomacyRelation;
		}
		messages.Clear();
		ushort num7 = reader.ReadUInt16();
		for (int num8 = 0; num8 < num7; num8++)
		{
			DiplomacyMessage diplomacyMessage = new DiplomacyMessage();
			diplomacyMessage.Deserialize(reader, version);
			messages.Add(diplomacyMessage);
		}
		killerId = reader.ReadByte();
		killedTurn = reader.ReadUInt32();
		if (version < 70)
		{
			return;
		}
		resignedAtCommandIndex = reader.ReadInt32();
		wipedAtCommand = reader.ReadInt32();
		if (version >= 86)
		{
			skinType = (SkinType)reader.ReadUInt16();
			if (version >= 93)
			{
				resignedTurn = reader.ReadInt32();
			}
		}
	}

	public override string ToString()
	{
		return string.Format("Player {0}:\t{1} ({2}{3}). {4}", Id, UserName, tribe, (tribeMix == TribeData.Type.None) ? "" : $" & {tribeMix}", AutoPlay ? $"AI (Difficulty: {handicap})" : $"Human ({AccountId})");
	}
}
