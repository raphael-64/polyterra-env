namespace Polytopia.Data;

public static class PlayerAbilityExtensions
{
	public static string GetDisplayName(this PlayerAbility.Type type)
	{
		return "player.abilities." + type.GetName();
	}

	public static string GetDescription(this PlayerAbility.Type type)
	{
		return "tooltip.ability." + type.GetName();
	}
}
