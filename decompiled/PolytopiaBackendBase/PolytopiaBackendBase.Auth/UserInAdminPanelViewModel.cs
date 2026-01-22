using System;

namespace PolytopiaBackendBase.Auth;

public class UserInAdminPanelViewModel
{
	public Guid UserId { get; set; }

	public string DisplayName { get; set; }

	public int PlayedGames { get; set; }

	public int NumberOfFriends { get; set; }

	public int PlayedMultiplayerGames { get; set; }

	public DateTime LastLoginDate { get; set; }

	public int MultiplayerRating { get; set; }
}
