using System;
using System.Collections.Generic;

namespace PolytopiaBackendBase.Game.BindingModels;

public class SubscribeToLobbyBindingModel
{
	public bool Subscribe { get; set; }

	public List<Guid> LobbyIds { get; set; }
}
