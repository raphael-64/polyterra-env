namespace PolytopiaBackendBase.Challengermode.Data;

public class TournamentPersonalViewModel
{
	public bool HasSignedUp { get; set; }

	public bool HasConfirmed { get; set; }

	public bool OnWaitingList { get; set; }

	public bool MatchRunning { get; set; }

	public bool WaitingForNextMatch { get; set; }

	public bool PariticipationComplete { get; set; }

	public int? BestPlacment { get; set; }

	public int? WorstPlacement { get; set; }

	public static TournamentPersonalViewModel None => new TournamentPersonalViewModel
	{
		HasSignedUp = false,
		HasConfirmed = false,
		MatchRunning = false,
		OnWaitingList = false,
		WaitingForNextMatch = false,
		PariticipationComplete = false,
		BestPlacment = null,
		WorstPlacement = null
	};
}
