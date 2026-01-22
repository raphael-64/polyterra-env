using Newtonsoft.Json;

namespace Polytopia.Data;

public class TerrainRequirements
{
	[JsonConverter(typeof(StringIDToObjectConverter<TerrainData, TerrainData.Type>))]
	public TerrainData terrain;

	[JsonConverter(typeof(StringIDToObjectConverter<ResourceData, ResourceData.Type>))]
	public ResourceData resource;
}
