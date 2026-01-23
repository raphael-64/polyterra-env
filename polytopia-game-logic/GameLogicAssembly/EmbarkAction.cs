using System.IO;
using Polytopia.Data;

public class EmbarkAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public EmbarkAction()
	{
	}

	public EmbarkAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState gameState)
	{
		if (gameState.Version <= 6)
		{
			ExecuteV6(gameState);
		}
		else if (gameState.Version < 81)
		{
			ExecuteV26(gameState);
		}
		else if (gameState.Version < 83)
		{
			ExecuteV82(gameState);
		}
		else
		{
			ExecuteDefault(gameState);
		}
	}

	private void ExecuteDefault(GameState gameState)
	{
		if (gameState.TryGetPlayer(base.PlayerId, out var playerState))
		{
			TileData tile = gameState.Map.GetTile(Coordinates);
			UnitState unit = tile.unit;
			UnitData.Type type = UnitData.Type.Boat;
			if (unit.HasAbility(UnitAbility.Type.Hide, gameState))
			{
				type = UnitData.Type.Cloak_Boat;
			}
			if (unit.type == UnitData.Type.Dagger)
			{
				type = UnitData.Type.Pirate;
			}
			gameState.GameLogicData.TryGetData(type, out var data);
			UnitState unitState = ActionUtils.TrainUnit(gameState, playerState, tile, data);
			unitState.home = unit.home;
			unitState.health = unit.health;
			unitState.direction = unit.direction;
			unitState.flipped = unit.flipped;
			unitState.passengerUnit = unit;
			unitState.effects = unit.effects;
			unitState.attacked = true;
			unitState.moved = true;
		}
	}

	private void ExecuteV26(GameState state)
	{
		if (state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			TileData tile = state.Map.GetTile(Coordinates);
			UnitState unit = tile.unit;
			UnitData.Type type = UnitData.Type.Boat;
			if (unit.HasAbility(UnitAbility.Type.Hide, state))
			{
				type = UnitData.Type.Cloak_Boat;
			}
			state.GameLogicData.TryGetData(type, out var data);
			UnitState unitState = ActionUtils.TrainUnit(state, playerState, tile, data);
			unitState.home = unit.home;
			unitState.health = unit.health;
			unitState.direction = unit.direction;
			unitState.flipped = unit.flipped;
			unitState.passengerUnit = unit;
			unitState.effects = unit.effects;
			unitState.attacked = true;
			unitState.moved = true;
		}
	}

	private void ExecuteV6(GameState state)
	{
		if (state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			TileData tile = state.Map.GetTile(Coordinates);
			UnitState unit = tile.unit;
			unit.attacked = true;
			state.GameLogicData.TryGetData(UnitData.Type.Boat, out var data);
			UnitState unitState = ActionUtils.TrainUnit(state, playerState, tile, data);
			unitState.home = unit.home;
			unitState.health = unit.health;
			unitState.direction = unit.direction;
			unitState.flipped = unit.flipped;
			unitState.passengerUnit = unit;
		}
	}

	private void ExecuteV82(GameState gameState)
	{
		if (gameState.TryGetPlayer(base.PlayerId, out var playerState))
		{
			TileData tile = gameState.Map.GetTile(Coordinates);
			UnitState unit = tile.unit;
			UnitData.Type type = UnitData.Type.Boat;
			if (unit.HasAbility(UnitAbility.Type.Hide, gameState))
			{
				type = UnitData.Type.Cloak_Boat;
			}
			gameState.GameLogicData.TryGetData(type, out var data);
			UnitState unitState = ActionUtils.TrainUnit(gameState, playerState, tile, data);
			unitState.home = unit.home;
			unitState.health = unit.health;
			unitState.direction = unit.direction;
			unitState.flipped = unit.flipped;
			unitState.passengerUnit = unit;
			unitState.effects = unit.effects;
			unitState.attacked = true;
			unitState.moved = true;
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Embark;
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
