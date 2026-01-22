using System;
using System.Collections.Generic;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;

public class IngameMenuScreen : SettingsScreen
{
	public UITextButton resignButton;

	public UIImageButton favoriteButton;

	public override void Init()
	{
		base.Init();
		SetupResignButton();
		SetupFavoriteButton();
		CreateSaveAndExitContainer();
	}

	public override void Show(bool instant = false)
	{
		base.Show(instant);
		((Component)resignButton).gameObject.SetActive(ShouldShowResignButton());
		((Component)favoriteButton).gameObject.SetActive(ShouldShowFavoriteButton());
	}

	private bool ShouldShowResignButton()
	{
		if (GameManager.GameState != null && !GameManager.Client.IsSpectating && GameManager.GameState.Settings.GameType != GameType.PassAndPlay)
		{
			return GameManager.GameState.Settings.GameType != GameType.SinglePlayer;
		}
		return false;
	}

	private bool ShouldShowFavoriteButton()
	{
		if (GameManager.Client != null)
		{
			return GameManager.Client.IsReplay;
		}
		return false;
	}

	private void CreateSaveAndExitContainer()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		SettingsButtonContainer settingsButtonContainer = Object.Instantiate<SettingsButtonContainer>(singleButtonContainerPrefab, (Transform)(object)container);
		settingsButtonContainer.button.OnClicked += OnSaveAndExitClicked;
		settingsButtonContainer.button.Key = "settings.saveexit";
		totalHeight += settingsButtonContainer.rectTransform.sizeDelta.y;
	}

	private void OnSaveAndExitClicked(int id, BaseEventData eventData)
	{
		Log.Verbose("Save and exit to the start menu", Array.Empty<object>());
		GameManager.ReturnToMenu();
		GameManager.GetAnalyticsManager().SendEvent("menu_exit_click", new Dictionary<string, object>());
	}

	private void SetupResignButton()
	{
		((Component)resignButton).gameObject.SetActive(ShouldShowResignButton());
		resignButton.OnClicked += OnResignClicked;
		resignButton.Key = "onlineview.game.resign";
	}

	private void SetupFavoriteButton()
	{
		favoriteButton.OnClicked += OnFavoriteButtonClickedAsync;
		UpdateFavoriteButton();
	}

	private async void UpdateFavoriteButton()
	{
		if (ShouldShowFavoriteButton())
		{
			((Component)favoriteButton).gameObject.SetActive(true);
			GameSummaryViewModel currentReplaySummary = GameManager.GetReplaysManager().GetCurrentReplaySummary();
			if (currentReplaySummary != null)
			{
				UpdateFavoriteButtonImage(await GameManager.GetReplaysManager().IsFavoriteReplay(currentReplaySummary.GameId));
			}
		}
		else
		{
			((Component)favoriteButton).gameObject.SetActive(false);
		}
	}

	private void UpdateFavoriteButtonImage(bool isFavorite)
	{
		favoriteButton.LoadImage(isFavorite ? "favorite-enabled" : "favorite-disabled");
	}

	private void CreateResignContainer()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.GameState != null && !GameManager.Client.IsReplay)
		{
			GameType gameType = GameManager.GameState.Settings.GameType;
			if (gameType != GameType.PassAndPlay && gameType != GameType.SinglePlayer)
			{
				SettingsButtonContainer settingsButtonContainer = Object.Instantiate<SettingsButtonContainer>(singleButtonContainerPrefab, (Transform)(object)container);
				settingsButtonContainer.button.OnClicked += OnResignClicked;
				settingsButtonContainer.button.Key = "onlineview.game.resign";
				totalHeight += settingsButtonContainer.rectTransform.sizeDelta.y;
			}
		}
	}

	private void OnResignClicked(int id, BaseEventData eventData)
	{
		Log.Verbose("Trigger resign confirmation popup", Array.Empty<object>());
		ShowResignPopup();
	}

	public async void OnFavoriteButtonClickedAsync(int id, BaseEventData eventData)
	{
		GameSummaryViewModel summary = GameManager.GetReplaysManager().GetCurrentReplaySummary();
		if (summary == null)
		{
			return;
		}
		bool flag = await GameManager.GetReplaysManager().IsFavoriteReplay(summary.GameId);
		UpdateFavoriteButtonImage(!flag);
		favoriteButton.ButtonEnabled = false;
		ErrorCode? errorCode = await GameManager.GetReplaysManager().SetCachedFavoriteReplay(summary, !flag);
		if (!((Object)(object)favoriteButton == (Object)null))
		{
			favoriteButton.ButtonEnabled = true;
			UpdateFavoriteButton();
			if (errorCode.HasValue)
			{
				NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(errorCode.Value));
			}
		}
	}

	protected void ShowResignPopup()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("onlineview.game.resign.title");
		basicPopup.Description = Localization.Get("onlineview.game.resign.info");
		basicPopup.IsUnskippable = true;
		basicPopup.buttonData = new PopupBase.PopupButtonData[2]
		{
			new PopupBase.PopupButtonData("buttons.back"),
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, OnResignOkClickedAsync)
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}

	protected async void OnResignOkClickedAsync(int id, BaseEventData eventData)
	{
		UIManager.Instance.OnBack();
		NotificationManager.Alert(Localization.Get("onlineview.game.resigning"));
		_ = GameManager.Client.CurrentGameId;
		GameManager.Client.GetCurrentLocalPlayer();
		ServerResponse<ResponseViewModel> serverResponse = await GameManager.Client.ResignCurrentGameAsync();
		NotificationManager.HideAlert();
		if (serverResponse == null)
		{
			PopupManager.ShowErrorPopup(Localization.GetErrorMessage(ErrorCode.GameNotFound));
		}
		else if (!serverResponse.Success)
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
		else if (!((Object)(object)this == (Object)null) && GameManager.GameState.Version < 93)
		{
			GameManager.Client.ActionManager.Pause();
			ResignReaction.ShowResignCompletePopup(delegate
			{
				GameManager.Client.ActionManager.Resume();
			});
		}
	}

	private async void ResubscribeToGameAsync(Guid? gameId)
	{
		if (gameId.HasValue)
		{
			await PolytopiaBackendAdapter.Instance.SubscribeToGame(new SubscribeToGameBindingModel
			{
				GameId = gameId.Value,
				SubscriptionType = SubscriptionType.Game
			});
		}
	}
}
