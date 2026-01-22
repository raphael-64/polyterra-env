using System;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using UnityEngine;

public class LoginManager
{
	private Task loginTask;

	private float lastLoginTime;

	private const float RETRY_TIME = 10f;

	public async void Login(bool silent = true, bool forced = false)
	{
		await LoginAsync(silent, forced);
	}

	public async Task LoginAsync(bool silent = true, bool forced = false)
	{
		if (!GameManager.IsNetworkEnabled())
		{
			return;
		}
		bool flag = Time.realtimeSinceStartup - lastLoginTime >= 10f;
		if (!flag && loginTask != null && !loginTask.IsCompleted)
		{
			await loginTask;
			return;
		}
		switch (PolytopiaBackendAdapter.Instance.ConnectionStatus)
		{
		case ConnectionStatus.Connected:
		case ConnectionStatus.Reconnected:
			return;
		case ConnectionStatus.Connecting:
		case ConnectionStatus.Reconnecting:
		{
			if (flag)
			{
				break;
			}
			TaskCompletionSource<bool> taskCompletion = new TaskCompletionSource<bool>();
			BackendEvents.OnBackendConnectionChangedEvent loginCallback = null;
			loginCallback = delegate(ConnectionStatus status)
			{
				if (status == ConnectionStatus.None || (uint)(status - 3) <= 3u)
				{
					BackendEvents.OnBackendConnectionChanged -= loginCallback;
					taskCompletion.SetResult(result: true);
				}
			};
			BackendEvents.OnBackendConnectionChanged += loginCallback;
			await taskCompletion.Task;
			return;
		}
		}
		bool wasAuthenticated = PolytopiaBackendAdapter.Instance.IsAuthenticated;
		Log.Info("[LoginManager] Logging in...", Array.Empty<object>());
		lastLoginTime = Time.realtimeSinceStartup;
		PolytopiaBackendAdapter.Instance.IsUserTriggeredLogin = forced;
		loginTask = PolytopiaBackendAdapter.Instance.Login(connect: true, silent);
		await loginTask;
		if (!wasAuthenticated && PolytopiaBackendAdapter.Instance.IsAuthenticated)
		{
			PolytopiaUserViewModel user = PolytopiaBackendAdapter.Instance.ClientUserData.User;
			PurchaseManager purchaseManager = GameManager.GetPurchaseManager();
			purchaseManager.UpdateServerUnlockedTribes(user.UnlockedTribes.ToArray());
			purchaseManager.UpdateServerUnlockedSkins(user.UnlockedSkins.ToArray());
			await ScoreManager.SyncNumSingleplayerGames(user.NumGames);
			await StartupManager.LoadStartModel();
			VersionMigration.MigrateTribeRatings();
			await ScoreManager.SyncTribeRatings();
		}
	}
}
