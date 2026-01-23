using System.Collections.Generic;
using Newtonsoft.Json;

namespace Polytopia.Data;

public class ImprovementData
{
	public enum Type
	{
		None,
		City,
		Ruin,
		Road,
		CustomsHouse,
		Farm,
		Windmill,
		Fishing,
		Port,
		Hunting,
		ClearForest,
		BurnForest,
		LumberHut,
		Sawmill,
		GrowForest,
		HarvestFruit,
		WhaleHunting,
		Temple,
		ForestTemple,
		WaterTemple,
		MountainTemple,
		Mine,
		Forge,
		Monument1,
		Monument2,
		Monument3,
		Monument4,
		Monument5,
		Monument6,
		Monument7,
		EnchantAnimal,
		EnchantWhale,
		Sanctuary,
		Outpost,
		IceBank,
		IceTemple,
		PolarisClimate,
		Fungi,
		Algae,
		Mycelium,
		BurnSpores,
		Clathrus,
		HiddenSanctuary,
		HarvestSpores,
		NullBuilding,
		Cultivate
	}

	public int idx;

	public bool hidden;

	public bool shouldSuggestUnlock;

	public int cost;

	public int work;

	public int borderSize;

	public int maxLevel;

	[JsonConverter(typeof(StringIDsToEnumsConverter<ImprovementAbility.Type>))]
	public List<ImprovementAbility.Type> improvementAbilities = new List<ImprovementAbility.Type>();

	public List<Creates> creates = new List<Creates>();

	public List<Rewards> rewards = new List<Rewards>();

	public List<TerrainRequirements> terrainRequirements = new List<TerrainRequirements>();

	public List<AdjacencyRequirements> adjacencyRequirements = new List<AdjacencyRequirements>();

	public List<AdjacencyImprovements> adjacencyImprovements = new List<AdjacencyImprovements>();

	[JsonConverter(typeof(StringIDsToObjectsConverter<TerrainData, TerrainData.Type>))]
	public List<TerrainData> routes = new List<TerrainData>();

	public int range;

	public int growthRate;

	public List<GrowthRewards> growthRewards = new List<GrowthRewards>();

	public string displayName => $"building.names.{type.GetName()}";

	public Type type => (Type)idx;
}
