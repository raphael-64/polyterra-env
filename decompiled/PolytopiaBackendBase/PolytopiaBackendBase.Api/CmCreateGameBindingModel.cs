using System.Collections.Generic;

namespace PolytopiaBackendBase.Api;

public class CmCreateGameBindingModel
{
	public PublicGameSettings settings { get; set; }

	public List<string> UserIds { get; set; }
}
