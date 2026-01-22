using System.Collections.Generic;
using Newtonsoft.Json;

namespace PolytopiaBackendBase.Game;

public class NewsObject : IServerResponseData
{
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public List<NewsItem> News { get; set; }
}
