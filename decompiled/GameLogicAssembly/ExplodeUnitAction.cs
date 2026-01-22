using System.IO;
using Polytopia.Data;

public class ExplodeUnitAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public WorldCoordinates HomeCoordinates { get; protected set; }

	public ExplodeUnitAction()
	{
	}

	public ExplodeUnitAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 43)
		{
			ExecuteV43(state);
		}
		else if (state.Version < 83)
		{
			ExecuteV82(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	public void ExecuteDefault(GameState state)
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
			state.ActionStack.Add(new ExplodeUnitAction(base.PlayerId, unit.coordinates));
		}
		if (unitState.HasLeader() && state.TryGetUnit(unitState.leader, out var unit2))
		{
			unit2.follower = 0u;
		}
		TileData[] areaSorted = state.Map.GetAreaSorted(Coordinates, 1, allowDiagonal: true);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		TileData[] array = areaSorted;
		foreach (TileData tileData in array)
		{
			if (tileData.unit != null && !tileData.unit.IsFriendly(state, playerState))
			{
				int num = BattleHelpers.GetBattleResults(state, unitState, tileData.unit).attackDamage / 2;
				if (num < tileData.unit.health)
				{
					state.ActionStack.Add(new PoisonUnitAction(base.PlayerId, tileData.coordinates, tileData.coordinates));
				}
				state.ActionStack.Add(new AttackAction(base.PlayerId, tileData.coordinates, tileData.coordinates, num, shouldMoveToTarget: false, AttackAction.AnimationType.Splash, 100));
				state.TryGetPlayer(tileData.unit.owner, out var playerState2);
				PlayerExtensions.ReactToAttack(playerState, playerState2, tileData, state);
				if (state.Version >= 41 && state.TryGetTask(playerState, TaskData.Type.Pacifist, out var task))
				{
					task.Reset();
				}
			}
		}
		if (base.PlayerId == unitState.owner && state.GameLogicData.TryGetData(unitState.type, out var data))
		{
			state.ActionStack.Add(new DecreaseScoreAction(base.PlayerId, (int)ScoreSheet.GetUnitScore(data)));
		}
		ActionUtils.SpawnPoisonResource(state, tile);
		tile.unit = null;
	}

	public void ExecuteV43(GameState state)
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
			state.ActionStack.Add(new ExplodeUnitAction(base.PlayerId, unit.coordinates));
		}
		TileData[] areaSorted = state.Map.GetAreaSorted(Coordinates, 1, allowDiagonal: true);
		foreach (TileData tileData in areaSorted)
		{
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId)
			{
				BattleResults battleResults = BattleHelpers.GetBattleResults(state, unitState, tileData.unit);
				if (battleResults.attackDamage < tileData.unit.health)
				{
					state.ActionStack.Add(new PoisonUnitAction(base.PlayerId, tileData.coordinates, tileData.coordinates));
				}
				state.ActionStack.Add(new AttackAction(base.PlayerId, tileData.coordinates, tileData.coordinates, battleResults.attackDamage, shouldMoveToTarget: false, AttackAction.AnimationType.Splash, 100));
				state.TryGetPlayer(base.PlayerId, out var playerState);
				state.TryGetPlayer(tileData.unit.owner, out var playerState2);
				PlayerExtensions.ReactToAttack(playerState, playerState2, tileData, state);
				if (state.Version >= 41 && state.TryGetTask(playerState, TaskData.Type.Pacifist, out var task))
				{
					task.Reset();
				}
			}
		}
		if (base.PlayerId == unitState.owner && state.GameLogicData.TryGetData(unitState.type, out var data))
		{
			state.ActionStack.Add(new DecreaseScoreAction(base.PlayerId, (int)ScoreSheet.GetUnitScore(data)));
		}
		ActionUtils.SpawnPoisonResource(state, tile);
		tile.unit = null;
	}

	public void ExecuteV82(GameState state)
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
			state.ActionStack.Add(new ExplodeUnitAction(base.PlayerId, unit.coordinates));
		}
		if (unitState.HasLeader() && state.TryGetUnit(unitState.leader, out var unit2))
		{
			unit2.follower = 0u;
		}
		TileData[] areaSorted = state.Map.GetAreaSorted(Coordinates, 1, allowDiagonal: true);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		TileData[] array = areaSorted;
		foreach (TileData tileData in array)
		{
			if (tileData.unit != null && !tileData.unit.IsFriendly(state, playerState))
			{
				BattleResults battleResults = BattleHelpers.GetBattleResults(state, unitState, tileData.unit);
				if (battleResults.attackDamage < tileData.unit.health)
				{
					state.ActionStack.Add(new PoisonUnitAction(base.PlayerId, tileData.coordinates, tileData.coordinates));
				}
				state.ActionStack.Add(new AttackAction(base.PlayerId, tileData.coordinates, tileData.coordinates, battleResults.attackDamage, shouldMoveToTarget: false, AttackAction.AnimationType.Splash, 100));
				state.TryGetPlayer(tileData.unit.owner, out var playerState2);
				PlayerExtensions.ReactToAttack(playerState, playerState2, tileData, state);
				if (state.Version >= 41 && state.TryGetTask(playerState, TaskData.Type.Pacifist, out var task))
				{
					task.Reset();
				}
			}
		}
		if (base.PlayerId == unitState.owner && state.GameLogicData.TryGetData(unitState.type, out var data))
		{
			state.ActionStack.Add(new DecreaseScoreAction(base.PlayerId, (int)ScoreSheet.GetUnitScore(data)));
		}
		ActionUtils.SpawnPoisonResource(state, tile);
		tile.unit = null;
	}

	public override ActionType GetActionType()
	{
		return ActionType.Explode;
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
