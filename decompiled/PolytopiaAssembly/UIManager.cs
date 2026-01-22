using System;
using System.Collections.Generic;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public enum Type
	{
		None,
		StartMenu,
		Ingame
	}

	protected static UIConstants.Screens queuedDeepLink;

	protected static UIDeepLinkData queuedDeepLinkData;

	public Type type;

	public UIConstants.Screens startScreen;

	public UIScreenBase[] screens;

	public LoginOverlay loginOverlay;

	[Tooltip("Components that will be initialized with the UI Manager")]
	public UIBasicComponent[] components;

	public UIIconData iconData;

	[SerializeField]
	protected UICanvasScalerHelper canvasScalerHelper;

	[Header("Debug")]
	[Info]
	[SerializeField]
	protected string info;

	protected List<UIConstants.Screens> screenStack = new List<UIConstants.Screens>();

	[SerializeField]
	protected Canvas canvas;

	[SerializeField]
	protected Canvas worldCanvas;

	[SerializeField]
	protected CanvasScaler canvasScaler;

	[SerializeField]
	protected CanvasScaler worldCanvasScaler;

	[SerializeField]
	protected CanvasGroup canvasGroup;

	[SerializeField]
	protected CanvasGroup worldCanvasGroup;

	[SerializeField]
	protected ScreenKeyboardManager screenKeyboardManager;

	[SerializeField]
	protected AdvisorManager advisorManager;

	protected RectTransform rectTransform;

	protected RectTransform worldRectTransform;

	protected bool waitingForConnection;

	protected UISpriteDuplicator spriteDuplicator;

	public static UIManager Instance { get; private set; }

	public UIConstants.Screens CurrentScreen
	{
		get
		{
			if (screenStack.Count == 0)
			{
				return UIConstants.Screens.None;
			}
			return screenStack[screenStack.Count - 1];
		}
	}

	public static Canvas Canvas
	{
		get
		{
			if (!Exists)
			{
				return null;
			}
			if ((Object)(object)Instance.canvas == (Object)null)
			{
				Instance.canvas = ((Component)Instance).GetComponent<Canvas>();
			}
			return Instance.canvas;
		}
	}

	public static Canvas WorldCanvas
	{
		get
		{
			if (!Exists || Instance.type == Type.StartMenu)
			{
				return null;
			}
			return Instance.worldCanvas;
		}
	}

	public static CanvasScaler WorldCanvasScaler
	{
		get
		{
			if (!Exists || Instance.type == Type.StartMenu)
			{
				return null;
			}
			if ((Object)(object)Instance.worldCanvasScaler == (Object)null)
			{
				Instance.worldCanvasScaler = ((Component)Instance.worldCanvas).GetComponent<CanvasScaler>();
			}
			return Instance.worldCanvasScaler;
		}
	}

	public static CanvasScaler CanvasScaler
	{
		get
		{
			if (!Exists)
			{
				return null;
			}
			if ((Object)(object)Instance.canvasScaler == (Object)null)
			{
				Instance.canvasScaler = ((Component)Instance).GetComponent<CanvasScaler>();
			}
			return Instance.canvasScaler;
		}
	}

	public static CanvasGroup CanvasGroup
	{
		get
		{
			if (!Exists)
			{
				return null;
			}
			if ((Object)(object)Instance.canvasGroup == (Object)null)
			{
				Instance.canvasGroup = ((Component)Instance).GetComponent<CanvasGroup>();
			}
			return Instance.canvasGroup;
		}
	}

	public static CanvasGroup WorldCanvasGroup
	{
		get
		{
			if (!Exists || Instance.type == Type.StartMenu)
			{
				return null;
			}
			return Instance.worldCanvasGroup;
		}
	}

	public static RectTransform WorldRectTransform
	{
		get
		{
			if (!Exists || Instance.type == Type.StartMenu)
			{
				return null;
			}
			if ((Object)(object)Instance.worldRectTransform == (Object)null)
			{
				Instance.worldRectTransform = ((Component)WorldCanvas).GetComponent<RectTransform>();
			}
			return Instance.worldRectTransform;
		}
	}

	public static UIIconData IconData => Instance.iconData;

	public static UIConstants.Screens QueuedDeepLink
	{
		set
		{
			queuedDeepLink = value;
		}
	}

	public static UIDeepLinkData QueuedDeepLinkData
	{
		set
		{
			queuedDeepLinkData = value;
		}
	}

	public static bool Exists => (Object)(object)Instance != (Object)null;

	public UISpriteDuplicator SpriteDuplicator
	{
		get
		{
			if (spriteDuplicator == null)
			{
				spriteDuplicator = new UISpriteDuplicator();
			}
			return spriteDuplicator;
		}
	}

	public ScreenKeyboardManager ScreenKeyboardManager
	{
		get
		{
			if ((Object)(object)screenKeyboardManager == (Object)null)
			{
				screenKeyboardManager = ((Component)this).GetComponent<ScreenKeyboardManager>();
			}
			return screenKeyboardManager;
		}
	}

	protected virtual void Awake()
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Expected O, but got Unknown
		Instance = this;
		if ((Object)(object)canvas == (Object)null)
		{
			canvas = ((Component)this).GetComponent<Canvas>();
		}
		if ((Object)(object)canvasScaler == (Object)null)
		{
			canvasScaler = ((Component)this).GetComponent<CanvasScaler>();
		}
		if ((Object)(object)rectTransform == (Object)null)
		{
			rectTransform = ((Component)this).GetComponent<RectTransform>();
		}
		if ((Object)(object)iconData != (Object)null)
		{
			iconData.Init();
		}
		if ((Object)(object)canvasScaler != (Object)null)
		{
			UIConstants.UI_PIXELS_PER_UNIT = canvasScaler.referencePixelsPerUnit;
		}
		canvasScalerHelper.Initialize();
		if (StartupManager.IsReady || type == Type.Ingame)
		{
			InitializeUIScreens();
		}
		else
		{
			GameEvents.OnStartupDone += OnStartupDone;
		}
		for (int i = 0; i < components.Length; i++)
		{
			components[i].Init();
		}
		if (queuedDeepLink != UIConstants.Screens.None)
		{
			OpenDeepLink(queuedDeepLink, queuedDeepLinkData);
			queuedDeepLink = UIConstants.Screens.None;
			queuedDeepLinkData = null;
		}
		else if (CurrentScreen == UIConstants.Screens.None)
		{
			ShowScreen(startScreen);
		}
		DebugConsole.AddCommand("toggle_ui", new CommandDelegate(CmdToggleUI), "Toggle UI");
	}

	private void OnEnable()
	{
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChanged;
		BackendEvents.OnReceivedGameSummary += OnReceivedGameSummary;
		InputEvents.OnButtonUp += OnButtonUp;
		InputEvents.OnButtonDown += OnButtonDown;
		UpdateCanvasInteraction();
	}

	private void OnDisable()
	{
		BackendEvents.OnBackendConnectionChanged -= OnBackendConnectionChanged;
		BackendEvents.OnReceivedGameSummary -= OnReceivedGameSummary;
		InputEvents.OnButtonUp -= OnButtonUp;
		InputEvents.OnButtonDown -= OnButtonDown;
	}

	public static void UpdateCanvasInteraction()
	{
		if (!((Object)(object)Instance == (Object)null))
		{
			bool interactable = InputManager.IsEnabled(InputManager.InputType.UI);
			if ((Object)(object)WorldCanvasGroup != (Object)null)
			{
				WorldCanvasGroup.interactable = interactable;
			}
			if ((Object)(object)CanvasGroup != (Object)null)
			{
				CanvasGroup.interactable = interactable;
			}
		}
	}

	private void OnDestroy()
	{
		DebugConsole.RemoveCommand("toggle_ui");
		DeInitializeUIScreens();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].DeInit();
		}
	}

	private void OnBackendConnectionChanged(ConnectionStatus status)
	{
		switch (status)
		{
		case ConnectionStatus.Connected:
		case ConnectionStatus.Reconnected:
			if (waitingForConnection)
			{
				NetworkUtils.HideLoader();
				waitingForConnection = false;
			}
			break;
		case ConnectionStatus.Reconnecting:
		case ConnectionStatus.Disconnected:
			if (type != Type.StartMenu && !waitingForConnection && GameManager.GameState != null && (GameManager.GameState.Settings.GameType == GameType.Multiplayer || GameManager.GameState.Settings.GameType == GameType.Competitive || GameManager.GameState.Settings.GameType == GameType.Matchmaking))
			{
				waitingForConnection = true;
				NetworkUtils.ShowLoader(Localization.Get("backend.connecting"), 0);
			}
			break;
		case ConnectionStatus.ConnectionFailed:
			break;
		}
	}

	private void OnButtonUp(InputManager.Buttons button)
	{
		GetCurrentScreen()?.OnButtonUp(button);
	}

	private void OnButtonDown(InputManager.Buttons button)
	{
		HudScreen hudScreen = GetCurrentScreen() as HudScreen;
		if (Object.op_Implicit((Object)(object)hudScreen))
		{
			switch (button)
			{
			case InputManager.Buttons.Tech:
				ShowScreen(UIConstants.Screens.TechTree);
				break;
			case InputManager.Buttons.GameStats:
				ShowScreen(UIConstants.Screens.StatsScreen);
				break;
			case InputManager.Buttons.EndTurn:
				hudScreen.OnNextTurn(forceConfirmation: true);
				break;
			}
		}
	}

	private void CmdToggleUI(string[] args)
	{
		if ((Object)(object)canvas == (Object)null)
		{
			Log.Verbose("Failed to toggle ui because the canvas was null {0}", new object[1] { ((object)this).GetHashCode() });
		}
		else
		{
			((Component)canvas).gameObject.SetActive(!((Component)canvas).gameObject.activeSelf);
		}
	}

	private void OnStartupDone()
	{
		InitializeUIScreens();
		GameEvents.OnStartupDone -= OnStartupDone;
	}

	private void InitializeUIScreens()
	{
		int num = screens.Length;
		for (int i = 0; i < num; i++)
		{
			if (!screens[i].Initialized)
			{
				screens[i].Init();
			}
		}
	}

	private void DeInitializeUIScreens()
	{
		int num = screens.Length;
		for (int i = 0; i < num; i++)
		{
			if (screens[i].Initialized)
			{
				screens[i].DeInit();
			}
		}
	}

	public UIScreenBase ShowScreen(UIConstants.Screens screen, bool instant = false)
	{
		if (screenStack.Count == 0 || screenStack[screenStack.Count - 1] != screen)
		{
			screenStack.Add(screen);
		}
		UIScreenBase uIScreenBase = null;
		int num = screens.Length;
		for (int i = 0; i < num; i++)
		{
			UIScreenBase uIScreenBase2 = screens[i];
			if (uIScreenBase2.screenType == screen)
			{
				uIScreenBase = uIScreenBase2;
			}
			else
			{
				screens[i].ShowScreen(screen, instant);
			}
		}
		if ((Object)(object)uIScreenBase != (Object)null)
		{
			uIScreenBase.ShowScreen(screen, instant);
		}
		RefreshInfo();
		return uIScreenBase;
	}

	public void OpenDeepLink(UIConstants.Screens screen, UIDeepLinkData deepLinData = null)
	{
		if (UIConstants.deepLinks.TryGetValue(screen, out var value))
		{
			screenStack.Clear();
			PopupManager.RemoveAllPopups();
			UIConstants.Screens[] array = value;
			foreach (UIConstants.Screens item in array)
			{
				screenStack.Add(item);
			}
			if (deepLinData != null)
			{
				GetScreen(screen).SetDeepLinkData(deepLinData);
			}
			ShowScreen(screen);
		}
		else
		{
			Log.Error("UIManager :: OpenDeepLink :: Could not find deep link for : {0}, please check UI constants for data", new object[1] { screen.ToString() });
		}
	}

	public static bool OpenMultiplayerScreen()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			return false;
		}
		if (Instance.CurrentScreen == UIConstants.Screens.MultiplayerScreen)
		{
			return false;
		}
		GameManager.GetSpriteAtlasManager().PreloadSpriteAtlases(SpriteAtlasManager.MENU_ATLASES, 10, delegate
		{
			GameManager.Instance.SetRemoteClient();
			string settingsNameFromModes = GameSettingsExtensions.GetSettingsNameFromModes((GameType)PolytopiaPlayerPrefs.GetInt("previous_multiplayer_game_type", 1), GameMode.Custom);
			if (!GameSettingsExtensions.TryLoadFromDisk(out var settings, settingsNameFromModes))
			{
				settings.GameType = GameType.Multiplayer;
			}
			GameManager.PreliminaryGameSettings = settings;
			Instance.OpenDeepLink(UIConstants.Screens.MultiplayerScreen);
		});
		return true;
	}

	public static bool OpenReplaysScreen()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			return false;
		}
		if (Instance.CurrentScreen == UIConstants.Screens.ReplaysScreen)
		{
			return false;
		}
		GameManager.GetSpriteAtlasManager().PreloadSpriteAtlases(SpriteAtlasManager.MENU_ATLASES, 10, delegate
		{
			Instance.OpenDeepLink(UIConstants.Screens.ReplaysScreen);
		});
		return true;
	}

	public static bool OpenLadderScreen()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			return false;
		}
		if (Instance.CurrentScreen == UIConstants.Screens.LadderScreen)
		{
			return false;
		}
		GameManager.GetSpriteAtlasManager().PreloadSpriteAtlases(SpriteAtlasManager.MENU_ATLASES, 10, delegate
		{
			Instance.OpenDeepLink(UIConstants.Screens.LadderScreen);
		});
		return true;
	}

	public static bool OpenTournamentsScreen()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			return false;
		}
		if (Instance.CurrentScreen == UIConstants.Screens.TournamentsScreen)
		{
			return false;
		}
		GameManager.GetSpriteAtlasManager().PreloadSpriteAtlases(SpriteAtlasManager.MENU_ATLASES, 10, delegate
		{
			Instance.OpenDeepLink(UIConstants.Screens.TournamentsScreen);
		});
		return true;
	}

	public UICanvasScalerHelper GetCanvasScalerHelper()
	{
		return canvasScalerHelper;
	}

	public UIScreenBase GetScreen(UIConstants.Screens screen)
	{
		int num = screens.Length;
		for (int i = 0; i < num; i++)
		{
			if (screens[i].screenType == screen)
			{
				return screens[i];
			}
		}
		return null;
	}

	public bool IsScreenInStack(UIConstants.Screens screen)
	{
		for (int i = 0; i < screenStack.Count; i++)
		{
			if (screenStack[i] == screen)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetCurrentScreen<T>(out T screen) where T : UIScreenBase
	{
		UIScreenBase screen2 = GetScreen(CurrentScreen);
		if ((Object)(object)screen2 != (Object)null && ((object)screen2).GetType() == typeof(T))
		{
			screen = screen2 as T;
			return true;
		}
		screen = null;
		return false;
	}

	public UIScreenBase GetCurrentScreen()
	{
		return GetScreen(CurrentScreen);
	}

	public void PopCurrentScreen()
	{
		screenStack.RemoveAt(screenStack.Count - 1);
		RefreshInfo();
	}

	public static Vector2 GetUISize()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = NativeHelpers.Screen();
		float num = ((Vector2Int)(ref val)).x;
		val = NativeHelpers.Screen();
		return new Vector2(num, (float)((Vector2Int)(ref val)).y) * UICanvasScalerHelper.GetInvertedUIScale();
	}

	public static float GetUIHeight()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = NativeHelpers.Screen();
		return ((float)((Vector2Int)(ref val)).y - (ScreenManager.SafeTop + ScreenManager.SafeBottom)) * UICanvasScalerHelper.GetInvertedUIScale();
	}

	public static float GetUIWidth()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = NativeHelpers.Screen();
		return ((float)((Vector2Int)(ref val)).x - (ScreenManager.SafeLeft + ScreenManager.SafeRight)) * UICanvasScalerHelper.GetInvertedUIScale();
	}

	public void AddScreenToStack(UIConstants.Screens screen)
	{
		screenStack.Add(screen);
		RefreshInfo();
	}

	public void RemoveScreenFromStack(UIConstants.Screens screen)
	{
		for (int num = screenStack.Count - 1; num >= 0; num--)
		{
			if (screenStack[num] == screen)
			{
				screenStack.RemoveAt(num);
				break;
			}
		}
		RefreshInfo();
	}

	public void OnBack()
	{
		if (screenStack.Count > 1)
		{
			screenStack.RemoveAt(screenStack.Count - 1);
			ShowScreen(CurrentScreen);
		}
	}

	private bool IsShowingTribePickerForGame(Guid gameId)
	{
		if (CurrentScreen == UIConstants.Screens.TribeSelector && GameManager.Client.CurrentGameId.HasValue)
		{
			return GameManager.Client.CurrentGameId.Value == gameId;
		}
		return false;
	}

	private void ShowPickTribeNotification(GameSummaryViewModel summaryViewModel, GameStateSummary summary)
	{
		AudioManager.PlaySFX(SFXTypes.Discover);
		if (type == Type.StartMenu && !PopupManager.IsPopupShowing<GameInfoPopup>() && !IsShowingTribePickerForGame(summaryViewModel.GameId))
		{
			GameInfoPopup gameInfoPopup = PopupManager.GetGameInfoPopup();
			gameInfoPopup.SetData(summaryViewModel);
			gameInfoPopup.Show();
		}
		else if (type == Type.Ingame && !PopupManager.IsPopupShowing<BasicPopup>("YOURTURN"))
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.identifier = "YOUR_TURN" + summaryViewModel.GameId;
			basicPopup.Header = Localization.Get("wcontroller.online.yourturn.title");
			basicPopup.Description = Localization.Get("wcontroller.picktribe.notification", summary.GameName);
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			PopupBase currentPopup = PopupManager.GetCurrentPopup();
			if ((Object)(object)currentPopup != (Object)null && currentPopup.identifier == basicPopup.identifier)
			{
				currentPopup.Hide();
			}
			basicPopup.Show();
		}
	}

	private void OnReceivedGameSummary(GameSummaryViewModel summaryViewModel, StateUpdateReason reason)
	{
		if (!SerializationHelpers.FromByteArray<GameStateSummary>(summaryViewModel.GameSummaryData, out var result))
		{
			return;
		}
		GameStateSummary.GamePlayerSummary gamePlayerSummary = null;
		foreach (GameStateSummary.GamePlayerSummary playerSummary in result.PlayerSummaries)
		{
			if (playerSummary.PolytopiaId == AccountManager.PlayerAccountId)
			{
				gamePlayerSummary = playerSummary;
				break;
			}
		}
		if (reason == StateUpdateReason.GameCreated && summaryViewModel.MatchmakingGameId.HasValue)
		{
			if (result.CurrentPlayer == gamePlayerSummary?.Id)
			{
				ShowPickTribeNotification(summaryViewModel, result);
			}
			else
			{
				AudioManager.PlaySFX(SFXTypes.Discover);
				NotificationManager.Notify(Localization.Get("wcontroller.matchmaking.gamestarted", result.GameName));
			}
		}
		if (reason == StateUpdateReason.ValidPickTribe && result.CurrentPlayer == gamePlayerSummary?.Id)
		{
			ShowPickTribeNotification(summaryViewModel, result);
		}
		if (type == Type.StartMenu && reason == StateUpdateReason.ValidStartGame && !PopupManager.IsPopupShowing<GameInfoPopup>())
		{
			GameInfoPopup gameInfoPopup = PopupManager.GetGameInfoPopup();
			gameInfoPopup.SetData(summaryViewModel);
			gameInfoPopup.Show();
		}
		if (type != Type.StartMenu)
		{
			if (type != Type.Ingame || GameManager.GameState == null)
			{
				return;
			}
			Guid gameId = summaryViewModel.GameId;
			Guid? currentGameId = GameManager.Client.CurrentGameId;
			if (!(gameId != currentGameId))
			{
				return;
			}
		}
		if (reason == StateUpdateReason.GameEnded)
		{
			AudioManager.PlaySFX(SFXTypes.Discover);
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("wcontroller.online.gameended.title");
			basicPopup.Description = Localization.Get("push.ended", result.GameName);
			basicPopup.buttonData = new PopupBase.PopupButtonData[2]
			{
				new PopupBase.PopupButtonData("buttons.back"),
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					TryOpenGameMultiplayerGame(summaryViewModel.GameId);
				})
			};
			basicPopup.Show();
		}
		bool flag = result.CurrentPlayer == gamePlayerSummary?.Id;
		if (reason == StateUpdateReason.PlayerRemindedToPlay && flag)
		{
			AudioManager.PlaySFX(SFXTypes.Discover);
			BasicPopup basicPopup2 = PopupManager.GetBasicPopup();
			basicPopup2.identifier = "TURN_REMINDER" + summaryViewModel.GameId;
			basicPopup2.Header = Localization.Get("wcontroller.online.yourturn.title");
			basicPopup2.Description = Localization.Get("gameitem.remind.notification", result.GameName);
			basicPopup2.buttonData = new PopupBase.PopupButtonData[2]
			{
				new PopupBase.PopupButtonData("buttons.back"),
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					TryOpenGameMultiplayerGame(summaryViewModel.GameId);
				})
			};
			if (PopupManager.TryGetOpenPopup<BasicPopup>(basicPopup2.identifier, out var openPopup))
			{
				openPopup.Hide();
			}
			basicPopup2.Show();
		}
		if ((reason == StateUpdateReason.ValidCommand || reason == StateUpdateReason.ValidStartGame) && summaryViewModel.State != GameSessionState.ReadyToStart && flag)
		{
			AudioManager.PlaySFX(SFXTypes.Discover);
			BasicPopup basicPopup3 = PopupManager.GetBasicPopup();
			basicPopup3.identifier = "YOUR_TURN" + summaryViewModel.GameId;
			basicPopup3.Header = Localization.Get("wcontroller.online.yourturn.title");
			basicPopup3.Description = string.Format("{0}\n\n{1}", Localization.Get("wcontroller.turn.notification", result.GameName, result.CurrentTurn), Localization.Get("wcontroller.online.yourturn.description"));
			basicPopup3.buttonData = new PopupBase.PopupButtonData[2]
			{
				new PopupBase.PopupButtonData("buttons.back"),
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					TryOpenGameMultiplayerGame(summaryViewModel.GameId);
				})
			};
			if (PopupManager.TryGetOpenPopup<BasicPopup>(basicPopup3.identifier, out var openPopup2))
			{
				openPopup2.Hide();
			}
			basicPopup3.Show();
		}
	}

	private void TryOpenGameMultiplayerGame(Guid gameId)
	{
		if (GameManager.IsMultiplayerEnabled)
		{
			UIBlackFader.FadeIn(0.5f, async delegate
			{
				NetworkUtils.ShowLoader(1000);
				if (!(await GameManager.Instance.OpenMultiplayerGame(gameId)))
				{
					UIBlackFader.FadeOut();
				}
				NetworkUtils.HideLoader();
			});
		}
		else if (type == Type.StartMenu)
		{
			ShowScreen(UIConstants.Screens.MultiplayerScreen);
		}
		else
		{
			GameManager.ReturnToMenu();
		}
	}

	private void RefreshInfo()
	{
	}

	public void BlockHints()
	{
		advisorManager.BlockHints();
	}

	public void UnblockHints()
	{
		advisorManager.UnblockHints();
	}

	public void SetTutorialCommandType(CommandType commandType)
	{
		advisorManager.SetTutorialCommandType(commandType);
	}
}
