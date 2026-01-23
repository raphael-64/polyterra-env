using System;
using UnityEngine;

public class ConvertReaction : ReactionBase
{
	private readonly ConvertAction action;

	public ConvertReaction(ConvertAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		return IsRecapOrOpponentAction(action);
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.Target;
	}

	public override void Execute(Action onComplete)
	{
		Tile tileInstance = MapRenderer.Current.GetTileInstance(action.Origin);
		Tile tileInstance2 = MapRenderer.Current.GetTileInstance(action.Target);
		if (!tileInstance.IsHidden)
		{
			if (!tileInstance2.IsHidden)
			{
				tileInstance2.SpawnPuff();
				tileInstance2.SpawnSparkles();
				tileInstance2.Render();
				AudioManager.PlaySFXAtTile(SFXTypes.Magic, tileInstance2.Coordinates);
			}
			if (tileInstance2.Data.IsBeingCaptured(GameManager.GameState))
			{
				tileInstance2.SpawnFire();
			}
			else
			{
				tileInstance2.StopFire();
			}
			tileInstance.RenderUnit();
			Tile tileInstance3 = MapRenderer.Current.GetTileInstance(action.PreviousHomeTown);
			if (Object.op_Implicit((Object)(object)tileInstance3))
			{
				tileInstance3.Render();
			}
			Tile tileInstance4 = MapRenderer.Current.GetTileInstance(action.NewHomeTown);
			if (Object.op_Implicit((Object)(object)tileInstance4))
			{
				tileInstance4.Render();
			}
			onComplete();
		}
		else
		{
			if (tileInstance2.Data.IsBeingCaptured(GameManager.GameState))
			{
				tileInstance2.SpawnFire();
			}
			else
			{
				tileInstance2.StopFire();
			}
			onComplete();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
