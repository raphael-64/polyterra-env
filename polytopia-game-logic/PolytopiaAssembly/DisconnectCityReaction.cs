using System;

public class DisconnectCityReaction : ReactionBase
{
	private readonly DisconnectCityAction action;

	public DisconnectCityReaction(DisconnectCityAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		GameManager.DelayCall(1000, onComplete);
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
