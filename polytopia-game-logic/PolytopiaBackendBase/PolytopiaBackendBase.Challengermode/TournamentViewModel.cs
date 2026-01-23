using System;
using System.Collections.Generic;
using PolytopiaBackendBase.Challengermode.Data;

namespace PolytopiaBackendBase.Challengermode;

public class TournamentViewModel : IServerResponseData
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public string ContactUrl { get; set; }

	public string CustomPrizeText { get; set; }

	public DateTime DateCreated { get; set; }

	public DateTime? ScheduledStartTime { get; set; }

	public DateTime? StartTime { get; set; }

	public DateTime? EndTime { get; set; }

	public DateTime? ReadyTime { get; set; }

	public List<TournamentMemberViewModel> Members { get; set; }

	public string OverviewUrl { get; set; }

	public string JoinUrl { get; set; }

	public int AvailableSlots { get; set; }

	public int TotalSlots { get; set; }

	public int NumberRegistered { get; set; }

	public TournamentFormat TournamentFormat { get; set; }

	public TournamentState State { get; set; }

	public int PlayersPerLineup { get; set; }

	public bool OfficialTournament { get; set; }

	public List<TournamentTopMemberViewModel> TopMembers { get; set; }

	public TournamentPersonalViewModel PersonalViewModel { get; set; }

	public override string ToString()
	{
		return "Tournament: " + Name;
	}
}
