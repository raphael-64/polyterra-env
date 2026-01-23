using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

public static class FriendUtils
{
	public static async void PerformFriendAction(Guid friendId, FriendActions action)
	{
		await PerformFriendActionAsync(friendId, action);
	}

	public static async Task<bool> PerformFriendActionAsync(Guid friendId, FriendActions action)
	{
		FriendRequestBindingModel model = new FriendRequestBindingModel
		{
			FriendUserId = friendId
		};
		NetworkUtils.ShowLoader();
		ServerResponse<ResponseViewModel> serverResponse = null;
		switch (action)
		{
		case FriendActions.Add:
			serverResponse = await PolytopiaBackendAdapter.Instance.SendFriendRequest(model);
			break;
		case FriendActions.Remove:
			serverResponse = await PolytopiaBackendAdapter.Instance.RemoveFriend(model);
			break;
		case FriendActions.Accept:
			serverResponse = await PolytopiaBackendAdapter.Instance.AcceptFriendRequest(model);
			break;
		case FriendActions.Decline:
			serverResponse = await PolytopiaBackendAdapter.Instance.RemoveFriend(model);
			break;
		}
		NetworkUtils.HideLoader();
		if (serverResponse != null && !serverResponse.Success)
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
			return false;
		}
		return true;
	}

	public static FriendActions GetActionFromState(PlayerData.State state)
	{
		return state switch
		{
			PlayerData.State.None => FriendActions.Add, 
			PlayerData.State.IsYou => FriendActions.None, 
			PlayerData.State.Accepted => FriendActions.Remove, 
			PlayerData.State.SentRequest => FriendActions.Remove, 
			PlayerData.State.ReceivedRequest => FriendActions.Accept, 
			PlayerData.State.Rejected => FriendActions.Add, 
			_ => FriendActions.None, 
		};
	}

	public static PlayerData[] GetFriendsByCategory(PlayerData[] data, PlayerData.State state)
	{
		List<PlayerData> list = new List<PlayerData>();
		foreach (PlayerData playerData in data)
		{
			if (playerData.state == state)
			{
				list.Add(playerData);
			}
		}
		return list.ToArray();
	}

	public static int GetIndexOfFriend(PlayerData[] data, Guid friendId)
	{
		int num = data.Length;
		for (int i = 0; i < num; i++)
		{
			if (data[i].profile.id == friendId)
			{
				return i;
			}
		}
		return -1;
	}

	public static string GetDescription(PlayerData playerData, string name)
	{
		return string.Format("{0}: {1}\n{2}: {3}\n{4}: {5}\n{6}: {7}\n{8}: {9}", Localization.Get("mplayerstats.alias"), name, Localization.Get("mplayerstats.friends"), playerData.profile.numFriends, Localization.Get("mplayerstats.games"), playerData.profile.numMultiplayerGames, Localization.Get("mplayerstats.gameversion"), playerData.profile.gameVersion, Localization.Get("mplayerstats.elo"), playerData.profile.multiplayerRating);
	}

	public static string GetSpriteStringForFriendIdWithFormat(Guid? friendId, string formatString)
	{
		return GetSpriteStringWithFormatInternal(GetSpriteStringForFriendId(friendId), formatString);
	}

	public static string GetSpriteStringForFriendIdOnPlatformWithFormat(Guid? friendId, Platform platform, string formatString)
	{
		return GetSpriteStringWithFormatInternal(GetSpriteStringForFriendIdWithPlatform(friendId, platform), formatString);
	}

	public static string GetSpriteStringForFriendWithFormat(PlayerData friend, string formatString)
	{
		return GetSpriteStringWithFormatInternal(GetSpriteStringForFriend(friend), formatString);
	}

	private static string GetSpriteStringWithFormatInternal(string spriteString, string formatString)
	{
		if (spriteString == string.Empty)
		{
			return spriteString;
		}
		return string.Format(formatString, spriteString);
	}

	public static string GetSpriteStringForFriend(PlayerData friend)
	{
		return GetSpriteStringForFriendInternal(AccountManager.IsPlayerOnline(friend.profile.id), friend.profile.platform);
	}

	public static string GetSpriteStringForFriendId(Guid? friendId)
	{
		bool isOnline = AccountManager.IsPlayerOnline(friendId);
		Platform preloadedFriendPlatform = AccountManager.GetPreloadedFriendPlatform(friendId);
		return GetSpriteStringForFriendInternal(isOnline, preloadedFriendPlatform);
	}

	public static string GetSpriteStringForFriendIdWithPlatform(Guid? friendId, Platform platform)
	{
		return GetSpriteStringForFriendInternal(AccountManager.IsPlayerOnline(friendId), platform);
	}

	private static string GetSpriteStringForFriendInternal(bool isOnline, Platform platform)
	{
		string arg = (isOnline ? "1CBA21" : "FFFFFF");
		if (platform.IsMobile())
		{
			return $"<sprite=\"mobile_platform\" index=0 color=#{arg}>";
		}
		if (platform.IsComputer())
		{
			return $"<sprite=\"computer_platform\" index=0 color=#{arg}>";
		}
		if (isOnline)
		{
			return $"<sprite=\"circle_30\" index=0 color=#{arg}>";
		}
		return "";
	}
}
