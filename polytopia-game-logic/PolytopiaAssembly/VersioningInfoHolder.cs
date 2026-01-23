using System;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using UnityEngine;

public class VersioningInfoHolder
{
	private bool hasAttemptedVersioningCacheLoad;

	private VersioningViewModel versioningViewModel;

	public Action OnUpdated;

	public VersioningViewModel GetVersioningViewModel()
	{
		if (versioningViewModel == null && !hasAttemptedVersioningCacheLoad)
		{
			hasAttemptedVersioningCacheLoad = true;
			versioningViewModel = CacheManager.GetCachedVersioningViewModel();
		}
		return versioningViewModel;
	}

	private bool IsFeatureEnabled(VersionedFeature feature, out string message)
	{
		message = null;
		VersioningViewModel versioningViewModel = GetVersioningViewModel();
		if (versioningViewModel == null)
		{
			return true;
		}
		foreach (VersionEnabledStatus versionEnabledStatus in versioningViewModel.VersionEnabledStatuses)
		{
			if (versionEnabledStatus.Feature == feature)
			{
				message = versionEnabledStatus.Message;
				return versionEnabledStatus.Enabled;
			}
		}
		return true;
	}

	public string GetSystemMessage()
	{
		return GetVersioningViewModel()?.SystemMessage;
	}

	public bool IsAppEnabled(out string message)
	{
		return IsFeatureEnabled(VersionedFeature.App, out message);
	}

	public bool IsNetworkEnabled(out string message)
	{
		if (!IsFeatureEnabled(VersionedFeature.Network, out message))
		{
			return false;
		}
		if (!IsAppEnabled(out message))
		{
			return false;
		}
		return true;
	}

	public bool IsNewMultiplayerEnabled(out string message)
	{
		if (!IsFeatureEnabled(VersionedFeature.NewMultiplayer, out message))
		{
			return false;
		}
		if (!IsNetworkEnabled(out message))
		{
			return false;
		}
		return true;
	}

	public bool IsNewMatchmakingEnabled(out string message)
	{
		if (!IsFeatureEnabled(VersionedFeature.NewMatchmaking, out message))
		{
			return false;
		}
		if (!IsNewMultiplayerEnabled(out message))
		{
			return false;
		}
		return true;
	}

	public bool IsHighscoreEnabled(out string message)
	{
		if (!IsFeatureEnabled(VersionedFeature.Highscores, out message))
		{
			return false;
		}
		if (!IsNetworkEnabled(out message))
		{
			return false;
		}
		return true;
	}

	public async void LoadVersionInformation()
	{
		_ = 2;
		try
		{
			VersioningBindingModel versioningBindingModel = new VersioningBindingModel();
			versioningBindingModel.SemanticVersion = VersionManager.SemanticVersion.ToString();
			versioningBindingModel.Platform = ((object)Application.platform/*cast due to .constrained prefix*/).ToString();
			versioningBindingModel.SystemLanguage = ((object)Localization.GetSystemLanguage()/*cast due to .constrained prefix*/).ToString();
			versioningBindingModel.Language = Localization.Language.ToString();
			ServerResponse<VersioningViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetVersioning(versioningBindingModel);
			if (serverResponse.Success)
			{
				versioningViewModel = serverResponse.Data;
				await CacheManager.CacheVersioningViewModel(versioningViewModel).ConfigureAwait(continueOnCapturedContext: false);
				Log.Info("[StartupManager] Version information loaded", Array.Empty<object>());
				await new WaitForUpdate();
				OnUpdated?.Invoke();
			}
			else
			{
				Log.Warning("[StartupManager] Version information failed to load: {0} ({1})", new object[2] { serverResponse.ErrorMessage, serverResponse.ErrorCode });
			}
		}
		catch (Exception ex)
		{
			Log.Warning("[StartupManager] Failed to load VersioningViewModel: {0}", new object[1] { ex });
		}
	}
}
