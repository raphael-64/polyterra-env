using System;

public static class VersionManager
{
	public const string DEFAULT_SEMANTIC_VERSION = "0.0.0.0";

	public const int DEFAULT_VERSION = 0;

	public const int CYMANTI_VERSION = 40;

	public const int BALANCE_1 = 45;

	public const int BALANCE_2 = 50;

	public const int DIPLOMACY_VERSION = 60;

	public const int ESPORT_VERSION = 70;

	public const int DIPLOMACY_VERSION_2 = 80;

	public const int DIPLOMACY_VERSION_3 = 81;

	public const int DIPLOMACY_VERSION_4 = 82;

	public const int DIPLOMACY_VERSION_5 = 83;

	public const int DIPLOMACY_VERSION_6 = 84;

	public const int DIPLOMACY_VERSION_7 = 85;

	public const int TRIBE_SKINS_VERSION = 86;

	public const int TRIBE_SKINS_VERSION_2 = 87;

	public const int LOBBY_VERSION = 90;

	public const int LOBBY_VERSION_2 = 91;

	public const int LOBBY_VERSION_3 = 92;

	public const int LOBBY_VERSION_4 = 93;

	public const int LOBBY_VERSION_5 = 94;

	public const int AVATAR_DATA_VERSION = 1;

	public const int GAME_LOGIC_DATA_VERSION = 14;

	public const int GAME_VERSION = 94;

	public const int MIN_NEW_GAME_VERSION = 51;

	private static Version currentSemanticVersion;

	public static int GameVersion => 94;

	public static int MinNewGameVersion => 51;

	public static int GameLogicDataVersion => 14;

	public static int AvatarDataVersion => 1;

	public static int AvatarVersion => GameVersion;

	public static Version SemanticVersion
	{
		get
		{
			return currentSemanticVersion;
		}
		set
		{
			currentSemanticVersion = value;
			Log.Verbose("[VersionManager] SemanticVersion is {0}", new object[1] { SemanticVersion });
		}
	}

	public static int GetGameLogicDataVersionFromGameVersion(int gameVersion)
	{
		if (gameVersion < 9)
		{
			return 1;
		}
		if (gameVersion < 10)
		{
			return 2;
		}
		if (gameVersion < 11)
		{
			return 3;
		}
		if (gameVersion < 14)
		{
			return 4;
		}
		if (gameVersion < 40)
		{
			return 5;
		}
		if (gameVersion < 43)
		{
			return 6;
		}
		if (gameVersion < 45)
		{
			return 7;
		}
		if (gameVersion < 50)
		{
			return 8;
		}
		if (gameVersion < 60)
		{
			return 9;
		}
		if (gameVersion < 80)
		{
			return 10;
		}
		if (gameVersion < 81)
		{
			return 11;
		}
		if (gameVersion < 83)
		{
			return 12;
		}
		if (gameVersion < 86)
		{
			return 13;
		}
		return GameLogicDataVersion;
	}

	public static bool IsGameVersionSupported(int version)
	{
		if (version >= GetMinimumSupportedVersion())
		{
			return version <= GameVersion;
		}
		return false;
	}

	public static int GetMinimumSupportedVersion()
	{
		return 15;
	}
}
