using System.Collections.Generic;
using Newtonsoft.Json;
using PolytopiaBackendBase.Common;

namespace PolytopiaBackendBase.Auth;

public class PlayersStatusesResponse : IServerResponseData
{
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public Dictionary<string, PlayerStatus> Statuses { get; set; }
}
