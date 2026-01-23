using System;

public class ResignReaction : ReactionBase
{
	private readonly ResignAction action;

	public ResignReaction(ResignAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (GameManager.GameState.Version < 93)
		{
			onComplete?.Invoke();
		}
		else if (action.ResignedPlayerId == GameManager.LocalPlayer.Id && !GameManager.Client.IsRecap)
		{
			InputEvents.SelectionCleared();
			if (action.WasKicked)
			{
				GameManager.GameState.TryGetPlayer(action.KickerPlayerId, out var playerState);
				ShowInGameKickedPopup(GameManager.Client.CurrentGameId.ToString(), GameManager.GameState.Settings.GameName, playerState?.UserName, onComplete);
			}
			else
			{
				ShowResignCompletePopup(onComplete);
			}
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

	public static void ShowResignCompletePopup(Action onComplete)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("onlineview.game.resigned");
		basicPopup.Description = Localization.Get("wcontroller.spectate.info");
		basicPopup.buttonData = new PopupBase.PopupButtonData[2]
		{
			new PopupBase.PopupButtonData("onlineview.game.delete", PopupBase.PopupButtonData.States.None, async delegate
			{
				Guid? gameId = GameManager.Client?.CurrentGameId;
				GameManager.ReturnToMenu();
				await GameManager.GetRemoteGameDataManager().DeleteGameAsync(gameId);
			}, -1, closesPopup: true, ColorConstants.redButtonColorStates),
			new PopupBase.PopupButtonData("buttons.stay", PopupBase.PopupButtonData.States.Selected, delegate
			{
				onComplete?.Invoke();
			})
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}

	public static void ShowInGameKickedPopup(string gameId, string gameName, string kickerName, Action onComplete)
	{
		BasicPopup kickedPopup = GetKickedPopup(gameId, gameName, kickerName);
		kickedPopup.Description = kickedPopup.Description + "\n" + Localization.Get("wcontroller.spectate.info");
		kickedPopup.buttonData = new PopupBase.PopupButtonData[2]
		{
			new PopupBase.PopupButtonData("onlineview.game.delete", PopupBase.PopupButtonData.States.None, async delegate
			{
				Guid? gameId2 = GameManager.Client?.CurrentGameId;
				GameManager.ReturnToMenu();
				await GameManager.GetRemoteGameDataManager().DeleteGameAsync(gameId2);
			}, -1, closesPopup: true, ColorConstants.redButtonColorStates),
			new PopupBase.PopupButtonData("buttons.stay", PopupBase.PopupButtonData.States.Selected, delegate
			{
				onComplete?.Invoke();
			})
		};
		ShowKickedPopupInternal(kickedPopup, gameId);
	}

	public static void ShowKickedPopup(string gameId, string gameName, string kickerName)
	{
		BasicPopup kickedPopup = GetKickedPopup(gameId, gameName, kickerName);
		kickedPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
			{
			})
		};
		ShowKickedPopupInternal(kickedPopup, gameId);
	}

	private static void ShowKickedPopupInternal(BasicPopup popup, string gameId)
	{
		if (PopupManager.TryGetOpenPopup<BasicPopup>(popup.identifier, out var openPopup))
		{
			openPopup.Hide();
		}
		if (PopupManager.TryGetOpenPopup<BasicPopup>("YOUR_TURN" + gameId, out openPopup))
		{
			openPopup.Hide();
		}
		popup.Show();
	}

	private static BasicPopup GetKickedPopup(string gameId, string gameName, string kickerName)
	{
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.IsUnskippable = true;
		basicPopup.identifier = "SKIP_NOTICE" + gameId;
		basicPopup.Header = Localization.Get("onlineview.kicked.title");
		if (string.IsNullOrEmpty(kickerName))
		{
			basicPopup.Description = Localization.Get("onlineview.kicked.automatic", gameName);
		}
		else
		{
			basicPopup.Description = Localization.Get("onlineview.kicked.info", kickerName, gameName);
		}
		return basicPopup;
	}
}
