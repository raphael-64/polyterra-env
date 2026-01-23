using System;

public class DecreaseScoreReaction : ReactionBase
{
	private readonly DecreaseScoreAction action;

	public DecreaseScoreReaction(DecreaseScoreAction action)
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
		ResourceManager.RemoveResourceOfType(action.PlayerId, ResourceManager.Type.Score, action.Amount);
		onComplete();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
