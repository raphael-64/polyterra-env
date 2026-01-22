using System.Collections.Generic;
using Polytopia.Data;

public static class CommandTriggerUIUtils
{
	public static bool TryShowNextCommandTrigger()
	{
		byte id = GameManager.LocalPlayer.Id;
		if (id != GameManager.GameState.CurrentPlayer)
		{
			return false;
		}
		if (GameManager.debugAutoPlayLocalPlayer)
		{
			return false;
		}
		if (!GameManager.GameState.TryGetPendingCommandTrigger(id, out var trigger))
		{
			return false;
		}
		if (trigger.type == CommandTriggerType.None)
		{
			return false;
		}
		ShowCommandTrigger(trigger);
		return true;
	}

	public static void ShowCommandTrigger(CommandTrigger commandTrigger)
	{
		GameManager.GameState.TryGetPlayer(GameManager.GameState.CurrentPlayer, out var playerState);
		switch (commandTrigger.type)
		{
		case CommandTriggerType.CityLevelUp:
			if (!PopupManager.IsPopupShowing<RewardPopup>())
			{
				RewardPopup rewardPopup2 = PopupManager.GetRewardPopup();
				rewardPopup2.RewardChoosenCallback = PerformCityRewardAction;
				if (GameManager.GameState.GameLogicData.TryGetData(ImprovementData.Type.City, out var data2))
				{
					TileData tile2 = GameManager.GameState.Map.GetTile(commandTrigger.coordinates);
					CityReward[] cityRewardsForLevel = data2.GetCityRewardsForLevel(tile2.improvement.level - 1);
					rewardPopup2.SetData(playerState, tile2, cityRewardsForLevel, RewardPopup.PopupType.CityLevelUp);
					rewardPopup2.Show();
					AudioManager.PlaySFX(SFXTypes.RewardStart);
				}
			}
			break;
		case CommandTriggerType.PeaceRequest:
		{
			if (PopupManager.IsPopupShowing<IconPopup>("peaceRequest"))
			{
				break;
			}
			GameManager.GameState.TryGetPlayer(commandTrigger.opponentId, out var opponent);
			WorldCoordinates currentCapitalCoordinates = opponent.GetCurrentCapitalCoordinates(GameManager.GameState);
			if (!opponent.IsAlive(GameManager.GameState))
			{
				GameManager.Client.SendCommand(new PeaceRequestResponseCommand(commandTrigger.playerId, commandTrigger.opponentId, accepted: false));
				break;
			}
			ReactionUtils.CameraFocusIfExplored(GameManager.LocalPlayer.Id, currentCapitalCoordinates, shouldNudgeToCenter: false, 0.8f, delegate
			{
				string linkedTribeNameWithSpace = opponent.GetLinkedTribeNameWithSpace(GameManager.GameState);
				IconPopup iconPopup = PopupManager.GetIconPopup();
				iconPopup.IsUnskippable = true;
				iconPopup.Header = Localization.Get("action.info.peacetreaty");
				iconPopup.Description = Localization.Get("diplomacy.peacetreaty.description", linkedTribeNameWithSpace);
				iconPopup.spriteHandle.Request(SpriteData.GetHeadSpriteAddresses(GameManager.GameState, opponent));
				iconPopup.SetTribeInfoButtons(TextType.Description);
				iconPopup.buttonData = new PopupBase.PopupButtonData[2]
				{
					new PopupBase.PopupButtonData("friendlist.new.reject", PopupBase.PopupButtonData.States.Alternative, delegate
					{
						GameManager.Client.SendCommand(new PeaceRequestResponseCommand(commandTrigger.playerId, commandTrigger.opponentId, accepted: false));
					}),
					new PopupBase.PopupButtonData("friendlist.new.accept", PopupBase.PopupButtonData.States.Selected, delegate
					{
						GameManager.Client.SendCommand(new PeaceRequestResponseCommand(commandTrigger.playerId, commandTrigger.opponentId, accepted: true));
					})
				};
				iconPopup.Show();
				AudioManager.PlaySFX(SFXTypes.TreatySend);
			});
			break;
		}
		case CommandTriggerType.Infiltrate:
			if (!PopupManager.IsPopupShowing<RewardPopup>())
			{
				RewardPopup rewardPopup = PopupManager.GetRewardPopup();
				rewardPopup.RewardChoosenCallback = PerformInfiltrationAction;
				if (GameManager.GameState.GameLogicData.TryGetData(ImprovementData.Type.City, out var _))
				{
					TileData tile = GameManager.GameState.Map.GetTile(commandTrigger.coordinates);
					CityReward[] infiltrateRewards = CityRewardData.infiltrateRewards;
					rewardPopup.SetData(playerState, tile, infiltrateRewards, RewardPopup.PopupType.Infiltrate);
					rewardPopup.Show();
					AudioManager.PlaySFX(SFXTypes.RewardStart);
				}
			}
			break;
		default:
			Log.Error("Unimplemented commandtriggertype {0}", new object[1] { commandTrigger.type });
			break;
		}
	}

	public static void PerformCityRewardAction(TileData tile, CityReward cityReward)
	{
		if (GameManager.IsPlayerViewing(GameManager.GameState.CurrentPlayer) && !GameManager.Client.IsRecap)
		{
			GameManager.GetAnalyticsManager().SendEvent("level_up_reward_choose", new Dictionary<string, object>
			{
				{
					"game_id",
					GameManager.Client.CurrentGameId
				},
				{ "type", cityReward }
			});
		}
		CityRewardCommand cityRewardCommand = new CityRewardCommand(GameManager.GameState.CurrentPlayer, cityReward, tile.coordinates);
		if (cityRewardCommand.IsValid(GameManager.GameState))
		{
			GameManager.Client.SendCommand(cityRewardCommand);
			AudioManager.PlaySFX(SFXTypes.RewardEnd);
		}
		else
		{
			TryShowNextCommandTrigger();
		}
	}

	public static void PerformInfiltrationAction(TileData tile, CityReward infiltrationReward)
	{
		if (GameManager.IsPlayerViewing(GameManager.GameState.CurrentPlayer) && !GameManager.Client.IsRecap)
		{
			GameManager.GetAnalyticsManager().SendEvent("infiltrate_reward_choose", new Dictionary<string, object>
			{
				{
					"game_id",
					GameManager.Client.CurrentGameId
				},
				{ "type", infiltrationReward }
			});
		}
		InfiltrateRewardCommand infiltrateRewardCommand = new InfiltrateRewardCommand(GameManager.GameState.CurrentPlayer, infiltrationReward, tile.coordinates);
		if (infiltrateRewardCommand.IsValid(GameManager.GameState))
		{
			GameManager.Client.SendCommand(infiltrateRewardCommand);
			AudioManager.PlaySFX(SFXTypes.RewardEnd);
		}
		else
		{
			TryShowNextCommandTrigger();
		}
	}
}
