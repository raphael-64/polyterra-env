using System;

public class DecreasePopulationReaction : ReactionBase
{
	private readonly DecreasePopulationAction action;

	public DecreasePopulationReaction(DecreasePopulationAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		return true;
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.Target;
	}

	public override void Execute(Action onComplete)
	{
		Tile tileInstance = MapRenderer.Current.GetTileInstance(action.Target);
		if (!tileInstance.IsHidden)
		{
			tileInstance.RenderImprovement();
			tileInstance.Sway();
			AudioManager.PlaySFXAtTile(SFXTypes.Shrink, tileInstance.Coordinates);
			if (GameManager.IsPlayerViewing(action.PlayerId))
			{
				ResourceManager.RemoveResourceOfType(action.PlayerId, ResourceManager.Type.Score, ScoreSheet.cityXPScore, null, "Population Decrease Score");
			}
			GameManager.DelayCall(action.Delay, onComplete);
		}
		else
		{
			onComplete();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
