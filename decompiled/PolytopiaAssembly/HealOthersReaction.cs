using System;
using UnityEngine;

public class HealOthersReaction : ReactionBase
{
	private readonly HealOthersAction action;

	public HealOthersReaction(HealOthersAction action)
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
			AudioManager.PlaySFXAtTile(SFXTypes.Magic, tileInstance.Coordinates);
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
