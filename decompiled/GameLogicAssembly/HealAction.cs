using System;
using System.IO;

public class HealAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public ushort FixedHealAmount { get; protected set; }

	public ushort HealAmount { get; protected set; }

	public HealAction()
	{
	}

	public HealAction(byte playerId, WorldCoordinates coordinates, ushort fixedHealAmount = 0)
		: base(playerId)
	{
		Coordinates = coordinates;
		FixedHealAmount = fixedHealAmount;
	}

	public override void Execute(GameState state)
	{
		if (state.Version < 50)
		{
			ExecuteV50(state);
		}
		else if (state.Version <= 81)
		{
			ExecuteV81(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	public void ExecuteDefault(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile == null)
		{
			return;
		}
		UnitState unit = tile.unit;
		if (unit != null)
		{
			if (unit.HasEffect(UnitEffect.Poisoned))
			{
				unit.RemoveEffect(UnitEffect.Poisoned);
				HealAmount = 0;
				return;
			}
			ushort val = (ushort)unit.GetMaxHealth(state);
			HealAmount = ((FixedHealAmount > 0) ? FixedHealAmount : ActionUtils.GetHealAmount(state, tile));
			ushort val2 = (ushort)(unit.health + HealAmount);
			unit.health = Math.Min(val2, val);
		}
	}

	public void ExecuteV81(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null)
		{
			if (tile.unit.HasEffect(UnitEffect.Poisoned))
			{
				tile.unit.RemoveEffect(UnitEffect.Poisoned);
				HealAmount = 0;
			}
			else
			{
				HealAmount = ((FixedHealAmount > 0) ? FixedHealAmount : ActionUtils.GetHealAmount(state, tile));
				tile.unit.health += HealAmount;
			}
		}
	}

	public void ExecuteV50(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null)
		{
			state.TryGetPlayer(tile.unit.owner, out var playerState);
			ushort num = (ushort)((tile.owner == tile.unit.owner || (playerState != null && playerState.HasPeaceWith(tile.owner))) ? 40u : 20u);
			if (tile.unit.HasEffect(UnitEffect.Poisoned))
			{
				num /= 2;
			}
			ushort val = (ushort)(tile.unit.GetMaxHealth(state) - tile.unit.health);
			HealAmount = Math.Min((FixedHealAmount > 0) ? FixedHealAmount : num, val);
			if (FixedHealAmount > 0)
			{
				tile.unit.RemoveEffect(UnitEffect.Poisoned);
			}
			tile.unit.health += HealAmount;
			if (tile.unit.health >= tile.unit.GetMaxHealth(state))
			{
				tile.unit.RemoveEffect(UnitEffect.Poisoned);
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Heal;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
		writer.Write(FixedHealAmount);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
		FixedHealAmount = reader.ReadUInt16();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Target: {Coordinates}, MinHealAmount {FixedHealAmount})";
	}
}
