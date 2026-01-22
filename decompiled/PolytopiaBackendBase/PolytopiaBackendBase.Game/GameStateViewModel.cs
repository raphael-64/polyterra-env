using System;
using System.Collections.Generic;

namespace PolytopiaBackendBase.Game;

public class GameStateViewModel
{
	public Guid GameId { get; set; }

	public byte[] SerializedGameState { get; set; }

	public List<PolytopiaCommandViewModel> ExecutedCommands { get; set; }
}
