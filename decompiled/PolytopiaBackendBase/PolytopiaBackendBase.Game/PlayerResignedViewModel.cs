using System;

namespace PolytopiaBackendBase.Game;

public class PlayerResignedViewModel
{
	public GameSummaryViewModel GameSummary { get; set; }

	public Guid? KickerId { get; set; }

	public Guid Resignee { get; set; }

	public bool Kicked { get; set; }
}
