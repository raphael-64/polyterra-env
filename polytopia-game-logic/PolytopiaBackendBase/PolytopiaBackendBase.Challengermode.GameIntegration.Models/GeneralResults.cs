using PolytopiaBackendBase.Game;

namespace PolytopiaBackendBase.Challengermode.GameIntegration.Models;

public class GeneralResults
{
	public GameMode GameMode { get; set; }

	public GameType GameType { get; set; }

	public MapPreset MapPreset { get; set; }

	public int MapSize { get; set; }

	public int CurrentTurnNumber { get; set; }

	public int NumberOfTeams { get; set; }

	public string GameName { get; set; }

	public string ReplayLink { get; set; }

	public int GameVersion { get; set; }
}
