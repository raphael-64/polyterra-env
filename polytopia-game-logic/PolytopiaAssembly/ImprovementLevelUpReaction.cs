using System;
using UnityEngine;

public class ImprovementLevelUpReaction : ReactionBase
{
	private readonly ImprovementLevelUpAction action;

	public ImprovementLevelUpReaction(ImprovementLevelUpAction action)
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
		ResourceManager.IncomeChanged(action.PlayerId);
		if ((Object)(object)instance != (Object)null && !instance.IsHidden)
		{
			instance.SpawnPuff();
			AudioManager.PlaySFXAtTile(SFXTypes.Plop, tile.coordinates);
			instance.RenderImprovement();
			GameManager.DelayCall(200, onComplete);
		}
		else
		{
			onComplete();
		}
	}
}
