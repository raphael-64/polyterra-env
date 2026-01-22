using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

public class AccountManager
{
	protected static Guid playerAccountId = Guid.Empty;

	private static ServerResponseList<PolytopiaFriendViewModel> friendsErrorResponse;

	private static List<PolytopiaFriendViewModel> friendViewModels;

	private static PlayerData[] friends;

	private static Dictionary<string, PlayerStatus> playersStatuses = new Dictionary<string, PlayerStatus>();

	public static PolytopiaUserViewModel UserModel
	{
		get
		{
			if (PolytopiaBackendAdapter.Instance.IsAuthenticated)
			{
				return PolytopiaBackendAdapter.Instance.ClientUserData.User;
			}
			return CacheManager.GetCachedUserViewModel();
		}
	}

	public static AvatarState AvatarState
	{
		get
		{
			if (PolytopiaBackendAdapter.Instance.IsAuthenticated)
			{
				SerializationHelpers.FromByteArray<AvatarState>(PolytopiaBackendAdapter.Instance.ClientUserData.User.AvatarStateData, out var result);
				return result;
			}
			SerializationHelpers.FromByteArray<AvatarState>(CacheManager.GetCachedUserViewModel()?.AvatarStateData, out var result2);
			return result2;
		}
	}

	public static Guid PlayerAccountId
	{
		get
		{
			Guid? guid = PolytopiaBackendAdapter.Instance?.ClientUserData?.User?.PolytopiaId;
			if (guid.HasValue)
			{
				return guid.Value;
			}
			return playerAccountId;
		}
	}

	public static string Alias
	{
		get
		{
			if (PolytopiaBackendAdapter.Instance.IsAuthenticated && !string.IsNullOrEmpty(PolytopiaBackendAdapter.Instance.ClientUserData.User.Alias))
			{
				return PolytopiaBackendAdapter.Instance.ClientUserData.User.Alias;
			}
			PolytopiaUserViewModel cachedUserViewModel = CacheManager.GetCachedUserViewModel();
			if (cachedUserViewModel != null && !string.IsNullOrEmpty(cachedUserViewModel.Alias))
			{
				return cachedUserViewModel.Alias;
			}
			string text = PolytopiaPlayerPrefs.GetString("playerAlias", string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				return Config.playerName.Value;
			}
			return text;
		}
		set
		{
			PolytopiaPlayerPrefs.SetString("playerAlias", value);
		}
	}

	public static ServerResponseList<PolytopiaFriendViewModel> GetFriendErrorResponse()
	{
		return friendsErrorResponse;
	}

	public static async Task<PlayerData[]> GetFriends(bool forceUpdate = false)
	{
		if ((friends == null || forceUpdate) && !(await TryGetFriendsFromServer(forceUpdate)))
		{
			return new PlayerData[0];
		}
		return friends;
	}

	public static async Task<List<PolytopiaFriendViewModel>> GetFriendViewModels(bool forceUpdate = false)
	{
		if ((friendViewModels == null || forceUpdate) && !(await TryGetFriendsFromServer(forceUpdate)))
		{
			return new List<PolytopiaFriendViewModel>();
		}
		return friendViewModels;
	}

	private static async Task<bool> TryGetFriendsFromServer(bool forceUpdate = false)
	{
		if (PolytopiaBackendAdapter.Instance.IsConnected || forceUpdate)
		{
			ServerResponseList<PolytopiaFriendViewModel> serverResponseList = await PolytopiaBackendAdapter.Instance.GetFriends();
			if (!serverResponseList.Success)
			{
				friendsErrorResponse = serverResponseList;
				return false;
			}
			UpdateFriends(serverResponseList.Data);
			return true;
		}
		return false;
	}

	public static void UpdateFriends(List<PolytopiaFriendViewModel> updatedFriends)
	{
		friendsErrorResponse = null;
		friendViewModels = updatedFriends;
		friends = PlayerDataUtils.PlayerDataFromFriendViewModels(updatedFriends);
		BackendEvents.RefreshFriends(friendViewModels);
		RefreshFriendsStatuses().WrapErrors();
	}

	public static void UpdateFriend(PolytopiaUserViewModel updatedFriend)
	{
		if (friendViewModels == null || friendViewModels.Count <= 0)
		{
			return;
		}
		bool flag = false;
		foreach (PolytopiaFriendViewModel friendViewModel in friendViewModels)
		{
			if (!(friendViewModel.User.PolytopiaId != updatedFriend.PolytopiaId))
			{
				friendViewModel.User = updatedFriend;
				flag = true;
			}
		}
		if (flag)
		{
			friends = PlayerDataUtils.PlayerDataFromFriendViewModels(friendViewModels);
			BackendEvents.RefreshFriends(friendViewModels);
		}
	}

	public static Platform GetPreloadedFriendPlatform(Guid? polytopiaId)
	{
		PolytopiaFriendViewModel friendViewModelWithId = GetFriendViewModelWithId(polytopiaId, friendViewModels);
		if (friendViewModelWithId == null)
		{
			return Platform.Unknown;
		}
		return PlayerDataUtils.GetPlatform(friendViewModelWithId.User.GameVersions);
	}

	public static async Task<PlayerData> GetFriendPlayerDataWithId(Guid? polytopiaId)
	{
		if (!polytopiaId.HasValue)
		{
			return null;
		}
		PlayerData[] array = await GetFriends();
		foreach (PlayerData playerData in array)
		{
			if (playerData.profile.id == polytopiaId.Value)
			{
				return playerData;
			}
		}
		return null;
	}

	public static async Task<PolytopiaFriendViewModel> GetFriendViewModelWithId(Guid? polytopiaId)
	{
		if (!polytopiaId.HasValue)
		{
			return null;
		}
		List<PolytopiaFriendViewModel> list = await GetFriendViewModels();
		return GetFriendViewModelWithId(polytopiaId, list);
	}

	private static PolytopiaFriendViewModel GetFriendViewModelWithId(Guid? polytopiaId, List<PolytopiaFriendViewModel> friends)
	{
		if (!polytopiaId.HasValue || friends == null)
		{
			return null;
		}
		foreach (PolytopiaFriendViewModel friend in friends)
		{
			if (friend.User.PolytopiaId == polytopiaId.Value)
			{
				return friend;
			}
		}
		return null;
	}

	public static int GetFriendCount()
	{
		if (friendViewModels != null)
		{
			int num = 0;
			{
				foreach (PolytopiaFriendViewModel friendViewModel in friendViewModels)
				{
					if (friendViewModel.FriendshipStatus == FriendshipStatus.Accepted)
					{
						num++;
					}
				}
				return num;
			}
		}
		return (UserModel?.NumFriends).GetValueOrDefault();
	}

	public static async Task<int> GetFriendRequestCount()
	{
		await GetFriendViewModels();
		if (friendViewModels != null)
		{
			int num = 0;
			foreach (PolytopiaFriendViewModel friendViewModel in friendViewModels)
			{
				if (friendViewModel.FriendshipStatus == FriendshipStatus.ReceivedRequest)
				{
					num++;
				}
			}
			return num;
		}
		return 0;
	}

	public static void UpdatePlayerStatus(string playerId, PlayerStatus status)
	{
		playersStatuses[playerId] = status;
		BackendEvents.PlayersStatusesUpdated();
	}

	public static void UpdatePlayersStatuses(Dictionary<string, PlayerStatus> statuses)
	{
		foreach (KeyValuePair<string, PlayerStatus> status in statuses)
		{
			playersStatuses[status.Key] = status.Value;
		}
		BackendEvents.PlayersStatusesUpdated();
	}

	public static bool IsPlayerOnline(Guid? playerId)
	{
		if (!playerId.HasValue)
		{
			return false;
		}
		if (playersStatuses.TryGetValue(playerId.ToString(), out var value))
		{
			return value.PlayerOnlineStatus != PlayerOnlineStatus.Offline;
		}
		return false;
	}

	public static bool IsPlayerPlayingCurrentGame(string playerId)
	{
		if (!playersStatuses.ContainsKey(playerId))
		{
			return false;
		}
		if (playersStatuses[playerId].PlayerOnlineStatus != PlayerOnlineStatus.PlayingGame)
		{
			return false;
		}
		Guid gameId = playersStatuses[playerId].GameId;
		Guid? currentGameId = GameManager.Client.CurrentGameId;
		return gameId == currentGameId;
	}

	private static async Task RefreshFriendsStatuses()
	{
		ServerResponse<PlayersStatusesResponse> serverResponse = await PolytopiaBackendAdapter.Instance.GetFriendsStatuses();
		if (serverResponse.Success)
		{
			UpdatePlayersStatuses(serverResponse.Data.Statuses);
		}
	}

	public static PlayerData CreatePlayerDataFromViewModel(PolytopiaUserViewModel viewModel)
	{
		SerializationHelpers.FromByteArray<AvatarState>(viewModel.AvatarStateData, out var result);
		return new PlayerData
		{
			state = PlayerData.State.None,
			profile = new PlayerProfileState
			{
				numFriends = (viewModel.NumFriends.HasValue ? viewModel.NumFriends.Value : 0),
				numMultiplayerGames = (viewModel.NumMultiplayergames.HasValue ? viewModel.NumMultiplayergames.Value : 0),
				gameVersion = PlayerDataUtils.GetCurrentPlatformGameVersion(viewModel.GameVersions),
				multiplayerRating = (viewModel.MultiplayerRating.HasValue ? viewModel.MultiplayerRating.Value : 0),
				lastLoginDate = viewModel.LastLoginDate,
				avatarState = result,
				id = viewModel.PolytopiaId,
				name = viewModel.Alias,
				numGames = (viewModel.NumGames.HasValue ? viewModel.NumGames.Value : 0)
			},
			type = PlayerData.Type.Player
		};
	}
}
