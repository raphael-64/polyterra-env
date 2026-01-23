using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UIRoundButton))]
public class TimerInfoButtonWrapper : MonoBehaviour
{
	[SerializeField]
	private UIRoundButton roundButton;

	[SerializeField]
	private TextMeshProUGUI roundButtonTextValue;

	[SerializeField]
	private TextMeshProUGUI roundButtonTextLabel;

	private int minutes;

	private void Awake()
	{
		roundButton.OnClicked += OnButtonClicked;
	}

	private void OnDestroy()
	{
		roundButton.OnClicked -= OnButtonClicked;
	}

	public void SetData(int minutes)
	{
		this.minutes = minutes;
		roundButton.text = Localization.Get("gamesettings.timer");
		if (minutes == -1)
		{
			((Component)roundButtonTextLabel).gameObject.SetActive(false);
			((TMP_Text)roundButtonTextValue).text = Localization.Get("gamesettings.timer.live");
			return;
		}
		((Component)roundButtonTextLabel).gameObject.SetActive(true);
		TimeSpan time = TimeSpan.FromMinutes(minutes);
		((TMP_Text)roundButtonTextValue).text = LocalizationUtils.GetTimeString(time, out var suffix);
		((TMP_Text)roundButtonTextLabel).text = suffix;
	}

	private void OnButtonClicked(int id, BaseEventData eventdata)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup moreGameInfoPopup = PopupManager.GetMoreGameInfoPopup();
		moreGameInfoPopup.Header = Localization.Get("gamesettings.timer");
		if (minutes == -1)
		{
			moreGameInfoPopup.Description = Localization.Get("gamesettings.livegame.description");
		}
		else
		{
			moreGameInfoPopup.Description = Localization.Get("gamesettings.timer.description");
		}
		moreGameInfoPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
		};
		moreGameInfoPopup.Show(InputManager.GetInputPosition());
	}
}
