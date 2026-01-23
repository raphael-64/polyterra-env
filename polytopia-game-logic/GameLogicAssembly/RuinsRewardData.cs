public static class RuinsRewardData
{
	public const int RESOURCE_REWARD = 10;

	public const int POPULATION_REWARD = 3;

	public static string GetRewardTitle(RuinsReward reward)
	{
		return reward switch
		{
			RuinsReward.Explorer => "wcontroller.examine.explorer.title", 
			RuinsReward.Battleship => "wcontroller.examine.water.title", 
			RuinsReward.FreeTech => "wcontroller.examine.tech.title", 
			RuinsReward.Resources => "wcontroller.examine.stars.title", 
			RuinsReward.PopulationGrowth => "wcontroller.examine.population.title", 
			RuinsReward.Swordsman => "wcontroller.examine.swordsman.title", 
			_ => "None", 
		};
	}

	public static string GetRewardDescription(RuinsReward reward)
	{
		return reward switch
		{
			RuinsReward.Explorer => "wcontroller.examine.explorer", 
			RuinsReward.SuperUnit => "wcontroller.examine.giant", 
			RuinsReward.Battleship => "wcontroller.examine.water", 
			RuinsReward.Seamonster => "wcontroller.examine.water.elyrion", 
			RuinsReward.FreeTech => "wcontroller.examine.tech", 
			RuinsReward.Resources => "wcontroller.examine.stars", 
			RuinsReward.PopulationGrowth => "wcontroller.examine.population", 
			RuinsReward.Swordsman => "wcontroller.examine.swordsman", 
			_ => "None", 
		};
	}
}
