using System;
using System.Collections.Generic;

namespace PolytopiaBackendBase.Game;

public class TribeRatingsViewModel : IServerResponseData
{
	public Guid PolytopiaUserId { get; set; }

	public Dictionary<int, TribeRatingViewModel> Ratings { get; set; }
}
