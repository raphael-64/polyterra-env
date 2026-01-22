using System;

namespace PolytopiaBackendBase.Game.BindingModels;

public class RemindPlayerBindingModel
{
	public Guid GameId { get; set; }

	public Guid UserId { get; set; }
}
