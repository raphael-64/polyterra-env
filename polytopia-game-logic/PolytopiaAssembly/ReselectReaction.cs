using System;
using UnityEngine;

public class ReselectReaction : ReactionBase
{
	private readonly ReselectAction action;

	public ReselectReaction(ReselectAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (GameManager.Client.IsRecap)
		{
			onComplete();
			return;
		}
		Unit unitInstance = MapRenderer.Current.GetUnitInstance(action.UnitId);
		if ((Object)(object)unitInstance != (Object)null && unitInstance.IsInteractableByPlayer(GameManager.LocalPlayer.Id) && !GameManager.LocalPlayer.AutoPlay)
		{
			LevelManager.GetClientInteraction().SelectUnit(unitInstance);
		}
		onComplete();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
