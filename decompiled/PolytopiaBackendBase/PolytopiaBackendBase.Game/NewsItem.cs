using Newtonsoft.Json;

namespace PolytopiaBackendBase.Game;

public class NewsItem
{
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public int Id { get; set; }

	public long? Date { get; set; }

	public string Body { get; set; }

	public string Image { get; set; }

	public string Link { get; set; }
}
