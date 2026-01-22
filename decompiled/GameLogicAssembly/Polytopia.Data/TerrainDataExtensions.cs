namespace Polytopia.Data;

public static class TerrainDataExtensions
{
	public static string GetDisplayName(this TerrainData.Type type)
	{
		return "terrain." + EnumCache<TerrainData.Type>.GetName(type);
	}
}
