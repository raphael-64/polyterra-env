using System;
using UnityEngine;

public class DestroyImprovementReaction : ReactionBase
{
	private readonly DestroyImprovementAction action;

	public DestroyImprovementReaction(DestroyImprovementAction action)
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
			instance.RenderImprovement();
			instance.RenderResource();
			instance.SpawnExplosion();
			AudioManager.PlaySFXAtTile(SFXTypes.Kill, tile.coordinates);
			ResourceEvents.IncomeChanged(action.PlayerId);
			onComplete();
		}
		else
		{
			ResourceEvents.IncomeChanged(action.PlayerId);
			onComplete();
		}
	}
}
