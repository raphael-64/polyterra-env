using System;
using UnityEngine;

public class DestroyResourceReaction : ReactionBase
{
	private readonly DestroyResourceAction action;

	public DestroyResourceReaction(DestroyResourceAction action)
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
		Tile instance = GameManager.GameState.Map.GetTile(action.Coordinates).GetInstance();
		if (Object.op_Implicit((Object)(object)instance) && !instance.IsHidden)
		{
			instance.RenderResource();
			ResourceEvents.IncomeChanged(action.PlayerId);
			GameManager.DelayCall(10, onComplete);
		}
		else
		{
			ResourceEvents.IncomeChanged(action.PlayerId);
			onComplete();
		}
	}
}
