using System;
using UnityEngine;

public class BoostOthersReaction : ReactionBase
{
	private readonly BoostOthersAction action;

	public BoostOthersReaction(BoostOthersAction action)
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
		if (Object.op_Implicit((Object)(object)tileInstance) && !tileInstance.IsHidden)
		{
			tileInstance.SpawnShine();
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
