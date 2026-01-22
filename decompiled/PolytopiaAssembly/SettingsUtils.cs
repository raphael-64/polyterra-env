using System;

public static class SettingsUtils
{
	public enum SettingsType
	{
		None,
		Volume,
		Language,
		SoundEffects,
		Ambience,
		TribeMusic,
		Suggestions,
		InfoOnBuild,
		ConfirmTurn,
		PrivacyConsent,
		UseCompactUI,
		UseAntiAliasing
	}

	private static float cachedVolume = -1f;

	private static float cachedScaleLimit = -2f;

	private static int cachedLanguage = -1;

	private static bool haveCachedSfx = false;

	private static bool cachedSfx = false;

	private static bool haveCachedAmbience = false;

	private static bool cachedAmbience = false;

	private static bool haveCachedTribeMusic = false;

	private static bool cachedTribeMusic = false;

	private static bool haveCachedSuggestions = false;

	private static bool cachedSuggestions = false;

	private static bool haveCachedInfoOnBuild = false;

	private static bool cachedInfoOnBuild = false;

	private static bool haveCachedConfirmTurn = false;

	private static bool cachedConfirmTurn = false;

	private static bool haveCachedPrivacyConsent = false;

	private static bool cachedPrivacyConsent = false;

	private static bool haveCachedIsFirstTime = false;

	private static bool cachedIsFirstTime = false;

	private static bool? cachedUseCompactUI = null;

	private static bool? cachedUseAntiAlising = null;

	private static string randomSeed = null;

	public static string RandomSeed
	{
		get
		{
			if (randomSeed == null)
			{
				randomSeed = PolytopiaPlayerPrefs.GetString("polytopia_random_seed", null);
			}
			return randomSeed;
		}
		set
		{
			Log.Verbose("setting seed {0}", new object[1] { value });
			randomSeed = value;
			PolytopiaPlayerPrefs.SetString("polytopia_random_seed", value);
			PlayerPrefsUtils.Save();
		}
	}

	public static float Volume
	{
		get
		{
			if (cachedVolume == -1f)
			{
				cachedVolume = PolytopiaPlayerPrefs.GetFloat("volume", 1f);
			}
			return cachedVolume;
		}
		set
		{
			cachedVolume = value;
			PolytopiaPlayerPrefs.SetFloat("volume", value);
			PlayerPrefsUtils.Save();
			AudioManager.VolumeChanged();
			SettingsEvents.SettingsUpdated(SettingsType.Volume);
		}
	}

	public static float ScaleLimit
	{
		get
		{
			if (cachedScaleLimit == -2f)
			{
				cachedScaleLimit = PolytopiaPlayerPrefs.GetFloat("scaleLimit", -1f);
			}
			return cachedScaleLimit;
		}
		set
		{
			cachedScaleLimit = value;
			PolytopiaPlayerPrefs.SetFloat("scaleLimit", value);
			PlayerPrefsUtils.Save();
			UIManager.Instance?.GetCanvasScalerHelper()?.SetDirty();
		}
	}

	public static int Language
	{
		get
		{
			if (cachedLanguage == -1)
			{
				cachedLanguage = PolytopiaPlayerPrefs.GetInt("language");
			}
			return cachedLanguage;
		}
		set
		{
			cachedLanguage = value;
			PolytopiaPlayerPrefs.SetInt("language", value);
			PlayerPrefsUtils.Save();
			Localization.Language = GetLanguageFromIndex(value);
			SettingsEvents.SettingsUpdated(SettingsType.Language);
		}
	}

	public static bool SoundEffects
	{
		get
		{
			if (!haveCachedSfx)
			{
				cachedSfx = PlayerPrefsUtils.GetBoolValue("soundEffects");
			}
			haveCachedSfx = true;
			return cachedSfx;
		}
		set
		{
			cachedSfx = value;
			haveCachedSfx = true;
			PlayerPrefsUtils.SetBoolValue("soundEffects", value);
			PlayerPrefsUtils.Save();
			AudioManager.VolumeChanged();
			SettingsEvents.SettingsUpdated(SettingsType.SoundEffects);
		}
	}

	public static bool Ambience
	{
		get
		{
			if (!haveCachedAmbience)
			{
				cachedAmbience = PlayerPrefsUtils.GetBoolValue("ambience");
			}
			haveCachedAmbience = true;
			return cachedAmbience;
		}
		set
		{
			cachedAmbience = value;
			haveCachedAmbience = true;
			PlayerPrefsUtils.SetBoolValue("ambience", value);
			PlayerPrefsUtils.Save();
			AudioManager.VolumeChanged();
			SettingsEvents.SettingsUpdated(SettingsType.Ambience);
		}
	}

	public static bool TribeMusic
	{
		get
		{
			if (!haveCachedTribeMusic)
			{
				cachedTribeMusic = PlayerPrefsUtils.GetBoolValue("tribeMusic");
			}
			haveCachedTribeMusic = true;
			return cachedTribeMusic;
		}
		set
		{
			cachedTribeMusic = value;
			haveCachedTribeMusic = true;
			PlayerPrefsUtils.SetBoolValue("tribeMusic", value);
			PlayerPrefsUtils.Save();
			AudioManager.VolumeChanged();
			SettingsEvents.SettingsUpdated(SettingsType.TribeMusic);
		}
	}

	public static bool Suggestions
	{
		get
		{
			if (!haveCachedSuggestions)
			{
				cachedSuggestions = PlayerPrefsUtils.GetBoolValue("suggestions");
			}
			haveCachedSuggestions = true;
			return cachedSuggestions;
		}
		set
		{
			cachedSuggestions = value;
			haveCachedSuggestions = true;
			PlayerPrefsUtils.SetBoolValue("suggestions", value);
			PlayerPrefsUtils.Save();
			SettingsEvents.SettingsUpdated(SettingsType.Suggestions);
		}
	}

	public static bool InfoOnBuild
	{
		get
		{
			if (!haveCachedInfoOnBuild)
			{
				cachedInfoOnBuild = PlayerPrefsUtils.GetBoolValue("infoOnBuild");
			}
			haveCachedInfoOnBuild = true;
			return cachedInfoOnBuild;
		}
		set
		{
			cachedInfoOnBuild = value;
			haveCachedInfoOnBuild = true;
			PlayerPrefsUtils.SetBoolValue("infoOnBuild", value);
			PlayerPrefsUtils.Save();
			SettingsEvents.SettingsUpdated(SettingsType.InfoOnBuild);
		}
	}

	public static bool ConfirmTurn
	{
		get
		{
			if (!haveCachedConfirmTurn)
			{
				cachedConfirmTurn = PlayerPrefsUtils.GetBoolValue("confirmTurn", defaultValue: false);
			}
			haveCachedConfirmTurn = true;
			return cachedConfirmTurn;
		}
		set
		{
			cachedConfirmTurn = value;
			haveCachedConfirmTurn = true;
			PlayerPrefsUtils.SetBoolValue("confirmTurn", value);
			PlayerPrefsUtils.Save();
			SettingsEvents.SettingsUpdated(SettingsType.ConfirmTurn);
		}
	}

	public static bool UseAntiAliasing
	{
		get
		{
			if (!cachedUseAntiAlising.HasValue)
			{
				bool defaultValue = false;
				cachedUseAntiAlising = PlayerPrefsUtils.GetBoolValue("useAntiAliasing", defaultValue);
			}
			return cachedUseAntiAlising.Value;
		}
		set
		{
			bool? flag = cachedUseAntiAlising;
			cachedUseAntiAlising = value;
			PlayerPrefsUtils.SetBoolValue("useAntiAliasing", value);
			PlayerPrefsUtils.Save();
			if (!flag.HasValue || flag.Value != value)
			{
				SettingsEvents.SettingsUpdated(SettingsType.UseAntiAliasing);
			}
		}
	}

	public static bool UseCompactUI
	{
		get
		{
			if (!SystemManager.ShouldUseMobileUI())
			{
				return false;
			}
			return true;
		}
		set
		{
			bool? flag = cachedUseCompactUI;
			cachedUseCompactUI = value;
			PlayerPrefsUtils.SetBoolValue("useCompactUI", value);
			PlayerPrefsUtils.Save();
			if (!flag.HasValue || flag.Value != value)
			{
				SettingsEvents.SettingsUpdated(SettingsType.UseCompactUI);
			}
		}
	}

	public static bool PrivacyConsent
	{
		get
		{
			if (!haveCachedPrivacyConsent)
			{
				cachedPrivacyConsent = PlayerPrefsUtils.GetBoolValue("privacyConsent", defaultValue: false);
			}
			haveCachedPrivacyConsent = true;
			return cachedPrivacyConsent;
		}
		set
		{
			cachedPrivacyConsent = value;
			haveCachedPrivacyConsent = true;
			PlayerPrefsUtils.SetBoolValue("privacyConsent", value);
			PlayerPrefsUtils.Save();
			SettingsEvents.SettingsUpdated(SettingsType.PrivacyConsent);
		}
	}

	public static bool HasPrivacyConsentKey => PolytopiaPlayerPrefs.HasKey("privacyConsent");

	public static bool IsFirstTime
	{
		get
		{
			if (!haveCachedIsFirstTime)
			{
				cachedIsFirstTime = PlayerPrefsUtils.GetBoolValue("isFirstTime");
			}
			haveCachedIsFirstTime = true;
			return cachedIsFirstTime;
		}
		set
		{
			cachedIsFirstTime = value;
			haveCachedIsFirstTime = true;
			PlayerPrefsUtils.SetBoolValue("isFirstTime", value);
			PlayerPrefsUtils.Save();
		}
	}

	public static string LastSeenSystemMessage
	{
		get
		{
			return PolytopiaPlayerPrefs.GetString("polytopia_last_seen_system_message", null);
		}
		set
		{
			PolytopiaPlayerPrefs.SetString("polytopia_last_seen_system_message", value);
			PlayerPrefsUtils.Save();
		}
	}

	public static bool HasRandomSeed()
	{
		return PolytopiaPlayerPrefs.HasKey("polytopia_random_seed");
	}

	public static string GetOrCreateRandomSeed()
	{
		if (!HasRandomSeed())
		{
			RandomSeed = Guid.NewGuid().ToString();
		}
		return RandomSeed;
	}

	public static Localization.Languages GetLanguageFromIndex(int idx)
	{
		Localization.Languages result = Localization.Language;
		if (idx >= 0)
		{
			result = (Localization.Languages)idx;
		}
		return result;
	}
}
