using Newtonsoft.Json;

namespace Polytopia.Data;

public class Creates
{
	[JsonConverter(typeof(StringIDToObjectConverter<TerrainData, TerrainData.Type>))]
	public TerrainData terrain;

	[JsonConverter(typeof(StringIDToObjectConverter<ResourceData, ResourceData.Type>))]
	public ResourceData resource;

	[JsonConverter(typeof(StringIDToObjectConverter<UnitData, UnitData.Type>))]
	public UnitData unit;
}
