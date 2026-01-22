using Newtonsoft.Json;

namespace Polytopia.Data;

public class AdjacencyImprovements
{
	[JsonConverter(typeof(StringIDToObjectConverter<ImprovementData, ImprovementData.Type>))]
	public ImprovementData improvement;

	[JsonConverter(typeof(StringIDToObjectConverter<ResourceData, ResourceData.Type>))]
	public ResourceData resources;
}
