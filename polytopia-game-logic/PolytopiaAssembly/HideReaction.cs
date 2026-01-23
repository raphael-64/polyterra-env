using System;
using UnityEngine;

public class HideReaction : ReactionBase
{
	private readonly HideAction action;

	public HideReaction(HideAction action)
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
		if (Object.op_Implicit((Object)(object)tileInstance) && !tileInstance.IsHidden)
		{
			tileInstance.RenderUnit();
			tileInstance.SpawnPuff();
			tileInstance.SpawnSparkles();
			GameManager.DelayCall(200, onComplete);
			AudioManager.PlaySFXAtTile(SFXTypes.Explore, tileInstance.Coordinates);
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
