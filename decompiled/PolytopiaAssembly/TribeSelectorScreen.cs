using System;
using System.Collections.Generic;
using DG.Tweening;
using Polytopia.Data;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class TribeSelectorScreen : UIScreenBase
{
	[Header("Tribe Selector Screen")]
	[SerializeField]
	protected VerticalLayoutGroup verticalList;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected PolytopiaScrollRect scrollRect;

	[SerializeField]
	protected TribeData.CategoryEnum[] categoryOrder;

	[SerializeField]
	protected GameObject blackOverlay;

	[SerializeField]
	protected MixerOverlay mixerOverlay;

	[SerializeField]
	protected Transform timerContainer;

	[SerializeField]
	protected RectTransform solidTop;

	[Header("Prefabs")]
	[SerializeField]
	protected TribeCategoryContainer tribeCategoryPrefab;

	[SerializeField]
	protected TribeEndContainer tribeEndPrefab;

	[SerializeField]
	protected GameSetupNameRow nameRowPrefab;

	[SerializeField]
	protected GameSetupInfoRow gameInfoPrefab;

	private List<TribeCategoryContainer> categoryContainers = new List<TribeCategoryContainer>();

	private SelectTribePopup popup;

	private RectTransform m_listRectTr;

	private TribeEndContainer endContainer;

	private PlayerButton lastSelectedPlayerButton;

	private bool isPurchaseMode;

	private bool alertShowing;

	private bool isWaitingForGameSummary;

	private TribeData selectedTribe;

	private TribeData selectedTribeMix;

	private SkinType selectedSkin;

	private Guid? gameId;

	private Guid? gameOwnerId;

	protected GameSummaryViewModel summaryViewModel;

	protected Guid lobbyId;

	public Action<TribeData.Type, SkinType, TribeData.Type, List<TribeData.Type>> onTribePicked;

	public Action onCancel;

	protected GameSetupNameRow gameSetupNameRow;

	protected int currPlayerIdx;

	protected GameSetupInfoRow gameSetupInfoRow;

	private Action<bool, string> purchasingInitCallback;

	private TribeData.Type currentPurchasingTribe;

	private SkinType currentPurchasingSkin;

	private int gameLogicDataVersion;

	private const float PURCHASE_PLING_TIMEOUT = 2f;

	private float lastPurchasePlingTime = -2f;

	protected static List<PlayerButton> mixButtons = new List<PlayerButton>();

	private RectTransform ListRectTr
	{
		get
		{
			if ((Object)(object)m_listRectTr == (Object)null)
			{
				m_listRectTr = ((Component)verticalList).GetComponent<RectTransform>();
			}
			return m_listRectTr;
		}
	}

	public override void Init()
	{
		CreateView();
		base.Init();
	}

	private async void OnEnable()
	{
		gameId = null;
		gameOwnerId = null;
		selectedTribe = null;
		selectedTribeMix = null;
		if (!PolytopiaBackendAdapter.Instance.IsConnected)
		{
			BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChanged;
		}
		if (!GameManager.GetPurchaseManager().IsInitialized)
		{
			SystemEvents.OnPurchaseManagerInitialized += OnPurchaseManagerInitialized;
		}
		SystemEvents.OnPurchaseManagerUpdated += OnUnlockChange;
		int num = ((GameManager.GameState != null) ? GameManager.GameState.Version : VersionManager.GameVersion);
		if (GameManager.PreliminaryGameSettings.GameType == GameType.Multiplayer && num < 90)
		{
			isWaitingForGameSummary = true;
			BackendEvents.OnReceivedGameState += OnRecievedGameState;
			BackendEvents.OnReceivedGameDeletion += OnRecievedGameDeletion;
			BackendEvents.OnReceivedPlayerResignation += OnRecievedPlayerResignation;
			if (GameManager.Client.CurrentGameId.HasValue && GameManager.Client.CurrentGameId.Value != Guid.Empty)
			{
				NetworkUtils.ShowLoader();
				gameId = GameManager.Client.CurrentGameId;
				await PolytopiaBackendAdapter.Instance.SubscribeToGame(new SubscribeToGameBindingModel
				{
					GameId = gameId.Value
				});
				await PolytopiaBackendAdapter.Instance.SubscribeToGameSummaries(new SubscribeToGameSummariesBindingModel
				{
					GameIds = new List<Guid> { gameId.Value }
				});
				ServerResponse<GameSummaryViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetGameSummaryViewModelByIdAsync(GameManager.Client.CurrentGameId.Value);
				NetworkUtils.HideLoader();
				if (!serverResponse.Success)
				{
					NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(serverResponse));
					return;
				}
				OnRecievedGameSummary(serverResponse.Data, StateUpdateReason.Unknown);
			}
			BackendEvents.OnReceivedGameSummary += OnRecievedGameSummary;
		}
		else
		{
			isWaitingForGameSummary = false;
		}
		StartSceneBg.Bright = false;
	}

	private void OnDisable()
	{
		isPurchaseMode = false;
		BackendEvents.OnReceivedGameState -= OnRecievedGameState;
		BackendEvents.OnReceivedGameSummary -= OnRecievedGameSummary;
		BackendEvents.OnReceivedGameDeletion -= OnRecievedGameDeletion;
		BackendEvents.OnReceivedPlayerResignation -= OnRecievedPlayerResignation;
		SystemEvents.OnPurchaseManagerInitialized -= OnPurchaseManagerInitialized;
		SystemEvents.OnPurchaseManagerUpdated -= OnUnlockChange;
		if (gameId.HasValue && gameId.Value != Guid.Empty)
		{
			PolytopiaBackendAdapter.Instance.UnsubscribeToGame(new UnsubscribeToGameBindingModel
			{
				GameId = gameId.Value
			}).WrapErrors();
		}
		if (alertShowing)
		{
			NotificationManager.HideAlert();
		}
		gameOwnerId = null;
		ClearPurchasingInitCallback();
		selectedTribe = null;
		selectedTribeMix = null;
		ClearTribeMixer();
		if ((Object)(object)popup != (Object)null && popup.IsShowing())
		{
			popup.Hide();
			popup = null;
		}
	}

	public void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			Log.Verbose("updating unlocks on return to focus", Array.Empty<object>());
			CheckForUnlockChange();
		}
	}

	private void Update()
	{
		if (purchasingInitCallback != null && GameManager.GetPurchaseManager().IsInitTimedOut())
		{
			purchasingInitCallback(arg1: false, PurchaseManager.GetDefaultInitErrorKey());
			purchasingInitCallback = null;
		}
	}

	public override void OnBack()
	{
		base.OnBack();
		if (((GameManager.GameState != null) ? GameManager.GameState.Version : VersionManager.GameVersion) >= 90)
		{
			onCancel?.Invoke();
			onCancel = null;
		}
	}

	public override void Show(bool instant = false)
	{
		base.Show(instant);
		((ScrollRect)scrollRect).verticalNormalizedPosition = 1f;
		RefreshBlackOverlay(null, isWaitingForGameSummary && !isPurchaseMode);
		RefreshItems();
		bool flag = GameManager.PreliminaryGameSettings.GameType == GameType.PassAndPlay && !isPurchaseMode;
		bool flag2 = (GameManager.PreliminaryGameSettings.GameType == GameType.Multiplayer || GameManager.PreliminaryGameSettings.GameType == GameType.Competitive) && !isPurchaseMode;
		((Component)gameSetupNameRow).gameObject.SetActive(flag);
		((Component)gameSetupInfoRow).gameObject.SetActive(flag || flag2);
		if (flag)
		{
			currPlayerIdx = 0;
			gameSetupNameRow.Name = GameManager.PreliminaryGameSettings.AlivePlayers[currPlayerIdx].GetName();
		}
		if (flag || flag2)
		{
			gameSetupInfoRow.Text = GetGameInfo();
		}
		if ((Object)(object)CurrentSelectable == (Object)null)
		{
			CurrentSelectable = ((categoryContainers.Count > 0) ? categoryContainers[0].GetButton(0) : null);
		}
		PolytopiaInput.Omnicursor.AffixToUIElement(((Component)categoryContainers[0].GetButton(0)).GetComponent<RectTransform>());
		OnScreenUpdated();
	}

	private void OnPurchaseManagerInitialized(bool success, string errorKey)
	{
		if (success)
		{
			SystemEvents.OnPurchaseManagerInitialized -= OnPurchaseManagerInitialized;
			OnUnlockChange();
		}
		if (purchasingInitCallback != null)
		{
			purchasingInitCallback(success, errorKey);
			purchasingInitCallback = null;
		}
	}

	private void ClearPurchasingInitCallback()
	{
		StopWaitingForPurchase();
		endContainer.StopWaitingForRestore();
		purchasingInitCallback = null;
	}

	public void SetPurchaseMode(bool value)
	{
		isPurchaseMode = value;
	}

	public void SetLobbyId(Guid lobbyId)
	{
		this.lobbyId = lobbyId;
	}

	public Guid GetLobbyId()
	{
		return lobbyId;
	}

	public void SetGameOwnerId(Guid ownerId)
	{
		gameOwnerId = ownerId;
	}

	private void OnBackendConnectionChanged(ConnectionStatus status)
	{
		if (PolytopiaBackendAdapter.Instance.IsConnected)
		{
			BackendEvents.OnBackendConnectionChanged -= OnBackendConnectionChanged;
			if ((Object)(object)gameSetupNameRow != (Object)null)
			{
				gameSetupNameRow.Name = AccountManager.Alias;
			}
		}
	}

	private void OnRecievedGameDeletion(Guid id)
	{
		Guid? guid = gameId;
		if (id == guid)
		{
			SerializationHelpers.FromByteArray<GameStateSummary>(summaryViewModel.GameSummaryData, out var result);
			string empty = string.Empty;
			empty = ((result == null) ? Localization.Get("onlineview.gamedeleted.unknowngame", empty) : result.GameName);
			if (!PopupManager.IsPopupShowing<GameInfoPopup>())
			{
				Log.Verbose("TribeSelectorScreen :: OnRecievedGameDeletion :: id: {0}", new object[1] { id });
				BasicPopup basicPopup = PopupManager.GetBasicPopup();
				basicPopup.Header = Localization.Get("onlineview.gamedeleted.title");
				basicPopup.Description = Localization.Get("onlineview.gamedeleted.info", empty);
				basicPopup.buttonData = new PopupBase.PopupButtonData[1]
				{
					new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, OnGameDeletedAccepted)
				};
				basicPopup.Show();
			}
		}
	}

	private void OnGameDeletedAccepted(int id, BaseEventData eventData)
	{
		OnBack();
	}

	private void OnRecievedPlayerResignation(PlayerResignedViewModel playerResignedViewModel)
	{
		if (playerResignedViewModel.Kicked && playerResignedViewModel.Resignee == AccountManager.PlayerAccountId)
		{
			Guid value = playerResignedViewModel.GameSummary.GameId;
			Guid? guid = gameId;
			if (value == guid)
			{
				OnBack();
			}
		}
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}

	private void OnRecievedGameState(Guid gameId, StateUpdateReason reason)
	{
		Guid? guid = this.gameId;
		if (gameId == guid)
		{
			RefreshItems();
		}
	}

	private void OnRecievedGameSummary(GameSummaryViewModel summary, StateUpdateReason reason)
	{
		if (summary?.GameId != gameId)
		{
			return;
		}
		summaryViewModel = summary;
		gameOwnerId = summary.OwnerId;
		SerializationHelpers.FromByteArray<GameStateSummary>(summary.GameSummaryData, out var result);
		if (result == null)
		{
			return;
		}
		PlayerSummaries playerSummaries = GameSummaryUtils.GetPlayerSummaries(result, summary);
		if (playerSummaries.Current != null)
		{
			bool flag = playerSummaries.IsLocalPlayerCurrent();
			bool hasChosenTribe = playerSummaries.Current.HasChosenTribe;
			bool showOverlay = !flag;
			string alertMessage = null;
			if (summary.State == GameSessionState.Started)
			{
				alertMessage = Localization.Get("gameitem.ready");
			}
			else if (!flag)
			{
				alertMessage = ((!hasChosenTribe || summary.State != GameSessionState.ReadyToStart) ? Localization.Get("gameitem.join.wait", playerSummaries.Current.UserName) : Localization.Get("gameitem.start.wait", playerSummaries.Current.UserName));
			}
			else if (hasChosenTribe && summary.State == GameSessionState.ReadyToStart)
			{
				alertMessage = Localization.Get("gameitem.start", playerSummaries.Current.UserName);
			}
			RefreshBlackOverlay(alertMessage, showOverlay);
		}
	}

	private void ShowTimer()
	{
		((Component)timerContainer).gameObject.SetActive(true);
		solidTop.SetHeight(30f);
		((LayoutGroup)verticalList).padding.top = 130;
	}

	private void HideTimer()
	{
		((Component)timerContainer).gameObject.SetActive(false);
		solidTop.SetHeight(0f);
		((LayoutGroup)verticalList).padding.top = 90;
	}

	private void RefreshBlackOverlay(string alertMessage = null, bool showOverlay = false)
	{
		if (!string.IsNullOrEmpty(alertMessage))
		{
			NotificationManager.Alert(alertMessage);
			alertShowing = true;
		}
		else
		{
			alertShowing = false;
			NotificationManager.HideAlert();
		}
		blackOverlay.gameObject.SetActive(showOverlay);
	}

	private void CreateView()
	{
		int num = categoryOrder.Length;
		gameSetupNameRow = Object.Instantiate<GameSetupNameRow>(nameRowPrefab, (Transform)(object)ListRectTr);
		gameSetupNameRow.HeaderKey = "gamesettings.yourname";
		gameSetupNameRow.inputDoneCallback = OnNameChanged;
		gameSetupInfoRow = Object.Instantiate<GameSetupInfoRow>(gameInfoPrefab, (Transform)(object)ListRectTr);
		gameLogicDataVersion = GetCurrentGameLogicDataVersion();
		GameLogicData gameLogicData = GetGameLogicData();
		for (int i = 0; i < num; i++)
		{
			TribeData.CategoryEnum categoryEnum = categoryOrder[i];
			if (categoryEnum != TribeData.CategoryEnum.Hidden)
			{
				TribeCategoryContainer tribeCategoryContainer = Object.Instantiate<TribeCategoryContainer>(tribeCategoryPrefab, (Transform)(object)ListRectTr);
				tribeCategoryContainer.Init();
				tribeCategoryContainer.selectCallback = TribeSelectorOnClicked;
				tribeCategoryContainer.longClickCallback = OnAnyTribeButtonLongClick;
				tribeCategoryContainer.SetCategory(categoryEnum);
				tribeCategoryContainer.SetTribesData(gameLogicData.GetTribes(categoryEnum));
				categoryContainers.Add(tribeCategoryContainer);
			}
		}
		endContainer = Object.Instantiate<TribeEndContainer>(tribeEndPrefab, (Transform)(object)ListRectTr);
		endContainer.restorePurchasesCallback = RestorePurchases;
		endContainer.randomTribeCallback = OnChooseRandomTribe;
		GameSettings gameSettings = GameManager.PreliminaryGameSettings;
		if ((gameSettings.GameType == GameType.Multiplayer || gameSettings.GameType == GameType.Competitive) && GameManager.GameState != null && GameManager.GameState.Settings != null)
		{
			gameSettings = GameManager.GameState.Settings;
		}
		endContainer.ButtonEnabled = gameLogicData.HaveValidTribesToPick(gameSettings);
		endContainer.SetRandomButtonVisible(!isPurchaseMode);
	}

	private void DestroyView()
	{
		if ((Object)(object)gameSetupNameRow != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)gameSetupNameRow).gameObject);
			gameSetupNameRow = null;
		}
		if ((Object)(object)gameSetupInfoRow != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)gameSetupInfoRow).gameObject);
			gameSetupInfoRow = null;
		}
		for (int i = 0; i < categoryContainers.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)categoryContainers[i]).gameObject);
		}
		categoryContainers.Clear();
		if ((Object)(object)endContainer != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)endContainer).gameObject);
			endContainer = null;
		}
	}

	private int GetCurrentGameLogicDataVersion()
	{
		if (GameManager.GameState == null)
		{
			return VersionManager.GameLogicDataVersion;
		}
		return VersionManager.GetGameLogicDataVersionFromGameVersion(GameManager.GameState.Version);
	}

	private GameLogicData GetGameLogicData()
	{
		return PolytopiaDataManager.GetGameLogicData(GetCurrentGameLogicDataVersion());
	}

	private string GetGameInfo()
	{
		GameSettings gameSettings = GameManager.PreliminaryGameSettings;
		int num = gameSettings.Players.Length;
		if (GameManager.GameState != null && GameManager.GameState.Settings != null)
		{
			num = GameManager.GameState.PlayerCount;
			gameSettings = GameManager.GameState.Settings;
		}
		string text = string.Empty;
		if (num > 0)
		{
			text = string.Format("{0}", Localization.Get("tribepicker.players", num));
		}
		text += string.Format("\n{0}", Localization.Get("tribepicker.maptype", Localization.Get(gameSettings.mapPreset.GetLocalizationName())));
		text += string.Format("\n{0}", Localization.Get("tribepicker.mapsize", gameSettings.MapSize * gameSettings.MapSize));
		text += string.Format("\n{0}", Localization.Get("tribepicker.gamemode", Localization.Get(GameModeUtils.GetTitle(gameSettings.RulesGameMode))));
		switch (gameSettings.RulesGameMode)
		{
		case GameMode.Perfection:
		case GameMode.Glory:
			text += $"\n{Localization.Get(GameModeUtils.GetDescription(gameSettings.RulesGameMode), LocalizationUtils.FormatNumber(gameSettings.rules.ScoreLimit))}";
			break;
		case GameMode.Domination:
		case GameMode.Might:
		case GameMode.Custom:
		case GameMode.Sandbox:
			text += $"\n{Localization.Get(GameModeUtils.GetDescription(gameSettings.RulesGameMode))}";
			break;
		}
		return text;
	}

	private void TribeSelectorOnClicked(TribeData.Type tribeType, TribeData.Type mixTribeType, PlayerButton playerButton)
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		if (!IsTribeInMixer(tribeType) && PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).TryGetData(tribeType, out var data))
		{
			selectedTribe = data;
			selectedSkin = GameManager.PreliminaryGameSettings.GetSelectedSkin(data.type);
			lastSelectedPlayerButton = playerButton;
			TribeData data2 = null;
			if (mixTribeType != TribeData.Type.None && mixTribeType != TribeData.Type.Nature && GameManager.GetPurchaseManager().IsTribeUnlocked(mixTribeType) && PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).TryGetData(mixTribeType, out data2))
			{
				selectedTribeMix = data2;
			}
			popup = PopupManager.GetSelectTribePopup();
			popup.GameOwnerId = gameOwnerId;
			popup.SetData(data, data2);
			popup.enabledChangedCallback = OnTribeEnabledChanged;
			popup.HideCallback = delegate
			{
				ClearPurchasingInitCallback();
			};
			SetPopupButtons(selectedTribe, selectedSkin);
			popup.SkinButtonCallback = SetButton;
			popup.Show(InputManager.GetInputPosition());
			ClearPurchasingInitCallback();
			AudioSource audioSource = AudioManager.GetAudioSource(AudioManager.AudioSourceTypes.Ambience);
			audioSource.volume = 0f;
			audioSource.clip = AudioManager.GetTribeAudio(data.type).ambienceClip;
			audioSource.Play();
			AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.Ambience, 1f, 0.6f, (Ease)1);
			AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.ThemeMusic, 0f, 0.6f, (Ease)1);
		}
	}

	private void SetPopupButtons(TribeData tribeData, SkinType skinType)
	{
		PurchaseManager purchaseManager = GameManager.GetPurchaseManager();
		bool flag = purchaseManager.IsTribeUnlocked(tribeData.type);
		bool flag2 = purchaseManager.IsSkinUnlocked(skinType);
		PopupBase.PopupButtonData popupButtonData = null;
		if (flag)
		{
			popup.Mode = SelectTribePopup.DisplayMode.Select;
			if (!isPurchaseMode)
			{
				if (flag2)
				{
					popupButtonData = new PopupBase.PopupButtonData("tribepicker.pick", PopupBase.PopupButtonData.States.Selected, OnSelectTribeMainButtonPressed, (int)tribeData.type, closesPopup: false);
				}
				else
				{
					popup.Mode = SelectTribePopup.DisplayMode.Purchase;
					popupButtonData = new PopupBase.PopupButtonData(purchaseManager.GetSkinPriceString(skinType), PopupBase.PopupButtonData.States.Selected, OnSelectTribeMainButtonPressed, (int)tribeData.type, closesPopup: false);
				}
			}
		}
		else
		{
			popup.Mode = SelectTribePopup.DisplayMode.Purchase;
			popupButtonData = ((flag2 && skinType == SkinType.Default) ? new PopupBase.PopupButtonData(purchaseManager.GetPriceString(tribeData.type), PopupBase.PopupButtonData.States.Selected, OnSelectTribeMainButtonPressed, (int)tribeData.type, closesPopup: false) : ((!flag2) ? new PopupBase.PopupButtonData(purchaseManager.GetSkinPriceString(skinType), PopupBase.PopupButtonData.States.Disabled, OnSelectTribeMainButtonPressed, (int)tribeData.type, closesPopup: false) : new PopupBase.PopupButtonData("tribepicker.pick", PopupBase.PopupButtonData.States.Disabled, OnSelectTribeMainButtonPressed, (int)tribeData.type, closesPopup: false)));
		}
		if (popupButtonData != null)
		{
			popup.buttonData = new PopupBase.PopupButtonData[2]
			{
				GetBackButtonData(),
				popupButtonData
			};
		}
		else
		{
			popup.buttonData = new PopupBase.PopupButtonData[1] { GetBackButtonData() };
		}
	}

	private PopupBase.PopupButtonData GetBackButtonData()
	{
		return new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.None, OnPickerClosed);
	}

	private void SetButton(SkinType skinType)
	{
		if (GameManager.GetPurchaseManager().IsSkinUnlocked(skinType))
		{
			lastSelectedPlayerButton.SetSkin(skinType);
		}
		SetPopupButtons(selectedTribe, skinType);
	}

	private void OnSelectTribeMainButtonPressed(int idx, BaseEventData eventData)
	{
		if ((Object)(object)popup == (Object)null)
		{
			return;
		}
		if (popup.Mode == SelectTribePopup.DisplayMode.Select)
		{
			selectedSkin = popup.SkinType;
			popup.Hide();
			popup = null;
			OnPickTribe(idx, eventData);
			return;
		}
		selectedSkin = popup.SkinType;
		PurchaseManager purchaseManager = GameManager.GetPurchaseManager();
		bool flag = purchaseManager.IsTribeUnlocked((TribeData.Type)idx);
		bool flag2 = purchaseManager.IsSkinUnlocked(selectedSkin);
		if (flag && !flag2)
		{
			OnGetSkinDLC(selectedSkin);
		}
		else
		{
			OnGetDLC((TribeData.Type)idx, eventData);
		}
	}

	private void CheckForUnlockChange()
	{
		GameManager.GetPurchaseManager().UpdateUnlockedProducts();
	}

	private void OnUnlockChange()
	{
		UnlockedTribesUpdated();
	}

	private void UnlockedTribesUpdated()
	{
		UpdateUnlockedTribesInSettings();
		RefreshItems();
		if ((Object)(object)popup != (Object)null && popup.IsShowing())
		{
			StopWaitingForPurchase();
		}
		if (Time.time - lastPurchasePlingTime > 2f)
		{
			AudioManager.PlaySFX(SFXTypes.Capture);
			lastPurchasePlingTime = Time.time;
		}
		if (isPurchaseMode)
		{
			OnBack();
		}
	}

	private void UpdateUnlockedTribesInSettings()
	{
		List<TribeData.Type> unlockedTribes = GameManager.GetPurchaseManager().GetUnlockedTribes();
		((GameManager.GameState != null) ? GameManager.GameState.Settings : GameManager.PreliminaryGameSettings).SetUnlockedTribes(unlockedTribes);
	}

	private void PerformAfterPurchasingInit(Action callback)
	{
		PurchaseManager purchaseManager = GameManager.GetPurchaseManager();
		if (purchaseManager.IsInitialized)
		{
			callback();
			return;
		}
		purchasingInitCallback = delegate(bool success, string errorKey)
		{
			if (success)
			{
				callback();
			}
			else
			{
				endContainer.StopWaitingForRestore();
				StopWaitingForPurchase();
				ShowPurchasingInitFailure(errorKey);
			}
		};
		purchaseManager.RetryInit();
	}

	private void PurchaseProduct(SkinType skinType)
	{
		PerformAfterPurchasingInit(delegate
		{
			GameManager.GetPurchaseManager().OnPurchaseProduct(skinType, PurchaseCompletion);
		});
		void PurchaseCompletion(bool success, IAPSkinProduct product, PurchaseFailureReason reason)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			Log.Verbose("completed purchase {0} {1}", new object[2] { success, reason });
			bool flag = (Object)(object)popup != (Object)null && popup.IsShowing();
			if (success && isPurchaseMode && flag)
			{
				popup.Hide();
				popup = null;
			}
			if (currentPurchasingSkin != SkinType.Default && (product == null || product.SkinType == currentPurchasingSkin))
			{
				StopWaitingForPurchase();
				if (!success && (Object)(object)((Component)this).gameObject != (Object)null && ((Component)this).gameObject.activeInHierarchy)
				{
					PurchaseManager.ShowPurchaseErrorPopup(reason);
				}
			}
		}
	}

	private void PurchaseProduct(TribeData.Type tribe)
	{
		PerformAfterPurchasingInit(delegate
		{
			GameManager.GetPurchaseManager().OnPurchaseProduct(tribe, PurchaseCompletion);
		});
		void PurchaseCompletion(bool success, IAPProduct product, PurchaseFailureReason reason)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			Log.Verbose("completed purchase {0} {1}", new object[2] { success, reason });
			bool flag = (Object)(object)popup != (Object)null && popup.IsShowing();
			if (success && isPurchaseMode && flag)
			{
				popup.Hide();
				popup = null;
			}
			if (currentPurchasingTribe != TribeData.Type.None && (product == null || product.tribeType == currentPurchasingTribe))
			{
				StopWaitingForPurchase();
				if (!success && (Object)(object)((Component)this).gameObject != (Object)null && ((Component)this).gameObject.activeInHierarchy)
				{
					PurchaseManager.ShowPurchaseErrorPopup(reason);
				}
			}
		}
	}

	private void RestorePurchases()
	{
		endContainer.StartWaitingForRestore();
		PerformAfterPurchasingInit(delegate
		{
			GameManager.GetPurchaseManager().RestorePurchases(RestoreCompletion);
		});
	}

	private void RestoreCompletion(bool success, int restoredTribesCount)
	{
		Log.Verbose("completed restore {0} count {1}", new object[2] { success, restoredTribesCount });
		endContainer.StopWaitingForRestore();
		if (!((Object)(object)((Component)this).gameObject == (Object)null) && ((Component)this).gameObject.activeInHierarchy)
		{
			if (!success)
			{
				PurchaseManager.ShowRestoreErrorPopup();
			}
			else
			{
				NotificationManager.Notify(Localization.Get("notifications.restorecomplete", restoredTribesCount));
			}
		}
	}

	private void ShowPurchasingInitFailure(string errorKey)
	{
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("purchasing.error.title");
		basicPopup.Description = Localization.Get(errorKey);
		basicPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		basicPopup.Show();
	}

	private void StartWaitingForPurchase(SkinType skinType)
	{
		if ((Object)(object)popup != (Object)null && popup.IsShowing())
		{
			popup.StartWaitingForPurchase();
		}
		currentPurchasingSkin = skinType;
	}

	private void StartWaitingForPurchase(TribeData.Type tribeType)
	{
		if ((Object)(object)popup != (Object)null && popup.IsShowing())
		{
			popup.StartWaitingForPurchase();
		}
		currentPurchasingTribe = tribeType;
	}

	private void StopWaitingForPurchase()
	{
		if ((Object)(object)popup != (Object)null && popup.IsShowing())
		{
			SetPopupButtons(selectedTribe, popup.SkinType);
			popup.StopWaitingForPurchase();
		}
		currentPurchasingTribe = TribeData.Type.None;
		currentPurchasingSkin = SkinType.Default;
	}

	private void OnGetSkinDLC(SkinType skinType)
	{
		StartWaitingForPurchase(skinType);
		PurchaseManager purchaseManager = GameManager.GetPurchaseManager();
		decimal? price = purchaseManager.GetPrice(skinType);
		if (price.HasValue)
		{
			string skinCurrency = purchaseManager.GetSkinCurrency(skinType);
			GameManager.GetAnalyticsManager().SendEvent("add_to_cart", new Dictionary<string, object>
			{
				{ "value", price },
				{ "currency", skinCurrency },
				{
					"items",
					$"[{{ \"item_name\": \"{skinType}\", \"item_category\": \"skin\" }}]"
				},
				{ "skin", skinType }
			});
		}
		PurchaseProduct(skinType);
		CancelTribeAudio();
	}

	private void OnGetDLC(TribeData.Type tribeType, BaseEventData eventData)
	{
		StartWaitingForPurchase(tribeType);
		PurchaseManager purchaseManager = GameManager.GetPurchaseManager();
		decimal? price = purchaseManager.GetPrice(tribeType);
		if (price.HasValue)
		{
			string currency = purchaseManager.GetCurrency(tribeType);
			GameManager.GetAnalyticsManager().SendEvent("add_to_cart", new Dictionary<string, object>
			{
				{ "value", price },
				{ "currency", currency },
				{
					"items",
					$"[{{ \"item_name\": \"{tribeType}\", \"item_category\": \"tribe\" }}]"
				},
				{ "tribe", tribeType }
			});
		}
		PurchaseProduct(tribeType);
		CancelTribeAudio();
	}

	private async void OnPickTribe(int idx, BaseEventData eventData)
	{
		GameManager.GetAnalyticsManager().SendEvent("tribe_pick", new Dictionary<string, object>
		{
			{
				"tribe",
				selectedTribe.type.ToString()
			},
			{
				"game_type",
				GameManager.PreliminaryGameSettings.GameType.ToString().ToLowerInvariant()
			}
		});
		if (GameManager.PreliminaryGameSettings.GameType == GameType.SinglePlayer)
		{
			GameManager.StartingTribe = selectedTribe.type;
			GameManager.StartingTribeMix = ((selectedTribeMix != null) ? selectedTribeMix.type : TribeData.Type.None);
			GameManager.StartingSkin = selectedSkin;
			if ((Object)(object)popup != (Object)null)
			{
				popup.ButtonsEnabled = false;
				popup.ButtonAnimationsEnabled = false;
			}
			UIManager.Instance.ShowScreen(UIConstants.Screens.GameSetup);
		}
		else if (GameManager.PreliminaryGameSettings.GameType == GameType.Multiplayer || GameManager.PreliminaryGameSettings.GameType == GameType.Competitive || GameManager.PreliminaryGameSettings.GameType == GameType.Matchmaking)
		{
			List<TribeData.Type> list = null;
			if (gameOwnerId == AccountManager.PlayerAccountId)
			{
				GameSettings gameSettings = GameManager.PreliminaryGameSettings;
				if (GameManager.GameState != null && GameManager.GameState.Settings != null)
				{
					gameSettings = GameManager.GameState.Settings;
				}
				list = gameSettings.disabledTribes;
			}
			if (((GameManager.GameState != null) ? GameManager.GameState.Version : VersionManager.GameVersion) >= 90)
			{
				onTribePicked?.Invoke(selectedTribe.type, selectedSkin, (selectedTribeMix != null) ? selectedTribeMix.type : TribeData.Type.None, list);
			}
			else
			{
				UIInputBlocker.IncreaseBlockerCount();
				NetworkUtils.ShowLoader();
				bool num = await GameManager.Client.PickTribe(selectedTribe.type, list);
				NetworkUtils.HideLoader();
				UIInputBlocker.DecreaseBlockerCount();
				if (num)
				{
					OnBack();
				}
			}
		}
		else if (GameManager.PreliminaryGameSettings.GameType == GameType.PassAndPlay)
		{
			GameManager.PreliminaryGameSettings.AlivePlayers[currPlayerIdx].tribe = selectedTribe.type;
			GameManager.PreliminaryGameSettings.AlivePlayers[currPlayerIdx].skinType = selectedSkin;
			GameManager.PreliminaryGameSettings.AlivePlayers[currPlayerIdx].knownTribe = true;
			currPlayerIdx++;
			if (currPlayerIdx < GameManager.PreliminaryGameSettings.AlivePlayers.Length)
			{
				UIBlackFader.FadeInOut(1f, delegate
				{
					Log.Verbose("Should refresh tribe list", Array.Empty<object>());
					gameSetupNameRow.Name = GameManager.PreliminaryGameSettings.AlivePlayers[currPlayerIdx].GetName();
				});
			}
			else
			{
				UIBlackFader.FadeIn(0.5f, delegate
				{
					OnStartPassAndplayGame();
				});
			}
		}
		CancelTribeAudio();
	}

	private void OnPickerClosed(int idx, BaseEventData eventData)
	{
		selectedTribeMix = null;
		GameManager.StartingTribeMix = TribeData.Type.None;
		CancelTribeAudio();
	}

	private void OnNameChanged(string newName)
	{
		GameManager.PreliminaryGameSettings.AlivePlayers[currPlayerIdx].profile.name = newName;
	}

	private void OnStartPassAndplayGame()
	{
		DOTween.KillAll(false);
		GameManager.Instance.CreateHotseatGame();
	}

	private void CancelTribeAudio()
	{
		AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 0f, 0.6f, (Ease)1);
		AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.Ambience, 0f, 0.6f, (Ease)1);
		AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.ThemeMusic, 1f, 0.6f, (Ease)1);
	}

	private void OnTribeEnabledChanged(int idx)
	{
		RefreshItems();
		CancelTribeAudio();
	}

	private void OnAnyTribeButtonLongClick(TribeData clickedTribe)
	{
		GameSettings gameSettings = GameManager.PreliminaryGameSettings;
		if (GameManager.GameState != null && GameManager.GameState.Settings != null)
		{
			gameSettings = GameManager.GameState.Settings;
		}
		if (!GameTypeUtils.ShouldAllowChangingDisabledTribes(gameSettings.GameType, gameOwnerId))
		{
			return;
		}
		bool flag = gameSettings.disabledTribes.Count > 0;
		List<TribeData> list = new List<TribeData>();
		TribeData.CategoryEnum[] array = categoryOrder;
		foreach (TribeData.CategoryEnum category in array)
		{
			list.AddRange(GetGameLogicData().GetTribes(category));
		}
		foreach (TribeData item in list)
		{
			if (item == clickedTribe)
			{
				gameSettings.EnableTribe(item.type);
			}
			else if (flag)
			{
				gameSettings.EnableTribe(item.type);
			}
			else
			{
				gameSettings.DisableTribe(item.type);
			}
		}
		gameSettings.SaveToDisk();
		RefreshItems();
		CancelTribeAudio();
	}

	private void RefreshItems()
	{
		if (gameLogicDataVersion != GetCurrentGameLogicDataVersion())
		{
			DestroyView();
			CreateView();
		}
		foreach (TribeCategoryContainer categoryContainer in categoryContainers)
		{
			categoryContainer.RefreshItems();
		}
		if ((Object)(object)endContainer != (Object)null)
		{
			GameSettings gameSettings = GameManager.PreliminaryGameSettings;
			if (gameSettings.GameType == GameType.Multiplayer && GameManager.GameState != null && GameManager.GameState.Settings != null)
			{
				gameSettings = GameManager.GameState.Settings;
			}
			UpdateUnlockedTribesInSettings();
			endContainer.ButtonEnabled = GetGameLogicData().HaveValidTribesToPick(gameSettings);
			endContainer.SetRandomButtonVisible(!isPurchaseMode);
		}
	}

	private async void OnChooseRandomTribe()
	{
		UpdateUnlockedTribesInSettings();
		GameSettings settings = ((GameManager.GameState != null) ? GameManager.GameState.Settings : GameManager.PreliminaryGameSettings);
		if (!GetGameLogicData().HaveValidTribesToPick(settings))
		{
			Log.Error("No valid tribe to pick, refreshing", Array.Empty<object>());
			RefreshItems();
			return;
		}
		selectedSkin = SkinType.Default;
		if (settings.GameType == GameType.SinglePlayer)
		{
			TribeData.Type tribeType = (GameManager.StartingTribe = GameStateUtils.GetRandomPickableTribe(VersionManager.GameVersion, settings, null));
			selectedSkin = settings.GetSelectedSkin(tribeType);
			GameManager.StartingSkin = selectedSkin;
			UIManager.Instance.ShowScreen(UIConstants.Screens.GameSetup);
		}
		else if (settings.GameType == GameType.Multiplayer || settings.GameType == GameType.Competitive || settings.GameType == GameType.Matchmaking)
		{
			List<TribeData.Type> list = null;
			if (gameOwnerId == AccountManager.PlayerAccountId)
			{
				if (GameManager.GameState != null && GameManager.GameState.Settings != null)
				{
					settings = GameManager.GameState.Settings;
				}
				list = settings.disabledTribes;
			}
			TribeData.Type randomPickableTribe2 = GameStateUtils.GetRandomPickableTribe(VersionManager.GameVersion, settings, null);
			selectedSkin = GameManager.PreliminaryGameSettings.GetSelectedSkin(randomPickableTribe2);
			if (((GameManager.GameState != null) ? GameManager.GameState.Version : VersionManager.GameVersion) >= 90)
			{
				onTribePicked?.Invoke(randomPickableTribe2, selectedSkin, TribeData.Type.None, list);
			}
			else
			{
				UIInputBlocker.IncreaseBlockerCount();
				NetworkUtils.ShowLoader();
				bool num = await GameManager.Client.PickTribe(randomPickableTribe2, list, selectedSkin);
				NetworkUtils.HideLoader();
				UIInputBlocker.DecreaseBlockerCount();
				if (num)
				{
					OnBack();
				}
			}
		}
		else if (settings.GameType == GameType.PassAndPlay)
		{
			TribeData.Type randomPickableTribe3 = GameStateUtils.GetRandomPickableTribe(VersionManager.GameVersion, settings, null);
			settings.AlivePlayers[currPlayerIdx].tribe = randomPickableTribe3;
			settings.AlivePlayers[currPlayerIdx].knownTribe = true;
			selectedSkin = settings.GetSelectedSkin(randomPickableTribe3);
			settings.AlivePlayers[currPlayerIdx].skinType = selectedSkin;
			currPlayerIdx++;
			if (currPlayerIdx < settings.AlivePlayers.Length)
			{
				UIBlackFader.FadeInOut(1f, delegate
				{
					Log.Verbose("Should refresh tribe list", Array.Empty<object>());
					gameSetupNameRow.Name = settings.AlivePlayers[currPlayerIdx].GetName();
				});
			}
			else
			{
				UIBlackFader.FadeIn(0.5f, delegate
				{
					OnStartPassAndplayGame();
				});
			}
		}
		NotificationManager.Notify(Localization.Get("tribepicker.categories.random.selected.text"), Localization.Get("tribepicker.categories.random.selected.title"));
	}

	public void StartMixingTribes()
	{
		mixerOverlay.Show(mixButtons[0].rectTransform, mixButtons[1].rectTransform, OnMixComplete);
	}

	public void StopMixingTribes()
	{
		mixerOverlay.Hide();
	}

	private void OnMixComplete()
	{
		if (mixButtons != null && mixButtons.Count >= 2)
		{
			TribeData.Type type = mixButtons[0].Tribe.type;
			TribeData.Type type2 = mixButtons[1].Tribe.type;
			PlayerButton playerButton = mixButtons[1];
			Log.Verbose($"Tribe Mixer :: Mix complete :: {type} with {type2}", Array.Empty<object>());
			ClearTribeMixer();
			TribeSelectorOnClicked(type2, type, playerButton);
			mixerOverlay.ShowFlash();
		}
	}

	public static void AddTribeToMixer(PlayerButton button)
	{
		if (!IsButtonInMixer(button) && mixButtons.Count < 2)
		{
			Log.Verbose($"Tribe Mixer :: Add Tribe to mixer: {button.Tribe.type}", Array.Empty<object>());
			mixButtons.Add(button);
		}
		if (mixButtons.Count >= 2)
		{
			Log.Verbose($"Tribe Mixer :: Start mixing {mixButtons[0].Tribe.type} with {mixButtons[1].Tribe.type}", Array.Empty<object>());
			(UIManager.Instance.GetScreen(UIConstants.Screens.TribeSelector) as TribeSelectorScreen).StartMixingTribes();
		}
	}

	public static void ClearTribeMixer()
	{
		if (mixButtons.Count > 0)
		{
			mixButtons.Clear();
			(UIManager.Instance.GetScreen(UIConstants.Screens.TribeSelector) as TribeSelectorScreen).StopMixingTribes();
		}
	}

	public static void MixedTribeButtonUp(PlayerButton button)
	{
		if (!SystemManager.ShouldUseTouchInterface() && mixButtons.Count >= 2)
		{
			ClearTribeMixer();
		}
		else if (SystemManager.ShouldUseTouchInterface())
		{
			mixButtons.Remove(button);
			if (mixButtons.Count < 2)
			{
				(UIManager.Instance.GetScreen(UIConstants.Screens.TribeSelector) as TribeSelectorScreen).StopMixingTribes();
			}
		}
	}

	public static bool IsButtonInMixer(PlayerButton button)
	{
		return mixButtons.Contains(button);
	}

	public static bool IsTribeInMixer(TribeData.Type tribeType)
	{
		foreach (PlayerButton mixButton in mixButtons)
		{
			if (mixButton.Tribe.type == tribeType)
			{
				return true;
			}
		}
		return false;
	}
}
