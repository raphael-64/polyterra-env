using System;

namespace PolytopiaBackendBase.Challengermode.Data;

public class GetLadderMatchesBindingModel
{
	public Guid LadderId { get; set; }

	public Guid UserId { get; set; }
}
