using System;

namespace PolytopiaBackendBase.Game;

public class PlayerBindingModel
{
	public Guid? UserId { get; set; }

	public bool? AutoPlay { get; set; }

	public string PlayerName { get; set; }

	public int Handicap { get; set; }

	public int Tribe { get; set; }

	public int Skin { get; set; }
}
