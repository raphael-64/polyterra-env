using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;

public class MultiplayerSelectionScreen : UIScreenBase
{
	private const int TOP_PADDING_TAB_ENABLED = 170;

	private const int TOP_PADDING_TAB_DISABLED = 100;

	[Header("Screens")]
	[SerializeField]
	private MultiplayerScreen ongoingScreen;

	[SerializeField]
	private ReplaysScreen replayScreen;

	[SerializeField]
	private LadderScreen ladderScreen;

	[Header("Connections")]
	[SerializeField]
	private UIHorizontalList screenSelectionList;

	[Header("Buttons")]
	[SerializeField]
	protected RectTransform bottomBar;

	[SerializeField]
	protected UIRoundButton newGameButton;

	[SerializeField]
	protected UIRoundButton tournamentsButton;

	[SerializeField]
	protected UIRoundButton friendsButton;

	[SerializeField]
	protected PlayerButton profileButton;

	[SerializeField]
	protected NotificationBadge friendsNotificationBadge;

	private UIConstants.Screens currentScreenType;

	private bool isConnectingBannerShowing;

	public bool IsConnectingBannerShowing => isConnectingBannerShowing;

	public UIRoundButton NewGameButton => newGameButton;

	public UIRoundButton TournamentsButton => tournamentsButton;

	public UIRoundButton FriendsButton => friendsButton;

	public PlayerButton ProfileButton => profileButton;

	public UIHorizontalList ScreenSelectionList => screenSelectionList;

	public void Awake()
	{
		screenSelectionList.SetData(new string[2] { "onlineview.ongoing", "onlineview.replays" }, 1);
		screenSelectionList.IndexSelectedCallback = OnScreenSelectionListChanged;
		((Component)screenSelectionList).gameObject.SetActive(!GameVersionUtils.HideEsport);
		((Component)tournamentsButton).gameObject.SetActive(!GameVersionUtils.HideTournaments);
	}

	private void OnEnable()
	{
		StartSceneBg.Bright = false;
		BackendEvents.OnRefreshFriends += OnRefreshFriendsAsync;
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChangedAsync;
		profileButton.SetAvatarState(AccountManager.AvatarState, keepText: true);
		SubscribeButtonsEvents();
	}

	private void OnDisable()
	{
		NetworkUtils.HideLoader();
		isConnectingBannerShowing = false;
		BackendEvents.OnBackendConnectionChanged -= OnBackendConnectionChangedAsync;
		BackendEvents.OnRefreshFriends -= OnRefreshFriendsAsync;
		UnsubscribeButtonsEvents();
	}

	public override void Show(bool instant = false)
	{
		((Component)this).gameObject.SetActive(true);
		if (GameManager.IsNetworkEnabled())
		{
			_ = GameManager.IsMultiplayerEnabled;
		}
		if (((Component)screenSelectionList).gameObject.activeSelf)
		{
			ongoingScreen.SetScreenPadding(170);
			replayScreen.SetScreenPadding(170);
			ladderScreen.SetScreenPadding(170);
		}
		else
		{
			ongoingScreen.SetScreenPadding(100);
			replayScreen.SetScreenPadding(100);
			ladderScreen.SetScreenPadding(100);
		}
	}

	public override void Hide(bool instant = false)
	{
		if (UIManager.Instance.CurrentScreen != UIConstants.Screens.MultiplayerScreen && UIManager.Instance.CurrentScreen != UIConstants.Screens.ReplaysScreen && UIManager.Instance.CurrentScreen != UIConstants.Screens.LadderScreen)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public async void OnRefreshFriendsAsync(List<PolytopiaFriendViewModel> friends)
	{
		await UpdateFriendsBadgeAsync();
	}

	public bool TryLoadPreviousSettings(GameType gameType)
	{
		string settingsNameFromModes = GameSettingsExtensions.GetSettingsNameFromModes(gameType, GameMode.Custom);
		GameSettings settings;
		bool num = GameSettingsExtensions.TryLoadFromDisk(out settings, settingsNameFromModes);
		if (!num)
		{
			settings.GameType = gameType;
		}
		GameManager.PreliminaryGameSettings = settings;
		return num;
	}

	public async Task UpdateFriendsBadgeAsync()
	{
		int value = await AccountManager.GetFriendRequestCount();
		if (!((Object)(object)this == (Object)null))
		{
			friendsNotificationBadge.Value = value;
		}
	}

	public void OnNewGameCommon()
	{
		UIManager.Instance.loginOverlay.Hide();
		UIManager.Instance.ShowScreen(UIConstants.Screens.GameSetup);
	}

	private async void OnBackendConnectionChangedAsync(ConnectionStatus status)
	{
		await new WaitForUpdate();
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		Log.Verbose($"MultiplayerSelectionScreen :: OnBackendConnectionChanged :: status: {status}", Array.Empty<object>());
		switch (status)
		{
		case ConnectionStatus.None:
		case ConnectionStatus.Connecting:
		case ConnectionStatus.Reconnecting:
		case ConnectionStatus.SocialConnected:
			isConnectingBannerShowing = true;
			NetworkUtils.ShowLoader(Localization.Get("backend.connecting"), 0);
			break;
		case ConnectionStatus.Connected:
		case ConnectionStatus.Reconnected:
			if (isConnectingBannerShowing)
			{
				NetworkUtils.HideLoader();
				isConnectingBannerShowing = false;
			}
			NetworkUtils.ShowLoader(Localization.Get("backend.login.successful", AccountManager.Alias), 0, 2f);
			break;
		case ConnectionStatus.ConnectionFailed:
		case ConnectionStatus.Disconnected:
			NetworkUtils.ShowLoaderError(Localization.Get("backend.connecting.failed"));
			break;
		}
	}

	public void OnScreenSelectionListChanged(int index)
	{
		UIConstants.Screens screens = IndexToScreen(index);
		if (screens != currentScreenType)
		{
			switch (screens)
			{
			case UIConstants.Screens.MultiplayerScreen:
				UIManager.OpenMultiplayerScreen();
				currentScreenType = UIConstants.Screens.MultiplayerScreen;
				break;
			case UIConstants.Screens.ReplaysScreen:
				UIManager.OpenReplaysScreen();
				currentScreenType = UIConstants.Screens.ReplaysScreen;
				break;
			case UIConstants.Screens.LadderScreen:
				UIManager.OpenLadderScreen();
				currentScreenType = UIConstants.Screens.LadderScreen;
				break;
			}
		}
	}

	public void UpdateScreenSelectionListSelectedIndex(UIConstants.Screens screen)
	{
		int num = ScreenToIndex(screen);
		if (num != -1)
		{
			screenSelectionList.SelectItem(num, instant: true);
		}
	}

	public static int ScreenToIndex(UIConstants.Screens screen)
	{
		return screen switch
		{
			UIConstants.Screens.MultiplayerScreen => 0, 
			UIConstants.Screens.ReplaysScreen => 1, 
			_ => -1, 
		};
	}

	public static UIConstants.Screens IndexToScreen(int index)
	{
		return index switch
		{
			0 => UIConstants.Screens.MultiplayerScreen, 
			1 => UIConstants.Screens.ReplaysScreen, 
			_ => UIConstants.Screens.None, 
		};
	}

	private void NewGameButton_OnClicked(int id, BaseEventData eventData = null)
	{
		GameManager.PreliminaryGameSettings.GameType = GameType.Multiplayer;
		TryLoadPreviousSettings((GameType)PlayerPrefs.GetInt("previous_multiplayer_game_type", 1));
		OnNewGameCommon();
	}

	private void TournamentsButton_OnClicked(int id, BaseEventData eventData = null)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!PolytopiaBackendAdapter.Instance.IsConnected || !GameManager.IsMultiplayerEnabled || !GameManager.IsNetworkEnabled())
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("onlineview.tournaments");
			basicPopup.Description = Localization.Get("onlineview.tournamentlist.available");
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup.Show(InputManager.GetInputPosition());
		}
		else
		{
			UIManager.Instance.ShowScreen(UIConstants.Screens.TournamentsScreen);
		}
	}

	private void FriendsButton_OnClicked(int id, BaseEventData eventData = null)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!PolytopiaBackendAdapter.Instance.IsConnected || !GameManager.IsMultiplayerEnabled || !GameManager.IsNetworkEnabled())
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("onlineview.friends");
			basicPopup.Description = Localization.Get("onlineview.friendlist.available");
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup.Show(InputManager.GetInputPosition());
		}
		else
		{
			(UIManager.Instance.GetScreen(UIConstants.Screens.FriendsList) as FriendsList).ListType = FriendsListType.FriendsList;
			UIManager.Instance.ShowScreen(UIConstants.Screens.FriendsList);
		}
	}

	private void ProfileButton_OnClicked(int id, BaseEventData eventData = null)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!PolytopiaBackendAdapter.Instance.IsConnected || !GameManager.IsMultiplayerEnabled || !GameManager.IsNetworkEnabled())
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("onlineview.profile");
			basicPopup.Description = Localization.Get("onlineview.profile.available");
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup.Show(InputManager.GetInputPosition());
		}
		else
		{
			UIManager.Instance.ShowScreen(UIConstants.Screens.ProfileScreen);
		}
	}

	protected override void SubscribeButtonsEvents()
	{
		base.SubscribeButtonsEvents();
		newGameButton.OnClicked += NewGameButton_OnClicked;
		tournamentsButton.OnClicked += TournamentsButton_OnClicked;
		friendsButton.OnClicked += FriendsButton_OnClicked;
		profileButton.OnClicked += ProfileButton_OnClicked;
	}

	protected override void UnsubscribeButtonsEvents()
	{
		base.UnsubscribeButtonsEvents();
		newGameButton.OnClicked -= NewGameButton_OnClicked;
		tournamentsButton.OnClicked -= TournamentsButton_OnClicked;
		friendsButton.OnClicked -= FriendsButton_OnClicked;
		profileButton.OnClicked -= ProfileButton_OnClicked;
	}
}
