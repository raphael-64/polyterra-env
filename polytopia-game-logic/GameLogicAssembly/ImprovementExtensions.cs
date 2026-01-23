using Polytopia.Data;

public static class ImprovementExtensions
{
	public static void LevelUp(this ImprovementState state)
	{
		state.level++;
		state.xp -= (short)state.level;
		state.upgrade--;
	}

	public static bool IsTemple(this ImprovementState state)
	{
		return state.type.IsTemple();
	}

	public static bool IsTemple(this ImprovementData.Type improvementType)
	{
		if ((uint)(improvementType - 17) <= 3u || improvementType == ImprovementData.Type.IceTemple)
		{
			return true;
		}
		return false;
	}

	public static bool IsMonument(this ImprovementState state)
	{
		return state.type.IsMonument();
	}

	public static bool IsMonument(this ImprovementData.Type improvementType)
	{
		if ((uint)(improvementType - 23) <= 6u)
		{
			return true;
		}
		return false;
	}

	public static int CalculateImprovementPopulationAtLevel(this ImprovementData improvementData, int level)
	{
		int num = (int)improvementData.GetPopulationReward();
		if (improvementData.maxLevel > 0)
		{
			foreach (GrowthRewards growthReward in improvementData.growthRewards)
			{
				num += growthReward.population * level;
			}
		}
		return num;
	}

	public static int CalculateImprovementPopulationAtLevel(this ImprovementState improvementState, GameState state, int level)
	{
		state.GameLogicData.TryGetData(improvementState.type, out var data);
		return data.CalculateImprovementPopulationAtLevel(level);
	}
}
