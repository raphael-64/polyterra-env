using System;

namespace PolytopiaBackendBase.Game;

public class RespondToInvitationBindingModel
{
	public Guid GameId { get; set; }

	public bool Accepted { get; set; }
}
