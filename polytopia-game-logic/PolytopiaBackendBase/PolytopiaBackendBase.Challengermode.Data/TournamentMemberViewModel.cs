using System;

namespace PolytopiaBackendBase.Challengermode.Data;

public class TournamentMemberViewModel : IServerResponseData
{
	public Guid PolytopiaUserId { get; set; }

	public Guid ChallengermodeUserId { get; set; }

	public bool IsConfirmed { get; set; }
}
