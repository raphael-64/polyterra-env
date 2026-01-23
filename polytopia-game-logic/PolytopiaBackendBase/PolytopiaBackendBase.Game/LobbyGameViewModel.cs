using System;
using System.Collections.Generic;
using PolytopiaBackendBase.Game.ViewModels;

namespace PolytopiaBackendBase.Game;

public class LobbyGameViewModel : IServerResponseData
{
	public Guid Id { get; set; }

	public LobbyUpdatedReason UpdatedReason { get; set; }

	public DateTime? DateCreated { get; set; }

	public DateTime? DateModified { get; set; }

	public string Name { get; set; }

	public MapPreset MapPreset { get; set; }

	public int MapSize { get; set; }

	public short OpponentCount { get; set; }

	public GameMode GameMode { get; set; }

	public Guid OwnerId { get; set; }

	public List<int> DisabledTribes { get; set; }

	public Guid? StartedGameId { get; set; }

	public bool IsPersistent { get; set; }

	public bool IsSharable { get; set; }

	public int TimeLimit { get; set; }

	public int ScoreLimit { get; set; }

	public string InviteLink { get; set; }

	public long? MatchmakingGameId { get; set; }

	public Guid? ChallengermodeGameId { get; set; }

	public DateTime? StartTime { get; set; }

	public GameContext GameContext { get; set; }

	public List<ParticipatorViewModel> Participators { get; set; }

	public List<int> Bots { get; set; }
}
