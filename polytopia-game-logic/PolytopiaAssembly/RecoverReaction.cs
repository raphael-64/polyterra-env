using System;
using UnityEngine;

public class RecoverReaction : ReactionBase
{
	private readonly RecoverAction action;

	public RecoverReaction(RecoverAction action)
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
		Tile tileInstance = MapRenderer.Current.GetTileInstance(action.Coordinates);
		InputEvents.SelectionCleared();
		if (Object.op_Implicit((Object)(object)tileInstance) && Object.op_Implicit((Object)(object)tileInstance.Unit) && !tileInstance.IsHidden)
		{
			tileInstance.RenderUnit();
			GameManager.DelayCall(200, onComplete);
		}
		else
		{
			onComplete();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
