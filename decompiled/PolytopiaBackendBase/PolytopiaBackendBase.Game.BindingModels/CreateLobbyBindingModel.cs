using System;
using System.Collections.Generic;
using PolytopiaBackendBase.Common;

namespace PolytopiaBackendBase.Game.BindingModels;

public class CreateLobbyBindingModel
{
	public int Version { get; set; }

	public int MapSize { get; set; }

	public MapPreset MapPreset { get; set; }

	public short OpponentCount { get; set; }

	public GameMode GameMode { get; set; }

	public string GameName { get; set; }

	public int OwnerTribe { get; set; }

	public int OwnerTribeSkin { get; set; }

	public List<int> DisabledTribes { get; set; } = new List<int>();

	public int TimeLimit { get; set; }

	public int ScoreLimit { get; set; }

	public bool IsPersistent { get; set; }

	public long? MatchmakingGameId { get; set; }

	public Platform Platform { get; set; } = Platform.None;

	public List<PlayerBindingModel> Invitations { get; set; }

	public Guid? ChallengermodeGameId { get; set; }

	public Guid? ChallengermodeTournamentId { get; set; }

	public DateTime? StartTime { get; set; }
}
