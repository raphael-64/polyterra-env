using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polytopia.IO;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PullToRefresh;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MultiplayerScreen : UIScreenBase
{
	[Header("Multiplayer Screen")]
	[SerializeField]
	protected UIRefreshControl refresher;

	[SerializeField]
	protected RectTransform container;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected LayoutGroup containerLayoutGroup;

	[SerializeField]
	protected LoginDetails loginDetails;

	[Header("Multiplayer Selection Screen Connection")]
	[SerializeField]
	protected MultiplayerSelectionScreen multiplayerSelectionScreen;

	[Header("Prefabs")]
	[SerializeField]
	protected GameInfoRow gameInfoRowPrefab;

	[SerializeField]
	protected MatchmakingGameInfoRow matchmakingGameInfoRowPrefab;

	[SerializeField]
	protected LobbyGameInfoRow lobbyInfoRowPrefab;

	[SerializeField]
	protected HeaderRow headerRowPrefab;

	[SerializeField]
	protected MultiplayerInfoRow infoRowPrefab;

	[SerializeField]
	protected ButtonRow buttonRowPrefab;

	[SerializeField]
	protected MultiplayerRequirementRow requirementRowPrefab;

	private List<GameSummaryViewModel> hotSeatSummaryViewModels = new List<GameSummaryViewModel>();

	protected List<UIBasicButton> rows = new List<UIBasicButton>();

	protected List<GameObject> otherRows = new List<GameObject>();

	protected List<IListCellNavigation> navigableCells = new List<IListCellNavigation>();

	public override void Show(bool instant = false)
	{
		GameManager.Client?.Reset();
		multiplayerSelectionScreen.Show(instant: true);
		multiplayerSelectionScreen.UpdateScreenSelectionListSelectedIndex(UIConstants.Screens.MultiplayerScreen);
		base.Show(instant);
		if (PolytopiaBackendAdapter.Instance.IsConnected && GameManager.IsNetworkEnabled())
		{
			Reload();
		}
		else
		{
			AddStartMessages();
		}
		OnScreenUpdated();
		if (rows.Count > 0)
		{
			PolytopiaInput.Omnicursor.AffixToUIElement(((Component)rows[0].button).GetComponent<RectTransform>());
		}
	}

	public override void Hide(bool instant = false)
	{
		base.Hide(instant);
		multiplayerSelectionScreen.Hide();
	}

	protected void OnEnable()
	{
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChangedAsync;
		BackendEvents.OnGameSummariesUpdated += OnGameSummariesUpdated;
		BackendEvents.OnLobbiesUpdated += OnLobbiesUpdated;
		SystemEvents.OnMultiplayerEnabledUpdated += OnMultiplayerEnabledUpdated;
	}

	protected void OnDisable()
	{
		BackendEvents.OnBackendConnectionChanged -= OnBackendConnectionChangedAsync;
		BackendEvents.OnGameSummariesUpdated -= OnGameSummariesUpdated;
		BackendEvents.OnLobbiesUpdated -= OnLobbiesUpdated;
		SystemEvents.OnMultiplayerEnabledUpdated -= OnMultiplayerEnabledUpdated;
	}

	private async void OnBackendConnectionChangedAsync(ConnectionStatus status)
	{
		await new WaitForUpdate();
		if (!((Object)(object)this == (Object)null))
		{
			switch (status)
			{
			case ConnectionStatus.None:
			case ConnectionStatus.Connecting:
			case ConnectionStatus.Reconnecting:
			case ConnectionStatus.SocialConnected:
				AddStartMessages();
				break;
			case ConnectionStatus.Connected:
			case ConnectionStatus.Reconnected:
				Reload(forceReload: true);
				break;
			case ConnectionStatus.ConnectionFailed:
			case ConnectionStatus.Disconnected:
				AddStartMessages();
				break;
			}
		}
	}

	public void OnGameSummariesUpdated()
	{
		RefreshUI();
		if (!PopupManager.PopupShowing)
		{
			UINavigationManager.Select(GetCurrentSelectableOrFallback());
		}
	}

	public void OnLobbiesUpdated()
	{
		RefreshUI();
		if (!PopupManager.PopupShowing)
		{
			UINavigationManager.Select(GetCurrentSelectableOrFallback());
		}
	}

	public async void OnMultiplayerEnabledUpdated()
	{
		if (GameManager.IsMultiplayerEnabled && !PolytopiaBackendAdapter.Instance.IsConnected)
		{
			GameManager.GetLoginManager().Login();
			await new WaitForUpdate();
			AddStartMessages();
		}
		else
		{
			Reload();
		}
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}

	private void AddStartMessages()
	{
		if (!((Object)(object)this == (Object)null))
		{
			loginDetails.ClearList();
			ClearList();
			if (SystemManager.IsSteam && PolytopiaBackendAdapter.Instance.ConnectionStatus == ConnectionStatus.ConnectionFailed && !GameManager.HasSuccessfullyCheckedSteamNotifications)
			{
				AddMultiplayerLoadingMessage();
			}
			else if (!GameManager.IsMultiplayerEnabled)
			{
				loginDetails.ShowLoginDetails();
			}
			else
			{
				AddMultiplayerLoadingMessage();
			}
			AddHotSeatGames();
		}
	}

	public void SetScreenPadding(int topPadding)
	{
		containerLayoutGroup.padding.top = topPadding;
	}

	public async Task ReloadAsync(bool forceReload = false)
	{
		if (GameManager.IsNetworkEnabled())
		{
			await LoadGamesAsync(forceReload);
		}
	}

	public async void Reload(bool forceReload = false)
	{
		await ReloadAsync(forceReload);
	}

	public void RefreshUI()
	{
		BuildListAsync();
	}

	protected async Task LoadGamesAsync(bool forceReload = false)
	{
		if (!PolytopiaBackendAdapter.Instance.IsConnected)
		{
			AddStartMessages();
			return;
		}
		if (forceReload)
		{
			NetworkUtils.ShowLoader(Localization.Get("onlineview.reloading"), 0);
			await GameManager.GetLobbyManager().UpdateLobbies();
			await GameManager.GetRemoteGameDataManager().UpdateGameSummaries(ignoreError: false);
		}
		else
		{
			NetworkUtils.ShowLoader(Localization.Get("onlineview.loading.title"));
			if (!GameManager.GetLobbyManager().HasLoadedCache)
			{
				await GameManager.GetLobbyManager().UpdateLobbies();
			}
			if (!GameManager.GetRemoteGameDataManager().HasLoadedCache)
			{
				await GameManager.GetRemoteGameDataManager().UpdateGameSummaries(ignoreError: false);
			}
		}
		NetworkUtils.HideLoader();
		BuildListAsync();
	}

	private async void BuildListAsync()
	{
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		List<LobbyGameViewModel> lobbies = await GameManager.GetLobbyManager().GetLobbies();
		List<GameSummaryViewModel> list = await GameManager.GetRemoteGameDataManager().GetGameSummaryViewModels();
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		List<LobbyGameViewModel> list2 = new List<LobbyGameViewModel>();
		List<LobbyGameViewModel> list3 = new List<LobbyGameViewModel>();
		List<LobbyGameViewModel> list4 = new List<LobbyGameViewModel>();
		foreach (LobbyGameViewModel item in lobbies)
		{
			if (item.Participators == null)
			{
				continue;
			}
			foreach (ParticipatorViewModel participator in item.Participators)
			{
				if (participator.UserId == AccountManager.PlayerAccountId)
				{
					if (participator.InvitationState == PlayerInvitationState.Invited)
					{
						list2.Add(item);
					}
					else if (LobbyManager.CanBeStartedByPlayer(item, AccountManager.PlayerAccountId))
					{
						list3.Add(item);
					}
					else
					{
						list4.Add(item);
					}
					break;
				}
			}
		}
		list.Sort(SortGameSummaryByTimeLimit);
		List<GameSummaryViewModel> invitations = new List<GameSummaryViewModel>();
		List<GameSummaryViewModel> localTurns = new List<GameSummaryViewModel>();
		List<GameSummaryViewModel> list5 = new List<GameSummaryViewModel>();
		List<GameSummaryViewModel> list6 = new List<GameSummaryViewModel>();
		List<GameSummaryViewModel> list7 = new List<GameSummaryViewModel>();
		foreach (GameSummaryViewModel item2 in list)
		{
			if (item2.GameSummaryData == null)
			{
				Log.Warning($"Backend couldn't deserialize game {item2.GameId}", Array.Empty<object>());
				continue;
			}
			ParticipatorViewModel participatorViewModel = null;
			foreach (ParticipatorViewModel participator2 in item2.Participators)
			{
				if (participator2.UserId == AccountManager.PlayerAccountId)
				{
					participatorViewModel = participator2;
					break;
				}
			}
			if (SerializationHelpers.FromByteArray<GameStateSummary>(item2.GameSummaryData, out var result))
			{
				if (participatorViewModel != null && participatorViewModel.HasFailedParse)
				{
					PolytopiaBackendAdapter.Instance.SetParticipationFailedParseFireAndForget(new SetParticipationHasFailedParseBindingModel
					{
						GameId = item2.GameId,
						HasFailedParse = false
					});
				}
				PlayerSummaries playerSummaries = GameSummaryUtils.GetPlayerSummaries(result, item2);
				if (playerSummaries.Local == null)
				{
					continue;
				}
				PlayerInvitationState playerInvitationState = PlayerInvitationState.Unknown;
				foreach (ParticipatorViewModel participator3 in item2.Participators)
				{
					Guid userId = participator3.UserId;
					Guid? polytopiaId = playerSummaries.Local.PolytopiaId;
					if (userId == polytopiaId)
					{
						playerInvitationState = participator3.InvitationState;
					}
				}
				if (playerInvitationState == PlayerInvitationState.Invited)
				{
					invitations.Add(item2);
				}
				else if (item2.State == GameSessionState.Ended)
				{
					list6.Add(item2);
				}
				else if (playerInvitationState == PlayerInvitationState.Resigned || playerSummaries.Local.IsDead)
				{
					list7.Add(item2);
				}
				else if (playerSummaries.IsLocalPlayerCurrent())
				{
					localTurns.Add(item2);
				}
				else
				{
					list5.Add(item2);
				}
			}
			else if (participatorViewModel != null && !participatorViewModel.HasFailedParse)
			{
				PolytopiaBackendAdapter.Instance.SetParticipationFailedParseFireAndForget(new SetParticipationHasFailedParseBindingModel
				{
					GameId = item2.GameId,
					HasFailedParse = true
				});
			}
		}
		if (localTurns.Count > 1)
		{
			localTurns.Reverse();
		}
		ClearList();
		loginDetails.ClearList();
		if (!GameManager.IsMultiplayerEnabled)
		{
			loginDetails.ShowLoginDetails();
		}
		else if (!PolytopiaBackendAdapter.Instance.IsAuthenticated)
		{
			AddMultiplayerLoadingMessage();
		}
		else
		{
			if (invitations.Count > 0 || list2.Count > 0)
			{
				AddHeader("onlineview.gameinvitations", "-{0}-");
				AddLobbyRows(list2);
				AddGameRows(invitations);
			}
			if (localTurns.Count > 0 || list3.Count > 0)
			{
				AddHeader("onlineview.yourturn", "-{0}-");
				AddLobbyRows(list3);
				AddGameRows(localTurns);
			}
			if (list5.Count > 0 || list4.Count > 0)
			{
				AddHeader("onlineview.theirturn", "-{0}-");
				AddLobbyRows(list4);
				AddGameRows(list5);
			}
			if (list6.Count > 0)
			{
				AddHeader("multiplayer.finishedgames", "-{0}-");
				AddGameRows(list6);
			}
			if (list7.Count > 0)
			{
				AddHeader("onlineview.defeated", "-{0}-");
				AddGameRows(list7);
			}
			if (lobbies.Count + invitations.Count + localTurns.Count + list5.Count + list6.Count == 0)
			{
				MultiplayerInfoRow multiplayerInfoRow = AddInfoRow();
				((TMP_Text)multiplayerInfoRow.header).text = Localization.Get("onlineview.nogames.intro", AccountManager.Alias);
				if (AccountManager.GetFriendCount() > 0)
				{
					((TMP_Text)multiplayerInfoRow.description).text = Localization.Get("onlineview.nogames.start");
				}
				else
				{
					((TMP_Text)multiplayerInfoRow.description).text = Localization.Get("onlineview.nogames.first");
				}
			}
			await AddMatchmakingGamesAsync();
			AddMatchmakingButton();
		}
		AddHotSeatGames();
		if (rows.Count == 0)
		{
			CurrentSelectable = (Selectable)(object)multiplayerSelectionScreen.NewGameButton.button;
		}
		GameManager.ActionableGamesCount = invitations.Count + localTurns.Count;
		CurrentSelectable = multiplayerSelectionScreen.ScreenSelectionList.GetCurrentSelectable();
		await multiplayerSelectionScreen.UpdateFriendsBadgeAsync();
		UpdateNavigation();
		Log.Verbose("MultiplayerScreen refreshed", Array.Empty<object>());
	}

	private void UpdateNavigation()
	{
		if (!((Object)(object)multiplayerSelectionScreen == (Object)null))
		{
			Transform transform = ((Component)multiplayerSelectionScreen).transform;
			UIUtils.SetExplicitNavigation((RectTransform)(object)((transform is RectTransform) ? transform : null), useCenter: true);
			UIUtils.UpdateNavigationOnListCells(navigableCells, multiplayerSelectionScreen.ScreenSelectionList.GetCurrentSelectable(), (Selectable)(object)multiplayerSelectionScreen.FriendsButton.button);
			IListCellNavigation listCellNavigation = ((navigableCells.Count > 0) ? navigableCells[navigableCells.Count - 1] : null);
			if (listCellNavigation != null)
			{
				UIUtils.SetSelectOnUp((Selectable)(object)multiplayerSelectionScreen.NewGameButton.button, listCellNavigation.GetMainSelectable());
				UIUtils.SetSelectOnUp((Selectable)(object)multiplayerSelectionScreen.FriendsButton.button, listCellNavigation.GetMainSelectable());
				UIUtils.SetSelectOnUp((Selectable)(object)multiplayerSelectionScreen.ProfileButton.button, listCellNavigation.GetMainSelectable());
			}
		}
	}

	private void AddMultiplayerLoadingMessage()
	{
		if (!GameManager.IsNetworkEnabled())
		{
			MultiplayerInfoRow multiplayerInfoRow = AddInfoRow();
			((TMP_Text)multiplayerInfoRow.header).text = Localization.Get("onlineview.nogames.intro", AccountManager.Alias);
			((TMP_Text)multiplayerInfoRow.description).text = Localization.Get("onlineview.loaderror");
			return;
		}
		switch (PolytopiaBackendAdapter.Instance.ConnectionStatus)
		{
		case ConnectionStatus.Connecting:
		case ConnectionStatus.Reconnecting:
		{
			MultiplayerInfoRow multiplayerInfoRow3 = AddInfoRow();
			((TMP_Text)multiplayerInfoRow3.header).text = Localization.Get("onlineview.nogames.intro", AccountManager.Alias);
			((TMP_Text)multiplayerInfoRow3.description).text = Localization.Get("firebaseservice.status.loading");
			break;
		}
		case ConnectionStatus.ConnectionFailed:
		case ConnectionStatus.Disconnected:
		{
			MultiplayerInfoRow multiplayerInfoRow2 = AddInfoRow();
			((TMP_Text)multiplayerInfoRow2.header).text = Localization.Get("onlineview.nogames.intro", AccountManager.Alias);
			((TMP_Text)multiplayerInfoRow2.description).text = Localization.Get("onlineview.loaderror");
			AddButtonRow(Localization.Get("buttons.reconnect"), OnReconnectBackendAsync);
			break;
		}
		case ConnectionStatus.None:
		case ConnectionStatus.Connected:
		case ConnectionStatus.Reconnected:
			break;
		}
	}

	private void AddHotSeatGames()
	{
		hotSeatSummaryViewModels = GetHotSeatGames();
		if (hotSeatSummaryViewModels.Count > 0)
		{
			hotSeatSummaryViewModels.Sort((GameSummaryViewModel a, GameSummaryViewModel b) => b.DateLastCommand.GetValueOrDefault().CompareTo(a.DateLastCommand.GetValueOrDefault()));
			AddHeader("onlineview.passplay", "-{0}-");
			AddGameRows(hotSeatSummaryViewModels);
			return;
		}
		MultiplayerInfoRow multiplayerInfoRow = AddInfoRow();
		if (otherRows.Count > 1)
		{
			((TMP_Text)multiplayerInfoRow.header).text = Localization.Get("onlineview.or");
		}
		else
		{
			((TMP_Text)multiplayerInfoRow.header).text = "";
		}
		((TMP_Text)multiplayerInfoRow.description).text = Localization.Get("onlineview.passplay.start");
	}

	private async Task AddMatchmakingGamesAsync()
	{
		if (!GameManager.GetRemoteGameDataManager().HasMatchmakingSummaryData)
		{
			return;
		}
		AddHeader("onlineview.waiting", "-{0}-");
		foreach (MatchmakingGameSummaryViewModel item in await GameManager.GetRemoteGameDataManager().GetMatchMakingGameSummaryViewModels())
		{
			if (!item.LobbyId.HasValue)
			{
				AddMatchmakingGameRow(item);
			}
		}
	}

	private void AddMatchmakingButton()
	{
		MultiplayerInfoRow multiplayerInfoRow = AddInfoRow();
		((TMP_Text)multiplayerInfoRow.header).text = Localization.Get("onlineview.or");
		((TMP_Text)multiplayerInfoRow.description).text = Localization.Get("onlineview.randommatch.start");
		ButtonRow buttonRow = AddButtonRow();
		buttonRow.buttonComp.text = Localization.Get("onlineview.matchmaking");
		buttonRow.buttonComp.OnClicked += OnNewMatchmakingGame;
	}

	private async Task RefreshGamesAsync()
	{
		if (PolytopiaBackendAdapter.Instance.IsConnected)
		{
			await ReloadAsync(forceReload: true);
			NetworkUtils.ShowLoader(Localization.Get("onlineview.uptodate"), 0, 2f);
		}
		refresher.EndRefreshing();
	}

	protected void AddLobbyRows(List<LobbyGameViewModel> lobbies)
	{
		foreach (LobbyGameViewModel lobby in lobbies)
		{
			AddLobbyRow(lobby);
		}
	}

	protected void AddLobbyRow(LobbyGameViewModel lobby)
	{
		LobbyGameInfoRow lobbyGameInfoRow = Object.Instantiate<LobbyGameInfoRow>(lobbyInfoRowPrefab, (Transform)(object)container);
		lobbyGameInfoRow.SetData(lobby);
		if (rows.Count == 0)
		{
			CurrentSelectable = (Selectable)(object)lobbyGameInfoRow.button;
		}
		rows.Add(lobbyGameInfoRow);
		navigableCells.Add(lobbyGameInfoRow);
	}

	protected void AddGameRows(List<GameSummaryViewModel> summaries)
	{
		foreach (GameSummaryViewModel summary in summaries)
		{
			if (summary != null)
			{
				AddGameRow(summary);
			}
		}
	}

	protected void AddGameRow(GameSummaryViewModel summary)
	{
		GameInfoRow gameInfoRow = Object.Instantiate<GameInfoRow>(gameInfoRowPrefab, (Transform)(object)container);
		gameInfoRow.SessionState = summary.State;
		gameInfoRow.SetData(summary);
		if (rows.Count == 0)
		{
			CurrentSelectable = (Selectable)(object)gameInfoRow.button;
		}
		rows.Add(gameInfoRow);
		navigableCells.Add(gameInfoRow);
	}

	protected void AddMatchmakingGameRow(MatchmakingGameSummaryViewModel summary)
	{
		MatchmakingGameInfoRow matchmakingGameInfoRow = Object.Instantiate<MatchmakingGameInfoRow>(matchmakingGameInfoRowPrefab, (Transform)(object)container);
		matchmakingGameInfoRow.SetData(summary);
		if (rows.Count == 0)
		{
			CurrentSelectable = (Selectable)(object)matchmakingGameInfoRow.button;
		}
		rows.Add(matchmakingGameInfoRow);
		navigableCells.Add(matchmakingGameInfoRow);
	}

	protected void AddHeader(string headerKey, string format = null)
	{
		HeaderRow headerRow = Object.Instantiate<HeaderRow>(headerRowPrefab, (Transform)(object)container);
		headerRow.label.format = format;
		headerRow.label.Key = headerKey;
		otherRows.Add(((Component)headerRow).gameObject);
	}

	protected void AddHeader(string header)
	{
		HeaderRow headerRow = Object.Instantiate<HeaderRow>(headerRowPrefab, (Transform)(object)container);
		headerRow.label.Text = header;
		otherRows.Add(((Component)headerRow).gameObject);
	}

	protected MultiplayerInfoRow AddInfoRow()
	{
		MultiplayerInfoRow multiplayerInfoRow = Object.Instantiate<MultiplayerInfoRow>(infoRowPrefab, (Transform)(object)container);
		otherRows.Add(((Component)multiplayerInfoRow).gameObject);
		return multiplayerInfoRow;
	}

	protected ButtonRow AddButtonRow()
	{
		ButtonRow buttonRow = Object.Instantiate<ButtonRow>(buttonRowPrefab, (Transform)(object)container);
		otherRows.Add(((Component)buttonRow).gameObject);
		navigableCells.Add(buttonRow);
		return buttonRow;
	}

	protected ButtonRow AddButtonRow(string buttonText, UIButtonBase.ButtonAction action)
	{
		ButtonRow buttonRow = AddButtonRow();
		buttonRow.buttonComp.text = buttonText;
		buttonRow.buttonComp.OnClicked += action;
		return buttonRowPrefab;
	}

	protected void ClearList()
	{
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		foreach (UIBasicButton row in rows)
		{
			Object.Destroy((Object)(object)((Component)row).gameObject);
		}
		foreach (GameObject otherRow in otherRows)
		{
			Object.Destroy((Object)(object)otherRow);
		}
		rows.Clear();
		otherRows.Clear();
		navigableCells.Clear();
	}

	public void OnRefreshTrigger()
	{
		if (PolytopiaBackendAdapter.Instance.IsConnected)
		{
			NetworkUtils.ShowLoader(Localization.Get("onlineview.reloading.release"), 0);
		}
	}

	public void OnRefreshCancelled()
	{
		if (!multiplayerSelectionScreen.IsConnectingBannerShowing)
		{
			NetworkUtils.HideLoader();
		}
	}

	public async void OnRefreshGames()
	{
		await RefreshGamesAsync();
	}

	public async void OnReconnectBackendAsync(int buttonId, BaseEventData eventData)
	{
		GameManager.GetLoginManager().Login(silent: false);
		await new WaitForUpdate();
		AddStartMessages();
	}

	public void OnNewMatchmakingGame(int id, BaseEventData eventData)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (!GameManager.GetVersioningInfoHolder().IsNewMatchmakingEnabled(out var message))
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("versioning.newmatchmaking.title");
			basicPopup.Description = (string.IsNullOrEmpty(message) ? Localization.Get("versioning.newmatchmaking") : message);
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup.Show(InputManager.GetInputPosition());
			return;
		}
		GameManager.PreliminaryGameSettings.GameType = GameType.Matchmaking;
		if (!multiplayerSelectionScreen.TryLoadPreviousSettings(GameType.Matchmaking))
		{
			GameManager.PreliminaryGameSettings.OpponentCount = 1;
			GameManager.PreliminaryGameSettings.mapPreset = MapPreset.None;
			GameManager.PreliminaryGameSettings.TimeLimit = TimeLimit.Long.ToSeconds();
			GameManager.PreliminaryGameSettings.BaseTimeSeconds = TimeLimit.Long.ToSeconds();
		}
		multiplayerSelectionScreen.OnNewGameCommon();
	}

	private void StartNewGame()
	{
		GameManager.PreliminaryGameSettings.GameType = GameType.Multiplayer;
		TryLoadPreviousSettings((GameType)PolytopiaPlayerPrefs.GetInt("previous_multiplayer_game_type", 1));
		OnNewGameCommon();
	}

	public bool TryLoadPreviousSettings(GameType gameType)
	{
		string settingsNameFromModes = GameSettingsExtensions.GetSettingsNameFromModes(gameType, GameMode.Custom);
		GameSettings settings;
		bool num = GameSettingsExtensions.TryLoadFromDisk(out settings, settingsNameFromModes);
		if (!num)
		{
			settings.GameType = gameType;
		}
		GameManager.PreliminaryGameSettings = settings;
		return num;
	}

	public void OnNewGameCommon()
	{
		UIManager.Instance.ShowScreen(UIConstants.Screens.GameSetup);
	}

	private List<GameSummaryViewModel> GetHotSeatGames()
	{
		List<GameSummaryViewModel> list = new List<GameSummaryViewModel>();
		string saveDirectoryPath = Paths.GetSaveDirectoryPath("Hotseat");
		if (PolytopiaDirectory.Exists(saveDirectoryPath))
		{
			string[] files = PolytopiaDirectory.GetFiles(saveDirectoryPath, "*.state");
			if (files != null && files.Length != 0)
			{
				string[] array = files;
				foreach (string filePath in array)
				{
					GameSummaryViewModel gameSummaryViewModel = ConvertStateOnFileToSummary(filePath);
					if (gameSummaryViewModel != null)
					{
						list.Add(gameSummaryViewModel);
					}
				}
			}
		}
		return list;
	}

	private GameSummaryViewModel ConvertStateOnFileToSummary(string filePath)
	{
		if (DiskSerializationHelpers.FromDisk<HotseatGameData>(filePath, out var result))
		{
			HotseatProfilesState hotseatProfilesState = GameManager.GetHotseatProfilesState();
			string text = filePath.Remove(0, Paths.GetSaveDirectoryPath("Hotseat").Length + 1);
			text = text.Remove(text.Length - ".state".Length);
			if (!Guid.TryParse(text, out var result2))
			{
				Log.Warning("Found file with incorrect id: {0}", new object[1] { text });
				return null;
			}
			int playerCount = result.currentGameState.PlayerCount;
			if (playerCount <= 0)
			{
				Log.Error("{0} players in game with id {1}", new object[2] { playerCount, result2 });
				return null;
			}
			List<ParticipatorViewModel> list = new List<ParticipatorViewModel>(playerCount);
			for (int i = 0; i < playerCount; i++)
			{
				PlayerState playerState = result.currentGameState.PlayerStates[i];
				PlayerProfileState playerProfileState = null;
				foreach (PlayerProfileState player in hotseatProfilesState.players)
				{
					if (player.id == playerState.AccountId.Value)
					{
						playerProfileState = player;
						break;
					}
				}
				list.Add(new ParticipatorViewModel
				{
					UserId = playerState.AccountId.Value,
					InvitationState = PlayerInvitationState.Accepted,
					AvatarStateData = SerializationHelpers.ToByteArray(playerProfileState?.avatarState, VersionManager.AvatarVersion),
					Name = (playerProfileState?.name ?? playerState.UserName)
				});
			}
			DateTime? dateLastCommand = PolytopiaFile.GetLastWriteTime(filePath);
			return new GameSummaryViewModel
			{
				GameId = result2,
				OwnerId = AccountManager.PlayerAccountId,
				State = GameSessionState.Started,
				GameSummaryData = GameStateSummary.FromGameStateByteArray(SerializationHelpers.ToByteArray(result.currentGameState, result.currentGameState.Version)),
				Participators = list,
				DateLastCommand = dateLastCommand
			};
		}
		return null;
	}

	private static int SortGameSummaryByTimeLimit(GameSummaryViewModel a, GameSummaryViewModel b)
	{
		if (a == null || b == null)
		{
			return 0;
		}
		DateTime utcNow = DateTime.UtcNow;
		DateTime? dateTime = a.DateLastEndTurn ?? a.DateCreated;
		DateTime? dateTime2 = b.DateLastEndTurn ?? b.DateCreated;
		TimeSpan timeSpan = TimeSpan.FromMinutes(a.TimeLimit);
		TimeSpan timeSpan2 = TimeSpan.FromMinutes(b.TimeLimit);
		TimeSpan value = timeSpan.Subtract(utcNow.Subtract(dateTime.Value));
		return timeSpan2.Subtract(utcNow.Subtract(dateTime2.Value)).CompareTo(value);
	}
}
