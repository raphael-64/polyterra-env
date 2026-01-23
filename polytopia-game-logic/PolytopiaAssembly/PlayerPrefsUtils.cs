using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Polytopia.Data;

public class PlayerPrefsUtils
{
	private static Dictionary<string, float> tribeRatings;

	private static Dictionary<string, int> tribeScores;

	public static void Initialize()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		DebugConsole.AddCommand("clear_playerprefs", new CommandDelegate(CmdClearPlayerPrefs), "Clear All Player Prefs");
	}

	public static bool GetBoolValue(string key, bool defaultValue = true)
	{
		return PolytopiaPlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
	}

	public static void SetBoolValue(string key, bool value)
	{
		PolytopiaPlayerPrefs.SetInt(key, value ? 1 : 0);
	}

	public static void Save()
	{
		PolytopiaPlayerPrefs.Save();
	}

	private static T GetJsonObject<T>(string key) where T : new()
	{
		string text = PolytopiaPlayerPrefs.GetString(key, null);
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(text);
			}
			catch (Exception ex)
			{
				Log.Error("Failed to load scores from string {0} with exception {1}", new object[2]
				{
					text,
					ex.ToString()
				});
			}
		}
		return new T();
	}

	private static Dictionary<string, int> GetTribeScores()
	{
		if (tribeScores == null)
		{
			tribeScores = GetJsonObject<Dictionary<string, int>>("polytopia_tribe_scores");
		}
		return tribeScores;
	}

	public static int GetTribeScore(TribeData.Type tribeType)
	{
		if (GetTribeScores().TryGetValue(tribeType.ToString(), out var value))
		{
			return value;
		}
		return -1;
	}

	private static Dictionary<string, float> GetTribeRatings()
	{
		if (tribeRatings == null)
		{
			tribeRatings = GetJsonObject<Dictionary<string, float>>("polytopia_tribe_ratings");
		}
		return tribeRatings;
	}

	public static float GetTribeRating(TribeData.Type tribeType)
	{
		if (GetTribeRatings().TryGetValue(tribeType.ToString(), out var value))
		{
			return value;
		}
		return -1f;
	}

	public static void Clear()
	{
		PolytopiaPlayerPrefs.DeleteAll();
		Save();
	}

	public static void CmdClearPlayerPrefs(string[] args)
	{
		DebugConsole.Write("Clearing all player prefs", Array.Empty<object>());
		Clear();
	}
}
