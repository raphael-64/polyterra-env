using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using I2.Loc;
using Newtonsoft.Json;
using Polytopia.IO;
using PolytopiaBackendBase;
using UnityEngine;

public class Localization
{
	public enum Languages
	{
		None,
		Automatic,
		Custom,
		en_US,
		pt_BR,
		ru_RU,
		it_IT,
		fr_FR,
		es_MX,
		de_DE,
		lang_el,
		ja_JP,
		ko_KR
	}

	public static Languages m_language = Languages.en_US;

	private static Dictionary<string, string> m_data;

	protected static bool m_debugMode = false;

	protected static bool m_initialized = false;

	public static Languages Language
	{
		get
		{
			return m_language;
		}
		set
		{
			if (value != m_language)
			{
				m_language = value;
				if (Initialized)
				{
					ReadLocalizationFile();
				}
				LocalizationEvents.LanguageChanged(m_language);
			}
		}
	}

	public static bool Initialized
	{
		get
		{
			return m_initialized;
		}
		protected set
		{
			m_initialized = value;
		}
	}

	public static bool DebugMode
	{
		get
		{
			return m_debugMode;
		}
		set
		{
			m_debugMode = value;
			LocalizationEvents.LocalizationUpdated();
		}
	}

	public static void Init()
	{
		if (!Initialized)
		{
			Language = SettingsUtils.GetLanguageFromIndex(SettingsUtils.Language);
			if (Language == Languages.None)
			{
				Language = Languages.Automatic;
			}
			ReadLocalizationFile();
			InitDebugCommands();
			Initialized = true;
		}
	}

	protected static void ReadLocalizationFile()
	{
		if (Language == Languages.None)
		{
			return;
		}
		if (Language != Languages.Custom)
		{
			if (Language == Languages.Automatic)
			{
				Language = GetAutoLanguage();
			}
			LocalizationManager.CurrentLanguage = GetLanguageName();
		}
		else
		{
			string localizationCacheDirectoryPath = Paths.GetLocalizationCacheDirectoryPath();
			string path = "customLanguage.json";
			string text = Path.Combine(localizationCacheDirectoryPath, path);
			Log.Verbose("Localization :: ReadLocalizationFile :: Read Custom Language File :: filePath: {0}", new object[1] { text });
			try
			{
				if (!PolytopiaDirectory.Exists(localizationCacheDirectoryPath))
				{
					PolytopiaDirectory.CreateDirectory(localizationCacheDirectoryPath);
				}
			}
			catch (IOException ex)
			{
				Log.Error(ex.Message, Array.Empty<object>());
			}
			if (!PolytopiaFile.Exists(text))
			{
				m_language = Languages.Automatic;
				ReadLocalizationFile();
				return;
			}
			m_data = JsonConvert.DeserializeObject<Dictionary<string, string>>(PolytopiaFile.ReadAllText(text));
		}
		LocalizationEvents.LocalizationUpdated();
	}

	private static string GetLanguageName()
	{
		return m_language switch
		{
			Languages.None => "English", 
			Languages.Automatic => "English", 
			Languages.Custom => "English", 
			Languages.en_US => "English", 
			Languages.pt_BR => "Portuguese (Brazil)", 
			Languages.ru_RU => "Russian", 
			Languages.it_IT => "Italian (Italy)", 
			Languages.fr_FR => "French (France)", 
			Languages.es_MX => "Spanish (Mexico)", 
			Languages.de_DE => "German (Germany)", 
			Languages.lang_el => "Elyrion", 
			Languages.ja_JP => "Japanese", 
			Languages.ko_KR => "Korean", 
			_ => "English", 
		};
	}

	public static CultureInfo GetCultureInfo()
	{
		return m_language switch
		{
			Languages.pt_BR => new CultureInfo("pt-BR", useUserOverride: true), 
			Languages.ru_RU => new CultureInfo("ru-RU", useUserOverride: true), 
			Languages.it_IT => new CultureInfo("it-IT", useUserOverride: true), 
			Languages.fr_FR => new CultureInfo("fr-FR", useUserOverride: true), 
			Languages.es_MX => new CultureInfo("es-MX", useUserOverride: true), 
			Languages.de_DE => new CultureInfo("de-DE", useUserOverride: true), 
			_ => new CultureInfo("en-US", useUserOverride: true), 
		};
	}

	public static SystemLanguage GetSystemLanguage()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Application.systemLanguage;
	}

	protected static Languages GetAutoLanguage()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected I4, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		Languages result = Languages.en_US;
		SystemLanguage systemLanguage = GetSystemLanguage();
		if ((int)systemLanguage <= 23)
		{
			if ((int)systemLanguage <= 14)
			{
				if ((int)systemLanguage != 6 && (int)systemLanguage == 14)
				{
					result = Languages.fr_FR;
				}
			}
			else if ((int)systemLanguage != 15)
			{
				switch (systemLanguage - 21)
				{
				case 0:
					result = Languages.it_IT;
					break;
				case 1:
					result = Languages.ja_JP;
					break;
				case 2:
					result = Languages.ko_KR;
					break;
				}
			}
			else
			{
				result = Languages.de_DE;
			}
		}
		else if ((int)systemLanguage <= 30)
		{
			if ((int)systemLanguage != 28)
			{
				if ((int)systemLanguage == 30)
				{
					result = Languages.ru_RU;
				}
			}
			else
			{
				result = Languages.pt_BR;
			}
		}
		else if ((int)systemLanguage != 34)
		{
			if (systemLanguage - 40 <= 1)
			{
			}
		}
		else
		{
			result = Languages.es_MX;
		}
		return result;
	}

	public static string GetErrorMessage(ErrorCode errorCode, string errorMessage = null)
	{
		string text = $"error.code.{(int)errorCode}";
		string text2 = default(string);
		if (m_language == Languages.Custom)
		{
			if (m_data.ContainsKey(text))
			{
				return Get(text, (int)errorCode);
			}
		}
		else if (LocalizationManager.TryGetTranslation(text, ref text2, true, 0, true, false, (GameObject)null, (string)null, true))
		{
			return Get(text, (int)errorCode);
		}
		return Get("error.unknown", errorMessage ?? errorCode.ToString(), (int)errorCode);
	}

	public static string GetErrorMessage<T>(ServerResponse<T> serverResponse) where T : IServerResponseData
	{
		if (serverResponse.Success)
		{
			return null;
		}
		return GetErrorMessage(serverResponse.ErrorCode, serverResponse.ErrorMessage);
	}

	public static string GetErrorMessage<T>(ServerResponseList<T> serverListResponse) where T : IServerResponseData
	{
		if (serverListResponse.Success)
		{
			return null;
		}
		return GetErrorMessage(serverListResponse.ErrorCode, serverListResponse.ErrorMessage);
	}

	public static string Get(string key)
	{
		return Get(key, null);
	}

	public static string Get(string key, object arg0)
	{
		return Get(key, new object[1] { arg0 });
	}

	public static string Get(string key, object arg0, object arg1)
	{
		return Get(key, new object[2] { arg0, arg1 });
	}

	public static string Get(string key, object arg0, object arg1, object arg2)
	{
		return Get(key, new object[3] { arg0, arg1, arg2 });
	}

	public static string Get(string key, object[] args)
	{
		if (string.IsNullOrEmpty(key))
		{
			return string.Empty;
		}
		if (SeasonManager.IsChristmas() && key == "unit.names.bunny")
		{
			key = "unit.names.santa";
		}
		string value = default(string);
		if (m_language == Languages.Custom)
		{
			if (!m_data.TryGetValue(key, out value))
			{
				Log.Warning("Localization manager :: Couldn't find key : |{0}| in the localization data", new object[1] { key });
				if (m_debugMode)
				{
					return $"?{key}?";
				}
				return key;
			}
		}
		else if (!LocalizationManager.TryGetTranslation(key, ref value, true, 0, true, false, (GameObject)null, (string)null, true))
		{
			Log.Warning("Localization manager :: Couldn't find key : |{0}| in the localization data", new object[1] { key });
			if (m_debugMode)
			{
				return $"?{key}?";
			}
			return key;
		}
		string text = value;
		if (args != null && args.Length != 0)
		{
			try
			{
				text = string.Format(value, args);
			}
			catch (Exception ex)
			{
				Log.Error("Failed string format for key [{0}] and value [{1}] and language {2} with error: {3}", new object[4]
				{
					key,
					value,
					m_language,
					ex.ToString()
				});
				return key + "<format error>";
			}
		}
		if (m_debugMode)
		{
			return $"#{text}#";
		}
		return text;
	}

	public static bool HaveKey(string key)
	{
		if (m_language == Languages.Custom)
		{
			return m_data.ContainsKey(key);
		}
		string text = default(string);
		return LocalizationManager.TryGetTranslation(key, ref text, true, 0, true, false, (GameObject)null, (string)null, true);
	}

	public static bool IsAsianLanguage()
	{
		if (m_language != Languages.ja_JP)
		{
			return m_language == Languages.ko_KR;
		}
		return true;
	}

	protected static void InitDebugCommands()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		DebugConsole.AddCommand("localization_language", new CommandDelegate(CmdSetLanguage), "Set the language of the game");
		DebugConsole.AddCommand("localization_toggledebugmode", new CommandDelegate(CmdToggleDebugMode), "Show #Value# for everything correctly localized, ?value? for missing keys and nothing for unlocalized texts");
	}

	protected static void CmdSetLanguage(string[] args)
	{
		if (args == null || args.Length == 0)
		{
			DebugConsole.Write("Language manager set language : no arguments", Array.Empty<object>());
			return;
		}
		string text = args[0];
		switch (text.ToLower())
		{
		case "en":
		case "us":
		case "en_us":
			Language = Languages.en_US;
			break;
		default:
			DebugConsole.Write("Language manager: language {0} was not found", new object[1] { text });
			break;
		}
	}

	protected static void CmdToggleDebugMode(string[] args)
	{
		DebugMode = !DebugMode;
	}
}
