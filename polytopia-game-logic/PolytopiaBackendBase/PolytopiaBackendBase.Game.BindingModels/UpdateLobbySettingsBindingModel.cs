using System;

namespace PolytopiaBackendBase.Game.BindingModels;

public class UpdateLobbySettingsBindingModel
{
	public Guid LobbyId { get; set; }

	public int MapSize { get; set; }

	public MapPreset MapPreset { get; set; }

	public short OpponentCount { get; set; }

	public GameMode GameMode { get; set; }

	public int TimeLimit { get; set; }

	public int ScoreLimit { get; set; }

	public string GameName { get; set; }
}
