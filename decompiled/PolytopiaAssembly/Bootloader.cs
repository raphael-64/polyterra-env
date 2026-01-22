using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootloader : MonoBehaviour
{
	public const string EDITOR_PLAY_SCENE_KEY = "Editor.PlayScene";

	[SerializeField]
	private GameManager gameManager;

	[SerializeField]
	protected LoggingLevel unityLogLevel = (LoggingLevel)3;

	[SerializeField]
	protected LoggingLevel debugConsoleLogLevel = (LoggingLevel)3;

	[SerializeField]
	protected LoggingLevel stackTraceLevel = (LoggingLevel)3;

	private string sceneToLoad;

	private DebugConsole debugConsole;

	private DebugOverlay debugOverlay;

	private UnityLogger logger;

	private void OnEnable()
	{
		Localization.Init();
	}

	private void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		NativeHelpers.InitScrollWheelListener();
		logger = new UnityLogger
		{
			LogLevel = unityLogLevel,
			StackTraceLevel = stackTraceLevel
		};
		Application.logMessageReceived += new LogCallback(logger.LogCallback);
		Log.AddLogger((Logger)(object)logger);
		GraphicsUtils.SetMSAAEnabled(SettingsUtils.UseAntiAliasing);
	}

	private void Start()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		Log.Info("[Bootloader] Initializing...", Array.Empty<object>());
		SetupOverlay();
		SetupConsole();
		Log.Info("[Bootloader] Loading ConfigVars...", Array.Empty<object>());
		ConfigVar.Initialize();
		Config.backendUri.Value = Config.GetDefaultBackendURI();
		DebugConsole.EnqueueCommandNoHistory("exec -s \"" + Paths.GetServerConfigFilePath() + "\"");
		DebugConsole.EnqueueCommandNoHistory("exec -s \"" + Paths.GetUserConfigFilePath() + "\"");
		DebugConsole.EnqueueCommandNoHistory("info");
		DebugConsole.TickUpdate(true);
		SetupBackendAdapter();
		LoggingLevel val = (LoggingLevel)Config.unityLogLevel.IntValue;
		Log.Info("[Bootloader] Logging level: {0}", new object[1] { val });
		((Logger)logger).LogLevel = val;
		VerifyVersion();
		SetupGame();
	}

	private void VerifyVersion()
	{
		Log.Info("[Bootloader] Verifying version...", Array.Empty<object>());
		Log.Verbose("[VersionManager] GameVersion is {0}", new object[1] { VersionManager.GameVersion });
		int gameVersion = VersionManager.GameVersion;
		if (VersionMigration.MigrateGameVersion(PolytopiaPlayerPrefs.GetInt("GameVersion"), gameVersion))
		{
			PolytopiaPlayerPrefs.SetInt("GameVersion", gameVersion);
			PolytopiaPlayerPrefs.Save();
		}
		else
		{
			Log.Error("GameVersion migration failed", Array.Empty<object>());
		}
		string version = Application.version;
		VersionManager.SemanticVersion = new Version(version);
		if (VersionMigration.MigrateSemanticVersion(PolytopiaPlayerPrefs.GetString("SemanticVersion", "0.0.0.0"), version))
		{
			PolytopiaPlayerPrefs.SetString("SemanticVersion", version);
			PolytopiaPlayerPrefs.Save();
		}
		else
		{
			Log.Error("SemanticVersion migration failed", Array.Empty<object>());
		}
	}

	private void SetupOverlay()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Log.Info("[Bootloader] Loading debug overlay...", Array.Empty<object>());
		debugOverlay = Object.Instantiate<DebugOverlay>(Resources.Load<DebugOverlay>("DebugOverlay"));
		((Object)debugOverlay).name = "[Debug Overlay]";
		DebugOverlay obj = debugOverlay;
		Vector2Int val = NativeHelpers.Screen();
		int x = ((Vector2Int)(ref val)).x;
		val = NativeHelpers.Screen();
		obj.Init(x, ((Vector2Int)(ref val)).y);
		Object.DontDestroyOnLoad((Object)(object)debugOverlay);
	}

	private void SetupConsole()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		if (!Object.op_Implicit((Object)(object)debugOverlay))
		{
			Log.Error("[Bootloader] Trying to load DebugConsole before loading the Debug Overlay", Array.Empty<object>());
			return;
		}
		Log.Info("[Bootloader] Loading debug console...", Array.Empty<object>());
		debugConsole = Object.Instantiate<DebugConsole>(Resources.Load<DebugConsole>("DebugConsole"));
		((Object)debugConsole).name = "[Debug Console]";
		debugConsole.Init();
		debugConsole.Resize(debugOverlay.width, debugOverlay.height - 4);
		Log.AddLogger((Logger)new Logger
		{
			LogLevel = debugConsoleLogLevel
		});
		Object.DontDestroyOnLoad((Object)(object)debugConsole);
	}

	private void SetupBackendAdapter()
	{
		Log.Info("[Bootloader] Setting up BackendAdapter", Array.Empty<object>());
		_ = PolytopiaBackendAdapter.Instance;
	}

	private void SetupGame()
	{
		if (string.IsNullOrEmpty(sceneToLoad))
		{
			sceneToLoad = SceneUtility.GetScenePathByBuildIndex(1);
		}
		Log.Info("[Bootloader] Loading GameManager...", Array.Empty<object>());
		gameManager = Object.Instantiate<GameManager>(gameManager);
		((Object)gameManager).name = "[GameManager]";
		gameManager.Initialize();
		GameManager.GetSpriteAtlasManager().PreloadSpriteAtlases(new List<string> { "StartScene", "UI" }, 10, delegate(bool success)
		{
			if (!success)
			{
				throw new Exception($"Failed to load atlases used in start game. Is first time {SettingsUtils.IsFirstTime}");
			}
			LoadScene(sceneToLoad);
		});
	}

	private void LoadScene(string scene)
	{
		SceneManager.LoadScene(scene, (LoadSceneMode)0);
	}
}
