using System;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class EnableTaskReaction : ReactionBase
{
	private readonly EnableTaskAction action;

	public EnableTaskReaction(EnableTaskAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (GameManager.IsPlayerViewing(action.PlayerId) && !GameManager.Client.IsRecap)
		{
			if (GameManager.GameState.GameLogicData.TryGetData(action.Type, out var taskData) && GameManager.GameState.TryGetPlayer(action.PlayerId, out var playerState))
			{
				List<string> localizedImprovements = new List<string>();
				foreach (ImprovementData improvementUnlock in taskData.improvementUnlocks)
				{
					localizedImprovements.Add(Localization.Get(improvementUnlock.displayName));
				}
				SpriteData.GetBuildingSprite(taskData.improvementUnlocks[0], playerState.skinType, playerState.GetTribeStyle(GameManager.GameState), delegate(string atlasName, string spriteName, Sprite sprite)
				{
					NotificationManager.Notify(Localization.Get("task.info", Localization.Get(taskData.description), LocalizationUtils.GetPrettyList(localizedImprovements)), Localization.Get("world.task.new"), sprite);
				});
			}
		}
		onComplete();
	}
}
