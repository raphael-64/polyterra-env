using System.Collections.Generic;

namespace PolytopiaBackendBase.Challengermode.Data;

public class LadderMatchesViewModel : IServerResponseData
{
	public List<LadderMatchViewModel> Matches { get; set; }
}
