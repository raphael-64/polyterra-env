using System;

public class ChangeCityConnectionReaction : ReactionBase
{
	private readonly ChangeCityConnectionAction action;

	public ChangeCityConnectionReaction(ChangeCityConnectionAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		onComplete();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
