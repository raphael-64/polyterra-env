using System;
using UnityEngine;

public static class Config
{
	public const float COPY_GAME_DATA_DELAY = 3f;

	private static readonly DateTime? EXPIRATION_DATE = new DateTime(2021, 9, 5);

	private const string BACKEND_URI = "https://polytopia-backend-prod.azurewebsites.net/";

	private const string UNITY_LOG_LEVEL = "3";

	private const string DEFAULT_SHOW_FPS = "0";

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar playerName;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar frameRate;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar vSync;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar dragThreshold;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar replayCameraDecoupling;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar fakePlayerId;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar loginFakeDebugLegacy;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar customSavePath;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar unityLogLevel;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar backendUri;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar signalRLogLevel;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar storeTokens;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar steamAppId;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar liveModeBaseTime;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar liveModeCityBonus;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar liveModePopulationBonus;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar showFPS;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar showClientState;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar showTileState;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar showTileOverlay;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar alwaysRenderUnits;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar showReplayState;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar aiEnabled;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar purchaseDebug;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar autoSkip;

	[ConfigVar(/*Could not decode attribute arguments.*/)]
	public static ConfigVar tournamentVersion;

	public static string GetDefaultBackendURI()
	{
		BuildConfig selectedBuildConfig = BuildConfigHelper.GetSelectedBuildConfig();
		if ((Object)(object)selectedBuildConfig != (Object)null)
		{
			return selectedBuildConfig.GetServerURL();
		}
		return "https://polytopia-backend-prod.azurewebsites.net/";
	}

	public static DateTime? GetExpirationDate()
	{
		BuildConfig selectedBuildConfig = BuildConfigHelper.GetSelectedBuildConfig();
		if ((Object)(object)selectedBuildConfig != (Object)null)
		{
			return selectedBuildConfig.GetExpirationDate();
		}
		return EXPIRATION_DATE;
	}
}
