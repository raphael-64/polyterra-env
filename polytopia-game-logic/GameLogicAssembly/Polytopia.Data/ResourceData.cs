using System.Collections.Generic;
using Newtonsoft.Json;

namespace Polytopia.Data;

public class ResourceData
{
	public enum Type
	{
		None,
		Game,
		Crop,
		Fish,
		Whale,
		Metal,
		Fruit,
		Spores
	}

	public int idx;

	[JsonConverter(typeof(StringIDsToObjectsConverter<TerrainData, TerrainData.Type>))]
	public List<TerrainData> resourceTerrainRequirements = new List<TerrainData>();

	public string displayName => "resource.names." + type.GetName();

	public Type type => (Type)idx;
}
