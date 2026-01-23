using PolytopiaBackendBase.Common;

namespace PolytopiaBackendBase.Game;

public class SubmitMatchmakingBindingModel
{
	public int Version { get; set; }

	public int MapSize { get; set; }

	public MapPreset MapPreset { get; set; }

	public short OpponentCount { get; set; }

	public GameMode GameMode { get; set; }

	public int ScoreLimit { get; set; }

	public int SelectedTribe { get; set; }

	public int TimeLimit { get; set; }

	public Platform Platform { get; set; } = Platform.None;

	public bool AllowCrossPlay { get; set; }

	public bool UseLobbies { get; set; }
}
