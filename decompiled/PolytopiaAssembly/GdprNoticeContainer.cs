using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GdprNoticeContainer : UIBasicComponent
{
	[SerializeField]
	protected TextMeshProUGUI header;

	[SerializeField]
	protected TextMeshProUGUI description;

	[SerializeField]
	protected UIButtonBase revokeButton;

	[SerializeField]
	protected GameObject approveButtonContainer;

	[SerializeField]
	protected UIButtonBase approveButton;

	private void OnEnable()
	{
		Refresh();
		SettingsEvents.OnSettingsUpdated += OnSettingsUpdated;
		revokeButton.OnClicked += RevokeButton_OnClicked;
		approveButton.OnClicked += ApproveButton_OnClicked;
	}

	private void OnDisable()
	{
		SettingsEvents.OnSettingsUpdated -= OnSettingsUpdated;
		revokeButton.OnClicked -= RevokeButton_OnClicked;
		approveButton.OnClicked -= ApproveButton_OnClicked;
	}

	private void OnSettingsUpdated(SettingsUtils.SettingsType type)
	{
		if (type == SettingsUtils.SettingsType.PrivacyConsent)
		{
			Refresh(reSelect: true);
		}
	}

	private void Refresh(bool reSelect = false)
	{
		((TMP_Text)header).text = Localization.Get(SettingsUtils.PrivacyConsent ? "consent.enabled" : "consent.disabled");
		string text = Localization.Get(SettingsUtils.PrivacyConsent ? "consent.enabled.info" : "consent.disabled.info");
		text = text.Replace("<a href='{0}'>", "<link=\"PrivacyPolicy\">");
		text = text.Replace("</a>", "</link>");
		((TMP_Text)description).text = text;
		((Component)revokeButton).gameObject.SetActive(SettingsUtils.PrivacyConsent);
		approveButtonContainer.SetActive(!SettingsUtils.PrivacyConsent);
		if (reSelect == ((Component)this).gameObject.activeInHierarchy)
		{
			if (((Component)revokeButton).gameObject.activeSelf)
			{
				UINavigationManager.Select((Selectable)(object)revokeButton.button);
			}
			if (approveButtonContainer.activeSelf)
			{
				UINavigationManager.Select((Selectable)(object)approveButton.button);
			}
		}
	}

	public static void OnDescriptionLinkClickedStatic(string linkId, string linkText)
	{
		if (linkId == "PrivacyPolicy")
		{
			NativeHelpers.OpenURL("https://polytopia.io/privacy-policy/");
		}
	}

	public void OnDescriptionLinkClicked(string linkId, string linkText)
	{
		OnDescriptionLinkClickedStatic(linkId, linkText);
	}

	private void RevokeButton_OnClicked(int id, BaseEventData eventData = null)
	{
		GameManager.GetAnalyticsManager().SendEvent("permissions_click", new Dictionary<string, object> { { "option", "deny" } });
		SettingsUtils.PrivacyConsent = false;
	}

	private void ApproveButton_OnClicked(int id, BaseEventData eventData = null)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		GameManager.GetAnalyticsManager().SendEvent("permissions_click", new Dictionary<string, object> { { "option", "accept" } });
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		SetupGDPRPopup(basicPopup);
		basicPopup.Show(InputManager.GetInputPosition());
	}

	public static void SetupGDPRPopup(BasicPopup popup)
	{
		popup.Header = Localization.Get("consent.approval.title");
		string text = Localization.Get("consent.approval.info");
		text = text.Replace("<a href='{0}'>", "<link=\"PrivacyPolicy\">");
		text = text.Replace("</a>", "</link>");
		popup.Description = text;
		popup.DescriptionLinkCallback = OnDescriptionLinkClickedStatic;
		popup.TopButtonData = new PopupBase.PopupButtonData("consent.deny", PopupBase.PopupButtonData.States.Selected, OnPrivacyPopupDenied);
		popup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("consent.approve", PopupBase.PopupButtonData.States.Selected, OnPrivacyPopupAccepted)
		};
	}

	private static void OnPrivacyPopupAccepted(int id, BaseEventData eventData)
	{
		SettingsUtils.PrivacyConsent = true;
	}

	private static void OnPrivacyPopupDenied(int id, BaseEventData eventData)
	{
		SettingsUtils.PrivacyConsent = false;
	}
}
