public struct PlayerSummaries
{
	public GameStateSummary.GamePlayerSummary Local;

	public GameStateSummary.GamePlayerSummary Current;

	public bool IsLocalPlayerCurrent()
	{
		return Local?.Id == Current?.Id;
	}
}
