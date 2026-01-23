using System;

public class EndCommandReaction : ReactionBase
{
	private readonly EndCommandAction action;

	public EndCommandReaction(EndCommandAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		CommandType commandType = action.commandType;
		if ((commandType == CommandType.Attack || commandType == CommandType.Capture || commandType == CommandType.Destroy) && GameManager.Client.IsRecap)
		{
			GameManager.DelayCall(300, onComplete.Invoke);
		}
		else
		{
			onComplete();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} (CommandType: {action.commandType})";
	}
}
