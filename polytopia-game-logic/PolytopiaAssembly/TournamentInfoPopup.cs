using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TournamentInfoPopup : BasicPopup
{
	public enum Mode
	{
		None,
		Game,
		Matchmaking
	}

	private const int MAX_SHOWN_PLAYERS = 4;

	[Header("Tournament Popup")]
	[SerializeField]
	protected Image headerIcon;

	[SerializeField]
	protected PopupHeaderContainer headerContainer;

	[SerializeField]
	protected TournamentStateButtonWrapper tournamentStateButton;

	[SerializeField]
	protected TournamentSlotsButtonWrapper tournamentSlotsButton;

	[SerializeField]
	protected MoreTournamentInfoButtonWrapper moreTournamentInfoButton;

	[SerializeField]
	protected GridLayoutGroup gridLayout;

	[SerializeField]
	protected LayoutElement gridBottomSpacer;

	[SerializeField]
	protected TournamentStatusDisplay statusDisplay;

	public UIButtonBase.ColorStates defaultColors;

	public UIButtonBase.ColorStates leaveColors;

	[Header("Prefabs")]
	[SerializeField]
	protected PlayerButton playerButtonPrefab;

	protected TournamentViewModel tournamentViewModel;

	protected TournamentMemberViewModel localMember;

	protected List<PlayerButton> playerButtons = new List<PlayerButton>();

	protected bool hasOwner;

	protected bool isLocalPlayerOwner;

	protected Guid? relatedGameId;

	protected Guid? relatedLobbyId;

	private void OnEnable()
	{
		TournamentManager.OnTournamentUpdated += OnReceivedTournamentUpdate;
		BackendEvents.OnGameSummariesUpdated += OnReceivedGameUpdate;
		BackendEvents.OnLobbiesUpdated += OnReceivedGameUpdate;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		TournamentManager.OnTournamentUpdated -= OnReceivedTournamentUpdate;
		BackendEvents.OnGameSummariesUpdated -= OnReceivedGameUpdate;
		BackendEvents.OnLobbiesUpdated -= OnReceivedGameUpdate;
	}

	public override void Show(Vector2 origin)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.Show(origin);
		GameManager.GetAnalyticsManager().SendEvent("tournament_popup_view", new Dictionary<string, object>());
	}

	private void OnReceivedTournamentUpdate(TournamentViewModel tournamentViewModel)
	{
		if (!((Object)(object)this == (Object)null) && tournamentViewModel != null && !(tournamentViewModel.Id != this.tournamentViewModel.Id))
		{
			Log.Verbose("TournamentInfoPopup: Received updated tournament data", Array.Empty<object>());
			SetData(tournamentViewModel);
		}
	}

	private void OnReceivedGameUpdate()
	{
		if (!((Object)(object)this == (Object)null) && tournamentViewModel != null)
		{
			Log.Verbose("TournamentInfoPopup: Received updated game data", Array.Empty<object>());
			SetData(tournamentViewModel);
		}
	}

	public async void SetData(TournamentViewModel tournamentViewModel)
	{
		this.tournamentViewModel = tournamentViewModel;
		ResetPopup();
		if (tournamentViewModel != null)
		{
			Header = tournamentViewModel.Name;
			((Component)headerIcon).gameObject.SetActive(false);
			tournamentStateButton.SetData(GetTournamentStateDescription(tournamentViewModel.State));
			moreTournamentInfoButton.SetData(tournamentViewModel.OverviewUrl, PrepareTournamentInfo());
			if (tournamentViewModel.ReadyTime.HasValue && DateTime.UtcNow > tournamentViewModel.ReadyTime.Value)
			{
				int num = tournamentViewModel.TotalSlots - tournamentViewModel.AvailableSlots;
				tournamentSlotsButton.SetData(tournamentViewModel.TotalSlots, Localization.Get("onlineview.tournament.info.players.description.confirm", num, tournamentViewModel.NumberRegistered, tournamentViewModel.TotalSlots));
			}
			else
			{
				tournamentSlotsButton.SetData(tournamentViewModel.TotalSlots, Localization.Get("onlineview.tournament.info.players.description", tournamentViewModel.NumberRegistered, tournamentViewModel.TotalSlots));
			}
			if (tournamentViewModel.ScheduledStartTime.HasValue)
			{
				Description = LocalizationUtils.GetDateString(tournamentViewModel.ScheduledStartTime.Value);
			}
			AddPlayerButtons(tournamentViewModel);
			List<PopupButtonData> tempButtonData = new List<PopupButtonData>();
			((Component)statusDisplay).gameObject.SetActive(true);
			statusDisplay.LabelLinkCallback = OnStatusLabelClicked;
			statusDisplay.LoadIcon(null);
			if (IsTournamentAvailable(tournamentViewModel.State))
			{
				if (tournamentViewModel.PersonalViewModel.HasSignedUp && !tournamentViewModel.PersonalViewModel.PariticipationComplete)
				{
					if (tournamentViewModel.PersonalViewModel.HasConfirmed)
					{
						if (tournamentViewModel.State == TournamentState.Running)
						{
							if (tournamentViewModel.PersonalViewModel.WaitingForNextMatch)
							{
								statusDisplay.SetLabel(Localization.Get("onlineview.tournament.state.waiting"));
							}
							else if (tournamentViewModel.PersonalViewModel.OnWaitingList)
							{
								statusDisplay.SetLabel(Localization.Get("onlineview.tournament.state.reserve.nospot"));
							}
							else
							{
								relatedLobbyId = await FindLobbyStartedByTournament(tournamentViewModel.Id);
								relatedGameId = await FindGameStartedByTournament(tournamentViewModel.Id);
								if (relatedLobbyId.HasValue)
								{
									statusDisplay.SetLabel(string.Format("{0}\n\n<b><u><link=RELATED_LOBBY_ID>{1}</link></b></u>", Localization.Get("onlineview.tournament.state.ongoing"), "View Game"));
								}
								else if (relatedGameId.HasValue)
								{
									statusDisplay.SetLabel(string.Format("{0}\n\n<b><u><link=RELATED_GAME_ID>{1}</link></b></u>", Localization.Get("onlineview.tournament.state.ongoing"), "View Game"));
								}
								else
								{
									statusDisplay.SetLabel(Localization.Get("onlineview.tournament.state.ongoing"));
								}
							}
						}
						else
						{
							string text = "";
							if (tournamentViewModel.PersonalViewModel.OnWaitingList)
							{
								text = string.Format("{0}\n", Localization.Get("onlineview.tournament.state.reserve.waiting"));
							}
							if (tournamentViewModel.ScheduledStartTime.HasValue && DateTime.UtcNow < tournamentViewModel.ScheduledStartTime.Value)
							{
								text += Localization.Get("onlineview.tournament.state.starting.in");
								statusDisplay.SetLabel(text, tournamentViewModel.ScheduledStartTime.Value, RefreshPopup);
							}
							else
							{
								text += Localization.Get("onlineview.tournament.state.starting.soon");
								statusDisplay.SetLabel(text);
							}
						}
					}
					else if (tournamentViewModel.ReadyTime.HasValue && DateTime.UtcNow < tournamentViewModel.ReadyTime.Value)
					{
						statusDisplay.SetLabel(Localization.Get("onlineview.tournament.state.confirm.in"), tournamentViewModel.ReadyTime.Value, RefreshPopup);
						tempButtonData.Add(new PopupButtonData("buttons.confirm", PopupButtonData.States.Disabled, OnConfirmParticipation, 1, closesPopup: false, defaultColors));
					}
					else if (tournamentViewModel.ReadyTime.HasValue && DateTime.UtcNow > tournamentViewModel.ReadyTime.Value)
					{
						if (DateTime.UtcNow < tournamentViewModel.ScheduledStartTime.Value)
						{
							statusDisplay.SetLabel(Localization.Get("onlineview.tournament.state.confirm.before"), tournamentViewModel.ScheduledStartTime.Value, RefreshPopup);
							tempButtonData.Add(new PopupButtonData("buttons.confirm", PopupButtonData.States.Selected, OnConfirmParticipation, 1, closesPopup: false));
						}
						else
						{
							statusDisplay.SetLabel(Localization.Get("onlineview.tournament.state.confirm"));
							tempButtonData.Add(new PopupButtonData("buttons.confirm", PopupButtonData.States.Selected, OnConfirmParticipation, 1, closesPopup: false));
						}
					}
					else
					{
						statusDisplay.SetLabel(Localization.Get("onlineview.tournament.state.confirm"));
						tempButtonData.Add(new PopupButtonData("buttons.confirm", PopupButtonData.States.Selected, OnConfirmParticipation, 1, closesPopup: false));
					}
				}
				else if (tournamentViewModel.PersonalViewModel.PariticipationComplete)
				{
					if (tournamentViewModel.PersonalViewModel.BestPlacment.HasValue && tournamentViewModel.PersonalViewModel.WorstPlacement.HasValue)
					{
						if (tournamentViewModel.PersonalViewModel.BestPlacment.Value == tournamentViewModel.PersonalViewModel.WorstPlacement.Value)
						{
							statusDisplay.SetLabel(Localization.Get("onlineview.tournament.state.placement.single", LocalizationUtils.AddOrdinal(tournamentViewModel.PersonalViewModel.BestPlacment.Value)));
						}
						else
						{
							statusDisplay.SetLabel(Localization.Get("onlineview.tournament.state.placement.multiple", tournamentViewModel.PersonalViewModel.WorstPlacement.Value));
						}
					}
					else
					{
						statusDisplay.SetLabel(Localization.Get("onlineview.tournament.state.placement.waiting"));
					}
				}
				else
				{
					((Component)statusDisplay).gameObject.SetActive(false);
					tempButtonData.Add(new PopupButtonData("buttons.signup", PopupButtonData.States.Selected, OnSignup, 1, closesPopup: false));
				}
			}
			else
			{
				statusDisplay.SetLabel(Localization.Get("onlineview.tournament.state.cancelled"));
			}
			tempButtonData.Insert(0, new PopupButtonData("buttons.back", PopupButtonData.States.None, OnBack));
			buttonData = tempButtonData.ToArray();
			topButton.Key = "buttons.leave";
			topButton.BgColorStates = leaveColors;
			((Component)topButton).gameObject.SetActive(tournamentViewModel.PersonalViewModel.HasSignedUp && tournamentViewModel.State == TournamentState.Published);
			headerContainer.UpdateLayout();
		}
		else
		{
			Description = Localization.Get("gameinfo.nodata");
			Header = Localization.Get("gameinfo.nodata.title");
			buttonData = new PopupButtonData[1]
			{
				new PopupButtonData("buttons.ok", PopupButtonData.States.Selected, OnBack)
			};
			((Component)topButton).gameObject.SetActive(false);
		}
		if (IsShowing())
		{
			RefreshButtons();
			((MonoBehaviour)this).StartCoroutine(RefreshHeight());
		}
	}

	public void RefreshPopup()
	{
		if (!((Object)(object)this == (Object)null) && ((Component)this).gameObject.activeInHierarchy)
		{
			SetData(tournamentViewModel);
		}
	}

	private void AddPlayerButtons(TournamentViewModel viewModel)
	{
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		ClearPlayerButtons();
		int num = ((viewModel.TopMembers != null) ? viewModel.TopMembers.Count : 0);
		float num2 = 0f;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				TournamentTopMemberViewModel player = viewModel.TopMembers[i];
				SerializationHelpers.FromByteArray<AvatarState>(player.AvatarStateData, out var result);
				PlayerButton playerButton = Object.Instantiate<PlayerButton>(playerButtonPrefab, ((Component)gridLayout).transform);
				playerButton.id = i;
				playerButton.PlayerButtonEnable = false;
				playerButton.rectTransform.sizeDelta = new Vector2(50f, 50f);
				playerButton.iconSizeMultiplier *= 0.2f;
				playerButton.shouldFitIconToParent = AccountManager.AvatarState == null;
				playerButton.SetPlayerData(player.PolytopiaUserId, player.Alias, result);
				((Component)playerButton.shine).gameObject.SetActive(false);
				((Component)playerButton.bg).gameObject.SetActive(false);
				((Component)playerButton.outline).gameObject.SetActive(false);
				playerButton.BadgeEnabled = player.IsConfirmed;
				playerButton.SetBadge("actionIcons_endTurn", ColorConstants.green);
				playerButton.ShowLocalPlayerLabel(player.PolytopiaUserId == AccountManager.PlayerAccountId);
				if (playerButton.label.PreferedValues.y > num2)
				{
					num2 = playerButton.label.PreferedValues.y;
				}
				playerButton.OnClicked += delegate(int id, BaseEventData eventData)
				{
					OnShowPlayerInfo(id, player);
				};
				playerButtons.Add(playerButton);
			}
		}
		int num3 = tournamentViewModel.NumberRegistered - num;
		if (num3 > 0)
		{
			PlayerButton playerButton2 = Object.Instantiate<PlayerButton>(playerButtonPrefab, ((Component)gridLayout).transform);
			playerButton2.PlayerButtonEnable = false;
			playerButton2.ButtonEnabled = false;
			playerButton2.rectTransform.sizeDelta = new Vector2(50f, 50f);
			((Component)playerButton2.text).gameObject.SetActive(true);
			((TMP_Text)playerButton2.text).text = $"+{num3}";
			((Component)playerButton2.label).gameObject.SetActive(false);
			((Component)playerButton2.icon).gameObject.SetActive(false);
			((Component)playerButton2.shine).gameObject.SetActive(false);
			((Component)playerButton2.bg).gameObject.SetActive(false);
			playerButtons.Add(playerButton2);
			if (playerButton2.label.PreferedValues.y > num2)
			{
				num2 = playerButton2.label.PreferedValues.y;
			}
		}
		gridLayout.spacing = new Vector2(gridLayout.spacing.x, num2);
		gridBottomSpacer.minHeight = num2;
	}

	private async Task<Guid?> FindLobbyStartedByTournament(Guid tournamentId)
	{
		List<LobbyGameViewModel> list = await GameManager.GetLobbyManager().GetLobbies();
		for (int i = 0; i < list.Count; i++)
		{
			LobbyGameViewModel lobbyGameViewModel = list[i];
			if (lobbyGameViewModel.GameContext != null && lobbyGameViewModel.GameContext.ExternalTournamentId.HasValue && lobbyGameViewModel.GameContext.ExternalTournamentId.Value == tournamentId)
			{
				return lobbyGameViewModel.Id;
			}
		}
		return null;
	}

	private async Task<Guid?> FindGameStartedByTournament(Guid tournamentId)
	{
		List<GameSummaryViewModel> list = await GameManager.GetRemoteGameDataManager().GetGameSummaryViewModels();
		for (int i = 0; i < list.Count; i++)
		{
			GameSummaryViewModel gameSummaryViewModel = list[i];
			if (gameSummaryViewModel.GameContext != null && gameSummaryViewModel.GameContext.ExternalTournamentId.HasValue && gameSummaryViewModel.GameContext.ExternalTournamentId.Value == tournamentId)
			{
				return gameSummaryViewModel.GameId;
			}
		}
		return null;
	}

	private TournamentMemberViewModel GetLocalMember(TournamentViewModel tournamentViewModel)
	{
		foreach (TournamentMemberViewModel member in tournamentViewModel.Members)
		{
			if (member.PolytopiaUserId == AccountManager.PlayerAccountId)
			{
				return member;
			}
		}
		return null;
	}

	public string PrepareTournamentInfo()
	{
		string text = string.Empty;
		List<string> list = new List<string>();
		list.Add(string.Format("{0}: {1}", Localization.Get("onlineview.tournament.info.name"), tournamentViewModel.Name));
		list.Add(string.Format("{0}: {1}", Localization.Get("onlineview.tournament.info.format"), GetTournamentFormatDescription(tournamentViewModel.TournamentFormat)));
		list.Add(string.Format("{0}: {1}", Localization.Get("onlineview.tournament.info.created"), tournamentViewModel.DateCreated.ToString("d")));
		if (tournamentViewModel.StartTime.HasValue)
		{
			list.Add(string.Format("{0}: {1}", Localization.Get("onlineview.tournament.info.starttime"), tournamentViewModel.StartTime.Value.ToString("g")));
		}
		else if (tournamentViewModel.ScheduledStartTime.HasValue)
		{
			list.Add(string.Format("{0}: {1}", Localization.Get("onlineview.tournament.info.starttime"), tournamentViewModel.ScheduledStartTime.Value.ToString("g")));
		}
		if (tournamentViewModel.EndTime.HasValue)
		{
			list.Add(string.Format("{0}: {1}", Localization.Get("onlineview.tournament.info.endtime"), tournamentViewModel.EndTime.Value.ToString("g")));
		}
		if (!string.IsNullOrEmpty(tournamentViewModel.ContactUrl))
		{
			list.Add(string.Format("{0}: <link=TOURNAMENT_CONTACT_INFO>{1}</link>", Localization.Get("onlineview.tournament.info.contact"), tournamentViewModel.ContactUrl));
		}
		list.Add("");
		list.Add(string.Format("<link=TOURNAMENT_LINK><b><u>{0}</u></b></link>", Localization.Get("onlineview.tournament.info.overview")));
		foreach (string item in list)
		{
			text = text + item + "\n";
		}
		return text;
	}

	protected string GetTournamentFormatDescription(TournamentFormat format)
	{
		return format switch
		{
			TournamentFormat.SingleElimination => "Single Elimination", 
			TournamentFormat.RoundRobinSingleElimination => "Groups + SE", 
			TournamentFormat.RoundRobinX2SingleElimination => "Groups + Groups + SE", 
			TournamentFormat.DoubleElimination => "Double Elimination", 
			TournamentFormat.RoundRobinDoubleElimination => "Groups + DE", 
			TournamentFormat.RoundRobinX2DoubleElimination => "Groups + Groups + DE", 
			TournamentFormat.Swiss => "Swiss", 
			TournamentFormat.RoundRobinSwiss => "Groups + Swiss", 
			TournamentFormat.RoundRobinX2Swiss => "Groups + Groups + Swiss", 
			TournamentFormat.RoundRobin => "Groups Only", 
			_ => "Unknown", 
		};
	}

	protected string GetTournamentStateDescription(TournamentState state)
	{
		switch (state)
		{
		case TournamentState.Unknown:
		case TournamentState.Unpublished:
		case TournamentState.Cancelling:
		case TournamentState.Cancelled:
		case TournamentState.Closing:
		case TournamentState.Closed:
			return Localization.Get("onlineview.tournament.info.state.value.cancelled");
		case TournamentState.Published:
			return Localization.Get("onlineview.tournament.info.state.value.open");
		case TournamentState.Starting:
		case TournamentState.Running:
			return Localization.Get("onlineview.tournament.info.state.value.ongoing");
		case TournamentState.Concluded:
		case TournamentState.Completed:
			return Localization.Get("onlineview.tournament.info.state.value.completed");
		default:
			return "Unknown";
		}
	}

	protected bool IsTournamentAvailable(TournamentState state)
	{
		switch (state)
		{
		case TournamentState.Unknown:
		case TournamentState.Unpublished:
		case TournamentState.Cancelling:
		case TournamentState.Cancelled:
		case TournamentState.Closing:
		case TournamentState.Closed:
			return false;
		default:
			return true;
		}
	}

	protected async void OnShowPlayerInfo(int id, TournamentTopMemberViewModel user)
	{
		if (user.PolytopiaUserId == AccountManager.PlayerAccountId)
		{
			PlayerData playerData = new PlayerData
			{
				profile = new PlayerProfileState
				{
					numFriends = user.NumFriends.GetValueOrDefault(),
					numMultiplayerGames = user.NumMultiplayergames.GetValueOrDefault(),
					gameVersion = PlayerDataUtils.GetCurrentPlatformGameVersion(user.GameVersions),
					multiplayerRating = user.MultiplayerRating.GetValueOrDefault(),
					lastLoginDate = user.LastLoginDate,
					avatarState = AccountManager.AvatarState,
					name = user.UserName,
					id = user.PolytopiaUserId,
					numGames = user.NumGames.GetValueOrDefault()
				}
			};
			PopupManager.GetFriendInfoPopup(user.PolytopiaUserId, user.Alias, PlayerData.State.IsYou, AccountManager.AvatarState, playerData).Show(InputManager.GetInputPosition());
			return;
		}
		PlayerData playerData2 = await AccountManager.GetFriendPlayerDataWithId(user.PolytopiaUserId);
		ServerResponseList<PolytopiaFriendViewModel> friendErrorResponse = AccountManager.GetFriendErrorResponse();
		if (playerData2 != null)
		{
			PopupManager.GetFriendInfoPopup(playerData2.profile.id, playerData2.GetName(), playerData2.state, playerData2.profile.avatarState, playerData2).Show();
			return;
		}
		if (friendErrorResponse != null)
		{
			PopupManager.ShowErrorPopup(Localization.GetErrorMessage(friendErrorResponse));
			return;
		}
		SerializationHelpers.FromByteArray<AvatarState>(user.AvatarStateData, out var result);
		playerData2 = new PlayerData
		{
			profile = new PlayerProfileState
			{
				numFriends = user.NumFriends.GetValueOrDefault(),
				numMultiplayerGames = user.NumMultiplayergames.GetValueOrDefault(),
				gameVersion = PlayerDataUtils.GetCurrentPlatformGameVersion(user.GameVersions),
				multiplayerRating = user.MultiplayerRating.GetValueOrDefault(),
				lastLoginDate = user.LastLoginDate,
				avatarState = result,
				name = user.UserName,
				id = user.PolytopiaUserId,
				numGames = user.NumGames.GetValueOrDefault()
			}
		};
		PopupManager.GetFriendInfoPopup(user.PolytopiaUserId, user.Alias, PlayerData.State.None, result, playerData2).Show();
	}

	public void ShowTournament()
	{
		GameManager.GetAnalyticsManager().SendEvent("tournament_info_click", new Dictionary<string, object> { { "option", "view" } });
		NativeHelpers.OpenURL(tournamentViewModel.OverviewUrl);
	}

	protected async void OnSignup(int id, BaseEventData eventData)
	{
		await GameManager.GetTournamentManager().JoinTournament(tournamentViewModel.Id);
	}

	protected async void OnConfirmParticipation(int id, BaseEventData eventData)
	{
		await GameManager.GetTournamentManager().ConfirmTournamentParticipation(tournamentViewModel.Id);
	}

	public async void OnLeave()
	{
		await GameManager.GetTournamentManager().LeaveTournament(tournamentViewModel.Id);
	}

	private void OnBack(int id, BaseEventData eventData = null)
	{
		GameManager.GetAnalyticsManager().SendEvent("tournament_info_click", new Dictionary<string, object> { { "option", "back" } });
	}

	private void OnStatusLabelClicked(string linkId, string linkText)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		LobbyGameViewModel lobbyGameViewModel;
		if (!(linkId == "RELATED_LOBBY_ID"))
		{
			if (linkId == "RELATED_GAME_ID" && relatedGameId.HasValue && GameManager.GetRemoteGameDataManager().TryGetGameSummaryViewModel(relatedGameId.Value, out var summaryViewModel))
			{
				Hide();
				UIManager.OpenMultiplayerScreen();
				GameInfoPopup gameInfoPopup = PopupManager.GetGameInfoPopup();
				gameInfoPopup.SetData(summaryViewModel);
				gameInfoPopup.Show(InputManager.GetInputPosition());
			}
		}
		else if (relatedLobbyId.HasValue && GameManager.GetLobbyManager().TryGetCachedLobby(relatedLobbyId.Value, out lobbyGameViewModel))
		{
			Hide();
			UIManager.OpenMultiplayerScreen();
			LobbyPopup lobbyPopup = PopupManager.GetLobbyPopup();
			lobbyPopup.SetData(lobbyGameViewModel);
			lobbyPopup.Show(InputManager.GetInputPosition());
		}
	}

	public override void ResetPopup()
	{
		base.ResetPopup();
		relatedLobbyId = null;
		relatedGameId = null;
		ClearPlayerButtons();
	}

	private void ClearPlayerButtons()
	{
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
