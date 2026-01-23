using System;
using System.Collections.Generic;
using Tesla;
using UnityEngine;

public static class NativeHelpers
{
	public static PlatformAudioState debugAudioState;

	public static Action<PlatformAudioState> debugAudioStateCallback;

	public static void OpenSettings()
	{
	}

	public static void InitScrollWheelListener()
	{
	}

	public static bool IsTrackpadInMomentumPhase()
	{
		return false;
	}

	public static bool IsTrackpadInNormalPhase()
	{
		return false;
	}

	public static Vector2Int Screen()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2Int(Screen.width, Screen.height);
	}

	public static float DPI()
	{
		return 0f;
	}

	public static int GetBundleVersion()
	{
		return 0;
	}

	public static void SetWindowTitle(string title)
	{
	}

	public static bool IsValidInstall()
	{
		return FacepunchHelpers.IsValidInstallation((uint)Config.steamAppId.IntValue);
	}

	public static bool AreNotificationsEnabled()
	{
		return false;
	}

	public static void SetIconBadgeNumber(int number)
	{
	}

	public static string GetApplicationSupportPath()
	{
		return null;
	}

	public static string GetLegacyPlayerId()
	{
		return null;
	}

	public static string GetTeamPlayerId()
	{
		return null;
	}

	public static string GetGamePlayerId()
	{
		return null;
	}

	private static PlatformAudioState TeslaAudioStateToPlatformAudioState(AudioState audioState)
	{
		return audioState switch
		{
			AudioState.muteMusic => PlatformAudioState.MuteMusic, 
			AudioState.muteAll => PlatformAudioState.MuteAll, 
			_ => PlatformAudioState.MuteNone, 
		};
	}

	public static void SetDebugPlatformAudioState(PlatformAudioState state)
	{
		debugAudioState = state;
		if (debugAudioStateCallback != null)
		{
			debugAudioStateCallback(state);
		}
	}

	public static PlatformAudioState GetPlatformAudioState()
	{
		return PlatformAudioState.MuteNone;
	}

	public static void SetPlatformAudioStateChangeListener(Action<PlatformAudioState> callback)
	{
		Log.Verbose("No listener for platform audio state on this platform yet", Array.Empty<object>());
	}

	public static void OpenURL(string url, bool isSwitchOfflineUrl = false)
	{
		GameManager.GetAnalyticsManager().SendEvent("outbound_link_click", new Dictionary<string, object> { { "link", url } });
		Application.OpenURL(Uri.EscapeUriString(url));
	}

	public static int GetKeyboardSize()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!TouchScreenKeyboard.isSupported)
		{
			return 0;
		}
		Rect area = TouchScreenKeyboard.area;
		return (int)((Rect)(ref area)).height;
	}
}
