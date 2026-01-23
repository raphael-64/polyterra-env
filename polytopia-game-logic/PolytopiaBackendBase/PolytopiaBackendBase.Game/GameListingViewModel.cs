using System.Collections.Generic;

namespace PolytopiaBackendBase.Game;

public class GameListingViewModel : IServerResponseData
{
	public List<GameSummaryViewModel> gameSummaries;

	public List<MatchmakingGameSummaryViewModel> matchmakingGameSummaries;
}
