using System;

namespace PolytopiaBackendBase.Game.BindingModels;

public class RespondToLobbyInvitation
{
	public Guid LobbyId { get; set; }

	public bool Accepted { get; set; }

	public int TribeId { get; set; }

	public int TribeSkinId { get; set; }
}
