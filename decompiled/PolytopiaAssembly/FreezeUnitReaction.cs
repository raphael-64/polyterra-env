using System;
using UnityEngine;

public class FreezeUnitReaction : ReactionBase
{
	private readonly FreezeUnitAction action;

	public FreezeUnitReaction(FreezeUnitAction action)
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

	private void CommonAnimation(Action onComplete)
	{
		Tile tileInstance = MapRenderer.Current.GetTileInstance(action.Target);
		if (Object.op_Implicit((Object)(object)tileInstance) && Object.op_Implicit((Object)(object)tileInstance.Unit) && !tileInstance.IsHidden)
		{
			tileInstance.SpawnPuff();
			tileInstance.Damage(action.Damage);
			tileInstance.RenderUnit();
			AudioManager.PlaySFXAtTile(SFXTypes.Freeze, tileInstance.Coordinates);
		}
		onComplete();
	}

	public override void Execute(Action onComplete)
	{
		if (action.Origin != WorldCoordinates.NULL_COORDINATES)
		{
			Tile originTile = MapRenderer.Current.GetTileInstance(action.Origin);
			if (Object.op_Implicit((Object)(object)originTile) && Object.op_Implicit((Object)(object)originTile.Unit) && !originTile.IsHidden)
			{
				originTile.Unit.Attack(action.Target, moveToTarget: false, delegate
				{
					originTile.RenderUnit();
					CommonAnimation(onComplete);
				});
			}
			else
			{
				CommonAnimation(onComplete);
			}
		}
		else
		{
			Tile tileInstance = MapRenderer.Current.GetTileInstance(action.Target);
			if (Object.op_Implicit((Object)(object)tileInstance) && Object.op_Implicit((Object)(object)tileInstance.Unit) && !tileInstance.IsHidden)
			{
				tileInstance.RenderUnit();
				AudioManager.PlaySFXAtTile(SFXTypes.Freeze, tileInstance.Coordinates);
			}
			onComplete();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
