using System;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class GameCenterPlatform
{
	public static async Task<ServerResponse<PolytopiaToken>> LoginAsync()
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		Social.localUser.Authenticate((Action<bool>)delegate(bool success)
		{
			tcs.TrySetResult(success);
		});
		bool flag = await tcs.Task;
		PolytopiaBackendAdapter.Instance.HasSocialLogin = flag;
		BackendEvents.BackendConnectionChanged(ConnectionStatus.SocialConnected);
		return await Login(flag);
	}

	private static async Task<ServerResponse<PolytopiaToken>> Login(bool success)
	{
		if (success)
		{
			LoginGameCenterBindingModel model = new LoginGameCenterBindingModel
			{
				BundleId = Application.identifier,
				GameVersion = VersionManager.GameVersion,
				DeviceId = SystemInfo.deviceUniqueIdentifier,
				GameCenterId = NativeHelpers.GetLegacyPlayerId(),
				TeamPlayerId = NativeHelpers.GetTeamPlayerId(),
				Username = ((IUserProfile)Social.localUser).userName,
				SemanticVersion = VersionManager.SemanticVersion.ToString(),
				LegacyIdClaim = VersionMigration.GetFloxLegacyID()
			};
			return await PolytopiaBackendAdapter.Instance.LoginIos(model);
		}
		return null;
	}
}
