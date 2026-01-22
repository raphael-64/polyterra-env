using System.Collections.Generic;
using Polytopia.IO;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartScreen : UIScreenBase
{
	[Header("Start Screen")]
	public UITextButton newGameButton;

	public UITextButton resumeButton;

	public UITextButton multiplayerButton;

	public UIRoundButton newsButton;

	public UIRoundButton betaButton;

	public UIRoundButton settingsButton;

	public UIRoundButton highscoreButton;

	public UIRoundButton throneRoomButton;

	public UIRoundButton aboutButton;

	[Header("Notification Badges")]
	public NotificationBadge newsBadge;

	public NotificationBadge multiplayerBadge;

	[SerializeField]
	private VerticalLayoutGroup startButtonLayoutGroup;

	[SerializeField]
	private UITextButton startButtonPrefab;

	private static bool hasShownNetworkDisabledInfo;

	private bool isResumeButtonHeld;

	private bool isSettingsButtonHeld;

	private float buttonPressTime;

	private void OnEnable()
	{
		GameManager.GetVersioningInfoHolder().OnUpdated = RefreshVersionInfo;
		UpdateNewsButtonVisibility();
		LocalizationEvents.OnLocalizationUpdated += UpdateNewsButtonVisibility;
		GameEvents.OnRefreshBadges += OnRefreshBadges;
		StartSceneBg.Bright = true;
	}

	private void OnDisable()
	{
		GameManager.GetVersioningInfoHolder().OnUpdated = null;
		GameEvents.OnRefreshBadges -= OnRefreshBadges;
		LocalizationEvents.OnLocalizationUpdated -= UpdateNewsButtonVisibility;
	}

	private void Start()
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		GameManager.GetSpriteAtlasManager().PreloadSpriteAtlases(SpriteAtlasManager.MENU_ATLASES, 10, delegate
		{
		});
		List<UITextButton> list = new List<UITextButton>();
		list.Add(newGameButton);
		bool hasSaveGame = GameManager.HasSaveGame;
		if (hasSaveGame)
		{
			list.Add(resumeButton);
		}
		((Component)resumeButton).gameObject.SetActive(hasSaveGame);
		bool flag = SystemManager.ShouldShowMultiplayerButton();
		if (flag)
		{
			list.Add(multiplayerButton);
		}
		((Component)multiplayerButton).gameObject.SetActive(flag);
		if (SystemManager.ShouldShowQuitButton() && UIManager.Instance.type == UIManager.Type.StartMenu)
		{
			UITextButton uITextButton = CreateQuitButton();
			UIUtils.SetSelectOnDown((Selectable)(object)uITextButton.button, (Selectable)(object)settingsButton.button);
			UIUtils.SetSelectOnUp((Selectable)(object)uITextButton.button, (Selectable)(object)list[list.Count - 1].button);
			list.Add(uITextButton);
		}
		for (int num = 0; num < list.Count; num++)
		{
			UITextButton uITextButton2 = list[num];
			Navigation navigation = ((Selectable)uITextButton2.button).navigation;
			if (num > 0)
			{
				((Navigation)(ref navigation)).selectOnUp = (Selectable)(object)list[num - 1].button;
			}
			if (num < list.Count - 1)
			{
				((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)list[num + 1].button;
			}
			((Selectable)uITextButton2.button).navigation = navigation;
		}
		Button button = list[list.Count - 1].button;
		SetSelectOnUpNavigation(settingsButton.button, button);
		SetSelectOnUpNavigation(highscoreButton.button, button);
		SetSelectOnUpNavigation(throneRoomButton.button, button);
		SetSelectOnUpNavigation(aboutButton.button, button);
		AudioManager.GetAudioSource(AudioManager.AudioSourceTypes.ThemeMusic).Play();
		bool flag2 = false;
		if (flag2)
		{
			SetSelectOnUpNavigation(newGameButton.button, betaButton.button);
			SetSelectOnDownNavigation(newsButton.button, betaButton.button);
		}
		((Component)betaButton).gameObject.SetActive(flag2);
		DebugConsole.Hide();
	}

	private void Update()
	{
		if (isResumeButtonHeld)
		{
			if (Time.time - buttonPressTime >= 3f)
			{
				isResumeButtonHeld = false;
				OnCopyGameDataToClipboard();
			}
		}
		else if (isSettingsButtonHeld && Time.time - buttonPressTime >= 3f)
		{
			isSettingsButtonHeld = false;
			OnCopyBackupDataToClipboard();
		}
	}

	private void SetSelectOnUpNavigation(Button button, Button nextButton)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = ((Selectable)button).navigation;
		((Navigation)(ref navigation)).selectOnUp = (Selectable)(object)nextButton;
		((Selectable)button).navigation = navigation;
	}

	private void SetSelectOnDownNavigation(Button button, Button nextButton)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = ((Selectable)button).navigation;
		((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)nextButton;
		((Selectable)button).navigation = navigation;
	}

	private UITextButton CreateQuitButton()
	{
		UITextButton uITextButton = Object.Instantiate<UITextButton>(startButtonPrefab, ((Component)startButtonLayoutGroup).transform);
		uITextButton.OnClicked += OnQuitGameClicked;
		uITextButton.Key = "startmenu.quit";
		return uITextButton;
	}

	private void OnQuitGameClicked(int id, BaseEventData eventData)
	{
		Application.Quit();
	}

	public override void Show(bool instant = false)
	{
		base.Show(instant);
		GameManager.PreliminaryGameSettings.HardReset();
		RefreshNewsBadge();
		RefreshMultiplayerBadge();
		RefreshVersionInfo();
	}

	public override void OnButtonUp(InputManager.Buttons button)
	{
		if (button == InputManager.Buttons.Cancel && !PopupManager.PopupShowing)
		{
			UINavigationManager.Select((Selectable)(object)settingsButton.button);
			ShowSettings();
		}
	}

	private void RefreshVersionInfo()
	{
		VersioningInfoHolder versioningInfoHolder = GameManager.GetVersioningInfoHolder();
		bool flag = PopupManager.IsPopupShowing<BasicPopup>("appversioning");
		string message2;
		if (!versioningInfoHolder.IsAppEnabled(out var message) && !flag)
		{
			string description = (string.IsNullOrEmpty(message) ? Localization.Get("versioning.updateapp") : message);
			PopupManager.GetBlockingPopup("appversioning", Localization.Get("versioning.updateapp.title"), description).Show();
		}
		else if (!versioningInfoHolder.IsNetworkEnabled(out message2) && !hasShownNetworkDisabledInfo)
		{
			hasShownNetworkDisabledInfo = true;
			string description2 = (string.IsNullOrEmpty(message2) ? Localization.Get("versioning.updatenetwork") : message2);
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("versioning.updatenetwork.title");
			basicPopup.Description = description2;
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup.Show();
		}
		string systemMessage = versioningInfoHolder.GetSystemMessage();
		if (!string.IsNullOrEmpty(systemMessage) && systemMessage != SettingsUtils.LastSeenSystemMessage)
		{
			SettingsUtils.LastSeenSystemMessage = systemMessage;
			BasicPopup basicPopup2 = PopupManager.GetBasicPopup();
			basicPopup2.Header = Localization.Get("versioning.systemmessage.title");
			basicPopup2.Description = systemMessage;
			basicPopup2.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup2.Show();
		}
	}

	private void OnRefreshBadges()
	{
		RefreshNewsBadge();
		RefreshMultiplayerBadge();
	}

	private void UpdateNewsButtonVisibility()
	{
		bool flag = Localization.IsAsianLanguage();
		((Component)newsButton).gameObject.SetActive(!flag);
	}

	public void RefreshNewsBadge()
	{
		newsBadge.Value = GameManager.UnreadNewsCount;
	}

	public void RefreshMultiplayerBadge()
	{
		multiplayerBadge.Value = GameManager.ActionableGamesCount;
	}

	private void NewGame()
	{
		GameManager.GetAnalyticsManager().SendEvent("new_game_click", new Dictionary<string, object>());
		GameManager.GetSpriteAtlasManager().PreloadSpriteAtlases(SpriteAtlasManager.MENU_ATLASES, 10, delegate
		{
			GameManager.PreliminaryGameSettings.GameType = GameType.SinglePlayer;
			if (SettingsUtils.IsFirstTime)
			{
				GameModeScreen.StartTutorial();
			}
			else
			{
				UIManager.Instance.ShowScreen(UIConstants.Screens.GameMode);
			}
		});
	}

	private void ResumeGame()
	{
		GameManager.Instance.ResumeSingleplayerGame();
		GameManager.GetAnalyticsManager().SendEvent("game_resume", new Dictionary<string, object> { 
		{
			"game_id",
			GameManager.Client.CurrentGameId
		} });
	}

	private void OnCopyGameDataToClipboard()
	{
		string saveDirectoryPath = Paths.GetSaveDirectoryPath("Singleplayer");
		if (PolytopiaDirectory.Exists(saveDirectoryPath))
		{
			DebugUtils.CopyFilesToClipBoard(new List<string>(PolytopiaDirectory.GetFiles(saveDirectoryPath, "*.state")));
		}
	}

	private void ShowMultiplayer()
	{
		UIManager.OpenMultiplayerScreen();
		GameManager.GetAnalyticsManager().SendEvent("multiplayer_button_click", new Dictionary<string, object>());
	}

	private void ShowSettings()
	{
		UIManager.Instance.ShowScreen(UIConstants.Screens.Settings);
	}

	private void OnCopyBackupDataToClipboard()
	{
		DebugUtils.CopyFilesToClipBoard(VersionMigration.GetAllBackupLegacySaveFilePaths());
	}

	private void ShowHighscore()
	{
		UIManager.Instance.ShowScreen(UIConstants.Screens.Highscore);
	}

	private void ShowThroneRoom()
	{
		UIManager.Instance.ShowScreen(UIConstants.Screens.ThroneRoom);
	}

	private void ShowAbout()
	{
		UIManager.Instance.ShowScreen(UIConstants.Screens.About);
	}

	private void ShowNews()
	{
		UIManager.Instance.ShowScreen(UIConstants.Screens.News);
	}

	private void ShowBetaInfo()
	{
		UIManager.Instance.ShowScreen(UIConstants.Screens.BetaInfo);
	}

	protected override void SubscribeButtonsEvents()
	{
		newGameButton.OnClicked += OnNewGameButtonClick;
		resumeButton.OnDown += OnResumeButtonDown;
		resumeButton.OnClicked += OnResumeButtonClick;
		resumeButton.OnExit += OnResumeButtonExit;
		multiplayerButton.OnClicked += OnMultiplayerButtonClick;
		settingsButton.OnDown += OnSettingsButtonDown;
		settingsButton.OnClicked += OnSettingsButtonClick;
		settingsButton.OnExit += OnSettingsButtonExit;
		aboutButton.OnClicked += OnAboutButtonButtonClick;
		highscoreButton.OnClicked += OnShowHighscoreButtonClick;
		throneRoomButton.OnClicked += OnThroneRoomButtonButtonClick;
		betaButton.OnClicked += OnBetaButtonClick;
		newsButton.OnClicked += OnNewsButtonClick;
	}

	protected override void UnsubscribeButtonsEvents()
	{
		newGameButton.OnClicked -= OnNewGameButtonClick;
		resumeButton.OnDown -= OnResumeButtonDown;
		resumeButton.OnClicked -= OnResumeButtonClick;
		resumeButton.OnExit -= OnResumeButtonExit;
		multiplayerButton.OnClicked -= OnMultiplayerButtonClick;
		settingsButton.OnDown -= OnSettingsButtonDown;
		settingsButton.OnClicked -= OnSettingsButtonClick;
		settingsButton.OnExit -= OnSettingsButtonExit;
		aboutButton.OnClicked -= OnAboutButtonButtonClick;
		highscoreButton.OnClicked -= OnShowHighscoreButtonClick;
		throneRoomButton.OnClicked -= OnThroneRoomButtonButtonClick;
		betaButton.OnClicked -= OnBetaButtonClick;
		newsButton.OnClicked -= OnNewsButtonClick;
	}

	private void OnNewGameButtonClick(int id, BaseEventData eventData)
	{
		NewGame();
	}

	private void OnResumeButtonDown(int id, BaseEventData eventData)
	{
		isResumeButtonHeld = true;
		buttonPressTime = Time.time;
	}

	private void OnResumeButtonClick(int id, BaseEventData eventData)
	{
		if (isResumeButtonHeld)
		{
			isResumeButtonHeld = false;
		}
		ResumeGame();
	}

	private void OnResumeButtonExit(int id, BaseEventData eventData)
	{
		isResumeButtonHeld = false;
	}

	private void OnMultiplayerButtonClick(int id, BaseEventData eventData)
	{
		ShowMultiplayer();
	}

	private void OnSettingsButtonDown(int id, BaseEventData eventData)
	{
		isSettingsButtonHeld = true;
		buttonPressTime = Time.time;
	}

	private void OnSettingsButtonClick(int id, BaseEventData eventData)
	{
		if (isSettingsButtonHeld)
		{
			isSettingsButtonHeld = false;
		}
		ShowSettings();
	}

	private void OnSettingsButtonExit(int id, BaseEventData eventData)
	{
		isSettingsButtonHeld = false;
	}

	private void OnShowHighscoreButtonClick(int id, BaseEventData eventData)
	{
		ShowHighscore();
	}

	private void OnThroneRoomButtonButtonClick(int id, BaseEventData eventdata)
	{
		ShowThroneRoom();
	}

	private void OnAboutButtonButtonClick(int id, BaseEventData eventdata)
	{
		ShowAbout();
	}

	private void OnBetaButtonClick(int id, BaseEventData eventdata)
	{
		ShowBetaInfo();
	}

	private void OnNewsButtonClick(int id, BaseEventData eventdata)
	{
		ShowNews();
	}
}
