using System;
using System.Collections.Generic;
using PolytopiaBackendBase.Auth;

namespace PolytopiaBackendBase.Challengermode.Data;

public class TournamentTopMemberViewModel : IServerResponseData
{
	public Guid PolytopiaUserId { get; set; }

	public string UserName { get; set; }

	public string Alias { get; set; }

	public Guid ChallengermodeUserId { get; set; }

	public bool IsConfirmed { get; set; }

	public int? NumFriends { get; set; }

	public int? NumGames { get; set; }

	public int? NumMultiplayergames { get; set; }

	public int? MultiplayerRating { get; set; }

	public List<ClientGameVersionViewModel> GameVersions { get; set; }

	public DateTime? LastLoginDate { get; set; }

	public byte[] AvatarStateData { get; set; }
}
