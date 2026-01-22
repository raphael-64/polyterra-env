using System;
using System.Collections.Generic;
using PolytopiaBackendBase.Game.ViewModels;

namespace PolytopiaBackendBase.Game;

public class GameSummaryViewModel : IServerResponseData
{
	public Guid GameId { get; set; }

	public long? MatchmakingGameId { get; set; }

	public Guid? OwnerId { get; set; }

	public DateTime? DateCreated { get; set; }

	public DateTime? DateLastCommand { get; set; }

	public DateTime? DateLastEndTurn { get; set; }

	public DateTime? DateEnded { get; set; }

	public int TimeLimit { get; set; }

	public GameSessionState State { get; set; }

	public List<ParticipatorViewModel> Participators { get; set; }

	public GameResultViewModel Result { get; set; }

	public byte[] GameSummaryData { get; set; }

	public DateTime? ReminderSent { get; set; }

	public GameContext GameContext { get; set; }
}
