using System;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AndroidSignInContainer : UIBasicComponent
{
	[SerializeField]
	protected TextMeshProUGUI description;

	[SerializeField]
	protected UIIconButton button;

	private bool IsLoggedIn => PolytopiaBackendAdapter.Instance.HasSocialLogin;

	private void OnEnable()
	{
		button.OnClicked += OnButtonClicked;
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChanged;
		UpdateStatus();
	}

	private void OnDisable()
	{
		button.OnClicked -= OnButtonClicked;
		BackendEvents.OnBackendConnectionChanged -= OnBackendConnectionChanged;
	}

	private void OnBackendConnectionChanged(ConnectionStatus status)
	{
		UpdateStatus();
	}

	private async void OnButtonClicked(int id, BaseEventData eventData = null)
	{
		Task task = (IsLoggedIn ? Logout() : Login());
		try
		{
			await task;
		}
		catch (Exception ex)
		{
			Log.Warning("Login failed: {0}", new object[1] { ex.Message });
		}
	}

	private async Task Login()
	{
		Log.Info("Logging in...", Array.Empty<object>());
		await GameManager.GetLoginManager().LoginAsync(silent: true, forced: true);
		UpdateStatus();
		if (PolytopiaBackendAdapter.Instance.IsAuthenticated)
		{
			NetworkUtils.HideLoader();
		}
	}

	private async Task Logout()
	{
		Log.Info("Logging out...", Array.Empty<object>());
		await PolytopiaBackendAdapter.Instance.LogoutPlatform();
		UpdateStatus();
	}

	private void UpdateStatus()
	{
		Log.Verbose("AndroidSignIn status: {0}", new object[1] { PolytopiaBackendAdapter.Instance.ConnectionStatus });
		if (PolytopiaBackendAdapter.Instance.ConnectionStatus == ConnectionStatus.Connecting || PolytopiaBackendAdapter.Instance.ConnectionStatus == ConnectionStatus.Reconnecting)
		{
			((TMP_Text)description).text = Localization.Get("throne.google.info");
			button.text = Localization.Get("throne.google.in");
			button.ButtonEnabled = false;
		}
		else
		{
			if (IsLoggedIn)
			{
				button.text = Localization.Get("throne.google.out");
				((TMP_Text)description).text = Localization.Get("throne.google.signedin");
			}
			else
			{
				button.text = Localization.Get("throne.google.in");
				((TMP_Text)description).text = Localization.Get("throne.google.info");
			}
			button.ButtonEnabled = true;
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
	}
}
