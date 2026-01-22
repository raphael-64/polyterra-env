using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.CrashReportHandler;

public class AnalyticsManager
{
	[RuntimeInitializeOnLoadMethod]
	public void EnforceGDPRConsent()
	{
		bool num = (Analytics.enabled = IsAnalyticsEnabled());
		Analytics.deviceStatsEnabled = num;
		PerformanceReporting.enabled = num;
		if (num)
		{
			InitFirebase();
		}
		EnforceGDPRConsentFirebase();
	}

	public void Init()
	{
		SettingsEvents.OnSettingsUpdated += OnGDPRConsentChanged;
		EnforceGDPRConsent();
		CacheInstallReferrerInfo();
	}

	private Dictionary<string, string> ParseQueryString(string queryString)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] array = queryString.Split('&');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('=');
			if (array2.Length == 2)
			{
				dictionary.Add(array2[0], array2[1]);
			}
		}
		return dictionary;
	}

	private void CacheInstallReferrerInfo()
	{
	}

	private void OnGDPRConsentChanged(SettingsUtils.SettingsType type)
	{
		if (type == SettingsUtils.SettingsType.PrivacyConsent)
		{
			EnforceGDPRConsent();
		}
	}

	public bool IsAnalyticsEnabled()
	{
		return SettingsUtils.PrivacyConsent;
	}

	public void SendEvent(string eventName, Dictionary<string, object> parameters)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (IsAnalyticsEnabled())
		{
			Log.Verbose("sending event {0}", new object[1] { eventName });
			if (ShouldSendFirebaseEvent())
			{
				SendFirebaseEvent(eventName, parameters);
			}
			Analytics.CustomEvent(eventName, (IDictionary<string, object>)parameters);
		}
	}

	public static void SetCrashMetaData(string key, string data)
	{
		CrashReportHandler.SetUserMetadata(key, data);
	}

	private void EnforceGDPRConsentFirebase()
	{
	}

	private void InitFirebase()
	{
	}

	public static bool IsIntValue(object value)
	{
		if (!(value is sbyte) && !(value is byte) && !(value is short) && !(value is ushort) && !(value is int) && !(value is uint) && !(value is long))
		{
			return value is ulong;
		}
		return true;
	}

	public static bool IsFloatValue(object value)
	{
		if (!(value is float) && !(value is double))
		{
			return value is decimal;
		}
		return true;
	}

	public static bool TryGetDoubleValue(object value, out double result)
	{
		if (!IsFloatValue(value))
		{
			result = 0.0;
			return false;
		}
		return double.TryParse(value.ToString(), out result);
	}

	public static bool TryGetLongValue(object value, out long result)
	{
		if (!IsIntValue(value))
		{
			result = 0L;
			return false;
		}
		return long.TryParse(value.ToString(), out result);
	}

	private void SendFirebaseEvent(string eventName, Dictionary<string, object> parameters)
	{
	}

	private bool ShouldSendFirebaseEvent()
	{
		return false;
	}
}
