using System;
using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class ImprovementState
{
	public ImprovementData.Type type;

	[Obsolete("ImprovementState.owner is obsolete, use the tiles owner instead")]
	public byte owner;

	public byte founder;

	public ushort level;

	public ushort founded;

	public short xp;

	public short population;

	public ushort production;

	public ushort baseScore;

	public ushort borderSize;

	public ushort upgrade;

	public byte connectedToCapitalOfPlayer;

	public string name;

	public List<CityReward> rewards;

	public List<ImprovementEffect> effects = new List<ImprovementEffect>();

	public bool HasEffect(ImprovementEffect effect)
	{
		return effects.Contains(effect);
	}

	public void AddEffect(ImprovementEffect effect)
	{
		if (!effects.Contains(effect))
		{
			effects.Add(effect);
		}
	}

	public void RemoveEffect(ImprovementEffect effect)
	{
		effects.Remove(effect);
	}

	public void AddPopulation(short populationIncrease)
	{
		population += populationIncrease;
		xp += populationIncrease;
	}

	public int GetAge(GameState state)
	{
		return (int)(state.CurrentTurn - founded);
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		if (version < 40)
		{
			SerializeV2(writer, version);
		}
		else
		{
			SerializeDefault(writer, version);
		}
	}

	private void SerializeDefault(BinaryWriter writer, int version)
	{
		writer.Write((ushort)type);
		writer.Write(level);
		writer.Write(founded);
		writer.Write(xp);
		writer.Write(population);
		writer.Write(production);
		writer.Write(baseScore);
		writer.Write(borderSize);
		writer.Write(upgrade);
		writer.Write(connectedToCapitalOfPlayer);
		bool flag = !string.IsNullOrEmpty(name);
		writer.Write(flag);
		if (flag)
		{
			writer.Write(name);
		}
		writer.Write(founder);
		writer.Write((ushort)((rewards != null) ? ((uint)rewards.Count) : 0u));
		if (rewards != null)
		{
			for (int i = 0; i < rewards.Count; i++)
			{
				writer.Write((ushort)rewards[i]);
			}
		}
		writer.Write((ushort)((effects != null) ? ((uint)effects.Count) : 0u));
		if (effects != null)
		{
			for (int j = 0; j < effects.Count; j++)
			{
				writer.Write((ushort)effects[j]);
			}
		}
	}

	private void SerializeV2(BinaryWriter writer, int version)
	{
		writer.Write((ushort)type);
		writer.Write(level);
		writer.Write(founded);
		writer.Write(xp);
		writer.Write(population);
		writer.Write(production);
		writer.Write(baseScore);
		writer.Write(borderSize);
		writer.Write(upgrade);
		writer.Write(connectedToCapitalOfPlayer);
		bool flag = !string.IsNullOrEmpty(name);
		writer.Write(flag);
		if (flag)
		{
			writer.Write(name);
		}
		writer.Write(founder);
		writer.Write((ushort)((rewards != null) ? ((uint)rewards.Count) : 0u));
		if (rewards != null)
		{
			for (int i = 0; i < rewards.Count; i++)
			{
				writer.Write((ushort)rewards[i]);
			}
		}
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		if (version < 40)
		{
			DeserializeV2(reader, version);
		}
		else
		{
			DeserializeDefault(reader, version);
		}
	}

	public void DeserializeDefault(BinaryReader reader, int version)
	{
		type = (ImprovementData.Type)reader.ReadUInt16();
		level = reader.ReadUInt16();
		founded = reader.ReadUInt16();
		xp = reader.ReadInt16();
		population = reader.ReadInt16();
		production = reader.ReadUInt16();
		baseScore = reader.ReadUInt16();
		borderSize = reader.ReadUInt16();
		upgrade = reader.ReadUInt16();
		connectedToCapitalOfPlayer = reader.ReadByte();
		if (reader.ReadBoolean())
		{
			name = reader.ReadString();
		}
		founder = reader.ReadByte();
		int num = reader.ReadUInt16();
		if (rewards == null || rewards.Count < num)
		{
			rewards = new List<CityReward>(num);
		}
		for (int i = 0; i < num; i++)
		{
			if (i < rewards.Count)
			{
				rewards[i] = (CityReward)reader.ReadUInt16();
			}
			else
			{
				rewards.Add((CityReward)reader.ReadUInt16());
			}
		}
		int num2 = reader.ReadUInt16();
		if (effects == null || effects.Count < num2)
		{
			effects = new List<ImprovementEffect>(num2);
		}
		for (int j = 0; j < num2; j++)
		{
			if (j < effects.Count)
			{
				effects[j] = (ImprovementEffect)reader.ReadUInt16();
			}
			else
			{
				effects.Add((ImprovementEffect)reader.ReadUInt16());
			}
		}
	}

	public void DeserializeV2(BinaryReader reader, int version)
	{
		type = (ImprovementData.Type)reader.ReadUInt16();
		level = reader.ReadUInt16();
		founded = reader.ReadUInt16();
		xp = reader.ReadInt16();
		population = reader.ReadInt16();
		production = reader.ReadUInt16();
		baseScore = reader.ReadUInt16();
		borderSize = reader.ReadUInt16();
		upgrade = reader.ReadUInt16();
		connectedToCapitalOfPlayer = reader.ReadByte();
		if (reader.ReadBoolean())
		{
			name = reader.ReadString();
		}
		founder = reader.ReadByte();
		int num = reader.ReadUInt16();
		if (rewards == null || rewards.Count < num)
		{
			rewards = new List<CityReward>(num);
		}
		for (int i = 0; i < num; i++)
		{
			if (i < rewards.Count)
			{
				rewards[i] = (CityReward)reader.ReadUInt16();
			}
			else
			{
				rewards.Add((CityReward)reader.ReadUInt16());
			}
		}
	}
}
