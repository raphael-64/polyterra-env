namespace Polytopia.Data;

public class TerrainData
{
	public enum Type
	{
		None,
		Water,
		Ocean,
		Field,
		Mountain,
		Forest,
		Ice
	}

	public int idx;

	public string displayName => $"terrain.{type.GetName()}";

	public Type type => (Type)idx;
}
