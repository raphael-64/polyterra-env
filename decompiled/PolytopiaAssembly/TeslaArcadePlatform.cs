using System;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using Tesla;
using UnityEngine;

public class TeslaArcadePlatform
{
	private const int VERSION = 1;

	private static Arcade arcade;

	private static Gamer gamer;

	public static string GetCachedVUID()
	{
		return gamer?.vuid;
	}

	public static string GetCachedNotificationToken()
	{
		return gamer?.notificationToken;
	}

	public static async void SendNotificationToken()
	{
		string cachedNotificationToken = GetCachedNotificationToken();
		if (string.IsNullOrEmpty(cachedNotificationToken))
		{
			Log.Error("Tesla notification not set, cannot send notification token", Array.Empty<object>());
			return;
		}
		if (arcade == null)
		{
			Log.Error("Tesla arcade not initialised, cannot send notification token", Array.Empty<object>());
			return;
		}
		NotificationBackendType notificationBackend = (arcade.isDevelopmentBackend ? NotificationBackendType.TeslaDev : NotificationBackendType.Tesla);
		await PolytopiaBackendAdapter.Instance.SetNotificationToken(new NotificationTokenBindingModel
		{
			Token = cachedNotificationToken,
			NotificationBackend = notificationBackend
		});
	}

	public static async Task<ServerResponse<PolytopiaToken>> LoginAsync()
	{
		if (arcade == null)
		{
			try
			{
				arcade = new Arcade(1u);
			}
			catch (Exception ex)
			{
				Log.Error("Failed to init tesla arcade with exception: {0}", new object[1] { ex });
				return null;
			}
		}
		TeslaEnvironment environment;
		try
		{
			environment = (arcade.isDevelopmentBackend ? TeslaEnvironment.Development : TeslaEnvironment.Production);
		}
		catch (Exception ex2)
		{
			Log.Error("Failed to get tesla enviroment with exception: {0}", new object[1] { ex2 });
			return null;
		}
		try
		{
			gamer = await arcade.RequestDefaultGamer(showSignInUI: true);
		}
		catch (InvalidOperationException ex3)
		{
			if (ex3.Message == "TeslaArcade_ResultCode_NotFound")
			{
				Log.Error("There is no tesla gamer to log in", Array.Empty<object>());
				PolytopiaBackendAdapter.Instance.HasSocialLogin = false;
			}
			else
			{
				PolytopiaBackendAdapter.Instance.HasSocialLogin = true;
				BackendEvents.BackendConnectionChanged(ConnectionStatus.SocialConnected);
			}
			Log.Error("Failed to get tesla gamer with exception: {0}", new object[1] { ex3 });
			return null;
		}
		catch (Exception ex4)
		{
			PolytopiaBackendAdapter.Instance.HasSocialLogin = true;
			BackendEvents.BackendConnectionChanged(ConnectionStatus.SocialConnected);
			Log.Error("Failed to get tesla gamer with exception: {0}", new object[1] { ex4 });
			return null;
		}
		if (gamer == null)
		{
			Log.Error("Failed to get tesla gamer", Array.Empty<object>());
			return null;
		}
		PolytopiaBackendAdapter.Instance.HasSocialLogin = true;
		BackendEvents.BackendConnectionChanged(ConnectionStatus.SocialConnected);
		Log.Verbose("Found tesla gamer with name {0} id {1} vehicle id {2} unity device id {3}", new object[4]
		{
			gamer.nickname,
			gamer.uuid,
			gamer.vuid,
			SystemInfo.deviceUniqueIdentifier
		});
		LoginTeslaBindingModel model = new LoginTeslaBindingModel
		{
			BundleId = Application.identifier,
			GameVersion = VersionManager.GameVersion,
			SemanticVersion = VersionManager.SemanticVersion.ToString(),
			DeviceId = gamer.vuid,
			AuthToken = gamer.authToken,
			UserName = gamer.nickname,
			UserId = gamer.uuid,
			Environment = environment
		};
		return await PolytopiaBackendAdapter.Instance.LoginTesla(model);
	}
}
