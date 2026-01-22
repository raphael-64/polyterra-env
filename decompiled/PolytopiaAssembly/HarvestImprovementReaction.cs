using System;
using UnityEngine;

public class HarvestImprovementReaction : ReactionBase
{
	private readonly HarvestImprovementAction action;

	public HarvestImprovementReaction(HarvestImprovementAction action)
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
			instance.SpawnPuff();
			instance.SpawnSparkles();
			AudioManager.PlaySFXAtTile(SFXTypes.Plop, tile.coordinates);
			onComplete();
		}
		else
		{
			onComplete();
		}
	}
}
