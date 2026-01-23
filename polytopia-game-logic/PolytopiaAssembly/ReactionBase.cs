using System;

public class ReactionBase
{
	public virtual void Execute(Action onComplete)
	{
		onComplete?.Invoke();
	}

	public virtual bool ShouldFocusCamera()
	{
		return false;
	}

	public virtual WorldCoordinates GetCameraFocusCoordinates()
	{
		return WorldCoordinates.NULL_COORDINATES;
	}

	public virtual float GetCameraFocusSpeed()
	{
		return 0.8f;
	}

	public virtual bool ShouldNudgeToCenter()
	{
		return false;
	}

	public bool IsRecapOrOpponentAction(ActionBase action)
	{
		if (!GameManager.Client.IsRecap)
		{
			return !GameManager.IsPlayerViewing(action.PlayerId);
		}
		return true;
	}
}
