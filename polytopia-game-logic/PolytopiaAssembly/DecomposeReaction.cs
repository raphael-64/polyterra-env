using System;

public class DecomposeReaction : ReactionBase
{
	private readonly DecomposeAction action;

	public DecomposeReaction(DecomposeAction action)
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
		Tile tileInstance = MapRenderer.Current.GetTileInstance(action.Coordinates);
		if (!tileInstance.IsHidden)
		{
			tileInstance.SpawnPoison();
			tileInstance.Render();
			AudioManager.PlaySFXAtTile(SFXTypes.Parasite, tileInstance.Coordinates);
			GameManager.DelayCall(100, onComplete);
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
