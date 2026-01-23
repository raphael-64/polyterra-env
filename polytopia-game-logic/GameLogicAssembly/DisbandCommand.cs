using System.IO;

public class DisbandCommand : CommandBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public DisbandCommand()
	{
	}

	public DisbandCommand(byte playerId, WorldCoordinates coordinates)
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
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		state.ActionStack.Add(new DisbandUnitAction(base.PlayerId, Coordinates));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.Disband;
	}

	public override bool ShouldAlwaysAskForConfirmation()
	{
		return true;
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
