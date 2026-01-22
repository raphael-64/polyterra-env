using System;
using UnityEngine;

public class BreakIceReaction : ReactionBase
{
	private readonly BreakIceAction action;

	public BreakIceReaction(BreakIceAction action)
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
			instance.Render();
			instance.SpawnPuff();
			AudioManager.PlaySFXAtTile(SFXTypes.BreakIce, action.Coordinates);
			GameManager.DelayCall(FreezeTileAction.DEFAULT_TIME_MILLISECONDS, onComplete);
		}
		else
		{
			onComplete();
		}
	}
}
