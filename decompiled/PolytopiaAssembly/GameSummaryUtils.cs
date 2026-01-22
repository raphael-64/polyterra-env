using Polytopia.Data;
using PolytopiaBackendBase.Game;

public static class GameSummaryUtils
{
	public static PlayerSummaries GetPlayerSummaries(GameStateSummary summary, GameSummaryViewModel summaryViewModel)
	{
		PlayerSummaries result = default(PlayerSummaries);
		foreach (GameStateSummary.GamePlayerSummary playerSummary in summary.PlayerSummaries)
		{
			if (playerSummary.TribeType == TribeData.Type.Nature)
			{
				continue;
			}
			bool flag = playerSummary.PolytopiaId == AccountManager.PlayerAccountId;
			if (flag)
			{
				result.Local = playerSummary;
			}
			if (summaryViewModel.State == GameSessionState.ReadyToStart)
			{
				if ((!summaryViewModel.OwnerId.HasValue && flag) || playerSummary.PolytopiaId == summaryViewModel.OwnerId)
				{
					result.Current = playerSummary;
				}
			}
			else if (playerSummary.Id == summary.CurrentPlayer)
			{
				result.Current = playerSummary;
			}
		}
		return result;
	}
}
