using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using UnityEngine;

public class RemoteGameDataManager
{
	private struct CachedGameData
	{
		public GameSummaryViewModel gameSummaryViewModel;

		public GameStateSummary gameStateSummary;

		public ParticipatorViewModel localParticipator;

		public ParticipatorViewModel currentParticipator;

		public Guid? currentPlayerId;

		public TimeSpan? currentPlayerTimeLeft;

		public TimeSpan? localPlayerTimeLeft;

		public bool lastPlayerStanding;

		public int gameVersion;
	}

	public const float TURN_TIMER_UPDATE_FREQUENCY = 1f;

	public const float SKIP_TIMER_UPDATE_FREQUENCY = 1f;

	private Dictionary<Guid, CachedGameData> gameDataCache;

	private Dictionary<long, MatchmakingGameSummaryViewModel> matchmakingGameDataCache;

	private List<Guid> gameIdCache;

	private Task<ServerResponse<GameListingViewModel>> updateTask;

	private List<SkipTurnBindingModel> skipRequests = new List<SkipTurnBindingModel>();

	private float skipTimer;

	private float turnTimer;

	public bool HasLoadedCache => gameDataCache != null;

	public bool HasGameSummaryData
	{
		get
		{
			if (gameDataCache != null)
			{
				return gameDataCache.Count > 0;
			}
			return false;
		}
	}

	public bool HasMatchmakingSummaryData
	{
		get
		{
			if (matchmakingGameDataCache != null)
			{
				return matchmakingGameDataCache.Count > 0;
			}
			return false;
		}
	}

	public void Initialize()
	{
		BackendEvents.OnReceivedGameSummary += OnReceivedGameSummary;
		BackendEvents.OnReceivedGameDeletion += OnGameDeleted;
		BackendEvents.OnReceivedPlayerResignation += OnReceivedPlayerResignation;
		BackendEvents.OnReceivedMatchmakingGameUpdate += OnReceivedMatchmakingGameUpdate;
		BackendEvents.OnReceivedPlayerSkipped += OnReceivedPlayerSkipped;
	}

	public void Destroy()
	{
		BackendEvents.OnReceivedGameSummary -= OnReceivedGameSummary;
		BackendEvents.OnReceivedGameDeletion -= OnGameDeleted;
		BackendEvents.OnReceivedPlayerResignation -= OnReceivedPlayerResignation;
		BackendEvents.OnReceivedMatchmakingGameUpdate -= OnReceivedMatchmakingGameUpdate;
		BackendEvents.OnReceivedPlayerSkipped -= OnReceivedPlayerSkipped;
	}

	public void Update()
	{
		if (turnTimer >= 1f)
		{
			turnTimer = 0f;
			UpdateTimers();
		}
		if (skipTimer >= 1f)
		{
			skipTimer = 0f;
			PerformAutoSkipInternal();
		}
		turnTimer += Time.deltaTime;
		skipTimer += Time.deltaTime;
	}

	private async void OnReceivedMatchmakingGameUpdate(long gameId, MatchmakingUpdateReason reason)
	{
		Log.Info("[RemoteGameDataManager] Recieved Matchmaking game update", Array.Empty<object>());
		if (reason == MatchmakingUpdateReason.GameDeleted)
		{
			if (HasMatchmakingSummaryData)
			{
				DeleteMatchmakingGame(gameId);
			}
			return;
		}
		ServerResponse<MatchmakingGameSummaryViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetMatchmakingGameById(gameId);
		if (serverResponse.Success)
		{
			OnReceivedMatchmakingGameSummary(serverResponse.Data, reason);
		}
	}

	private void OnGameDeleted(Guid id)
	{
		if (HasGameSummaryData)
		{
			RemoveGameSummary(id);
		}
	}

	private void OnReceivedPlayerSkipped(PlayerSkippedViewModel skippedViewModel)
	{
		if (gameDataCache == null)
		{
			gameDataCache = new Dictionary<Guid, CachedGameData>();
		}
		if (gameDataCache.TryGetValue(skippedViewModel.GameId, out var value))
		{
			bool flag = value.localParticipator.UserId == skippedViewModel.SkippedUserId;
			string arg = "";
			if (!flag)
			{
				foreach (GameStateSummary.GamePlayerSummary playerSummary in value.gameStateSummary.PlayerSummaries)
				{
					if (playerSummary.PolytopiaId == skippedViewModel.SkippedUserId)
					{
						arg = playerSummary.UserName;
					}
				}
			}
			bool flag2 = UIManager.Instance.CurrentScreen == UIConstants.Screens.Hud && GameManager.Client != null && GameManager.Client.CurrentGameId.HasValue && GameManager.Client.CurrentGameId.Value == skippedViewModel.GameId;
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.identifier = "SKIP_NOTICE" + skippedViewModel.GameId;
			basicPopup.Header = (flag2 ? Localization.Get("onlineview.skipped.title.empty") : Localization.Get("onlineview.skipped.title.game", value.gameStateSummary.GameName));
			if (!skippedViewModel.WasAutoSkip && skippedViewModel.SkippedBy.HasValue && skippedViewModel.SkippedBy.Value == AccountManager.PlayerAccountId)
			{
				basicPopup.Description = Localization.Get("onlineview.skipped.info.bylocalplayer", arg);
			}
			else if (value.gameSummaryViewModel.State == GameSessionState.Lobby || value.gameSummaryViewModel.State == GameSessionState.ReadyToStart)
			{
				basicPopup.Description = (flag ? Localization.Get("onlineview.skipped.info.tribe.local") : Localization.Get("onlineview.skipped.info.tribe.other", arg));
			}
			else
			{
				basicPopup.Description = (flag ? Localization.Get("onlineview.skipped.info.local") : Localization.Get("onlineview.skipped.info.other", arg));
			}
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					if (UIManager.Instance.CurrentScreen == UIConstants.Screens.TribeSelector)
					{
						UIManager.OpenMultiplayerScreen();
					}
				})
			};
			if (PopupManager.TryGetOpenPopup<BasicPopup>(basicPopup.identifier, out var openPopup))
			{
				openPopup.Hide();
			}
			if (flag && PopupManager.TryGetOpenPopup<BasicPopup>("YOUR_TURN" + skippedViewModel.GameId, out openPopup))
			{
				openPopup.Hide();
			}
			basicPopup.Show();
		}
		BackendEvents.GameSummariesUpdated();
	}

	private void OnReceivedPlayerResignation(PlayerResignedViewModel playerResignedViewModel)
	{
		if (playerResignedViewModel.Resignee == AccountManager.PlayerAccountId)
		{
			if (playerResignedViewModel.Kicked)
			{
				string kickerName = null;
				SerializationHelpers.FromByteArray<GameStateSummary>(playerResignedViewModel.GameSummary.GameSummaryData, out var result);
				foreach (GameStateSummary.GamePlayerSummary playerSummary in result.PlayerSummaries)
				{
					if (playerSummary.PolytopiaId == playerResignedViewModel.KickerId)
					{
						kickerName = playerSummary.UserName;
						break;
					}
				}
				bool flag = UIManager.Instance.type == UIManager.Type.Ingame && (GameManager.Client?.CurrentGameId).HasValue && playerResignedViewModel.GameSummary.GameId == GameManager.Client.CurrentGameId.Value;
				if (flag && GameManager.GameState.Version < 93)
				{
					GameManager.Client.ActionManager.Pause();
					ResignReaction.ShowInGameKickedPopup(playerResignedViewModel.GameSummary.GameId.ToString(), result.GameName, kickerName, delegate
					{
						GameManager.Client.ActionManager.Resume();
					});
				}
				else if (!flag)
				{
					ResignReaction.ShowKickedPopup(playerResignedViewModel.GameSummary.GameId.ToString(), result.GameName, kickerName);
				}
			}
		}
		else
		{
			string arg = null;
			SerializationHelpers.FromByteArray<GameStateSummary>(playerResignedViewModel.GameSummary.GameSummaryData, out var result2);
			foreach (GameStateSummary.GamePlayerSummary playerSummary2 in result2.PlayerSummaries)
			{
				if (playerSummary2.PolytopiaId == playerResignedViewModel.Resignee)
				{
					arg = playerSummary2.UserName;
				}
			}
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.identifier = "SKIP_NOTICE" + playerResignedViewModel.GameSummary.GameId;
			if (playerResignedViewModel.Kicked)
			{
				basicPopup.Header = Localization.Get("onlineview.kicked.title.other", arg);
				basicPopup.Description = Localization.Get("onlineview.kicked.automatic.other", arg, result2.GameName);
			}
			else
			{
				basicPopup.Header = Localization.Get("onlineview.resigned.title", arg);
				if (result2.GameType == GameType.Competitive || result2.GameType == GameType.Multiplayer)
				{
					basicPopup.Description = Localization.Get("onlineview.resigned.competitive.info", arg, result2.GameName);
				}
				else
				{
					basicPopup.Description = Localization.Get("onlineview.resigned.info", arg, result2.GameName);
				}
			}
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			if (PopupManager.TryGetOpenPopup<BasicPopup>(basicPopup.identifier, out var openPopup))
			{
				openPopup.Hide();
			}
			basicPopup.Show();
		}
		BackendEvents.GameSummariesUpdated();
	}

	private void OnReceivedGameSummary(GameSummaryViewModel summary, StateUpdateReason reason)
	{
		if (summary != null)
		{
			if (gameDataCache == null)
			{
				gameDataCache = new Dictionary<Guid, CachedGameData>();
			}
			if (gameDataCache.TryGetValue(summary.GameId, out var value))
			{
				value = CreateCachedGameDataFromSummary(summary);
				gameDataCache[summary.GameId] = value;
			}
			else
			{
				value = CreateCachedGameDataFromSummary(summary);
				gameDataCache.Add(summary.GameId, value);
				SubscribeToGameBindingModel model = new SubscribeToGameBindingModel
				{
					GameId = summary.GameId,
					SubscriptionType = SubscriptionType.GameSummary
				};
				PolytopiaBackendAdapter.Instance.SubscribeToGame(model);
			}
			gameIdCache = new List<Guid>(gameDataCache.Keys);
			if ((reason == StateUpdateReason.GameCreated || reason == StateUpdateReason.ValidStartGame) && summary.MatchmakingGameId.HasValue && matchmakingGameDataCache != null && matchmakingGameDataCache.ContainsKey(summary.MatchmakingGameId.Value))
			{
				matchmakingGameDataCache.Remove(summary.MatchmakingGameId.Value);
			}
			UpdateSkipRequest(value);
			PerformAutoSkipInternal(value);
			BackendEvents.GameSummariesUpdated();
		}
	}

	private void OnReceivedMatchmakingGameSummary(MatchmakingGameSummaryViewModel summary, MatchmakingUpdateReason reason)
	{
		if (summary == null)
		{
			return;
		}
		if (matchmakingGameDataCache == null)
		{
			matchmakingGameDataCache = new Dictionary<long, MatchmakingGameSummaryViewModel>();
		}
		bool flag = reason == MatchmakingUpdateReason.PlayerLeft && summary.Participators.Find((ParticipatorViewModel participator) => participator.UserId == AccountManager.PlayerAccountId) == null;
		if (reason == MatchmakingUpdateReason.GameDeleted || flag)
		{
			if (HasMatchmakingSummaryData)
			{
				DeleteMatchmakingGame(summary.Id);
			}
			return;
		}
		if (matchmakingGameDataCache.ContainsKey(summary.Id))
		{
			matchmakingGameDataCache[summary.Id] = summary;
		}
		else
		{
			matchmakingGameDataCache.Add(summary.Id, summary);
		}
		BackendEvents.GameSummariesUpdated();
	}

	public void AddOrUpdateGameSummary(GameSummaryViewModel summary)
	{
		if (gameDataCache == null)
		{
			gameDataCache = new Dictionary<Guid, CachedGameData>();
		}
		if (gameDataCache.ContainsKey(summary.GameId))
		{
			gameDataCache[summary.GameId] = CreateCachedGameDataFromSummary(summary);
		}
		else
		{
			gameDataCache.Add(summary.GameId, CreateCachedGameDataFromSummary(summary));
		}
		gameIdCache = new List<Guid>(gameDataCache.Keys);
		BackendEvents.GameSummariesUpdated();
	}

	public void AddOrUpdateGameSummary(MatchmakingGameSummaryViewModel summary)
	{
		OnReceivedMatchmakingGameSummary(summary, MatchmakingUpdateReason.PlayerJoined);
	}

	public GameSummaryViewModel RemoveGameSummary(Guid gameId)
	{
		if (gameDataCache == null)
		{
			return null;
		}
		if (gameDataCache.TryGetValue(gameId, out var value))
		{
			gameDataCache.Remove(gameId);
		}
		gameIdCache = new List<Guid>(gameDataCache.Keys);
		BackendEvents.GameSummariesUpdated();
		return value.gameSummaryViewModel;
	}

	public async Task<ServerResponse<ResponseViewModel>> DeleteGameAsync(Guid? gameId)
	{
		if (!gameId.HasValue)
		{
			return null;
		}
		GameSummaryViewModel game = RemoveGameSummary(gameId.Value);
		NetworkUtils.ShowLoader();
		ServerResponse<ResponseViewModel> obj = await PolytopiaBackendAdapter.Instance.SetParticipationDone(new SetParticipationDoneBindingModel
		{
			GameId = gameId.Value
		});
		NetworkUtils.HideLoader();
		if (!obj.Success && game != null)
		{
			AddOrUpdateGameSummary(game);
		}
		return obj;
	}

	public async Task<bool> UpdateGameSummaries(bool ignoreError)
	{
		if (updateTask == null || updateTask.IsCompleted)
		{
			Log.Info("[RemoteGameDataManager] Updating GameSummaries...", Array.Empty<object>());
			updateTask = PolytopiaBackendAdapter.Instance.GetGameListingsV3();
		}
		else
		{
			Log.Verbose("[RemoteGameDataManager] Waiting for GameSummaries to update...", Array.Empty<object>());
		}
		ServerResponse<GameListingViewModel> serverResponse = await updateTask;
		if (!serverResponse.Success)
		{
			if (!ignoreError)
			{
				NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(serverResponse));
			}
			return false;
		}
		GameListingViewModel data = serverResponse.Data;
		if (gameDataCache == null)
		{
			gameDataCache = new Dictionary<Guid, CachedGameData>();
		}
		gameDataCache.Clear();
		for (int i = 0; i < data.gameSummaries.Count; i++)
		{
			GameSummaryViewModel gameSummaryViewModel = data.gameSummaries[i];
			if (gameDataCache.ContainsKey(gameSummaryViewModel.GameId))
			{
				gameDataCache[gameSummaryViewModel.GameId] = CreateCachedGameDataFromSummary(gameSummaryViewModel);
			}
			else
			{
				gameDataCache.Add(gameSummaryViewModel.GameId, CreateCachedGameDataFromSummary(gameSummaryViewModel));
			}
		}
		gameIdCache = new List<Guid>(gameDataCache.Keys);
		matchmakingGameDataCache = new Dictionary<long, MatchmakingGameSummaryViewModel>();
		for (int j = 0; j < data.matchmakingGameSummaries.Count; j++)
		{
			MatchmakingGameSummaryViewModel matchmakingGameSummaryViewModel = data.matchmakingGameSummaries[j];
			if (matchmakingGameDataCache.ContainsKey(matchmakingGameSummaryViewModel.Id))
			{
				matchmakingGameDataCache[matchmakingGameSummaryViewModel.Id] = matchmakingGameSummaryViewModel;
			}
			else
			{
				matchmakingGameDataCache.Add(matchmakingGameSummaryViewModel.Id, matchmakingGameSummaryViewModel);
			}
		}
		UpdateSkipRequests();
		PerformAutoSkipInternal();
		await new WaitForUpdate();
		return true;
	}

	public async Task<List<MatchmakingGameSummaryViewModel>> GetMatchMakingGameSummaryViewModels(bool forceUpdate = false)
	{
		if ((matchmakingGameDataCache == null || forceUpdate) && !(await UpdateGameSummaries(ignoreError: false)))
		{
			return new List<MatchmakingGameSummaryViewModel>();
		}
		List<MatchmakingGameSummaryViewModel> list = new List<MatchmakingGameSummaryViewModel>(matchmakingGameDataCache.Count);
		foreach (KeyValuePair<long, MatchmakingGameSummaryViewModel> item in matchmakingGameDataCache)
		{
			list.Add(item.Value);
		}
		return list;
	}

	public async Task<List<GameSummaryViewModel>> GetGameSummaryViewModels(bool forceUpdate = false)
	{
		if ((gameDataCache == null || forceUpdate) && !(await UpdateGameSummaries(ignoreError: false)))
		{
			return new List<GameSummaryViewModel>();
		}
		List<GameSummaryViewModel> list = new List<GameSummaryViewModel>(gameDataCache.Count);
		foreach (KeyValuePair<Guid, CachedGameData> item in gameDataCache)
		{
			list.Add(item.Value.gameSummaryViewModel);
		}
		return list;
	}

	public bool TryGetGameSummaryViewModel(Guid gameId, out GameSummaryViewModel summaryViewModel)
	{
		if (gameDataCache.TryGetValue(gameId, out var value) && value.gameSummaryViewModel != null)
		{
			summaryViewModel = value.gameSummaryViewModel;
			return true;
		}
		summaryViewModel = null;
		return false;
	}

	public bool TryGetGameStateSummary(Guid gameId, out GameStateSummary gameStateSummary)
	{
		if (gameDataCache.TryGetValue(gameId, out var value) && value.gameStateSummary != null)
		{
			gameStateSummary = value.gameStateSummary;
			return true;
		}
		gameStateSummary = null;
		return false;
	}

	public bool TryGetCurrentParticipator(Guid gameId, out ParticipatorViewModel participatorViewModel)
	{
		if (gameDataCache.TryGetValue(gameId, out var value) && value.currentParticipator != null)
		{
			participatorViewModel = value.currentParticipator;
			return true;
		}
		participatorViewModel = null;
		return false;
	}

	public bool TryGetLocalParticipator(Guid gameId, out ParticipatorViewModel participatorViewModel)
	{
		if (gameDataCache != null && gameDataCache.TryGetValue(gameId, out var value) && value.localParticipator != null)
		{
			participatorViewModel = value.localParticipator;
			return true;
		}
		participatorViewModel = null;
		return false;
	}

	public bool TryGetLocalParticipator(long gameId, out ParticipatorViewModel participatorViewModel)
	{
		if (matchmakingGameDataCache.TryGetValue(gameId, out var value))
		{
			foreach (ParticipatorViewModel participator in value.Participators)
			{
				if (participator.UserId == AccountManager.PlayerAccountId)
				{
					participatorViewModel = participator;
					return true;
				}
			}
		}
		participatorViewModel = null;
		return false;
	}

	public TimeSpan? GetGameTimeLeftForCurrentPlayer(Guid gameId)
	{
		if (gameDataCache != null && gameDataCache.TryGetValue(gameId, out var value) && value.currentPlayerTimeLeft.HasValue)
		{
			return value.currentPlayerTimeLeft;
		}
		return null;
	}

	public TimeSpan? GetGameTimeLeftForLocalPlayer(Guid gameId)
	{
		if (gameDataCache != null && gameDataCache.TryGetValue(gameId, out var value))
		{
			return GetGameTimeLeftForParticipator(gameId, value.localParticipator);
		}
		return null;
	}

	public TimeSpan? GetGameTimeLeftForParticipator(Guid gameId, ParticipatorViewModel participator, bool forceUpdate = false)
	{
		if (gameDataCache != null && gameDataCache.TryGetValue(gameId, out var value) && participator != null)
		{
			if (participator == value.currentParticipator && !forceUpdate)
			{
				return value.currentPlayerTimeLeft;
			}
			if (participator == value.localParticipator && !forceUpdate)
			{
				return value.localPlayerTimeLeft;
			}
			return CalculateTimeLeftForParticipator(gameId, participator);
		}
		return null;
	}

	private TimeSpan? CalculateTimeLeftForParticipator(GameSummaryViewModel gameSummaryViewModel, ParticipatorViewModel participator)
	{
		int num = -1;
		if (gameDataCache.TryGetValue(gameSummaryViewModel.GameId, out var value))
		{
			num = value.gameVersion;
		}
		if (num >= 94)
		{
			if (participator == null)
			{
				DateTime value2 = (gameSummaryViewModel.DateLastEndTurn ?? gameSummaryViewModel.DateCreated).Value;
				return TimeSpan.FromMinutes(gameSummaryViewModel.TimeLimit).Subtract(DateTime.UtcNow.Subtract(value2));
			}
			if (participator.DateCurrentTurnDeadline.HasValue)
			{
				return TimeSpan.FromMinutes((participator.DateCurrentTurnDeadline - DateTime.UtcNow).Value.TotalMinutes);
			}
			if (participator.TimeBank.HasValue)
			{
				return participator.TimeBank;
			}
		}
		else
		{
			if (participator != null && participator.DateCurrentTurnDeadline.HasValue)
			{
				return TimeSpan.FromMinutes((participator.DateCurrentTurnDeadline - DateTime.UtcNow).Value.TotalMinutes);
			}
			if (participator == null || GameManager.GameState == null || !GameManager.GameState.Settings.LiveGamePreset || gameSummaryViewModel.State != GameSessionState.Started)
			{
				DateTime value3 = (gameSummaryViewModel.DateLastEndTurn ?? gameSummaryViewModel.DateCreated).Value;
				return TimeSpan.FromMinutes(gameSummaryViewModel.TimeLimit).Subtract(DateTime.UtcNow.Subtract(value3));
			}
			if (GameManager.GameState.TryGetPlayer(GameManager.GameState.CurrentPlayer, out var playerState))
			{
				TimeSpan timeSpan = TimeSpan.FromSeconds(GameManager.GameState.Settings.BaseTimeSeconds);
				TimeSpan timeSpan2 = (participator.TimeBank.HasValue ? participator.TimeBank.Value : TimeSpan.Zero);
				TimeSpan timeBonusForPlayer = GameManager.GameState.GetTimeBonusForPlayer(playerState);
				return timeSpan + timeSpan2 + timeBonusForPlayer;
			}
		}
		return null;
	}

	private TimeSpan? CalculateTimeLeftForParticipator(Guid gameId, ParticipatorViewModel participator)
	{
		if (gameDataCache != null && gameDataCache.TryGetValue(gameId, out var value))
		{
			return CalculateTimeLeftForParticipator(value.gameSummaryViewModel, participator);
		}
		return null;
	}

	private CachedGameData CreateCachedGameDataFromSummary(GameSummaryViewModel summary)
	{
		CachedGameData result = new CachedGameData
		{
			gameSummaryViewModel = summary
		};
		if (SerializationHelpers.FromByteArray<GameStateSummary>(summary.GameSummaryData, out var result2, out var version))
		{
			result.gameVersion = version;
			result.gameStateSummary = result2;
			int num = 0;
			for (int i = 0; i < result2.PlayerSummaries.Count; i++)
			{
				GameStateSummary.GamePlayerSummary gamePlayerSummary = result2.PlayerSummaries[i];
				if (gamePlayerSummary.Id == result2.CurrentPlayer)
				{
					result.currentPlayerId = gamePlayerSummary.PolytopiaId;
				}
				if (!gamePlayerSummary.IsDead && !gamePlayerSummary.AutoPlay)
				{
					num++;
				}
			}
			result.lastPlayerStanding = num <= 1;
			for (int j = 0; j < summary.Participators.Count; j++)
			{
				ParticipatorViewModel participatorViewModel = summary.Participators[j];
				if (participatorViewModel.UserId == AccountManager.PlayerAccountId)
				{
					result.localParticipator = participatorViewModel;
					result.localPlayerTimeLeft = CalculateTimeLeftForParticipator(summary, participatorViewModel);
				}
				if (result.currentPlayerId.HasValue && result.currentPlayerId.Value == participatorViewModel.UserId)
				{
					result.currentParticipator = participatorViewModel;
					result.currentPlayerTimeLeft = CalculateTimeLeftForParticipator(summary, participatorViewModel);
				}
			}
		}
		return result;
	}

	public MatchmakingGameSummaryViewModel DeleteMatchmakingGame(long id)
	{
		Log.Info("Delete matchmaking game...", Array.Empty<object>());
		if (matchmakingGameDataCache.TryGetValue(id, out var value))
		{
			matchmakingGameDataCache.Remove(id);
		}
		BackendEvents.GameSummariesUpdated();
		return value;
	}

	private void UpdateTimers()
	{
		if (gameDataCache == null)
		{
			return;
		}
		for (int i = 0; i < gameIdCache.Count; i++)
		{
			if (gameDataCache.ContainsKey(gameIdCache[i]))
			{
				CachedGameData gameData = gameDataCache[gameIdCache[i]];
				gameDataCache[gameIdCache[i]] = UpdateTimer(gameData);
			}
		}
	}

	private CachedGameData UpdateTimer(CachedGameData gameData)
	{
		if (gameData.gameSummaryViewModel.State != GameSessionState.Ended)
		{
			gameData.currentPlayerTimeLeft = CalculateTimeLeftForParticipator(gameData.gameSummaryViewModel.GameId, gameData.currentParticipator);
			if (gameData.currentParticipator == gameData.localParticipator)
			{
				gameData.localPlayerTimeLeft = gameData.currentPlayerTimeLeft;
			}
			else
			{
				gameData.localPlayerTimeLeft = CalculateTimeLeftForParticipator(gameData.gameSummaryViewModel.GameId, gameData.localParticipator);
			}
		}
		return gameData;
	}

	private void PerformAutoSkipInternal()
	{
		if (gameDataCache == null)
		{
			return;
		}
		foreach (KeyValuePair<Guid, CachedGameData> item in gameDataCache)
		{
			PerformAutoSkipInternal(item.Value);
		}
	}

	private void PerformAutoSkipInternal(CachedGameData gameData)
	{
		if (gameData.currentParticipator != null)
		{
			SkipTurnBindingModel skipTurnBindingModel = new SkipTurnBindingModel
			{
				GameId = gameData.gameSummaryViewModel.GameId,
				UserId = gameData.currentParticipator.UserId,
				TurnNumber = gameData.gameStateSummary.CurrentTurn,
				IsAutoSkip = true
			};
			if (skipRequests != null && skipRequests.Count > 0 && skipRequests.Contains(skipTurnBindingModel))
			{
				Log.Verbose("[AutoSkip] Skip request already sent for game {0}", new object[1] { gameData.gameSummaryViewModel.GameId });
			}
			else if (ShouldAutoSkipPlayer(gameData))
			{
				Log.Verbose("[AutoSkip] Sending skip request for game {0}", new object[1] { gameData.gameSummaryViewModel.GameId });
				PolytopiaBackendAdapter.Instance.SkipTurn(skipTurnBindingModel);
				skipRequests.Add(skipTurnBindingModel);
			}
		}
	}

	private void UpdateSkipRequests()
	{
		if (gameDataCache == null)
		{
			return;
		}
		foreach (KeyValuePair<Guid, CachedGameData> item in gameDataCache)
		{
			UpdateSkipRequest(item.Value);
		}
	}

	private void UpdateSkipRequest(CachedGameData gameData)
	{
		if (skipRequests == null || skipRequests.Count == 0)
		{
			return;
		}
		int count = skipRequests.Count;
		for (int i = 0; i < count; i++)
		{
			SkipTurnBindingModel skipTurnBindingModel = skipRequests[i];
			if (skipTurnBindingModel.GameId == gameData.gameSummaryViewModel.GameId)
			{
				if (gameData.gameStateSummary.CurrentTurn > skipTurnBindingModel.TurnNumber)
				{
					skipRequests.Remove(skipTurnBindingModel);
					break;
				}
				ParticipatorViewModel participatorViewModel = ((gameData.currentParticipator != null) ? gameData.currentParticipator : gameData.localParticipator);
				if (skipTurnBindingModel.UserId != participatorViewModel.UserId)
				{
					skipRequests.Remove(skipTurnBindingModel);
					break;
				}
			}
		}
	}

	private bool ShouldAutoSkipPlayer(CachedGameData gameData)
	{
		if (gameData.gameSummaryViewModel.State != GameSessionState.Started)
		{
			return false;
		}
		if (!gameData.gameStateSummary.IsAutoSkipEnabled)
		{
			return false;
		}
		if (!gameData.currentPlayerTimeLeft.HasValue)
		{
			return false;
		}
		return gameData.currentPlayerTimeLeft.Value.TotalSeconds <= 0.0;
	}
}
