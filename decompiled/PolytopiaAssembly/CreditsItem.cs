using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreditsItem : UIBasicComponent
{
	[SerializeField]
	protected TextMeshProUGUI label;

	[SerializeField]
	protected UILabelButton button;

	protected bool haveValidLink;

	protected string link;

	public string Link
	{
		set
		{
			link = value;
			haveValidLink = !string.IsNullOrEmpty(link);
			button.ButtonEnabled = haveValidLink;
			((Selectable)button.button).interactable = haveValidLink;
			((TMP_Text)label).fontStyle = (FontStyles)(haveValidLink ? 4 : 0);
			button.OnClicked -= Button_OnClicked;
			if (haveValidLink)
			{
				button.OnClicked += Button_OnClicked;
			}
		}
	}

	public string Text
	{
		set
		{
			button.text = value;
		}
	}

	private void Button_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL(link);
	}
}
