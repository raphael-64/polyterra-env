using System;
using UnityEngine;

public class ClimateChangeReaction : ReactionBase
{
	private readonly ClimateChangeAction action;

	public ClimateChangeReaction(ClimateChangeAction action)
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
		TileData tile = GameManager.GameState.Map.GetTile(action.Coordinates);
		Tile instance = tile.GetInstance();
		if (Object.op_Implicit((Object)(object)instance) && !instance.IsHidden)
		{
			instance.Render();
			instance.Sway();
			AudioManager.PlaySFXAtTile(SFXTypes.Transform, tile.coordinates);
			GameManager.DelayCall(FreezeTileAction.DEFAULT_TIME_MILLISECONDS, onComplete);
		}
		else
		{
			onComplete();
		}
	}
}
