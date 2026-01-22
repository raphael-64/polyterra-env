using Polytopia.Data;

public static class BattleHelpers
{
	public static BattleResults GetBattleResults(GameState gameState, UnitState attackingUnit, UnitState defendingUnit)
	{
		BattleResults result = new BattleResults
		{
			retaliationDamage = -1
		};
		long num = 45L;
		gameState.GameLogicData.TryGetData(attackingUnit.type, out var data);
		long num2 = attackingUnit.GetMaxHealth(gameState);
		if (num2 <= 0)
		{
			Log.Error("Attacker of type {1} max health is {0}", new object[2] { num2, attackingUnit.type });
			return result;
		}
		long num3 = defendingUnit.GetMaxHealth(gameState);
		if (num3 <= 0)
		{
			Log.Error("Defender of type {1} max health is {0}", new object[2] { num3, defendingUnit.type });
			return result;
		}
		long num4 = attackingUnit.GetAttack(gameState) * attackingUnit.health * 100 / num2;
		long num5 = defendingUnit.GetDefence(gameState) * defendingUnit.health * 100 / num3;
		long num6 = num4 + num5;
		if (num6 <= 0)
		{
			Log.Error("Total damage is {0}, attacking unit of type {1} and force {2}, defending unit of type {3} and defence force {4}", new object[5] { num6, attackingUnit.type, num4, defendingUnit.type, num5 });
			return result;
		}
		result.attackDamage = Round((int)(num4 * attackingUnit.GetAttack(gameState) * num * 10 / (1000 * num6)));
		bool num7 = defendingUnit.health <= result.attackDamage;
		if (!num7 && CanDefenderRetaliate(gameState, attackingUnit, defendingUnit))
		{
			long num8 = defendingUnit.GetDefenceBonus(gameState);
			result.retaliationDamage = Round((int)(num5 * defendingUnit.GetDefence(gameState) * num * 100 / (1000 * num6 * num8)));
		}
		result.shouldMoveToDefeatedEnemyTile = false;
		if (num7 && data.GetRange() == 1)
		{
			PathFinderSettings settings = PathFinderSettings.CreateForUnit(attackingUnit, gameState);
			settings.shouldAllowOccupiedTiles = true;
			result.shouldMoveToDefeatedEnemyTile = gameState.Map.GetPath(attackingUnit.coordinates, defendingUnit.coordinates, attackingUnit.GetMovement(gameState), settings) != null;
		}
		return result;
	}

	private static bool CanDefenderRetaliate(GameState gameState, UnitState attackingUnit, UnitState defendingUnit)
	{
		gameState.GameLogicData.TryGetData(defendingUnit.type, out var data);
		if (defendingUnit.GetAttack(gameState) == 0)
		{
			return false;
		}
		if (attackingUnit.HasAbility(UnitAbility.Type.Convert, gameState))
		{
			return false;
		}
		if (attackingUnit.HasAbility(UnitAbility.Type.Surprise, gameState))
		{
			return false;
		}
		if (gameState.Version <= 17)
		{
			if (attackingUnit.HasEffect(UnitEffect.Frozen))
			{
				return false;
			}
			if (defendingUnit.HasEffect(UnitEffect.Frozen))
			{
				return false;
			}
		}
		else
		{
			if (defendingUnit.HasEffect(UnitEffect.Frozen))
			{
				return false;
			}
			if (data.weapon == UnitData.WeaponEnum.IceArrow)
			{
				return false;
			}
		}
		return defendingUnit.GetAttackOptions(gameState, data.GetRange(), ignoreDiplomacyRelation: true).Contains(attackingUnit.coordinates);
	}

	public static int Round(int n)
	{
		int num = n / 10 * 10;
		int num2 = num + 10;
		if (n - num < num2 - n)
		{
			return num;
		}
		return num2;
	}
}
