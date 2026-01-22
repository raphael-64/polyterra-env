using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polytopia.Data;
using Polytopia.IO;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameInfoPopup : BasicPopup
{
	public enum Mode
	{
		None,
		Game,
		Matchmaking
	}

	[Header("Game Info Popup")]
	[SerializeField]
	protected UIImageButton headerIcon;

	[SerializeField]
	protected UITextButton deleteButton;

	[SerializeField]
	protected MapSizeButtonWrapper mapSizeButton;

	[SerializeField]
	protected GameModeButtonWrapper gameModeButton;

	[SerializeField]
	protected TimerInfoButtonWrapper timerInfoButton;

	[SerializeField]
	protected MoreInfoButtonWrapper moreInfoButton;

	[SerializeField]
	protected GridLayoutGroup gridLayout;

	[SerializeField]
	protected LayoutElement gridBottomSpacer;

	[SerializeField]
	protected UIRoundButton shareButton;

	[Header("prefabs")]
	[SerializeField]
	protected PlayerButton playerButtonPrefab;

	protected List<PlayerButton> playerButtons = new List<PlayerButton>();

	private Mode mode = Mode.Game;

	protected GameSummaryViewModel summaryViewModel;

	protected GameStateSummary.GamePlayerSummary localPlayerSummary;

	protected GameStateSummary.GamePlayerSummary currentPlayerSummary;

	protected PlayerInvitationState localPlayerInvitationState;

	protected bool isLocalPlayerOwner;

	protected GameType gameType = GameType.Multiplayer;

	protected bool isReplay;

	protected Guid gameId = Guid.Empty;

	protected MatchmakingGameSummaryViewModel matchmakingSummaryViewModel;

	protected long matchmakingGameId;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	private Action headerIconAction;

	protected string MainButtonKey
	{
		get
		{
			string result = string.Empty;
			switch (summaryViewModel.State)
			{
			case GameSessionState.Lobby:
				result = ((localPlayerInvitationState != PlayerInvitationState.Accepted) ? "onlineview.game.join" : "tribepicker.pick");
				break;
			case GameSessionState.ReadyToStart:
				result = "onlineview.game.start";
				break;
			case GameSessionState.Started:
				result = "onlineview.game.open";
				break;
			case GameSessionState.Ended:
				result = "onlineview.game.open";
				break;
			}
			return result;
		}
	}

	protected string DeleteButtonKey
	{
		get
		{
			if (mode == Mode.Matchmaking)
			{
				return "onlineview.game.leavematchmaking";
			}
			string result = string.Empty;
			switch (summaryViewModel.State)
			{
			case GameSessionState.Lobby:
				result = "onlineview.game.decline";
				break;
			case GameSessionState.ReadyToStart:
				result = (isLocalPlayerOwner ? "onlineview.game.delete" : "onlineview.game.resign");
				break;
			case GameSessionState.Started:
				result = ((localPlayerInvitationState == PlayerInvitationState.Resigned || (localPlayerSummary != null && localPlayerSummary.IsDead)) ? "onlineview.game.delete" : "onlineview.game.resign");
				break;
			case GameSessionState.Ended:
				result = "onlineview.game.delete";
				break;
			}
			if (gameType == GameType.PassAndPlay)
			{
				result = "onlineview.game.delete";
			}
			return result;
		}
	}

	public bool MainButtonVisible
	{
		get
		{
			if (gameType == GameType.PassAndPlay)
			{
				return true;
			}
			if ((localPlayerSummary == null || isReplay) && summaryViewModel.State != GameSessionState.Ended)
			{
				return false;
			}
			if (gameType == GameType.Multiplayer || gameType == GameType.Competitive)
			{
				if (summaryViewModel.State == GameSessionState.ReadyToStart)
				{
					Guid? polytopiaId = localPlayerSummary.PolytopiaId;
					Guid? starterPlayer = GetStarterPlayer();
					if (polytopiaId.HasValue != starterPlayer.HasValue)
					{
						return false;
					}
					if (!polytopiaId.HasValue)
					{
						return true;
					}
					return polytopiaId.GetValueOrDefault() == starterPlayer.GetValueOrDefault();
				}
				if (summaryViewModel.State == GameSessionState.Ended)
				{
					return true;
				}
				if (localPlayerSummary.HasChosenTribe && summaryViewModel.State != GameSessionState.Started)
				{
					Guid? starterPlayer = localPlayerSummary.PolytopiaId;
					Guid? polytopiaId = GetStarterPlayer();
					if (starterPlayer.HasValue != polytopiaId.HasValue)
					{
						return false;
					}
					if (!starterPlayer.HasValue)
					{
						return true;
					}
					return starterPlayer.GetValueOrDefault() == polytopiaId.GetValueOrDefault();
				}
			}
			return true;
		}
	}

	public override void Init()
	{
		base.Init();
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetHeaderIcon(spriteHandle.sprite);
		});
	}

	private void OnEnable()
	{
		BackendEvents.OnReceivedGameSummary += OnRecievedGameSummary;
		BackendEvents.OnReceivedGameDeletion += OnRecievedGameDeletion;
		BackendEvents.OnReceivedPlayerResignation += OnRecievedPlayerResignation;
		BackendEvents.OnReceivedMatchmakingGameUpdate += OnMatchmakingGameUpdated;
		BackendEvents.OnGameSummariesUpdated += OnGameSummariesUpdated;
		headerIcon.OnClicked += OnHeaderIconClicked;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		BackendEvents.OnReceivedGameSummary -= OnRecievedGameSummary;
		BackendEvents.OnReceivedGameDeletion -= OnRecievedGameDeletion;
		BackendEvents.OnReceivedPlayerResignation -= OnRecievedPlayerResignation;
		BackendEvents.OnReceivedMatchmakingGameUpdate -= OnMatchmakingGameUpdated;
		BackendEvents.OnGameSummariesUpdated -= OnGameSummariesUpdated;
		headerIcon.OnClicked -= OnHeaderIconClicked;
	}

	public override void Show(Vector2 origin)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.Show(origin);
		GameManager.GetAnalyticsManager().SendEvent("match_info_popup_view", new Dictionary<string, object>());
	}

	private void OnGameSummariesUpdated()
	{
		if (!GameManager.GetRemoteGameDataManager().TryGetLocalParticipator(gameId, out var _))
		{
			Hide();
		}
	}

	private void OnRecievedPlayerResignation(PlayerResignedViewModel playerResignedViewModel)
	{
		if (playerResignedViewModel.Kicked && playerResignedViewModel.Resignee == AccountManager.PlayerAccountId && playerResignedViewModel.GameSummary.GameId == gameId)
		{
			Hide();
		}
	}

	public async void OnMatchmakingGameUpdated(long gameId, MatchmakingUpdateReason reason)
	{
		if (mode != Mode.Matchmaking || gameId != matchmakingSummaryViewModel.Id)
		{
			return;
		}
		if (reason == MatchmakingUpdateReason.GameDeleted)
		{
			Hide();
			return;
		}
		ServerResponse<MatchmakingGameSummaryViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetMatchmakingGameById(gameId);
		if (serverResponse.Success && (Object)(object)this != (Object)null && ((Component)this).gameObject.activeSelf)
		{
			ResetPopup();
			SetData(serverResponse.Data);
		}
	}

	private void OnRecievedGameSummary(GameSummaryViewModel summary, StateUpdateReason reason)
	{
		if (summary != null && (mode != Mode.Game || (summaryViewModel != null && !(summaryViewModel.GameId != summary.GameId))) && (mode != Mode.Matchmaking || (matchmakingSummaryViewModel != null && summary.MatchmakingGameId.HasValue && matchmakingSummaryViewModel.Id == summary.MatchmakingGameId.Value)))
		{
			SetData(summary);
		}
	}

	private void OnRecievedGameDeletion(Guid id)
	{
		if (id == gameId)
		{
			SerializationHelpers.FromByteArray<GameStateSummary>(summaryViewModel.GameSummaryData, out var result);
			string empty = string.Empty;
			empty = ((result == null) ? Localization.Get("onlineview.gamedeleted.unknowngame") : result.GameName);
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("onlineview.gamedeleted.title");
			basicPopup.Description = Localization.Get("onlineview.gamedeleted.info", empty);
			basicPopup.buttonData = new PopupButtonData[1]
			{
				new PopupButtonData("buttons.ok", PopupButtonData.States.Selected, OnGameDeletedAccepted)
			};
			basicPopup.Show();
		}
	}

	private void OnGameDeletedAccepted(int id, BaseEventData eventData)
	{
		Hide();
	}

	public static ParticipatorViewModel GetParticipatorFromUserId(List<ParticipatorViewModel> participators, Guid? userId)
	{
		if (!userId.HasValue)
		{
			return null;
		}
		foreach (ParticipatorViewModel participator in participators)
		{
			if (participator.UserId == userId.Value)
			{
				return participator;
			}
		}
		return null;
	}

	public void OnShareReplayButtonClicked()
	{
		SharePopup sharePopup = PopupManager.GetSharePopup(summaryViewModel.GameId);
		sharePopup.buttonData = new PopupButtonData[1]
		{
			new PopupButtonData("buttons.back")
		};
		sharePopup.Show();
	}

	private void OnHeaderIconClicked(int id, BaseEventData eventData = null)
	{
		headerIconAction?.Invoke();
	}

	public void SetData(GameSummaryViewModel summaryViewModel, bool isReplay = false)
	{
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		ResetPopup();
		gameId = summaryViewModel.GameId;
		this.summaryViewModel = summaryViewModel;
		matchmakingSummaryViewModel = null;
		mode = Mode.Game;
		this.isReplay = isReplay;
		SerializationHelpers.FromByteArray<GameStateSummary>(summaryViewModel.GameSummaryData, out var result);
		if (result != null)
		{
			((Component)moreInfoButton).gameObject.SetActive(true);
			mapSizeButton.SetData(result.MapWidth, result.MapPreset);
			gameModeButton.SetData(result.GameMode, result.GameType, result.Rules.ScoreLimit);
			timerInfoButton.SetData(summaryViewModel.TimeLimit);
			moreInfoButton.SetData(PrepareGameInfo());
			if (summaryViewModel.GameContext != null && summaryViewModel.GameContext.ExternalMatchId.HasValue)
			{
				moreInfoButton.matchURL = $"https://www.challengermode.com/games/{summaryViewModel.GameContext.ExternalMatchId.Value}";
			}
			((Component)shareButton).gameObject.SetActive(isReplay && summaryViewModel.State == GameSessionState.Ended);
			((Component)timerInfoButton).gameObject.SetActive(!isReplay && result.GameType != GameType.PassAndPlay);
			if (summaryViewModel.GameContext != null && summaryViewModel.GameContext.ExternalTournamentId.HasValue)
			{
				LoadHeaderIcon("externalIcons_cm");
				headerIconAction = delegate
				{
					NativeHelpers.OpenURL($"https://www.challengermode.com/games/{summaryViewModel.GameContext.ExternalMatchId.Value}");
				};
			}
			else if (summaryViewModel.TimeLimit == -1)
			{
				LoadHeaderIcon("live-icon");
				headerIconAction = null;
			}
			else
			{
				SetHeaderIcon(null);
				headerIconAction = null;
			}
			gameType = result.GameType;
			Header = result.GameName;
			float num = 0f;
			int num2 = -1;
			int num3 = -1;
			for (int num4 = 0; num4 < result.PlayerSummaries.Count; num4++)
			{
				GameStateSummary.GamePlayerSummary gamePlayerSummary = result.PlayerSummaries[num4];
				if (gamePlayerSummary.TribeType != TribeData.Type.Nature)
				{
					if (gamePlayerSummary.PolytopiaId == AccountManager.PlayerAccountId)
					{
						localPlayerSummary = gamePlayerSummary;
					}
					if (gamePlayerSummary.Id == result.CurrentPlayer)
					{
						currentPlayerSummary = gamePlayerSummary;
						num3 = playerButtons.Count;
					}
					if (!gamePlayerSummary.AutoPlay && num2 == -1)
					{
						num2 = playerButtons.Count;
					}
					SerializationHelpers.FromByteArray<AvatarState>(GetParticipatorFromUserId(summaryViewModel.Participators, gamePlayerSummary.PolytopiaId)?.AvatarStateData, out var result2);
					PlayerButton playerButton = Object.Instantiate<PlayerButton>(playerButtonPrefab, ((Component)gridLayout).transform);
					playerButton.PlayerButtonEnable = false;
					playerButton.rectTransform.sizeDelta = new Vector2(50f, 50f);
					playerButton.iconSizeMultiplier *= 0.2f;
					playerButton.shouldFitIconToParent = result2 == null || gamePlayerSummary.AutoPlay || gamePlayerSummary.IsDead;
					playerButton.SetPlayerSummary(gamePlayerSummary, result2, isReplay);
					playerButton.ShowLocalPlayerLabel(gamePlayerSummary.PolytopiaId == AccountManager.PlayerAccountId && !gamePlayerSummary.AutoPlay);
					((Component)playerButton.shine).gameObject.SetActive(false);
					((Component)playerButton.bgContainer).gameObject.SetActive(false);
					if (playerButton.label.PreferedValues.y > num)
					{
						num = playerButton.label.PreferedValues.y;
					}
					playerButton.OnClicked += OnShowPlayerInfo;
					playerButton.id = gamePlayerSummary.Id;
					playerButtons.Add(playerButton);
				}
			}
			if (summaryViewModel.State == GameSessionState.ReadyToStart && num2 != -1)
			{
				((Component)playerButtons[num2].bgContainer).gameObject.SetActive(true);
			}
			else if (num3 != -1)
			{
				((Component)playerButtons[num3].bgContainer).gameObject.SetActive(true);
			}
			gridLayout.spacing = new Vector2(gridLayout.spacing.x, num + 10f);
			gridBottomSpacer.minHeight = num + 10f;
			Log.Verbose($"GameInfoPopup :: tallestPlayerName: {num}", Array.Empty<object>());
			if (localPlayerSummary != null)
			{
				localPlayerInvitationState = PlayerInvitationState.Unknown;
				isLocalPlayerOwner = localPlayerSummary.PolytopiaId == summaryViewModel.OwnerId;
				foreach (ParticipatorViewModel participator in summaryViewModel.Participators)
				{
					Guid userId = participator.UserId;
					Guid? polytopiaId = localPlayerSummary.PolytopiaId;
					if (userId == polytopiaId)
					{
						localPlayerInvitationState = participator.InvitationState;
					}
				}
			}
			GameRules gameRules = result.Rules;
			if (gameRules == null)
			{
				gameRules = new GameRules(result.GameMode);
			}
			if (result.GameMode == GameMode.Glory)
			{
				Description = $"{Localization.Get(GameModeUtils.GetDescription(result.GameMode), LocalizationUtils.FormatNumber(gameRules.ScoreLimit))}\n{GetDescription()}";
			}
			else
			{
				Description = $"{Localization.Get(GameModeUtils.GetDescription(result.GameMode))}\n{GetDescription()}";
			}
			List<PopupButtonData> list = new List<PopupButtonData>();
			list.Add(new PopupButtonData("buttons.back", (!MainButtonVisible) ? PopupButtonData.States.Selected : PopupButtonData.States.None, OnBack));
			if (MainButtonVisible)
			{
				if (localPlayerInvitationState == PlayerInvitationState.Accepted && summaryViewModel.State == GameSessionState.Lobby)
				{
					isLocalPlayerOwner = localPlayerSummary.PolytopiaId == currentPlayerSummary.PolytopiaId;
					list.Add(new PopupButtonData(MainButtonKey, isLocalPlayerOwner ? PopupButtonData.States.Selected : PopupButtonData.States.Disabled, OnMainButtonClicked));
				}
				else
				{
					bool flag = summaryViewModel.State == GameSessionState.Ended || localPlayerInvitationState != PlayerInvitationState.Declined;
					list.Add(new PopupButtonData(MainButtonKey, flag ? PopupButtonData.States.Selected : PopupButtonData.States.Disabled, OnMainButtonClicked));
				}
			}
			buttonData = list.ToArray();
			((Component)deleteButton).gameObject.SetActive(!isReplay);
			deleteButton.Key = DeleteButtonKey;
		}
		else
		{
			Description = Localization.Get("gameinfo.nodata");
			Header = Localization.Get("gameinfo.nodata.title");
			buttonData = new PopupButtonData[1]
			{
				new PopupButtonData("buttons.ok", PopupButtonData.States.Selected, OnBack)
			};
			((Component)deleteButton).gameObject.SetActive(false);
			gameModeButton.SetData(GameMode.None, GameType.Multiplayer);
		}
	}

	public static async Task ShowPlayerInfo(ParticipatorViewModel participator, Guid? id, string name, bool isAutoPlay, int handicap, bool isHotseatGame)
	{
		if (id.HasValue && id.Value != default(Guid) && id.Value == AccountManager.PlayerAccountId)
		{
			SerializationHelpers.FromByteArray<AvatarState>(participator?.AvatarStateData, out var result);
			PlayerData playerData = new PlayerData
			{
				profile = new PlayerProfileState
				{
					numFriends = participator.NumberOfFriends,
					numMultiplayerGames = participator.NumberOfMultiplayerGames,
					gameVersion = PlayerDataUtils.GetCurrentPlatformGameVersion(participator.GameVersion),
					multiplayerRating = participator.MultiplayerRating,
					lastLoginDate = DateTime.Now
				}
			};
			PopupManager.GetFriendInfoPopup(id.Value, name, PlayerData.State.IsYou, result, playerData).Show(InputManager.GetInputPosition());
			return;
		}
		if (isAutoPlay && (!id.HasValue || id.Value == default(Guid)))
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.AutoCapitalizeHeader = false;
			GameSettings.Difficulties difficulties = GameSettings.DifficultyFromHandicap(handicap);
			basicPopup.Header = Localization.Get("playerpickerview.bot", name, difficulties);
			string text = Localization.Get("playerinfopopup.bot.description");
			basicPopup.Description = text;
			basicPopup.buttonData = new PopupButtonData[1]
			{
				new PopupButtonData("buttons.ok")
			};
			basicPopup.Show(InputManager.GetInputPosition());
			return;
		}
		if (isHotseatGame)
		{
			BasicPopup basicPopup2 = PopupManager.GetBasicPopup();
			basicPopup2.AutoCapitalizeHeader = false;
			basicPopup2.Header = name;
			string text2 = Localization.Get("playerinfopopup.passandplay.description");
			basicPopup2.Description = text2;
			basicPopup2.buttonData = new PopupButtonData[1]
			{
				new PopupButtonData("buttons.ok")
			};
			basicPopup2.Show(InputManager.GetInputPosition());
			return;
		}
		PlayerData playerData2 = await AccountManager.GetFriendPlayerDataWithId(id);
		ServerResponseList<PolytopiaFriendViewModel> friendErrorResponse = AccountManager.GetFriendErrorResponse();
		if (playerData2 != null)
		{
			PopupManager.GetFriendInfoPopup(playerData2.profile.id, name, playerData2.state, playerData2.profile.avatarState, playerData2).Show();
			return;
		}
		if (friendErrorResponse != null)
		{
			PopupManager.ShowErrorPopup(Localization.GetErrorMessage(friendErrorResponse));
			return;
		}
		SerializationHelpers.FromByteArray<AvatarState>(participator?.AvatarStateData, out var result2);
		playerData2 = new PlayerData
		{
			profile = new PlayerProfileState
			{
				numFriends = participator.NumberOfFriends,
				numMultiplayerGames = participator.NumberOfMultiplayerGames,
				gameVersion = PlayerDataUtils.GetCurrentPlatformGameVersion(participator.GameVersion),
				multiplayerRating = participator.MultiplayerRating,
				lastLoginDate = DateTime.Now
			}
		};
		PopupManager.GetFriendInfoPopup(id.Value, name, PlayerData.State.None, result2, playerData2).Show();
	}

	private void LoadHeaderIcon(string iconId)
	{
		iconSpriteHandle.Request(SpriteData.GetUISpriteAddress(iconId));
	}

	private void SetHeaderIcon(Sprite sprite)
	{
		if ((Object)(object)sprite != (Object)null)
		{
			headerIcon.Icon = sprite;
			((Component)headerIcon).gameObject.SetActive(true);
			((TMP_Text)header).rectTransform.SetAnchoredX(28f);
		}
		else
		{
			((Component)headerIcon).gameObject.SetActive(false);
			((TMP_Text)header).rectTransform.SetAnchoredX(0f);
		}
	}

	protected async void OnShowPlayerInfo(int id, BaseEventData eventData)
	{
		SerializationHelpers.FromByteArray<GameStateSummary>(summaryViewModel.GameSummaryData, out var result);
		GameStateSummary.GamePlayerSummary gamePlayerSummary = FindPlayerSummaryWithId(result.PlayerSummaries, id);
		if (gamePlayerSummary == null)
		{
			Log.Error("Could not find player with id {0}", new object[1] { id });
			return;
		}
		ParticipatorViewModel participator = null;
		if (summaryViewModel?.Participators != null)
		{
			foreach (ParticipatorViewModel participator2 in summaryViewModel.Participators)
			{
				Guid userId = participator2.UserId;
				Guid? polytopiaId = gamePlayerSummary.PolytopiaId;
				if (userId == polytopiaId)
				{
					participator = participator2;
					break;
				}
			}
		}
		await ShowPlayerInfo(participator, gamePlayerSummary.PolytopiaId, gamePlayerSummary.UserName, gamePlayerSummary.AutoPlay, gamePlayerSummary.Handicap, result.GameType == GameType.PassAndPlay);
	}

	private GameStateSummary.GamePlayerSummary FindPlayerSummaryWithId(List<GameStateSummary.GamePlayerSummary> playerSummaries, int id)
	{
		for (int i = 0; i < playerSummaries.Count; i++)
		{
			GameStateSummary.GamePlayerSummary gamePlayerSummary = playerSummaries[i];
			if (gamePlayerSummary.Id == id)
			{
				return gamePlayerSummary;
			}
		}
		return null;
	}

	private string GetDescription()
	{
		string text = string.Empty;
		string text2 = string.Empty;
		if (summaryViewModel.State == GameSessionState.Ended)
		{
			text = string.Format("{0}{1}", text, Localization.Get("gameinfo.gameover"));
		}
		else
		{
			switch (summaryViewModel.State)
			{
			case GameSessionState.Lobby:
				text2 = Localization.Get("gameinfo.picktribe");
				break;
			case GameSessionState.ReadyToStart:
				text2 = Localization.Get("gameinfo.start");
				break;
			case GameSessionState.Started:
				text2 = Localization.Get("gameinfo.play");
				break;
			}
			SerializationHelpers.FromByteArray<GameStateSummary>(summaryViewModel.GameSummaryData, out var result);
			PlayerSummaries playerSummaries = GameSummaryUtils.GetPlayerSummaries(result, summaryViewModel);
			if (gameType == GameType.Multiplayer || gameType == GameType.Competitive)
			{
				text = ((localPlayerInvitationState == PlayerInvitationState.Invited) ? string.Format("{0}{1}", text, Localization.Get("gameitem.join")) : ((!playerSummaries.IsLocalPlayerCurrent()) ? string.Format("{0}{1}", text, Localization.Get("gameinfo.opponentsturn", playerSummaries.Current?.UserName, text2)) : string.Format("{0}{1}", text, Localization.Get("gameinfo.yourturn", text2))));
			}
			else if (gameType == GameType.PassAndPlay)
			{
				text = string.Format("{0}{1}", text, Localization.Get("gameinfo.yourturn", text2));
			}
		}
		return text;
	}

	private Guid? GetStarterPlayer()
	{
		for (int i = 0; i < summaryViewModel.Participators.Count; i++)
		{
			ParticipatorViewModel participatorViewModel = summaryViewModel.Participators[i];
			Log.Info("- Participator {0}, State: {1}", new object[2] { participatorViewModel.Name, participatorViewModel.InvitationState });
			if (participatorViewModel.InvitationState == PlayerInvitationState.Accepted)
			{
				Guid userId = participatorViewModel.UserId;
				Guid? ownerId = summaryViewModel.OwnerId;
				if (userId == ownerId)
				{
					return participatorViewModel.UserId;
				}
				userId = participatorViewModel.UserId;
				ownerId = currentPlayerSummary.PolytopiaId;
				if (userId == ownerId)
				{
					return participatorViewModel.UserId;
				}
			}
		}
		return null;
	}

	protected async void OnMainButtonClicked(int id, BaseEventData eventData)
	{
		GameType currentGameType = gameType;
		if (summaryViewModel?.GameSummaryData != null && SerializationHelpers.PeekVersion(summaryViewModel.GameSummaryData, out var version) && version > VersionManager.GameVersion)
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("wcontroller.load.update.title");
			basicPopup.Description = Localization.Get("wcontroller.load.update", SystemManager.GetUpdateDealer());
			basicPopup.buttonData = new PopupButtonData[1]
			{
				new PopupButtonData("buttons.ok", PopupButtonData.States.Selected)
			};
			basicPopup.Show(InputManager.GetInputPosition());
			return;
		}
		Log.Verbose($"GameInfoPopup :: currentGameType: {currentGameType}", Array.Empty<object>());
		switch (summaryViewModel.State)
		{
		case GameSessionState.Lobby:
			GameManager.GetAnalyticsManager().SendEvent("match_info_click", new Dictionary<string, object> { { "option", "pick" } });
			if (localPlayerInvitationState != PlayerInvitationState.Accepted)
			{
				NetworkUtils.ShowLoader();
				ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.RespondToInvitation(new RespondToInvitationBindingModel
				{
					GameId = summaryViewModel.GameId,
					Accepted = true
				});
				if (!serverResponse.Success)
				{
					NetworkUtils.HideLoader();
					PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
					break;
				}
				await PolytopiaBackendAdapter.Instance.SubscribeToGameSummaries(new SubscribeToGameSummariesBindingModel
				{
					GameIds = new List<Guid> { summaryViewModel.GameId }
				});
				NetworkUtils.HideLoader();
			}
			if (!localPlayerSummary.HasChosenTribe && localPlayerSummary.PolytopiaId == currentPlayerSummary.PolytopiaId)
			{
				GameManager.Instance.OnShowTribePicker(summaryViewModel.GameId);
			}
			break;
		case GameSessionState.ReadyToStart:
			UIBlackFader.FadeIn(0.5f, async delegate
			{
				NetworkUtils.ShowLoader(1000);
				if (!(await GameManager.Instance.StartMultiplayerGame(summaryViewModel.GameId)))
				{
					UIBlackFader.FadeOut();
				}
				NetworkUtils.HideLoader();
			});
			break;
		case GameSessionState.Started:
		case GameSessionState.Ended:
			UIBlackFader.FadeIn(0.5f, async delegate
			{
				if (isReplay)
				{
					NetworkUtils.ShowLoader(1000);
					if (!(await GameManager.Instance.OpenReplay(summaryViewModel.GameId, summaryViewModel)))
					{
						UIBlackFader.FadeOut();
					}
					NetworkUtils.HideLoader();
				}
				else if (currentGameType == GameType.Multiplayer || currentGameType == GameType.Competitive)
				{
					NetworkUtils.ShowLoader(1000);
					if (!(await GameManager.Instance.OpenMultiplayerGame(summaryViewModel.GameId)))
					{
						UIBlackFader.FadeOut();
					}
					NetworkUtils.HideLoader();
				}
				else if (currentGameType == GameType.PassAndPlay && !(await GameManager.Instance.ResumeHotseatGame(gameId)))
				{
					UIBlackFader.FadeOut();
				}
			});
			break;
		case GameSessionState.Unknown:
			break;
		}
	}

	public void OnDeleteButtonClicked()
	{
		if (mode == Mode.Matchmaking)
		{
			ShowMatchmakingLeavePopup();
			return;
		}
		if (isReplay)
		{
			ShowDeletePopup();
			return;
		}
		switch (summaryViewModel.State)
		{
		case GameSessionState.Lobby:
			GameManager.GetAnalyticsManager().SendEvent("match_info_click", new Dictionary<string, object> { { "option", "decline" } });
			ShowRejectPopup();
			break;
		case GameSessionState.ReadyToStart:
		case GameSessionState.Started:
		case GameSessionState.Ended:
			GameManager.GetAnalyticsManager().SendEvent("match_info_click", new Dictionary<string, object> { { "option", "resign" } });
			ShowRejectPopup();
			break;
		case GameSessionState.Unknown:
			break;
		}
	}

	protected void ShowDeletePopup()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("onlineview.replay.delete.title");
		basicPopup.Description = Localization.Get("onlineview.replay.delete.info");
		basicPopup.buttonData = new PopupButtonData[2]
		{
			new PopupButtonData("buttons.back"),
			new PopupButtonData("buttons.ok", PopupButtonData.States.Selected, OnDeleteOkClicked)
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}

	protected async void OnDeleteOkClicked(int id, BaseEventData eventData)
	{
		NetworkUtils.ShowLoader();
		ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.Resign(new ResignBindingModel
		{
			GameId = summaryViewModel.GameId
		});
		NetworkUtils.HideLoader();
		if (serverResponse != null && !serverResponse.Success)
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
		else
		{
			Hide();
		}
	}

	protected void ShowRejectPopup()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		bool flag = gameType == GameType.PassAndPlay || summaryViewModel.State == GameSessionState.Ended || localPlayerInvitationState == PlayerInvitationState.Resigned || (localPlayerSummary != null && localPlayerSummary.IsDead);
		basicPopup.Header = (flag ? Localization.Get("onlineview.game.delete.title") : Localization.Get("onlineview.game.resign.title"));
		basicPopup.Description = (flag ? Localization.Get("onlineview.game.delete.info") : Localization.Get("onlineview.game.resign.info"));
		basicPopup.buttonData = new PopupButtonData[2]
		{
			new PopupButtonData("buttons.back"),
			new PopupButtonData("buttons.ok", PopupButtonData.States.Selected, OnResignOkClicked)
		};
		basicPopup.Show(InputManager.GetInputPosition());
		GameManager.GetAnalyticsManager().SendEvent("resign_popup_view", new Dictionary<string, object> { { "game_id", summaryViewModel.GameId } });
	}

	protected async void OnResignOkClicked(int id, BaseEventData eventData)
	{
		GameManager.GetAnalyticsManager().SendEvent("player_resign", new Dictionary<string, object> { { "game_id", summaryViewModel.GameId } });
		BackendEvents.OnReceivedGameSummary -= OnRecievedGameSummary;
		BackendEvents.OnReceivedGameDeletion -= OnRecievedGameDeletion;
		BackendEvents.OnReceivedPlayerResignation -= OnRecievedPlayerResignation;
		if (gameType == GameType.PassAndPlay)
		{
			((MonoBehaviour)this).StartCoroutine(DeletePaPGameAndWait());
			return;
		}
		NetworkUtils.ShowLoader();
		ServerResponse<ResponseViewModel> serverResponse = null;
		switch (summaryViewModel.State)
		{
		case GameSessionState.Lobby:
			serverResponse = await PolytopiaBackendAdapter.Instance.RespondToInvitation(new RespondToInvitationBindingModel
			{
				GameId = summaryViewModel.GameId,
				Accepted = false
			});
			break;
		case GameSessionState.ReadyToStart:
		case GameSessionState.Started:
			serverResponse = ((localPlayerInvitationState != PlayerInvitationState.Resigned && !localPlayerSummary.IsDead) ? (await PolytopiaBackendAdapter.Instance.Resign(new ResignBindingModel
			{
				GameId = summaryViewModel.GameId
			})) : (await PolytopiaBackendAdapter.Instance.SetParticipationDone(new SetParticipationDoneBindingModel
			{
				GameId = summaryViewModel.GameId
			})));
			break;
		case GameSessionState.Ended:
			serverResponse = await PolytopiaBackendAdapter.Instance.SetParticipationDone(new SetParticipationDoneBindingModel
			{
				GameId = summaryViewModel.GameId
			});
			break;
		}
		NetworkUtils.HideLoader();
		if (serverResponse != null && !serverResponse.Success)
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
		else
		{
			Hide();
		}
	}

	protected IEnumerator DeletePaPGameAndWait()
	{
		string hotseatFilePath = Paths.GetHotseatFilePath(gameId.ToString());
		PolytopiaFileInfo fi = new PolytopiaFileInfo(hotseatFilePath);
		if (fi.Exists)
		{
			fi.Delete();
			fi.Refresh();
			yield return (object)new WaitWhile((Func<bool>)delegate
			{
				fi.Refresh();
				return fi.Exists;
			});
		}
		Hide();
		BackendEvents.ReceivedGameDeletion(gameId);
	}

	public string PrepareGameInfo()
	{
		string text = string.Empty;
		SerializationHelpers.FromByteArray<GameStateSummary>(summaryViewModel.GameSummaryData, out var result, out var version);
		if (result != null)
		{
			List<string> list = new List<string>();
			list.Add(string.Format("<link=GAME_INFO_ID>{0}</link>", Localization.Get("gameinfo.id", summaryViewModel.GameId.ToString())));
			if (version != -1)
			{
				list.Add(string.Format("{0}: {1}", Localization.Get("mplayerstats.gameversion"), version));
			}
			if (summaryViewModel.DateLastEndTurn.HasValue)
			{
				DateTime value = summaryViewModel.DateLastEndTurn.Value;
				list.Add(Localization.Get("gameinfo.lastmove", LocalizationUtils.GetTimeString(value)));
			}
			if (summaryViewModel.DateLastCommand.HasValue)
			{
				DateTime value2 = summaryViewModel.DateLastCommand.Value;
				list.Add(Localization.Get("gameinfo.updated", LocalizationUtils.GetTimeString(value2)));
			}
			list.Add(Localization.Get("gameinfo.turn", result.CurrentTurn));
			list.Add(Localization.Get("gameinfo.move", result.LastProcessedCommand));
			if (summaryViewModel.GameContext != null && summaryViewModel.GameContext.ExternalMatchId.HasValue)
			{
				list.Add("");
				list.Add(string.Format("<link=TOURNAMENT_MATCH_LINK><b><u>{0}</u></b></link>", Localization.Get("onlineview.tournament.info.overview")));
			}
			foreach (string item in list)
			{
				text = text + item + "\n";
			}
		}
		return text;
	}

	public void SetData(MatchmakingGameSummaryViewModel summaryViewModel)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		matchmakingSummaryViewModel = summaryViewModel;
		this.summaryViewModel = null;
		mode = Mode.Matchmaking;
		if (summaryViewModel != null)
		{
			Header = summaryViewModel.Name;
			gameModeButton.SetData(summaryViewModel.GameMode, GameType.Matchmaking);
			((Component)moreInfoButton).gameObject.SetActive(false);
			((Component)timerInfoButton).gameObject.SetActive(false);
			((Component)shareButton).gameObject.SetActive(false);
			mapSizeButton.SetData(summaryViewModel.MapSize, summaryViewModel.MapPreset);
			int num = ((summaryViewModel.OpponentCount == 0) ? 2 : (summaryViewModel.OpponentCount + 1));
			float num2 = 0f;
			for (int i = 0; i < summaryViewModel.Participators.Count; i++)
			{
				ParticipatorViewModel participator = summaryViewModel.Participators[i];
				SerializationHelpers.FromByteArray<AvatarState>(participator?.AvatarStateData, out var result);
				PlayerButton playerButton = Object.Instantiate<PlayerButton>(playerButtonPrefab, ((Component)gridLayout).transform);
				playerButton.PlayerButtonEnable = false;
				playerButton.rectTransform.sizeDelta = new Vector2(50f, 50f);
				playerButton.iconSizeMultiplier *= 0.2f;
				playerButton.shouldFitIconToParent = result == null;
				playerButton.SetPlayerData(participator, result);
				playerButton.ShowLocalPlayerLabel(participator.UserId == AccountManager.PlayerAccountId);
				((Component)playerButton.shine).gameObject.SetActive(false);
				if (playerButton.label.PreferedValues.y > num2)
				{
					num2 = playerButton.label.PreferedValues.y;
				}
				playerButton.OnClicked += delegate
				{
					OnShowMatchmakingPlayerInfo(participator?.UserId);
				};
				playerButtons.Add(playerButton);
			}
			for (int num3 = summaryViewModel.Participators.Count; num3 < num; num3++)
			{
				PlayerButton playerButton2 = Object.Instantiate<PlayerButton>(playerButtonPrefab, ((Component)gridLayout).transform);
				playerButton2.label.Key = "onlineview.game.missingplayer";
				((Component)playerButton2.shine).gameObject.SetActive(false);
				playerButton2.PlayerButtonEnable = false;
				playerButton2.ButtonEnabled = false;
				playerButtons.Add(playerButton2);
			}
			gridLayout.spacing = new Vector2(gridLayout.spacing.x, num2 + 10f);
			gridBottomSpacer.minHeight = num2 + 10f;
			int num4 = num - summaryViewModel.Participators.Count;
			string arg = ((num4 < 2) ? Localization.Get("misc.player") : Localization.Get("misc.players"));
			Description = Localization.Get("matchmakinggameinfo.waitingforplayers", num4, arg);
			((Component)deleteButton).gameObject.SetActive(true);
			deleteButton.Key = DeleteButtonKey;
		}
		buttonData = new PopupButtonData[1]
		{
			new PopupButtonData("buttons.back", PopupButtonData.States.None, OnBack)
		};
	}

	private void OnBack(int id, BaseEventData eventData = null)
	{
		GameManager.GetAnalyticsManager().SendEvent("match_info_click", new Dictionary<string, object> { { "option", "back" } });
	}

	protected async void OnShowMatchmakingPlayerInfo(Guid? id)
	{
		ParticipatorViewModel participatorFromUserId = GetParticipatorFromUserId(matchmakingSummaryViewModel.Participators, id);
		await ShowPlayerInfo(participatorFromUserId, id, participatorFromUserId.Name, isAutoPlay: false, 0, isHotseatGame: false);
	}

	protected void ShowMatchmakingLeavePopup()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("onlineview.game.leavematchmaking.title");
		basicPopup.Description = Localization.Get("onlineview.game.leavematchmaking.info");
		basicPopup.buttonData = new PopupButtonData[2]
		{
			new PopupButtonData("buttons.back"),
			new PopupButtonData("buttons.ok", PopupButtonData.States.Selected, OnLeaveMatchmakingOkClicked)
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}

	protected async void OnLeaveMatchmakingOkClicked(int id, BaseEventData eventData)
	{
		MatchmakingGameSummaryViewModel matchmakingGame = GameManager.GetRemoteGameDataManager().DeleteMatchmakingGame(matchmakingSummaryViewModel.Id);
		ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.LeaveMatchmakingGame(matchmakingSummaryViewModel.Id);
		if (serverResponse.Success)
		{
			Hide();
			return;
		}
		GameManager.GetRemoteGameDataManager().AddOrUpdateGameSummary(matchmakingGame);
		PopupManager.ShowErrorPopup(Localization.GetErrorMessage(serverResponse));
	}

	public override void ResetPopup()
	{
		base.ResetPopup();
		foreach (PlayerButton playerButton in playerButtons)
		{
			if (!((Object)(object)playerButton == (Object)null))
			{
				Object.Destroy((Object)(object)((Component)playerButton).gameObject);
			}
		}
		playerButtons.Clear();
	}
}
