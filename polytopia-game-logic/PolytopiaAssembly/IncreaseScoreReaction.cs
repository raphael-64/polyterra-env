using System;

public class IncreaseScoreReaction : ReactionBase
{
	private readonly IncreaseScoreAction action;

	public IncreaseScoreReaction(IncreaseScoreAction action)
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
		if (action.Source == WorldCoordinates.NULL_COORDINATES)
		{
			ResourceManager.AddResourceOfType(action.PlayerId, ResourceManager.Type.Score, action.Amount);
		}
		else
		{
			ResourceManager.AddResourceOfTypeToResourceBar(action.PlayerId, ResourceManager.Type.Score, action.Amount, action.Source);
		}
		GameManager.DelayCall(action.Delay, onComplete);
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
