using Newtonsoft.Json;

namespace Polytopia.Data;

public class AdjacencyRequirements
{
	[JsonConverter(typeof(StringIDToObjectConverter<TerrainData, TerrainData.Type>))]
	public TerrainData terrain;

	[JsonConverter(typeof(StringIDToObjectConverter<ImprovementData, ImprovementData.Type>))]
	public ImprovementData improvement;

	[JsonConverter(typeof(StringIDToObjectConverter<ResourceData, ResourceData.Type>))]
	public ResourceData resource;
}
