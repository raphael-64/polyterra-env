using System;

namespace PolytopiaBackendBase.Game;

public class SendCommandBindingModel
{
	public Guid GameId { get; set; }

	public PolytopiaCommandViewModel Command { get; set; }
}
