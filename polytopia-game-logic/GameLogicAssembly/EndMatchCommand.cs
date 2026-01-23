using System.IO;

public class EndMatchCommand : CommandBase
{
	public EndMatchCommand()
	{
	}

	public EndMatchCommand(byte playerId)
		: base(playerId)
	{
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		if (!PassesBasicValidation(state, out validationError))
		{
			return false;
		}
		return true;
	}

	public override bool NeedServerConfirmation()
	{
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		state.CurrentState = GameState.State.Ended;
		state.ActionStack.Add(new EndMatchAction());
	}

	public override CommandType GetCommandType()
	{
		return CommandType.EndMatch;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
	}
}
