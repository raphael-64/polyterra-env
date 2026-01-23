using System;
using System.Collections.Generic;

namespace PolytopiaBackendBase.Game.BindingModels;

public class ModifyPlayersInLobbyBindingModel
{
	public Guid LobbyId { get; set; }

	public List<Guid> InvitePlayers { get; set; }

	public List<int> Bots { get; set; }

	public List<Guid> RemovePlayers { get; set; }
}
