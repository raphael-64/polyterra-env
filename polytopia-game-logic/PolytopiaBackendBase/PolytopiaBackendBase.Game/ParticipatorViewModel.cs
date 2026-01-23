using System;
using System.Collections.Generic;
using PolytopiaBackendBase.Auth;

namespace PolytopiaBackendBase.Game;

public class ParticipatorViewModel
{
	public Guid UserId { get; set; }

	public string Name { get; set; }

	public int NumberOfFriends { get; set; }

	public int NumberOfMultiplayerGames { get; set; }

	public List<ClientGameVersionViewModel> GameVersion { get; set; }

	public int MultiplayerRating { get; set; }

	public DateTime? DateLastCommand { get; set; }

	public DateTime? DateLastStartTurn { get; set; }

	public DateTime? DateLastEndTurn { get; set; }

	public DateTime? DateCurrentTurnDeadline { get; set; }

	public TimeSpan? TimeBank { get; set; }

	public TimeSpan? LastConsumedTimeBank { get; set; }

	public PlayerInvitationState InvitationState { get; set; }

	public int SelectedTribe { get; set; }

	public bool HasFailedParse { get; set; }

	public byte[] AvatarStateData { get; set; }

	public int AutoSkipStrikeCount { get; set; }
}
