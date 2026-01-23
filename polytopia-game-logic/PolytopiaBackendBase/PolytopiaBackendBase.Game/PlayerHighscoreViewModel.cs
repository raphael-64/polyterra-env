using System;

namespace PolytopiaBackendBase.Game;

public class PlayerHighscoreViewModel
{
	public Guid? PolytopiaUserId { get; set; }

	public int TribeType { get; set; }

	public uint Score { get; set; }
}
