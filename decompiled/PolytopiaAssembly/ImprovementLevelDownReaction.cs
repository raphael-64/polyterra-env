using System;
using UnityEngine;

public class ImprovementLevelDownReaction : ReactionBase
{
	private readonly ImprovementLevelDownAction action;

	public ImprovementLevelDownReaction(ImprovementLevelDownAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		return IsRecapOrOpponentAction(action);
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.Coordinates;
	}

	public override void Execute(Action onComplete)
	{
		ResourceManager.IncomeChanged(action.PlayerId);
		Tile instance = GameManager.GameState.Map.GetTile(action.Coordinates).GetInstance();
		if ((Object)(object)instance != (Object)null && !instance.IsHidden)
		{
			instance.SpawnPuff();
			instance.RenderImprovement();
			onComplete();
		}
		else
		{
			onComplete();
		}
	}
}
