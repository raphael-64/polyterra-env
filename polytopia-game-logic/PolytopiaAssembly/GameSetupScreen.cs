using System;
using System.Collections.Generic;
using DG.Tweening;
using Polytopia.Data;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameSetupScreen : UIScreenBase
{
	[Header("Game Setup Screen")]
	[SerializeField]
	protected RectTransform container;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected VerticalLayoutGroup verticalList;

	[SerializeField]
	protected ScrollRect scrollRect;

	[SerializeField]
	protected ButtonRow continueButtonRow;

	[Header("Prefabs")]
	[SerializeField]
	protected UIHorizontalList horizontalListPrefab;

	[SerializeField]
	protected GameSetupInfoRow infoRowPrefab;

	[SerializeField]
	protected GameSetupNameRow nameRowPrefab;

	[SerializeField]
	protected GameSetupGameNameRow gameNameRowPrefab;

	[SerializeField]
	protected ButtonRow buttonRowPrefab;

	[SerializeField]
	protected TribeCategoryContainer tribeCategoryContainerPrefab;

	[SerializeField]
	protected HeaderRow headerRowprefab;

	[SerializeField]
	protected UISpacer spacerPrefab;

	[SerializeField]
	protected UICollapsableContainer collapsableContainerPrefab;

	[SerializeField]
	protected GameSetupInfoRow gamePreferencesHeaderPrefab;

	protected float totalHeight;

	protected List<GameObject> rows = new List<GameObject>(6);

	private List<TribeCategoryContainer> tribeCategoryContainers = new List<TribeCategoryContainer>();

	protected RectTransform m_verticalListRectTr;

	protected GameSetupNameRow nameRow;

	protected GameSetupGameNameRow gameNameRow;

	protected GameSetupInfoRow singlePlayerInfoRow;

	protected GameSetupInfoRow gameModeInfoRow;

	protected GameSetupInfoRow networkInfoRow;

	protected GameSetupInfoRow continueButtonInfoRow;

	protected GameSetupInfoRow mapSizeInfoRow;

	protected GameSetupInfoRow matchmakingInfoRow;

	protected GameSetupInfoRow timeLimitInfoRow;

	private UIHorizontalList opponentList;

	private UIHorizontalList mapPresetList;

	private UIHorizontalList mapSizeList;

	private UIHorizontalList gameModeList;

	private UIHorizontalList networkList;

	private UIHorizontalList timeLimitList;

	private UIHorizontalList scoreLimitList;

	private UIHorizontalList autoSkipList;

	private UIHorizontalList topmostHorizontalList;

	private ButtonRow findButtonRow;

	private TimeLimit[] timeLimitsIndicies;

	private int[] scoreLimitsIndicies = new int[5] { 5000, 10000, 15000, 20000, 25000 };

	private int[] matchmakingOpponentCountIndices = new int[4] { 0, 1, 3, 8 };

	private bool hasBeenShown;

	private bool wasAutoSkipEnabled;

	private RectTransform VerticalListRectTr
	{
		get
		{
			if ((Object)(object)m_verticalListRectTr == (Object)null)
			{
				m_verticalListRectTr = ((Component)verticalList).GetComponent<RectTransform>();
			}
			return m_verticalListRectTr;
		}
	}

	protected void OnEnable()
	{
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChanged;
		StartSceneBg.Bright = false;
		wasAutoSkipEnabled = GameManager.PreliminaryGameSettings.IsAutoSkipEnabled;
	}

	protected void OnDisable()
	{
		BackendEvents.OnBackendConnectionChanged -= OnBackendConnectionChanged;
		if (hasBeenShown)
		{
			GameManager.PreliminaryGameSettings.SaveToDisk();
		}
	}

	private void OnBackendConnectionChanged(ConnectionStatus status)
	{
		RefreshInfo();
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}

	public override void OnButtonUp(InputManager.Buttons button)
	{
		if (button == InputManager.Buttons.Cancel && !PopupManager.PopupShowing && (Object)(object)nameRow != (Object)null && nameRow.IsSelected)
		{
			Selectable val = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
			UINavigationManager.Select(val);
			Log.Verbose("Should escape the name field and select : {0}", new object[1] { ((Object)val).name });
		}
		else
		{
			base.OnButtonUp(button);
		}
	}

	public override void Show(bool instant = false)
	{
		base.Show(instant);
		RedrawScreen();
	}

	public void RedrawScreen()
	{
		topmostHorizontalList = null;
		hasBeenShown = true;
		ClearList();
		scrollRect.verticalNormalizedPosition = 1f;
		if (GameManager.PreliminaryGameSettings.GameType == GameType.SinglePlayer)
		{
			if (!PolytopiaBackendAdapter.Instance.HasSocialLogin && SystemManager.ShouldShowLocalPlayerNameInput())
			{
				nameRow = CreateNameInputRow("gamesettings.yourname", AccountManager.Alias, OnNameChanged);
				CreateSpacer(20f);
			}
			if (GameManager.PreliminaryGameSettings.BaseGameMode == GameMode.Custom)
			{
				gameModeList = CreateCustomGameModeList();
				gameModeInfoRow = CreateInfoRow();
			}
			opponentList = CreateOpponentList();
			CreateDifficultyList();
			if (GameManager.PreliminaryGameSettings.BaseGameMode == GameMode.Custom)
			{
				mapPresetList = CreateMapPresetList();
				CreateMapSizeList();
			}
			singlePlayerInfoRow = CreateInfoRow();
			CreateSpacer(20f);
			CreateStartGameButton();
			UpdateOpponentList();
			RefreshInfo();
			if ((Object)(object)gameModeList != (Object)null)
			{
				CurrentSelectable = gameModeList.GetCurrentSelectable();
			}
			else
			{
				CurrentSelectable = opponentList.GetCurrentSelectable();
			}
		}
		else if (GameManager.PreliminaryGameSettings.GameType == GameType.Matchmaking)
		{
			if (VersionManager.GameVersion >= 90 && GameManager.PreliminaryGameSettings.LiveGamePreset)
			{
				gameNameRow = CreateRandomNameRow("playerpickerview.name", OnGameNameChanged);
				CreateSpacer(20f);
			}
			timeLimitList = CreateMatchmakingTimeLimitList();
			gameModeList = CreateMatchmakingGameModeList();
			opponentList = CreateMatchmakingOpponentList();
			((Component)timeLimitList).gameObject.SetActive(true);
			matchmakingInfoRow = CreateInfoRow();
			CreateSpacer(15f);
			UICollapsableContainer uICollapsableContainer = CreateCollapsableContainer();
			RectTransform contentRectTransform = uICollapsableContainer.GetContentRectTransform();
			uICollapsableContainer.SetCollapsed(HasAdvancedSettings());
			uICollapsableContainer.CollapsedStateChangedCallback = delegate(bool isCollapsed)
			{
				if (!isCollapsed)
				{
					ClearAdvancedSettings();
				}
			};
			CreateSpacer(15f, contentRectTransform);
			mapPresetList = CreateMapPresetList(contentRectTransform);
			CreateSpacer(10f, contentRectTransform);
			mapSizeList = CreateMapSizeList(contentRectTransform);
			CreateSpacer(20f, contentRectTransform);
			CreateFindMatchButton();
			RefreshInfo();
			CurrentSelectable = findButtonRow.GetMainSelectable();
		}
		else
		{
			if (VersionManager.GameVersion >= 90)
			{
				gameNameRow = CreateRandomNameRow("playerpickerview.name", OnGameNameChanged);
				CreateSpacer(20f);
			}
			networkList = CreateNetworkList();
			networkInfoRow = CreateInfoRow();
			gameModeList = CreateGameModeList();
			scoreLimitList = CreateScoreLimitsList();
			gameModeInfoRow = CreateInfoRow();
			mapPresetList = CreateMapPresetList();
			mapSizeList = CreateMapSizeList();
			mapSizeInfoRow = CreateInfoRow();
			timeLimitList = CreateTimeLimitList();
			((Component)timeLimitList).gameObject.SetActive(GameManager.PreliminaryGameSettings.GameType == GameType.Multiplayer);
			timeLimitInfoRow = CreateInfoRow();
			UpdateTimeInfo();
			CreateSpacer(40f);
			CreateContinueButton();
			RefreshInfo();
			GameManager.PreliminaryGameSettings.ClearPlayers();
			CurrentSelectable = networkList.GetCurrentSelectable();
		}
		if ((Object)(object)gameNameRow != (Object)null)
		{
			GameManager.PreliminaryGameSettings.GameName = gameNameRow.GetValue();
		}
		else
		{
			GameManager.PreliminaryGameSettings.GameName = PolyLanguage.MakeGameName();
		}
		OnScreenUpdated();
	}

	private bool HasAdvancedSettings()
	{
		if (GameManager.PreliminaryGameSettings.MapSize != 0)
		{
			return true;
		}
		if (GameManager.PreliminaryGameSettings.mapPreset != MapPreset.None)
		{
			return true;
		}
		return false;
	}

	private void ClearAdvancedSettings()
	{
		GameManager.PreliminaryGameSettings.MapSize = 0;
		GameManager.PreliminaryGameSettings.mapPreset = MapPreset.None;
		mapSizeList.SelectItem(GetMapSizeIndexFromSettings(), instant: true);
		mapPresetList.SelectItem(GetMapPresetIndexFromSettings(), instant: true);
		RefreshInfo();
	}

	private void RefreshValuesFromSettings()
	{
		if ((Object)(object)gameNameRow != (Object)null)
		{
			GameManager.PreliminaryGameSettings.GameName = gameNameRow.GetValue();
		}
		if ((Object)(object)gameModeList != (Object)null)
		{
			gameModeList.SelectItem(GetGameModeIndexFromSettings());
		}
		if ((Object)(object)mapPresetList != (Object)null)
		{
			mapPresetList.SelectItem(GetMapPresetIndexFromSettings());
		}
		if ((Object)(object)mapSizeList != (Object)null)
		{
			mapSizeList.SelectItem(GetMapSizeIndexFromSettings());
		}
		if ((Object)(object)timeLimitList != (Object)null && GameManager.PreliminaryGameSettings.GameType != GameType.PassAndPlay)
		{
			timeLimitList.SelectItem(GetTimeLimitIndexFromSettings());
		}
	}

	private GameSetupNameRow CreateNameInputRow(string headerKey, string name, Action<string> inputDoneCallback, RectTransform parent = null)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		GameSetupNameRow gameSetupNameRow = Object.Instantiate<GameSetupNameRow>(nameRowPrefab, (Transform)(object)(parent ?? VerticalListRectTr));
		gameSetupNameRow.HeaderKey = headerKey;
		gameSetupNameRow.inputDoneCallback = inputDoneCallback;
		gameSetupNameRow.Name = AccountManager.Alias;
		totalHeight += gameSetupNameRow.rectTransform.sizeDelta.y;
		rows.Add(((Component)gameSetupNameRow).gameObject);
		return gameSetupNameRow;
	}

	private UIHorizontalList CreateMatchmakingOpponentList(RectTransform parent = null)
	{
		string[] array = new string[matchmakingOpponentCountIndices.Length];
		array[0] = Localization.Get("gamesettings.unspecified");
		for (int i = 1; i < matchmakingOpponentCountIndices.Length; i++)
		{
			array[i] = (matchmakingOpponentCountIndices[i] + 1).ToString();
		}
		int selectedMatchmakingOpponentIndex = GetSelectedMatchmakingOpponentIndex();
		return CreateHorizontalList("gamesettings.players", array, OnMatchmakingOpponentsChanged, selectedMatchmakingOpponentIndex, parent);
	}

	private int GetSelectedMatchmakingOpponentIndex()
	{
		int num = Array.IndexOf(matchmakingOpponentCountIndices, GameManager.PreliminaryGameSettings.OpponentCount);
		if (num >= 0)
		{
			return num;
		}
		return 0;
	}

	private UIHorizontalList CreateOpponentList(RectTransform parent = null)
	{
		int maxOpponents = GameManager.GetMaxOpponents();
		int num = maxOpponents;
		if (GameManager.PreliminaryGameSettings.GameType == GameType.SinglePlayer)
		{
			num = Mathf.Min(GameManager.GetPurchaseManager().GetUnlockedTribeCount() - 1, maxOpponents);
		}
		string[] array = new string[maxOpponents + 1];
		for (int i = 0; i <= maxOpponents; i++)
		{
			array[i] = i.ToString();
		}
		int selectedIndex = Mathf.Min(num, GameManager.PreliminaryGameSettings.OpponentCount);
		return CreateHorizontalList("gamesettings.opponents", array, OnOpponentsChanged, selectedIndex, parent, num + 1, OnTriedSelectDisabledOpponent);
	}

	private void OnTriedSelectDisabledOpponent()
	{
		NotificationManager.Notify(Localization.Get("gamesettings.unlockmore"), Localization.Get("gamesettings.notavailable"));
	}

	private void UpdateOpponentList()
	{
		if (!((Object)(object)opponentList == (Object)null) && GameManager.PreliminaryGameSettings.GameType != GameType.Matchmaking)
		{
			bool num = GameManager.PreliminaryGameSettings.BaseGameMode == GameMode.Custom;
			bool flag = num && GameManager.PreliminaryGameSettings.RulesGameMode != GameMode.Domination;
			int num2 = (num ? MapDataExtensions.GetMaximumOpponentCountForMapSize(GameManager.PreliminaryGameSettings.MapSize) : GameManager.GetMaxOpponents());
			for (int i = 0; i < opponentList.items.Length; i++)
			{
				bool active = (flag && i == 0) || (i > 0 && i <= num2);
				((Component)opponentList.items[i]).gameObject.SetActive(active);
			}
			opponentList.RefreshScrollpositions();
			opponentList.RefreshNavigationIndexes();
		}
	}

	private UIHorizontalList CreateDifficultyList()
	{
		string[] items = new string[4]
		{
			Localization.Get(GameModeUtils.GetDifficultyName(GameSettings.Difficulties.Easy)),
			Localization.Get(GameModeUtils.GetDifficultyName(GameSettings.Difficulties.Normal)),
			Localization.Get(GameModeUtils.GetDifficultyName(GameSettings.Difficulties.Hard)),
			Localization.Get(GameModeUtils.GetDifficultyName(GameSettings.Difficulties.Crazy))
		};
		return CreateHorizontalList("gamesettings.difficulty", items, OnDifficultyChanged, (int)GameManager.PreliminaryGameSettings.Difficulty, null, PurchaseManager.UnlockedDifficulties.Length);
	}

	private void OnNameChanged(string value)
	{
		Log.Verbose("Name changed: {0}", new object[1] { value });
		AccountManager.Alias = value;
	}

	private void OnGameNameChanged(string value)
	{
		GameManager.PreliminaryGameSettings.GameName = value;
		RefreshInfo();
	}

	private void OnMatchmakingOpponentsChanged(int index)
	{
		GameManager.PreliminaryGameSettings.OpponentCount = matchmakingOpponentCountIndices[index];
		GameManager.PreliminaryGameSettings.SaveToDisk();
		RefreshInfo();
	}

	private void OnOpponentsChanged(int index)
	{
		GameManager.PreliminaryGameSettings.OpponentCount = index;
		GameManager.PreliminaryGameSettings.SaveToDisk();
		RefreshInfo();
	}

	private void OnDifficultyChanged(int index)
	{
		GameSettings.Difficulties difficulties = GameSettings.Difficulties.Easy;
		GameManager.PreliminaryGameSettings.Difficulty = index switch
		{
			1 => GameSettings.Difficulties.Normal, 
			2 => GameSettings.Difficulties.Hard, 
			3 => GameSettings.Difficulties.Crazy, 
			_ => GameSettings.Difficulties.Easy, 
		};
		GameManager.PreliminaryGameSettings.SaveToDisk();
		RefreshInfo();
	}

	private GameSetupGameNameRow CreateRandomNameRow(string headerKey, Action<string> onChangeCallback, RectTransform parent = null)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		GameSetupGameNameRow gameSetupGameNameRow = Object.Instantiate<GameSetupGameNameRow>(gameNameRowPrefab, (Transform)(object)(parent ?? VerticalListRectTr));
		gameSetupGameNameRow.HeaderKey = headerKey;
		gameSetupGameNameRow.OnValueChanged = onChangeCallback;
		gameSetupGameNameRow.UpdateGameName();
		totalHeight += gameSetupGameNameRow.rectTransform.sizeDelta.y;
		rows.Add(((Component)gameSetupGameNameRow).gameObject);
		return gameSetupGameNameRow;
	}

	private UIHorizontalList CreateNetworkList()
	{
		string[] items = new string[2]
		{
			Localization.Get("gamesettings.network.online"),
			Localization.Get("gamesettings.network.passplay")
		};
		int selectedIndex = 0;
		switch (GameManager.PreliminaryGameSettings.GameType)
		{
		case GameType.Multiplayer:
			selectedIndex = 0;
			break;
		case GameType.PassAndPlay:
			selectedIndex = 1;
			break;
		}
		return CreateHorizontalList("gamesettings.network", items, OnNetworkChanged, selectedIndex);
	}

	private UIHorizontalList CreateMatchmakingGameModeList()
	{
		string[] items = new string[3]
		{
			Localization.Get("gamesettings.unspecified"),
			Localization.Get(GameModeUtils.GetTitle(GameMode.Glory)),
			Localization.Get(GameModeUtils.GetTitle(GameMode.Might))
		};
		int selectedIndex = 0;
		switch (GameManager.PreliminaryGameSettings.BaseGameMode)
		{
		case GameMode.None:
			selectedIndex = 0;
			break;
		case GameMode.Glory:
			selectedIndex = 1;
			break;
		case GameMode.Might:
			selectedIndex = 2;
			break;
		}
		return CreateHorizontalList("gamesettings.mode", items, OnMatchmakingGameModeChanged, selectedIndex);
	}

	private int GetGameModeIndexFromSettings()
	{
		int result = 0;
		switch (GameManager.PreliminaryGameSettings.BaseGameMode)
		{
		case GameMode.Glory:
			result = 0;
			break;
		case GameMode.Might:
			result = 1;
			break;
		}
		return result;
	}

	private UIHorizontalList CreateGameModeList()
	{
		string[] items = new string[2]
		{
			Localization.Get(GameModeUtils.GetTitle(GameMode.Glory)),
			Localization.Get(GameModeUtils.GetTitle(GameMode.Might))
		};
		return CreateHorizontalList("gamesettings.mode", items, OnGameModeChanged, GetGameModeIndexFromSettings());
	}

	private int GetCustomGameModeIndexFromSettings()
	{
		int result = 0;
		switch (GameManager.PreliminaryGameSettings.RulesGameMode)
		{
		case GameMode.Perfection:
			result = 0;
			break;
		case GameMode.Domination:
			result = 1;
			break;
		case GameMode.Sandbox:
			result = 2;
			break;
		}
		return result;
	}

	private UIHorizontalList CreateCustomGameModeList()
	{
		string[] items = new string[3]
		{
			Localization.Get(GameModeUtils.GetTitle(GameMode.Perfection)),
			Localization.Get(GameModeUtils.GetTitle(GameMode.Domination)),
			Localization.Get(GameModeUtils.GetTitle(GameMode.Sandbox))
		};
		return CreateHorizontalList("gamesettings.mode", items, OnCustomGameModeChanged, GetCustomGameModeIndexFromSettings());
	}

	private int GetMapPresetIndexFromSettings()
	{
		int num = (int)GameManager.PreliminaryGameSettings.mapPreset;
		if (GameManager.PreliminaryGameSettings.GameType != GameType.Matchmaking)
		{
			num = Mathf.Max(num - 1, 0);
		}
		return num;
	}

	private UIHorizontalList CreateMapPresetList(RectTransform parent = null)
	{
		string[] array = null;
		array = ((GameManager.PreliminaryGameSettings.GameType != GameType.Matchmaking) ? new string[5]
		{
			Localization.Get("gamesettings.map.dryland"),
			Localization.Get("gamesettings.map.lakes"),
			Localization.Get("gamesettings.map.continents"),
			Localization.Get("gamesettings.map.archipelago"),
			Localization.Get("gamesettings.map.waterworld")
		} : new string[6]
		{
			Localization.Get("gamesettings.unspecified"),
			Localization.Get("gamesettings.map.dryland"),
			Localization.Get("gamesettings.map.lakes"),
			Localization.Get("gamesettings.map.continents"),
			Localization.Get("gamesettings.map.archipelago"),
			Localization.Get("gamesettings.map.waterworld")
		});
		return CreateHorizontalList("gamesettings.map", array, OnMapPresetChanged, GetMapPresetIndexFromSettings(), parent);
	}

	private int GetTimeLimitIndexFromSettings()
	{
		TimeLimit timeLimit = TimeLimitExtensions.TimeLimitFromSeconds((int)GameManager.PreliminaryGameSettings.BaseTimeSeconds);
		int timeLimitIndexFromTimeLimitIndicies = GetTimeLimitIndexFromTimeLimitIndicies(timeLimit);
		if (GameManager.PreliminaryGameSettings.GameType != GameType.Matchmaking && timeLimit == TimeLimit.None)
		{
			timeLimitIndexFromTimeLimitIndicies = GetTimeLimitIndexFromTimeLimitIndicies(TimeLimit.Medium);
		}
		return timeLimitIndexFromTimeLimitIndicies;
	}

	private int GetTimeLimitIndexFromTimeLimitIndicies(TimeLimit timeLimit)
	{
		for (int i = 0; i < timeLimitsIndicies.Length; i++)
		{
			if (timeLimitsIndicies[i] == timeLimit)
			{
				return i;
			}
		}
		return 0;
	}

	private UIHorizontalList CreateTimeLimitList(RectTransform parent = null)
	{
		string[] array = null;
		timeLimitsIndicies = new TimeLimit[3]
		{
			TimeLimit.Live,
			TimeLimit.Long,
			TimeLimit.VeryLong
		};
		array = new string[3]
		{
			Localization.Get("gamesettings.livegame"),
			LocalizationUtils.GetTimeString(TimeLimit.Long.ToTimeSpan()),
			LocalizationUtils.GetTimeString(TimeLimit.VeryLong.ToTimeSpan())
		};
		return CreateHorizontalList("gamesettings.timelimit", array, OnTimeLimitChanged, GetTimeLimitIndexFromSettings(), parent);
	}

	private UIHorizontalList CreateMatchmakingTimeLimitList(RectTransform parent = null)
	{
		string[] array = null;
		timeLimitsIndicies = new TimeLimit[2]
		{
			TimeLimit.Live,
			TimeLimit.Long
		};
		array = new string[2]
		{
			Localization.Get("gamesettings.livegame"),
			LocalizationUtils.GetTimeString(TimeLimit.Long.ToTimeSpan())
		};
		return CreateHorizontalList("gamesettings.timelimit", array, OnTimeLimitChanged, GetTimeLimitIndexFromSettings(), parent);
	}

	private UIHorizontalList CreateScoreLimitsList(RectTransform parent = null)
	{
		string[] array = new string[scoreLimitsIndicies.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = LocalizationUtils.FormatNumber(scoreLimitsIndicies[i]);
		}
		return CreateHorizontalList("gamesettings.scorelimit", array, OnScoreLimitChanged, 1, parent);
	}

	private UIHorizontalList CreateAutoSkipList(RectTransform parent = null)
	{
		string[] items = new string[2]
		{
			Localization.Get("gamesettings.autoskip.enabled"),
			Localization.Get("gamesettings.autoskip.disabled")
		};
		return CreateHorizontalList("gamesettings.autoskip", items, OnAutoSkipChanged, (!GameManager.PreliminaryGameSettings.IsAutoSkipEnabled) ? 1 : 0, parent);
	}

	private int GetMapSizeIndexFromSettings()
	{
		int num = (int)MapSizeExtensions.MapSizeFromInt(GameManager.PreliminaryGameSettings.MapSize);
		if (GameManager.PreliminaryGameSettings.GameType != GameType.Matchmaking)
		{
			num = Mathf.Max(0, num - 1);
		}
		return num;
	}

	private UIHorizontalList CreateMapSizeList(RectTransform parent = null)
	{
		string[] array = null;
		array = ((GameManager.PreliminaryGameSettings.GameType != GameType.Matchmaking) ? new string[6]
		{
			Localization.Get("gamesettings.size.tiny"),
			Localization.Get("gamesettings.size.small"),
			Localization.Get("gamesettings.size.normal"),
			Localization.Get("gamesettings.size.large"),
			Localization.Get("gamesettings.size.huge"),
			Localization.Get("gamesettings.size.massive")
		} : new string[7]
		{
			Localization.Get("gamesettings.unspecified"),
			Localization.Get("gamesettings.size.tiny"),
			Localization.Get("gamesettings.size.small"),
			Localization.Get("gamesettings.size.normal"),
			Localization.Get("gamesettings.size.large"),
			Localization.Get("gamesettings.size.huge"),
			Localization.Get("gamesettings.size.massive")
		});
		return CreateHorizontalList("gamesettings.size", array, OnMapSizeChanged, GetMapSizeIndexFromSettings(), parent);
	}

	private void CreateGamePreferencesHeaderRow(RectTransform parent = null)
	{
		GameSetupInfoRow gameSetupInfoRow = Object.Instantiate<GameSetupInfoRow>(gamePreferencesHeaderPrefab, (Transform)(object)(parent ?? VerticalListRectTr));
		rows.Add(((Component)gameSetupInfoRow).gameObject);
	}

	private UICollapsableContainer CreateCollapsableContainer()
	{
		UICollapsableContainer uICollapsableContainer = Object.Instantiate<UICollapsableContainer>(collapsableContainerPrefab, (Transform)(object)VerticalListRectTr);
		rows.Add(((Component)uICollapsableContainer).gameObject);
		return uICollapsableContainer;
	}

	private void CreateSpacer(float verticalSpace, RectTransform parent = null)
	{
		UISpacer uISpacer = Object.Instantiate<UISpacer>(spacerPrefab, (Transform)(object)(parent ?? VerticalListRectTr));
		uISpacer.Height = verticalSpace;
		rows.Add(((Component)uISpacer).gameObject);
	}

	private void CreateFindMatchButton()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)continueButtonRow != (Object)null)
		{
			((Component)continueButtonRow).gameObject.SetActive(false);
		}
		findButtonRow = Object.Instantiate<ButtonRow>(buttonRowPrefab, (Transform)(object)VerticalListRectTr);
		((Object)findButtonRow).name = "findMatchButton";
		findButtonRow.buttonComp.Key = "gamesettings.findmatch";
		findButtonRow.buttonComp.OnClicked += OnFindMatchClicked;
		totalHeight += findButtonRow.rectTransform.sizeDelta.y;
		rows.Add(((Component)findButtonRow).gameObject);
	}

	private void CreateContinueButton()
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)continueButtonRow != (Object)null)
		{
			((Component)continueButtonRow).gameObject.SetActive(false);
		}
		continueButtonRow = Object.Instantiate<ButtonRow>(buttonRowPrefab, (Transform)(object)VerticalListRectTr);
		((Object)continueButtonRow).name = "continueButton";
		continueButtonRow.buttonComp.Key = "gamesettings.continue";
		continueButtonRow.buttonComp.OnClicked -= OnContinueClicked;
		continueButtonRow.buttonComp.OnClicked -= OnStartGameClicked;
		continueButtonRow.buttonComp.OnClicked += OnContinueClicked;
		continueButtonRow.buttonComp.ButtonEnabled = GameManager.PreliminaryGameSettings.GameType != GameType.Multiplayer || GameManager.IsMultiplayerEnabled;
		((Component)continueButtonRow).gameObject.SetActive(true);
		totalHeight += continueButtonRow.rectTransform.sizeDelta.y;
		rows.Add(((Component)continueButtonRow).gameObject);
		continueButtonInfoRow = CreateInfoRow();
		((Component)continueButtonInfoRow).gameObject.SetActive(false);
	}

	private void CreateStartGameButton()
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)continueButtonRow != (Object)null)
		{
			((Component)continueButtonRow).gameObject.SetActive(false);
		}
		continueButtonRow = Object.Instantiate<ButtonRow>(buttonRowPrefab, (Transform)(object)VerticalListRectTr);
		((Object)continueButtonRow).name = "startGameButton";
		continueButtonRow.buttonComp.Key = "gamesettings.startgame";
		continueButtonRow.buttonComp.OnClicked -= OnContinueClicked;
		continueButtonRow.buttonComp.OnClicked -= OnStartGameClicked;
		continueButtonRow.buttonComp.OnClicked += OnStartGameClicked;
		((Component)continueButtonRow).gameObject.SetActive(true);
		totalHeight += continueButtonRow.rectTransform.sizeDelta.y;
		rows.Add(((Component)continueButtonRow).gameObject);
	}

	private void CreateHeaderRow(string key)
	{
		HeaderRow headerRow = Object.Instantiate<HeaderRow>(headerRowprefab, (Transform)(object)VerticalListRectTr);
		headerRow.label.format = "-{0}-";
		headerRow.label.Key = key;
		rows.Add(((Component)headerRow).gameObject);
	}

	private void OnNetworkChanged(int index)
	{
		Log.Verbose("OnNetworkChanged index: {0}, can create MP Game: {1}", new object[2]
		{
			index,
			GameManager.CanCreateMultiplayerGame()
		});
		GameType gameType = ((index == 0) ? GameType.Multiplayer : GameType.PassAndPlay);
		GameType gameType2 = (GameType)PolytopiaPlayerPrefs.GetInt("previous_multiplayer_game_type", 1);
		bool num = gameType != gameType2;
		PolytopiaPlayerPrefs.SetInt("previous_multiplayer_game_type", (int)gameType);
		string settingsNameFromModes = GameSettingsExtensions.GetSettingsNameFromModes(gameType, GameMode.Custom);
		if (!GameSettingsExtensions.TryLoadFromDisk(out var settings, settingsNameFromModes) || settings.GameType != gameType)
		{
			settings.GameType = gameType;
			GameManager.PreliminaryGameSettings = settings;
			GameManager.PreliminaryGameSettings.SaveToDisk();
		}
		else
		{
			EnsureAddedPlayersExist(settings);
			GameManager.PreliminaryGameSettings = settings;
		}
		if (num)
		{
			RefreshValuesFromSettings();
		}
		RefreshInfo();
	}

	private void EnsureAddedPlayersExist(GameSettings settings)
	{
		if (GameManager.PreliminaryGameSettings != null)
		{
			Dictionary<Guid, PlayerData> players = GameManager.PreliminaryGameSettings.players;
			if (players != null)
			{
				settings.players = players;
			}
		}
	}

	private void OnTimeLimitChanged(int index)
	{
		TimeLimit timeLimit = timeLimitsIndicies[index];
		float baseTimeSeconds;
		if (timeLimit == TimeLimit.Live && GameManager.PreliminaryGameSettings.GameType != GameType.PassAndPlay)
		{
			GameManager.PreliminaryGameSettings.TimeLimit = -1;
			GameManager.PreliminaryGameSettings.UseDynamicTimers = true;
			GameManager.PreliminaryGameSettings.UseTimeBanks = true;
			GameManager.PreliminaryGameSettings.LiveGamePreset = true;
			GameManager.PreliminaryGameSettings.TimeBonusPerCity = Config.liveModeCityBonus.FloatValue;
			GameManager.PreliminaryGameSettings.TimeBonusPerPopulation = Config.liveModePopulationBonus.FloatValue;
			wasAutoSkipEnabled = GameManager.PreliminaryGameSettings.IsAutoSkipEnabled;
			GameManager.PreliminaryGameSettings.IsAutoSkipEnabled = true;
			baseTimeSeconds = Config.liveModeBaseTime.FloatValue;
			if ((Object)(object)autoSkipList != (Object)null)
			{
				((Component)autoSkipList).gameObject.SetActive(false);
			}
		}
		else
		{
			baseTimeSeconds = timeLimit.ToSeconds();
			GameManager.PreliminaryGameSettings.LiveGamePreset = false;
			GameManager.PreliminaryGameSettings.TimeLimit = timeLimit.ToSeconds() / 60;
			if ((Object)(object)autoSkipList != (Object)null)
			{
				((Component)autoSkipList).gameObject.SetActive(true);
			}
			GameManager.PreliminaryGameSettings.IsAutoSkipEnabled = false;
		}
		GameManager.PreliminaryGameSettings.BaseTimeSeconds = baseTimeSeconds;
		GameManager.PreliminaryGameSettings.SaveToDisk();
		RefreshInfo();
	}

	private void OnScoreLimitChanged(int index)
	{
		GameManager.PreliminaryGameSettings.rules.ScoreLimit = scoreLimitsIndicies[index];
		RefreshInfo();
	}

	private void OnAutoSkipChanged(int index)
	{
		GameManager.PreliminaryGameSettings.IsAutoSkipEnabled = index == 0;
		GameManager.PreliminaryGameSettings.SaveToDisk();
	}

	private void OnGameModeChanged(int index)
	{
		GameManager.PreliminaryGameSettings.BaseGameMode = ((index == 0) ? GameMode.Glory : GameMode.Might);
		GameManager.PreliminaryGameSettings.SaveToDisk();
		RefreshInfo();
	}

	private void OnMatchmakingGameModeChanged(int index)
	{
		switch (index)
		{
		case 0:
			GameManager.PreliminaryGameSettings.BaseGameMode = GameMode.None;
			break;
		case 1:
			GameManager.PreliminaryGameSettings.BaseGameMode = GameMode.Glory;
			break;
		case 2:
			GameManager.PreliminaryGameSettings.BaseGameMode = GameMode.Might;
			break;
		}
		GameManager.PreliminaryGameSettings.SaveToDisk();
		RefreshInfo();
	}

	private void OnCustomGameModeChanged(int index)
	{
		switch (index)
		{
		case 0:
			GameManager.PreliminaryGameSettings.RulesGameMode = GameMode.Perfection;
			break;
		case 1:
			GameManager.PreliminaryGameSettings.RulesGameMode = GameMode.Domination;
			break;
		case 2:
			GameManager.PreliminaryGameSettings.RulesGameMode = GameMode.Sandbox;
			break;
		}
		UpdateOpponentList();
		GameManager.PreliminaryGameSettings.SaveToDisk();
		RefreshInfo();
	}

	private void OnMapPresetChanged(int index)
	{
		int num = index;
		if (GameManager.PreliminaryGameSettings.GameType != GameType.Matchmaking)
		{
			num++;
		}
		GameManager.PreliminaryGameSettings.mapPreset = (MapPreset)num;
		GameManager.PreliminaryGameSettings.SaveToDisk();
		RefreshInfo();
	}

	private void OnMapSizeChanged(int index)
	{
		int num = index;
		if (GameManager.PreliminaryGameSettings.GameType != GameType.Matchmaking)
		{
			num++;
		}
		int mapSize = ((MapSize)num).ToMapWidth();
		GameManager.PreliminaryGameSettings.MapSize = mapSize;
		UpdateOpponentList();
		GameManager.PreliminaryGameSettings.SaveToDisk();
		RefreshInfo();
	}

	private void OnTribeSelected(TribeData.Type tribeType, TribeData.Type secondTribeType = TribeData.Type.None)
	{
		if (PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).TryGetData(tribeType, out var data))
		{
			bool flag = true;
			if (GameManager.GameState != null && GameManager.GameState.Settings != null)
			{
				flag = GameManager.GameState.Settings.IsTribeEnabled(tribeType);
			}
			else if (GameManager.PreliminaryGameSettings != null)
			{
				flag = GameManager.PreliminaryGameSettings.IsTribeEnabled(tribeType);
			}
			SelectTribePopup selectTribePopup = PopupManager.GetSelectTribePopup();
			selectTribePopup.SetData(data);
			selectTribePopup.enabledChangedCallback = null;
			selectTribePopup.buttonData = new PopupBase.PopupButtonData[2]
			{
				new PopupBase.PopupButtonData("buttons.back"),
				new PopupBase.PopupButtonData(flag ? "tribepicker.disable" : "tribepicker.enable", PopupBase.PopupButtonData.States.Selected, OnTribeEnabledChanged, (int)tribeType)
			};
			selectTribePopup.Show();
		}
	}

	private void OnTribeEnabledChanged(int id, BaseEventData eventData)
	{
		Log.Verbose("GameSetupScreen :: OnTribeEnabledChanged :: id: {0}", new object[1] { id });
		if (GameManager.PreliminaryGameSettings != null)
		{
			GameManager.PreliminaryGameSettings.IsTribeEnabled((TribeData.Type)id);
		}
		foreach (TribeCategoryContainer tribeCategoryContainer in tribeCategoryContainers)
		{
			tribeCategoryContainer.RefreshItems();
		}
	}

	private void OnContinueClicked(int id, BaseEventData eventData)
	{
		Log.Verbose("Continue Clicked", Array.Empty<object>());
		if (GameManager.PreliminaryGameSettings.GameType == GameType.Multiplayer && !GameManager.CanCreateMultiplayerGame())
		{
			Log.Verbose("Can't create multiplayer game as requirements are not met.", Array.Empty<object>());
			return;
		}
		VersioningInfoHolder versioningInfoHolder = GameManager.GetVersioningInfoHolder();
		if (GameManager.PreliminaryGameSettings.GameType == GameType.Multiplayer && !versioningInfoHolder.IsNewMultiplayerEnabled(out var message))
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("versioning.newmultiplayer.title");
			basicPopup.Description = (string.IsNullOrEmpty(message) ? Localization.Get("versioning.newmultiplayer") : message);
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup.Show();
		}
		else if (VersionManager.GameVersion >= 90 && GameManager.PreliminaryGameSettings.GameType == GameType.Multiplayer)
		{
			TribeSelectorScreen obj = UIManager.Instance.ShowScreen(UIConstants.Screens.TribeSelector) as TribeSelectorScreen;
			obj.SetGameOwnerId(AccountManager.PlayerAccountId);
			obj.onTribePicked = async delegate(TribeData.Type tribe, SkinType skin, TribeData.Type tribeMix, List<TribeData.Type> disabledTribes)
			{
				List<int> list = new List<int>();
				if (disabledTribes != null)
				{
					for (int i = 0; i < disabledTribes.Count; i++)
					{
						list.Add((int)disabledTribes[i]);
					}
				}
				List<PlayerBindingModel> list2 = new List<PlayerBindingModel>
				{
					new PlayerBindingModel
					{
						PlayerName = AccountManager.Alias,
						AutoPlay = false,
						Handicap = 1,
						UserId = AccountManager.PlayerAccountId,
						Tribe = (int)tribe,
						Skin = (int)skin
					}
				};
				if (GameManager.PreliminaryGameSettings.players != null && GameManager.PreliminaryGameSettings.players.Count > 0)
				{
					foreach (KeyValuePair<Guid, PlayerData> player in GameManager.PreliminaryGameSettings.players)
					{
						if (player.Value.state != PlayerData.State.IsYou)
						{
							Log.Info("Adding player: {0} ({1})", new object[2]
							{
								player.Value.GetName(),
								player.Key
							});
							bool flag = player.Value.type == PlayerData.Type.Bot;
							list2.Add(new PlayerBindingModel
							{
								PlayerName = player.Value.GetName(),
								Handicap = ((!flag) ? 1 : GameSettings.HandicapFromDifficulty(player.Value.botDifficulty)),
								UserId = player.Key,
								AutoPlay = flag
							});
						}
					}
				}
				UIInputBlocker.IncreaseBlockerCount();
				NetworkUtils.ShowLoader();
				ServerResponse<LobbyGameViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.CreateLobby(new CreateLobbyBindingModel
				{
					GameName = GameManager.PreliminaryGameSettings.GameName,
					GameMode = GameManager.PreliminaryGameSettings.BaseGameMode,
					MapPreset = GameManager.PreliminaryGameSettings.mapPreset,
					MapSize = GameManager.PreliminaryGameSettings.MapSize,
					OpponentCount = (short)Mathf.Min(GameManager.GetMaxOpponents(), MapDataExtensions.GetMaximumOpponentCountForMapSize(GameManager.PreliminaryGameSettings.MapSize)),
					Platform = PolytopiaBackendAdapter.GetCurrentPlatform(),
					TimeLimit = GameManager.PreliminaryGameSettings.TimeLimit,
					ScoreLimit = GameManager.PreliminaryGameSettings.rules.ScoreLimit,
					Version = VersionManager.GameVersion,
					DisabledTribes = list,
					OwnerTribe = (int)tribe,
					OwnerTribeSkin = (int)skin,
					IsPersistent = !GameManager.PreliminaryGameSettings.LiveGamePreset,
					Invitations = list2
				});
				NetworkUtils.HideLoader();
				UIInputBlocker.DecreaseBlockerCount();
				if (serverResponse.Success)
				{
					GameManager.GetLobbyManager().AddOrUpdateLobby(serverResponse.Data);
					UIManager.Instance.RemoveScreenFromStack(UIConstants.Screens.MultiplayerScreen);
					UIManager.Instance.RemoveScreenFromStack(UIConstants.Screens.TribeSelector);
					UIManager.Instance.RemoveScreenFromStack(UIConstants.Screens.GameSetup);
					UIManager.Instance.ShowScreen(UIConstants.Screens.MultiplayerScreen);
					LobbyPopup lobbyPopup = PopupManager.GetLobbyPopup();
					lobbyPopup.SetData(serverResponse.Data);
					lobbyPopup.Show();
				}
				else
				{
					PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
				}
			};
		}
		else
		{
			UIManager.Instance.ShowScreen(UIConstants.Screens.PlayerPicker);
		}
	}

	private async void OnFindMatchClicked(int id, BaseEventData eventData)
	{
		NetworkUtils.ShowLoader();
		findButtonRow.buttonComp.ButtonEnabled = false;
		ServerResponse<MatchmakingSubmissionViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.SubmitMatchmakingRequest(new SubmitMatchmakingBindingModel
		{
			GameMode = GameManager.PreliminaryGameSettings.RulesGameMode,
			MapSize = GameManager.PreliminaryGameSettings.MapSize,
			MapPreset = GameManager.PreliminaryGameSettings.mapPreset,
			OpponentCount = (short)GameManager.PreliminaryGameSettings.OpponentCount,
			Version = VersionManager.GameVersion,
			TimeLimit = (GameManager.PreliminaryGameSettings.LiveGamePreset ? (-1) : GameManager.PreliminaryGameSettings.TimeLimit),
			Platform = PolytopiaBackendAdapter.GetCurrentPlatform(),
			ScoreLimit = GameManager.PreliminaryGameSettings.rules.ScoreLimit,
			AllowCrossPlay = true,
			UseLobbies = true
		});
		findButtonRow.buttonComp.ButtonEnabled = true;
		NetworkUtils.HideLoader();
		if (serverResponse.Success)
		{
			UIManager.Instance.OnBack();
			return;
		}
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = "Backend Error";
		basicPopup.Description = serverResponse.ErrorMessage;
		basicPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		basicPopup.Show();
		Log.Error("Backend Error {0}", new object[1] { serverResponse.ErrorMessage });
	}

	private UIHorizontalList CreateHorizontalList(string headerKey, string[] items, Action<int> indexChangedCallback = null, int selectedIndex = 0, RectTransform parent = null, int enabledItemCount = -1, Action onClickDisabledItemCallback = null)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		UIHorizontalList uIHorizontalList = Object.Instantiate<UIHorizontalList>(horizontalListPrefab, (Transform)(object)(parent ?? VerticalListRectTr));
		uIHorizontalList.HeaderKey = headerKey;
		uIHorizontalList.EnabledItemCount = enabledItemCount;
		uIHorizontalList.IndexChangedCallback = indexChangedCallback;
		uIHorizontalList.OnSelectDisabledItemCallback = onClickDisabledItemCallback;
		uIHorizontalList.SetData(items, selectedIndex);
		uIHorizontalList.UpdateScrollerOnHighlight = true;
		totalHeight += uIHorizontalList.rectTransform.sizeDelta.y;
		rows.Add(((Component)uIHorizontalList).gameObject);
		if ((Object)(object)topmostHorizontalList == (Object)null)
		{
			topmostHorizontalList = uIHorizontalList;
			PolytopiaInput.Omnicursor.AffixToUIElement(((Component)uIHorizontalList.items[uIHorizontalList.SelectedIndex].button).GetComponent<RectTransform>());
		}
		return uIHorizontalList;
	}

	private GameSetupInfoRow CreateInfoRow(RectTransform parent = null)
	{
		GameSetupInfoRow gameSetupInfoRow = Object.Instantiate<GameSetupInfoRow>(infoRowPrefab, (Transform)(object)(parent ?? VerticalListRectTr));
		rows.Add(((Component)gameSetupInfoRow).gameObject);
		return gameSetupInfoRow;
	}

	private void ShowScoreLimitList(bool shouldShow)
	{
		((Component)scoreLimitList).gameObject.SetActive(shouldShow);
		if (!shouldShow)
		{
			GameManager.PreliminaryGameSettings.rules.ScoreLimit = 0;
		}
	}

	private void RefreshInfo()
	{
		GameSettings preliminaryGameSettings = GameManager.PreliminaryGameSettings;
		bool flag = true;
		if (preliminaryGameSettings.GameType == GameType.Multiplayer)
		{
			flag = GameManager.CanCreateMultiplayerGame();
		}
		if ((Object)(object)continueButtonInfoRow != (Object)null)
		{
			((Component)continueButtonInfoRow).gameObject.SetActive(!flag);
		}
		if ((Object)(object)timeLimitList != (Object)null)
		{
			((Component)timeLimitList).gameObject.SetActive(GameManager.PreliminaryGameSettings.GameType == GameType.Multiplayer || GameManager.PreliminaryGameSettings.GameType == GameType.Matchmaking);
		}
		if ((Object)(object)scoreLimitList != (Object)null)
		{
			ShowScoreLimitList(preliminaryGameSettings.RulesGameMode == GameMode.Glory);
		}
		if ((Object)(object)continueButtonRow != (Object)null)
		{
			continueButtonRow.buttonComp.ButtonEnabled = flag;
		}
		if (preliminaryGameSettings.GameType == GameType.SinglePlayer)
		{
			if ((Object)(object)singlePlayerInfoRow == (Object)null)
			{
				return;
			}
			string text = Localization.Get("gamesettings.info.local", preliminaryGameSettings.OpponentCount, LocalizationUtils.FormatNumber(preliminaryGameSettings.MapSize * preliminaryGameSettings.MapSize));
			if (preliminaryGameSettings.RulesGameMode == GameMode.Perfection)
			{
				text += Localization.Get("gamesettings.info.turnlimit30");
				int num = Mathf.RoundToInt((1f + preliminaryGameSettings.DifficultyBonusMultiplier) * 100f);
				text = text + "\n" + Localization.Get("gamesettings.info.difficulty.bonus", num);
				if ((Object)(object)gameModeInfoRow != (Object)null)
				{
					gameModeInfoRow.Text = Localization.Get("gamemode.perfection.description");
				}
			}
			if (preliminaryGameSettings.RulesGameMode == GameMode.Domination && (Object)(object)gameModeInfoRow != (Object)null)
			{
				gameModeInfoRow.Text = Localization.Get("gamemode.domination.description");
			}
			if (preliminaryGameSettings.RulesGameMode == GameMode.Sandbox && (Object)(object)gameModeInfoRow != (Object)null)
			{
				gameModeInfoRow.Text = Localization.Get("gamemode.sandbox.description");
			}
			singlePlayerInfoRow.Text = text;
			return;
		}
		if ((Object)(object)networkInfoRow != (Object)null)
		{
			networkInfoRow.Text = ((preliminaryGameSettings.GameType == GameType.Multiplayer) ? Localization.Get("gamesettings.online.info") : Localization.Get("gamesettings.passplay.info"));
		}
		if ((Object)(object)gameModeInfoRow != (Object)null)
		{
			if (preliminaryGameSettings.RulesGameMode == GameMode.Glory)
			{
				gameModeInfoRow.Text = Localization.Get(GameModeUtils.GetDescription(preliminaryGameSettings.RulesGameMode), LocalizationUtils.FormatNumber(GameManager.PreliminaryGameSettings.rules.ScoreLimit));
			}
			else
			{
				gameModeInfoRow.Text = Localization.Get(GameModeUtils.GetDescription(preliminaryGameSettings.RulesGameMode));
			}
		}
		if ((Object)(object)mapSizeInfoRow != (Object)null)
		{
			if (preliminaryGameSettings.MapSize == 0)
			{
				mapSizeInfoRow.Text = Localization.Get("gamesettings.size.tiles.unspecified");
			}
			else
			{
				mapSizeInfoRow.Text = Localization.Get("gamesettings.size.tiles", LocalizationUtils.FormatNumber(preliminaryGameSettings.MapSize * preliminaryGameSettings.MapSize));
			}
		}
		if ((Object)(object)matchmakingInfoRow != (Object)null)
		{
			if (preliminaryGameSettings.RulesGameMode == GameMode.None && preliminaryGameSettings.OpponentCount == 0 && preliminaryGameSettings.mapPreset == MapPreset.None && preliminaryGameSettings.MapSize == 0 && preliminaryGameSettings.TimeLimit == 0)
			{
				matchmakingInfoRow.Text = Localization.Get("gamesettings.matchmaking.noedit");
			}
			else
			{
				string text2 = string.Empty;
				if (preliminaryGameSettings.RulesGameMode != GameMode.None)
				{
					text2 = Localization.Get("gamesettings.info.matchmaking.gamemode", Localization.Get(GameModeUtils.GetTitle(preliminaryGameSettings.RulesGameMode)));
				}
				if (preliminaryGameSettings.OpponentCount != 0)
				{
					string text3 = ((text2.Length > 0) ? ", " : "");
					text2 = text2 + text3 + Localization.Get("tribepicker.players", preliminaryGameSettings.OpponentCount + 1);
				}
				if (preliminaryGameSettings.mapPreset != MapPreset.None)
				{
					string text4 = ((text2.Length > 0) ? ", " : "");
					text2 = text2 + text4 + Localization.Get("gamesettings.info.matchmaking.mappreset", Localization.Get(preliminaryGameSettings.mapPreset.GetLocalizationName()));
				}
				if (preliminaryGameSettings.MapSize != 0)
				{
					string text5 = ((text2.Length > 0) ? ", " : "");
					text2 = text2 + text5 + Localization.Get("gamesettings.info.matchmaking.mapsize", LocalizationUtils.FormatNumber(preliminaryGameSettings.MapSize * preliminaryGameSettings.MapSize));
				}
				if (preliminaryGameSettings.TimeLimit == -1)
				{
					string text6 = ((text2.Length > 0) ? ", " : "");
					text2 = text2 + text6 + Localization.Get("gamesettings.livegame");
				}
				else if (preliminaryGameSettings.TimeLimit != 0)
				{
					string text7 = ((text2.Length > 0) ? ", " : "");
					text2 = text2 + text7 + Localization.Get("gamesettings.info.matchmaking.timelimit", LocalizationUtils.GetTimeString(TimeSpan.FromMinutes(preliminaryGameSettings.TimeLimit)));
				}
				matchmakingInfoRow.Text = text2;
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(VerticalListRectTr);
		}
		if ((Object)(object)continueButtonInfoRow != (Object)null && !GameManager.CanCreateMultiplayerGame())
		{
			continueButtonInfoRow.Text = Localization.Get("gamesettings.online.disabled.info");
		}
		UpdateTimeInfo();
	}

	private void UpdateTimeInfo()
	{
		if (!((Object)(object)timeLimitInfoRow == (Object)null))
		{
			bool flag = timeLimitsIndicies[timeLimitList.SelectedIndex] == TimeLimit.Live && GameManager.PreliminaryGameSettings.GameType != GameType.PassAndPlay;
			((Component)timeLimitInfoRow).gameObject.SetActive(flag);
			if (flag && !GameVersionUtils.HideEsport)
			{
				timeLimitInfoRow.Text = Localization.Get("gamesettings.livegame.description");
			}
		}
	}

	private void ClearList()
	{
		foreach (GameObject row in rows)
		{
			Object.DestroyImmediate((Object)(object)row);
		}
		rows.Clear();
		totalHeight = 0f;
		singlePlayerInfoRow = null;
		networkInfoRow = null;
		gameModeInfoRow = null;
		mapSizeInfoRow = null;
		tribeCategoryContainers.Clear();
	}

	public void OnStartGameClicked(int id, BaseEventData eventData)
	{
		ShowBlackFader();
	}

	private void ShowBlackFader()
	{
		UIBlackFader.FadeIn(0.5f, delegate
		{
			OnStartGame();
		});
	}

	private async void OnStartGame()
	{
		DOTween.KillAll(false);
		await GameManager.Instance.CreateSinglePlayerGame();
	}
}
