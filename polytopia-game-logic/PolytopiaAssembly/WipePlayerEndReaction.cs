using System;
using UnityEngine;

public class WipePlayerEndReaction : ReactionBase
{
	private readonly WipePlayerEndAction action;

	public WipePlayerEndReaction(WipePlayerEndAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (GameManager.Client.IsRecap || !GameManager.IsPlayerViewing(action.PlayerId))
		{
			onComplete();
			return;
		}
		for (int i = 0; i < GameManager.GameState.Map.Tiles.Length; i++)
		{
			Tile tileInstance = MapRenderer.Current.GetTileInstance(GameManager.GameState.Map.Tiles[i].coordinates);
			if ((Object)(object)tileInstance.Unit != (Object)null)
			{
				tileInstance.Unit.UpdateObject();
			}
		}
		onComplete();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
