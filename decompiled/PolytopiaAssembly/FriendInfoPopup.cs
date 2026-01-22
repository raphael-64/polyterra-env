using System;
using System.Collections;
using System.Collections.Generic;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class FriendInfoPopup : BasicPopup
{
	[Header("Friend Info Popup")]
	[SerializeField]
	protected AvatarView avatarView;

	[SerializeField]
	protected UITextButton invitationStateButton;

	[SerializeField]
	protected WinRatioSlider winRatioSlider;

	[SerializeField]
	protected TextMeshProUGUI innerMessage;

	private Guid friendId;

	private PlayerData.State friendshipState;

	private string friendName;

	private AvatarState avatarState;

	private PlayerData playerData;

	public Action<FriendActions, bool> OnFriendActionComplete;

	private bool isWaitingForResponse;

	private static float POPUP_SIZE_WITHOUT_WINRATIO = 350f;

	private static float POPUP_SIZE_WITH_WINRATIO = 400f;

	private static float POPUP_SIZE_CURRENT_PLAYER_INFO = 300f;

	private void OnEnable()
	{
		BackendEvents.OnRefreshFriends += OnRefreshFriends;
		BackendEvents.OnRefreshUser += OnRefreshUser;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		BackendEvents.OnRefreshFriends -= OnRefreshFriends;
		BackendEvents.OnRefreshUser -= OnRefreshUser;
	}

	public override void Show(Vector2 origin)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.Show(origin);
		((MonoBehaviour)this).StartCoroutine(WaitForFrameEnd());
	}

	private IEnumerator WaitForFrameEnd()
	{
		yield return (object)new WaitForEndOfFrame();
		yield return (object)new WaitForEndOfFrame();
		UpdateLayout();
	}

	private async void OnRefreshUser(PolytopiaUserViewModel user)
	{
		if (!isWaitingForResponse && !(user.PolytopiaId != friendId))
		{
			ResetPopup();
			PlayerData friendData = await AccountManager.GetFriendPlayerDataWithId(friendId);
			if (friendData == null)
			{
				SetData(friendId, friendName, PlayerData.State.None, avatarState, playerData);
				return;
			}
			await AccountManager.GetFriendViewModelWithId(friendId);
			SetData(friendData.profile.id, friendData.GetName(), friendData.state, friendData.profile.avatarState, friendData);
		}
	}

	private async void OnRefreshFriends(List<PolytopiaFriendViewModel> friends)
	{
		if (!isWaitingForResponse)
		{
			ResetPopup();
			PlayerData friendData = await AccountManager.GetFriendPlayerDataWithId(friendId);
			if (friendData == null)
			{
				SetData(friendId, friendName, PlayerData.State.None, avatarState, playerData);
				return;
			}
			await AccountManager.GetFriendViewModelWithId(friendId);
			SetData(friendData.profile.id, friendData.GetName(), friendData.state, friendData.profile.avatarState, friendData);
		}
	}

	public void SetData(Guid id, string name, PlayerData.State friendshipState, AvatarState avatarState, PlayerData playerData)
	{
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		friendId = id;
		friendName = name;
		this.avatarState = avatarState;
		this.friendshipState = friendshipState;
		this.playerData = playerData;
		((Component)invitationStateButton).gameObject.SetActive(false);
		string spriteStringForFriend = FriendUtils.GetSpriteStringForFriend(playerData);
		if (avatarState != null)
		{
			((Component)avatarView).gameObject.SetActive(true);
			avatarView.SetState(avatarState);
		}
		else
		{
			((Component)avatarView).gameObject.SetActive(false);
			((Component)winRatioSlider).gameObject.SetActive(false);
		}
		AutoCapitalizeHeader = false;
		if (playerData != null && playerData.profile != null && !playerData.profile.lastLoginDate.HasValue)
		{
			Header = spriteStringForFriend + name;
			Description = Localization.Get("versioning.unmigrateduser");
			((TMP_Text)innerMessage).text = "";
		}
		else if (friendshipState == PlayerData.State.Accepted && playerData != null)
		{
			Header = string.Format("{0} ({1})", spriteStringForFriend + name, Localization.Get("playerinfopopup.friend"));
			Description = FriendUtils.GetDescription(playerData, name);
		}
		else if (friendshipState == PlayerData.State.ReceivedRequest)
		{
			Header = spriteStringForFriend + name;
			Description = FriendUtils.GetDescription(playerData, name);
			((TMP_Text)innerMessage).text = Localization.Get($"friendlist.friendshipstate.{friendshipState.ToString().ToLowerInvariant()}", name);
		}
		else
		{
			Header = spriteStringForFriend + name;
			string key = ((friendshipState == PlayerData.State.Rejected) ? "friendlist.friendshipstate.none" : $"friendlist.friendshipstate.{friendshipState.ToString().ToLowerInvariant()}");
			Description = FriendUtils.GetDescription(playerData, name);
			((TMP_Text)innerMessage).text = Localization.Get(key);
		}
		string text = string.Empty;
		bool flag = true;
		bool flag2 = false;
		Color blue = ColorConstants.blue;
		PopupButtonData popupButtonData = null;
		SetPopupHeightAccordingToState();
		switch (friendshipState)
		{
		case PlayerData.State.None:
		case PlayerData.State.Rejected:
			text = "friendlist.new.button";
			((Component)winRatioSlider).gameObject.SetActive(false);
			((Component)((TMP_Text)innerMessage).transform.parent).gameObject.SetActive(true);
			break;
		case PlayerData.State.IsYou:
			flag = false;
			((Component)winRatioSlider).gameObject.SetActive(false);
			((Component)((TMP_Text)innerMessage).transform.parent).gameObject.SetActive(true);
			break;
		case PlayerData.State.Accepted:
		{
			flag = false;
			flag2 = true;
			popupButtonData = new PopupButtonData("friendlist.unfriend", PopupButtonData.States.None, OnRejectFriend, -1, closesPopup: false);
			int leftPlayerWinCount = (playerData.profile.victories.ContainsKey(AccountManager.PlayerAccountId) ? playerData.profile.victories[AccountManager.PlayerAccountId] : 0);
			int rightPlayerWinCount = (playerData.profile.defeats.ContainsKey(AccountManager.PlayerAccountId) ? playerData.profile.defeats[AccountManager.PlayerAccountId] : 0);
			((Component)winRatioSlider).gameObject.SetActive(true);
			((Component)((TMP_Text)innerMessage).transform.parent).gameObject.SetActive(false);
			winRatioSlider.SetUp(new WinRatioSliderData
			{
				leftAvatarState = AccountManager.AvatarState,
				leftAvatarName = AccountManager.Alias,
				leftPlayerWinCount = leftPlayerWinCount,
				rightAvatarState = avatarState,
				rightAvatarName = playerData.GetName(),
				rightPlayerWinCount = rightPlayerWinCount
			});
			break;
		}
		case PlayerData.State.SentRequest:
			flag = false;
			popupButtonData = new PopupButtonData("friendlist.unfriend", PopupButtonData.States.None, OnRejectFriend, -1, closesPopup: false);
			((Component)winRatioSlider).gameObject.SetActive(false);
			((Component)((TMP_Text)innerMessage).transform.parent).gameObject.SetActive(true);
			break;
		case PlayerData.State.ReceivedRequest:
			text = "friendlist.new.accept";
			popupButtonData = new PopupButtonData("friendlist.new.reject", PopupButtonData.States.None, OnRejectFriend, -1, closesPopup: false);
			((Component)winRatioSlider).gameObject.SetActive(false);
			((Component)((TMP_Text)innerMessage).transform.parent).gameObject.SetActive(true);
			break;
		}
		TopButtonData = popupButtonData;
		if (flag)
		{
			buttonData = new PopupButtonData[2]
			{
				new PopupButtonData("buttons.back"),
				new PopupButtonData(text, PopupButtonData.States.Selected, OnFriendAction, -1, closesPopup: false)
			};
			Buttons[1].BgColorStates.defaultColor = blue;
		}
		else if (flag2)
		{
			buttonData = new PopupButtonData[2]
			{
				new PopupButtonData("buttons.back"),
				new PopupButtonData("startmenu.new_game", PopupButtonData.States.Selected, delegate(int i, BaseEventData data)
				{
					StartGameWithFriend(i, data, playerData);
				})
			};
			Buttons[1].BgColorStates.defaultColor = blue;
		}
		else
		{
			buttonData = new PopupButtonData[1]
			{
				new PopupButtonData("buttons.back")
			};
		}
	}

	public void SetLobbyData(Guid id, string name, PlayerData.State friendshipState, AvatarState avatarState, PlayerData playerData, PlayerInvitationState invitationState)
	{
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_054d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		friendId = id;
		friendName = name;
		this.avatarState = avatarState;
		this.friendshipState = friendshipState;
		this.playerData = playerData;
		string spriteStringForFriendWithFormat = FriendUtils.GetSpriteStringForFriendWithFormat(playerData, "{0} ");
		if (avatarState != null)
		{
			((Component)avatarView).gameObject.SetActive(true);
			avatarView.SetState(avatarState);
		}
		else
		{
			((Component)avatarView).gameObject.SetActive(false);
			((Component)winRatioSlider).gameObject.SetActive(false);
		}
		AutoCapitalizeHeader = false;
		if (playerData != null && playerData.profile != null && !playerData.profile.lastLoginDate.HasValue)
		{
			Header = spriteStringForFriendWithFormat + name;
			Description = Localization.Get("versioning.unmigrateduser");
		}
		else if (friendshipState == PlayerData.State.Accepted && playerData != null)
		{
			Header = spriteStringForFriendWithFormat + name;
			Description = string.Format("{0}: {1}\n{2}: {3}\n{4}: {5}\n{6}: {7}\n{8}: {9}", Localization.Get("mplayerstats.alias"), name, Localization.Get("mplayerstats.friends"), playerData.profile.numFriends, Localization.Get("mplayerstats.games"), playerData.profile.numMultiplayerGames, Localization.Get("mplayerstats.gameversion"), playerData.profile.gameVersion, Localization.Get("mplayerstats.elo"), playerData.profile.multiplayerRating);
		}
		else if (friendshipState == PlayerData.State.ReceivedRequest)
		{
			Header = spriteStringForFriendWithFormat + name;
			Description = string.Format("{0}: {1}\n{2}: {3}\n{4}: {5}\n{6}: {7}\n{8}: {9}", Localization.Get("mplayerstats.alias"), name, Localization.Get("mplayerstats.friends"), playerData.profile.numFriends, Localization.Get("mplayerstats.games"), playerData.profile.numMultiplayerGames, Localization.Get("mplayerstats.gameversion"), playerData.profile.gameVersion, Localization.Get("mplayerstats.elo"), playerData.profile.multiplayerRating);
		}
		else
		{
			Header = spriteStringForFriendWithFormat + name;
			Description = string.Format("{0}: {1}\n{2}: {3}\n{4}: {5}\n{6}: {7}\n{8}: {9}", Localization.Get("mplayerstats.alias"), name, Localization.Get("mplayerstats.friends"), playerData.profile.numFriends, Localization.Get("mplayerstats.games"), playerData.profile.numMultiplayerGames, Localization.Get("mplayerstats.gameversion"), playerData.profile.gameVersion, Localization.Get("mplayerstats.elo"), playerData.profile.multiplayerRating);
		}
		SetPopupHeightAccordingToState();
		((Component)((TMP_Text)innerMessage).transform.parent).gameObject.SetActive(false);
		switch (friendshipState)
		{
		case PlayerData.State.None:
		case PlayerData.State.Rejected:
			((Component)winRatioSlider).gameObject.SetActive(false);
			break;
		case PlayerData.State.IsYou:
			((Component)winRatioSlider).gameObject.SetActive(false);
			break;
		case PlayerData.State.Accepted:
		{
			int leftPlayerWinCount = (playerData.profile.victories.ContainsKey(AccountManager.PlayerAccountId) ? playerData.profile.victories[AccountManager.PlayerAccountId] : 0);
			int rightPlayerWinCount = (playerData.profile.defeats.ContainsKey(AccountManager.PlayerAccountId) ? playerData.profile.defeats[AccountManager.PlayerAccountId] : 0);
			((Component)winRatioSlider).gameObject.SetActive(true);
			winRatioSlider.SetUp(new WinRatioSliderData
			{
				leftAvatarState = AccountManager.AvatarState,
				leftAvatarName = AccountManager.Alias,
				leftPlayerWinCount = leftPlayerWinCount,
				rightAvatarState = avatarState,
				rightAvatarName = playerData.GetName(),
				rightPlayerWinCount = rightPlayerWinCount
			});
			break;
		}
		case PlayerData.State.SentRequest:
			((Component)winRatioSlider).gameObject.SetActive(false);
			break;
		case PlayerData.State.ReceivedRequest:
			((Component)winRatioSlider).gameObject.SetActive(false);
			break;
		}
		((Component)invitationStateButton).gameObject.SetActive(true);
		invitationStateButton.ButtonEnabled = false;
		switch (invitationState)
		{
		case PlayerInvitationState.Invited:
			invitationStateButton.text = Localization.Get("onlineview.lobby.player.invited");
			invitationStateButton.BgColorStates = new UIButtonBase.ColorStates
			{
				defaultColor = ColorConstants.yellow,
				hoverColor = ColorConstants.yellow,
				highlightedColor = ColorConstants.yellow,
				highlightedHoverColor = ColorConstants.yellow,
				disabledColor = ColorConstants.yellow
			};
			break;
		case PlayerInvitationState.Accepted:
			invitationStateButton.text = Localization.Get("onlineview.lobby.player.accepted");
			invitationStateButton.BgColorStates = new UIButtonBase.ColorStates
			{
				defaultColor = ColorConstants.green,
				hoverColor = ColorConstants.green,
				highlightedColor = ColorConstants.green,
				highlightedHoverColor = ColorConstants.green,
				disabledColor = ColorConstants.green
			};
			break;
		default:
			((Component)invitationStateButton).gameObject.SetActive(false);
			break;
		}
		((Component)TopButton).gameObject.SetActive(false);
	}

	private void UpdateLayout()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)invitationStateButton).gameObject.activeInHierarchy)
		{
			float num = 12f;
			float num2 = ((TMP_Text)header).renderedWidth;
			float num3 = 6f;
			float width = invitationStateButton.rectTransform.GetWidth();
			float num4 = num + num2 + num3 + width + num;
			Rect rect = base.rectTransform.rect;
			if (num4 > ((Rect)(ref rect)).width)
			{
				rect = base.rectTransform.rect;
				num2 = ((Rect)(ref rect)).width - (num3 + width + num * 2f);
				((TMP_Text)header).rectTransform.SetWidth(num2);
			}
			invitationStateButton.rectTransform.SetAnchoredX(num2 + num3 + width * 0.5f);
		}
	}

	public async void OnFriendAction(int id, BaseEventData eventData)
	{
		FriendActions action = FriendUtils.GetActionFromState(friendshipState);
		ButtonsEnabled = false;
		isWaitingForResponse = true;
		bool flag = await FriendUtils.PerformFriendActionAsync(friendId, action);
		ButtonsEnabled = true;
		if (flag && action == FriendActions.Add)
		{
			GameManager.GetAnalyticsManager().SendEvent("friend_request_send", new Dictionary<string, object>());
			NotificationManager.Notify(Localization.Get("friendlist.requestsent.notification", friendName));
		}
		if (OnFriendActionComplete != null)
		{
			OnFriendActionComplete(action, flag);
		}
		isWaitingForResponse = false;
		if (flag)
		{
			Hide();
		}
		else
		{
			OnRefreshFriends(await AccountManager.GetFriendViewModels());
		}
	}

	public void StartGameWithFriend(int id, BaseEventData eventData, PlayerData friendPlayerData)
	{
		GameManager.PreliminaryGameSettings.GameType = GameType.Multiplayer;
		TryLoadPreviousSettings((GameType)PolytopiaPlayerPrefs.GetInt("previous_multiplayer_game_type", 1));
		UIManager.Instance.RemoveScreenFromStack(UIConstants.Screens.FriendsList);
		GameManager.PreliminaryGameSettings.GameType = GameType.Multiplayer;
		UIManager.Instance.ShowScreen(UIConstants.Screens.GameSetup);
		GameManager.PreliminaryGameSettings.AddPlayer(friendPlayerData);
		GameManager.GetAnalyticsManager().SendEvent("friends_add", new Dictionary<string, object>());
	}

	public void TryLoadPreviousSettings(GameType gameType)
	{
		string settingsNameFromModes = GameSettingsExtensions.GetSettingsNameFromModes(gameType, GameMode.Custom);
		if (!GameSettingsExtensions.TryLoadFromDisk(out var settings, settingsNameFromModes))
		{
			settings.GameType = gameType;
		}
		GameManager.PreliminaryGameSettings = settings;
	}

	public async void OnRejectFriend(int id, BaseEventData eventData)
	{
		NetworkUtils.ShowLoader();
		if ((Object)(object)topButton != (Object)null)
		{
			topButton.ButtonEnabled = false;
		}
		FriendRequestBindingModel model = new FriendRequestBindingModel
		{
			FriendUserId = friendId
		};
		ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.RemoveFriend(model);
		NetworkUtils.HideLoader();
		if (serverResponse == null || serverResponse.Success)
		{
			OnHide(-1, null);
			return;
		}
		if ((Object)(object)topButton != (Object)null)
		{
			topButton.ButtonEnabled = true;
		}
		PopupManager.ShowBackendErrorPopup(Localization.GetErrorMessage(serverResponse));
	}

	public override void ResetPopup()
	{
		base.ResetPopup();
		buttonContainer.ResetContainer();
		TopButtonData = null;
		OnFriendActionComplete = null;
		if ((Object)(object)topButton != (Object)null)
		{
			topButton.ButtonEnabled = true;
			topButton.OnClicked -= OnHide;
			topButton.OnClicked -= topButtonCallback;
		}
	}

	private void SetPopupHeightAccordingToState()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (friendshipState == PlayerData.State.Accepted)
		{
			content.sizeDelta = new Vector2(content.sizeDelta.x, POPUP_SIZE_WITH_WINRATIO);
		}
		else if (friendshipState == PlayerData.State.IsYou)
		{
			content.sizeDelta = new Vector2(content.sizeDelta.x, POPUP_SIZE_CURRENT_PLAYER_INFO);
		}
		else
		{
			content.sizeDelta = new Vector2(content.sizeDelta.x, POPUP_SIZE_WITHOUT_WINRATIO);
		}
	}
}
