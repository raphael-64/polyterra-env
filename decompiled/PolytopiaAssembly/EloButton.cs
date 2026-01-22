using UnityEngine.EventSystems;

public class EloButton : UITextButton
{
	protected override void OnEnable()
	{
		base.OnEnable();
		base.OnClicked += ShowInfo;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.OnClicked -= ShowInfo;
	}

	private void ShowInfo(int id, BaseEventData eventData)
	{
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("mplayerstats.elo");
		basicPopup.Description = Localization.Get("mplayerstats.elo.description");
		basicPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
			{
			})
		};
		basicPopup.Show();
	}
}
