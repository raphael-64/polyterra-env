using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PullToRefresh;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendsListComponent : UIBasicComponent
{
	[Header("Friends List")]
	[SerializeField]
	protected RectTransform container;

	[SerializeField]
	protected ScrollRect scrollRect;

	[SerializeField]
	protected UIRefreshControl refresher;

	[Header("Prefabs")]
	[SerializeField]
	protected FriendsCategoryContainer friendsCategoryPrefab;

	[SerializeField]
	protected ButtonRow buttonRow;

	[SerializeField]
	protected FriendsListPlayerInfo playerInfoPrefab;

	protected List<FriendsCategoryContainer> friendCategories = new List<FriendsCategoryContainer>();

	protected List<GameObject> otherListComponents = new List<GameObject>();

	protected FriendsListType friendsListType;

	protected FriendsListType lastFriendsListType;

	protected PlayerData[] friendsData;

	protected SearchFriendPopup searchFriendPopup;

	[NonSerialized]
	public Action<PlayerData> PlayerPickedCallback;

	public FriendsListType ListType
	{
		get
		{
			return friendsListType;
		}
		set
		{
			friendsListType = value;
		}
	}

	private void OnEnable()
	{
		BackendEvents.OnRefreshFriends += OnRefreshFriends;
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChanged;
		BackendEvents.OnPlayersStatusesUpdated += OnPlayersStatusesUpdated;
	}

	private void OnDisable()
	{
		BackendEvents.OnRefreshFriends -= OnRefreshFriends;
		BackendEvents.OnBackendConnectionChanged -= OnBackendConnectionChanged;
		BackendEvents.OnPlayersStatusesUpdated -= OnPlayersStatusesUpdated;
	}

	private async void OnRefreshFriends(List<PolytopiaFriendViewModel> friends)
	{
		switch (ListType)
		{
		case FriendsListType.FriendsList:
			NetworkUtils.ShowLoader(Localization.Get("friendlist.reloading", 0));
			lastFriendsListType = FriendsListType.None;
			await CreateFriendsList();
			NetworkUtils.HideLoader();
			break;
		case FriendsListType.FriendsPicker:
			NetworkUtils.ShowLoader(Localization.Get("friendlist.reloading", 0));
			lastFriendsListType = FriendsListType.None;
			await CreatePickFriendsList();
			NetworkUtils.HideLoader();
			break;
		case FriendsListType.None:
		case FriendsListType.PassAndPlayPicker:
			break;
		}
	}

	private void OnBackendConnectionChanged(ConnectionStatus status)
	{
		Show();
	}

	public async void Show()
	{
		if (ListType == FriendsListType.None)
		{
			Log.Warning("FriendsList :: Show :: ListType : None", Array.Empty<object>());
		}
		else if (PolytopiaBackendAdapter.Instance.IsConnected || (ListType != FriendsListType.FriendsList && ListType != FriendsListType.FriendsPicker))
		{
			switch (ListType)
			{
			case FriendsListType.FriendsList:
				NetworkUtils.ShowLoader(Localization.Get("friendlist.loading"));
				await CreateFriendsList();
				NetworkUtils.HideLoader();
				break;
			case FriendsListType.FriendsPicker:
				NetworkUtils.ShowLoader(Localization.Get("friendlist.loading"));
				await CreatePickFriendsList();
				NetworkUtils.HideLoader();
				break;
			case FriendsListType.PassAndPlayPicker:
				CreateHotseatList();
				break;
			}
			if (AccountManager.GetFriendErrorResponse() != null)
			{
				NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(AccountManager.GetFriendErrorResponse()));
			}
			lastFriendsListType = ListType;
		}
	}

	protected async Task CreateFriendsList(bool forceUpdate = false)
	{
		if (lastFriendsListType == FriendsListType.FriendsList)
		{
			RefreshCurrentList();
			return;
		}
		await LoadFriends(forceUpdate);
		ClearCurrentList();
		RefreshSearchData();
		PlayerData[] friendsByCategory = FriendUtils.GetFriendsByCategory(friendsData, PlayerData.State.Accepted);
		PlayerData[] friendsByCategory2 = FriendUtils.GetFriendsByCategory(friendsData, PlayerData.State.ReceivedRequest);
		PlayerData[] friendsByCategory3 = FriendUtils.GetFriendsByCategory(friendsData, PlayerData.State.SentRequest);
		PlayerData[] friendsByCategory4 = FriendUtils.GetFriendsByCategory(friendsData, PlayerData.State.Rejected);
		CreateButtonRow(Localization.Get("friendlist.new.caps"), OnAddFriend);
		if (friendsByCategory2.Length != 0)
		{
			CreateCategory(friendsByCategory2, "friendlist.friendrequests", OnShowplayerInfo, FriendsCategoryContainer.DisableFlags.Unmigrated);
		}
		if (friendsByCategory.Length != 0)
		{
			CreateCategory(friendsByCategory, "friendlist.friends", OnShowplayerInfo, FriendsCategoryContainer.DisableFlags.Unmigrated);
		}
		if (friendsByCategory3.Length != 0)
		{
			CreateCategory(friendsByCategory3, "friendlist.pendinginvitations", OnShowplayerInfo, FriendsCategoryContainer.DisableFlags.Unmigrated);
		}
		if (friendsByCategory4.Length != 0)
		{
			CreateCategory(friendsByCategory4, "friendlist.rejectedinvitations", OnShowplayerInfo, FriendsCategoryContainer.DisableFlags.Unmigrated);
		}
	}

	protected async Task CreatePickFriendsList(bool forceUpdate = false)
	{
		if (lastFriendsListType == FriendsListType.FriendsPicker)
		{
			RefreshCurrentList();
			return;
		}
		await LoadFriends(forceUpdate);
		ClearCurrentList();
		PlayerData[] friendsByCategory = FriendUtils.GetFriendsByCategory(friendsData, PlayerData.State.Accepted);
		PlayerData[] friendsByCategory2 = FriendUtils.GetFriendsByCategory(friendsData, PlayerData.State.ReceivedRequest);
		PlayerData[] friendsByCategory3 = FriendUtils.GetFriendsByCategory(friendsData, PlayerData.State.SentRequest);
		FriendsCategoryContainer.DisableFlags disableFlags = FriendsCategoryContainer.DisableFlags.PickedForMatch | FriendsCategoryContainer.DisableFlags.Unmigrated;
		CreateButtonRow(Localization.Get("friendlist.new.caps"), OnAddFriend);
		if (friendsByCategory2.Length != 0)
		{
			CreateCategory(friendsByCategory2, "friendlist.friendrequests", OnShowplayerInfo, disableFlags);
		}
		if (friendsByCategory.Length != 0)
		{
			CreateCategory(friendsByCategory, "friendlist.friends", OnPlayerPicked, disableFlags);
		}
		CreateBotList(OnPlayerPicked);
		if (friendsByCategory3.Length != 0)
		{
			CreateCategory(friendsByCategory3, "friendlist.pendinginvitations", OnShowplayerInfo, disableFlags);
		}
	}

	protected void CreateHotseatList()
	{
		if (lastFriendsListType == FriendsListType.PassAndPlayPicker)
		{
			RefreshCurrentList();
			return;
		}
		ClearCurrentList();
		HotseatProfilesState hotseatProfilesState = GameManager.GetHotseatProfilesState();
		PlayerData[] array = new PlayerData[12];
		for (int i = 0; i < 12; i++)
		{
			PlayerProfileState profile = hotseatProfilesState.players[i];
			PlayerData playerData = new PlayerData();
			playerData.profile = profile;
			playerData.defaultName = Localization.Get("friendlist.player", i + 1);
			playerData.type = PlayerData.Type.Local;
			playerData.state = PlayerData.State.Accepted;
			array[i] = playerData;
		}
		CreateCategory(array, "friendlist.local", OnPlayerPicked, (FriendsCategoryContainer.DisableFlags)0, shouldSortData: false);
		CreateBotList(OnPlayerPicked);
	}

	protected void CreateBotList(Action<PlayerData> PlayerPickedCallback, int count = 4)
	{
		PlayerData[] array = new PlayerData[count];
		for (int i = 0; i < count; i++)
		{
			PlayerData playerData = new PlayerData();
			playerData.type = PlayerData.Type.Bot;
			playerData.profile.id = Guid.NewGuid();
			playerData.botDifficulty = (GameSettings.Difficulties)i;
			playerData.profile.name = string.Empty;
			array[i] = playerData;
		}
		CreateCategory(array, "friendlist.bots", PlayerPickedCallback, (FriendsCategoryContainer.DisableFlags)0, shouldSortData: false);
	}

	protected void CreateCategory(PlayerData[] data, string categoryStringId, Action<PlayerData> PlayerPickedCallback, FriendsCategoryContainer.DisableFlags disableFlags, bool shouldSortData = true)
	{
		FriendsCategoryContainer friendsCategoryContainer = Object.Instantiate<FriendsCategoryContainer>(friendsCategoryPrefab, (Transform)(object)container);
		friendsCategoryContainer.disableFlags = disableFlags;
		friendsCategoryContainer.SetData(data, categoryStringId, shouldSortData);
		friendsCategoryContainer.selectCallback = PlayerPickedCallback;
		friendsCategoryContainer.GetButton(0);
		friendCategories.Add(friendsCategoryContainer);
	}

	protected void CreateButtonRow(string label, UIButtonBase.ButtonAction callback)
	{
		ButtonRow buttonRow = Object.Instantiate<ButtonRow>(this.buttonRow, (Transform)(object)container);
		buttonRow.buttonComp.text = label;
		buttonRow.buttonComp.OnClicked += callback;
		otherListComponents.Add(((Component)buttonRow).gameObject);
	}

	protected void CreatePlayerInfo()
	{
		FriendsListPlayerInfo friendsListPlayerInfo = Object.Instantiate<FriendsListPlayerInfo>(playerInfoPrefab, (Transform)(object)container);
		otherListComponents.Add(((Component)friendsListPlayerInfo).gameObject);
	}

	protected void OnShowplayerInfo(PlayerData friend)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		PopupManager.GetFriendInfoPopup(friend.profile.id, friend.GetName(), friend.state, friend.profile.avatarState, friend).Show(InputManager.GetInputPosition());
	}

	protected void OnAddFriend(int id, BaseEventData eventData)
	{
		GameManager.GetAnalyticsManager().SendEvent("friends_add_popup_view", new Dictionary<string, object>());
		searchFriendPopup = PopupManager.GetSearchFriendPopup();
		searchFriendPopup.Show();
	}

	protected void OnPlayerPicked(PlayerData friend)
	{
		if (friend.type == PlayerData.Type.Bot)
		{
			friend = friend.Clone();
			friend.profile.id = Guid.NewGuid();
		}
		if (friend.type != PlayerData.Type.Local && friend.type != PlayerData.Type.Bot && !friend.profile.lastLoginDate.HasValue)
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("versioning.unmigrateduser.title");
			basicPopup.Description = Localization.Get("versioning.unmigrateduser");
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup.Show();
		}
		else
		{
			GameManager.GetAnalyticsManager().SendEvent("friends_add", new Dictionary<string, object>());
			Log.Verbose("friend.profile.id: {0}", new object[1] { friend.profile.id });
			PlayerPickedCallback?.Invoke(friend);
		}
	}

	public async void OnRefreshList()
	{
		if (GameManager.IsNetworkEnabled())
		{
			await PolytopiaBackendAdapter.Instance.EnsureAuthenticatedAsync();
		}
		if (PolytopiaBackendAdapter.Instance.IsConnected)
		{
			switch (ListType)
			{
			case FriendsListType.FriendsList:
				NetworkUtils.ShowLoader(Localization.Get("friendlist.reloading"), 0);
				lastFriendsListType = FriendsListType.None;
				NetworkUtils.HideLoader();
				await CreateFriendsList(forceUpdate: true);
				break;
			case FriendsListType.FriendsPicker:
				NetworkUtils.ShowLoader(Localization.Get("friendlist.reloading"), 0);
				lastFriendsListType = FriendsListType.None;
				await CreatePickFriendsList(forceUpdate: true);
				NetworkUtils.HideLoader();
				break;
			default:
				refresher.EndRefreshing();
				return;
			}
		}
		else
		{
			NetworkUtils.ShowLoaderError(Localization.Get("friendlist.loaderror"));
		}
		if (AccountManager.GetFriendErrorResponse() != null)
		{
			NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(AccountManager.GetFriendErrorResponse()));
		}
		else if (PolytopiaBackendAdapter.Instance.IsConnected)
		{
			NetworkUtils.ShowLoader(Localization.Get("friendlist.uptodate"), 0, 2f);
		}
		refresher.EndRefreshing();
	}

	public void OnRefreshTrigger()
	{
		if (ListType != FriendsListType.PassAndPlayPicker)
		{
			NetworkUtils.ShowLoader(Localization.Get("onlineview.reloading.release"), 0);
		}
	}

	public void OnRefreshCancelled()
	{
		NetworkUtils.HideLoader();
	}

	protected void RefreshCurrentList()
	{
		foreach (FriendsCategoryContainer friendCategory in friendCategories)
		{
			friendCategory.RefreshItems();
		}
	}

	private void RefreshSearchData()
	{
		if ((Object)(object)searchFriendPopup != (Object)null && searchFriendPopup.IsShowing())
		{
			searchFriendPopup.Refresh(friendsData);
		}
	}

	protected void ClearCurrentList()
	{
		foreach (FriendsCategoryContainer friendCategory in friendCategories)
		{
			Object.Destroy((Object)(object)((Component)friendCategory).gameObject);
		}
		friendCategories.Clear();
		foreach (GameObject otherListComponent in otherListComponents)
		{
			Object.Destroy((Object)(object)otherListComponent);
		}
		otherListComponents.Clear();
	}

	private void OnPlayersStatusesUpdated()
	{
		OnRefreshFriends(null);
	}

	protected async Task LoadFriends(bool forceUpdate)
	{
		friendsData = await AccountManager.GetFriends(forceUpdate);
	}
}
