using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;

public class StartupManager : MonoBehaviour
{
	[SerializeField]
	protected GameObject bg;

	private static bool readySent;

	private static ConfiguredTaskAwaitable<int> getNewsCountTask;

	public static bool IsReady { get; private set; }

	public async void Start()
	{
		DebugConsole.Hide();
		bg.SetActive(true);
		await new WaitForEndOfFrame();
		await DoStartup();
	}

	private async Task DoStartup()
	{
		Log.Verbose("[StartupManager] Starting up", Array.Empty<object>());
		if (IsReady)
		{
			RefreshState();
			return;
		}
		IsReady = true;
		RefreshState();
		if (Config.GetExpirationDate().HasValue && Config.GetExpirationDate().Value < DateTime.UtcNow)
		{
			PopupManager.GetBlockingPopup("expirationPopup", "Build Expired", $"This build expired on {Config.GetExpirationDate().Value.ToShortDateString()}, update to a newer version!").Show();
			Log.Error("Build has expired", Array.Empty<object>());
			return;
		}
		if (!SettingsUtils.HasPrivacyConsentKey)
		{
			TriggerGDPRPopup();
		}
		if (!NativeHelpers.IsValidInstall())
		{
			string description = Localization.Get("validation.error.steam");
			PopupManager.GetBlockingPopup("piracyPopup", Localization.Get("validation.error.title"), description).Show();
			Log.Error("Build is pirated", Array.Empty<object>());
		}
		else
		{
			GameManager.GetDeepLinkManager().Initialize();
			await GameManager.GetLoginManager().LoginAsync();
		}
	}

	private void OnQuitGameClicked(int id, BaseEventData eventData)
	{
		Application.Quit();
	}

	public static async Task LoadStartModel()
	{
		try
		{
			DateTime.TryParse(PolytopiaPlayerPrefs.GetString("polytopia_last_seen_news", DateTime.Now.ToString("O")), out var result);
			StartBindingModel bindingModel = new StartBindingModel
			{
				LastSeenNews = result
			};
			ServerResponse<StartViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetStartViewModel(bindingModel);
			if (serverResponse.Success)
			{
				Log.Verbose("[StartupManager] StartModel loaded", Array.Empty<object>());
				GameManager.ActionableGamesCount = serverResponse.Data.ActionableGamesCount;
				GameManager.UnreadNewsCount = serverResponse.Data.UnseenNewsItemCount;
				GameEvents.RefreshBadges();
			}
			else
			{
				Log.Warning("[StartupManager] Failed fetch to StartViewModel: {0}", new object[1] { serverResponse.ErrorMessage });
			}
		}
		catch (Exception ex)
		{
			Log.Warning("[StartupManager] Failed fetch to StartViewModel: {0}", new object[1] { ex });
		}
		RefreshState();
	}

	private static void RefreshState()
	{
		if (!readySent && IsReady)
		{
			GameEvents.StartupDone();
			readySent = true;
		}
	}

	private void TriggerGDPRPopup()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		GameManager.GetAnalyticsManager().SendEvent("permissions_popup_view", new Dictionary<string, object>());
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		GdprNoticeContainer.SetupGDPRPopup(basicPopup);
		basicPopup.Show(InputManager.GetInputPosition());
	}
}
