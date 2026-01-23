using System.Collections.Generic;
using PolytopiaBackendBase.Game;

namespace PolytopiaBackendBase.Challengermode;

public class ChallengermodeGameStateSummary
{
	public MapPreset MapPreset { get; set; }

	public GameMode GameMode { get; set; }

	public int MapSize { get; set; }

	public List<ChallengermodePlayerStateSummary> PlayerStates { get; set; }

	public string GameName { get; set; }
}
