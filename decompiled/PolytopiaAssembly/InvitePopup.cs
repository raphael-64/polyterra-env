using System;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using UnityEngine;
using UnityEngine.EventSystems;

public class InvitePopup : BasicPopup
{
	[SerializeField]
	protected FriendsListComponent friendsListComponent;

	[NonSerialized]
	public int maxPlayers;

	private LobbyGameViewModel lobbyData;

	public Action<PlayerData> OnFriendClicked;

	public Action<int> OnBotClicked;

	public override void Show(Vector2 origin)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		CreateFriendsList();
		TopButtonData = new PopupButtonData
		{
			text = "onlineview.lobby.share.button",
			callback = OnInviteLinkButtonClicked,
			closesPopup = false
		};
		TopButton.BGColors.defaultColor = ColorConstants.blue;
		UpdateHeader();
		base.Show(origin);
	}

	public void SetData(LobbyGameViewModel lobbyGameViewModel, int maxPlayers)
	{
		lobbyData = lobbyGameViewModel;
		this.maxPlayers = maxPlayers;
	}

	private void CreateFriendsList(bool forceReload = false)
	{
		friendsListComponent.PlayerPickedCallback = PlayerPickedCallback;
		friendsListComponent.ListType = FriendsListType.FriendsPicker;
		friendsListComponent.Show();
	}

	private void UpdateHeader()
	{
		Header = Localization.Get("onlineview.lobby.invite.title", GetTotalPlayers(), maxPlayers);
	}

	private int GetTotalPlayers()
	{
		return LobbyManager.GetActiveParticipatorsCount(lobbyData);
	}

	protected void OnInviteLinkButtonClicked(int id, BaseEventData eventData = null)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (!lobbyData.IsSharable)
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("onlineview.lobby.share.title");
			basicPopup.Description = Localization.Get("onlineview.lobby.share.description");
			basicPopup.buttonData = new PopupButtonData[2]
			{
				new PopupButtonData("buttons.back"),
				new PopupButtonData("buttons.enable", PopupButtonData.States.Selected, OnEnableInviteLink)
			};
			basicPopup.Show(InputManager.GetInputPosition());
		}
		else
		{
			OnCopyInviteLink();
		}
	}

	private async void OnEnableInviteLink(int id, BaseEventData eventData = null)
	{
		OnCopyInviteLink();
		ServerResponse<LobbyGameViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.ActivateLobbyLinkInvitations(new ActivateLobbyLinkInvitationsBindingModel
		{
			LobbyId = lobbyData.Id
		});
		if (serverResponse.Success)
		{
			GameManager.GetLobbyManager().AddOrUpdateLobby(serverResponse.Data);
			lobbyData = serverResponse.Data;
		}
		else
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
	}

	private void OnCopyInviteLink()
	{
		GUIUtility.systemCopyBuffer = lobbyData.InviteLink;
		NotificationManager.Notify(Localization.Get("onlineview.lobby.clipboard"), Localization.Get("onlineview.lobby.clipboard.title"));
	}

	protected void PlayerPickedCallback(PlayerData friend)
	{
		if (friend.type == PlayerData.Type.Bot)
		{
			OnBotClicked((int)friend.botDifficulty);
		}
		else if (friend.profile.gameVersion < 90)
		{
			PopupManager.ShowErrorPopup(Localization.Get("friendlist.friend.update", friend.GetName()));
		}
		else
		{
			OnFriendClicked(friend);
		}
	}
}
