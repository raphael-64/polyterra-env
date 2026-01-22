using System;
using System.Collections.Generic;
using PolytopiaBackendBase.Game;

public class WipePlayerReaction : ReactionBase
{
	private readonly WipePlayerAction action;

	public WipePlayerReaction(WipePlayerAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (GameManager.IsPlayerViewing(action.TargetPlayerId) && GameManager.Client is ReplayClient replayClient && !replayClient.GetAutoSwitchPlayers() && action.TargetPlayerId == replayClient.GetCurrentLocalPlayer().Id)
		{
			replayClient.doAutoSwitchPlayers = true;
			UIEvents.ForceRefreshHud();
			onComplete();
			return;
		}
		if (GameManager.IsPlayerViewing(action.PlayerId) && GameManager.Client.IsRecap)
		{
			onComplete();
			return;
		}
		GameManager.GameState.TryGetPlayer(action.PlayerId, out var playerState);
		GameManager.GameState.TryGetPlayer(action.TargetPlayerId, out var playerState2);
		if (GameManager.Client.IsSpectating)
		{
			NotificationManager.Notify(Localization.Get("wcontroller.tribe.destroy2", playerState.GetLocalizedTribeName(GameManager.GameState), playerState2.GetLocalizedTribeName(GameManager.GameState)), Localization.Get("wcontroller.tribe.destroy.title"));
			onComplete();
			return;
		}
		IconPopup iconPopup = PopupManager.GetIconPopup();
		iconPopup.Header = Localization.Get("wcontroller.tribe.destroy.title");
		iconPopup.Description = GetDescription(playerState, playerState2);
		iconPopup.SetTribeInfoButtons(TextType.Description);
		iconPopup.spriteHandle.Request(SpriteData.GetHeadSpriteAddress("dead"));
		iconPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
			{
				if (GameManager.IsPlayerViewing(action.TargetPlayerId) && GameManager.GameState.Settings.GameType != GameType.SinglePlayer)
				{
					BasicPopup basicPopup = PopupManager.GetBasicPopup();
					basicPopup.Header = Localization.Get("wcontroller.game.lost.title");
					basicPopup.Description = Localization.Get("wcontroller.spectate.info");
					basicPopup.buttonData = new PopupBase.PopupButtonData[2]
					{
						new PopupBase.PopupButtonData("onlineview.game.delete", PopupBase.PopupButtonData.States.None, async delegate
						{
							onComplete();
							GameManager.GetAnalyticsManager().SendEvent("multiplayer_exit_click", new Dictionary<string, object> { 
							{
								"game_id",
								GameManager.Client.CurrentGameId
							} });
							Guid? gameId = GameManager.Client?.CurrentGameId;
							GameManager.ReturnToMenu();
							await GameManager.GetRemoteGameDataManager().DeleteGameAsync(gameId);
						}, -1, closesPopup: true, ColorConstants.redButtonColorStates),
						new PopupBase.PopupButtonData("buttons.stay", PopupBase.PopupButtonData.States.Selected, delegate
						{
							onComplete();
						})
					};
					basicPopup.Show();
				}
				else
				{
					onComplete();
				}
			})
		};
		iconPopup.Show();
	}

	private string GetDescription(PlayerState playerState, PlayerState targetPlayerState)
	{
		if (GameManager.IsPlayerViewing(action.PlayerId))
		{
			return Localization.Get("wcontroller.tribe.destroy", targetPlayerState.GetLinkedTribeNameWithSpace(GameManager.GameState));
		}
		return Localization.Get("wcontroller.tribe.destroy2", targetPlayerState.GetLinkedTribeNameWithSpace(GameManager.GameState), playerState.GetLinkedTribeNameWithSpace(GameManager.GameState));
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
