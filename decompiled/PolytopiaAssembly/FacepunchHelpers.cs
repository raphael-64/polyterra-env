using System;
using System.IO;
using System.Threading.Tasks;
using PolytopiaBackendBase.Auth;
using Steamworks;
using UnityEngine;

public static class FacepunchHelpers
{
	public static bool TryInit(uint appId)
	{
		if (!SteamClient.IsValid)
		{
			try
			{
				Debug.Log((object)"Steamworks: Initializing Steam");
				SteamClient.Init(appId, true);
				return true;
			}
			catch (Exception ex)
			{
				Debug.Log((object)"Could not connect to steam. Is steam running?");
				Debug.Log((object)ex);
				return false;
			}
		}
		return false;
	}

	public static async Task<SteamAuthTicket> CreateSteamTicket(uint appId)
	{
		TryInit(appId);
		if (!SteamClient.IsValid)
		{
			return null;
		}
		Debug.Log((object)"Creating steam session ticket");
		AuthTicket val = await SteamUser.GetAuthSessionTicketAsync(10.0);
		if (val == null)
		{
			return null;
		}
		return new SteamAuthTicket
		{
			Data = val.Data,
			Handle = val.Handle
		};
	}

	public static void EndSteamSession()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (SteamClient.IsLoggedOn)
		{
			Debug.Log((object)"Ending steam session");
			SteamUser.EndAuthSession(SteamClient.SteamId);
		}
		SteamClient.Shutdown();
	}

	public static void LogStatus()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (SteamClient.IsValid && SteamClient.IsLoggedOn)
		{
			Debug.Log((object)("Logged into Steam -> \n Steam Name: " + SteamClient.Name + $" Steam Id: {SteamClient.SteamId}"));
		}
		else
		{
			Debug.Log((object)"Not logged into steam.");
		}
	}

	public static bool IsDLCPurchased(uint appId, uint dlcId)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		TryInit(appId);
		if (!SteamClient.IsValid)
		{
			Log.Warning("Failed to init Steam. Possible reasons: Steam isn't running, couldn't find Steam, AppId is not released, don't own AppId.", Array.Empty<object>());
			return false;
		}
		if (!SteamApps.IsDlcInstalled(AppId.op_Implicit(dlcId)))
		{
			SteamApps.InstallDlc(AppId.op_Implicit(dlcId));
		}
		return SteamApps.IsDlcInstalled(AppId.op_Implicit(dlcId));
	}

	public static bool IsValidInstallation(uint appId)
	{
		TryDeleteSteamAppIdFile();
		if (HasSteamAppIdFile())
		{
			Debug.LogError((object)"Has undeletable steam_appid.txt. Quitting...");
			return false;
		}
		if (SteamClient.RestartAppIfNecessary(appId))
		{
			Application.Quit();
			return false;
		}
		TryInit(appId);
		if (!SteamClient.IsValid)
		{
			return false;
		}
		if (!SteamApps.IsSubscribed)
		{
			return false;
		}
		return true;
	}

	private static bool HasSteamAppIdFile()
	{
		return File.Exists("steam_appid.txt");
	}

	private static bool TryDeleteSteamAppIdFile()
	{
		if (HasSteamAppIdFile())
		{
			try
			{
				File.Delete("steam_appid.txt");
				return true;
			}
			catch (Exception ex)
			{
				Debug.Log((object)ex.Message);
				return false;
			}
		}
		return false;
	}
}
