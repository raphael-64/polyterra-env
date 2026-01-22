using System;
using System.Collections.Generic;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerPickerScreen : UIScreenBase
{
	[Header("Player Picker Screen")]
	[SerializeField]
	protected HeaderRow gameNameHeader;

	[SerializeField]
	protected TextMeshProUGUI gameDescription;

	[SerializeField]
	protected TextMeshProUGUI playerCountHeader;

	[SerializeField]
	protected UITextButton addplayerButton;

	[SerializeField]
	protected UITextButton startGameButton;

	[SerializeField]
	protected RectTransform playerList;

	[Header("Prefabs")]
	[SerializeField]
	protected FriendsRow friendRowPrefab;

	protected Stack<FriendsRow> cachedFriendRows = new Stack<FriendsRow>();

	protected List<FriendsRow> currentFriendRows = new List<FriendsRow>();

	protected bool haveSentGameStartRequest;

	private int MaxPlayers
	{
		get
		{
			int mapSize = GameManager.PreliminaryGameSettings.MapSize;
			return Mathf.Min(GameManager.GetMaxOpponents(), MapDataExtensions.GetMaximumOpponentCountForMapSize(mapSize)) + 1;
		}
	}

	public override void Show(bool instant = false)
	{
		base.Show(instant);
		haveSentGameStartRequest = false;
		Log.Verbose("PlayerPickerScreen :: Show :: Testing game name : {0}", new object[1] { PolyLanguage.MakeGameName() });
		GameSettings preliminaryGameSettings = GameManager.PreliminaryGameSettings;
		if (preliminaryGameSettings.GameType == GameType.Multiplayer || preliminaryGameSettings.GameType == GameType.Competitive)
		{
			if (!preliminaryGameSettings.HavePlayer(AccountManager.PlayerAccountId))
			{
				PlayerData playerData = new PlayerData();
				playerData.profile.id = AccountManager.PlayerAccountId;
				playerData.profile.name = AccountManager.Alias;
				playerData.type = PlayerData.Type.Player;
				playerData.state = PlayerData.State.IsYou;
				playerData.profile.avatarState = AccountManager.AvatarState;
				preliminaryGameSettings.AddPlayer(playerData);
			}
		}
		else if (preliminaryGameSettings.HavePlayer(AccountManager.PlayerAccountId))
		{
			preliminaryGameSettings.RemovePlayer(AccountManager.PlayerAccountId);
		}
		string text = ((preliminaryGameSettings.RulesGameMode != GameMode.Glory) ? Localization.Get(GameModeUtils.GetDescription(preliminaryGameSettings.RulesGameMode)) : Localization.Get(GameModeUtils.GetDescription(preliminaryGameSettings.RulesGameMode), LocalizationUtils.FormatNumber(GameManager.PreliminaryGameSettings.rules.ScoreLimit)));
		((TMP_Text)gameDescription).text = string.Format("{0}\n{1}\n{2}\n{3}", Localization.Get("playerpickerview.maptype", Localization.Get(preliminaryGameSettings.mapPreset.GetLocalizationName())), Localization.Get("playerpickerview.size", preliminaryGameSettings.MapSize * preliminaryGameSettings.MapSize), Localization.Get("playerpickerview.mode", Localization.Get(GameModeUtils.GetTitle(preliminaryGameSettings.RulesGameMode))), text);
		UpdateButtonStates();
		UpdateGameNameLabel();
		ClearPlayerList();
		UpdatePlayerList();
		UpdatePlayercountLabel();
		UpdateNavigation();
	}

	public override void OnBack()
	{
		base.OnBack();
	}

	private void UpdateGameNameLabel()
	{
		gameNameHeader.label.Text = GameManager.PreliminaryGameSettings.GameName;
	}

	private void UpdatePlayercountLabel()
	{
		((TMP_Text)playerCountHeader).text = Localization.Get("playerpickerview.players", GameManager.PreliminaryGameSettings.OpponentCount, MaxPlayers);
	}

	private void UpdateButtonStates()
	{
		UITextButton uITextButton = addplayerButton;
		bool buttonEnabled = (((Behaviour)addplayerButton.button).enabled = GameManager.PreliminaryGameSettings.OpponentCount < MaxPlayers);
		uITextButton.ButtonEnabled = buttonEnabled;
		int num = 2;
		UITextButton uITextButton2 = startGameButton;
		buttonEnabled = (((Behaviour)startGameButton.button).enabled = GameManager.PreliminaryGameSettings.OpponentCount >= num && !haveSentGameStartRequest);
		uITextButton2.ButtonEnabled = buttonEnabled;
	}

	private void UpdatePlayerList()
	{
		PlayerData[] players = GameManager.PreliminaryGameSettings.Players;
		int num = players.Length;
		for (int i = 0; i < num; i++)
		{
			PlayerData data = players[i];
			FriendsRow friendsRow;
			if (cachedFriendRows.Count > 0)
			{
				friendsRow = cachedFriendRows.Pop();
				((Component)friendsRow).gameObject.SetActive(true);
			}
			else
			{
				friendsRow = Object.Instantiate<FriendsRow>(friendRowPrefab, (Transform)(object)playerList);
			}
			((Transform)friendsRow.rectTransform).SetSiblingIndex(i);
			friendsRow.SetData(data);
			friendsRow.FriendActionCallback = OnFriendAction;
			currentFriendRows.Add(friendsRow);
		}
	}

	private void UpdateNavigation()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(playerList);
		Transform transform = ((Component)this).transform;
		UIUtils.SetExplicitNavigation((RectTransform)(object)((transform is RectTransform) ? transform : null), useCenter: true);
		UIUtils.UpdateNavigationOnListCells(currentFriendRows, null, (Selectable)(object)addplayerButton.button);
	}

	private void ClearPlayerList()
	{
		foreach (FriendsRow currentFriendRow in currentFriendRows)
		{
			((Component)currentFriendRow).gameObject.SetActive(false);
			cachedFriendRows.Push(currentFriendRow);
		}
		currentFriendRows.Clear();
	}

	private void OnFriendAction(FriendActions action, Guid id)
	{
		if (action != FriendActions.Remove)
		{
			return;
		}
		GameManager.PreliminaryGameSettings.RemovePlayer(id);
		for (int i = 0; i < currentFriendRows.Count; i++)
		{
			FriendsRow friendsRow = currentFriendRows[i];
			if (friendsRow.Id == id)
			{
				((Component)friendsRow).gameObject.SetActive(false);
				cachedFriendRows.Push(friendsRow);
				currentFriendRows.RemoveAt(i);
				if (!((Component)currentSelectable).gameObject.activeInHierarchy)
				{
					UINavigationManager.Select((Selectable)(object)addplayerButton.button);
				}
				break;
			}
		}
		UpdatePlayercountLabel();
		UpdateButtonStates();
		UpdateNavigation();
	}

	public void AddPlayer()
	{
		Log.Verbose("On Add Player", Array.Empty<object>());
		FriendsList friendsList = (FriendsList)UIManager.Instance.GetScreen(UIConstants.Screens.FriendsList);
		switch (GameManager.PreliminaryGameSettings.GameType)
		{
		case GameType.Multiplayer:
		case GameType.Competitive:
			friendsList.ListType = FriendsListType.FriendsPicker;
			break;
		case GameType.PassAndPlay:
			friendsList.ListType = FriendsListType.PassAndPlayPicker;
			break;
		}
		UIManager.Instance.ShowScreen(UIConstants.Screens.FriendsList);
	}

	public async void StartGame()
	{
		haveSentGameStartRequest = true;
		UpdateButtonStates();
		bool flag = false;
		PlayerData[] players = GameManager.PreliminaryGameSettings.Players;
		foreach (PlayerData playerData in players)
		{
			if (playerData.type == PlayerData.Type.Friend || playerData.type == PlayerData.Type.Local)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (GameManager.PreliminaryGameSettings.GameType == GameType.Multiplayer || GameManager.PreliminaryGameSettings.GameType == GameType.Competitive)
			{
				NetworkUtils.ShowLoader();
				UIInputBlocker.IncreaseBlockerCount();
				CreateSessionResult createSessionResult = await GameManager.Instance.CreateMultiplayerGame();
				NetworkUtils.HideLoader();
				UIInputBlocker.DecreaseBlockerCount();
				switch (createSessionResult)
				{
				case CreateSessionResult.Success:
					UIManager.Instance.ShowScreen(UIConstants.Screens.TribeSelector);
					UIManager.Instance.RemoveScreenFromStack(screenType);
					UIManager.Instance.RemoveScreenFromStack(UIConstants.Screens.GameSetup);
					break;
				case CreateSessionResult.FailedOpen:
					UIManager.Instance.OnBack();
					UIManager.Instance.OnBack();
					break;
				default:
					haveSentGameStartRequest = false;
					UpdateButtonStates();
					break;
				}
			}
			else if (GameManager.PreliminaryGameSettings.GameType == GameType.PassAndPlay)
			{
				UIManager.Instance.ShowScreen(UIConstants.Screens.TribeSelector);
			}
		}
		else
		{
			GameManager.GetAnalyticsManager().SendEvent("no_human_player_popup_view", new Dictionary<string, object>());
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("playerpickerview.human");
			basicPopup.Description = Localization.Get("playerpickerview.human.info");
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					haveSentGameStartRequest = false;
					UpdateButtonStates();
				})
			};
			basicPopup.Show(InputManager.GetInputPosition());
		}
	}

	protected override void SubscribeButtonsEvents()
	{
		base.SubscribeButtonsEvents();
		addplayerButton.OnClicked += AddPlayerButtonOnClicked;
		startGameButton.OnClicked += StartGameButtonOnClicked;
	}

	protected override void UnsubscribeButtonsEvents()
	{
		base.UnsubscribeButtonsEvents();
		addplayerButton.OnClicked -= AddPlayerButtonOnClicked;
		startGameButton.OnClicked -= StartGameButtonOnClicked;
	}

	private void AddPlayerButtonOnClicked(int id, BaseEventData eventdata)
	{
		AddPlayer();
	}

	private void StartGameButtonOnClicked(int id, BaseEventData eventdata)
	{
		StartGame();
	}
}
