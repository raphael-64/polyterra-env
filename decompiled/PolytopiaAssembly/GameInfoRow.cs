using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polytopia.Data;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameInfoRow : UIBasicButton, IListCellNavigation
{
	[Header("Game Info row")]
	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected Image labelIcon;

	[SerializeField]
	protected TextMeshProUGUI nameLabel;

	[SerializeField]
	protected TextMeshProUGUI infoLabel;

	[SerializeField]
	protected GameInfoTimer timer;

	private UIBasicButton timerButton;

	[SerializeField]
	protected ColorStates labelColorStates = new ColorStates
	{
		defaultColor = new Color(1f, 1f, 1f, 1f),
		hoverColor = new Color(0f, 0f, 0f, 1f),
		highlightedColor = new Color(0f, 0f, 0f, 1f),
		highlightedHoverColor = new Color(0f, 0f, 0f, 1f),
		disabledColor = new Color(1f, 1f, 1f, 0.502f)
	};

	protected GameSummaryViewModel summaryViewModel;

	protected GameSessionState sessionState;

	protected bool isLocalPlayersTurn;

	protected bool isButtonHeld;

	protected Vector2 buttonStartPosition;

	protected float buttonPressTime;

	private GameType gameType;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	public GameSessionState SessionState
	{
		get
		{
			return sessionState;
		}
		set
		{
			sessionState = value;
		}
	}

	public override void Awake()
	{
		base.Awake();
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetFaceIcon(spriteHandle.sprite);
		});
		GameInfoTimer gameInfoTimer = timer;
		timerButton = ((gameInfoTimer != null) ? ((Component)gameInfoTimer).GetComponent<UIBasicButton>() : null);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.OnClicked += OnButtonClick;
		base.OnDown += OnButtonPress;
		base.OnUp += OnButtonRelease;
		base.OnExit += OnButtonExit;
		if ((Object)(object)timerButton != (Object)null)
		{
			timerButton.OnClicked += OnTimerClicked;
			timerButton.OnClicked += OnTimerButtonExit;
			timerButton.OnUp += OnTimerButtonExit;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.OnClicked -= OnButtonClick;
		base.OnDown -= OnButtonPress;
		base.OnUp -= OnButtonRelease;
		base.OnExit -= OnButtonExit;
		if ((Object)(object)timerButton != (Object)null)
		{
			timerButton.OnClicked -= OnTimerClicked;
			timerButton.OnClicked -= OnTimerButtonExit;
			timerButton.OnUp -= OnTimerButtonExit;
		}
	}

	private void Update()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (isButtonHeld)
		{
			Vector2 val = (buttonStartPosition - PolytopiaInput.mousePosition) / ScalingUtils.GetDPI();
			if (((Vector2)(ref val)).sqrMagnitude > 0.0035f)
			{
				isButtonHeld = false;
			}
			if (Time.time - buttonPressTime >= 3f)
			{
				isButtonHeld = false;
				OnCopyGameDataToClipboard();
			}
		}
	}

	private void OnTimerButtonExit(int id, BaseEventData eventData = null)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		if (eventData is PointerEventData)
		{
			OnPointerExit((PointerEventData)eventData);
		}
	}

	public void SetData(GameSummaryViewModel summaryViewModel)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		this.summaryViewModel = summaryViewModel;
		sessionState = summaryViewModel.State;
		Color black = Color.black;
		((Component)labelIcon).gameObject.SetActive(false);
		SerializationHelpers.FromByteArray<GameStateSummary>(summaryViewModel.GameSummaryData, out var summary);
		if (summary == null)
		{
			((TMP_Text)nameLabel).text = Localization.Get("gameinfo.nodata.title");
			black = ColorConstants.red;
			LoadFaceIcon("neutral");
			((Component)timer).gameObject.SetActive(false);
			((TMP_Text)infoLabel).text = Localization.Get("gameinfo.nodata");
			return;
		}
		gameType = summary.GameType;
		if (summary.GameType == GameType.Multiplayer || summary.GameType == GameType.Competitive)
		{
			((TMP_Text)nameLabel).text = summary.GameName;
			PlayerSummaries playerSummaries = GameSummaryUtils.GetPlayerSummaries(summary, summaryViewModel);
			bool flag = true;
			if (playerSummaries.Local == null)
			{
				return;
			}
			GameStateSummary.GamePlayerSummary gamePlayerSummary = summary.PlayerSummaries.FirstOrDefault((GameStateSummary.GamePlayerSummary playerSummary) => playerSummary.Id == summary.CurrentPlayer);
			ParticipatorViewModel participatorViewModel = GetParticipatorViewModel(gamePlayerSummary.PolytopiaId);
			if (summaryViewModel.GameContext != null)
			{
				((Component)labelIcon).gameObject.SetActive(summaryViewModel.GameContext.ExternalMatchId.HasValue);
			}
			PlayerInvitationState playerInvitationState = PlayerInvitationState.Unknown;
			foreach (ParticipatorViewModel participator in summaryViewModel.Participators)
			{
				Guid userId = participator.UserId;
				Guid? polytopiaId = playerSummaries.Local.PolytopiaId;
				if (userId == polytopiaId)
				{
					playerInvitationState = participator.InvitationState;
				}
			}
			if (playerInvitationState == PlayerInvitationState.Invited)
			{
				flag = false;
			}
			isLocalPlayersTurn = playerSummaries.IsLocalPlayerCurrent();
			black = ((!isLocalPlayersTurn) ? bgColorStates.defaultColor : bgColorStates.highlightedColor);
			string text = string.Empty;
			switch (SessionState)
			{
			case GameSessionState.Lobby:
				if (flag)
				{
					text = (isLocalPlayersTurn ? Localization.Get("gameitem.pick", playerSummaries.Local.UserName) : Localization.Get("gameitem.join.wait", playerSummaries.Current?.UserName));
					break;
				}
				text = Localization.Get("gameitem.join");
				black = ColorConstants.blue;
				break;
			case GameSessionState.ReadyToStart:
				text = (isLocalPlayersTurn ? Localization.Get("gameitem.ready") : Localization.Get("gameitem.ready.wait", playerSummaries.Current?.UserName));
				break;
			case GameSessionState.Started:
				text = (isLocalPlayersTurn ? Localization.Get("gameitem.turn.your") : Localization.Get("gameitem.turn.other", playerSummaries.Current?.UserName));
				break;
			case GameSessionState.Ended:
				text = Localization.Get("gameitem.gameover");
				black = ColorConstants.green;
				break;
			}
			((TMP_Text)infoLabel).text = text;
			if (playerSummaries.Local.HasChosenTribe)
			{
				LoadFaceIcon(playerSummaries.Local.TribeType, playerSummaries.Local.SkinType);
			}
			else
			{
				LoadFaceIcon("neutral");
			}
			timer.SetGameId(summaryViewModel.GameId);
			timer.SetTimeLimit(GetPlayerTotalTimeLimit(participatorViewModel));
			timer.RefreshCountDown(GetPlayerLastStartTurn(participatorViewModel), GetPlayerTotalTimeLimit(participatorViewModel));
			((Component)timer).gameObject.SetActive(SessionState != GameSessionState.Ended);
			bgColorStates.defaultColor = ColorUtil.SetAlphaOnColor(black, 0.8f);
			UpdateColors();
		}
		else
		{
			if (summary.GameType != GameType.PassAndPlay)
			{
				return;
			}
			black = ColorConstants.blue;
			((TMP_Text)nameLabel).text = summary.GameName;
			GameStateSummary.GamePlayerSummary gamePlayerSummary2 = null;
			foreach (GameStateSummary.GamePlayerSummary playerSummary in summary.PlayerSummaries)
			{
				if (playerSummary.Id == summary.CurrentPlayer)
				{
					gamePlayerSummary2 = playerSummary;
				}
			}
			if (gamePlayerSummary2 != null)
			{
				LoadFaceIcon(gamePlayerSummary2.TribeType, gamePlayerSummary2.SkinType);
				((TMP_Text)infoLabel).text = Localization.Get("gameitem.turn", gamePlayerSummary2.UserName);
			}
			((Component)timer).gameObject.SetActive(false);
			bgColorStates.defaultColor = ColorUtil.SetAlphaOnColor(black, 0.8f);
			UpdateColors();
		}
	}

	public override void UpdateColors()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateColors();
		TextMeshProUGUI obj = nameLabel;
		Color color = (((Graphic)infoLabel).color = GetColorForState(labelColorStates));
		((Graphic)obj).color = color;
	}

	private void LoadFaceIcon(string faceId)
	{
		iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(faceId));
	}

	private void LoadFaceIcon(TribeData.Type type, SkinType skinType)
	{
		if (EnumCache<SkinType>.TryGetName(skinType, out var value))
		{
			iconSpriteHandle.Request(new SpriteAddress[2]
			{
				SpriteData.GetHeadSpriteAddress(value),
				SpriteData.GetHeadSpriteAddress(type)
			});
		}
		else
		{
			Log.Error("Invalid skin type {0}", new object[1] { skinType });
			iconSpriteHandle.Request(new SpriteAddress[1] { SpriteData.GetHeadSpriteAddress(type) });
		}
	}

	private void SetFaceIcon(Sprite faceIcon)
	{
		icon.sprite = faceIcon;
		icon.useSpriteMesh = true;
		((Graphic)icon).SetNativeSize();
		UIUtils.FitImageContentInParent(((Graphic)icon).rectTransform);
	}

	private void OnButtonPress(int id, BaseEventData eventData)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		isButtonHeld = true;
		buttonPressTime = Time.time;
		buttonStartPosition = PolytopiaInput.mousePosition;
	}

	private void OnButtonRelease(int id, BaseEventData eventData)
	{
		if (isButtonHeld)
		{
			isButtonHeld = false;
			ShowGameInfo();
		}
	}

	private void OnButtonClick(int id, BaseEventData eventData)
	{
		if (!(eventData is PointerEventData))
		{
			ShowGameInfo();
		}
	}

	private void OnButtonExit(int id, BaseEventData eventData)
	{
		isButtonHeld = false;
	}

	private void ShowGameInfo()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		GameInfoPopup gameInfoPopup = PopupManager.GetGameInfoPopup();
		if (summaryViewModel != null)
		{
			gameInfoPopup.SetData(summaryViewModel);
		}
		gameInfoPopup.Show(InputManager.GetInputPosition());
	}

	private void OnCopyGameDataToClipboard()
	{
		if (gameType == GameType.PassAndPlay)
		{
			string saveDirectoryPath = Paths.GetSaveDirectoryPath("Hotseat");
			string text = Path.Combine(saveDirectoryPath, $"{summaryViewModel.GameId.ToString()}.state");
			if (Directory.Exists(saveDirectoryPath) && File.Exists(text))
			{
				DebugUtils.CopyFilesToClipBoard(new List<string>(1) { text });
			}
		}
	}

	private DateTime? GetPlayerDeadlineHalfTime(ParticipatorViewModel currentPlayer)
	{
		if (currentPlayer == null)
		{
			return null;
		}
		double value = GetPlayerTotalTimeLimit(currentPlayer).TotalMinutes * 0.5;
		if (currentPlayer.DateCurrentTurnDeadline.HasValue)
		{
			return new DateTime?(currentPlayer.DateLastStartTurn.Value).Value.AddMinutes(value);
		}
		return (summaryViewModel.DateLastEndTurn ?? summaryViewModel.DateCreated).Value.AddMinutes(value);
	}

	private TimeSpan GetPlayerTotalTimeLimit(ParticipatorViewModel currentPlayer)
	{
		if (currentPlayer != null && currentPlayer.DateCurrentTurnDeadline.HasValue)
		{
			return TimeSpan.FromMinutes((currentPlayer.DateCurrentTurnDeadline - currentPlayer.DateLastStartTurn).Value.TotalMinutes);
		}
		return TimeSpan.FromMinutes(summaryViewModel.TimeLimit);
	}

	private DateTime? GetPlayerDeadline(ParticipatorViewModel currentPlayer)
	{
		if (currentPlayer != null && currentPlayer.DateCurrentTurnDeadline.HasValue)
		{
			return currentPlayer.DateCurrentTurnDeadline;
		}
		return GetPlayerLastStartTurn(currentPlayer) + GetPlayerTotalTimeLimit(currentPlayer);
	}

	private DateTime? GetPlayerLastStartTurn(ParticipatorViewModel currentPlayer)
	{
		if (currentPlayer != null && currentPlayer.DateLastStartTurn.HasValue)
		{
			return currentPlayer.DateLastStartTurn;
		}
		return summaryViewModel.DateLastEndTurn ?? summaryViewModel.DateCreated;
	}

	private ParticipatorViewModel GetParticipatorViewModel(Guid? userId)
	{
		return summaryViewModel.Participators.FirstOrDefault(delegate(ParticipatorViewModel participator)
		{
			Guid userId2 = participator.UserId;
			Guid? guid = userId;
			return userId2 == guid;
		});
	}

	public void OnTimerClicked(int timerID, BaseEventData timerEventData)
	{
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		SerializationHelpers.FromByteArray<GameStateSummary>(summaryViewModel.GameSummaryData, out var summary);
		if (summary == null)
		{
			return;
		}
		GameStateSummary.GamePlayerSummary currentPlayerSummary = summary.PlayerSummaries.FirstOrDefault((GameStateSummary.GamePlayerSummary playerSummary) => playerSummary.Id == summary.CurrentPlayer);
		if (currentPlayerSummary == null)
		{
			return;
		}
		ParticipatorViewModel participatorViewModel = GetParticipatorViewModel(currentPlayerSummary.PolytopiaId);
		DateTime? playerDeadline = GetPlayerDeadline(participatorViewModel);
		DateTime? playerDeadlineHalfTime = GetPlayerDeadlineHalfTime(participatorViewModel);
		DateTime utcNow = DateTime.UtcNow;
		int num;
		if (!playerDeadline.HasValue)
		{
			num = 0;
		}
		else
		{
			DateTime value = utcNow;
			DateTime? dateTime = playerDeadline;
			num = ((value >= dateTime) ? 1 : 0);
		}
		bool flag = (byte)num != 0;
		int num2;
		if (!playerDeadlineHalfTime.HasValue)
		{
			num2 = 0;
		}
		else
		{
			DateTime value = utcNow;
			DateTime? dateTime = playerDeadlineHalfTime;
			num2 = ((value >= dateTime) ? 1 : 0);
		}
		bool flag2 = (byte)num2 != 0;
		TimeSpan time = GameManager.GetRemoteGameDataManager().GetGameTimeLeftForCurrentPlayer(summaryViewModel.GameId) ?? TimeSpan.Zero;
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		if (flag && !summary.IsAutoSkipEnabled)
		{
			if (currentPlayerSummary.PolytopiaId == AccountManager.PlayerAccountId)
			{
				basicPopup.Header = Localization.Get("gameitem.timeup");
				basicPopup.Description = Localization.Get("gameitem.timeup.info");
				basicPopup.buttonData = new PopupBase.PopupButtonData[2]
				{
					new PopupBase.PopupButtonData("buttons.back"),
					new PopupBase.PopupButtonData((summaryViewModel.State == GameSessionState.ReadyToStart) ? "onlineview.game.delete" : "onlineview.game.resign", PopupBase.PopupButtonData.States.Selected, ShowResignPopup)
				};
				GameManager.GetAnalyticsManager().SendEvent("time_up_popup_view", new Dictionary<string, object> { { "game_id", summaryViewModel.GameId } });
			}
			else if (summaryViewModel.State == GameSessionState.Started)
			{
				basicPopup.Header = Localization.Get("gameitem.skipturn");
				basicPopup.Description = Localization.Get("gameitem.skipturn.info", currentPlayerSummary.UserName);
				basicPopup.buttonData = new PopupBase.PopupButtonData[2]
				{
					new PopupBase.PopupButtonData("buttons.back"),
					new PopupBase.PopupButtonData("gameitem.skipturn.action", PopupBase.PopupButtonData.States.Selected, delegate
					{
						SkipTurn(currentPlayerSummary.PolytopiaId.Value);
					})
				};
				basicPopup.TopButtonData = new PopupBase.PopupButtonData("gameitem.kick.action", PopupBase.PopupButtonData.States.Alternative, delegate
				{
					KickPlayer(currentPlayerSummary.PolytopiaId.Value);
				});
				GameManager.GetAnalyticsManager().SendEvent("opponent_time_up_popup_view", new Dictionary<string, object> { { "game_id", summaryViewModel.GameId } });
			}
			else
			{
				basicPopup.Header = Localization.Get("gameitem.kick");
				basicPopup.Description = Localization.Get("gameitem.kick.info", currentPlayerSummary.UserName);
				basicPopup.buttonData = new PopupBase.PopupButtonData[2]
				{
					new PopupBase.PopupButtonData("buttons.back"),
					new PopupBase.PopupButtonData("gameitem.kick.action", PopupBase.PopupButtonData.States.Selected, delegate
					{
						KickPlayer(currentPlayerSummary.PolytopiaId.Value);
					})
				};
				GameManager.GetAnalyticsManager().SendEvent("opponent_time_up_popup_view", new Dictionary<string, object> { { "game_id", summaryViewModel.GameId } });
			}
		}
		else if ((!summary.IsAutoSkipEnabled || !flag) && flag2 && participatorViewModel.UserId != AccountManager.PlayerAccountId)
		{
			basicPopup.Header = Localization.Get("gameitem.slow");
			basicPopup.Description = Localization.Get("gameitem.slow.info", currentPlayerSummary.UserName, LocalizationUtils.GetTimeString(time));
			basicPopup.buttonData = new PopupBase.PopupButtonData[2]
			{
				new PopupBase.PopupButtonData("buttons.back"),
				new PopupBase.PopupButtonData("gameitem.slow.action", PopupBase.PopupButtonData.States.Selected, delegate
				{
					if (!summaryViewModel.ReminderSent.HasValue)
					{
						RemindPlayer(currentPlayerSummary);
					}
					else
					{
						PlayerAlreadyReminded(currentPlayerSummary);
					}
				})
			};
		}
		else
		{
			string timeString = LocalizationUtils.GetTimeString(time);
			if (currentPlayerSummary.PolytopiaId == AccountManager.PlayerAccountId)
			{
				basicPopup.Header = Localization.Get("gameitem.timelimit");
				basicPopup.Description = Localization.Get("gameitem.timelimit.info", timeString);
			}
			else
			{
				basicPopup.Header = Localization.Get("gameitem.timeleft");
				basicPopup.Description = Localization.Get("gameitem.timeleft.info", currentPlayerSummary.UserName, timeString);
			}
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
		}
		basicPopup.Show(InputManager.GetInputPosition());
	}

	protected void ShowResignPopup(int id, BaseEventData eventData)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("onlineview.game.resign.title");
		basicPopup.Description = Localization.Get("onlineview.game.resign.info");
		basicPopup.buttonData = new PopupBase.PopupButtonData[2]
		{
			new PopupBase.PopupButtonData("buttons.back"),
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, OnResignOkClicked)
		};
		basicPopup.Show(InputManager.GetInputPosition());
		GameManager.GetAnalyticsManager().SendEvent("resign_popup_view", new Dictionary<string, object> { { "game_id", summaryViewModel.GameId } });
	}

	protected async void OnResignOkClicked(int id, BaseEventData eventData)
	{
		GameManager.GetAnalyticsManager().SendEvent("player_resign", new Dictionary<string, object> { { "game_id", summaryViewModel.GameId } });
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
			serverResponse = await PolytopiaBackendAdapter.Instance.Resign(new ResignBindingModel
			{
				GameId = summaryViewModel.GameId
			});
			break;
		}
		if (serverResponse != null && !serverResponse.Success)
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
	}

	protected async void KickPlayer(Guid playerToKick)
	{
		GameManager.GetAnalyticsManager().SendEvent("opponent_time_up_popup_click", new Dictionary<string, object>
		{
			{ "option", "kick" },
			{ "game_id", summaryViewModel.GameId }
		});
		ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.Kick(new KickBindingModel
		{
			GameId = summaryViewModel.GameId,
			UserId = playerToKick
		});
		if (!serverResponse.Success)
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
	}

	protected async void SkipTurn(Guid playerToSkip)
	{
		Log.Info("Skip Player: {0}", new object[1] { playerToSkip });
		GameManager.GetAnalyticsManager().SendEvent("opponent_time_up_popup_click", new Dictionary<string, object>
		{
			{ "option", "skip turn" },
			{ "game_id", summaryViewModel.GameId }
		});
		ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.SkipTurn(new SkipTurnBindingModel
		{
			GameId = summaryViewModel.GameId,
			UserId = playerToSkip
		});
		if (!serverResponse.Success)
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
	}

	protected async void RemindPlayer(GameStateSummary.GamePlayerSummary playerSummary)
	{
		GameManager.GetAnalyticsManager().SendEvent("opponent_time_up_popup_click", new Dictionary<string, object>
		{
			{ "option", "remind" },
			{ "game_id", summaryViewModel.GameId }
		});
		ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.RemindPlayer(new RemindPlayerBindingModel
		{
			GameId = summaryViewModel.GameId,
			UserId = playerSummary.PolytopiaId.Value
		});
		if (serverResponse.Success)
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("gameitem.remind.notify");
			basicPopup.Description = Localization.Get("gameitem.remind.notify.info", playerSummary.UserName);
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup.Show(InputManager.GetInputPosition());
		}
		else
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
	}

	protected void PlayerAlreadyReminded(GameStateSummary.GamePlayerSummary playerSummary)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("gameitem.remind.max");
		basicPopup.Description = Localization.Get("gameitem.remind.max.info", playerSummary.UserName);
		basicPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}

	public Selectable GetMainSelectable()
	{
		return (Selectable)(object)base.button;
	}

	public Selectable GetAccessorySelectable()
	{
		if (UINavigationManager.IsValidSelectable((Selectable)(object)timer.button))
		{
			return (Selectable)(object)timer.button;
		}
		return (Selectable)(object)base.button;
	}
}
