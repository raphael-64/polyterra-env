namespace PolytopiaBackendBase.Game;

public class MatchmakingSubmissionViewModel : IServerResponseData
{
	public string GameName;

	public bool IsWaitingForOpponents;

	public MatchmakingGameSummaryViewModel MatchmakingGameSummaryViewModel;
}
