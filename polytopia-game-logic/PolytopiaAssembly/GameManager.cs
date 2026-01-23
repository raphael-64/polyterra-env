using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DG.Tweening;
using Polytopia.Data;
using Polytopia.IO;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Challengermode.GameIntegration.BindingModels;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using PolytopiaBackendBase.Notifications;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class GameManager : MonoBehaviour
{
	public const string LOG_PREFIX = "<color=#FFFFFF>[GameManager]</color>";

	private static GameManager instance;

	[Header("Game settings")]
	[SerializeField]
	protected PolytopiaDataHolder dataHolder;

	[SerializeField]
	protected IAPData iapData;

	[Header("References")]
	[SerializeField]
	private AnimationCurves sharedAnimationCurves;

	private Guid CmdGameId = Guid.NewGuid();

	[Header("Debug")]
	[SerializeField]
	[Tooltip("If enabled a new game will always be created when using debug start")]
	protected bool alwaysCreate;

	[SerializeField]
	[Tooltip("Which play mode should debug start use")]
	protected GameType debugGameType;

	[SerializeField]
	[Tooltip("Which gamemode should debug start use")]
	protected GameMode gameMode = GameMode.Perfection;

	[SerializeField]
	protected int aiOpponents = 1;

	[SerializeField]
	protected int playerOpponents;

	[SerializeField]
	protected bool randomTribe = true;

	[SerializeField]
	protected TribeData.Type startingTribe;

	[SerializeField]
	protected SkinType startingSkin;

	protected TribeData.Type startingTribeMix;

	private GameSettings settings;

	private ClientBase client;

	private PolytopiaSpriteRendererManager spriteRendererManager = new PolytopiaSpriteRendererManager();

	private SpriteAtlasManager spriteAtlasManager = new SpriteAtlasManager();

	private PurchaseManager purchaseManager = new PurchaseManager();

	private AnalyticsManager analyticsManager = new AnalyticsManager();

	private VersioningInfoHolder versioningInfoHolder = new VersioningInfoHolder();

	private MeshCache meshCache = new MeshCache();

	private DeepLinkManager deepLinkManager = new DeepLinkManager();

	private RemoteGameDataManager remoteGameDataManager = new RemoteGameDataManager();

	private ReplaysManager replaysManager = new ReplaysManager();

	private LadderManager ladderManager = new LadderManager();

	private LobbyManager lobbyManager = new LobbyManager();

	private TournamentManager tournamentManager = new TournamentManager();

	private LoginManager loginManager = new LoginManager();

	private HotseatProfilesState hotseatProfilesState;

	private float defaultFixedDelta;

	private float defaultMaximumDelta;

	private List<Coroutine> delayCoroutines = new List<Coroutine>();

	private bool isPaused;

	private bool isLevelLoaded;

	private bool isLoadingGame;

	private int actionableGamesCount;

	private int unreadNewsCount;

	private static bool hasSuccessfullyCheckedSteamNotifications;

	private static bool steamNotificationsEnabled;

	public static Tile debugTile;

	public static bool debugShouldSkipReactions;

	public static bool debugAutoPlayLocalPlayer;

	public static bool debugSkipNextSend;

	public static bool HasSaveGame
	{
		get
		{
			string saveDirectoryPath = Paths.GetSaveDirectoryPath("Singleplayer");
			if (PolytopiaDirectory.Exists(saveDirectoryPath))
			{
				string[] files = PolytopiaDirectory.GetFiles(saveDirectoryPath, "*.state");
				if (files != null)
				{
					return files.Length != 0;
				}
				return false;
			}
			return false;
		}
	}

	public static GameManager Instance => instance;

	public static string LocalUsername => AccountManager.Alias;

	public static PlayerState LocalPlayer => Client?.GetCurrentLocalPlayer();

	public static ClientBase Client => Instance.client;

	public static GameState GameState => Instance.client?.GameState;

	public static TribeData.Type StartingTribe
	{
		get
		{
			if (Instance.randomTribe)
			{
				GameState.GameLogicData.GetAllTribes();
				Instance.startingTribe = GameStateUtils.GetRandomPickableTribe(VersionManager.GameVersion, instance.settings, null);
			}
			return Instance.startingTribe;
		}
		set
		{
			Instance.startingTribe = value;
			Instance.randomTribe = false;
		}
	}

	public static TribeData.Type StartingTribeMix
	{
		get
		{
			return Instance.startingTribeMix;
		}
		set
		{
			Instance.startingTribeMix = value;
		}
	}

	public static SkinType StartingSkin
	{
		get
		{
			return Instance.startingSkin;
		}
		set
		{
			Instance.startingSkin = value;
		}
	}

	public static GameSettings PreliminaryGameSettings
	{
		get
		{
			if (instance.settings == null)
			{
				instance.settings = new GameSettings();
			}
			return instance.settings;
		}
		set
		{
			instance.settings = value;
		}
	}

	public static float TimeScale
	{
		get
		{
			return Time.timeScale;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)instance) && value != Time.timeScale)
			{
				Time.timeScale = Mathf.Max(0f, value);
				Time.fixedDeltaTime = instance.defaultFixedDelta * value;
				Time.maximumDeltaTime = instance.defaultMaximumDelta * value;
			}
		}
	}

	public static Uri LocalEndpoint => new Uri($"http://{IPAddress.Loopback}");

	public static Uri RemoteEndpoint => new Uri(Config.backendUri.Value);

	public static IAPData IAPData => instance.iapData;

	public static bool IsMultiplayerEnabled
	{
		get
		{
			if (SystemManager.IsMobile)
			{
				bool num = GetPurchaseManager().IsAnythingBought();
				bool flag = AreNotificationsEnabled();
				bool hasSocialLogin = PolytopiaBackendAdapter.Instance.HasSocialLogin;
				return num && flag && hasSocialLogin;
			}
			if (SystemManager.IsTesla)
			{
				return PolytopiaBackendAdapter.Instance.HasSocialLogin;
			}
			return AreNotificationsEnabled();
		}
	}

	public static bool HasSuccessfullyCheckedSteamNotifications => hasSuccessfullyCheckedSteamNotifications;

	public static int ActionableGamesCount
	{
		get
		{
			return instance.actionableGamesCount;
		}
		set
		{
			instance.actionableGamesCount = value;
			NativeHelpers.SetIconBadgeNumber(value);
		}
	}

	public static int UnreadNewsCount
	{
		get
		{
			return instance.unreadNewsCount;
		}
		set
		{
			instance.unreadNewsCount = value;
		}
	}

	private void Awake()
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Expected O, but got Unknown
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Expected O, but got Unknown
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Expected O, but got Unknown
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Expected O, but got Unknown
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Expected O, but got Unknown
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Expected O, but got Unknown
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Expected O, but got Unknown
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Expected O, but got Unknown
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Expected O, but got Unknown
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Expected O, but got Unknown
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Expected O, but got Unknown
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Expected O, but got Unknown
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Expected O, but got Unknown
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Expected O, but got Unknown
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Expected O, but got Unknown
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Expected O, but got Unknown
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Expected O, but got Unknown
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Expected O, but got Unknown
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Expected O, but got Unknown
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Expected O, but got Unknown
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Expected O, but got Unknown
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Expected O, but got Unknown
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Expected O, but got Unknown
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Expected O, but got Unknown
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Expected O, but got Unknown
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Expected O, but got Unknown
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Expected O, but got Unknown
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Expected O, but got Unknown
		//IL_04e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Expected O, but got Unknown
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Expected O, but got Unknown
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_052a: Expected O, but got Unknown
		//IL_0536: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)instance))
		{
			Log.Warning("{0} An instance is already loaded", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		SceneManager.sceneUnloaded += OnSceneUnloaded;
		SceneManager.sceneLoaded += OnSceneLoaded;
		meshCache.CacheLayerIDs();
		AndroidRotationLockUtil.FixAndroidAutorotate();
		instance = this;
		SpriteAtlasManager.atlasRequested += RequestAtlas;
		bool flag = !NativeHelpers.IsValidInstall();
		if (flag)
		{
			Log.Warning("Yarr, the game be pirated!", Array.Empty<object>());
		}
		analyticsManager.Init();
		analyticsManager.SendEvent("AppStarted", new Dictionary<string, object>
		{
			{
				"version",
				VersionManager.SemanticVersion.ToString()
			},
			{
				"gameVersion",
				VersionManager.GameVersion
			},
			{ "isPirated", flag },
			{
				"installerName",
				Application.installerName
			},
			{
				"installMode",
				Application.installMode
			}
		});
		AnalyticsManager.SetCrashMetaData("is_pirated", flag.ToString());
		LocalizationEvents.OnLanguageChanged += LanguageChangedMetadata;
		LanguageChangedMetadata(Localization.Language);
		ServerResponseHelpers.OnError = HandleServerError;
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChanged;
		versioningInfoHolder.LoadVersionInformation();
		lobbyManager.Initialize();
		remoteGameDataManager.Initialize();
		tournamentManager.Initialize();
		Application.targetFrameRate = Config.frameRate.IntValue;
		QualitySettings.vSyncCount = Config.vSync.IntValue;
		defaultFixedDelta = Time.fixedDeltaTime;
		defaultMaximumDelta = Time.maximumDeltaTime;
		CmdGameInfo(null);
		DebugConsole.AddCommand("quit", new CommandDelegate(CmdQuit), "Quit the game");
		DebugConsole.AddCommand("saveconfig", new CommandDelegate(CmdSaveConfig), "Save the user config variables");
		DebugConsole.AddCommand("loadconfig", new CommandDelegate(CmdLoadConfig), "Load the user config variables");
		DebugConsole.AddCommand("gameinfo", new CommandDelegate(CmdGameInfo), "Show game information");
		DebugConsole.AddCommand("cl_timescale", new CommandDelegate(CmdTimeScale), "Set the time scale");
		DebugConsole.AddCommand("ai_getaction", new CommandDelegate(CmdGetAIAction), "Get AI Move");
		DebugConsole.AddCommand("ai_performaction", new CommandDelegate(CmdPerformAIAction), "Perform AI Move");
		DebugConsole.AddCommand("cl_gamesummary", new CommandDelegate(CmdCreateGameSummary), "Create game summary from current gamestate");
		DebugConsole.AddCommand("sv_backendstatus", new CommandDelegate(CmdBackendStatus), "Log backend connection status");
		DebugConsole.AddCommand("sv_whoami", new CommandDelegate(CmdWhoAmI), "Who am I?");
		DebugConsole.AddCommand("sv_uploadhighscore", new CommandDelegate(CmdUploadHighscore), "Upload a highscore");
		DebugConsole.AddCommand("sv_addfriend", new CommandDelegate(CmdAddFriend), "Add a friend.");
		DebugConsole.AddCommand("sv_removefriend", new CommandDelegate(CmdRemoveFriend), "Remove an existing friend or friend request.");
		DebugConsole.AddCommand("sv_acceptfriend", new CommandDelegate(CmdAcceptFriend), "Accept a friend request.");
		DebugConsole.AddCommand("sv_login", new CommandDelegate(CmdLoginAndConnect), "Login & Connect");
		DebugConsole.AddCommand("sv_uploadhighscore", new CommandDelegate(CmdUploadHighscore), "Upload a highscore");
		DebugConsole.AddCommand("sv_observe", new CommandDelegate(CmdObserveGame), "Observe a game or replay");
		DebugConsole.AddCommand("sv_join", new CommandDelegate(CmdJoinGame), "Join a game");
		DebugConsole.AddCommand("sv_savegame", new CommandDelegate(CmdSaveGame), "Save game");
		DebugConsole.AddCommand("sv_listsaved", new CommandDelegate(CmdGetSavedGames), "List saved games");
		DebugConsole.AddCommand("sv_resign", new CommandDelegate(CmdResignFromGame), "Resign from game");
		DebugConsole.AddCommand("sv_kick", new CommandDelegate(CmdKickFromGame), "Resign from game");
		DebugConsole.AddCommand("sv_login_debug_legacy", new CommandDelegate(CmdLoginAndConnectLegacyDebug), "Login & Connect");
		DebugConsole.AddCommand("sv_create_local", new CommandDelegate(CmdCreateMultiplayerGame), "Create multiplayer games using this client");
		DebugConsole.AddCommand("sv_create_remote", new CommandDelegate(CmdCreateMultiplayerGameConfig), "Create multiplayer games directly on the server using a config file");
		DebugConsole.AddCommand("sv_start_game", new CommandDelegate(CmdStartMultiplayerGame), "Start a game with a sppecific id");
		DebugConsole.AddCommand("tribe_setscore", new CommandDelegate(CmdSetTribeScore), "Set the score for a certain tribe (used in perfection). first argument is tribe number, second argument is the score");
		DebugConsole.AddCommand("tribe_setrating", new CommandDelegate(CmdSetTribeRating), "Set the rating for a certain tribe(used in domination). first argument is tribe number, second argument is the rating in %");
		DebugConsole.AddCommand("sv_connect_challengermode", new CommandDelegate(CmdConnectChallengermodeOAuth), "Connect game account to Challengermode.");
		DebugConsole.AddCommand("sv_challengermode_connection_status", new CommandDelegate(CmdLogCmConnectionStatus), "Log Challengermode connection status.");
		DebugConsole.AddCommand("sv_join_ladder", new CommandDelegate(CmdJoinLadder), "Join ladder.");
		DebugConsole.AddCommand("sv_get_tournament", new CommandDelegate(CmdGetTournament), "Get tournament view model");
		DebugConsole.AddCommand("sv_get_tournament_list", new CommandDelegate(CmdGetTournamentList), "Get tournament list from search api");
		ResourceManager.InitResources();
		TweenUtils.CacheRoughTweens();
		PlayerPrefsUtils.Initialize();
		NativeHelpers.SetPlatformAudioStateChangeListener(delegate
		{
			AudioManager.VolumeChanged();
		});
		Object.DontDestroyOnLoad((Object)(object)this);
	}

	public void Initialize()
	{
		Log.Verbose("{0} Initializing PolytopiaData...", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		Log.Verbose("[VersionManager] GameLogicData version is {0}", new object[1] { VersionManager.GameLogicDataVersion });
		SetupPolytopiaData(dataHolder);
		purchaseManager.Init();
		VersionMigration.MigrateMiscSettings();
		VersionMigration.MigrateLegacyConsentData();
		VersionMigration.MigrateLegacySaveData();
		Log.Verbose("{0} Initialized", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
	}

	private void CallApplicationPause(bool paused)
	{
		if (isPaused != paused)
		{
			isPaused = paused;
			Log.Verbose("Pausing {0}", new object[1] { paused });
			GameObject[] array = Object.FindObjectsOfType<GameObject>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SendMessage("OnApplicationPause", (object)paused);
			}
		}
	}

	private void SetupPolytopiaData(PolytopiaDataHolder dataHolder)
	{
		PolytopiaDataManager.provider = dataHolder;
		PolytopiaDataManager.LoadGameLogicData(VersionManager.GameLogicDataVersion);
		PolytopiaDataManager.LoadAvatarData(VersionManager.AvatarDataVersion);
	}

	private void OnSceneUnloaded(Scene current)
	{
		Log.Info("Killing tweens upon unloading scene", Array.Empty<object>());
		DOTween.KillAll(false);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Log.Spam("Finished loading scene: {0}", new object[1] { ((Scene)(ref scene)).name });
		AnalyticsHelpers.SendCurrencyEvent();
		if (((Scene)(ref scene)).name == "StartScene")
		{
			InputManager.EnableAllInput();
		}
		else if (((Scene)(ref scene)).name == "Level")
		{
			InputManager.EnableAllInput();
			InputManager.ResetInputBlocker();
			InputManager.BlockInputIfShowingLoadingScreen();
		}
	}

	private void LanguageChangedMetadata(Localization.Languages language)
	{
		AnalyticsManager.SetCrashMetaData("application_language", language.ToString());
		if (language == Localization.Languages.Custom)
		{
			PolytopiaPlayerPrefs.GetString("customlanguageurl");
			AnalyticsManager.SetCrashMetaData("application_custom_language_url", language.ToString());
		}
		else
		{
			AnalyticsManager.SetCrashMetaData("application_custom_language_url", null);
		}
	}

	private void HandleServerError(Exception exception)
	{
		Log.Warning("Server error {0}", new object[1] { exception.ToString() });
		analyticsManager.SendEvent("network_error", new Dictionary<string, object>
		{
			{ "Message", exception.Message },
			{ "StackTrace", exception.StackTrace }
		});
	}

	private void RequestAtlas(string tag, Action<SpriteAtlas> callback)
	{
		GetSpriteAtlasManager().LoadSpriteAtlas(tag, callback);
	}

	public static void DelayCall(int milliseconds, Action onComplete)
	{
		if (milliseconds <= 0)
		{
			onComplete();
		}
		else
		{
			DelayCall((float)milliseconds / 1000f, onComplete);
		}
	}

	private static void DelayCall(float seconds, Action onComplete)
	{
		if (seconds <= 0f)
		{
			onComplete();
		}
		Coroutine newDelayCoroutine = null;
		newDelayCoroutine = ((MonoBehaviour)Instance).StartCoroutine(Instance.DelayedCall(seconds, delegate
		{
			Instance.delayCoroutines.Remove(newDelayCoroutine);
			onComplete();
		}));
		Instance.delayCoroutines.Add(newDelayCoroutine);
	}

	public static void CancelDelay()
	{
		for (int i = 0; i < Instance.delayCoroutines.Count; i++)
		{
			Log.Verbose("Cancelled ongoing delay", Array.Empty<object>());
			((MonoBehaviour)Instance).StopCoroutine(Instance.delayCoroutines[i]);
		}
		Instance.delayCoroutines.Clear();
	}

	private IEnumerator DelayedCall(float seconds, Action onComplete)
	{
		yield return (object)new WaitForSeconds(seconds);
		onComplete();
	}

	public static async void ReturnToMenu()
	{
		Log.Info("Returning to menu {0}", new object[1] { Time.frameCount });
		if (!Instance.isLevelLoaded)
		{
			Log.Warning("Failed to return to menu, because we're not in game scene", Array.Empty<object>());
		}
		else
		{
			LoadStartScene();
		}
	}

	private static void SetMenuReturnScreen()
	{
		if (Client.IsReplay)
		{
			UIManager.QueuedDeepLink = UIConstants.Screens.ReplaysScreen;
			return;
		}
		GameState gameState = GameState;
		if (gameState == null || gameState.Settings.GameType != GameType.Multiplayer)
		{
			GameState gameState2 = GameState;
			if (gameState2 == null || gameState2.Settings.GameType != GameType.PassAndPlay)
			{
				return;
			}
		}
		UIManager.QueuedDeepLink = UIConstants.Screens.MultiplayerScreen;
	}

	private static async Task UnloadLevelScene()
	{
		CancelDelay();
		Client?.ActionManager?.AbortExecution(null);
		Instance.isLevelLoaded = false;
		WorldIconContainer.CancelDelays();
		DOTween.KillAll(false);
		InputManager.DisableAllInput();
		Instance.SubscribeToGameParticipantStatuses(subscribe: false).WrapErrors();
		Log.Spam("Waiting one frame... {0} ({1})", new object[2]
		{
			Instance,
			((Component)Instance).gameObject.activeInHierarchy
		});
		await new WaitForUpdate();
		Log.Spam("Waiting another frame...", Array.Empty<object>());
		await new WaitForUpdate();
		Log.Spam("Disconnecting client...", Array.Empty<object>());
		Client?.Disconnect();
		Log.Spam("Destroying map...", Array.Empty<object>());
		MapRenderer current = MapRenderer.Current;
		Object.DestroyImmediate((Object)(object)((current != null) ? ((Component)current).gameObject : null));
		Log.Spam("Removing popups...", Array.Empty<object>());
		PopupManager.RemoveAllPopups();
		Log.Spam("Killing tweens...", Array.Empty<object>());
		DOTween.KillAll(false);
	}

	private static async void LoadStartScene()
	{
		SetMenuReturnScreen();
		await UnloadLevelScene();
		Log.Spam("Loading scene...", Array.Empty<object>());
		SceneManager.LoadScene("StartScene", (LoadSceneMode)0);
	}

	public async Task CreateSinglePlayerGame()
	{
		if (!IsLoadingGame())
		{
			SetLoadingGame(isLoading: true);
			Log.Info("{0} Starting new Singleplayer {1} Game...", new object[2] { "<color=#FFFFFF>[GameManager]</color>", settings.BaseGameMode });
			SetLocalClient();
			int opponentCount = settings.OpponentCount;
			settings.AddPlayer(new PlayerData
			{
				profile = new PlayerProfileState
				{
					id = AccountManager.PlayerAccountId,
					name = AccountManager.Alias,
					avatarState = AccountManager.AvatarState
				},
				type = PlayerData.Type.Player,
				state = PlayerData.State.IsYou,
				defaultName = LocalUsername,
				tribe = StartingTribe,
				tribeMix = StartingTribeMix,
				knownTribe = true,
				skinType = startingSkin
			});
			new Random();
			for (int i = 0; i < opponentCount; i++)
			{
				PlayerData playerData = new PlayerData
				{
					type = PlayerData.Type.Bot,
					botDifficulty = settings.Difficulty
				};
				playerData.profile.id = Guid.NewGuid();
				settings.AddPlayer(playerData);
			}
			settings.OpponentCount = opponentCount;
			settings.mapPreset = ((settings.mapPreset == MapPreset.None) ? MapPreset.Continents : settings.mapPreset);
			if (await client.CreateSession(settings, null) == CreateSessionResult.Success)
			{
				LoadLevel();
				return;
			}
			Log.Warning("{0} Failed to start singleplayer game", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		}
	}

	private void SetLoadingGame(bool isLoading)
	{
		isLoadingGame = isLoading;
		if (isLoadingGame && Object.op_Implicit((Object)(object)InputSystemCursorOverride.INSTANCE))
		{
			InputSystemCursorOverride.INSTANCE.Hide();
		}
	}

	private bool IsLoadingGame()
	{
		return isLoadingGame;
	}

	public async void ResumeSingleplayerGame()
	{
		if (IsLoadingGame())
		{
			return;
		}
		SetLoadingGame(isLoading: true);
		Log.Info("{0} Loading Singleplayer Game...", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		SetLocalClient();
		Guid[] sessions = client.GetSessions(0L);
		if (sessions != null && sessions.Length != 0 && await client.OpenSession(sessions[0]))
		{
			UIBlackFader.FadeIn(0.5f, delegate
			{
				LoadLevel();
			});
		}
		else
		{
			Log.Warning("{0} Failed to resume Singleplayer game: No saved game found", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
			SetLoadingGame(isLoading: false);
		}
	}

	public async void CreateHotseatGame()
	{
		if (!IsLoadingGame())
		{
			SetLoadingGame(isLoading: true);
			Log.Info("{0} Starting new Hotseat {1} Game...", new object[2] { "<color=#FFFFFF>[GameManager]</color>", settings.BaseGameMode });
			SetHotseatClient();
			if (await client.CreateSession(settings, null) == CreateSessionResult.Success)
			{
				LoadLevel();
				return;
			}
			Log.Warning("{0} Failed to create Hotseat game", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
			SetLoadingGame(isLoading: false);
		}
	}

	public async Task<bool> ResumeHotseatGame(Guid gameId)
	{
		if (IsLoadingGame())
		{
			return false;
		}
		SetLoadingGame(isLoading: true);
		Log.Info("{0} Loading Hotseat Game...", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		SetHotseatClient();
		if (await client.OpenSession(gameId))
		{
			LoadLevel();
			return true;
		}
		Log.Warning("{0} Failed to resume Hotseat game: {1}", new object[2]
		{
			"<color=#FFFFFF>[GameManager]</color>",
			gameId.ToString()
		});
		SetLoadingGame(isLoading: false);
		return false;
	}

	public async Task<CreateSessionResult> CreateMultiplayerGame()
	{
		Log.Verbose("{0} Creating Multiplayer {1} game...", new object[2] { "<color=#FFFFFF>[GameManager]</color>", settings.BaseGameMode });
		SetRemoteClient();
		return await client.CreateSession(settings, null);
	}

	public async Task<bool> StartMultiplayerGame(Guid gameId)
	{
		if (IsLoadingGame())
		{
			return false;
		}
		SetLoadingGame(isLoading: true);
		Log.Info("{0} Starting multiplayer game...", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		SetRemoteClient();
		ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.StartGame(new StartGameBindingModel
		{
			GameId = gameId
		});
		if (!serverResponse.Success)
		{
			PopupManager.ShowErrorPopup(Localization.GetErrorMessage(serverResponse));
			SetLoadingGame(isLoading: false);
			return false;
		}
		if (await client.OpenSession(gameId))
		{
			AnalyticsHelpers.SendGameStartEvent(gameId, settings, LocalPlayer?.tribe);
			LoadLevel();
			return true;
		}
		Log.Warning("{0} Failed to start Multiplayer game: {1}", new object[2]
		{
			"<color=#FFFFFF>[GameManager]</color>",
			gameId.ToString()
		});
		SetLoadingGame(isLoading: false);
		return false;
	}

	public async Task<bool> OpenMultiplayerGame(Guid gameId)
	{
		if (IsLoadingGame())
		{
			return false;
		}
		SetLoadingGame(isLoading: true);
		Log.Info("{0} Opening multiplayer game...", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		RemoteClient newClient = CreateRemoteClient();
		if (await newClient.OpenSession(gameId))
		{
			if (UIManager.Instance.type == UIManager.Type.Ingame)
			{
				await UnloadLevelScene();
			}
			SetRemoteClient(newClient);
			LoadLevel();
			return true;
		}
		Log.Warning("{0} Failed to open Multiplayer game: {1}", new object[2]
		{
			"<color=#FFFFFF>[GameManager]</color>",
			gameId.ToString()
		});
		SetLoadingGame(isLoading: false);
		return false;
	}

	public async Task<bool> OpenReplay(Guid gameId, GameSummaryViewModel summaryViewModel)
	{
		if (IsLoadingGame())
		{
			return false;
		}
		SetLoadingGame(isLoading: true);
		Log.Info("{0} Opening replay...", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		SetReplayClient();
		GetReplaysManager().SetCurrentReplaySummary(summaryViewModel);
		if (await Client.OpenSession(gameId))
		{
			LoadLevel();
			return true;
		}
		Log.Warning("{0} Failed to open replay: {1}", new object[2]
		{
			"<color=#FFFFFF>[GameManager]</color>",
			gameId.ToString()
		});
		SetLoadingGame(isLoading: false);
		return false;
	}

	public void LoadLevel()
	{
		Log.Info("{0} Loading level...", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		isLevelLoaded = false;
		PopupManager.RemoveAllPopups();
		InputManager.DisableAllInput();
		SceneManager.LoadScene("Level", (LoadSceneMode)0);
	}

	public void SetLocalClient()
	{
		settings.GameType = GameType.SinglePlayer;
		if (client == null || !(client is LocalClient))
		{
			Log.Verbose("{0} Setting up local client...", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
			client = new LocalClient
			{
				OnConnected = OnLocalClientConnected,
				OnDisconnected = OnClientDisconnected,
				OnSessionOpened = OnClientSessionOpened,
				OnStateUpdated = OnStateUpdated,
				OnStartedProcessingActions = OnStartedProcessingActions,
				OnFinishedProcessingActions = OnFinishedProcessingActions
			};
		}
	}

	public void SetHotseatClient()
	{
		settings.GameType = GameType.PassAndPlay;
		if (client == null || !(client is HotseatClient))
		{
			Log.Verbose("{0} Setting up hotseat client...", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
			client = new HotseatClient
			{
				OnConnected = OnLocalClientConnected,
				OnDisconnected = OnClientDisconnected,
				OnSessionOpened = OnClientSessionOpened,
				OnStateUpdated = OnStateUpdated,
				OnStartedProcessingActions = OnStartedProcessingActions,
				OnFinishedProcessingActions = OnFinishedProcessingActions
			};
		}
	}

	public void SetRemoteClient()
	{
		if (settings.GameType != GameType.Multiplayer && settings.GameType != GameType.Competitive)
		{
			settings.GameType = GameType.Multiplayer;
		}
		if (client == null || !(client is RemoteClient))
		{
			Log.Verbose("{0} Setting up remote client...", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
			client = CreateRemoteClient();
		}
	}

	public void SetRemoteClient(RemoteClient client)
	{
		if (settings.GameType != GameType.Multiplayer && settings.GameType != GameType.Competitive)
		{
			settings.GameType = GameType.Multiplayer;
		}
		this.client = client;
	}

	public RemoteClient CreateRemoteClient()
	{
		return new RemoteClient
		{
			OnConnected = OnRemoteClientConnected,
			OnDisconnected = OnClientDisconnected,
			OnSessionOpened = OnClientSessionOpened,
			OnStateUpdated = OnStateUpdated,
			OnStartedProcessingActions = OnStartedProcessingActions,
			OnFinishedProcessingActions = OnFinishedProcessingActions
		};
	}

	public void SetReplayClient()
	{
		if (settings.GameType != GameType.Multiplayer && settings.GameType != GameType.Competitive)
		{
			settings.GameType = GameType.Multiplayer;
		}
		if (client == null || !client.IsReplay)
		{
			Log.Verbose("{0} Setting up replay client...", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
			client = new ReplayClient
			{
				OnConnected = OnReplayClientConnected,
				OnDisconnected = OnClientDisconnected,
				OnSessionOpened = OnClientSessionOpened,
				OnStateUpdated = OnStateUpdated,
				OnStartedProcessingActions = OnStartedProcessingActions,
				OnFinishedProcessingActions = OnFinishedProcessingActions
			};
		}
	}

	private void OnStartedProcessingActions()
	{
		GameEvents.StartedProcessing();
	}

	private void OnFinishedProcessingActions()
	{
		if (Client.CurrentGameId.HasValue)
		{
			Client.SaveSession(Client.CurrentGameId.Value, showSaveErrorPopup: true);
		}
		if (!WorldIconContainer.HasActiveScoreAnimation())
		{
			ResourceEvents.RefreshWallets(LocalPlayer.Id);
		}
		((MonoBehaviour)this).StartCoroutine(DelayedProcessing());
	}

	private IEnumerator DelayedProcessing()
	{
		yield return null;
		if ((Object)(object)this == (Object)null || GameState == null || !Instance.isLevelLoaded)
		{
			yield break;
		}
		if (GameState.TryGetPlayer(GameState.CurrentPlayer, out var playerState) && GameState.CurrentState != GameState.State.Ended && ((playerState.AutoPlay && Client.HasLocalAI && !Client.ActionManager.IsRecap) || (GameState.CurrentPlayer == LocalPlayer.Id && debugAutoPlayLocalPlayer)))
		{
			if (!CommandTriggerUtils.TryGetTriggerCommand(GameState, out var command))
			{
				command = AI.GetMove(GameState, playerState);
			}
			if (ClientActionManager.CanExecuteCommand(command, GameState))
			{
				Client.SendCommand(command);
			}
		}
		GameEvents.StateUpdated();
		GameEvents.FinishedProcessing();
	}

	private void OnLocalClientConnected()
	{
		Log.Verbose("{0} Local client connected", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
	}

	private void OnRemoteClientConnected()
	{
		Log.Verbose("{0} Remote client connected", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
	}

	private void OnReplayClientConnected()
	{
		Log.Verbose("{0} Replay client connected", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
	}

	private void OnClientSessionOpened()
	{
		Log.Verbose("{0} Session opened", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		if (isLevelLoaded)
		{
			OnGameReady();
		}
	}

	private void OnStateUpdated(StateUpdateReason reason)
	{
		Log.Info("{0} State updated ({1}), refreshing...", new object[2] { "<color=#FFFFFF>[GameManager]</color>", reason });
		if ((Object)(object)MapRenderer.Current != (Object)null)
		{
			MapRenderer.Current.RenderMap(GameState.Map);
		}
		if (reason == StateUpdateReason.Unknown || reason == StateUpdateReason.InvalidCommand || (uint)(reason - 17) <= 1u)
		{
			if (LocalPlayer != null)
			{
				NotificationManager.UpdateIngameAlert();
			}
			if (LocalPlayer != null)
			{
				ResourceEvents.RefreshWallets(LocalPlayer.Id);
			}
			if (Client.IsReady)
			{
				Client.ActionManager.Resume();
			}
		}
		if (Client is HotseatClient hotseatClient)
		{
			hotseatClient.SyncCurrentLocalPlayer();
		}
	}

	private void OnClientDisconnected()
	{
		Log.Info("{0} Disconnected from server", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		GameEvents.SessionEnded();
	}

	public async void OnLevelLoaded()
	{
		Log.Verbose("{0} Level loaded", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		if (Client == null)
		{
			Log.Verbose("{0} Level loaded without client, doing debug start...", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
			settings = new GameSettings
			{
				BaseGameMode = gameMode,
				GameType = debugGameType,
				Difficulty = GameSettings.Difficulties.Normal,
				OpponentCount = aiOpponents + playerOpponents
			};
			switch (settings.GameType)
			{
			case GameType.SinglePlayer:
				if (alwaysCreate || !HasSaveGame)
				{
					await CreateSinglePlayerGame();
				}
				else
				{
					ResumeSingleplayerGame();
				}
				break;
			case GameType.Multiplayer:
				Log.Warning("Multiplayer matches are not supported in debug start, start through the menu instead", Array.Empty<object>());
				break;
			case GameType.PassAndPlay:
			{
				HotseatProfilesState hotseatProfilesState = GetHotseatProfilesState();
				PlayerData playerData = new PlayerData
				{
					defaultName = "Player 1",
					type = PlayerData.Type.Local,
					tribe = StartingTribe,
					knownTribe = true,
					skinType = startingSkin
				};
				playerData.profile = hotseatProfilesState.players[0];
				settings.AddPlayer(playerData);
				for (int i = 0; i < playerOpponents; i++)
				{
					PlayerData playerData2 = new PlayerData
					{
						defaultName = $"Player {i + 2}",
						type = PlayerData.Type.Local,
						tribe = GameStateUtils.GetRandomPickableTribe(VersionManager.GameVersion, settings, null),
						knownTribe = true
					};
					playerData2.profile = hotseatProfilesState.players[i + 1];
					settings.AddPlayer(playerData2);
				}
				for (int j = 0; j < aiOpponents; j++)
				{
					PlayerData playerData3 = new PlayerData
					{
						defaultName = $"Bot {j + 1}",
						type = PlayerData.Type.Bot,
						tribe = GameStateUtils.GetRandomPickableTribe(VersionManager.GameVersion, settings, null),
						knownTribe = true
					};
					playerData3.profile.id = Guid.NewGuid();
					settings.AddPlayer(playerData3);
				}
				if (alwaysCreate)
				{
					CreateHotseatGame();
					break;
				}
				SetHotseatClient();
				Guid[] sessions = client.GetSessions(0L);
				if (sessions != null && sessions.Length != 0)
				{
					await ResumeHotseatGame(sessions[0]);
				}
				else
				{
					CreateHotseatGame();
				}
				break;
			}
			}
			return;
		}
		LevelManager.PreloadAtlases(delegate
		{
			isLevelLoaded = true;
			SetLoadingGame(isLoading: false);
			if (Client.IsReady)
			{
				OnGameReady();
			}
		});
	}

	private void OnGameReady()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Log.Info("{0} Game is ready, in state {1} let's go!", new object[2]
		{
			"<color=#FFFFFF>[GameManager]</color>",
			Client.GameState.CurrentState
		});
		DebugConsole.Hide();
		MapRenderer.Current.RenderMap(Client.GameState.Map);
		CameraController.Instance.CenterOnPosition(LocalPlayer.startTile.ToPosition(), 0f);
		GameEvents.MapLoaded();
		if (Client.GameState.CurrentState == GameState.State.Started || Client.GameState.CurrentState == GameState.State.FinalTurn || Client.GameState.CurrentState == GameState.State.Ended)
		{
			Log.Verbose("{0} Game is resumed", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
			GameEvents.MatchResumed();
		}
		else
		{
			Log.Verbose("{0} Game is started", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		}
		ResourceEvents.RefreshWallets(LocalPlayer.Id);
		GameState.TryGetPlayer(GameState.CurrentPlayer, out var playerState);
		if (Client.GameState.Settings.GameType == GameType.PassAndPlay)
		{
			UIBlackFader.FadeOut(1f);
			PassPlayerReaction.ShowOverlay(playerState, instant: true);
		}
		else if (Client.GetLastSeenCommand() == 0)
		{
			Client.ActionManager.Resume();
			UIBlackFader.FadeOut(1f);
		}
		else
		{
			UIBlackFader.FadeOut(1f, Client.ActionManager.Resume);
		}
		if (Object.op_Implicit((Object)(object)InputSystemCursorOverride.INSTANCE))
		{
			InputSystemCursorOverride.INSTANCE.Hide();
		}
		Instance.SubscribeToGameParticipantStatuses(subscribe: true).WrapErrors();
	}

	public async void OnShowTribePicker(Guid gameId)
	{
		Log.Info("{0} Showing tribe picker", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		SetRemoteClient();
		NetworkUtils.ShowLoader();
		if (await Client.OpenSession(gameId))
		{
			UIManager.Instance.ShowScreen(UIConstants.Screens.TribeSelector);
		}
		NetworkUtils.HideLoader();
	}

	public static void Simulate(ClientActionManager actionManager, int commands, Action onComplete = null)
	{
		((MonoBehaviour)Instance).StartCoroutine(Instance.SimulateRoutine(actionManager, commands, onComplete));
	}

	public IEnumerator SimulateRoutine(ClientActionManager actionManager, int commands, Action onComplete = null)
	{
		Log.Verbose("{0} Simulating {1} commands...", new object[2] { "<color=#FFFFFF>[GameManager]</color>", commands });
		int steps = 100;
		actionManager.IsSimulating = true;
		while (commands + 1 > 0)
		{
			try
			{
				if (!ActionManagerUtils.SimulateAction(actionManager.GameState))
				{
					if (commands > 0)
					{
						actionManager.SimulateCommand();
					}
					commands--;
					int num = steps - 1;
					steps = num;
					if (num <= 0)
					{
						steps = 100;
						goto IL_0115;
					}
				}
			}
			catch (Exception ex)
			{
				actionManager.IsSimulating = false;
				actionManager.EndRecap();
				actionManager.StopProcessing();
				Log.Error("Simulation failed with {0}", new object[1] { ex });
				if (Client.HasTargetState())
				{
					Client.ApplyTargetState();
				}
				else
				{
					Client.ResetGameState(StateUpdateReason.StateReset);
				}
				break;
			}
			continue;
			IL_0115:
			yield return null;
			if (actionManager.isAborting)
			{
				Log.Verbose("Aborting simulation", Array.Empty<object>());
				yield break;
			}
		}
		Log.Verbose("{0} Simulation complete", new object[1] { "<color=#FFFFFF>[GameManager]</color>" });
		actionManager.IsSimulating = false;
		onComplete?.Invoke();
	}

	private void OnDestroy()
	{
		client?.Disconnect();
	}

	public async Task OnApplicationQuit()
	{
		Debug.Log((object)("Application ending after " + Time.time + " seconds"));
		if (PolytopiaBackendAdapter.Instance.IsConnected)
		{
			await PolytopiaBackendAdapter.Instance.LeaveAllMatchmakingGames();
		}
		await PolytopiaBackendAdapter.Instance.CloseConnection();
		FacepunchHelpers.EndSteamSession();
		float num = PolytopiaPlayerPrefs.GetFloat("total_playtime");
		num += Time.unscaledTime;
		PolytopiaPlayerPrefs.SetFloat("total_playtime", num);
	}

	private void OnApplicationPause(bool pause)
	{
		AndroidRotationLockUtil.FixAndroidAutorotate();
		Log.Info("ApplicationPause {0}", new object[1] { pause });
		if (pause)
		{
			AnalyticsManager.SetCrashMetaData("last_pause_frame", Time.frameCount.ToString());
		}
		else
		{
			AnalyticsManager.SetCrashMetaData("last_unpause_frame", Time.frameCount.ToString());
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		AndroidRotationLockUtil.FixAndroidAutorotate();
		Log.Info("ApplicationFocused {0}", new object[1] { focus });
		if (focus)
		{
			AnalyticsManager.SetCrashMetaData("last_focus_frame", Time.frameCount.ToString());
			Log.Info("ApplicationFocused", Array.Empty<object>());
			RefreshOnlineData();
			deepLinkManager.ProcessIntents();
		}
		else
		{
			AnalyticsManager.SetCrashMetaData("last_unfocus_frame", Time.frameCount.ToString());
		}
	}

	private async void RefreshOnlineData()
	{
		await RefreshSteamNotificationStatus();
		if (lobbyManager.HasLoadedCache)
		{
			await lobbyManager.UpdateLobbies();
		}
		if (remoteGameDataManager.HasLoadedCache)
		{
			await remoteGameDataManager.UpdateGameSummaries(ignoreError: true);
		}
		if (tournamentManager.HasLoadedCache)
		{
			await tournamentManager.UpdateTournaments();
		}
		await new WaitForUpdate();
		if ((Object)(object)UIManager.Instance != (Object)null && UIManager.Instance.TryGetCurrentScreen<MultiplayerScreen>(out var screen))
		{
			screen.Reload();
		}
	}

	private async void OnBackendConnectionChanged(ConnectionStatus status)
	{
		Log.Info("Connection changed {0}", new object[1] { status });
		switch (status)
		{
		case ConnectionStatus.Connected:
		case ConnectionStatus.Reconnected:
			deepLinkManager.ProcessIntents();
			AnalyticsManager.SetCrashMetaData("connected_frame", Time.frameCount.ToString());
			if (Client != null && Client.ActionManager != null && Client is RemoteClient)
			{
				Client.GetLastSeenCommand();
				Guid gameId = Client.CurrentGameId.Value;
				ServerResponse<GameViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetGameViewModelByIdAsync(gameId);
				bool flag = Client != null && Client.ActionManager != null && Client is RemoteClient;
				bool flag2 = Client.CurrentGameId.Value == gameId;
				if (serverResponse.Success && flag && flag2)
				{
					GameViewModel data = serverResponse.Data;
					Client.UpdateGameState(data.CurrentGameStateData ?? data.InitialGameStateData, StateUpdateReason.Reconnect);
				}
			}
			await remoteGameDataManager.UpdateGameSummaries(ignoreError: true);
			await AccountManager.GetFriends(forceUpdate: true);
			await SubscribeToGameParticipantStatuses(subscribe: true);
			break;
		case ConnectionStatus.None:
		case ConnectionStatus.Connecting:
		case ConnectionStatus.Reconnecting:
		case ConnectionStatus.ConnectionFailed:
		case ConnectionStatus.Disconnected:
			break;
		}
	}

	private async Task SubscribeToGameParticipantStatuses(bool subscribe)
	{
		_ = 1;
		try
		{
			if (client?.GameState?.PlayerStates != null)
			{
				List<Guid> participants = client.GameState.PlayerStates.Select((PlayerState x) => x.AccountId ?? Guid.Empty).ToList();
				participants.RemoveAll((Guid x) => x == Guid.Empty);
				PlayerData[] source = await AccountManager.GetFriends();
				Guid playerAccountId = AccountManager.PlayerAccountId;
				List<Guid> friendsIds = source.Select((PlayerData x) => x.profile.id).ToList();
				participants.RemoveAll((Guid x) => friendsIds.Contains(x));
				participants.Remove(playerAccountId);
				ServerResponse<PlayersStatusesResponse> serverResponse = await PolytopiaBackendAdapter.Instance.SubscribeToGameParticipantsStatuses(new SubscribeToGameParticipantsStatusesBindingModel
				{
					Subscribe = subscribe,
					GameId = (Client.CurrentGameId ?? Guid.Empty),
					ParticipantsToSubscribe = participants.ToArray(),
					Friends = friendsIds.ToArray()
				});
				if (serverResponse.Success)
				{
					AccountManager.UpdatePlayersStatuses(serverResponse.Data.Statuses);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
	}

	private void Update()
	{
		deepLinkManager.SetShouldAllowProcessing(!isLoadingGame && !UIBlackFader.IsShowing());
		if (deepLinkManager.ShouldProcessQueued())
		{
			deepLinkManager.ProcessQueued();
		}
		AnalyticsManager.SetCrashMetaData("last_frame", Time.frameCount.ToString());
		remoteGameDataManager?.Update();
		client?.Update();
		if (Config.frameRate.IntValue != Application.targetFrameRate)
		{
			Application.targetFrameRate = Config.frameRate.IntValue;
		}
		if (Config.vSync.IntValue != QualitySettings.vSyncCount)
		{
			QualitySettings.vSyncCount = Config.vSync.IntValue;
		}
		if (Client != null && Client.ActionManager != null && GameState != null && Config.showClientState.IntValue >= 1)
		{
			DebugUtils.LogState(GameState, Client, Config.showClientState.IntValue);
		}
		if ((Object)(object)debugTile != (Object)null && Config.showTileState.IntValue >= 1)
		{
			DebugUtils.LogTile(debugTile, GameState);
		}
	}

	public static bool IsPlayerLocal(byte playerId)
	{
		return Client.IsPlayerLocal(playerId);
	}

	public static bool IsPlayerViewing(byte playerId)
	{
		if (Client == null || GameState == null)
		{
			return false;
		}
		if (IsPlayerLocal(playerId))
		{
			return playerId == Client.GetCurrentLocalPlayer().Id;
		}
		return false;
	}

	public static async void TrackFinishedGame(GameState gameState)
	{
		try
		{
			string cachedVUID = TeslaArcadePlatform.GetCachedVUID();
			if (string.IsNullOrWhiteSpace(cachedVUID))
			{
				Log.Error("No cached vuid, won't track this game completion", Array.Empty<object>());
			}
			FinishedGameBindingModel model = new FinishedGameBindingModel
			{
				GameType = gameState.Settings.GameType,
				DeviceId = cachedVUID
			};
			await PolytopiaBackendAdapter.Instance.TrackFinishedGame(model);
		}
		catch (Exception ex)
		{
			Log.Error("Failed to track finished game with exception {0}", new object[1] { ex.ToString() });
		}
	}

	public static async void UpdateFinishedSingleplayerGamesCount()
	{
		if (PolytopiaBackendAdapter.Instance.IsAuthenticated)
		{
			await CacheManager.CacheNumSinglePlayerGames(CacheManager.GetCachedNumSingleplayerGames() + 1);
			await ScoreManager.SyncNumSingleplayerGames(PolytopiaBackendAdapter.Instance.ClientUserData.User.NumGames);
		}
	}

	public static void SendEndGameEvents()
	{
		GameState gameState = GameState;
		string value = "";
		if (gameState.Settings.GameType == GameType.SinglePlayer)
		{
			UpdateFinishedSingleplayerGamesCount();
		}
		PlayerState playerState = gameState.GetPlayersSortedByRank()[0];
		string value2 = ((LocalPlayer.Id == playerState.Id) ? "won" : "lost");
		GetAnalyticsManager().SendEvent("game_finish", new Dictionary<string, object>
		{
			{ "game_id", Client.CurrentGameId },
			{ "result", value2 },
			{ "score", LocalPlayer.score },
			{
				"game_mode",
				gameState.Settings.BaseGameMode
			},
			{ "player_tribe", LocalPlayer.tribe },
			{ "player_score", LocalPlayer.score },
			{
				"winning_tribe",
				playerState.tribe.ToString()
			},
			{ "winning_score", playerState.score },
			{ "turns", gameState.CurrentTurn },
			{ "vehicle_id", value }
		});
		if (LocalPlayer.Id == playerState.Id)
		{
			Log.Learn("extra: won", Array.Empty<object>());
		}
		else
		{
			Log.Learn("extra: lost", Array.Empty<object>());
		}
		Log.Learn("episode_end", Array.Empty<object>());
	}

	public static void MatchEnded(bool localPlayerIsWinner, ScoreDetails scoreDetails, byte winnerId = 0)
	{
		SendEndGameEvents();
		if (GameState.Settings.GameType == GameType.SinglePlayer)
		{
			GameState.CurrentState = GameState.State.Ended;
		}
		ResultScreen.Show(localPlayerIsWinner, scoreDetails, winnerId);
		if (CanBeAuthenticated())
		{
			Client.UploadHighscore();
		}
		Client.EndSession();
	}

	public static bool CanBeAuthenticated()
	{
		if (SystemManager.IsMobile)
		{
			return PolytopiaBackendAdapter.Instance.HasSocialLogin;
		}
		if (SystemManager.IsTesla)
		{
			return PolytopiaBackendAdapter.Instance.HasSocialLogin;
		}
		return true;
	}

	public static bool AreNotificationsEnabled()
	{
		if (SystemManager.IsMobile)
		{
			return NativeHelpers.AreNotificationsEnabled();
		}
		return steamNotificationsEnabled;
	}

	public static async Task RefreshSteamNotificationStatus()
	{
		ServerResponse<SteamNotificationsModels.RequestSteamNotificationsResponse> serverResponse = await PolytopiaBackendAdapter.Instance.RequestSteamNotifications(((object)SteamClient.SteamId/*cast due to .constrained prefix*/).ToString());
		hasSuccessfullyCheckedSteamNotifications = serverResponse.Success;
		if (serverResponse.Success)
		{
			steamNotificationsEnabled = serverResponse.Data.Allow_Notifications;
			SystemEvents.MultiplayerEnabledUpdated();
		}
	}

	public static bool CanCreateMultiplayerGame()
	{
		if (!IsMultiplayerEnabled)
		{
			return false;
		}
		if (!IsNetworkEnabled())
		{
			return false;
		}
		if (!PolytopiaBackendAdapter.Instance.IsConnected)
		{
			return false;
		}
		return true;
	}

	public static PolytopiaSpriteRendererManager GetSpriteRendererManager()
	{
		return instance.spriteRendererManager;
	}

	public static SpriteAtlasManager GetSpriteAtlasManager()
	{
		return instance.spriteAtlasManager;
	}

	public static PurchaseManager GetPurchaseManager()
	{
		return instance.purchaseManager;
	}

	public static AnalyticsManager GetAnalyticsManager()
	{
		return instance.analyticsManager;
	}

	public static MeshCache GetMeshCache()
	{
		return instance.meshCache;
	}

	public static VersioningInfoHolder GetVersioningInfoHolder()
	{
		return instance.versioningInfoHolder;
	}

	public static DeepLinkManager GetDeepLinkManager()
	{
		return instance.deepLinkManager;
	}

	public static RemoteGameDataManager GetRemoteGameDataManager()
	{
		return instance.remoteGameDataManager;
	}

	public static ReplaysManager GetReplaysManager()
	{
		return instance.replaysManager;
	}

	public static LadderManager GetLadderManager()
	{
		return instance.ladderManager;
	}

	public static LobbyManager GetLobbyManager()
	{
		return instance.lobbyManager;
	}

	public static TournamentManager GetTournamentManager()
	{
		return instance.tournamentManager;
	}

	public static LoginManager GetLoginManager()
	{
		return instance.loginManager;
	}

	public static bool IsNetworkEnabled()
	{
		string message;
		return instance.versioningInfoHolder.IsNetworkEnabled(out message);
	}

	public static AnimationCurves GetSharedAnimationCurves()
	{
		return instance.sharedAnimationCurves;
	}

	public static HotseatProfilesState GetHotseatProfilesState()
	{
		if (instance.hotseatProfilesState == null && !TryLoadHotseatProfilesState())
		{
			instance.hotseatProfilesState = HotseatProfilesState.CreateRandom(VersionManager.AvatarVersion, Random.Range(0, int.MaxValue));
		}
		return instance.hotseatProfilesState;
	}

	public static void SaveHotseatProfilesState()
	{
		if (instance.hotseatProfilesState != null)
		{
			DiskSerializationHelpers.ToDisk(instance.hotseatProfilesState, Paths.GetHotseatProfilesStatePath(), VersionManager.GameVersion, out var _);
		}
	}

	private static bool TryLoadHotseatProfilesState()
	{
		if (DiskSerializationHelpers.FromDisk<HotseatProfilesState>(Paths.GetHotseatProfilesStatePath(), out var result))
		{
			instance.hotseatProfilesState = result;
			return true;
		}
		return false;
	}

	private void CmdGameInfo(string[] args)
	{
		DebugConsole.Write("Game information", Array.Empty<object>());
		DebugConsole.Write(" Game Version: {0}", new object[1] { Application.version });
		DebugConsole.Write(" Logic Version: {0}", new object[1] { VersionManager.GameVersion });
		DebugConsole.Write(" Avatar Version: {0}", new object[1] { VersionManager.AvatarVersion });
	}

	private void CmdGetAIAction(string[] args)
	{
		if (Client.GameState.TryGetPlayer(1, out var playerState))
		{
			AI.GetMove(Client.GameState, playerState);
		}
	}

	private void CmdPerformAIAction(string[] args)
	{
		PlayerState playerState;
		if (Client.ActionManager.IsProcessing)
		{
			Log.Verbose("Waiting for queue to empty", Array.Empty<object>());
		}
		else if (Client.GameState.TryGetPlayer(Client.GameState.CurrentPlayer, out playerState))
		{
			playerState.opinions.UpdateOpinions(Client.GameState, playerState);
			CommandBase move = AI.GetMove(Client.GameState, playerState);
			Client.SendCommand(move);
		}
	}

	private void CmdAddCurrency(int amount)
	{
		if (GameState.TryGetPlayer(Client.GameState.CurrentPlayer, out var playerState))
		{
			playerState.Currency += amount;
			ResourceManager.AddResourceOfTypeToResourceBar(playerState.Id, ResourceManager.Type.Currency, amount, playerState.startTile);
		}
	}

	private void CmdUnlockTech(string[] args)
	{
		if (GameState.TryGetPlayer(GameState.CurrentPlayer, out var playerState))
		{
			List<TechData> allTechForTribe = GameState.GameLogicData.GetAllTechForTribe(playerState.tribe);
			for (int i = 0; i < allTechForTribe.Count; i++)
			{
				ActionUtils.LearnTech(GameState, playerState, allTechForTribe[i].type, 0, shouldUseActions: false);
			}
		}
	}

	private void CmdQuit(string[] args)
	{
		DebugConsole.Write("Shutting down...", Array.Empty<object>());
		Application.Quit();
	}

	private void CmdSaveConfig(string[] args)
	{
		if (args == null || args.Length == 0)
		{
			ConfigVar.Save(Paths.GetUserConfigFilePath());
			return;
		}
		DebugConsole.Write("Saving config: {0}", new object[1] { args[0] });
		ConfigVar.Save(args[0]);
	}

	private void CmdLoadConfig(string[] args)
	{
		if (args == null || args.Length == 0)
		{
			DebugConsole.EnqueueCommandNoHistory("exec " + Paths.GetUserConfigFilePath());
			return;
		}
		DebugConsole.Write("Loading config: {0}", new object[1] { args[0] });
		DebugConsole.EnqueueCommandNoHistory("exec " + args[0]);
	}

	private void CmdTimeScale(string[] args)
	{
		float result;
		if (args == null || args.Length == 0)
		{
			DebugConsole.Write("TimeScale: {0}", new object[1] { TimeScale });
		}
		else if (float.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result))
		{
			TimeScale = result;
		}
		else
		{
			DebugConsole.Write("Failed to parse parameter \"{0}\" correct format \"0.5\"", Array.Empty<object>());
		}
	}

	private async void CmdLoginAndConnect(string[] args)
	{
		SteamAuthTicket steamAuthTicket = await FacepunchHelpers.CreateSteamTicket((uint)Config.steamAppId.IntValue);
		if (steamAuthTicket != null)
		{
			await PolytopiaBackendAdapter.Instance.LoginSteam(new SteamLoginBindingModel
			{
				GameVersion = VersionManager.GameVersion,
				DeviceId = SystemInfo.deviceUniqueIdentifier,
				SteamAuthTicket = steamAuthTicket
			});
			await PolytopiaBackendAdapter.Instance.Connect();
		}
	}

	private async void CmdLoginAndConnectLegacyDebug(string[] args)
	{
		await PolytopiaBackendAdapter.Instance.LoginDebugLegacyUser(new LoginFakeBindingModel
		{
			GameVersion = VersionManager.GameVersion,
			DeviceId = SystemInfo.deviceUniqueIdentifier,
			UserName = args[0]
		});
		await PolytopiaBackendAdapter.Instance.Connect();
	}

	private void CmdJoinGame(string[] args)
	{
		if (args.Length == 0)
		{
			alwaysCreate = true;
		}
		else
		{
			alwaysCreate = false;
			CmdGameId = Guid.Parse(args[0]);
		}
		SceneManager.LoadSceneAsync("Level", (LoadSceneMode)0);
	}

	private async void CmdSaveGame(string[] args)
	{
		bool flag = args.Length > 1 && args[1] == "false";
		ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.SaveGame(new SaveGameBindingModel
		{
			GameId = Guid.Parse(args[0]),
			Save = !flag
		});
		if (!serverResponse.Success)
		{
			DebugConsole.Write(serverResponse.ErrorCode.ToString(), Array.Empty<object>());
			DebugConsole.Write(serverResponse.ErrorMessage, Array.Empty<object>());
		}
	}

	private async void CmdGetSavedGames(string[] args)
	{
		ServerResponseList<GameSummaryViewModel> serverResponseList = await PolytopiaBackendAdapter.Instance.GetSavedGames();
		if (!serverResponseList.Success)
		{
			DebugConsole.Write(serverResponseList.ErrorCode.ToString(), Array.Empty<object>());
			DebugConsole.Write(serverResponseList.ErrorMessage, Array.Empty<object>());
			return;
		}
		foreach (GameSummaryViewModel datum in serverResponseList.Data)
		{
			DebugConsole.Write(datum.GameId.ToString(), Array.Empty<object>());
		}
	}

	private async void CmdAddFriend(string[] args)
	{
		await PolytopiaBackendAdapter.Instance.SendFriendRequest(new FriendRequestBindingModel
		{
			FriendUserId = Guid.Parse(args[0])
		});
	}

	private async void CmdRemoveFriend(string[] args)
	{
		await PolytopiaBackendAdapter.Instance.RemoveFriend(new FriendRequestBindingModel
		{
			FriendUserId = Guid.Parse(args[0])
		});
	}

	private async void CmdAcceptFriend(string[] args)
	{
		await PolytopiaBackendAdapter.Instance.AcceptFriendRequest(new FriendRequestBindingModel
		{
			FriendUserId = Guid.Parse(args[0])
		});
	}

	private async void CmdUploadHighscore(string[] args)
	{
		await PolytopiaBackendAdapter.Instance.UploadHighscores(new UploadHighscoresBindingModel
		{
			InitialGameStateData = SerializationHelpers.ToByteArray(GameState, GameState.Version),
			CurrentGameStateData = SerializationHelpers.ToByteArray(GameState, GameState.Version)
		});
	}

	private async void CmdObserveGame(string[] args)
	{
		await PolytopiaBackendAdapter.Instance.GetGameViewModelByIdAsync(Guid.Parse(args[0]));
	}

	private async void CmdBackendStatus(string[] args)
	{
		await PolytopiaBackendAdapter.Instance.LogStatus();
	}

	private async void CmdWhoAmI(string[] args)
	{
		PolytopiaToken obj = (await PolytopiaBackendAdapter.Instance.WhoAmI())?.Data;
		Log.Verbose(obj.ToString(), Array.Empty<object>());
		DebugConsole.Write(obj.ToString(), Array.Empty<object>());
	}

	private async void CmdCreateGameSummary(string[] args)
	{
		if (client?.GameState != null)
		{
			GameStateSummary.FromGameStateByteArray(SerializationHelpers.ToByteArray(client.GameState, client.GameState.Version), out var summary, out var _);
			SerializationHelpers.FromByteArray<GameStateSummary>(SerializationHelpers.ToByteArray(summary, client.GameState.Version), out var _);
		}
		ServerResponseList<GameSummaryViewModel> serverResponseList = await PolytopiaBackendAdapter.Instance.GetGameSummariesByParticipation();
		if (!serverResponseList.Success)
		{
			Log.Error("Failed to get game summaries: {0} ({1})", new object[2] { serverResponseList.ErrorMessage, serverResponseList.ErrorCode });
			return;
		}
		foreach (GameSummaryViewModel datum in serverResponseList.Data)
		{
			SerializationHelpers.FromByteArray<GameStateSummary>(datum.GameSummaryData, out var _);
		}
	}

	private async void CmdResignFromGame(string[] args)
	{
		if (args.Length >= 1)
		{
			Guid gameId = Guid.Parse(args[0]);
			await PolytopiaBackendAdapter.Instance.Resign(new ResignBindingModel
			{
				GameId = gameId
			});
		}
	}

	private async void CmdKickFromGame(string[] args)
	{
		if (args.Length >= 2)
		{
			Guid gameId = Guid.Parse(args[0]);
			Guid userId = Guid.Parse(args[1]);
			ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.Kick(new KickBindingModel
			{
				GameId = gameId,
				UserId = userId
			});
			if (!serverResponse.Success)
			{
				PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
			}
		}
	}

	private void CmdForceSpawnUI(string[] args)
	{
		GameEvents.MatchStart();
	}

	private void CmdSetTribeScore(string[] args)
	{
		if (int.TryParse(args[0], out var result) && uint.TryParse(args[1], out var result2))
		{
			ScoreManager.SetTribeScore((TribeData.Type)result, result2, force: true);
		}
	}

	private void CmdSetTribeRating(string[] args)
	{
		if (int.TryParse(args[0], out var result) && uint.TryParse(args[1], out var result2))
		{
			ScoreManager.SetTribeRating((TribeData.Type)result, result2, force: true);
		}
	}

	private void CmdCreateMultiplayerGame(string[] args)
	{
		if (args == null || args.Length == 0 || args[0] == "help")
		{
			DebugConsole.Write("sv_create parameters:\n  GameType:\n    competitive/multiplayer/matchmaking/hotseat\n  GameMode:\n   glory/might\n  MapPreset:\n    dryland/lakes/continents/archipelago/waterworld\n  MapSize:\n    tiny/normal/large/huge/massive\n  TimeLimit:\n    1/2.../N minutes\n  You can also use integers as index, so mapSize 2 would be large", Array.Empty<object>());
			return;
		}
		if (args == null || args.Length <= 2)
		{
			DebugConsole.Write("Incorrect number of parameters, for instructions use: sv_create help", Array.Empty<object>());
			return;
		}
		GameSettings preliminaryGameSettings = PreliminaryGameSettings;
		GameType gameType = GameType.SinglePlayer;
		switch (args[0].ToLower())
		{
		case "competitive":
		case "0":
			gameType = GameType.Competitive;
			break;
		case "multiplayer":
		case "1":
			gameType = GameType.Multiplayer;
			break;
		case "matchmaking":
		case "2":
			gameType = GameType.Matchmaking;
			break;
		case "passandplay":
		case "hotseat":
		case "3":
			gameType = GameType.PassAndPlay;
			break;
		default:
			DebugConsole.Write("Invalid parameter: {0} for instructions use: sv_create help", new object[1] { args[0] });
			return;
		}
		GameMode gameMode = GameMode.None;
		switch (args[1].ToLower())
		{
		case "glory":
		case "0":
			gameMode = GameMode.Glory;
			break;
		case "might":
		case "1":
			gameMode = GameMode.Might;
			break;
		default:
			DebugConsole.Write("Invalid parameter: {0} for instructions use: sv_create help", new object[1] { args[0] });
			return;
		}
		MapPreset mapPreset = MapPreset.Continents;
		switch (args[2].ToLower())
		{
		case "dryland":
		case "0":
			mapPreset = MapPreset.Dryland;
			break;
		case "lakes":
		case "1":
			mapPreset = MapPreset.Lakes;
			break;
		case "continents":
		case "2":
			mapPreset = MapPreset.Continents;
			break;
		case "archipelago":
		case "3":
			mapPreset = MapPreset.Archipelago;
			break;
		case "waterworld":
		case "4":
			mapPreset = MapPreset.WaterWorld;
			break;
		default:
			DebugConsole.Write("Invalid parameter: {0} for instructions use: sv_create help", new object[1] { args[0] });
			return;
		}
		MapSize mapSize = MapSize.Normal;
		switch (args[3].ToLower())
		{
		case "tiny":
		case "0":
			mapSize = MapSize.Tiny;
			break;
		case "normal":
		case "1":
			mapSize = MapSize.Normal;
			break;
		case "large":
		case "2":
			mapSize = MapSize.Large;
			break;
		case "huge":
		case "3":
			mapSize = MapSize.Huge;
			break;
		case "massive":
		case "4":
			mapSize = MapSize.Massive;
			break;
		default:
			DebugConsole.Write("Invalid parameter: {0} for instructions use: sv_create help", new object[1] { args[0] });
			return;
		}
		if (!int.TryParse(args[4], out var result))
		{
			result = 5;
		}
		preliminaryGameSettings.GameType = gameType;
		preliminaryGameSettings.BaseGameMode = gameMode;
		preliminaryGameSettings.mapPreset = mapPreset;
		preliminaryGameSettings.MapSize = mapSize.ToMapWidth();
		preliminaryGameSettings.GameName = PolyLanguage.MakeGameName();
		if (gameType == GameType.Competitive || gameType == GameType.Multiplayer)
		{
			preliminaryGameSettings.UseDynamicTimers = true;
			preliminaryGameSettings.UseTimeBanks = true;
			preliminaryGameSettings.BaseTimeSeconds = Config.liveModeBaseTime.IntValue;
			preliminaryGameSettings.TimeBonusPerCity = Config.liveModeCityBonus.FloatValue;
			preliminaryGameSettings.TimeBonusPerPopulation = Config.liveModePopulationBonus.FloatValue;
			preliminaryGameSettings.IsAutoSkipEnabled = false;
		}
		else
		{
			preliminaryGameSettings.TimeLimit = result;
		}
		DebugConsole.Write("Created a {0} {1} game on a {2} {3} map with a time limit of {4} minutes", new object[5] { gameType, gameMode, mapSize, mapPreset, result });
		UIManager.Instance.ShowScreen(UIConstants.Screens.PlayerPicker);
	}

	private void CmdCreateMultiplayerGameConfig(string[] args)
	{
		string text = "defaultgameconfig.json";
		if (args != null && args.Length != 0)
		{
			text = $"{args[0]}.json";
		}
		string text2 = null;
		if (PolytopiaFile.Exists(text))
		{
			text2 = PolytopiaFile.ReadAllText(text);
			if (!string.IsNullOrEmpty(text2))
			{
				DebugConsole.Write("Communicating with server...", Array.Empty<object>());
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost:8080/api/cm/create_game");
				httpWebRequest.ContentType = "application/json";
				httpWebRequest.Method = "POST";
				using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
				{
					streamWriter.Write(text2);
				}
				using StreamReader streamReader = new StreamReader(((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream());
				string text3 = streamReader.ReadToEnd();
				if (text3 != null)
				{
					Log.Info("Response: {0}", new object[1] { text3 });
				}
				return;
			}
			Log.Info("Unable to create game, config was empty", Array.Empty<object>());
		}
		else
		{
			Log.Info("Could not load config file: {0}, file does not exist", new object[1] { text });
		}
	}

	private void CmdStartMultiplayerGame(string[] args)
	{
		if (args == null || args.Length == 0)
		{
			DebugConsole.Write("Incorrect number of paramters, sv_start_game expects a game id (Guid)", Array.Empty<object>());
			return;
		}
		DebugConsole.Write("Communicating with server...", Array.Empty<object>());
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost:8080/api/cm/start_game");
		httpWebRequest.ContentType = "application/json";
		httpWebRequest.Method = "POST";
		using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
		{
			string value = "{\"GameId\": " + args[0];
			streamWriter.Write(value);
		}
		using StreamReader streamReader = new StreamReader(((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream());
		string text = streamReader.ReadToEnd();
		if (text != null)
		{
			Log.Info("Response: {0}", new object[1] { text });
		}
	}

	private async void CmdLogCmConnectionStatus(string[] args)
	{
		ServerResponse<ChallengermodeConnectionStatus> serverResponse = await PolytopiaBackendAdapter.Instance.GetChallengermodeConnectionStatus();
		DebugConsole.Write($"connected: {serverResponse.Data?.IsConnected}", Array.Empty<object>());
		DebugConsole.Write($"Challengermode userId: {serverResponse.Data?.ChallengermodeUserId}", Array.Empty<object>());
	}

	private async void CmdConnectChallengermodeOAuth(string[] args)
	{
		ServerResponse<CmStartOAuthFlowResponse> serverResponse = await PolytopiaBackendAdapter.Instance.StartOauthFlow(new CmStartOAuthFlowBindingModel
		{
			Platform = PolytopiaBackendAdapter.GetCurrentPlatform()
		});
		if (serverResponse.Success)
		{
			string url = serverResponse.Data.Url;
			DebugConsole.Write(url, Array.Empty<object>());
			NativeHelpers.OpenURL(url);
		}
	}

	private async void CmdJoinLadder(string[] args)
	{
		await JoinLadderAsync();
	}

	private async void CmdGetTournament(string[] args)
	{
		Guid tournamentId;
		if (args.Length == 0)
		{
			DebugConsole.Write("Incorrect number of paramters, sv_get_tournament expects a tournament id (Guid)", Array.Empty<object>());
		}
		else if (Guid.TryParse(args[0], out tournamentId))
		{
			DebugConsole.Write("Getting tournament...", Array.Empty<object>());
			ServerResponse<TournamentViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetTournament(tournamentId);
			if (serverResponse.Success)
			{
				DebugConsole.Write("Got tournament: {0} ({1})", new object[2]
				{
					serverResponse.Data.Name,
					tournamentId.ToString()
				});
			}
		}
	}

	private async void CmdGetTournamentList(string[] args)
	{
		DebugConsole.Write("Getting tournament list...", Array.Empty<object>());
		ServerResponse<TournamentListViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetTournamentList(new GetTournamentListBindingModel
		{
			Platform = PolytopiaBackendAdapter.GetCurrentPlatform()
		});
		if (serverResponse.Success)
		{
			DebugConsole.Write("Got tournaments. ", new object[1] { serverResponse });
		}
	}

	private async Task JoinLadderAsync()
	{
		ServerResponse<RecurringLadderViewModel> recurringLadderResponse = await PolytopiaBackendAdapter.Instance.GetRecurringLadder();
		if (!recurringLadderResponse.Success)
		{
			DebugConsole.Write("Failed to get retrieve ladder: " + recurringLadderResponse.ErrorMessage, Array.Empty<object>());
			return;
		}
		ServerResponse<ChallengermodeConnectionStatus> serverResponse = await PolytopiaBackendAdapter.Instance.GetChallengermodeConnectionStatus();
		if (!serverResponse.Success)
		{
			DebugConsole.Write("Failed to get connection status: " + serverResponse.ErrorMessage, Array.Empty<object>());
			return;
		}
		if (!serverResponse.Data.IsConnected)
		{
			ServerResponse<CmStartOAuthFlowResponse> serverResponse2 = await PolytopiaBackendAdapter.Instance.StartOauthFlow(new CmStartOAuthFlowBindingModel
			{
				Platform = PolytopiaBackendAdapter.GetCurrentPlatform()
			});
			if (!serverResponse2.Success)
			{
				DebugConsole.Write("Failed to link account: " + serverResponse2.ErrorMessage, Array.Empty<object>());
			}
			NativeHelpers.OpenURL(serverResponse2.Data.Url);
			DebugConsole.Write("Proceeding to link account!", Array.Empty<object>());
			return;
		}
		DebugConsole.Write("(Logged in!)", Array.Empty<object>());
		Guid? ownUserId = serverResponse.Data?.ChallengermodeUserId;
		bool num = recurringLadderResponse.Data.Current.ParticipantUserIds.Any(delegate(Guid participant)
		{
			Guid? guid = ownUserId;
			return participant == guid;
		});
		Guid id = recurringLadderResponse.Data.Current.Id;
		if (!num)
		{
			ServerResponse<ResponseViewModel> serverResponse3 = await PolytopiaBackendAdapter.Instance.JoinLadder(new JoinLadderBindingModel
			{
				LadderId = id
			});
			if (!serverResponse3.Success)
			{
				DebugConsole.Write("Failed to join ladder: " + serverResponse3.ErrorMessage, Array.Empty<object>());
			}
		}
		else
		{
			DebugConsole.Write("Already joined! :)", Array.Empty<object>());
		}
	}

	public static int GetMaxOpponents()
	{
		return GameStateUtils.GetMaxOpponents((GameState != null) ? GameState.Version : VersionManager.GameVersion);
	}
}
