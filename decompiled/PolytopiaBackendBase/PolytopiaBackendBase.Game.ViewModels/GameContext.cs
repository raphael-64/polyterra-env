using System;

namespace PolytopiaBackendBase.Game.ViewModels;

public class GameContext
{
	public Guid? ExternalTournamentId { get; set; }

	public Guid? ExternalMatchId { get; set; }
}
