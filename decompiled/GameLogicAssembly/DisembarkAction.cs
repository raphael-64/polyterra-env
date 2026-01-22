using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class DisembarkAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public DisembarkAction()
	{
	}

	public DisembarkAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		if (state.Version >= 26)
		{
			ExecuteV26(state);
		}
		else
		{
			ExecuteV6(state);
		}
	}

	private void ExecuteV26(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		UnitState unit = tile.unit;
		if (tile.unit == null)
		{
			return;
		}
		UnitState passengerUnit = unit.passengerUnit;
		if (passengerUnit == null)
		{
			return;
		}
		passengerUnit.health = unit.health;
		passengerUnit.flipped = unit.flipped;
		passengerUnit.direction = unit.direction;
		passengerUnit.moved = true;
		passengerUnit.attacked = true;
		tile.SetUnit(passengerUnit);
		passengerUnit.coordinates = tile.coordinates;
		if (state.GameLogicData.TryGetData(passengerUnit.type, out var data) && passengerUnit.HasAbility(UnitAbility.Type.Grow, state))
		{
			state.TryGetPlayer(base.PlayerId, out var playerState);
			List<UnitData> unlockedUpgradesForUnit = state.GameLogicData.GetUnlockedUpgradesForUnit(playerState, state, data);
			if (unlockedUpgradesForUnit.Count > 0 && tile.unit.GetAge(state) >= 3)
			{
				state.ActionStack.Add(new UpgradeAction(base.PlayerId, unlockedUpgradesForUnit[0].type, tile.coordinates, 0));
			}
		}
	}

	private void ExecuteV6(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		UnitState unit = tile.unit;
		unit.attacked = true;
		UnitState passengerUnit = unit.passengerUnit;
		if (passengerUnit == null)
		{
			return;
		}
		if (passengerUnit.owner == unit.owner)
		{
			passengerUnit.health = unit.health;
		}
		passengerUnit.flipped = unit.flipped;
		passengerUnit.direction = unit.direction;
		tile.SetUnit(passengerUnit);
		passengerUnit.coordinates = tile.coordinates;
		if (state.GameLogicData.TryGetData(passengerUnit.type, out var data) && passengerUnit.HasAbility(UnitAbility.Type.Grow, state))
		{
			state.TryGetPlayer(base.PlayerId, out var playerState);
			List<UnitData> unlockedUpgradesForUnit = state.GameLogicData.GetUnlockedUpgradesForUnit(playerState, state, data);
			if (unlockedUpgradesForUnit.Count > 0 && tile.unit.GetAge(state) >= data.growthRate)
			{
				state.ActionStack.Add(new UpgradeAction(base.PlayerId, unlockedUpgradesForUnit[0].type, tile.coordinates, 0));
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Disembark;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordiantes: {Coordinates})";
	}
}
