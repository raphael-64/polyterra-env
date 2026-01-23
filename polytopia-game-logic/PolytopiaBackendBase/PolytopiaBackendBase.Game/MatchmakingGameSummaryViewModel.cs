using System;
using System.Collections.Generic;

namespace PolytopiaBackendBase.Game;

public class MatchmakingGameSummaryViewModel : IServerResponseData
{
	public long Id { get; set; }

	public DateTime? DateCreated { get; set; }

	public DateTime? DateModified { get; set; }

	public string Name { get; set; }

	public MapPreset MapPreset { get; set; }

	public int MapSize { get; set; }

	public short OpponentCount { get; set; }

	public GameMode GameMode { get; set; }

	public bool WithPickedTribe { get; set; }

	public Guid? LobbyId { get; set; }

	public List<ParticipatorViewModel> Participators { get; set; }
}
