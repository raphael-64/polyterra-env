using System.IO;

public class FreezeAreaCommand : CommandBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public FreezeAreaCommand()
	{
	}

	public FreezeAreaCommand(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		if (!PassesBasicValidation(state, out validationError))
		{
			return false;
		}
		UnitState unit = state.Map.GetTile(Coordinates).unit;
		if (unit == null)
		{
			validationError = CommandBase.VALIDATION_ERROR_UNIT_MISSING;
			return false;
		}
		if (!unit.CanFreezeArea(state))
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_FREEZE;
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.unit != null)
		{
			tile.unit.moved = true;
			tile.unit.attacked = true;
		}
		state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, Coordinates, 1, freezeUnits: true));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.FreezeArea;
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
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates})";
	}
}
