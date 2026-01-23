using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AboutScreen : UIScreenBase
{
	[SerializeField]
	protected RectTransform container;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected ScrollRect scrollRect;

	[SerializeField]
	protected VerticalLayoutGroup layoutGroup;

	[Header("Buttons")]
	[SerializeField]
	protected UIButtonBase instagramButton;

	[SerializeField]
	protected UIButtonBase twitterButton;

	[SerializeField]
	protected UIButtonBase facebookButton;

	[SerializeField]
	protected UIButtonBase discordButton;

	[SerializeField]
	protected UIButtonBase youtubeButton;

	[SerializeField]
	protected UIButtonBase redditButton;

	[Space(10f)]
	[SerializeField]
	protected UIButtonBase googlePlayButton;

	[SerializeField]
	protected UIButtonBase appStoreButton;

	[SerializeField]
	protected UIButtonBase steamButton;

	[Space(10f)]
	[SerializeField]
	protected UIButtonBase homepageButton;

	[SerializeField]
	protected UIButtonBase wikiButton;

	[SerializeField]
	protected UIButtonBase musicButton;

	[SerializeField]
	protected UIButtonBase shopButton;

	[Space(10f)]
	[SerializeField]
	protected UIButtonBase debugButton;

	[Space(10f)]
	[SerializeField]
	protected UIButtonBase privacyButton;

	[SerializeField]
	protected UIButtonBase termsButton;

	[Header("Switch")]
	[SerializeField]
	protected List<GameObject> objectsToHideOnSwitch = new List<GameObject>();

	[Header("iOS")]
	[SerializeField]
	protected List<GameObject> objectsToHideOniOS = new List<GameObject>();

	[Header("Android")]
	[SerializeField]
	protected List<GameObject> objectsToHideOnAndroid = new List<GameObject>();

	public void OnEnable()
	{
		StartSceneBg.Bright = false;
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}

	protected override void SubscribeButtonsEvents()
	{
		base.SubscribeButtonsEvents();
		instagramButton.OnClicked += InstagramButton_OnClicked;
		twitterButton.OnClicked += TwitterButton_OnClicked;
		facebookButton.OnClicked += FacebookButton_OnClicked;
		discordButton.OnClicked += DiscordButton_OnClicked;
		youtubeButton.OnClicked += YoutubeButton_OnClicked;
		redditButton.OnClicked += RedditButton_OnClicked;
		googlePlayButton.OnClicked += GooglePlayButton_OnClicked;
		appStoreButton.OnClicked += AppStoreButton_OnClicked;
		steamButton.OnClicked += SteamButton_OnClicked;
		homepageButton.OnClicked += HomepageButton_OnClicked;
		wikiButton.OnClicked += WikiButton_OnClicked;
		musicButton.OnClicked += MusicButton_OnClicked;
		shopButton.OnClicked += ShopButton_OnClicked;
		debugButton.OnClicked += DebugButton_OnClicked;
		privacyButton.OnClicked += PrivacyButton_OnClicked;
		termsButton.OnClicked += TermsButton_OnClicked;
	}

	protected override void UnsubscribeButtonsEvents()
	{
		base.UnsubscribeButtonsEvents();
		instagramButton.OnClicked -= InstagramButton_OnClicked;
		twitterButton.OnClicked -= TwitterButton_OnClicked;
		facebookButton.OnClicked -= FacebookButton_OnClicked;
		discordButton.OnClicked -= DiscordButton_OnClicked;
		youtubeButton.OnClicked -= YoutubeButton_OnClicked;
		redditButton.OnClicked -= RedditButton_OnClicked;
		googlePlayButton.OnClicked -= GooglePlayButton_OnClicked;
		appStoreButton.OnClicked -= AppStoreButton_OnClicked;
		steamButton.OnClicked -= SteamButton_OnClicked;
		homepageButton.OnClicked -= HomepageButton_OnClicked;
		wikiButton.OnClicked -= WikiButton_OnClicked;
		musicButton.OnClicked -= MusicButton_OnClicked;
		shopButton.OnClicked -= ShopButton_OnClicked;
		debugButton.OnClicked -= DebugButton_OnClicked;
		privacyButton.OnClicked -= PrivacyButton_OnClicked;
		termsButton.OnClicked -= TermsButton_OnClicked;
	}

	private void InstagramButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://www.instagram.com/midjiwan");
	}

	private void TwitterButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://www.twitter.com/midjiwan");
	}

	private void FacebookButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://facebook.com/polytopia");
	}

	private void HomepageButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://polytopia.io/");
	}

	private void DiscordButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://discord.gg/polytopia");
	}

	private void RedditButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://www.reddit.com/r/Polytopia");
	}

	private void YoutubeButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://www.youtube.com/midjiwan");
	}

	private void GooglePlayButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://play.google.com/store/apps/details?id=air.com.midjiwan.polytopia");
	}

	private void AppStoreButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://apps.apple.com/app/the-battle-of-polytopia/id1006393168");
	}

	private void SteamButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://store.steampowered.com/app/874390/The_Battle_of_Polytopia/");
	}

	private void WikiButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://polytopia.wikia.com");
	}

	private void MusicButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://open.spotify.com/artist/5uHFdvBOhBGyOLCaJlTNH3");
	}

	private void ShopButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://shop.midjiwan.com");
	}

	private void DebugButton_OnClicked(int id, BaseEventData eventData = null)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		DebugInfoPopup debugInfoPopup = PopupManager.GetDebugInfoPopup();
		debugInfoPopup.Header = Localization.Get("credits.debuginfo");
		debugInfoPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		debugInfoPopup.Show(InputManager.GetInputPosition());
	}

	private void PrivacyButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://polytopia.io/privacy-policy/", isSwitchOfflineUrl: true);
	}

	private void TermsButton_OnClicked(int id, BaseEventData eventData = null)
	{
		NativeHelpers.OpenURL("https://polytopia.io/terms-of-service/", isSwitchOfflineUrl: true);
	}
}
