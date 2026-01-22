using System;
using PolytopiaBackendBase.Common;

namespace PolytopiaBackendBase.Challengermode.GameIntegration.BindingModels;

public class CmStartOAuthFlowBindingModel
{
	public Platform Platform { get; set; }

	public Guid? LadderId { get; set; }

	public Guid? TournamentId { get; set; }
}
