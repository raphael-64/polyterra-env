using System;
using UnityEngine;

public class HealReaction : ReactionBase
{
	private readonly HealAction action;

	public HealReaction(HealAction action)
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
		InputEvents.SelectionCleared();
		Tile tileInstance = MapRenderer.Current.GetTileInstance(action.Coordinates);
		if (Object.op_Implicit((Object)(object)tileInstance) && Object.op_Implicit((Object)(object)tileInstance.Unit) && !tileInstance.IsHidden && !tileInstance.Unit.IsInvisibleForLocalPlayer)
		{
			tileInstance.RenderUnit();
			tileInstance.Heal(action.HealAmount);
			GameManager.DelayCall(200, onComplete);
			AudioManager.PlaySFXAtTile(SFXTypes.Heal, tileInstance.Coordinates);
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
