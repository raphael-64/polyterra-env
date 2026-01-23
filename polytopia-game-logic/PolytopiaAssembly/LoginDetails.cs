using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginDetails : MonoBehaviour
{
	[SerializeField]
	protected RectTransform container;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected MultiplayerRequirementRow requirementRowPrefab;

	[SerializeField]
	protected MultiplayerInfoRow infoRowPrefab;

	protected List<GameObject> loginRows = new List<GameObject>();

	public void ShowLoginDetails()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		if (SystemManager.IsMobile && !GameManager.IsMultiplayerEnabled)
		{
			AddMobileLoginStepRows();
		}
		else if (SystemManager.IsTesla && !GameManager.IsMultiplayerEnabled)
		{
			AddTeslaMultiplayerLockedRows();
		}
		else if (SystemManager.IsSteam && !GameManager.IsMultiplayerEnabled)
		{
			if (!PolytopiaBackendAdapter.Instance.IsAuthenticated)
			{
				AddConnectingRow();
			}
			else
			{
				AddDesktopLockedRows();
			}
		}
		OnScreenSizeChanged(Vector2.zero);
	}

	public void OnEnable()
	{
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChanged;
	}

	private void OnDisable()
	{
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChanged;
	}

	public void OnScreenSizeChanged(Vector2 screenSize)
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}

	private void OnBackendConnectionChanged(ConnectionStatus status)
	{
		ShowLoginDetails();
	}

	private void AddMobileLoginStepRows()
	{
		MultiplayerRequirementRow multiplayerRequirementRow = Object.Instantiate<MultiplayerRequirementRow>(requirementRowPrefab, (Transform)(object)container);
		string statsName = Localization.Get("onlineview.login.ios", Localization.Get((SystemManager.Platform == SystemManager.Platforms.iOS) ? "gameservice.ios" : "gameservice.android"));
		string description = Localization.Get("onlineview.login.ios.info", Localization.Get((SystemManager.Platform == SystemManager.Platforms.iOS) ? "gameservice.ios" : "gameservice.android"));
		multiplayerRequirementRow.SetData(statsName, description, PolytopiaBackendAdapter.Instance.HasSocialLogin, shouldShowButton: true);
		multiplayerRequirementRow.ClickCallback = async delegate
		{
			await GameManager.GetLoginManager().LoginAsync(silent: false, forced: true);
		};
		loginRows.Add(((Component)multiplayerRequirementRow).gameObject);
	}

	private void AddDesktopLockedRows()
	{
		MultiplayerRequirementRow multiplayerRequirementRow = Object.Instantiate<MultiplayerRequirementRow>(requirementRowPrefab, (Transform)(object)container);
		multiplayerRequirementRow.SetData(Localization.Get("onlineview.notifications"), Localization.Get("onlineview.notifications.info"), GameManager.AreNotificationsEnabled(), shouldShowButton: true);
		multiplayerRequirementRow.ClickCallback = delegate
		{
			EnableDesktopPushNotifications();
		};
		loginRows.Add(((Component)multiplayerRequirementRow).gameObject);
	}

	private void AddConnectingRow()
	{
		MultiplayerInfoRow multiplayerInfoRow = Object.Instantiate<MultiplayerInfoRow>(infoRowPrefab, (Transform)(object)container);
		((TMP_Text)multiplayerInfoRow.header).text = Localization.Get("onlineview.nogames.intro", AccountManager.Alias);
		((TMP_Text)multiplayerInfoRow.description).text = Localization.Get("firebaseservice.status.loading");
		loginRows.Add(((Component)multiplayerInfoRow).gameObject);
	}

	private void AddTeslaMultiplayerLockedRows()
	{
		MultiplayerRequirementRow multiplayerRequirementRow = Object.Instantiate<MultiplayerRequirementRow>(requirementRowPrefab, (Transform)(object)container);
		string statsName = Localization.Get("onlineview.login.tesla");
		string description = Localization.Get("onlineview.login.tesla.info");
		multiplayerRequirementRow.SetData(statsName, description, PolytopiaBackendAdapter.Instance.HasSocialLogin, shouldShowButton: false);
		loginRows.Add(((Component)multiplayerRequirementRow).gameObject);
	}

	private async void EnablePushNotifications()
	{
		await Task.FromResult(result: true);
		if (!GameManager.AreNotificationsEnabled())
		{
			NativeHelpers.OpenSettings();
		}
	}

	private async void EnableDesktopPushNotifications()
	{
		if (!GameManager.AreNotificationsEnabled())
		{
			NativeHelpers.OpenURL($"https://steamcommunity.com/profiles/{SteamClient.SteamId}/gamenotificationsettings/");
		}
		else
		{
			await Task.FromResult(result: true);
		}
	}

	public void ClearList()
	{
		foreach (GameObject loginRow in loginRows)
		{
			Object.Destroy((Object)(object)loginRow);
		}
		loginRows.Clear();
	}
}
