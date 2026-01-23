using System;

namespace PolytopiaBackendBase.Game;

public class SaveGameBindingModel
{
	public Guid GameId { get; set; }

	public bool Save { get; set; } = true;
}
