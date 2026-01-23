using System;

namespace PolytopiaBackendBase.Game;

public class PlayerRankingViewModel
{
	public Guid? PolytopiaUserId { get; set; }

	public byte TribeType { get; set; }

	public byte Rank { get; set; }

	public int GameVersion { get; set; }

	public uint Score { get; set; }

	public int ResultingMultiplayerRating { get; set; }

	public int MultiplayerRatingChange { get; set; }
}
