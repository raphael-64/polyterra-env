using System;
using System.IO;
using Polytopia.IO;
using UnityEngine;

public static class Paths
{
	public const string SAVE_DIRECTORY = "Data";

	public const string BACKUP_DIRECTORY = "Backup";

	public const string SINGLEPLAYER_SAVE_DIRECTORY = "Singleplayer";

	public const string MULTIPLAYER_SAVE_DIRECTORY = "Multiplayer";

	public const string HOTSEAT_SAVE_DIRECTORY = "Hotseat";

	public const string REPLAY_SAVE_DIRECTORY = "Replay";

	public const string USER_CONFIG_FILE = "user.cfg";

	public const string SERVER_CONFIG_FILE = "server.cfg";

	public const string IMAGE_CACHE_DIRECTORY = "ImageCache";

	public const string CUSTOM_LANGUAGE_CACHE_DIRECTORY = "LanguageCache";

	public const string SETTINGS_DIRECTORY = "Settings";

	public const string CACHE_DIRECTORY = "Cache";

	public static string GetSaveDirectoryPath(string subDirectory)
	{
		string text = Path.Combine(GetUserDirectory(), "Data", subDirectory);
		PolytopiaDirectory.CreateDirectory(text);
		return text;
	}

	public static string GetSpriteAtlesesAddressablesPath()
	{
		string text = "";
		text = "Linux/StandaloneLinux64";
		return Path.Combine(Application.streamingAssetsPath, "aa", text);
	}

	public static string GetUserDirectory()
	{
		string path = AccountManager.PlayerAccountId.ToString();
		if (!string.IsNullOrEmpty(Config.customSavePath.Value))
		{
			path = Config.fakePlayerId.Value;
		}
		string text = Path.Combine(PolytopiaDirectory.PERSISTENT_DATA_PATH, Guid.Empty.ToString());
		string text2 = Path.Combine(PolytopiaDirectory.PERSISTENT_DATA_PATH, path);
		bool num = PolytopiaDirectory.Exists(text2);
		bool flag = PolytopiaDirectory.Exists(text);
		if (!num && flag)
		{
			PolytopiaDirectory.Move(text, text2);
		}
		if (!num && !flag && TryGetLastUsedUserDirectory(out var directory))
		{
			return directory;
		}
		PolytopiaDirectory.CreateDirectory(text2);
		return text2;
	}

	public static bool TryGetLastUsedUserDirectory(out string directory)
	{
		directory = null;
		long num = long.MinValue;
		string[] directories = Directory.GetDirectories(Application.persistentDataPath);
		if (directories != null && directories.Length != 0)
		{
			for (int i = 0; i < directories.Length; i++)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(directories[i]);
				if (Guid.TryParse(directoryInfo.Name, out var result) && directoryInfo.LastWriteTimeUtc.Ticks > num && result != Guid.Empty)
				{
					num = directoryInfo.LastWriteTimeUtc.Ticks;
					directory = directoryInfo.FullName;
				}
			}
		}
		if (string.IsNullOrEmpty(directory))
		{
			return false;
		}
		return true;
	}

	public static string GetCacheDirectoryPath()
	{
		string text = Path.Combine(PolytopiaDirectory.PERSISTENT_DATA_PATH, "Cache");
		PolytopiaDirectory.CreateDirectory(text);
		return text;
	}

	public static string GetUserCacheDirectoryPath()
	{
		string text = Path.Combine(GetUserDirectory(), "Cache");
		PolytopiaDirectory.CreateDirectory(text);
		return text;
	}

	public static string GetImageCacheDirectoryPath()
	{
		string text = Path.Combine(PolytopiaDirectory.IMAGE_CACHE_ROOT, "ImageCache");
		PolytopiaDirectory.CreateDirectory(text);
		return text;
	}

	public static string GetLocalizationCacheDirectoryPath()
	{
		string text = Path.Combine(PolytopiaDirectory.PERSISTENT_DATA_PATH, "LanguageCache");
		PolytopiaDirectory.CreateDirectory(text);
		return text;
	}

	public static string GetGameSettingsDirectoryPath()
	{
		string text = Path.Combine(GetUserDirectory(), "Settings");
		PolytopiaDirectory.CreateDirectory(text);
		return text;
	}

	public static string GetBackupDirectoryPath()
	{
		string text = Path.Combine(PolytopiaDirectory.PERSISTENT_DATA_PATH, "Backup");
		PolytopiaDirectory.CreateDirectory(text);
		return text;
	}

	public static string GetDebugDirectoryPath()
	{
		string text = Path.Combine(PolytopiaDirectory.PERSISTENT_DATA_PATH, "Debug");
		PolytopiaDirectory.CreateDirectory(text);
		return text;
	}

	public static string GetSingleplayerFilePath(string key)
	{
		return Path.Combine(GetSaveDirectoryPath("Singleplayer"), key + ".state");
	}

	public static string GetMultiplayerFilePath(string key)
	{
		return Path.Combine(GetSaveDirectoryPath("Multiplayer"), key + ".state");
	}

	public static string GetHotseatFilePath(string key)
	{
		return Path.Combine(GetSaveDirectoryPath("Hotseat"), key + ".state");
	}

	public static string GetReplayFilePath(string key)
	{
		return Path.Combine(GetSaveDirectoryPath("Replay"), key + ".state");
	}

	public static string GetUserConfigFilePath()
	{
		return "user.cfg";
	}

	public static string GetServerConfigFilePath()
	{
		return "server.cfg";
	}

	public static string GetGameSettingsPath(string settingsName)
	{
		return Path.Combine(GetGameSettingsDirectoryPath(), settingsName + ".state");
	}

	public static string GetHotseatProfilesStatePath()
	{
		return Path.Combine(GetGameSettingsDirectoryPath(), "hotseatprofiles.state");
	}

	public static string GetUserProfileCachePath()
	{
		return Path.Combine(GetCacheDirectoryPath(), "user.json");
	}

	public static string GetVersioningCachePath()
	{
		return Path.Combine(GetCacheDirectoryPath(), "versioning.json");
	}

	public static string GetTribeRatingsCachePath()
	{
		return Path.Combine(GetUserCacheDirectoryPath(), "ratings.json");
	}

	public static string GetNumSingleplayerGamesCachePath()
	{
		return Path.Combine(GetUserCacheDirectoryPath(), "numgames.json");
	}

	public static string GetNewsCachePath()
	{
		return Path.Combine(GetCacheDirectoryPath(), "news.json");
	}

	public static string GetPurchaseCachePatch()
	{
		return Path.Combine(GetCacheDirectoryPath(), "purchases.state");
	}

	public static string GetSelectedBuildConfigResourcePath()
	{
		return "SelectedBuildConfig";
	}
}
