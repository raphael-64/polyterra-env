using System;

public class IncreaseCurrencyReaction : ReactionBase
{
	private readonly IncreaseCurrencyAction action;

	public IncreaseCurrencyReaction(IncreaseCurrencyAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (!GameManager.IsPlayerViewing(action.PlayerId))
		{
			onComplete();
			return;
		}
		ResourceManager.AddResourceOfTypeToResourceBar(action.PlayerId, ResourceManager.Type.Currency, action.Amount, action.Source);
		GameManager.DelayCall(action.Delay, onComplete);
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
