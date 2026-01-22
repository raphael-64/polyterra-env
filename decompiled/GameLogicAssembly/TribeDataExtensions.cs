using Polytopia.Data;

public static class TribeDataExtensions
{
	public static bool HasUnitAbility(this TribeData tribe, UnitAbility.Type ability, GameState gameState)
	{
		foreach (TechData item in gameState.GameLogicData.GetAllTechForTribe(tribe))
		{
			foreach (UnitData unitUnlock in item.unitUnlocks)
			{
				if (gameState.GameLogicData.GetOverride(unitUnlock, tribe).unitAbilities.Contains(ability))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasAbility(this TribeData tribe, TribeAbility.Type ability)
	{
		foreach (TribeAbility.Type tribeAbility in tribe.tribeAbilities)
		{
			if (tribeAbility == ability)
			{
				return true;
			}
		}
		return false;
	}
}
