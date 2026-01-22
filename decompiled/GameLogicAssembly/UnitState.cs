using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class UnitState
{
	public uint id;

	public uint leader;

	public uint follower;

	public byte owner;

	public short style = -1;

	public SkinType skinType;

	public UnitData.Type type;

	public WorldCoordinates previousTurnEndCoordinates;

	public WorldCoordinates coordinates;

	public WorldCoordinates home;

	public UnitState passengerUnit;

	public ushort health = 10;

	public ushort promotionLevel;

	public ushort xp;

	public bool moved;

	public bool attacked;

	public GridDirection direction = GridDirection.S;

	public bool flipped;

	public ushort createdTurn;

	public List<UnitEffect> effects = new List<UnitEffect>();

	public bool HasEffect(UnitEffect effect)
	{
		return effects.Contains(effect);
	}

	public void AddEffect(UnitEffect effect)
	{
		if (!effects.Contains(effect))
		{
			effects.Add(effect);
		}
	}

	public void RemoveEffect(UnitEffect effect)
	{
		effects.Remove(effect);
	}

	public void MakeExhauseted(GameState gameState)
	{
		moved = true;
		attacked = true;
		RemoveEffect(UnitEffect.Boosted);
		DisableFollowers(gameState);
	}

	public void DisableFollowers(GameState state)
	{
		if (HasFollower() && state.TryGetUnit(follower, out var unit))
		{
			unit.attacked = true;
			unit.moved = true;
			unit.DisableFollowers(state);
		}
	}

	public bool CanAttack()
	{
		if (!attacked)
		{
			return !HasEffect(UnitEffect.Frozen);
		}
		return false;
	}

	public bool CanMove()
	{
		if (!moved)
		{
			return !HasEffect(UnitEffect.Frozen);
		}
		return false;
	}

	public bool IsFreezable(GameState state, PlayerState player)
	{
		if (IsFriendly(state, player))
		{
			return false;
		}
		if (HasEffect(UnitEffect.Frozen))
		{
			return false;
		}
		return true;
	}

	public bool IsInvisible(GameState gameState, byte playerId)
	{
		gameState.TryGetPlayer(playerId, out var playerState);
		if (HasEffect(UnitEffect.Invisible) && owner != playerId && !playerState.HasPeaceWith(owner))
		{
			return true;
		}
		return false;
	}

	public bool IsFriendly(GameState state, PlayerState player)
	{
		if (owner != player.Id)
		{
			return player.HasPeaceWith(owner);
		}
		return true;
	}

	public bool HasFollower()
	{
		return follower != 0;
	}

	public bool HasLeader()
	{
		return leader != 0;
	}

	public GridDirection FollowerDirection(GameState state)
	{
		if (state.TryGetUnit(follower, out var unit))
		{
			GridDirection? gridDirection = WorldCoordinates.GetDirection(coordinates, unit.coordinates);
			if (gridDirection.HasValue)
			{
				return gridDirection.Value;
			}
		}
		return GridDirection.SW;
	}

	public GridDirection LeaderDirection(GameState state)
	{
		if (state.TryGetUnit(leader, out var unit))
		{
			GridDirection? gridDirection = WorldCoordinates.GetDirection(coordinates, unit.coordinates);
			if (gridDirection.HasValue)
			{
				return gridDirection.Value;
			}
		}
		return GridDirection.SW;
	}

	public static UnitState Create(GameState gameState, byte playerId, ushort currentTurn, UnitData unitData, WorldCoordinates coordinates, WorldCoordinates home)
	{
		WorldCoordinates distanceVector = new WorldCoordinates(gameState.Map.Width / 2, gameState.Map.Height / 2) - coordinates;
		if (unitData.HasAbility(UnitAbility.Type.Independent))
		{
			home = WorldCoordinates.NULL_COORDINATES;
		}
		return new UnitState
		{
			type = unitData.type,
			owner = playerId,
			coordinates = coordinates,
			home = home,
			health = (ushort)unitData.health,
			xp = 0,
			createdTurn = currentTurn,
			direction = WorldCoordinates.GetDirectionFromDistanceVector(distanceVector)
		};
	}

	public bool IsDetectingHiddenUnits(GameState gameState)
	{
		List<TileData> tileNeighbors = gameState.Map.GetTileNeighbors(coordinates);
		for (int i = 0; i < tileNeighbors.Count; i++)
		{
			TileData tileData = tileNeighbors[i];
			if (tileData.unit != null && tileData.unit.IsInvisible(gameState, owner))
			{
				return true;
			}
		}
		return false;
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		if (version < 7)
		{
			Serialize6(writer, version);
		}
		else if (version < 9)
		{
			Serialize7(writer, version);
		}
		else if (version < 40)
		{
			Serialize9(writer, version);
		}
		else
		{
			SerializeDefault(writer, version);
		}
	}

	public void Serialize6(BinaryWriter writer, int version)
	{
		writer.Write(id);
		writer.Write(owner);
		writer.Write((ushort)type);
		coordinates.Serialize(writer, version);
		home.Serialize(writer, version);
		writer.Write(health);
		writer.Write(promotionLevel);
		writer.Write(xp);
		writer.Write(moved);
		writer.Write(attacked);
		writer.Write(flipped);
		writer.Write(createdTurn);
		bool flag = passengerUnit != null;
		writer.Write(flag);
		if (flag)
		{
			passengerUnit.Serialize(writer, version);
		}
	}

	public void Serialize7(BinaryWriter writer, int version)
	{
		writer.Write(id);
		writer.Write(owner);
		writer.Write((ushort)type);
		coordinates.Serialize(writer, version);
		home.Serialize(writer, version);
		writer.Write(health);
		writer.Write(promotionLevel);
		writer.Write(xp);
		writer.Write(moved);
		writer.Write(attacked);
		writer.Write(flipped);
		writer.Write(createdTurn);
		bool flag = passengerUnit != null;
		writer.Write(flag);
		if (flag)
		{
			passengerUnit.Serialize(writer, version);
		}
		writer.Write((ushort)((effects != null) ? ((uint)effects.Count) : 0u));
		if (effects != null)
		{
			for (int i = 0; i < effects.Count; i++)
			{
				writer.Write((ushort)effects[i]);
			}
		}
	}

	public void Serialize9(BinaryWriter writer, int version)
	{
		writer.Write(id);
		writer.Write(owner);
		writer.Write((ushort)type);
		coordinates.Serialize(writer, version);
		home.Serialize(writer, version);
		writer.Write(health);
		writer.Write(promotionLevel);
		writer.Write(xp);
		writer.Write(moved);
		writer.Write(attacked);
		writer.Write(flipped);
		writer.Write(createdTurn);
		bool flag = passengerUnit != null;
		writer.Write(flag);
		if (flag)
		{
			passengerUnit.Serialize(writer, version);
		}
		writer.Write((ushort)((effects != null) ? ((uint)effects.Count) : 0u));
		if (effects != null)
		{
			for (int i = 0; i < effects.Count; i++)
			{
				writer.Write((ushort)effects[i]);
			}
		}
		writer.Write(style);
		writer.Write((byte)direction);
	}

	public void SerializeDefault(BinaryWriter writer, int version)
	{
		writer.Write(id);
		writer.Write(owner);
		writer.Write((ushort)type);
		writer.Write(follower);
		writer.Write(leader);
		coordinates.Serialize(writer, version);
		home.Serialize(writer, version);
		writer.Write(health);
		writer.Write(promotionLevel);
		writer.Write(xp);
		writer.Write(moved);
		writer.Write(attacked);
		writer.Write(flipped);
		writer.Write(createdTurn);
		bool flag = passengerUnit != null;
		writer.Write(flag);
		if (flag)
		{
			passengerUnit.Serialize(writer, version);
		}
		writer.Write((ushort)((effects != null) ? ((uint)effects.Count) : 0u));
		if (effects != null)
		{
			for (int i = 0; i < effects.Count; i++)
			{
				writer.Write((ushort)effects[i]);
			}
		}
		writer.Write(style);
		writer.Write((byte)direction);
		if (version >= 87)
		{
			writer.Write((ushort)skinType);
		}
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		if (version < 7)
		{
			Deserialize6(reader, version);
		}
		else if (version < 9)
		{
			Deserialize7(reader, version);
		}
		else if (version < 40)
		{
			Deserialize9(reader, version);
		}
		else
		{
			DeserializeDefault(reader, version);
		}
	}

	public void Deserialize6(BinaryReader reader, int version)
	{
		id = reader.ReadUInt32();
		owner = reader.ReadByte();
		type = (UnitData.Type)reader.ReadUInt16();
		coordinates = new WorldCoordinates(reader, version);
		home = new WorldCoordinates(reader, version);
		health = reader.ReadUInt16();
		promotionLevel = reader.ReadUInt16();
		xp = reader.ReadUInt16();
		moved = reader.ReadBoolean();
		attacked = reader.ReadBoolean();
		flipped = reader.ReadBoolean();
		createdTurn = reader.ReadUInt16();
		if (reader.ReadBoolean())
		{
			passengerUnit = new UnitState();
			passengerUnit.Deserialize(reader, version);
		}
	}

	public void Deserialize7(BinaryReader reader, int version)
	{
		id = reader.ReadUInt32();
		owner = reader.ReadByte();
		type = (UnitData.Type)reader.ReadUInt16();
		coordinates = new WorldCoordinates(reader, version);
		home = new WorldCoordinates(reader, version);
		health = reader.ReadUInt16();
		promotionLevel = reader.ReadUInt16();
		xp = reader.ReadUInt16();
		moved = reader.ReadBoolean();
		attacked = reader.ReadBoolean();
		flipped = reader.ReadBoolean();
		createdTurn = reader.ReadUInt16();
		if (reader.ReadBoolean())
		{
			passengerUnit = new UnitState();
			passengerUnit.Deserialize(reader, version);
		}
		int num = reader.ReadUInt16();
		if (effects == null || effects.Count < num)
		{
			effects = new List<UnitEffect>(num);
		}
		for (int i = 0; i < num; i++)
		{
			if (i < effects.Count)
			{
				effects[i] = (UnitEffect)reader.ReadUInt16();
			}
			else
			{
				effects.Add((UnitEffect)reader.ReadUInt16());
			}
		}
	}

	public void Deserialize9(BinaryReader reader, int version)
	{
		id = reader.ReadUInt32();
		owner = reader.ReadByte();
		type = (UnitData.Type)reader.ReadUInt16();
		coordinates = new WorldCoordinates(reader, version);
		home = new WorldCoordinates(reader, version);
		health = reader.ReadUInt16();
		promotionLevel = reader.ReadUInt16();
		xp = reader.ReadUInt16();
		moved = reader.ReadBoolean();
		attacked = reader.ReadBoolean();
		flipped = reader.ReadBoolean();
		createdTurn = reader.ReadUInt16();
		if (reader.ReadBoolean())
		{
			passengerUnit = new UnitState();
			passengerUnit.Deserialize(reader, version);
		}
		int num = reader.ReadUInt16();
		if (effects == null || effects.Count < num)
		{
			effects = new List<UnitEffect>(num);
		}
		for (int i = 0; i < num; i++)
		{
			if (i < effects.Count)
			{
				effects[i] = (UnitEffect)reader.ReadUInt16();
			}
			else
			{
				effects.Add((UnitEffect)reader.ReadUInt16());
			}
		}
		style = reader.ReadInt16();
		direction = (GridDirection)reader.ReadByte();
	}

	public void DeserializeDefault(BinaryReader reader, int version)
	{
		id = reader.ReadUInt32();
		owner = reader.ReadByte();
		type = (UnitData.Type)reader.ReadUInt16();
		follower = reader.ReadUInt32();
		leader = reader.ReadUInt32();
		coordinates = new WorldCoordinates(reader, version);
		home = new WorldCoordinates(reader, version);
		health = reader.ReadUInt16();
		promotionLevel = reader.ReadUInt16();
		xp = reader.ReadUInt16();
		moved = reader.ReadBoolean();
		attacked = reader.ReadBoolean();
		flipped = reader.ReadBoolean();
		createdTurn = reader.ReadUInt16();
		if (reader.ReadBoolean())
		{
			passengerUnit = new UnitState();
			passengerUnit.Deserialize(reader, version);
		}
		int num = reader.ReadUInt16();
		if (effects == null || effects.Count < num)
		{
			effects = new List<UnitEffect>(num);
		}
		for (int i = 0; i < num; i++)
		{
			if (i < effects.Count)
			{
				effects[i] = (UnitEffect)reader.ReadUInt16();
			}
			else
			{
				effects.Add((UnitEffect)reader.ReadUInt16());
			}
		}
		style = reader.ReadInt16();
		direction = (GridDirection)reader.ReadByte();
		if (version >= 87)
		{
			skinType = (SkinType)reader.ReadUInt16();
		}
	}

	public override string ToString()
	{
		return $"Unit (Id: {id}, Owner: {owner}, Type: {type}, Coordinates: {coordinates})";
	}
}
