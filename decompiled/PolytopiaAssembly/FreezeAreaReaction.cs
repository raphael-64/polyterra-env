using System;
using UnityEngine;

public class FreezeAreaReaction : ReactionBase
{
	private readonly FreezeAreaAction action;

	public FreezeAreaReaction(FreezeAreaAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		if (IsRecapOrOpponentAction(action))
		{
			return action.frozenTiles > 0;
		}
		return false;
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.Coordinates;
	}

	public override void Execute(Action onComplete)
	{
		Tile instance = GameManager.GameState.Map.GetTile(action.Coordinates).GetInstance();
		if (Object.op_Implicit((Object)(object)instance) && !instance.IsHidden && action.frozenTiles > 0)
		{
			instance.Render();
			instance.SpawnShine();
			onComplete();
		}
		else
		{
			onComplete();
		}
	}
}
