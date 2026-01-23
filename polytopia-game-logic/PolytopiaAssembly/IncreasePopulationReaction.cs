using System;

public class IncreasePopulationReaction : ReactionBase
{
	private readonly IncreasePopulationAction action;

	public IncreasePopulationReaction(IncreasePopulationAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		if (GameManager.Client.IsRecap)
		{
			return GameManager.IsPlayerViewing(action.PlayerId);
		}
		return false;
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.Target;
	}

	public override void Execute(Action onComplete)
	{
		if (!GameManager.IsPlayerViewing(action.PlayerId))
		{
			onComplete();
			return;
		}
		Tile tile = MapRenderer.Current.GetTileInstance(action.Target);
		ResourceManager.AddPopulationToCity(action.PlayerId, 1f, action.Source, tile.Improvement, delegate
		{
			tile.Sway();
			tile.SpawnSparkles(0.2f);
			AudioManager.PlaySFXAtTile(SFXTypes.GrowSmall, tile.Coordinates);
			tile.RenderImprovement();
			GameManager.DelayCall(action.Delay, onComplete);
		}, null, "Population Growth");
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
