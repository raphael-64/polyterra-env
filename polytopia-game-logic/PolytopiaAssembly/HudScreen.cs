using System;
using System.Collections.Generic;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;

public class HudScreen : UIScreenBase
{
	public float SELECT_NEXT_UNIT_DELAY = 1f;

	public InteractionBar interactionBarMobile;

	public InteractionBar interactionBarPc;

	public HudButtonBar buttonBar;

	public ReplayInterface replayInterface;

	[SerializeField]
	protected QuickActions quickActionPrefab;

	protected QuickActions quickActions;

	protected InteractionBar interactionBar;

	public bool isButtonHeld;

	public float buttonPressTime;

	public InteractionBar InteractionBar => interactionBar;

	public override void Init()
	{
		if (!initialized)
		{
			base.Init();
			((Component)interactionBarMobile).gameObject.SetActive(false);
			((Component)interactionBarPc).gameObject.SetActive(false);
			if (SettingsUtils.UseCompactUI)
			{
				interactionBar = interactionBarMobile;
			}
			else
			{
				interactionBar = interactionBarPc;
			}
			interactionBar.Init();
			buttonBar.Init(this);
			GameEvents.OnMatchStart += OnMatchStart;
			GameEvents.OnMatchResumed += OnMatchStart;
			SettingsEvents.OnSettingsUpdated += OnSettingsUpdated;
			SubscribeButtonsEvents();
		}
	}

	public override void DeInit()
	{
		if (initialized)
		{
			base.DeInit();
			interactionBar.DeInit();
			buttonBar.DeInit();
			SettingsEvents.OnSettingsUpdated -= OnSettingsUpdated;
			UnsubscribeButtonsEvents();
		}
	}

	public float GetCoveredBottomArea()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = buttonBar.rectTransform.rect;
		float yMax = ((Rect)(ref rect)).yMax;
		float top = interactionBar.GetTop();
		return (Mathf.Max(yMax, top) + ScreenManager.SafeBottom * UICanvasScalerHelper.GetInvertedUIScale()) / UIManager.GetUISize().y;
	}

	public float GetCoveredBottomAreaAt(float xPos)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = buttonBar.rectTransform.rect;
		float yMax = ((Rect)(ref rect)).yMax;
		float num = interactionBar.GetTop();
		float num2 = (interactionBar.rectTransform.GetWidth() + 100f) / UIManager.GetUISize().x;
		if (SettingsUtils.UseCompactUI)
		{
			num2 = 1f;
		}
		if (xPos > num2)
		{
			num = 0f;
		}
		return (Mathf.Max(yMax, num) + ScreenManager.SafeBottom * UICanvasScalerHelper.GetInvertedUIScale()) / UIManager.GetUISize().y;
	}

	private void OnSettingsUpdated(SettingsUtils.SettingsType type)
	{
		if (type == SettingsUtils.SettingsType.UseCompactUI)
		{
			if (SettingsUtils.UseCompactUI && (Object)(object)interactionBar != (Object)(object)interactionBarMobile)
			{
				((Component)interactionBarPc).gameObject.SetActive(false);
				interactionBarPc.Reset();
				interactionBar = interactionBarMobile;
				interactionBar.Init();
				((Component)interactionBar).gameObject.SetActive(true);
			}
			else if (!SettingsUtils.UseCompactUI && (Object)(object)interactionBar != (Object)(object)interactionBarPc)
			{
				((Component)interactionBarMobile).gameObject.SetActive(false);
				interactionBarMobile.Reset();
				interactionBar = interactionBarPc;
				interactionBar.Init();
				((Component)interactionBar).gameObject.SetActive(true);
			}
		}
	}

	private void OnMatchStart()
	{
		UIManager.Instance.ShowScreen(UIConstants.Screens.Hud);
		GameEvents.OnMatchStart -= OnMatchStart;
		GameEvents.OnMatchResumed -= OnMatchStart;
		NotificationManager.UpdateIngameAlert();
		if (GameManager.GameState.CurrentTurn != 0 && GameManager.GameState.Settings.GameType == GameType.SinglePlayer)
		{
			string empty = string.Empty;
			string message = string.Empty;
			if (GameManager.GameState.Settings.rules.TurnLimit > 0)
			{
				if (GameManager.GameState.CurrentTurn >= GameManager.GameState.Settings.rules.TurnLimit)
				{
					empty = Localization.Get("world.turn.last");
				}
				else
				{
					empty = Localization.Get("world.turn.your");
					message = Localization.Get("world.turn.remaining", GameManager.GameState.Settings.rules.TurnLimit - GameManager.GameState.CurrentTurn);
				}
			}
			else
			{
				empty = string.Format("{0}!", Localization.Get("world.turn.your"));
			}
			NotificationManager.ShowCenterNotification(empty, message, 1f);
		}
		if (GameManager.GameState != null && GameManager.Client.IsReplay)
		{
			((Component)replayInterface).gameObject.SetActive(true);
			replayInterface.SetData(GameManager.GameState);
		}
	}

	public override void OnButtonUp(InputManager.Buttons button)
	{
		if (PopupManager.PopupShowing || GameManager.GameState.CurrentState != GameState.State.Started)
		{
			return;
		}
		if (button == InputManager.Buttons.Cancel)
		{
			if (interactionBar.IsShowing)
			{
				InputEvents.SelectionCleared();
			}
			else
			{
				ShowMenu();
			}
		}
		if (Object.op_Implicit((Object)(object)quickActions) && quickActions.IsShowing)
		{
			switch (button)
			{
			case InputManager.Buttons.ButtonBarMoveLeft:
				quickActions.MoveSelectedButton(-1);
				break;
			case InputManager.Buttons.ButtonBarMoveRight:
				quickActions.MoveSelectedButton(1);
				break;
			}
		}
		else if (interactionBar.IsShowing)
		{
			switch (button)
			{
			case InputManager.Buttons.ButtonBarMoveLeft:
				interactionBar.MoveSelectedButton(-1);
				break;
			case InputManager.Buttons.ButtonBarMoveRight:
				interactionBar.MoveSelectedButton(1);
				break;
			}
		}
	}

	protected void OnDestroy()
	{
		interactionBar.Reset();
	}

	private void Update()
	{
		if (isButtonHeld && Time.time - buttonPressTime >= SELECT_NEXT_UNIT_DELAY)
		{
			OnNextUnit();
		}
	}

	private void ShowMenu()
	{
		if (!PopupManager.IsUnskippablePopupShowing())
		{
			PopupManager.HideCurrentPopup();
			UIManager.Instance.ShowScreen(UIConstants.Screens.IngameMenu);
		}
	}

	private void ShowStats()
	{
		if (!PopupManager.IsUnskippablePopupShowing())
		{
			PopupManager.HideCurrentPopup();
			UIManager.Instance.ShowScreen(UIConstants.Screens.StatsScreen);
		}
	}

	private void ShowTechTree()
	{
		if (!PopupManager.IsUnskippablePopupShowing())
		{
			PopupManager.HideCurrentPopup();
			InputEvents.SelectionCleared();
			UIManager.Instance.ShowScreen(UIConstants.Screens.TechTree);
		}
	}

	public void OnNextTurnPress()
	{
		if (!PopupManager.IsUnskippablePopupShowing())
		{
			PopupManager.HideCurrentPopup();
			isButtonHeld = true;
			buttonPressTime = Time.time;
		}
	}

	public void OnNextTurnPointerExit()
	{
		isButtonHeld = false;
	}

	public void OnNextTurnRelease()
	{
		if (isButtonHeld)
		{
			OnNextTurn();
			isButtonHeld = false;
		}
	}

	private void OnNextUnit()
	{
		if (GameManager.IsPlayerLocal(GameManager.GameState.CurrentPlayer))
		{
			InputEvents.SelectNextUnit(GameManager.GameState.CurrentPlayer);
			isButtonHeld = false;
		}
	}

	public QuickActions GetQuickActions()
	{
		if ((Object)(object)quickActions == (Object)null)
		{
			quickActions = Object.Instantiate<QuickActions>(quickActionPrefab);
			((Transform)quickActions.rectTransform).SetParent(((Component)this).transform, false);
		}
		((Transform)quickActions.rectTransform).SetAsFirstSibling();
		return quickActions;
	}

	public void OnNextTurn(bool forceConfirmation = false)
	{
		if (GameManager.LocalPlayer == null || GameManager.GameState == null)
		{
			return;
		}
		if (GameManager.Client.IsSpectating)
		{
			if (GameManager.Client.IsReplay && GameManager.GameState.TryGetWinner(out var winner))
			{
				ScoreDetails score = GameManager.LocalPlayer.GetScore(GameManager.GameState);
				GameManager.MatchEnded(winner.Id == GameManager.LocalPlayer.Id, score, winner.Id);
			}
			else
			{
				OnExit();
			}
			return;
		}
		InputEvents.SelectionCleared();
		if (!GameManager.IsPlayerLocal(GameManager.GameState.CurrentPlayer) && GameManager.GameState.CurrentState != GameState.State.Ended)
		{
			if (GameManager.GameState.Settings.GameType != GameType.SinglePlayer && GameManager.GameState.Settings.GameType != GameType.PassAndPlay)
			{
				OnExit();
			}
		}
		else if (GameManager.GameState.CurrentState == GameState.State.Ended || GameManager.Client.GameState.CurrentState == GameState.State.FinalTurn)
		{
			DoNextTurn();
		}
		else if (!PopupManager.IsPopupShowing<BasicPopup>("endturnpopup"))
		{
			if (SettingsUtils.ConfirmTurn || forceConfirmation)
			{
				BasicPopup basicPopup = PopupManager.GetBasicPopup();
				basicPopup.identifier = "endturnpopup";
				basicPopup.Header = Localization.Get("world.turn.end.confirm");
				basicPopup.Description = Localization.Get("world.turn.end.question");
				basicPopup.buttonData = new PopupBase.PopupButtonData[2]
				{
					new PopupBase.PopupButtonData("buttons.back"),
					new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, DoNextTurn)
				};
				basicPopup.Show();
			}
			else
			{
				DoNextTurn();
			}
		}
	}

	private void OnExit()
	{
		GameManager.GetAnalyticsManager().SendEvent("multiplayer_exit_click", new Dictionary<string, object> { 
		{
			"game_id",
			GameManager.Client.CurrentGameId
		} });
		Log.Verbose("Save and exit to the start menu", Array.Empty<object>());
		GameManager.ReturnToMenu();
	}

	private void DoNextTurn(int id = 0, BaseEventData eventData = null)
	{
		GameState gameState = GameManager.Client.GameState;
		if (GameManager.Client.GameState.CurrentState == GameState.State.Ended)
		{
			if (GameManager.GameState.TryGetWinner(out var winner))
			{
				ScoreDetails score = GameManager.LocalPlayer.GetScore(GameManager.GameState);
				GameManager.MatchEnded(GameManager.LocalPlayer.Id == winner.Id, score, winner.Id);
			}
		}
		else if (GameManager.Client.GameState.CurrentState == GameState.State.FinalTurn)
		{
			Log.Verbose("Final turn, trigger the end!", Array.Empty<object>());
			EndMatchCommand command = new EndMatchCommand(GameManager.LocalPlayer.Id);
			if (ClientActionManager.CanExecuteCommand(command, GameManager.GameState))
			{
				GameManager.Client.SendCommand(command);
				PlayerState playerState = gameState.GetPlayersSortedByRank()[0];
				if (gameState.Settings.GameType == GameType.SinglePlayer)
				{
					GameManager.GetAnalyticsManager().SendEvent(gameState.Settings.GameType.ToString() + "_EndGame", new Dictionary<string, object>
					{
						{
							"mode",
							gameState.Settings.BaseGameMode
						},
						{
							"reason",
							GameManager.LocalPlayer.IsAlive(gameState) ? "death" : ((playerState == GameManager.LocalPlayer) ? "won" : "lost")
						},
						{
							"winningTribe",
							playerState.tribe.ToString()
						},
						{ "winningScore", playerState.score },
						{
							"playerTribe",
							GameManager.LocalPlayer.tribe.ToString()
						},
						{
							"playerScore",
							GameManager.LocalPlayer.score
						},
						{ "turns", gameState.CurrentTurn }
					});
				}
				else
				{
					GameManager.GetAnalyticsManager().SendEvent(gameState.Settings.GameType.ToString() + "_EndGame", new Dictionary<string, object>
					{
						{
							"mode",
							gameState.Settings.BaseGameMode
						},
						{
							"winningTribe",
							playerState.tribe.ToString()
						},
						{ "winningScore", playerState.score },
						{ "turns", gameState.CurrentTurn }
					});
				}
			}
		}
		else
		{
			EndTurnCommand command2 = new EndTurnCommand(GameManager.LocalPlayer.Id);
			if (ClientActionManager.CanExecuteCommand(command2, GameManager.GameState))
			{
				GameManager.Client.SendCommand(command2);
			}
		}
	}

	protected override void SubscribeButtonsEvents()
	{
		base.SubscribeButtonsEvents();
		buttonBar.menuButton.OnClicked += MenuButtonOnClicked;
		buttonBar.statsButton.OnClicked += StatsButtonOnClicked;
		buttonBar.techTreeButton.OnClicked += TechTreeButtonOnClicked;
		buttonBar.nextTurnButton.OnDown += NextTurnButtonOnDown;
		buttonBar.nextTurnButton.OnClicked += NextTurnButtonOnClicked;
		buttonBar.nextTurnButton.OnExit += NextTurnButtonOnExit;
	}

	protected override void UnsubscribeButtonsEvents()
	{
		base.UnsubscribeButtonsEvents();
		buttonBar.menuButton.OnClicked -= MenuButtonOnClicked;
		buttonBar.statsButton.OnClicked -= StatsButtonOnClicked;
		buttonBar.techTreeButton.OnClicked -= TechTreeButtonOnClicked;
		buttonBar.nextTurnButton.OnDown -= NextTurnButtonOnDown;
		buttonBar.nextTurnButton.OnClicked -= NextTurnButtonOnClicked;
		buttonBar.nextTurnButton.OnExit -= NextTurnButtonOnExit;
	}

	private void MenuButtonOnClicked(int id, BaseEventData eventdata)
	{
		ShowMenu();
	}

	private void NextTurnButtonOnDown(int id, BaseEventData eventdata)
	{
		if (!PopupManager.IsUnskippablePopupShowing())
		{
			PopupManager.HideCurrentPopup();
			isButtonHeld = true;
			buttonPressTime = Time.time;
		}
	}

	private void NextTurnButtonOnClicked(int id, BaseEventData eventdata)
	{
		if (isButtonHeld)
		{
			OnNextTurn();
			isButtonHeld = false;
		}
	}

	private void NextTurnButtonOnExit(int id, BaseEventData eventdata)
	{
		isButtonHeld = false;
	}

	private void TechTreeButtonOnClicked(int id, BaseEventData eventdata)
	{
		ShowTechTree();
	}

	private void StatsButtonOnClicked(int id, BaseEventData eventdata)
	{
		ShowStats();
	}
}
