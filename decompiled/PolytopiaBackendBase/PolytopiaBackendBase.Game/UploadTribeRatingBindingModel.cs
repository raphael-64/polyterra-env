using System.Collections.Generic;

namespace PolytopiaBackendBase.Game;

public class UploadTribeRatingBindingModel
{
	public Dictionary<int, TribeRatingViewModel> Entries { get; set; }
}
