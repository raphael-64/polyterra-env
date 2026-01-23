using System.Collections.Generic;

namespace PolytopiaBackendBase.Game;

public class CreateGameBindingModel
{
	public bool UseLowestGameVersionAmongPlayers { get; set; }

	public int Version { get; set; }

	public List<PlayerBindingModel> Players { get; set; }

	public byte[] GameSettingsData { get; set; }
}
