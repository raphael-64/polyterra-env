using System;

namespace PolytopiaBackendBase.Challengermode.Data;

public class LadderPlacementViewModel
{
	public Guid UserId { get; set; }

	public int Placement { get; set; }

	public string DateCreated { get; set; }

	public double Score { get; set; }

	public string PolytopiaId { get; set; }

	public string DisplayName { get; set; }
}
