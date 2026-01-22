using UnityEngine;
using UnityEngine.EventSystems;

public class BetaInfoScreen : UIScreenBase
{
	[SerializeField]
	protected UIButtonBase discordButton;

	public void OpenDiscordLink()
	{
		if (PolytopiaBackendAdapter.Instance.IsConnected)
		{
			NativeHelpers.OpenURL("https://discord.gg/polytopia");
		}
		else
		{
			PopupManager.ShowErrorPopup("You must be logged in to our servers in order to get access to the beta discord. E-mail us if you have trouble logging in.");
		}
	}

	protected override void SubscribeButtonsEvents()
	{
		discordButton.OnClicked += DiscordButtonOnClicked;
	}

	protected override void UnsubscribeButtonsEvents()
	{
		discordButton.OnClicked -= DiscordButtonOnClicked;
	}

	private void DiscordButtonOnClicked(int id, BaseEventData eventdata)
	{
		OpenDiscordLink();
	}
}
