using System;
using System.Collections.Generic;
using System.Linq;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

public static class PlayerDataUtils
{
	public static PlayerData GetPlayerData(PolytopiaFriendViewModel friendViewModel)
	{
		SerializationHelpers.FromByteArray<AvatarState>(friendViewModel.User.AvatarStateData, out var result);
		PlayerData playerData = new PlayerData
		{
			type = PlayerData.Type.Friend,
			state = GetStateFromStatus(friendViewModel.FriendshipStatus)
		};
		playerData.profile.id = friendViewModel.User.PolytopiaId;
		playerData.profile.name = friendViewModel.User.Alias;
		playerData.profile.avatarState = result;
		playerData.profile.numFriends = friendViewModel.User.NumFriends.GetValueOrDefault();
		playerData.profile.numGames = friendViewModel.User.NumGames.GetValueOrDefault();
		playerData.profile.numMultiplayerGames = friendViewModel.User.NumMultiplayergames.GetValueOrDefault();
		playerData.profile.gameVersion = GetCurrentPlatformGameVersion(friendViewModel.User.GameVersions);
		playerData.profile.multiplayerRating = friendViewModel.User.Elo;
		playerData.profile.lastLoginDate = friendViewModel.User.LastLoginDate;
		playerData.profile.platform = GetPlatform(friendViewModel.User.GameVersions);
		if (friendViewModel.User.Victories == null)
		{
			playerData.profile.victories = new Dictionary<Guid, int> { 
			{
				friendViewModel.User.PolytopiaId,
				0
			} };
		}
		else
		{
			playerData.profile.victories = friendViewModel.User.Victories.ToDictionary((KeyValuePair<string, int> x) => new Guid(x.Key), (KeyValuePair<string, int> y) => y.Value);
		}
		if (friendViewModel.User.Defeats == null)
		{
			playerData.profile.defeats = new Dictionary<Guid, int> { 
			{
				friendViewModel.User.PolytopiaId,
				0
			} };
		}
		else
		{
			playerData.profile.defeats = friendViewModel.User.Defeats.ToDictionary((KeyValuePair<string, int> x) => new Guid(x.Key), (KeyValuePair<string, int> y) => y.Value);
		}
		return playerData;
	}

	public static PlayerData[] PlayerDataFromFriendViewModels(List<PolytopiaFriendViewModel> friendViewModels)
	{
		PlayerData[] array = new PlayerData[friendViewModels.Count];
		for (int i = 0; i < friendViewModels.Count; i++)
		{
			array[i] = GetPlayerData(friendViewModels[i]);
		}
		return array;
	}

	public static PlayerData GetPlayerData(PolytopiaUserViewModel userViewModel)
	{
		SerializationHelpers.FromByteArray<AvatarState>(userViewModel.AvatarStateData, out var result);
		PlayerData playerData = new PlayerData
		{
			type = PlayerData.Type.Friend,
			state = PlayerData.State.None
		};
		playerData.profile.id = userViewModel.PolytopiaId;
		playerData.profile.name = userViewModel.Alias;
		playerData.profile.avatarState = result;
		playerData.profile.numFriends = userViewModel.NumFriends.GetValueOrDefault();
		playerData.profile.numGames = userViewModel.NumGames.GetValueOrDefault();
		playerData.profile.numMultiplayerGames = userViewModel.NumMultiplayergames.GetValueOrDefault();
		playerData.profile.gameVersion = GetCurrentPlatformGameVersion(userViewModel.GameVersions);
		playerData.profile.multiplayerRating = userViewModel.Elo;
		playerData.profile.platform = GetPlatform(userViewModel.GameVersions);
		if (userViewModel.Victories == null)
		{
			playerData.profile.victories = new Dictionary<Guid, int> { { userViewModel.PolytopiaId, 0 } };
		}
		else
		{
			playerData.profile.victories = userViewModel.Victories.ToDictionary((KeyValuePair<string, int> x) => new Guid(x.Key), (KeyValuePair<string, int> y) => y.Value);
		}
		if (userViewModel.Defeats == null)
		{
			playerData.profile.defeats = new Dictionary<Guid, int> { { userViewModel.PolytopiaId, 0 } };
		}
		else
		{
			playerData.profile.defeats = userViewModel.Defeats.ToDictionary((KeyValuePair<string, int> x) => new Guid(x.Key), (KeyValuePair<string, int> y) => y.Value);
		}
		return playerData;
	}

	public static int GetCurrentPlatformGameVersion(List<ClientGameVersionViewModel> gameVersions)
	{
		int num = -1;
		if (gameVersions == null)
		{
			return num;
		}
		foreach (ClientGameVersionViewModel gameVersion in gameVersions)
		{
			num = Math.Max(num, gameVersion.GameVersion);
		}
		return num;
	}

	public static Platform GetPlatform(List<ClientGameVersionViewModel> gameVersions)
	{
		if (gameVersions == null || gameVersions.Count == 0)
		{
			return Platform.Unknown;
		}
		return gameVersions[0].Platform;
	}

	public static PlayerData.State GetStateFromStatus(FriendshipStatus status)
	{
		return status switch
		{
			FriendshipStatus.None => PlayerData.State.None, 
			FriendshipStatus.SentRequest => PlayerData.State.SentRequest, 
			FriendshipStatus.ReceivedRequest => PlayerData.State.ReceivedRequest, 
			FriendshipStatus.Accepted => PlayerData.State.Accepted, 
			FriendshipStatus.Rejected => PlayerData.State.Rejected, 
			_ => PlayerData.State.None, 
		};
	}
}
