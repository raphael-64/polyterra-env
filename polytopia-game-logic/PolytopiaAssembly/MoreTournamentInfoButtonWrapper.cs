using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UIRoundButton))]
public class MoreTournamentInfoButtonWrapper : MonoBehaviour
{
	[SerializeField]
	private UIRoundButton roundButton;

	private string overviewUrl = string.Empty;

	private string displayedInfo = string.Empty;

	private void Awake()
	{
		roundButton.OnClicked += OnButtonClicked;
	}

	private void OnDestroy()
	{
		roundButton.OnClicked -= OnButtonClicked;
	}

	public void SetData(string overviewUrl, string moreInfoContent)
	{
		this.overviewUrl = overviewUrl;
		displayedInfo = moreInfoContent;
	}

	private void OnButtonClicked(int id, BaseEventData eventData = null)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup moreTournamentInfoPopup = PopupManager.GetMoreTournamentInfoPopup();
		moreTournamentInfoPopup.Header = Localization.Get("onlineview.tournament.info.header");
		moreTournamentInfoPopup.Description = displayedInfo;
		moreTournamentInfoPopup.DescriptionLinkCallback = OnDescriptionTextClicked;
		moreTournamentInfoPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
		};
		moreTournamentInfoPopup.Show(InputManager.GetInputPosition());
	}

	private void OnDescriptionTextClicked(string linkId, string linkText)
	{
		if (!(linkId == "TOURNAMENT_CONTACT_INFO"))
		{
			if (linkId == "TOURNAMENT_LINK" && overviewUrl != null)
			{
				NativeHelpers.OpenURL(overviewUrl);
			}
		}
		else if (linkText != null)
		{
			NativeHelpers.OpenURL(linkText);
		}
	}
}
