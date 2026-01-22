using System;

public class PoisonUnitReaction : ReactionBase
{
	private readonly PoisonUnitAction action;

	public PoisonUnitReaction(PoisonUnitAction action)
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

	public override void Execute(Action onComplete)
	{
		Tile tileInstance = MapRenderer.Current.GetTileInstance(action.Origin);
		Tile tileInstance2 = MapRenderer.Current.GetTileInstance(action.Target);
		if (!tileInstance2.IsHidden)
		{
			if (!tileInstance.IsHidden)
			{
				tileInstance.RenderUnit();
			}
			tileInstance2.SpawnPoison();
			tileInstance2.Render();
			AudioManager.PlaySFXAtTile(SFXTypes.Parasite, tileInstance2.Coordinates);
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
