using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class MoveAction : ActionBase
{
	public enum MoveReason : byte
	{
		Command,
		Attack,
		Push
	}

	public uint UnitId { get; protected set; }

	public List<WorldCoordinates> Path { get; protected set; }

	public bool ShouldAnimate { get; protected set; }

	public MoveReason Reason { get; protected set; }

	public MoveAction()
	{
	}

	public MoveAction(byte playerId, uint unitId, List<WorldCoordinates> path, MoveReason moveReason = MoveReason.Command)
		: base(playerId)
	{
		UnitId = unitId;
		Path = path;
		Reason = moveReason;
		ShouldAnimate = moveReason != MoveReason.Attack;
	}

	public MoveAction(byte playerId, uint unitId, List<WorldCoordinates> path, bool shouldAnimate)
		: base(playerId)
	{
		UnitId = unitId;
		Path = path;
		ShouldAnimate = shouldAnimate;
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 6)
		{
			ExecuteV6(state);
		}
		else if (state.Version <= 17)
		{
			ExecuteV7(state);
		}
		else if (state.Version <= 18)
		{
			ExecuteV18(state);
		}
		else if (state.Version <= 19)
		{
			ExecuteV19(state);
		}
		else if (state.Version <= 26)
		{
			ExecuteV20(state);
		}
		else if (state.Version <= 27)
		{
			ExecuteV27(state);
		}
		else if (state.Version <= 28)
		{
			ExecuteV28(state);
		}
		else if (state.Version <= 40)
		{
			ExecuteV40(state);
		}
		else if (state.Version <= 43)
		{
			ExecuteV43(state);
		}
		else if (state.Version < 60)
		{
			ExecuteV59(state);
		}
		else if (state.Version < 84)
		{
			ExecuteV83(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	private void ExecuteDefault(GameState gameState)
	{
		if (!gameState.TryGetUnit(UnitId, out var unit) || !gameState.TryGetPlayer(base.PlayerId, out var playerState) || !gameState.GameLogicData.TryGetData(unit.type, out var data))
		{
			return;
		}
		WorldCoordinates worldCoordinates = Path[0];
		WorldCoordinates worldCoordinates2 = Path[Path.Count - 1];
		TileData tile = gameState.Map.GetTile(worldCoordinates2);
		TileData tile2 = gameState.Map.GetTile(worldCoordinates);
		unit.attacked = unit.attacked || (!unit.HasAbility(UnitAbility.Type.Dash, gameState, worldCoordinates) && unit.moved);
		unit.moved = unit.moved || ((Reason != MoveReason.Attack || !unit.HasAbility(UnitAbility.Type.Escape, gameState, worldCoordinates)) && Reason != MoveReason.Push);
		tile.unit = null;
		tile2.unit = unit;
		unit.coordinates = worldCoordinates;
		if (Path.Count > 1)
		{
			unit.SetUnitDirection(Path[1], worldCoordinates);
		}
		ActionUtils.CheckStepOnPoison(tile2, unit, gameState);
		if (Reason != MoveReason.Push && unit.HasAbility(UnitAbility.Type.AutoFreeze, gameState))
		{
			gameState.ActionStack.Add(new FreezeAreaAction(base.PlayerId, tile2.coordinates, 1, freezeUnits: true));
		}
		int sightRange = data.GetSightRange();
		foreach (WorldCoordinates item in Path)
		{
			TileData tile3 = gameState.Map.GetTile(item);
			ActionUtils.ExploreFromTile(gameState, playerState, tile3, sightRange, shouldUseActions: true);
		}
		if (unit.type == UnitData.Type.Bunny && tile2.improvement != null)
		{
			gameState.GameLogicData.TryGetData(tile2.improvement.type, out var data2);
			if (!tile2.HasImprovement(ImprovementData.Type.City) && !tile2.HasImprovement(ImprovementData.Type.Ruin) && !data2.HasAbility(ImprovementAbility.Type.Bridge))
			{
				gameState.ActionStack.Add(new DestroyImprovementAction(tile2.owner, tile2.coordinates));
			}
		}
		if (!data.IsAquatic() && !unit.HasAbility(UnitAbility.Type.Fly, gameState) && tile2.IsWater && tile2.HasImprovement(ImprovementData.Type.Port))
		{
			gameState.ActionStack.Add(new EmbarkAction(base.PlayerId, worldCoordinates));
		}
		else if (!tile2.IsWater && data.IsVehicle())
		{
			gameState.ActionStack.Add(new DisembarkAction(base.PlayerId, worldCoordinates));
		}
		if (unit.HasFollower() && gameState.TryGetUnit(unit.follower, out var unit2))
		{
			if (MapDataExtensions.ChebyshevDistance(unit2.coordinates, worldCoordinates) > 1)
			{
				gameState.ActionStack.Add(new MoveAction(base.PlayerId, unit.follower, new List<WorldCoordinates> { worldCoordinates2, unit2.coordinates }));
			}
			else
			{
				gameState.ActionStack.Add(new MoveAction(base.PlayerId, unit.follower, new List<WorldCoordinates>(2) { unit2.coordinates, unit2.coordinates }));
			}
		}
		if (tile2.HasImprovement(ImprovementData.Type.City) && tile2.owner != 0 && tile2.owner != playerState.Id)
		{
			playerState.SetLastAttack(tile2.owner, (int)gameState.CurrentTurn, gameState);
		}
	}

	private void ExecuteV83(GameState gameState)
	{
		if (!gameState.TryGetUnit(UnitId, out var unit) || !gameState.TryGetPlayer(base.PlayerId, out var playerState) || !gameState.GameLogicData.TryGetData(unit.type, out var data))
		{
			return;
		}
		WorldCoordinates worldCoordinates = Path[0];
		WorldCoordinates worldCoordinates2 = Path[Path.Count - 1];
		TileData tile = gameState.Map.GetTile(worldCoordinates2);
		TileData tile2 = gameState.Map.GetTile(worldCoordinates);
		unit.attacked = unit.attacked || (!unit.HasAbility(UnitAbility.Type.Dash, gameState, worldCoordinates) && unit.moved);
		unit.moved = unit.moved || ((Reason != MoveReason.Attack || !unit.HasAbility(UnitAbility.Type.Escape, gameState, worldCoordinates)) && Reason != MoveReason.Push);
		tile.unit = null;
		tile2.unit = unit;
		unit.coordinates = worldCoordinates;
		if (Path.Count > 1)
		{
			unit.SetUnitDirection(Path[1], worldCoordinates);
		}
		ActionUtils.CheckStepOnPoison(tile2, unit, gameState);
		if (Reason != MoveReason.Push && unit.HasAbility(UnitAbility.Type.AutoFreeze, gameState))
		{
			gameState.ActionStack.Add(new FreezeAreaAction(base.PlayerId, tile2.coordinates, 1, freezeUnits: true));
		}
		int sightRange = data.GetSightRange();
		foreach (WorldCoordinates item in Path)
		{
			TileData tile3 = gameState.Map.GetTile(item);
			ActionUtils.ExploreFromTile(gameState, playerState, tile3, sightRange, shouldUseActions: true);
		}
		if (unit.type == UnitData.Type.Bunny && tile2.improvement != null)
		{
			gameState.GameLogicData.TryGetData(tile2.improvement.type, out var data2);
			if (!tile2.HasImprovement(ImprovementData.Type.City) && !tile2.HasImprovement(ImprovementData.Type.Ruin) && !data2.HasAbility(ImprovementAbility.Type.Bridge))
			{
				gameState.ActionStack.Add(new DestroyImprovementAction(tile2.owner, tile2.coordinates));
			}
		}
		if (!data.IsAquatic() && !unit.HasAbility(UnitAbility.Type.Fly, gameState) && !tile.IsWater && tile2.IsWater && tile2.HasImprovement(ImprovementData.Type.Port))
		{
			gameState.ActionStack.Add(new EmbarkAction(base.PlayerId, worldCoordinates));
		}
		if (!tile2.IsWater && data.IsVehicle())
		{
			gameState.ActionStack.Add(new DisembarkAction(base.PlayerId, worldCoordinates));
		}
		if (unit.HasFollower() && gameState.TryGetUnit(unit.follower, out var unit2))
		{
			if (MapDataExtensions.ChebyshevDistance(unit2.coordinates, worldCoordinates) > 1)
			{
				gameState.ActionStack.Add(new MoveAction(base.PlayerId, unit.follower, new List<WorldCoordinates> { worldCoordinates2, unit2.coordinates }));
			}
			else
			{
				gameState.ActionStack.Add(new MoveAction(base.PlayerId, unit.follower, new List<WorldCoordinates>(2) { unit2.coordinates, unit2.coordinates }));
			}
		}
		if (tile2.HasImprovement(ImprovementData.Type.City) && tile2.owner != 0 && tile2.owner != playerState.Id)
		{
			playerState.SetLastAttack(tile2.owner, (int)gameState.CurrentTurn, gameState);
		}
	}

	private void ExecuteV59(GameState gameState)
	{
		if (!gameState.TryGetUnit(UnitId, out var unit) || !gameState.TryGetPlayer(base.PlayerId, out var playerState) || !gameState.GameLogicData.TryGetData(unit.type, out var data))
		{
			return;
		}
		WorldCoordinates worldCoordinates = Path[0];
		WorldCoordinates worldCoordinates2 = Path[Path.Count - 1];
		TileData tile = gameState.Map.GetTile(worldCoordinates2);
		TileData tile2 = gameState.Map.GetTile(worldCoordinates);
		unit.attacked = unit.attacked || (!unit.HasAbility(UnitAbility.Type.Dash, gameState, worldCoordinates) && unit.moved);
		unit.moved = unit.moved || ((Reason != MoveReason.Attack || !unit.HasAbility(UnitAbility.Type.Escape, gameState, worldCoordinates)) && Reason != MoveReason.Push);
		tile.unit = null;
		tile2.unit = unit;
		unit.coordinates = worldCoordinates;
		if (Path.Count > 1)
		{
			unit.SetUnitDirection(Path[1], worldCoordinates);
		}
		ActionUtils.CheckStepOnPoison(tile2, unit, gameState);
		if (Reason != MoveReason.Push && unit.HasAbility(UnitAbility.Type.AutoFreeze, gameState))
		{
			gameState.ActionStack.Add(new FreezeAreaAction(base.PlayerId, tile2.coordinates, 1, freezeUnits: true));
		}
		int sightRange = data.GetSightRange();
		foreach (WorldCoordinates item in Path)
		{
			TileData tile3 = gameState.Map.GetTile(item);
			ActionUtils.ExploreFromTile(gameState, playerState, tile3, sightRange, shouldUseActions: true);
		}
		if (unit.type == UnitData.Type.Bunny && tile2.improvement != null)
		{
			gameState.GameLogicData.TryGetData(tile2.improvement.type, out var data2);
			if (!tile2.HasImprovement(ImprovementData.Type.City) && !tile2.HasImprovement(ImprovementData.Type.Ruin) && !data2.HasAbility(ImprovementAbility.Type.Bridge))
			{
				gameState.ActionStack.Add(new DestroyImprovementAction(tile2.owner, tile2.coordinates));
			}
		}
		bool flag = !data.IsAquatic() && !unit.HasAbility(UnitAbility.Type.Fly, gameState);
		if (tile2.IsWater && flag && tile2.HasImprovement(ImprovementData.Type.Port))
		{
			gameState.ActionStack.Add(new EmbarkAction(base.PlayerId, worldCoordinates));
		}
		else if (tile.IsWater && !tile2.IsWater && data.IsVehicle())
		{
			gameState.ActionStack.Add(new DisembarkAction(base.PlayerId, worldCoordinates));
		}
		if (unit.HasFollower() && gameState.TryGetUnit(unit.follower, out var unit2))
		{
			if (MapDataExtensions.ChebyshevDistance(unit2.coordinates, worldCoordinates) > 1)
			{
				gameState.ActionStack.Add(new MoveAction(base.PlayerId, unit.follower, new List<WorldCoordinates> { worldCoordinates2, unit2.coordinates }));
			}
			else
			{
				gameState.ActionStack.Add(new MoveAction(base.PlayerId, unit.follower, new List<WorldCoordinates>(2) { unit2.coordinates, unit2.coordinates }));
			}
		}
	}

	private void ExecuteV43(GameState state)
	{
		if (!state.TryGetUnit(UnitId, out var unit) || !state.TryGetPlayer(base.PlayerId, out var playerState) || !state.GameLogicData.TryGetData(unit.type, out var data))
		{
			return;
		}
		WorldCoordinates worldCoordinates = Path[0];
		WorldCoordinates worldCoordinates2 = Path[Path.Count - 1];
		TileData tile = state.Map.GetTile(worldCoordinates2);
		TileData tile2 = state.Map.GetTile(worldCoordinates);
		unit.attacked = unit.attacked || (!unit.HasAbility(UnitAbility.Type.Dash, state, worldCoordinates) && unit.moved);
		unit.moved = unit.moved || ((Reason != MoveReason.Attack || !unit.HasAbility(UnitAbility.Type.Escape, state, worldCoordinates)) && Reason != MoveReason.Push);
		tile.unit = null;
		tile2.unit = unit;
		unit.coordinates = worldCoordinates;
		if (Path.Count > 1)
		{
			unit.SetUnitDirection(Path[1], worldCoordinates);
		}
		if (tile2.improvement != null && state.GameLogicData.TryGetData(tile2.improvement.type, out var data2) && data2 != null && data2.HasAbility(ImprovementAbility.Type.Poison) && !playerState.HasTribeAbility(TribeAbility.Type.PoisonResist, state) && !unit.HasAbility(UnitAbility.Type.Fly, state))
		{
			state.ActionStack.Add(new PoisonUnitAction(base.PlayerId, worldCoordinates, worldCoordinates));
			state.ActionStack.Add(new AttackAction(base.PlayerId, worldCoordinates, worldCoordinates, 20, shouldMoveToTarget: false, AttackAction.AnimationType.None, 0));
		}
		if (Reason != MoveReason.Push && unit.HasAbility(UnitAbility.Type.AutoFreeze, state))
		{
			state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, tile2.coordinates, 1, freezeUnits: true));
		}
		int sightRange = data.GetSightRange();
		foreach (WorldCoordinates item in Path)
		{
			TileData tile3 = state.Map.GetTile(item);
			ActionUtils.ExploreFromTile(state, playerState, tile3, sightRange, shouldUseActions: true);
		}
		if (unit.type == UnitData.Type.Bunny && tile2.improvement != null)
		{
			state.GameLogicData.TryGetData(tile2.improvement.type, out var data3);
			if (!tile2.HasImprovement(ImprovementData.Type.City) && !tile2.HasImprovement(ImprovementData.Type.Ruin) && !data3.HasAbility(ImprovementAbility.Type.Bridge))
			{
				state.ActionStack.Add(new DestroyImprovementAction(tile2.owner, tile2.coordinates));
			}
		}
		bool flag = !data.IsAquatic() && !unit.HasAbility(UnitAbility.Type.Fly, state);
		if (tile2.IsWater && flag && tile2.HasImprovement(ImprovementData.Type.Port))
		{
			state.ActionStack.Add(new EmbarkAction(base.PlayerId, worldCoordinates));
		}
		else if (tile.IsWater && !tile2.IsWater && data.IsVehicle())
		{
			state.ActionStack.Add(new DisembarkAction(base.PlayerId, worldCoordinates));
		}
		if (!unit.HasFollower() || !state.TryGetUnit(unit.follower, out var unit2))
		{
			return;
		}
		if (MapDataExtensions.ChebyshevDistance(unit2.coordinates, worldCoordinates) > 1)
		{
			List<WorldCoordinates> path = state.GetPath(unit2.coordinates, worldCoordinates2, 10, unit);
			if (path != null && path.Count >= 2)
			{
				state.ActionStack.Add(new MoveAction(base.PlayerId, unit.follower, path));
			}
		}
		else
		{
			state.ActionStack.Add(new MoveAction(base.PlayerId, unit.follower, new List<WorldCoordinates>(2) { unit2.coordinates, unit2.coordinates }));
		}
	}

	private void ExecuteV40(GameState state)
	{
		if (!state.TryGetUnit(UnitId, out var unit) || !state.TryGetPlayer(base.PlayerId, out var playerState) || !state.GameLogicData.TryGetData(unit.type, out var data))
		{
			return;
		}
		WorldCoordinates worldCoordinates = Path[0];
		WorldCoordinates worldCoordinates2 = Path[Path.Count - 1];
		TileData tile = state.Map.GetTile(worldCoordinates2);
		TileData tile2 = state.Map.GetTile(worldCoordinates);
		unit.attacked = unit.attacked || (!unit.HasAbility(UnitAbility.Type.Dash, state, worldCoordinates) && unit.moved);
		unit.moved = unit.moved || ((Reason != MoveReason.Attack || !unit.HasAbility(UnitAbility.Type.Escape, state, worldCoordinates)) && Reason != MoveReason.Push);
		tile.unit = null;
		tile2.unit = unit;
		unit.coordinates = worldCoordinates;
		if (Path.Count > 1)
		{
			unit.SetUnitDirection(Path[1], worldCoordinates);
		}
		if (Reason != MoveReason.Push && unit.HasAbility(UnitAbility.Type.AutoFreeze, state))
		{
			state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, tile2.coordinates, 1, freezeUnits: true));
		}
		int sightRange = data.GetSightRange();
		foreach (WorldCoordinates item in Path)
		{
			TileData tile3 = state.Map.GetTile(item);
			ActionUtils.ExploreFromTile(state, playerState, tile3, sightRange, shouldUseActions: true);
		}
		if (unit.type == UnitData.Type.Bunny && tile2.improvement != null && !tile2.HasImprovement(ImprovementData.Type.City) && !tile2.HasImprovement(ImprovementData.Type.Ruin))
		{
			state.ActionStack.Add(new DestroyImprovementAction(base.PlayerId, tile2.coordinates));
		}
		bool flag = !data.IsAquatic() && !unit.HasAbility(UnitAbility.Type.Fly, state);
		if (tile2.IsWater && flag && tile2.HasImprovement(ImprovementData.Type.Port))
		{
			state.ActionStack.Add(new EmbarkAction(base.PlayerId, worldCoordinates));
		}
		else if (tile.IsWater && !tile2.IsWater && data.IsVehicle())
		{
			state.ActionStack.Add(new DisembarkAction(base.PlayerId, worldCoordinates));
		}
		if (tile2.improvement != null && state.GameLogicData.TryGetData(tile2.improvement.type, out var data2) && data2 != null && data2.HasAbility(ImprovementAbility.Type.Poison) && !playerState.HasTribeAbility(TribeAbility.Type.PoisonResist, state) && !unit.HasAbility(UnitAbility.Type.Fly, state))
		{
			state.ActionStack.Add(new PoisonUnitAction(base.PlayerId, worldCoordinates, worldCoordinates));
			state.ActionStack.Add(new AttackAction(base.PlayerId, worldCoordinates, worldCoordinates, 20, shouldMoveToTarget: false, AttackAction.AnimationType.None, 0));
		}
		if (!unit.HasFollower() || !state.TryGetUnit(unit.follower, out var unit2))
		{
			return;
		}
		if (MapDataExtensions.ChebyshevDistance(unit2.coordinates, worldCoordinates) > 1)
		{
			List<WorldCoordinates> path = state.GetPath(unit2.coordinates, worldCoordinates2, 10, unit);
			if (path != null && path.Count >= 2)
			{
				state.ActionStack.Add(new MoveAction(base.PlayerId, unit.follower, path));
			}
		}
		else
		{
			state.ActionStack.Add(new MoveAction(base.PlayerId, unit.follower, new List<WorldCoordinates>(2) { unit2.coordinates, unit2.coordinates }));
		}
	}

	private void ExecuteV28(GameState state)
	{
		if (!state.TryGetUnit(UnitId, out var unit) || !state.TryGetPlayer(base.PlayerId, out var playerState) || !state.GameLogicData.TryGetData(unit.type, out var data))
		{
			return;
		}
		WorldCoordinates worldCoordinates = Path[0];
		WorldCoordinates coordinates = Path[Path.Count - 1];
		TileData tile = state.Map.GetTile(coordinates);
		TileData tile2 = state.Map.GetTile(worldCoordinates);
		unit.attacked = unit.attacked || (!unit.HasAbility(UnitAbility.Type.Dash, state, worldCoordinates) && unit.moved);
		unit.moved = unit.moved || ((Reason != MoveReason.Attack || !unit.HasAbility(UnitAbility.Type.Escape, state, worldCoordinates)) && Reason != MoveReason.Push);
		tile.unit = null;
		tile2.unit = unit;
		unit.coordinates = worldCoordinates;
		if (Path.Count > 1)
		{
			unit.SetUnitDirection(Path[1], worldCoordinates);
		}
		if (Reason != MoveReason.Push && unit.HasAbility(UnitAbility.Type.AutoFreeze, state))
		{
			state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, tile2.coordinates, 1, freezeUnits: true));
		}
		int sightRange = data.GetSightRange();
		foreach (WorldCoordinates item in Path)
		{
			TileData tile3 = state.Map.GetTile(item);
			ActionUtils.ExploreFromTile(state, playerState, tile3, sightRange, shouldUseActions: true);
		}
		if (unit.type == UnitData.Type.Bunny && tile2.improvement != null && !tile2.HasImprovement(ImprovementData.Type.City) && !tile2.HasImprovement(ImprovementData.Type.Ruin))
		{
			state.ActionStack.Add(new DestroyImprovementAction(base.PlayerId, tile2.coordinates));
		}
		bool flag = !data.IsAquatic() && !unit.HasAbility(UnitAbility.Type.Fly, state);
		if (!tile.IsWater && tile2.IsWater && flag)
		{
			state.ActionStack.Add(new EmbarkAction(base.PlayerId, worldCoordinates));
		}
		else if (tile.IsWater && !tile2.IsWater && data.IsVehicle())
		{
			state.ActionStack.Add(new DisembarkAction(base.PlayerId, worldCoordinates));
		}
	}

	private void ExecuteV27(GameState state)
	{
		if (!state.TryGetUnit(UnitId, out var unit) || !state.TryGetPlayer(base.PlayerId, out var playerState) || !state.GameLogicData.TryGetData(unit.type, out var data))
		{
			return;
		}
		WorldCoordinates worldCoordinates = Path[0];
		WorldCoordinates coordinates = Path[Path.Count - 1];
		TileData tile = state.Map.GetTile(coordinates);
		TileData tile2 = state.Map.GetTile(worldCoordinates);
		unit.attacked = unit.attacked || (!unit.HasAbility(UnitAbility.Type.Dash, state, worldCoordinates) && unit.moved);
		unit.moved = (Reason != MoveReason.Attack || !unit.HasAbility(UnitAbility.Type.Escape, state, worldCoordinates)) && Reason != MoveReason.Push;
		tile.unit = null;
		tile2.unit = unit;
		unit.coordinates = worldCoordinates;
		if (Path.Count > 1)
		{
			unit.SetUnitDirection(Path[1], worldCoordinates);
		}
		if (Reason != MoveReason.Push && unit.HasAbility(UnitAbility.Type.AutoFreeze, state))
		{
			state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, tile2.coordinates, 1, freezeUnits: true));
		}
		int sightRange = data.GetSightRange();
		foreach (WorldCoordinates item in Path)
		{
			TileData tile3 = state.Map.GetTile(item);
			ActionUtils.ExploreFromTile(state, playerState, tile3, sightRange, shouldUseActions: true);
		}
		if (unit.type == UnitData.Type.Bunny && tile2.improvement != null && !tile2.HasImprovement(ImprovementData.Type.City) && !tile2.HasImprovement(ImprovementData.Type.Ruin))
		{
			state.ActionStack.Add(new DestroyImprovementAction(base.PlayerId, tile2.coordinates));
		}
		bool flag = !data.IsAquatic() && !unit.HasAbility(UnitAbility.Type.Fly, state);
		if (!tile.IsWater && tile2.IsWater && flag)
		{
			state.ActionStack.Add(new EmbarkAction(base.PlayerId, worldCoordinates));
		}
		else if (tile.IsWater && !tile2.IsWater && data.IsVehicle())
		{
			state.ActionStack.Add(new DisembarkAction(base.PlayerId, worldCoordinates));
		}
	}

	private void ExecuteV20(GameState state)
	{
		if (!state.TryGetUnit(UnitId, out var unit) || !state.TryGetPlayer(base.PlayerId, out var playerState) || !state.GameLogicData.TryGetData(unit.type, out var data))
		{
			return;
		}
		WorldCoordinates worldCoordinates = Path[0];
		WorldCoordinates coordinates = Path[Path.Count - 1];
		TileData tile = state.Map.GetTile(coordinates);
		TileData tile2 = state.Map.GetTile(worldCoordinates);
		unit.attacked = unit.attacked || (!unit.HasAbility(UnitAbility.Type.Dash, state, worldCoordinates) && unit.moved);
		unit.moved = Reason != MoveReason.Attack || !unit.HasAbility(UnitAbility.Type.Escape, state, worldCoordinates);
		tile.unit = null;
		tile2.unit = unit;
		unit.coordinates = worldCoordinates;
		if (Path.Count > 1)
		{
			unit.SetUnitDirection(Path[1], worldCoordinates);
		}
		if (Reason != MoveReason.Push && unit.HasAbility(UnitAbility.Type.AutoFreeze, state))
		{
			state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, tile2.coordinates, 1, freezeUnits: true));
		}
		int sightRange = data.GetSightRange();
		foreach (WorldCoordinates item in Path)
		{
			TileData tile3 = state.Map.GetTile(item);
			ActionUtils.ExploreFromTile(state, playerState, tile3, sightRange, shouldUseActions: true);
		}
		bool flag = !data.IsAquatic() && !unit.HasAbility(UnitAbility.Type.Fly, state);
		if (!tile.IsWater && tile2.IsWater && flag)
		{
			state.ActionStack.Add(new EmbarkAction(base.PlayerId, worldCoordinates));
		}
		else if (tile.IsWater && !tile2.IsWater && data.IsVehicle())
		{
			state.ActionStack.Add(new DisembarkAction(base.PlayerId, worldCoordinates));
		}
	}

	private void ExecuteV19(GameState state)
	{
		if (!state.TryGetUnit(UnitId, out var unit) || !state.TryGetPlayer(base.PlayerId, out var playerState) || !state.GameLogicData.TryGetData(unit.type, out var data))
		{
			return;
		}
		WorldCoordinates worldCoordinates = Path[0];
		WorldCoordinates coordinates = Path[Path.Count - 1];
		TileData tile = state.Map.GetTile(coordinates);
		TileData tile2 = state.Map.GetTile(worldCoordinates);
		unit.attacked = unit.attacked || (!unit.HasAbility(UnitAbility.Type.Dash, state) && unit.moved);
		unit.moved = Reason != MoveReason.Attack || !unit.HasAbility(UnitAbility.Type.Escape, state);
		tile.unit = null;
		tile2.unit = unit;
		unit.coordinates = worldCoordinates;
		if (Path.Count > 1)
		{
			unit.SetUnitDirection(Path[1], worldCoordinates);
		}
		if (Reason != MoveReason.Push && unit.HasAbility(UnitAbility.Type.AutoFreeze, state))
		{
			state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, tile2.coordinates, 1, freezeUnits: true));
		}
		int sightRange = data.GetSightRange();
		foreach (WorldCoordinates item in Path)
		{
			TileData tile3 = state.Map.GetTile(item);
			ActionUtils.ExploreFromTile(state, playerState, tile3, sightRange, shouldUseActions: true);
		}
		bool flag = !data.IsAquatic() && !unit.HasAbility(UnitAbility.Type.Fly, state);
		if (!tile.IsWater && tile2.IsWater && flag)
		{
			state.ActionStack.Add(new EmbarkAction(base.PlayerId, worldCoordinates));
		}
		else if (tile.IsWater && !tile2.IsWater && data.IsVehicle())
		{
			state.ActionStack.Add(new DisembarkAction(base.PlayerId, worldCoordinates));
		}
	}

	private void ExecuteV18(GameState state)
	{
		if (!state.TryGetUnit(UnitId, out var unit) || !state.TryGetPlayer(base.PlayerId, out var playerState) || !state.GameLogicData.TryGetData(unit.type, out var data))
		{
			return;
		}
		WorldCoordinates worldCoordinates = Path[0];
		WorldCoordinates coordinates = Path[Path.Count - 1];
		TileData tile = state.Map.GetTile(coordinates);
		TileData tile2 = state.Map.GetTile(worldCoordinates);
		unit.attacked = unit.attacked || (!unit.HasAbility(UnitAbility.Type.Dash, state) && unit.moved);
		unit.moved = true;
		tile.unit = null;
		tile2.unit = unit;
		unit.coordinates = worldCoordinates;
		if (Path.Count > 1)
		{
			unit.SetUnitDirection(Path[1], worldCoordinates);
		}
		if (Reason != MoveReason.Push && unit.HasAbility(UnitAbility.Type.AutoFreeze, state))
		{
			state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, tile2.coordinates, 1, freezeUnits: true));
		}
		int sightRange = data.GetSightRange();
		foreach (WorldCoordinates item in Path)
		{
			TileData tile3 = state.Map.GetTile(item);
			ActionUtils.ExploreFromTile(state, playerState, tile3, sightRange, shouldUseActions: true);
		}
		bool flag = !data.IsAquatic() && !unit.HasAbility(UnitAbility.Type.Fly, state);
		if (!tile.IsWater && tile2.IsWater && flag)
		{
			state.ActionStack.Add(new EmbarkAction(base.PlayerId, worldCoordinates));
		}
		else if (tile.IsWater && !tile2.IsWater && data.IsVehicle())
		{
			state.ActionStack.Add(new DisembarkAction(base.PlayerId, worldCoordinates));
		}
	}

	private void ExecuteV7(GameState state)
	{
		if (!state.TryGetUnit(UnitId, out var unit) || !state.TryGetPlayer(base.PlayerId, out var playerState) || !state.GameLogicData.TryGetData(unit.type, out var data))
		{
			return;
		}
		WorldCoordinates worldCoordinates = Path[0];
		WorldCoordinates coordinates = Path[Path.Count - 1];
		TileData tile = state.Map.GetTile(coordinates);
		TileData tile2 = state.Map.GetTile(worldCoordinates);
		unit.attacked = unit.attacked || (!unit.HasAbility(UnitAbility.Type.Dash, state) && unit.moved);
		tile.unit = null;
		tile2.unit = unit;
		unit.coordinates = worldCoordinates;
		if (Path.Count > 1)
		{
			unit.SetUnitDirection(Path[1], worldCoordinates);
		}
		if (unit.HasAbility(UnitAbility.Type.AutoFreeze, state))
		{
			state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, tile2.coordinates, 1, freezeUnits: true));
		}
		int sightRange = data.GetSightRange();
		foreach (WorldCoordinates item in Path)
		{
			TileData tile3 = state.Map.GetTile(item);
			ActionUtils.ExploreFromTile(state, playerState, tile3, sightRange, shouldUseActions: true);
		}
		bool flag = !data.IsAquatic() && !unit.HasAbility(UnitAbility.Type.Fly, state);
		if (!tile.IsWater && tile2.IsWater && flag)
		{
			state.ActionStack.Add(new EmbarkAction(base.PlayerId, worldCoordinates));
		}
		else if (tile.IsWater && !tile2.IsWater && flag)
		{
			state.ActionStack.Add(new DisembarkAction(base.PlayerId, worldCoordinates));
		}
	}

	private void ExecuteV6(GameState state)
	{
		if (!state.TryGetUnit(UnitId, out var unit) || !state.TryGetPlayer(base.PlayerId, out var playerState) || !state.GameLogicData.TryGetData(unit.type, out var data))
		{
			return;
		}
		WorldCoordinates worldCoordinates = Path[0];
		WorldCoordinates coordinates = Path[Path.Count - 1];
		TileData tile = state.Map.GetTile(coordinates);
		TileData tile2 = state.Map.GetTile(worldCoordinates);
		if (!unit.attacked && (unit.HasAbility(UnitAbility.Type.Dash, state) || !unit.moved))
		{
			List<WorldCoordinates> attackOptionsAtPosition = UnitDataExtensions.GetAttackOptionsAtPosition(state, base.PlayerId, worldCoordinates, data.GetRange());
			unit.attacked = attackOptionsAtPosition == null || attackOptionsAtPosition.Count == 0;
		}
		else
		{
			unit.attacked = true;
		}
		tile.unit = null;
		tile2.unit = unit;
		unit.coordinates = worldCoordinates;
		if (Path.Count > 1)
		{
			unit.SetUnitDirection(Path[1], worldCoordinates);
		}
		if (unit.HasAbility(UnitAbility.Type.AutoFreeze, state))
		{
			state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, tile2.coordinates, 1, freezeUnits: true));
		}
		int sightRange = data.GetSightRange();
		foreach (WorldCoordinates item in Path)
		{
			TileData tile3 = state.Map.GetTile(item);
			ActionUtils.ExploreFromTile(state, playerState, tile3, sightRange, shouldUseActions: true);
		}
		bool flag = !data.IsAquatic() && !unit.HasAbility(UnitAbility.Type.Fly, state);
		if (!tile.IsWater && tile2.IsWater && flag)
		{
			state.ActionStack.Add(new EmbarkAction(base.PlayerId, worldCoordinates));
		}
		else if (tile.IsWater && !tile2.IsWater && flag)
		{
			state.ActionStack.Add(new DisembarkAction(base.PlayerId, worldCoordinates));
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Move;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(UnitId);
		writer.Write(Path.Count);
		foreach (WorldCoordinates item in Path)
		{
			item.Serialize(writer, version);
		}
		writer.Write(ShouldAnimate);
		if (version > 17)
		{
			writer.Write((byte)Reason);
		}
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		UnitId = reader.ReadUInt32();
		int num = reader.ReadInt32();
		Path = new List<WorldCoordinates>(num);
		for (int i = 0; i < num; i++)
		{
			Path.Add(new WorldCoordinates(reader, version));
		}
		ShouldAnimate = reader.ReadBoolean();
		if (version > 17)
		{
			Reason = (MoveReason)reader.ReadByte();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, UnitId: {UnitId}, From: {Path[Path.Count - 1]}, To {Path[0]})";
	}
}
