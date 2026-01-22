namespace Polytopia.Data;

public static class ImprovementAbilityExtensions
{
	public static string GetDisplayName(this ImprovementAbility.Type type)
	{
		return "building.ability." + EnumCache<ImprovementAbility.Type>.GetName(type);
	}
}
