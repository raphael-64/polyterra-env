using System;
using System.Collections.Generic;
using UnityEngine;

public class TrainReaction : ReactionBase
{
	private readonly TrainAction action;

	public TrainReaction(TrainAction action)
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
		if (GameManager.IsPlayerViewing(action.PlayerId) && !GameManager.Client.IsRecap)
		{
			GameManager.GetAnalyticsManager().SendEvent("unit_train", new Dictionary<string, object>
			{
				{
					"unit_type",
					action.Type.ToString().ToLowerInvariant()
				},
				{
					"game_id",
					GameManager.Client.CurrentGameId
				}
			});
		}
		Tile instance = GameManager.GameState.Map.GetTile(action.Coordinates).GetInstance();
		ResourceManager.RemoveResourceOfType(action.PlayerId, ResourceManager.Type.Currency, action.Cost);
		if (Object.op_Implicit((Object)(object)instance) && (Object)(object)instance.Unit != (Object)null && instance.Unit.State.health == 0)
		{
			Tile tileInstance = MapRenderer.Current.GetTileInstance(instance.Unit.GetHomeTile());
			if ((Object)(object)tileInstance != (Object)null && !tileInstance.IsHidden)
			{
				tileInstance.RenderImprovement();
			}
			instance.Unit.Destroy();
		}
		if (Object.op_Implicit((Object)(object)instance) && !instance.IsHidden)
		{
			ShowSpawn(instance, onComplete);
		}
		else
		{
			onComplete();
		}
	}

	private void ShowSpawn(Tile instance, Action onComplete)
	{
		instance.Render();
		instance.SpawnPuff();
		if (GameManager.IsPlayerLocal(action.PlayerId))
		{
			AudioManager.PlaySFXAtTile(SFXTypes.Spawn, instance.Coordinates);
		}
		else
		{
			AudioManager.PlaySFXAtTile(SFXTypes.SpawnEnemy, instance.Coordinates);
		}
		if (instance.Data.IsBeingCaptured(GameManager.GameState))
		{
			instance.SpawnFire();
		}
		if ((Object)(object)instance.Unit != (Object)null)
		{
			Tile tileInstance = MapRenderer.Current.GetTileInstance(instance.Unit.GetHomeTile());
			if ((Object)(object)tileInstance != (Object)null && !tileInstance.IsHidden)
			{
				tileInstance.RenderImprovement();
			}
		}
		GameManager.DelayCall(100, onComplete);
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
