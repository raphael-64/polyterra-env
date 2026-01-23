using System.Collections.Generic;

namespace PolytopiaBackendBase.Challengermode.Data;

public class LadderPlacementsViewModel : IServerResponseData
{
	public List<LadderPlacementViewModel> Placements { get; set; }
}
