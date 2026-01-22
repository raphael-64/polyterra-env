namespace Polytopia.Data;

public static class ImprovementDataExtensions
{
	public static string GetDisplayName(this ImprovementData.Type type)
	{
		return "building.ability." + EnumCache<ImprovementData.Type>.GetName(type);
	}
}
