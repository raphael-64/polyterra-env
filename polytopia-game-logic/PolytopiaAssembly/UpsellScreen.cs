using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpsellScreen : UIScreenBase
{
	[Header("Buttons")]
	[SerializeField]
	protected UIButtonBase upgradeButton;

	public void OnEnable()
	{
		StartSceneBg.Bright = false;
	}

	public override void OnScreenUpdated()
	{
		base.OnScreenUpdated();
	}

	protected override void SubscribeButtonsEvents()
	{
		base.SubscribeButtonsEvents();
		upgradeButton.OnClicked += UpgradeButton_OnClicked;
	}

	protected override void UnsubscribeButtonsEvents()
	{
		base.UnsubscribeButtonsEvents();
		upgradeButton.OnClicked -= UpgradeButton_OnClicked;
	}

	private void UpgradeButton_OnClicked(int id, BaseEventData eventData = null)
	{
		Log.Info("If I was a Switch, I'd open the eShop for you now.", Array.Empty<object>());
	}
}
