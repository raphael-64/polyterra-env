using System;
using System.Collections.Generic;
using Polytopia.Data;

public static class ImprovementDataExtensions
{
	public static bool HasAbility(this ImprovementData data, ImprovementAbility.Type ability)
	{
		if (data.improvementAbilities != null)
		{
			foreach (ImprovementAbility.Type improvementAbility in data.improvementAbilities)
			{
				if (improvementAbility == ability)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsRouteOpener(this ImprovementData data)
	{
		return data.routes.Count > 0;
	}

	public static CityReward[] GetCityRewardsForLevel(this ImprovementData data, int level)
	{
		int num = Math.Min(level - 1, CityRewardData.cityRewards.Length / 2 - 1) * 2;
		return new CityReward[2]
		{
			CityRewardData.cityRewards[num],
			CityRewardData.cityRewards[num + 1]
		};
	}

	public static bool HasReward(this ImprovementState state, CityReward reward)
	{
		if (state == null || state.rewards == null || state.rewards.Count == 0)
		{
			return false;
		}
		return state.rewards.Contains(reward);
	}

	public static int RewardCount(this ImprovementState state, CityReward reward)
	{
		if (state.rewards == null)
		{
			return 0;
		}
		int num = 0;
		foreach (CityReward reward2 in state.rewards)
		{
			if (reward2 == reward)
			{
				num++;
			}
		}
		return num;
	}

	public static void AddReward(this ImprovementState state, CityReward reward)
	{
		if (state != null)
		{
			if (state.rewards == null)
			{
				state.rewards = new List<CityReward>();
			}
			state.rewards.Add(reward);
		}
	}

	public static bool CanCreateUnit(this ImprovementData data)
	{
		if (data.creates != null && data.creates.Count > 0)
		{
			foreach (Creates create in data.creates)
			{
				if (create.unit != null && create.unit.type != UnitData.Type.None)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool ShouldLevelUp(this ImprovementState state)
	{
		return state.xp > state.level;
	}
}
