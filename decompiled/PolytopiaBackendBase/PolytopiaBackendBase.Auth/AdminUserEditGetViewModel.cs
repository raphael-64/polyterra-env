using System;

namespace PolytopiaBackendBase.Auth;

public class AdminUserEditGetViewModel : IServerResponseData
{
	public Guid UserId { get; set; }

	public string DisplayName { get; set; }

	public int PlayedGames { get; set; }

	public int NumberOfFriends { get; set; }

	public int PlayedMultiplayerGames { get; set; }

	public DateTime LastLoginDate { get; set; }

	public int MultiplayerRating { get; set; }

	public string UnlockedTribes { get; set; }

	public string UnlockedSkins { get; set; }

	public string Games { get; set; }

	public string Friendships { get; set; }
}
