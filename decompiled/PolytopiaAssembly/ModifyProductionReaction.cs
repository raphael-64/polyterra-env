using System;
using UnityEngine;

public class ModifyProductionReaction : ReactionBase
{
	private readonly ModifyProductionAction action;

	public ModifyProductionReaction(ModifyProductionAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		Tile instance = GameManager.GameState.Map.GetTile(action.Source).GetInstance();
		if (Object.op_Implicit((Object)(object)instance) && !instance.IsHidden)
		{
			instance.Render();
		}
		ResourceManager.IncomeChanged(action.PlayerId);
		onComplete();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
