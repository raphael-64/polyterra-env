using System;
using UnityEngine;

public class EatReaction : ReactionBase
{
	private readonly EatAction action;

	public EatReaction(EatAction action)
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
		if ((Object)(object)instance != (Object)null && (Object)(object)instance.Unit != (Object)null && instance.Unit.State.health == 0)
		{
			RenderHomeTile(instance.Unit);
			instance.Unit.Destroy();
		}
		if ((Object)(object)instance != (Object)null && instance.Data.IsBeingCaptured(GameManager.GameState))
		{
			instance.SpawnFire();
		}
		if ((Object)(object)instance != (Object)null && !instance.IsHidden)
		{
			instance.Render();
			instance.SpawnPuff();
			if (GameManager.IsPlayerLocal(action.PlayerId))
			{
				AudioManager.PlaySFXAtTile(SFXTypes.Spawn, tile.coordinates);
			}
			else
			{
				AudioManager.PlaySFXAtTile(SFXTypes.SpawnEnemy, tile.coordinates);
			}
			if ((Object)(object)instance.Unit != (Object)null)
			{
				RenderHomeTile(instance.Unit);
			}
			GameManager.DelayCall(100, onComplete);
		}
		else
		{
			onComplete();
		}
	}

	private void RenderHomeTile(Unit unit)
	{
		Tile tileInstance = MapRenderer.Current.GetTileInstance(unit.GetHomeTile());
		if ((Object)(object)tileInstance != (Object)null && !tileInstance.IsHidden)
		{
			tileInstance.RenderImprovement();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
