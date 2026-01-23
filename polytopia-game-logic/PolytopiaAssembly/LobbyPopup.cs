using System;
using System.Collections.Generic;
using Polytopia.Data;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyPopup : BasicPopup
{
	public enum Mode
	{
		None,
		Game,
		Matchmaking
	}

	protected const float TIME_UPDATE_IMTERVAL = 1f;

	[Header("Lobby Popup")]
	[SerializeField]
	protected UIImageButton headerIcon;

	[SerializeField]
	protected UITextButton leaveButton;

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

	public UIButtonBase.ColorStates defaultColors;

	public UIButtonBase.ColorStates leaveColors;

	[Header("prefabs")]
	[SerializeField]
	protected PlayerButton playerButtonPrefab;

	[SerializeField]
	protected PlayerButton addPlayerButtonPrefab;

	protected LobbyGameViewModel lobbyGameViewModel;

	protected ParticipatorViewModel localParticipator;

	protected ParticipatorViewModel ownerParticipator;

	protected InvitePopup invitePopup;

	protected PlayerButton addPlayerButton;

	protected List<PlayerButton> playerButtons = new List<PlayerButton>();

	protected List<int> addedBots = new List<int>();

	protected List<int> removedBots = new List<int>();

	protected List<int> presentedBots = new List<int>();

	protected List<ParticipatorViewModel> addedPlayers = new List<ParticipatorViewModel>();

	protected List<Guid> removedPlayers = new List<Guid>();

	protected List<ParticipatorViewModel> presentedPlayers = new List<ParticipatorViewModel>();

	protected bool hasOwner;

	protected bool isLocalPlayerOwner;

	protected bool isWaitingForStartTime;

	protected float timerUpdate;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	private Action headerIconAction;

	private int MaxPlayers
	{
		get
		{
			int mapSize = lobbyGameViewModel.MapSize;
			return Mathf.Min(GameManager.GetMaxOpponents(), MapDataExtensions.GetMaximumOpponentCountForMapSize(mapSize)) + 1;
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
		BackendEvents.OnReceivedLobbyUpdate += OnReceivedLobbyUpdate;
		headerIcon.OnClicked += OnHeaderIconClicked;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		BackendEvents.OnReceivedLobbyUpdate -= OnReceivedLobbyUpdate;
		headerIcon.OnClicked -= OnHeaderIconClicked;
	}

	public override void Show(Vector2 origin)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.Show(origin);
		GameManager.GetAnalyticsManager().SendEvent("lobby_popup_view", new Dictionary<string, object>());
	}

	protected override void Update()
	{
		base.Update();
		if (!((Object)(object)this != (Object)null) || lobbyGameViewModel == null || !lobbyGameViewModel.StartTime.HasValue || !isWaitingForStartTime)
		{
			return;
		}
		if (timerUpdate >= 1f)
		{
			if (DateTime.UtcNow >= lobbyGameViewModel.StartTime.Value)
			{
				RefreshPopup();
				isWaitingForStartTime = false;
			}
			timerUpdate = 0f;
		}
		timerUpdate += Time.deltaTime;
	}

	private void OnReceivedLobbyUpdate(LobbyGameViewModel lobbyGameViewModel)
	{
		if (lobbyGameViewModel != null && lobbyGameViewModel.Id != this.lobbyGameViewModel.Id)
		{
			return;
		}
		Log.Info("Got lobby update", Array.Empty<object>());
		if (lobbyGameViewModel.StartedGameId.HasValue && !lobbyGameViewModel.IsPersistent)
		{
			Description = Localization.Get("onlineview.lobby.starting");
			for (int i = 0; i < Buttons.Length; i++)
			{
				Buttons[i].ButtonEnabled = false;
			}
		}
		else if (!LobbyManager.IsUserParticipant(lobbyGameViewModel, AccountManager.PlayerAccountId))
		{
			Hide();
		}
		else
		{
			SetData(lobbyGameViewModel);
		}
	}

	public void SetData(LobbyGameViewModel lobbyGameViewModel)
	{
		this.lobbyGameViewModel = lobbyGameViewModel;
		ResetPopup();
		if (lobbyGameViewModel != null)
		{
			hasOwner = lobbyGameViewModel.OwnerId != Guid.Empty;
			isLocalPlayerOwner = lobbyGameViewModel.OwnerId == AccountManager.PlayerAccountId;
			LobbyManager.GetAcceptedPlayersCount(lobbyGameViewModel);
			localParticipator = LobbyManager.GetLocalParticipator(lobbyGameViewModel);
			ownerParticipator = LobbyManager.GetOwnerParticipator(lobbyGameViewModel);
			if (lobbyGameViewModel.ChallengermodeGameId.HasValue)
			{
				LoadHeaderIcon("externalIcons_cm");
				headerIconAction = delegate
				{
					NativeHelpers.OpenURL($"https://www.challengermode.com/games/{lobbyGameViewModel.ChallengermodeGameId.Value}");
				};
			}
			else if (lobbyGameViewModel.TimeLimit == -1)
			{
				LoadHeaderIcon("live-icon");
				headerIconAction = null;
			}
			else
			{
				SetHeaderIcon(null);
				headerIconAction = null;
			}
			Header = lobbyGameViewModel.Name;
			mapSizeButton.SetData(lobbyGameViewModel.MapSize, lobbyGameViewModel.MapPreset);
			gameModeButton.SetData(lobbyGameViewModel.GameMode, GameType.Multiplayer, lobbyGameViewModel.ScoreLimit);
			timerInfoButton.SetData(lobbyGameViewModel.TimeLimit);
			((Component)moreInfoButton).gameObject.SetActive(false);
			UpdatePresentedPlayers();
			Description = LobbyManager.GetLobbyDescription(lobbyGameViewModel, AccountManager.PlayerAccountId);
			List<PopupButtonData> list = new List<PopupButtonData>();
			if (localParticipator != null && localParticipator.InvitationState == PlayerInvitationState.Accepted)
			{
				((Component)leaveButton).gameObject.SetActive(true);
				leaveButton.Key = "buttons.leave";
				list.Add(new PopupButtonData("buttons.back"));
				if (isLocalPlayerOwner || !hasOwner)
				{
					if (lobbyGameViewModel.StartTime.HasValue && lobbyGameViewModel.StartTime.Value > DateTime.UtcNow)
					{
						isWaitingForStartTime = true;
					}
					bool flag = LobbyManager.CanBeStartedByPlayer(lobbyGameViewModel, AccountManager.PlayerAccountId);
					list.Add(new PopupButtonData("buttons.start", flag ? PopupButtonData.States.Selected : PopupButtonData.States.Disabled, OnStartClicked, -1, closesPopup: false, defaultColors));
				}
			}
			else if (localParticipator == null)
			{
				((Component)leaveButton).gameObject.SetActive(false);
				list.Add(new PopupButtonData("buttons.leave"));
				list.Add(new PopupButtonData("buttons.join", PopupButtonData.States.Selected, OnAcceptedInvite, -1, closesPopup: false));
			}
			else if (lobbyGameViewModel.MatchmakingGameId.HasValue)
			{
				((Component)leaveButton).gameObject.SetActive(false);
				list.Add(new PopupButtonData("buttons.decline", PopupButtonData.States.None, OnDeclinedInvite, -1, closesPopup: true, ColorConstants.redButtonColorStates));
				list.Add(new PopupButtonData("buttons.accept", PopupButtonData.States.Selected, OnAcceptedInvite, -1, closesPopup: false));
			}
			else
			{
				((Component)leaveButton).gameObject.SetActive(true);
				leaveButton.Key = "buttons.decline";
				list.Add(new PopupButtonData("buttons.back"));
				list.Add(new PopupButtonData("buttons.accept", PopupButtonData.States.Selected, OnAcceptedInvite, -1, closesPopup: false));
			}
			buttonData = list.ToArray();
		}
		else
		{
			Description = Localization.Get("gameinfo.nodata");
			Header = Localization.Get("gameinfo.nodata.title");
			buttonData = new PopupButtonData[1]
			{
				new PopupButtonData("buttons.ok", PopupButtonData.States.Selected, OnBack)
			};
			((Component)leaveButton).gameObject.SetActive(false);
			gameModeButton.SetData(GameMode.None, GameType.Multiplayer);
		}
		if (IsShowing())
		{
			RefreshButtons();
			((MonoBehaviour)this).StartCoroutine(RefreshHeight());
		}
	}

	public void RefreshPopup()
	{
		if (!((Object)(object)this == (Object)null))
		{
			SetData(lobbyGameViewModel);
		}
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

	private void RemoveUserIds(List<ParticipatorViewModel> users, List<Guid> userIds)
	{
		for (int i = 0; i < userIds.Count; i++)
		{
			for (int num = users.Count - 1; num >= 0; num--)
			{
				if (userIds[i] == users[num].UserId)
				{
					users.RemoveAt(num);
					break;
				}
			}
		}
	}

	private void RemoveValues(List<int> vals, List<int> removedVals)
	{
		for (int i = 0; i < removedVals.Count; i++)
		{
			for (int num = vals.Count - 1; num >= 0; num--)
			{
				if (removedVals[i] == vals[num])
				{
					vals.RemoveAt(num);
					break;
				}
			}
		}
	}

	private void UpdatePresentedPlayers()
	{
		presentedPlayers.Clear();
		presentedPlayers.AddRange(lobbyGameViewModel.Participators);
		presentedPlayers.AddRange(addedPlayers);
		RemoveUserIds(presentedPlayers, removedPlayers);
		presentedBots.Clear();
		presentedBots.AddRange(lobbyGameViewModel.Bots);
		presentedBots.AddRange(addedBots);
		RemoveValues(presentedBots, removedBots);
		presentedBots.Sort();
		AddPlayerButtons();
	}

	private void AddPlayerButtons()
	{
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		if (lobbyGameViewModel == null)
		{
			return;
		}
		ClearPlayerButtons();
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < presentedPlayers.Count; i++)
		{
			ParticipatorViewModel participator = presentedPlayers[i];
			if (participator.InvitationState != PlayerInvitationState.Declined && participator.InvitationState != PlayerInvitationState.Done)
			{
				num2++;
				SerializationHelpers.FromByteArray<AvatarState>(participator?.AvatarStateData, out var result);
				PlayerButton playerButton = Object.Instantiate<PlayerButton>(playerButtonPrefab, ((Component)gridLayout).transform);
				playerButton.id = i;
				playerButton.PlayerButtonEnable = false;
				playerButton.rectTransform.sizeDelta = new Vector2(50f, 50f);
				playerButton.iconSizeMultiplier *= 0.2f;
				playerButton.shouldFitIconToParent = result == null;
				playerButton.SetPlayerData(participator, result);
				((Component)playerButton.shine).gameObject.SetActive(false);
				((Component)playerButton.bg).gameObject.SetActive(false);
				((Component)playerButton.outline).gameObject.SetActive(false);
				playerButton.BadgeEnabled = true;
				playerButton.SetBadge(GetBadgeIdForParticipator(participator), GetColorForParticipator(participator));
				if (playerButton.label.PreferedValues.y > num)
				{
					num = playerButton.label.PreferedValues.y;
				}
				playerButton.OnClicked += delegate(int id, BaseEventData eventData)
				{
					OnShowPlayerInfo(id, participator?.UserId);
				};
				playerButtons.Add(playerButton);
			}
		}
		for (int num3 = 0; num3 < presentedBots.Count; num3++)
		{
			PlayerButton playerButton2 = Object.Instantiate<PlayerButton>(playerButtonPrefab, ((Component)gridLayout).transform);
			playerButton2.id = num3;
			playerButton2.PlayerButtonEnable = false;
			playerButton2.rectTransform.sizeDelta = new Vector2(50f, 50f);
			playerButton2.iconSizeMultiplier *= 0.2f;
			playerButton2.shouldFitIconToParent = true;
			playerButton2.SetBotData(Localization.Get("friendlist.bot", Localization.Get(GameModeUtils.GetDifficultyName((GameSettings.Difficulties)presentedBots[num3]))));
			((Component)playerButton2.shine).gameObject.SetActive(false);
			((Component)playerButton2.outline).gameObject.SetActive(false);
			if (playerButton2.label.PreferedValues.y > num)
			{
				num = playerButton2.label.PreferedValues.y;
			}
			playerButton2.OnClicked += delegate(int id, BaseEventData eventData)
			{
				OnShowBotInfo(id);
			};
			playerButtons.Add(playerButton2);
		}
		if (lobbyGameViewModel.MatchmakingGameId.HasValue)
		{
			for (int num4 = 0; num4 < lobbyGameViewModel.OpponentCount + 1 - num2; num4++)
			{
				PlayerButton playerButton3 = Object.Instantiate<PlayerButton>(playerButtonPrefab, ((Component)gridLayout).transform);
				playerButton3.label.Key = "onlineview.game.waitingforplayer";
				((Component)playerButton3.shine).gameObject.SetActive(false);
				playerButton3.PlayerButtonEnable = false;
				playerButton3.OnClicked += delegate(int id, BaseEventData eventData)
				{
					OnShowSlotInfo(id);
				};
				playerButtons.Add(playerButton3);
			}
		}
		else if (LobbyManager.GetActiveParticipatorsCount(lobbyGameViewModel) < MaxPlayers && lobbyGameViewModel.OwnerId == AccountManager.PlayerAccountId)
		{
			addPlayerButton = Object.Instantiate<PlayerButton>(addPlayerButtonPrefab, ((Component)gridLayout).transform);
			addPlayerButton.label.Key = "onlineview.lobby.player.add";
			addPlayerButton.OnClicked += delegate
			{
				OnShowInvitePlayer();
			};
			if (addPlayerButton.label.PreferedValues.y > num)
			{
				num = addPlayerButton.label.PreferedValues.y;
			}
			playerButtons.Add(addPlayerButton);
		}
		gridLayout.spacing = new Vector2(gridLayout.spacing.x, num + 10f);
		gridBottomSpacer.minHeight = num + 10f;
	}

	protected async void OnShowPlayerInfo(int id, Guid? userId)
	{
		ParticipatorViewModel participator = GetParticipatorFromUserId(lobbyGameViewModel.Participators, userId);
		if (participator == null)
		{
			Log.Warning("Missing participator", Array.Empty<object>());
			return;
		}
		PlayerData playerData = await AccountManager.GetFriendPlayerDataWithId(userId);
		SerializationHelpers.FromByteArray<AvatarState>(participator?.AvatarStateData, out var result);
		if (playerData == null)
		{
			playerData = new PlayerData
			{
				state = PlayerData.State.None,
				profile = new PlayerProfileState
				{
					numFriends = participator.NumberOfFriends,
					numMultiplayerGames = participator.NumberOfMultiplayerGames,
					gameVersion = PlayerDataUtils.GetCurrentPlatformGameVersion(participator.GameVersion),
					multiplayerRating = participator.MultiplayerRating,
					lastLoginDate = DateTime.Now,
					avatarState = result,
					id = participator.UserId,
					name = participator.Name,
					numGames = participator.NumberOfMultiplayerGames
				}
			};
		}
		FriendInfoPopup friendInfoPopup = PopupManager.GetFriendInfoPopup();
		friendInfoPopup.SetLobbyData(participator.UserId, participator.Name, playerData.state, result, playerData, participator.InvitationState);
		if (lobbyGameViewModel.OwnerId == AccountManager.PlayerAccountId && participator.UserId != AccountManager.PlayerAccountId)
		{
			friendInfoPopup.buttonData = new PopupButtonData[2]
			{
				new PopupButtonData("buttons.back"),
				new PopupButtonData("buttons.remove", PopupButtonData.States.None, delegate
				{
					OnRemovePlayer(participator.UserId);
				}, id)
			};
			friendInfoPopup.Buttons[1].BgColorStates.defaultColor = ColorConstants.red;
		}
		else
		{
			friendInfoPopup.buttonData = new PopupButtonData[1]
			{
				new PopupButtonData("buttons.back")
			};
		}
		friendInfoPopup.Show(InputManager.GetInputPosition());
	}

	protected void OnShowSlotInfo(int index)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("onlineview.game.missingplayer");
		basicPopup.Description = Localization.Get("playerinfopopup.waiting.description");
		basicPopup.buttonData = new PopupButtonData[1]
		{
			new PopupButtonData("buttons.ok")
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}

	protected void OnShowBotInfo(int index)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.AutoCapitalizeHeader = false;
		GameSettings.Difficulties difficulty = (GameSettings.Difficulties)presentedBots[index];
		basicPopup.Header = Localization.Get("friendlist.bot", Localization.Get(GameModeUtils.GetDifficultyName(difficulty)));
		basicPopup.Description = Localization.Get("playerinfopopup.bot.description");
		basicPopup.buttonData = new PopupButtonData[1]
		{
			new PopupButtonData("buttons.ok")
		};
		basicPopup.TopButtonData = new PopupButtonData("buttons.remove", PopupButtonData.States.None, delegate
		{
			OnRemoveBot(difficulty);
		}, index);
		basicPopup.Show(InputManager.GetInputPosition());
	}

	protected async void OnRemovePlayer(Guid userId)
	{
		removedPlayers.Add(userId);
		UpdatePresentedPlayers();
		ServerResponse<BoolResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.ModifyPlayersInLobby(new ModifyPlayersInLobbyBindingModel
		{
			LobbyId = lobbyGameViewModel.Id,
			RemovePlayers = new List<Guid> { userId }
		});
		removedPlayers.Remove(userId);
		UpdatePresentedPlayers();
		if (!serverResponse.Success)
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
			return;
		}
		Log.Info("Removed player: {0}", new object[1] { userId });
	}

	protected async void OnRemoveBot(GameSettings.Difficulties difficulty)
	{
		removedBots.Add((int)difficulty);
		UpdatePresentedPlayers();
		ServerResponse<BoolResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.ModifyPlayersInLobby(new ModifyPlayersInLobbyBindingModel
		{
			LobbyId = lobbyGameViewModel.Id,
			Bots = new List<int>(presentedBots)
		});
		removedBots.Remove((int)difficulty);
		UpdatePresentedPlayers();
		if (!serverResponse.Success)
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
			return;
		}
		Log.Info("Removed bot: {0}", new object[1] { difficulty });
	}

	protected void OnShowInvitePlayer()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (!PopupManager.IsPopupShowing<InvitePopup>())
		{
			invitePopup = PopupManager.GetInvitePopup();
			invitePopup.Header = Localization.Get("onlineview.lobby.invite.title");
			invitePopup.buttonData = new PopupButtonData[1]
			{
				new PopupButtonData("buttons.back")
			};
			invitePopup.SetData(lobbyGameViewModel, MaxPlayers);
			invitePopup.OnFriendClicked = OnInviteFriend;
			invitePopup.OnBotClicked = OnInviteBot;
			invitePopup.Show(InputManager.GetInputPosition());
		}
	}

	protected void OnInviteFriend(PlayerData playerData)
	{
		GameManager.GetAnalyticsManager().SendEvent("friends_add", new Dictionary<string, object>());
		OnInvite(new List<PlayerData> { playerData }, new List<int>());
	}

	protected void OnInviteBot(int id)
	{
		int num = Mathf.Min(GameManager.GetMaxOpponents(), MapDataExtensions.GetMaximumOpponentCountForMapSize(lobbyGameViewModel.MapSize));
		if (LobbyManager.GetActiveParticipatorsCount(lobbyGameViewModel) < num + 1)
		{
			OnInvite(new List<PlayerData>(), new List<int> { id });
		}
	}

	private ParticipatorViewModel ParticipatorFromPlayerData(PlayerData playerData)
	{
		return new ParticipatorViewModel
		{
			InvitationState = PlayerInvitationState.Invited,
			AvatarStateData = SerializationHelpers.ToByteArray(playerData.profile.avatarState, VersionManager.AvatarVersion),
			Name = playerData.profile.name,
			UserId = playerData.profile.id
		};
	}

	protected async void OnInvite(List<PlayerData> newPlayers, List<int> newBots)
	{
		if ((Object)(object)invitePopup != (Object)null)
		{
			invitePopup.Hide();
		}
		List<Guid> newPlayerIds = new List<Guid>();
		for (int i = 0; i < newPlayers.Count; i++)
		{
			addedPlayers.Add(ParticipatorFromPlayerData(newPlayers[i]));
			newPlayerIds.Add(newPlayers[i].profile.id);
		}
		addedBots.AddRange(newBots);
		UpdatePresentedPlayers();
		ServerResponse<BoolResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.ModifyPlayersInLobby(new ModifyPlayersInLobbyBindingModel
		{
			LobbyId = lobbyGameViewModel.Id,
			InvitePlayers = newPlayerIds,
			Bots = new List<int>(presentedBots)
		});
		RemoveValues(addedBots, newBots);
		RemoveUserIds(addedPlayers, newPlayerIds);
		UpdatePresentedPlayers();
		if (!serverResponse.Success)
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
	}

	protected void ShowLeavePopup(int id, BaseEventData eventData)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("onlineview.lobby.leave.title");
		basicPopup.Description = Localization.Get("onlineview.lobby.leave.info");
		basicPopup.buttonData = new PopupButtonData[2]
		{
			new PopupButtonData("buttons.back"),
			new PopupButtonData("buttons.ok", PopupButtonData.States.Selected, OnLeaveLobbyOkClicked)
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}

	protected async void OnLeaveLobbyOkClicked(int id, BaseEventData eventData)
	{
		Hide();
		GameManager.GetLobbyManager().RemoveLobby(lobbyGameViewModel.Id);
		ServerResponse<BoolResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.LeaveLobby(lobbyGameViewModel.Id);
		if (!serverResponse.Success)
		{
			GameManager.GetLobbyManager().AddOrUpdateLobby(lobbyGameViewModel);
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
	}

	public void OnLeaveClicked()
	{
		if (localParticipator != null && localParticipator.InvitationState == PlayerInvitationState.Accepted)
		{
			ShowLeavePopup(0, null);
		}
		else
		{
			OnDeclinedInvite(0);
		}
	}

	protected void OnStartClicked(int id, BaseEventData eventData)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (!LobbyManager.CanBeStartedByPlayer(lobbyGameViewModel, AccountManager.PlayerAccountId))
		{
			return;
		}
		if (LobbyManager.GetWaitingForPlayersCount(lobbyGameViewModel) > 0)
		{
			BasicPopup popup = PopupManager.GetBasicPopup();
			popup.Header = Localization.Get("onlineview.lobby.waitingforplayers.title");
			popup.Description = Localization.Get("onlineview.lobby.waitingforplayers.warning");
			popup.buttonData = new PopupButtonData[2]
			{
				new PopupButtonData("buttons.back"),
				new PopupButtonData("buttons.start", PopupButtonData.States.Selected, delegate(int id2, BaseEventData eventData2)
				{
					OnStartGame(id2, eventData2);
					popup.Hide();
				}, -1, closesPopup: false)
			};
			popup.Show(InputManager.GetInputPosition());
		}
		else
		{
			OnStartGame(id, eventData);
		}
	}

	protected void OnStartGame(int id, BaseEventData eventData)
	{
		NetworkUtils.ShowLoader(1000);
		UIBlackFader.FadeIn(0.5f, async delegate
		{
			ServerResponse<LobbyGameViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.StartLobbyGame(new StartLobbyBindingModel
			{
				LobbyId = lobbyGameViewModel.Id
			});
			if (serverResponse.Success)
			{
				Hide();
				if (!(await GameManager.Instance.OpenMultiplayerGame(serverResponse.Data.StartedGameId.Value)))
				{
					UIBlackFader.FadeOut();
					NetworkUtils.HideLoader();
				}
			}
			else
			{
				NetworkUtils.HideLoader();
				PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
				UIBlackFader.FadeOut();
			}
		});
	}

	protected void OnAcceptedInvite(int id, BaseEventData eventData = null)
	{
		GameManager.PreliminaryGameSettings.ApplyLobbySettings(lobbyGameViewModel);
		Hide();
		TribeSelectorScreen obj = UIManager.Instance.ShowScreen(UIConstants.Screens.TribeSelector) as TribeSelectorScreen;
		obj.SetLobbyId(lobbyGameViewModel.Id);
		obj.SetGameOwnerId(lobbyGameViewModel.OwnerId);
		obj.onCancel = delegate
		{
			LobbyPopup lobbyPopup = PopupManager.GetLobbyPopup();
			lobbyPopup.SetData(lobbyGameViewModel);
			lobbyPopup.Show();
		};
		obj.onTribePicked = async delegate(TribeData.Type tribe, SkinType skin, TribeData.Type tribeMix, List<TribeData.Type> disabledTribes)
		{
			Log.Info("Accepting invite to: {0}", new object[1] { lobbyGameViewModel.Id });
			UIManager.Instance.OnBack();
			ParticipatorViewModel participatorViewModel = lobbyGameViewModel.Participators.Find((ParticipatorViewModel participator) => participator.UserId == AccountManager.PlayerAccountId);
			bool wasInvited = participatorViewModel != null;
			if (wasInvited)
			{
				participatorViewModel.InvitationState = PlayerInvitationState.Accepted;
			}
			else
			{
				participatorViewModel = new ParticipatorViewModel
				{
					Name = AccountManager.Alias,
					UserId = AccountManager.PlayerAccountId,
					AvatarStateData = AccountManager.UserModel.AvatarStateData,
					InvitationState = PlayerInvitationState.Accepted
				};
				lobbyGameViewModel.Participators.Add(participatorViewModel);
				PolytopiaBackendAdapter.Instance.SubscribeToLobby(new SubscribeToLobbyBindingModel
				{
					LobbyIds = new List<Guid> { lobbyGameViewModel.Id },
					Subscribe = true
				}).WrapErrors();
				await new WaitForUpdate();
				GameManager.GetLobbyManager().AddOrUpdateLobby(lobbyGameViewModel);
			}
			LobbyPopup popup = PopupManager.GetLobbyPopup();
			popup.SetData(lobbyGameViewModel);
			popup.Show();
			ServerResponse<LobbyGameViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.RespondToLobbyInvitation(new RespondToLobbyInvitation
			{
				LobbyId = lobbyGameViewModel.Id,
				TribeId = (int)tribe,
				TribeSkinId = (int)skin,
				Accepted = true
			});
			if (!serverResponse.Success || serverResponse.Data == null)
			{
				PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
				if (wasInvited)
				{
					participatorViewModel.InvitationState = PlayerInvitationState.Invited;
				}
				else
				{
					lobbyGameViewModel.Participators.Remove(participatorViewModel);
				}
				if ((Object)(object)popup != (Object)null && popup.IsShowing())
				{
					popup.SetData(lobbyGameViewModel);
				}
			}
		};
	}

	protected async void OnDeclinedInvite(int id, BaseEventData eventData = null)
	{
		Log.Info("Declining invite to: {0}", new object[1] { lobbyGameViewModel.Id });
		Hide();
		GameManager.GetLobbyManager().RemoveLobby(lobbyGameViewModel.Id);
		ServerResponse<LobbyGameViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.RespondToLobbyInvitation(new RespondToLobbyInvitation
		{
			LobbyId = lobbyGameViewModel.Id,
			Accepted = false
		});
		if (!serverResponse.Success)
		{
			GameManager.GetLobbyManager().AddOrUpdateLobby(lobbyGameViewModel);
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
	}

	private void OnHeaderIconClicked(int id, BaseEventData eventData = null)
	{
		headerIconAction?.Invoke();
	}

	private void OnBack(int id, BaseEventData eventData = null)
	{
		GameManager.GetAnalyticsManager().SendEvent("lobby_info_click", new Dictionary<string, object> { { "option", "back" } });
	}

	private Color GetColorForParticipator(ParticipatorViewModel participator)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (participator.InvitationState != PlayerInvitationState.Accepted)
		{
			return ColorConstants.yellow;
		}
		return ColorConstants.green;
	}

	private string GetBadgeIdForParticipator(ParticipatorViewModel participator)
	{
		if (participator.InvitationState != PlayerInvitationState.Accepted)
		{
			return "actionIcons_unknown";
		}
		return "actionIcons_endTurn";
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

	public override void ResetPopup()
	{
		base.ResetPopup();
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
