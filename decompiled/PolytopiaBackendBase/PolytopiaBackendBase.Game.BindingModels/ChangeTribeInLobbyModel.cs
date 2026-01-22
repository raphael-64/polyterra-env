using System;

namespace PolytopiaBackendBase.Game.BindingModels;

public class ChangeTribeInLobbyModel
{
	public Guid LobbyId { get; set; }

	public int TribeId { get; set; }
}
