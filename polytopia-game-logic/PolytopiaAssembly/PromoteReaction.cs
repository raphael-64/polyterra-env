using System;
using UnityEngine;

public class PromoteReaction : ReactionBase
{
	private readonly PromoteAction action;

	public PromoteReaction(PromoteAction action)
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
		if (Object.op_Implicit((Object)(object)tileInstance) && Object.op_Implicit((Object)(object)tileInstance.Unit) && !tileInstance.IsHidden)
		{
			tileInstance.SpawnSparkles();
			tileInstance.SpawnHalo();
			tileInstance.RenderUnit();
			AudioManager.PlaySFXAtTile(SFXTypes.Upgrade, tileInstance.Coordinates);
			onComplete();
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
