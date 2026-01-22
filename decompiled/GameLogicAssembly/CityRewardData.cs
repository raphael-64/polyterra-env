public static class CityRewardData
{
	public static CityReward[] cityRewards = new CityReward[8]
	{
		CityReward.Workshop,
		CityReward.Explorer,
		CityReward.CityWall,
		CityReward.Resources,
		CityReward.PopulationGrowth,
		CityReward.BorderGrowth,
		CityReward.Park,
		CityReward.SuperUnit
	};

	public static CityReward[] infiltrateRewards = new CityReward[2]
	{
		CityReward.StealResources,
		CityReward.Rebellion
	};

	public const int PARK_SCORE = 250;

	public const int RESOURCE_REWARD = 5;

	public const int POPULATION_REWARD = 3;

	public static string GetRewardName(CityReward reward)
	{
		if (reward == CityReward.TutorialExplorer)
		{
			return "wcontroller.reward.explorer";
		}
		return "wcontroller.reward." + reward.ToString().ToLower();
	}

	public static int GetRewardCost(CityReward reward)
	{
		if (reward == CityReward.Rebellion)
		{
			return 5;
		}
		return 0;
	}

	public static string GetPopupDescription(CityReward reward)
	{
		return "world." + reward.GetName() + ".description";
	}

	public static string GetPopupTitle(CityReward reward)
	{
		return "world." + reward.GetName() + ".title";
	}
}
