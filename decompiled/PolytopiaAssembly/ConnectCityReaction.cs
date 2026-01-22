using System;
using UnityEngine;

public class ConnectCityReaction : ReactionBase
{
	private readonly ConnectCityAction action;

	public ConnectCityReaction(ConnectCityAction action)
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
		Tile instance = GameManager.GameState.Map.GetTile(action.Coordinates).GetInstance();
		if (Object.op_Implicit((Object)(object)instance) && !instance.IsHidden)
		{
			instance.SpawnShine();
			AudioManager.PlaySFXAtTile(SFXTypes.Connect, instance.Coordinates);
			GameManager.DelayCall(1000, onComplete);
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
