using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TribeEndContainer : UIBasicComponent
{
	[SerializeField]
	protected RectTransform randomizerHeader;

	[SerializeField]
	protected RectTransform randomizerButtonHolder;

	[SerializeField]
	protected UITextButton randomizerButton;

	[SerializeField]
	protected UILabelButton restorePurchasesButton;

	[SerializeField]
	protected RectTransform spacer;

	[HideInInspector]
	public Action randomTribeCallback;

	[HideInInspector]
	public Action restorePurchasesCallback;

	public bool ButtonEnabled
	{
		get
		{
			return randomizerButton.ButtonEnabled;
		}
		set
		{
			randomizerButton.ButtonEnabled = value;
		}
	}

	private void Awake()
	{
		randomizerButton.UpdateScrollerOnHighlight = true;
		bool restorePurchasesVisible = false;
		SetRestorePurchasesVisible(restorePurchasesVisible);
	}

	private void OnEnable()
	{
		randomizerButton.OnClicked += OnRandomButtonClicked;
		restorePurchasesButton.OnClicked += OnRestorePurchases;
	}

	private void OnDisable()
	{
		randomizerButton.OnClicked -= OnRandomButtonClicked;
		restorePurchasesButton.OnClicked -= OnRestorePurchases;
	}

	public void SetRestorePurchasesVisible(bool visible)
	{
		((Component)spacer).gameObject.SetActive(visible);
		((Component)restorePurchasesButton).gameObject.SetActive(visible);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
	}

	public void StartWaitingForRestore()
	{
		restorePurchasesButton.Key = "tribepicker.restoring";
		restorePurchasesButton.ButtonEnabled = false;
	}

	public void StopWaitingForRestore()
	{
		restorePurchasesButton.Key = "tribepicker.restore";
		restorePurchasesButton.ButtonEnabled = true;
	}

	public void SetRandomButtonVisible(bool visible)
	{
		((Component)randomizerHeader).gameObject.SetActive(visible);
		((Component)randomizerButtonHolder).gameObject.SetActive(visible);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
	}

	public void OnRandomButtonClicked(int id, BaseEventData eventData = null)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (ButtonEnabled)
		{
			randomTribeCallback?.Invoke();
			AudioManager.PlaySFX(SFXTypes.Magic);
			return;
		}
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("gamesettings.novalidtribes");
		basicPopup.Description = Localization.Get("gamesettings.novalidtribes.info");
		basicPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}

	public void OnRestorePurchases(int id, BaseEventData eventData = null)
	{
		restorePurchasesCallback?.Invoke();
	}
}
