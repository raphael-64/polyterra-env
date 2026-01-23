using System;

namespace PolytopiaBackendBase.Game;

public class SetParticipationHasFailedParseBindingModel
{
	public Guid GameId { get; set; }

	public bool HasFailedParse { get; set; }
}
