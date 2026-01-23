using System;
using System.IO;
using Polytopia.Data;

public class ConvertAction : ActionBase
{
	public WorldCoordinates Origin { get; private set; }

	public WorldCoordinates Target { get; private set; }

	public WorldCoordinates PreviousHomeTown { get; protected set; }

	public WorldCoordinates NewHomeTown { get; protected set; }

	public ConvertAction()
	{
	}

	public ConvertAction(byte playerId, WorldCoordinates origin, WorldCoordinates target)
		: base(playerId)
	{
		Origin = origin;
		Target = target;
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 42)
		{
			ExecuteV42(state);
		}
		if (state.Version < 50)
		{
			ExecuteV50(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	private void ExecuteDefault(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Origin);
		TileData tile2 = gameState.Map.GetTile(Target);
		UnitState unit = tile.unit;
		UnitState unit2 = tile2.unit;
		if (unit2.owner == base.PlayerId)
		{
			return;
		}
		gameState.TryGetPlayer(unit.owner, out var playerState);
		if (unit2.HasFollower() && gameState.TryGetUnit(unit2.follower, out var unit3) && unit3.owner != base.PlayerId)
		{
			gameState.ActionStack.Add(new ConvertAction(base.PlayerId, unit2.coordinates, unit3.coordinates));
		}
		if (unit2.HasLeader() && gameState.TryGetUnit(unit2.leader, out var unit4) && unit4.owner != base.PlayerId)
		{
			gameState.ActionStack.Add(new ConvertAction(base.PlayerId, unit2.coordinates, unit4.coordinates));
		}
		unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, gameState);
		unit.attacked = true;
		unit2.owner = unit.owner;
		unit2.moved = true;
		unit2.attacked = true;
		bool flag = unit2.HasAbility(UnitAbility.Type.Independent, gameState);
		if (!flag)
		{
			PreviousHomeTown = unit2.home;
			NewHomeTown = playerState.GetCurrentCapitalCoordinates(gameState);
			unit2.home = NewHomeTown;
		}
		if (unit2.passengerUnit != null)
		{
			unit2.passengerUnit.owner = unit.owner;
			if (!flag)
			{
				unit2.passengerUnit.home = NewHomeTown;
			}
		}
		if (gameState.GameLogicData.TryGetData(unit2.type, out var data))
		{
			ActionUtils.ExploreFromTile(gameState, playerState, tile2, data.GetSightRange(), shouldUseActions: true);
		}
	}

	private void ExecuteV50(GameState gameState)
	{
		Log.Verbose("[felix] Old Convert", Array.Empty<object>());
		TileData tile = gameState.Map.GetTile(Origin);
		TileData tile2 = gameState.Map.GetTile(Target);
		UnitState unit = tile.unit;
		UnitState unit2 = tile2.unit;
		gameState.TryGetPlayer(unit.owner, out var playerState);
		if (unit2.HasFollower() && gameState.TryGetUnit(unit2.follower, out var unit3))
		{
			gameState.ActionStack.Add(new ConvertAction(base.PlayerId, unit2.coordinates, unit3.coordinates));
		}
		if (unit != unit2)
		{
			unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, gameState);
			unit.attacked = true;
			unit2.moved = true;
			unit2.attacked = true;
		}
		unit2.owner = base.PlayerId;
		bool flag = unit2.HasAbility(UnitAbility.Type.Independent, gameState) || unit2.HasAbility(UnitAbility.Type.Agent, gameState);
		if (!flag)
		{
			PreviousHomeTown = unit2.home;
			NewHomeTown = playerState.GetCurrentCapitalCoordinates(gameState);
			unit2.home = NewHomeTown;
		}
		if (unit2.passengerUnit != null)
		{
			unit2.passengerUnit.owner = unit.owner;
			if (!flag)
			{
				unit2.passengerUnit.home = NewHomeTown;
			}
		}
		if (gameState.GameLogicData.TryGetData(unit2.type, out var data))
		{
			ActionUtils.ExploreFromTile(gameState, playerState, tile2, data.GetSightRange(), shouldUseActions: true);
		}
	}

	private void ExecuteV42(GameState state)
	{
		TileData tile = state.Map.GetTile(Origin);
		TileData tile2 = state.Map.GetTile(Target);
		UnitState unit = tile.unit;
		UnitState unit2 = tile2.unit;
		state.TryGetPlayer(unit.owner, out var playerState);
		if (unit2.HasFollower() && state.TryGetUnit(unit2.follower, out var unit3))
		{
			state.ActionStack.Add(new ConvertAction(base.PlayerId, unit2.coordinates, unit3.coordinates));
		}
		unit.moved = !unit.HasAbility(UnitAbility.Type.Escape, state);
		unit.attacked = true;
		unit2.owner = unit.owner;
		unit2.moved = true;
		unit2.attacked = true;
		PreviousHomeTown = unit2.home;
		NewHomeTown = playerState.GetCurrentCapitalCoordinates(state);
		unit2.home = NewHomeTown;
		if (unit2.passengerUnit != null)
		{
			unit2.passengerUnit.owner = unit.owner;
			unit2.passengerUnit.home = NewHomeTown;
		}
		if (state.GameLogicData.TryGetData(unit2.type, out var data))
		{
			ActionUtils.ExploreFromTile(state, playerState, tile2, data.GetSightRange(), shouldUseActions: true);
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Convert;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Origin.Serialize(writer, version);
		Target.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Origin = new WorldCoordinates(reader, version);
		Target = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Origin: {Origin}, Target: {Target})";
	}
}
