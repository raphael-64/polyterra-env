using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UIRoundButton))]
public class MoreInfoButtonWrapper : MonoBehaviour
{
	[SerializeField]
	private UIRoundButton roundButton;

	private string displayedInfo = string.Empty;

	public string matchURL = string.Empty;

	private void Awake()
	{
		roundButton.OnClicked += OnButtonClicked;
	}

	private void OnDestroy()
	{
		roundButton.OnClicked -= OnButtonClicked;
	}

	public void SetData(string moreInfoContent)
	{
		displayedInfo = moreInfoContent;
	}

	private void OnButtonClicked(int id, BaseEventData eventData = null)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup moreGameInfoPopup = PopupManager.GetMoreGameInfoPopup();
		moreGameInfoPopup.Header = Localization.Get("onlineview.game.gameinfo");
		moreGameInfoPopup.Description = displayedInfo;
		moreGameInfoPopup.DescriptionLinkCallback = OnDescriptionTextClicked;
		moreGameInfoPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
		};
		moreGameInfoPopup.Show(InputManager.GetInputPosition());
	}

	private void OnDescriptionTextClicked(string linkId, string linkText)
	{
		if (!(linkId == "GAME_INFO_ID"))
		{
			if (linkId == "TOURNAMENT_MATCH_LINK" && matchURL != null)
			{
				NativeHelpers.OpenURL(matchURL);
			}
		}
		else if (linkText != null)
		{
			string arg = (GUIUtility.systemCopyBuffer = linkText.Substring(linkText.LastIndexOf(' ') + 1));
			NotificationManager.Notify(Localization.Get("onlineview.clipboard", arg), Localization.Get("onlineview.clipboard.title"));
		}
	}
}
