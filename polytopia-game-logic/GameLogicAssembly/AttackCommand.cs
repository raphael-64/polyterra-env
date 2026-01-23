using System;
using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class AttackCommand : CommandBase
{
	public uint UnitId { get; private set; }

	public WorldCoordinates Origin { get; private set; }

	public WorldCoordinates Target { get; private set; }

	public AttackCommand()
	{
	}

	public AttackCommand(byte playerId, UnitState unit, WorldCoordinates target)
		: base(playerId)
	{
		UnitId = unit.id;
		Origin = unit.coordinates;
		Target = target;
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		if (!PassesBasicValidation(state, out validationError))
		{
			return false;
		}
		if (!state.TryGetPlayer(base.PlayerId, out var _))
		{
			Log.Error("Failed to load player {0}, won't perform attack action", new object[1] { base.PlayerId });
			validationError = CommandBase.VALIDATION_ERROR_MISSING_PLAYER;
			return false;
		}
		TileData tile = state.Map.GetTile(Origin);
		if (tile.unit == null || tile.unit.id != UnitId)
		{
			validationError = CommandBase.VALIDATION_ERROR_UNIT_MISSING;
			Log.Error("Unit {0} is not on origin tile {1}", new object[2] { UnitId, Origin });
			return false;
		}
		if (!state.TryGetUnit(UnitId, out var unit) || !state.GameLogicData.TryGetData(unit.type, out var data))
		{
			validationError = CommandBase.VALIDATION_ERROR_MISSING_UNIT_DATA;
			Log.Error("Invalid unit {0}", new object[1] { UnitId });
			return false;
		}
		if (!unit.CanAttack())
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_ATTACK;
			return false;
		}
		state.Map.GetTile(Target);
		if (!unit.GetAttackOptions(state, data.GetRange()).Contains(Target))
		{
			validationError = CommandBase.VALIDATION_ERROR_INVALID_ATTACK_TARGET;
			Log.Error("Invalid target {0}", new object[1] { Target });
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		if (state.Version <= 17)
		{
			ExecuteV17(state);
		}
		else if (state.Version < 50)
		{
			ExecuteV50(state);
		}
		else if (state.Version < 60)
		{
			ExecuteV59(state);
		}
		else if (state.Version < 80)
		{
			ExecuteV79(state);
		}
		else if (state.Version < 81)
		{
			ExecuteV80(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	public void ExecuteDefault(GameState gameState)
	{
		gameState.TryGetUnit(UnitId, out var unit);
		TileData tile = gameState.Map.GetTile(Target);
		UnitState unit2 = tile.unit;
		gameState.TryGetPlayer(base.PlayerId, out var playerState);
		if (unit.HasAbility(UnitAbility.Type.Infiltrate, gameState) || (unit.passengerUnit != null && unit.passengerUnit.HasAbility(UnitAbility.Type.Infiltrate, gameState)))
		{
			gameState.ActionStack.Add(new InfiltrateAction(base.PlayerId, unit.coordinates, Target));
			unit.attacked = true;
			unit.moved = true;
			playerState.SetLastAttack(tile.owner, (int)gameState.CurrentTurn, gameState);
			return;
		}
		gameState.TryGetPlayer(unit2.owner, out var playerState2);
		gameState.GameLogicData.TryGetData(unit.type, out var data);
		PlayerExtensions.ReactToAttack(playerState, playerState2, tile, gameState);
		BattleResults battleResults = BattleHelpers.GetBattleResults(gameState, unit, unit2);
		if (unit.HasAbility(UnitAbility.Type.Convert, gameState) && battleResults.attackDamage < unit2.health)
		{
			if (unit2.HasAbility(UnitAbility.Type.Disloyal, gameState))
			{
				gameState.ActionStack.Add(new DisbandUnitAction(base.PlayerId, Target));
			}
			else if (unit2.owner != byte.MaxValue)
			{
				gameState.ActionStack.Add(new ConvertAction(base.PlayerId, Origin, Target));
			}
		}
		if (battleResults.retaliationDamage < unit.health)
		{
			gameState.ActionStack.Add(new ReselectAction(base.PlayerId, UnitId));
		}
		Log.Verbose("battleResults: {0}", new object[1] { battleResults });
		if (battleResults.shouldMoveToDefeatedEnemyTile)
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, gameState, Target);
		}
		else
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, gameState);
		}
		unit.attacked = true;
		if (unit.HasAbility(UnitAbility.Type.Persist, gameState) && battleResults.attackDamage >= unit2.health)
		{
			WorldCoordinates position = (battleResults.shouldMoveToDefeatedEnemyTile ? Target : Origin);
			int range = data.GetRange();
			bool includeHiddenTiles = data.GetSightRange() >= range;
			List<WorldCoordinates> attackOptionsAtPosition = UnitDataExtensions.GetAttackOptionsAtPosition(gameState, base.PlayerId, position, range, includeHiddenTiles, unit);
			if (attackOptionsAtPosition != null && attackOptionsAtPosition.Count > 0)
			{
				foreach (WorldCoordinates item in attackOptionsAtPosition)
				{
					if (!(item == Target))
					{
						unit.attacked = false;
						break;
					}
				}
			}
		}
		if (unit.HasAbility(UnitAbility.Type.Freeze, gameState))
		{
			gameState.ActionStack.Add(new FreezeUnitAction(base.PlayerId, Origin, Target, battleResults.attackDamage));
			return;
		}
		ushort delay = 100;
		if (battleResults.attackDamage >= unit2.health && data.HasAbility(UnitAbility.Type.Eat) && unit2.owner != byte.MaxValue)
		{
			UnitState unitState = unit;
			while (unitState.HasFollower())
			{
				gameState.TryGetUnit(unitState.follower, out var unit3);
				if (unit3 == null)
				{
					break;
				}
				unitState = unit3;
			}
			gameState.ActionStack.Add(new EatAction(base.PlayerId, unitState.id));
		}
		if (battleResults.shouldMoveToDefeatedEnemyTile)
		{
			gameState.ActionStack.Add(new MoveAction(base.PlayerId, UnitId, new List<WorldCoordinates> { Target, Origin }, MoveAction.MoveReason.Attack));
		}
		else if (battleResults.retaliationDamage > 0)
		{
			delay = 250;
			gameState.ActionStack.Add(new AttackAction(base.PlayerId, Target, Origin, battleResults.retaliationDamage, shouldMoveToTarget: false, AttackAction.AnimationType.Normal, 100));
		}
		if (unit.HasAbility(UnitAbility.Type.Splash, gameState) && unit.GetRange(gameState) > 1)
		{
			foreach (TileData tileNeighbor in gameState.Map.GetTileNeighbors(Target))
			{
				if (tileNeighbor.unit != null && !tileNeighbor.unit.IsFriendly(gameState, playerState))
				{
					if (unit.HasAbility(UnitAbility.Type.Poison, gameState))
					{
						gameState.ActionStack.Add(new PoisonUnitAction(base.PlayerId, Origin, tileNeighbor.coordinates));
						continue;
					}
					BattleResults battleResults2 = BattleHelpers.GetBattleResults(gameState, unit, tileNeighbor.unit);
					gameState.ActionStack.Add(new AttackAction(base.PlayerId, Origin, tileNeighbor.coordinates, battleResults2.attackDamage / 2, shouldMoveToTarget: false, AttackAction.AnimationType.Splash, 20));
				}
			}
		}
		gameState.ActionStack.Add(new AttackAction(base.PlayerId, Origin, Target, battleResults.attackDamage, battleResults.shouldMoveToDefeatedEnemyTile, AttackAction.AnimationType.Normal, delay));
		if (gameState.TryGetTask(playerState, TaskData.Type.Pacifist, out var task))
		{
			task.Reset();
		}
	}

	public void ExecuteV80(GameState gameState)
	{
		Log.Verbose("[felix] Execute attack", Array.Empty<object>());
		gameState.TryGetUnit(UnitId, out var unit);
		TileData tile = gameState.Map.GetTile(Target);
		UnitState unit2 = tile.unit;
		gameState.TryGetPlayer(base.PlayerId, out var playerState);
		if (unit.HasAbility(UnitAbility.Type.Infiltrate, gameState) || (unit.passengerUnit != null && unit.passengerUnit.HasAbility(UnitAbility.Type.Infiltrate, gameState)))
		{
			Log.Verbose("[felix] Do infiltrate", Array.Empty<object>());
			gameState.ActionStack.Add(new InfiltrateAction(base.PlayerId, unit.coordinates, Target));
			gameState.ActionStack.Add(new RevealAction(base.PlayerId, unit.coordinates));
			unit.attacked = true;
			unit.moved = true;
			playerState.SetLastAttack(tile.owner, (int)gameState.CurrentTurn, gameState);
			return;
		}
		gameState.TryGetPlayer(unit2.owner, out var playerState2);
		gameState.GameLogicData.TryGetData(unit.type, out var data);
		PlayerExtensions.ReactToAttack(playerState, playerState2, tile, gameState);
		BattleResults battleResults = BattleHelpers.GetBattleResults(gameState, unit, unit2);
		if (unit.HasAbility(UnitAbility.Type.Convert, gameState) && battleResults.attackDamage < unit2.health)
		{
			if (unit2.HasAbility(UnitAbility.Type.Disloyal, gameState))
			{
				gameState.ActionStack.Add(new DisbandUnitAction(base.PlayerId, Target));
			}
			else if (unit2.owner != byte.MaxValue)
			{
				gameState.ActionStack.Add(new ConvertAction(base.PlayerId, Origin, Target));
			}
		}
		if (battleResults.retaliationDamage < unit.health)
		{
			gameState.ActionStack.Add(new ReselectAction(base.PlayerId, UnitId));
		}
		Log.Verbose("battleResults: {0}", new object[1] { battleResults });
		if (battleResults.shouldMoveToDefeatedEnemyTile)
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, gameState, Target);
		}
		else
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, gameState);
		}
		unit.attacked = true;
		if (unit.HasAbility(UnitAbility.Type.Persist, gameState) && battleResults.attackDamage >= unit2.health)
		{
			WorldCoordinates position = (battleResults.shouldMoveToDefeatedEnemyTile ? Target : Origin);
			int range = data.GetRange();
			bool includeHiddenTiles = data.GetSightRange() >= range;
			List<WorldCoordinates> attackOptionsAtPosition = UnitDataExtensions.GetAttackOptionsAtPosition(gameState, base.PlayerId, position, range, includeHiddenTiles, unit);
			if (attackOptionsAtPosition != null && attackOptionsAtPosition.Count > 0)
			{
				foreach (WorldCoordinates item in attackOptionsAtPosition)
				{
					if (!(item == Target))
					{
						unit.attacked = false;
						break;
					}
				}
			}
		}
		if (unit.HasAbility(UnitAbility.Type.Freeze, gameState))
		{
			gameState.ActionStack.Add(new FreezeUnitAction(base.PlayerId, Origin, Target, battleResults.attackDamage));
			return;
		}
		ushort delay = 100;
		if (battleResults.attackDamage >= unit2.health && data.HasAbility(UnitAbility.Type.Eat) && unit2.owner != byte.MaxValue)
		{
			UnitState unitState = unit;
			while (unitState.HasFollower())
			{
				gameState.TryGetUnit(unitState.follower, out var unit3);
				if (unit3 == null)
				{
					break;
				}
				unitState = unit3;
			}
			gameState.ActionStack.Add(new EatAction(base.PlayerId, unitState.id));
		}
		if (battleResults.shouldMoveToDefeatedEnemyTile)
		{
			gameState.ActionStack.Add(new MoveAction(base.PlayerId, UnitId, new List<WorldCoordinates> { Target, Origin }, MoveAction.MoveReason.Attack));
		}
		else if (battleResults.retaliationDamage > 0)
		{
			delay = 250;
			gameState.ActionStack.Add(new AttackAction(base.PlayerId, Target, Origin, battleResults.retaliationDamage, shouldMoveToTarget: false, AttackAction.AnimationType.Normal, 100));
		}
		if (unit.HasAbility(UnitAbility.Type.Splash, gameState) && unit.GetRange(gameState) > 1)
		{
			foreach (TileData tileNeighbor in gameState.Map.GetTileNeighbors(Target))
			{
				if (tileNeighbor.unit != null && !tileNeighbor.unit.IsFriendly(gameState, playerState))
				{
					if (unit.HasAbility(UnitAbility.Type.Poison, gameState))
					{
						gameState.ActionStack.Add(new PoisonUnitAction(base.PlayerId, Origin, tileNeighbor.coordinates));
						continue;
					}
					BattleResults battleResults2 = BattleHelpers.GetBattleResults(gameState, unit, tileNeighbor.unit);
					gameState.ActionStack.Add(new AttackAction(base.PlayerId, Origin, tileNeighbor.coordinates, battleResults2.attackDamage / 2, shouldMoveToTarget: false, AttackAction.AnimationType.Splash, 20));
				}
			}
		}
		gameState.ActionStack.Add(new AttackAction(base.PlayerId, Origin, Target, battleResults.attackDamage, battleResults.shouldMoveToDefeatedEnemyTile, AttackAction.AnimationType.Normal, delay));
		if (gameState.TryGetTask(playerState, TaskData.Type.Pacifist, out var task))
		{
			task.Reset();
		}
	}

	public void ExecuteV79(GameState gameState)
	{
		Log.Verbose("[felix] Execute attack", Array.Empty<object>());
		gameState.TryGetUnit(UnitId, out var unit);
		TileData tile = gameState.Map.GetTile(Target);
		UnitState unit2 = tile.unit;
		gameState.TryGetPlayer(base.PlayerId, out var playerState);
		if (unit.HasAbility(UnitAbility.Type.Infiltrate, gameState) || (unit.passengerUnit != null && unit.passengerUnit.HasAbility(UnitAbility.Type.Infiltrate, gameState)))
		{
			Log.Verbose("[felix] Do infiltrate", Array.Empty<object>());
			gameState.ActionStack.Add(new InfiltrateAction(base.PlayerId, unit.coordinates, Target));
			gameState.ActionStack.Add(new RevealAction(base.PlayerId, unit.coordinates));
			unit.attacked = true;
			unit.moved = true;
			playerState.SetLastAttack(tile.owner, (int)gameState.CurrentTurn, gameState);
			return;
		}
		gameState.TryGetPlayer(unit2.owner, out var playerState2);
		gameState.GameLogicData.TryGetData(unit.type, out var data);
		PlayerExtensions.ReactToAttack(playerState, playerState2, tile, gameState);
		BattleResults battleResults = BattleHelpers.GetBattleResults(gameState, unit, unit2);
		if (unit.HasAbility(UnitAbility.Type.Convert, gameState) && battleResults.attackDamage < unit2.health && !unit2.HasLeader())
		{
			if (unit2.HasAbility(UnitAbility.Type.Disloyal, gameState))
			{
				gameState.ActionStack.Add(new DisbandUnitAction(base.PlayerId, Target));
			}
			else if (unit2.owner != byte.MaxValue)
			{
				gameState.ActionStack.Add(new ConvertAction(base.PlayerId, Origin, Target));
			}
		}
		if (battleResults.retaliationDamage < unit.health)
		{
			gameState.ActionStack.Add(new ReselectAction(base.PlayerId, UnitId));
		}
		Log.Verbose("battleResults: {0}", new object[1] { battleResults });
		if (battleResults.shouldMoveToDefeatedEnemyTile)
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, gameState, Target);
		}
		else
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, gameState);
		}
		unit.attacked = true;
		if (unit.HasAbility(UnitAbility.Type.Persist, gameState) && battleResults.attackDamage >= unit2.health)
		{
			WorldCoordinates position = (battleResults.shouldMoveToDefeatedEnemyTile ? Target : Origin);
			int range = data.GetRange();
			bool includeHiddenTiles = data.GetSightRange() >= range;
			List<WorldCoordinates> attackOptionsAtPosition = UnitDataExtensions.GetAttackOptionsAtPosition(gameState, base.PlayerId, position, range, includeHiddenTiles);
			if (attackOptionsAtPosition != null && attackOptionsAtPosition.Count > 0)
			{
				foreach (WorldCoordinates item in attackOptionsAtPosition)
				{
					if (!(item == Target))
					{
						unit.attacked = false;
						break;
					}
				}
			}
		}
		if (unit.HasAbility(UnitAbility.Type.Freeze, gameState))
		{
			gameState.ActionStack.Add(new FreezeUnitAction(base.PlayerId, Origin, Target, battleResults.attackDamage));
			return;
		}
		ushort delay = 100;
		if (battleResults.attackDamage >= unit2.health && data.HasAbility(UnitAbility.Type.Eat) && unit2.owner != byte.MaxValue)
		{
			UnitState unitState = unit;
			while (unitState.HasFollower())
			{
				gameState.TryGetUnit(unitState.follower, out var unit3);
				if (unit3 == null)
				{
					break;
				}
				unitState = unit3;
			}
			gameState.ActionStack.Add(new EatAction(base.PlayerId, unitState.id));
		}
		if (battleResults.shouldMoveToDefeatedEnemyTile)
		{
			gameState.ActionStack.Add(new MoveAction(base.PlayerId, UnitId, new List<WorldCoordinates> { Target, Origin }, MoveAction.MoveReason.Attack));
		}
		else if (battleResults.retaliationDamage > 0)
		{
			delay = 250;
			gameState.ActionStack.Add(new AttackAction(base.PlayerId, Target, Origin, battleResults.retaliationDamage, shouldMoveToTarget: false, AttackAction.AnimationType.Normal, 100));
		}
		if (unit.HasAbility(UnitAbility.Type.Splash, gameState) && unit.GetRange(gameState) > 1)
		{
			foreach (TileData tileNeighbor in gameState.Map.GetTileNeighbors(Target))
			{
				if (tileNeighbor.unit != null && !tileNeighbor.unit.IsFriendly(gameState, playerState))
				{
					if (unit.HasAbility(UnitAbility.Type.Poison, gameState))
					{
						gameState.ActionStack.Add(new PoisonUnitAction(base.PlayerId, Origin, tileNeighbor.coordinates));
						continue;
					}
					BattleResults battleResults2 = BattleHelpers.GetBattleResults(gameState, unit, tileNeighbor.unit);
					gameState.ActionStack.Add(new AttackAction(base.PlayerId, Origin, tileNeighbor.coordinates, battleResults2.attackDamage / 2, shouldMoveToTarget: false, AttackAction.AnimationType.Splash, 20));
				}
			}
		}
		gameState.ActionStack.Add(new AttackAction(base.PlayerId, Origin, Target, battleResults.attackDamage, battleResults.shouldMoveToDefeatedEnemyTile, AttackAction.AnimationType.Normal, delay));
		if (gameState.TryGetTask(playerState, TaskData.Type.Pacifist, out var task))
		{
			task.Reset();
		}
	}

	public void ExecuteV59(GameState state)
	{
		state.TryGetUnit(UnitId, out var unit);
		TileData tile = state.Map.GetTile(Target);
		UnitState unit2 = tile.unit;
		state.TryGetPlayer(base.PlayerId, out var playerState);
		state.TryGetPlayer(unit2.owner, out var playerState2);
		state.GameLogicData.TryGetData(unit.type, out var data);
		PlayerExtensions.ReactToAttack(playerState, playerState2, tile, state);
		BattleResults battleResults = BattleHelpers.GetBattleResults(state, unit, unit2);
		if (unit.HasAbility(UnitAbility.Type.Convert, state) && battleResults.attackDamage < unit2.health && unit2.owner != byte.MaxValue)
		{
			state.ActionStack.Add(new ConvertAction(base.PlayerId, Origin, Target));
		}
		if (battleResults.retaliationDamage < unit.health)
		{
			state.ActionStack.Add(new ReselectAction(base.PlayerId, UnitId));
		}
		if (battleResults.shouldMoveToDefeatedEnemyTile)
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, state, Target);
		}
		else
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, state);
		}
		unit.attacked = true;
		if (unit.HasAbility(UnitAbility.Type.Persist, state) && battleResults.attackDamage >= unit2.health)
		{
			WorldCoordinates position = (battleResults.shouldMoveToDefeatedEnemyTile ? Target : Origin);
			int range = data.GetRange();
			bool includeHiddenTiles = data.GetSightRange() >= range;
			List<WorldCoordinates> attackOptionsAtPosition = UnitDataExtensions.GetAttackOptionsAtPosition(state, base.PlayerId, position, range, includeHiddenTiles);
			if (attackOptionsAtPosition != null && attackOptionsAtPosition.Count > 0)
			{
				foreach (WorldCoordinates item in attackOptionsAtPosition)
				{
					if (!(item == Target))
					{
						unit.attacked = false;
						break;
					}
				}
			}
		}
		if (unit.HasAbility(UnitAbility.Type.Freeze, state))
		{
			state.ActionStack.Add(new FreezeUnitAction(base.PlayerId, Origin, Target, battleResults.attackDamage));
			return;
		}
		ushort delay = 100;
		if (battleResults.attackDamage >= unit2.health && data.HasAbility(UnitAbility.Type.Eat) && unit2.owner != byte.MaxValue)
		{
			UnitState unitState = unit;
			while (unitState.HasFollower())
			{
				state.TryGetUnit(unitState.follower, out var unit3);
				if (unit3 == null)
				{
					break;
				}
				unitState = unit3;
			}
			state.ActionStack.Add(new EatAction(base.PlayerId, unitState.id));
		}
		if (battleResults.shouldMoveToDefeatedEnemyTile)
		{
			state.ActionStack.Add(new MoveAction(base.PlayerId, UnitId, new List<WorldCoordinates> { Target, Origin }, MoveAction.MoveReason.Attack));
		}
		else if (battleResults.retaliationDamage > 0)
		{
			delay = 250;
			state.ActionStack.Add(new AttackAction(base.PlayerId, Target, Origin, battleResults.retaliationDamage, shouldMoveToTarget: false, AttackAction.AnimationType.Normal, 100));
		}
		if (unit.HasAbility(UnitAbility.Type.Splash, state) && unit.GetRange(state) > 1)
		{
			foreach (TileData tileNeighbor in state.Map.GetTileNeighbors(Target))
			{
				if (tileNeighbor.unit != null && tileNeighbor.unit.owner != base.PlayerId)
				{
					if (unit.HasAbility(UnitAbility.Type.Poison, state))
					{
						state.ActionStack.Add(new PoisonUnitAction(base.PlayerId, Origin, tileNeighbor.coordinates));
						continue;
					}
					BattleResults battleResults2 = BattleHelpers.GetBattleResults(state, unit, tileNeighbor.unit);
					state.ActionStack.Add(new AttackAction(base.PlayerId, Origin, tileNeighbor.coordinates, battleResults2.attackDamage / 2, shouldMoveToTarget: false, AttackAction.AnimationType.Splash, 20));
				}
			}
		}
		state.ActionStack.Add(new AttackAction(base.PlayerId, Origin, Target, battleResults.attackDamage, battleResults.shouldMoveToDefeatedEnemyTile, AttackAction.AnimationType.Normal, delay));
		if (state.TryGetTask(playerState, TaskData.Type.Pacifist, out var task))
		{
			task.Reset();
		}
	}

	public void ExecuteV50(GameState state)
	{
		state.TryGetUnit(UnitId, out var unit);
		TileData tile = state.Map.GetTile(Target);
		UnitState unit2 = tile.unit;
		state.TryGetPlayer(base.PlayerId, out var playerState);
		state.TryGetPlayer(unit2.owner, out var playerState2);
		state.GameLogicData.TryGetData(unit.type, out var data);
		PlayerExtensions.ReactToAttack(playerState, playerState2, tile, state);
		BattleResults battleResults = BattleHelpers.GetBattleResults(state, unit, unit2);
		if (unit.HasAbility(UnitAbility.Type.Convert, state) && battleResults.attackDamage < unit2.health && !unit2.HasLeader() && unit2.owner != byte.MaxValue)
		{
			state.ActionStack.Add(new ConvertAction(base.PlayerId, Origin, Target));
		}
		if (battleResults.retaliationDamage < unit.health)
		{
			state.ActionStack.Add(new ReselectAction(base.PlayerId, UnitId));
		}
		Log.Verbose("battleResults: {0}", new object[1] { battleResults });
		if (battleResults.shouldMoveToDefeatedEnemyTile)
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, state, Target);
		}
		else
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, state);
		}
		unit.attacked = true;
		if (unit.HasAbility(UnitAbility.Type.Persist, state) && battleResults.attackDamage >= unit2.health)
		{
			WorldCoordinates position = (battleResults.shouldMoveToDefeatedEnemyTile ? Target : Origin);
			int range = data.GetRange();
			bool includeHiddenTiles = data.GetSightRange() >= range;
			List<WorldCoordinates> attackOptionsAtPosition = UnitDataExtensions.GetAttackOptionsAtPosition(state, base.PlayerId, position, range, includeHiddenTiles);
			if (attackOptionsAtPosition != null && attackOptionsAtPosition.Count > 0)
			{
				foreach (WorldCoordinates item in attackOptionsAtPosition)
				{
					if (!(item == Target))
					{
						unit.attacked = false;
						break;
					}
				}
			}
		}
		if (unit.HasAbility(UnitAbility.Type.Freeze, state))
		{
			state.ActionStack.Add(new FreezeUnitAction(base.PlayerId, Origin, Target, battleResults.attackDamage));
			return;
		}
		ushort delay = 100;
		if (battleResults.attackDamage >= unit2.health && data.HasAbility(UnitAbility.Type.Eat) && unit2.owner != byte.MaxValue)
		{
			UnitState unitState = unit;
			while (unitState.HasFollower())
			{
				state.TryGetUnit(unitState.follower, out var unit3);
				if (unit3 == null)
				{
					break;
				}
				unitState = unit3;
			}
			state.ActionStack.Add(new EatAction(base.PlayerId, unitState.id));
		}
		if (battleResults.shouldMoveToDefeatedEnemyTile)
		{
			state.ActionStack.Add(new MoveAction(base.PlayerId, UnitId, new List<WorldCoordinates> { Target, Origin }, MoveAction.MoveReason.Attack));
		}
		else if (battleResults.retaliationDamage > 0)
		{
			delay = 250;
			state.ActionStack.Add(new AttackAction(base.PlayerId, Target, Origin, battleResults.retaliationDamage, shouldMoveToTarget: false, AttackAction.AnimationType.Normal, 100));
		}
		if (unit.HasAbility(UnitAbility.Type.Splash, state) && unit.GetRange(state) > 1)
		{
			foreach (TileData tileNeighbor in state.Map.GetTileNeighbors(Target))
			{
				if (tileNeighbor.unit != null && tileNeighbor.unit.owner != base.PlayerId)
				{
					if (unit.HasAbility(UnitAbility.Type.Poison, state))
					{
						state.ActionStack.Add(new PoisonUnitAction(base.PlayerId, Origin, tileNeighbor.coordinates));
						continue;
					}
					BattleResults battleResults2 = BattleHelpers.GetBattleResults(state, unit, tileNeighbor.unit);
					state.ActionStack.Add(new AttackAction(base.PlayerId, Origin, tileNeighbor.coordinates, battleResults2.attackDamage / 2, shouldMoveToTarget: false, AttackAction.AnimationType.Splash, 20));
				}
			}
		}
		state.ActionStack.Add(new AttackAction(base.PlayerId, Origin, Target, battleResults.attackDamage, battleResults.shouldMoveToDefeatedEnemyTile, AttackAction.AnimationType.Normal, delay));
		if (state.TryGetTask(playerState, TaskData.Type.Pacifist, out var task))
		{
			task.Reset();
		}
	}

	public void ExecuteV17(GameState state)
	{
		state.TryGetUnit(UnitId, out var unit);
		TileData tile = state.Map.GetTile(Target);
		UnitState unit2 = tile.unit;
		state.TryGetPlayer(base.PlayerId, out var playerState);
		state.TryGetPlayer(unit2.owner, out var playerState2);
		state.GameLogicData.TryGetData(unit.type, out var data);
		playerState2.RemoveNonagression(base.PlayerId);
		playerState2.ModifyAggression(base.PlayerId, 1000);
		if (playerState.GetAggression(unit2.owner, state) > 1000)
		{
			playerState.ModifyAggression(unit2.owner, -200);
		}
		foreach (PlayerState playerState3 in state.PlayerStates)
		{
			if (playerState3.Id != base.PlayerId && tile.GetExplored(playerState3.Id))
			{
				int aggressionModifier = -(playerState3.GetAggression(playerState2.Id, state) - 1000) / 5;
				playerState3.ModifyAggression(base.PlayerId, aggressionModifier);
			}
		}
		if (unit.HasAbility(UnitAbility.Type.Convert, state))
		{
			if (unit2.owner != byte.MaxValue)
			{
				state.ActionStack.Add(new ConvertAction(base.PlayerId, Origin, Target));
			}
			return;
		}
		foreach (PlayerState playerState4 in state.PlayerStates)
		{
			if (tile.GetExplored(playerState4.Id))
			{
				playerState4.ModifyAggression(base.PlayerId, 1000 - playerState4.GetAggression(unit2.owner, state) / 2);
			}
		}
		BattleResults battleResults = BattleHelpers.GetBattleResults(state, unit, unit2);
		if (battleResults.retaliationDamage < unit.health)
		{
			state.ActionStack.Add(new ReselectAction(base.PlayerId, UnitId));
		}
		Log.Verbose("battleResults: {0}", new object[1] { battleResults });
		if (battleResults.shouldMoveToDefeatedEnemyTile)
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, state, Target);
		}
		else
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, state);
		}
		unit.attacked = true;
		if (unit.HasAbility(UnitAbility.Type.Persist, state) && battleResults.attackDamage >= unit2.health)
		{
			WorldCoordinates position = (battleResults.shouldMoveToDefeatedEnemyTile ? Target : Origin);
			int range = data.GetRange();
			bool includeHiddenTiles = data.GetSightRange() >= range;
			List<WorldCoordinates> attackOptionsAtPosition = UnitDataExtensions.GetAttackOptionsAtPosition(state, base.PlayerId, position, range, includeHiddenTiles);
			if (attackOptionsAtPosition != null && attackOptionsAtPosition.Count > 0)
			{
				foreach (WorldCoordinates item in attackOptionsAtPosition)
				{
					if (!(item == Target))
					{
						unit.attacked = false;
						break;
					}
				}
			}
		}
		if (unit.HasAbility(UnitAbility.Type.Freeze, state))
		{
			state.ActionStack.Add(new FreezeUnitAction(base.PlayerId, Origin, Target, battleResults.attackDamage));
			return;
		}
		ushort delay = 100;
		if (battleResults.shouldMoveToDefeatedEnemyTile)
		{
			state.ActionStack.Add(new MoveAction(base.PlayerId, UnitId, new List<WorldCoordinates> { Target, Origin }, shouldAnimate: false));
		}
		else if (battleResults.retaliationDamage > 0)
		{
			delay = 250;
			state.ActionStack.Add(new AttackAction(base.PlayerId, Target, Origin, battleResults.retaliationDamage, shouldMoveToTarget: false, AttackAction.AnimationType.Normal, 100));
		}
		if (unit.HasAbility(UnitAbility.Type.Splash, state))
		{
			foreach (TileData tileNeighbor in state.Map.GetTileNeighbors(Target))
			{
				if (tileNeighbor.unit != null && tileNeighbor.unit.owner != base.PlayerId)
				{
					BattleResults battleResults2 = BattleHelpers.GetBattleResults(state, unit, tileNeighbor.unit);
					state.ActionStack.Add(new AttackAction(base.PlayerId, Origin, tileNeighbor.coordinates, battleResults2.attackDamage / 2, shouldMoveToTarget: false, AttackAction.AnimationType.Splash, 0));
				}
			}
		}
		state.ActionStack.Add(new AttackAction(base.PlayerId, Origin, Target, battleResults.attackDamage, battleResults.shouldMoveToDefeatedEnemyTile, AttackAction.AnimationType.Normal, delay));
		if (state.TryGetTask(playerState, TaskData.Type.Pacifist, out var task))
		{
			task.Reset();
		}
	}

	public override CommandType GetCommandType()
	{
		return CommandType.Attack;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(UnitId);
		Origin.Serialize(writer, version);
		Target.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		UnitId = reader.ReadUInt32();
		Origin = new WorldCoordinates(reader, version);
		Target = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, UnitId: {UnitId}, Origin: {Origin}, Target {Target})";
	}
}
