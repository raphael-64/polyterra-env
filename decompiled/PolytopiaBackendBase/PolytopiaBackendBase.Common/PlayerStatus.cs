using System;
using PolytopiaBackendBase.Game;

namespace PolytopiaBackendBase.Common;

public struct PlayerStatus
{
	public PlayerOnlineStatus PlayerOnlineStatus { get; set; }

	public Guid GameId { get; set; }
}
