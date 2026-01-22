using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class MoveCommand : CommandBase
{
	public uint UnitId { get; protected set; }

	public WorldCoordinates From { get; protected set; }

	public WorldCoordinates To { get; protected set; }

	public MoveCommand()
	{
	}

	public MoveCommand(byte playerId, UnitState unit, WorldCoordinates target)
		: base(playerId)
	{
		UnitId = unit.id;
		From = unit.coordinates;
		To = target;
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		if (!PassesBasicValidation(state, out validationError))
		{
			return false;
		}
		if (!state.TryGetPlayer(base.PlayerId, out var _))
		{
			validationError = CommandBase.VALIDATION_ERROR_MISSING_PLAYER;
			return false;
		}
		TileData tile = state.Map.GetTile(From);
		if (tile.unit == null || tile.unit.id != UnitId)
		{
			validationError = CommandBase.VALIDATION_ERROR_UNIT_MISSING;
			return false;
		}
		UnitState unit = tile.unit;
		if (!unit.CanMove())
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_MOVE;
			return false;
		}
		if (!state.GameLogicData.TryGetData(unit.type, out var _))
		{
			validationError = CommandBase.VALIDATION_ERROR_MISSING_UNIT_DATA;
			return false;
		}
		List<WorldCoordinates> path = state.GetPath(From, To, unit.GetMovement(state), unit);
		if (path == null || path.Count < 2)
		{
			validationError = CommandBase.VALIDATION_ERROR_INVALID_PATH;
			return false;
		}
		return true;
	}

	public override void Execute(GameState gameState)
	{
		base.Execute(gameState);
		if (!gameState.TryGetUnit(UnitId, out var unit) || !gameState.GameLogicData.TryGetData(unit.type, out var _))
		{
			return;
		}
		unit.moved = true;
		gameState.ActionStack.Add(new ReselectAction(base.PlayerId, UnitId));
		if (gameState.Map.GetTile(To).unit != null)
		{
			unit.moved = false;
			gameState.ActionStack.Add(new RevealAction(base.PlayerId, To));
			return;
		}
		List<WorldCoordinates> path = gameState.GetPath(From, To, unit.GetMovement(gameState), unit);
		if (path != null && path.Count >= 2)
		{
			gameState.ActionStack.Add(new MoveAction(base.PlayerId, UnitId, path));
			if (unit.HasAbility(UnitAbility.Type.Hide, gameState) && !unit.HasEffect(UnitEffect.Invisible))
			{
				gameState.ActionStack.Add(new HideAction(base.PlayerId, From));
			}
		}
	}

	public override CommandType GetCommandType()
	{
		return CommandType.Move;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		From.Serialize(writer, version);
		To.Serialize(writer, version);
		writer.Write(UnitId);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		From = new WorldCoordinates(reader, version);
		To = new WorldCoordinates(reader, version);
		UnitId = reader.ReadUInt32();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, UnitId: {UnitId}, From: {From}, To {To})";
	}
}
