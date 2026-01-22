using System;
using UnityEngine;

public class BreakIceAreaReaction : ReactionBase
{
	private readonly BreakIceAreaAction action;

	public BreakIceAreaReaction(BreakIceAreaAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		Tile instance = GameManager.GameState.Map.GetTile(action.Coordinates).GetInstance();
		if (Object.op_Implicit((Object)(object)instance) && !instance.IsHidden)
		{
			instance.Render();
		}
		onComplete();
	}
}
