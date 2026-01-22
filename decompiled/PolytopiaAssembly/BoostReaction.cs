using System;
using UnityEngine;

public class BoostReaction : ReactionBase
{
	private readonly BoostAction action;

	public BoostReaction(BoostAction action)
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
		if (Object.op_Implicit((Object)(object)tileInstance) && Object.op_Implicit((Object)(object)tileInstance.Unit) && !tileInstance.IsHidden)
		{
			tileInstance.RenderUnit();
			tileInstance.SpawnShine();
			tileInstance.SpawnEmbers();
			GameManager.DelayCall(200, onComplete);
			AudioManager.PlaySFXAtTile(SFXTypes.Sword, tileInstance.Coordinates);
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
