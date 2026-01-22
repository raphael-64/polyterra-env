public class StartMatchCommand : CommandBase
{
	public StartMatchCommand()
	{
	}

	public StartMatchCommand(byte playerId)
		: base(playerId)
	{
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		validationError = null;
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		state.ActionStack.Add(new StartMatchAction(base.PlayerId));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.StartMatch;
	}
}
