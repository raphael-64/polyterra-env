using System.Collections.Generic;

namespace PolytopiaBackendBase.Game;

public class GetLobbyInvitationsViewModel : IServerResponseData
{
	public List<LobbyGameViewModel> Lobbies { get; set; }
}
