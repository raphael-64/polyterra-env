using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UIRoundButton))]
public class TournamentSlotsButtonWrapper : MonoBehaviour
{
	[SerializeField]
	private UIRoundButton roundButton;

	[SerializeField]
	private TextMeshProUGUI roundButtonTextContent;

	private string popupDescription;

	private void Awake()
	{
		roundButton.OnClicked += OnButtonClicked;
	}

	private void OnDestroy()
	{
		roundButton.OnClicked -= OnButtonClicked;
	}

	public void SetData(int slots, string popupDescription)
	{
		this.popupDescription = popupDescription;
		roundButton.text = Localization.Get("onlineview.tournament.info.slots");
		((TMP_Text)roundButtonTextContent).text = slots.ToString();
		roundButton.sprite = null;
	}

	private void OnButtonClicked(int id, BaseEventData eventdata)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup moreGameInfoPopup = PopupManager.GetMoreGameInfoPopup();
		moreGameInfoPopup.Header = Localization.Get("onlineview.tournament.info.players");
		moreGameInfoPopup.Description = popupDescription;
		moreGameInfoPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
		};
		moreGameInfoPopup.Show(InputManager.GetInputPosition());
	}
}
