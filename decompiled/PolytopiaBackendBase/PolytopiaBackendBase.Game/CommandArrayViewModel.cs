using System;
using System.Collections.Generic;

namespace PolytopiaBackendBase.Game;

public class CommandArrayViewModel
{
	public Guid GameId { get; set; }

	public List<PolytopiaCommandViewModel> Commands { get; set; }
}
