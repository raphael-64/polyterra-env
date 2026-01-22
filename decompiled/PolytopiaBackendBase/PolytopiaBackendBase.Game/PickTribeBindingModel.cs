using System;
using System.Collections.Generic;

namespace PolytopiaBackendBase.Game;

public class PickTribeBindingModel
{
	public Guid GameId { get; set; }

	public int TribeType { get; set; }

	public List<int> DisabledTribes { get; set; }

	public int SkinType { get; set; }
}
