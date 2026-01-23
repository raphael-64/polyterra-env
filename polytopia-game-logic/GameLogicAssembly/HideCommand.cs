using System.IO;

public class HideCommand : CommandBase
{
	public WorldCoordinates Coordinates { get; private set; }

	public HideCommand()
	{
	}

	public HideCommand(byte playerId, WorldCoordinates coordinates)
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
		if (!state.TryGetPlayer(base.PlayerId, out var _))
		{
			validationError = CommandBase.VALIDATION_ERROR_MISSING_PLAYER;
			return false;
		}
		return true;
	}

	public override void Execute(GameState gameState)
	{
		gameState.ActionStack.Add(new HideAction(base.PlayerId, Coordinates));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.Hide;
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
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates {Coordinates})";
	}
}
