using System.Collections.Generic;

namespace PolytopiaBackendBase.Challengermode;

public class TournamentListViewModel : IServerResponseData
{
	public List<TournamentViewModel> Tournaments { get; set; }
}
