using System.IO;

public class PromoteCommand : CommandBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public PromoteCommand()
	{
	}

	public PromoteCommand(byte playerId, WorldCoordinates coordinates)
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
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile == null || tile.unit == null)
		{
			validationError = CommandBase.VALIDATION_ERROR_UNIT_MISSING;
			return false;
		}
		if (!tile.unit.CanBePromoted(state))
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_PROMOTE;
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		state.ActionStack.Add(new PromoteAction(base.PlayerId, Coordinates));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.Promote;
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
}
