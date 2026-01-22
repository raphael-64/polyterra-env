using System;
using System.Collections.Generic;
using Polytopia.Data;

public class TaskCompletedReaction : ReactionBase
{
	private readonly TaskCompletedAction action;

	public TaskCompletedReaction(TaskCompletedAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (action.PlayerId == GameManager.LocalPlayer.Id)
		{
			if (GameManager.Client.IsSpectating)
			{
				onComplete();
				return;
			}
			GameManager.GameState.GameLogicData.TryGetData(action.Type, out var data);
			GameManager.GameState.TryGetPlayer(action.PlayerId, out var playerState);
			GameManager.GetAnalyticsManager().SendEvent("task_completed", new Dictionary<string, object> { 
			{
				"task_name",
				data.type.ToString()
			} });
			IconPopup iconPopup = PopupManager.GetIconPopup();
			iconPopup.Header = Localization.Get("world.reward.building.title", Localization.Get(data.displayName));
			List<string> list = new List<string>();
			foreach (ImprovementData improvementUnlock in data.improvementUnlocks)
			{
				list.Add(Localization.Get(improvementUnlock.displayName));
			}
			iconPopup.Description = Localization.Get("world.reward.building", LocalizationUtils.GetPrettyList(list));
			iconPopup.spriteHandle.Request(SpriteData.GetBuildingSpriteAddresses(data.improvementUnlocks[0].type, playerState.skinType, playerState.GetTribeStyle(GameManager.GameState)));
			iconPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					onComplete();
				})
			};
			iconPopup.Show();
			AudioManager.PlaySFX(SFXTypes.Achievement);
		}
		else
		{
			onComplete();
		}
	}
}
