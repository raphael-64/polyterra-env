using System.IO;
using Polytopia.Data;

public class DisbandUnitAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public WorldCoordinates HomeCoordinates { get; protected set; }

	public DisbandUnitAction()
	{
	}

	public DisbandUnitAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 6)
		{
			ExecuteV6(state);
		}
		else if (state.Version < 18)
		{
			ExecuteV7(state);
		}
		else if (state.Version < 21)
		{
			ExecuteV18(state);
		}
		else if (state.Version < 41)
		{
			ExecuteV21(state);
		}
		else if (state.Version < 43)
		{
			ExecuteV43(state);
		}
		else if (state.Version <= 59)
		{
			ExecuteV59(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	private void ExecuteDefault(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.unit == null)
		{
			return;
		}
		HomeCoordinates = tile.unit.home;
		UnitState unitState = tile.unit;
		if (unitState.HasAbility(UnitAbility.Type.Carry, state))
		{
			unitState = tile.unit.passengerUnit;
		}
		if (unitState.HasFollower() && state.TryGetUnit(unitState.follower, out var unit))
		{
			state.ActionStack.Add(new DisbandUnitAction(base.PlayerId, unit.coordinates));
		}
		if (unitState.HasLeader() && state.TryGetUnit(unitState.leader, out var unit2))
		{
			unit2.follower = 0u;
		}
		if (state.GameLogicData.TryGetData(unitState.type, out var data))
		{
			int num = data.cost / 2;
			for (int i = 0; i < num; i++)
			{
				state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, 40));
			}
			state.ActionStack.Add(new DecreaseScoreAction(unitState.owner, (int)ScoreSheet.GetUnitScore(data)));
		}
		tile.unit = null;
	}

	private void ExecuteV59(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.unit == null)
		{
			return;
		}
		HomeCoordinates = tile.unit.home;
		UnitState unitState = tile.unit;
		if (unitState.HasAbility(UnitAbility.Type.Carry, state))
		{
			unitState = tile.unit.passengerUnit;
		}
		if (unitState.HasFollower() && state.TryGetUnit(unitState.follower, out var unit))
		{
			state.ActionStack.Add(new DisbandUnitAction(base.PlayerId, unit.coordinates));
		}
		if (unitState.HasLeader() && state.TryGetUnit(unitState.leader, out var unit2))
		{
			unit2.follower = 0u;
		}
		if (base.PlayerId == unitState.owner && state.GameLogicData.TryGetData(unitState.type, out var data))
		{
			int num = data.cost / 2;
			for (int i = 0; i < num; i++)
			{
				state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, 40));
			}
			state.ActionStack.Add(new DecreaseScoreAction(base.PlayerId, (int)ScoreSheet.GetUnitScore(data)));
		}
		tile.unit = null;
	}

	private void ExecuteV43(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.unit == null)
		{
			return;
		}
		HomeCoordinates = tile.unit.home;
		UnitState unitState = tile.unit;
		if (unitState.HasAbility(UnitAbility.Type.Carry, state))
		{
			unitState = tile.unit.passengerUnit;
		}
		if (unitState.HasFollower() && state.TryGetUnit(unitState.follower, out var unit))
		{
			state.ActionStack.Add(new DisbandUnitAction(base.PlayerId, unit.coordinates));
		}
		if (base.PlayerId == unitState.owner && state.GameLogicData.TryGetData(unitState.type, out var data))
		{
			int num = data.cost / 2;
			for (int i = 0; i < num; i++)
			{
				state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, 40));
			}
			state.ActionStack.Add(new DecreaseScoreAction(base.PlayerId, (int)ScoreSheet.GetUnitScore(data)));
		}
		tile.unit = null;
	}

	private void ExecuteV21(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.unit == null)
		{
			return;
		}
		HomeCoordinates = tile.unit.home;
		UnitState unitState = tile.unit;
		if (unitState.HasAbility(UnitAbility.Type.Carry, state))
		{
			unitState = tile.unit.passengerUnit;
		}
		if (base.PlayerId == unitState.owner && state.GameLogicData.TryGetData(unitState.type, out var data))
		{
			int num = data.cost / 2;
			for (int i = 0; i < num; i++)
			{
				state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, 40));
			}
			state.ActionStack.Add(new DecreaseScoreAction(base.PlayerId, (int)ScoreSheet.GetUnitScore(data)));
		}
		tile.unit = null;
	}

	private void ExecuteV18(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.unit == null)
		{
			return;
		}
		HomeCoordinates = tile.unit.home;
		UnitState unitState = tile.unit;
		if (unitState.HasAbility(UnitAbility.Type.Carry, state))
		{
			unitState = tile.unit.passengerUnit;
		}
		if (base.PlayerId == unitState.owner && state.GameLogicData.TryGetData(unitState.type, out var data))
		{
			int num = data.cost / 2;
			for (int i = 0; i < num; i++)
			{
				state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, 40));
			}
		}
		tile.unit = null;
	}

	private void ExecuteV7(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.unit == null)
		{
			return;
		}
		HomeCoordinates = tile.unit.home;
		UnitState unitState = tile.unit;
		if (unitState.HasAbility(UnitAbility.Type.Carry, state))
		{
			unitState = tile.unit.passengerUnit;
		}
		if (state.GameLogicData.TryGetData(unitState.type, out var data))
		{
			int num = data.cost / 2;
			for (int i = 0; i < num; i++)
			{
				state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, 40));
			}
		}
		tile.unit = null;
	}

	private void ExecuteV6(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.unit == null || !state.TryGetUnit(tile.unit.id, out var unit))
		{
			return;
		}
		HomeCoordinates = tile.unit.home;
		state.Map.GetTile(HomeCoordinates);
		tile.unit = null;
		if (state.GameLogicData.TryGetData(unit.type, out var data))
		{
			int num = data.cost / 2;
			for (int i = 0; i < num; i++)
			{
				state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, 40));
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.DisbandUnit;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
		HomeCoordinates.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
		HomeCoordinates = new WorldCoordinates(reader, version);
	}
}
