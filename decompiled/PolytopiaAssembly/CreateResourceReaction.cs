using System;

public class CreateResourceReaction : ReactionBase
{
	private readonly CreateResourceAction action;

	public CreateResourceReaction(CreateResourceAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		return true;
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.Coordinates;
	}

	public override void Execute(Action onComplete)
	{
		Tile instance = GameManager.GameState.Map.GetTile(action.Coordinates).GetInstance();
		if (!instance.IsHidden)
		{
			instance.Render();
			if (GameManager.IsPlayerViewing(action.PlayerId) && action.Reason == CreateResourceAction.CreateReason.Attract)
			{
				instance.SpawnShine();
				AudioManager.PlaySFXAtTile(SFXTypes.EnemyAction, instance.Coordinates);
				NotificationManager.Notify(Localization.Get("world.attract.sanctuary"));
			}
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
