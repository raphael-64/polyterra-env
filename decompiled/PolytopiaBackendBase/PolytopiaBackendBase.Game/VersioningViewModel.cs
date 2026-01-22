using System.Collections.Generic;
using Newtonsoft.Json;

namespace PolytopiaBackendBase.Game;

public class VersioningViewModel : IServerResponseData
{
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public List<VersionEnabledStatus> VersionEnabledStatuses { get; set; }

	public string SystemMessage { get; set; }
}
