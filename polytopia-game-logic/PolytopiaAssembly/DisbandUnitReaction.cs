using System;
using UnityEngine;

public class DisbandUnitReaction : ReactionBase
{
	private readonly DisbandUnitAction action;

	public DisbandUnitReaction(DisbandUnitAction action)
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
		if ((Object)(object)instance == (Object)null || instance.IsHidden)
		{
			if ((Object)(object)instance != (Object)null)
			{
				instance.StopFire();
			}
			RenderHomeTile();
			onComplete();
		}
		else
		{
			AudioManager.PlaySFXAtTile(SFXTypes.Plop, tile.coordinates);
			instance.Unit.Destroy();
			instance.RenderUnit();
			instance.SpawnPuff();
			instance.StopFire();
			RenderHomeTile();
			onComplete();
		}
	}

	private void RenderHomeTile()
	{
		Tile instance = GameManager.GameState.Map.GetTile(action.HomeCoordinates).GetInstance();
		if (Object.op_Implicit((Object)(object)instance) && !instance.IsHidden)
		{
			instance.RenderImprovement();
		}
	}
}
