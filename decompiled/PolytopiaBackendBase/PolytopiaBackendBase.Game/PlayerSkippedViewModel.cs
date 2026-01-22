using System;

namespace PolytopiaBackendBase.Game;

public class PlayerSkippedViewModel
{
	public Guid GameId { get; set; }

	public Guid SkippedUserId { get; set; }

	public Guid? SkippedBy { get; set; }

	public int SkipCount { get; set; }

	public int MaxSkipCount { get; set; }

	public bool WasAutoSkip { get; set; }
}
