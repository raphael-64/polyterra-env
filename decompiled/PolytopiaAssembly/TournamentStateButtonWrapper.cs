using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UIRoundButton))]
public class TournamentStateButtonWrapper : MonoBehaviour
{
	[SerializeField]
	private UIRoundButton roundButton;

	[SerializeField]
	private TextMeshProUGUI roundButtonTextContent;

	private string state = "Unknown";

	private void Awake()
	{
		roundButton.OnClicked += OnButtonClicked;
	}

	private void OnDestroy()
	{
		roundButton.OnClicked -= OnButtonClicked;
	}

	public void SetData(string state)
	{
		this.state = state;
		roundButton.text = Localization.Get("onlineview.tournament.info.state");
		((TMP_Text)roundButtonTextContent).text = state;
		roundButton.sprite = null;
	}

	private void OnButtonClicked(int id, BaseEventData eventdata)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup moreGameInfoPopup = PopupManager.GetMoreGameInfoPopup();
		moreGameInfoPopup.Header = Localization.Get("onlineview.tournament.info.state");
		moreGameInfoPopup.Description = Localization.Get("onlineview.tournament.info.state.description", state);
		moreGameInfoPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
		};
		moreGameInfoPopup.Show(InputManager.GetInputPosition());
	}
}
