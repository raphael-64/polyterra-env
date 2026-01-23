using System;
using UnityEngine;

public class UpgradeReaction : ReactionBase
{
	private readonly UpgradeAction action;

	public UpgradeReaction(UpgradeAction action)
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
			instance.SpawnPuff();
			instance.Render();
			if (GameManager.IsPlayerLocal(action.PlayerId))
			{
				AudioManager.PlaySFXAtTile(SFXTypes.Spawn, tile.coordinates);
			}
			else
			{
				AudioManager.PlaySFXAtTile(SFXTypes.SpawnEnemy, tile.coordinates);
			}
			ResourceManager.RemoveResourceOfType(action.PlayerId, ResourceManager.Type.Currency, action.Cost);
			GameManager.DelayCall(200, onComplete);
		}
		else
		{
			ResourceManager.RemoveResourceOfType(action.PlayerId, ResourceManager.Type.Currency, action.Cost);
			onComplete();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
