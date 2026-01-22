using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public static class BuildingUtils
{
	public static int ICEBANK_TILES = 20;

	public static string GetInfo(ImprovementData buildingData, Building building = null, PlayerState owner = null, Tile tile = null)
	{
		_ = (Object)(object)building != (Object)null;
		if (Object.op_Implicit((Object)(object)building))
		{
			owner = building.Owner;
		}
		List<string> list = new List<string>();
		if (Object.op_Implicit((Object)(object)building) && buildingData.type == ImprovementData.Type.City)
		{
			if (owner != null)
			{
				if (building.CapitalOf != null)
				{
					if (owner.startTile == building.Tile.Coordinates)
					{
						list.Add(string.Format("{0} ", Localization.Get("building.capital.owner", building.DisplayName, owner.GetLocalizedTribeName(GameManager.GameState))));
					}
					else
					{
						list.Add(string.Format("{0} ", Localization.Get("building.capital.owner.former", building.DisplayName, building.CapitalOf.GetLocalizedTribeName(GameManager.GameState), owner.GetLocalizedTribeName(GameManager.GameState))));
					}
				}
				else
				{
					list.Add(string.Format("{0} ", Localization.Get("building.city.owner", building.DisplayName, owner.GetLocalizedTribeName(GameManager.GameState))));
				}
			}
			else
			{
				list.Add(Localization.Get("building.village.owner"));
			}
		}
		string text = string.Empty;
		bool flag = false;
		if (buildingData.adjacencyImprovements != null && buildingData.adjacencyImprovements.Count > 0)
		{
			foreach (AdjacencyImprovements adjacencyImprovement in buildingData.adjacencyImprovements)
			{
				if (adjacencyImprovement.improvement != null && adjacencyImprovement.improvement.type != ImprovementData.Type.None)
				{
					text = adjacencyImprovement.improvement.displayName;
					flag = adjacencyImprovement.improvement.type == ImprovementData.Type.PolarisClimate;
				}
				if (adjacencyImprovement.resources != null && adjacencyImprovement.resources.type != ResourceData.Type.None)
				{
					text = adjacencyImprovement.resources.displayName;
				}
			}
		}
		if (owner != null || buildingData.type != ImprovementData.Type.City)
		{
			int amount = buildingData.work;
			if (Object.op_Implicit((Object)(object)building))
			{
				amount = building.Tile.Data.CalculateRawProduction(GameManager.GameState);
			}
			if (buildingData.work > 0)
			{
				string text2 = "";
				text2 = ((building?.Level != 0 || !IsStarProductionType(buildingData.type)) ? Localization.Get("building.produce", ResourceUtils.GetResourceEntryText(ResourceManager.Type.Currency, amount)) : Localization.Get("building.produce.multiply2", ResourceUtils.GetResourceEntryText(ResourceManager.Type.Currency, buildingData.work), Localization.Get(text)));
				if (text != string.Empty)
				{
					if (flag)
					{
						text2 = ((owner == null) ? Localization.Get("building.produce.multiply2.polaris", ResourceUtils.GetResourceEntryText(ResourceManager.Type.Currency, buildingData.work), ICEBANK_TILES) : ((!((Object)(object)building == (Object)null) && building.State != null && building.State.level != 0) ? (text2 + Localization.Get("building.produce.multiply.polaris", ResourceUtils.GetResourceEntryText(ResourceManager.Type.Currency, buildingData.work), ICEBANK_TILES)) : Localization.Get("building.produce.multiply2.polaris", ResourceUtils.GetResourceEntryText(ResourceManager.Type.Currency, buildingData.work), ICEBANK_TILES)));
					}
					else if (building?.Level != 0 || !IsStarProductionType(buildingData.type))
					{
						text2 += Localization.Get("building.produce.multiply", ResourceUtils.GetResourceEntryText(ResourceManager.Type.Currency, buildingData.work), Localization.Get(text));
						if (owner == null)
						{
							text2 = Localization.Get("building.produce.multiply2", ResourceUtils.GetResourceEntryText(ResourceManager.Type.Currency, amount), Localization.Get(text));
						}
					}
				}
				list.Add(text2);
			}
			if (!string.IsNullOrEmpty(text))
			{
				if (GameManager.GameState.Version >= 40 && buildingData.growthRewards != null && buildingData.growthRewards.Count > 0)
				{
					List<string> list2 = new List<string>();
					foreach (GrowthRewards growthReward in buildingData.growthRewards)
					{
						if (growthReward.population > 0)
						{
							list2.Add(ResourceUtils.GetResourceEntryText(ResourceManager.Type.Population, growthReward.population));
						}
					}
					list.Add(Localization.Get("building.produce.multiply3", string.Join(", ", list2), Localization.Get(text)));
				}
				else if (GameManager.GameState.Version < 40 && buildingData.rewards != null && buildingData.rewards.Count > 0)
				{
					list.Add(Localization.Get("building.produce.multiply3", buildingData.rewards.GetPopulation(), Localization.Get(text)));
				}
			}
			else if (buildingData.rewards != null && buildingData.rewards.Count > 0)
			{
				if (buildingData.rewards.GetPopulation() > 0)
				{
					if (Object.op_Implicit((Object)(object)building) && (Object)(object)building.Tile != (Object)null && building.Tile.Owner != null)
					{
						City city = MapRenderer.Current.GetTileInstance(building.Tile.Data.rulingCityCoordinates).Improvement as City;
						list.Add(Localization.Get("building.produce.reward.named", ResourceUtils.GetResourceEntryText(buildingData.rewards), city.DisplayName));
					}
					else if ((Object)(object)tile != (Object)null && tile.Owner != null)
					{
						City city2 = MapRenderer.Current.GetTileInstance(tile.Data.rulingCityCoordinates).Improvement as City;
						list.Add(Localization.Get("building.produce.reward.named", ResourceUtils.GetResourceEntryText(buildingData.rewards), city2.DisplayName));
					}
					else
					{
						list.Add(Localization.Get("building.produce.reward", ResourceUtils.GetResourceEntryText(buildingData.rewards)));
					}
				}
				if (buildingData.rewards.GetCurrency() > 0 || buildingData.rewards.GetScore() > 0)
				{
					list.Add(Localization.Get("building.reward.instant", ResourceUtils.GetResourceEntryText(buildingData.rewards)));
				}
			}
		}
		if (buildingData.creates != null && buildingData.creates.Count > 0)
		{
			string text3 = string.Empty;
			if (buildingData.terrainRequirements != null)
			{
				foreach (TerrainRequirements terrainRequirement in buildingData.terrainRequirements)
				{
					if (terrainRequirement.terrain != null)
					{
						text3 += Localization.Get(terrainRequirement.terrain.displayName);
					}
					else if (terrainRequirement.resource != null)
					{
						text3 += Localization.Get(terrainRequirement.resource.displayName);
					}
				}
			}
			string text4 = string.Empty;
			string text5 = string.Empty;
			foreach (Creates create in buildingData.creates)
			{
				if (create.terrain != null)
				{
					text4 += Localization.Get(create.terrain.displayName);
				}
				if (create.resource != null)
				{
					text5 += Localization.Get(create.resource.displayName);
				}
				if (create.unit != null)
				{
					text5 += Localization.Get(create.unit.displayName);
				}
			}
			if (!string.IsNullOrEmpty(text4))
			{
				if (!string.IsNullOrEmpty(text5))
				{
					list.Add(Localization.Get("building.transform2", text3, text4, text5));
				}
				else
				{
					list.Add(Localization.Get("building.transform", text3, text4));
				}
			}
			else if (!string.IsNullOrEmpty(text5))
			{
				list.Add(Localization.Get("building.resource", text5));
			}
		}
		float num = buildingData.rewards.GetScore();
		if ((Object)(object)building != (Object)null)
		{
			num = GameManager.GameState.CalculateImprovementScore(building.Tile.Data);
		}
		if (num > 5f)
		{
			list.Add(Localization.Get("building.value", num.ToString()));
		}
		if (list.Count > 0)
		{
			List<string> list3 = new List<string>();
			foreach (string item in list)
			{
				list3.Add(LocalizationUtils.CapitalizeAndEndSentence(item));
			}
			list = list3;
		}
		if (buildingData.type == ImprovementData.Type.Port)
		{
			list.Add(string.Format("{0} ", Localization.Get("building.ability.embark")));
		}
		if (buildingData.routes != null && buildingData.routes.Count > 0)
		{
			int count = buildingData.routes.Count;
			List<string> list4 = new List<string>();
			for (int i = 0; i < count; i++)
			{
				list4.Add(Localization.Get(buildingData.routes[i].displayName));
			}
			string arg = ((count <= 1) ? list4[0] : LocalizationUtils.GetPrettyList(list4));
			if (buildingData.HasAbility(ImprovementAbility.Type.Network))
			{
				list.Add(Localization.Get("building.ability.network", arg, Localization.Get(buildingData.displayName), buildingData.range));
			}
			else
			{
				list.Add(Localization.Get("building.ability.route", arg, Localization.Get(buildingData.displayName), buildingData.range));
			}
		}
		if (buildingData.type == ImprovementData.Type.Road)
		{
			list.Add(Localization.Get("building.ability.road"));
		}
		if (buildingData.type == ImprovementData.Type.Ruin)
		{
			list.Add(Localization.Get("building.ability.ruin"));
		}
		string arg2 = "";
		if (GameManager.GameState.Version < 40 && buildingData.rewards != null && buildingData.rewards.Count > 0)
		{
			List<string> list5 = new List<string>();
			foreach (Rewards reward in buildingData.rewards)
			{
				if (reward.score > 0)
				{
					list5.Add(ResourceUtils.GetResourceEntryText(ResourceManager.Type.Score, reward.score / 2));
				}
			}
			arg2 = string.Join(", ", list5);
		}
		else if (GameManager.GameState.Version >= 40 && buildingData.growthRewards != null && buildingData.growthRewards.Count > 0)
		{
			List<string> list6 = new List<string>();
			foreach (GrowthRewards growthReward2 in buildingData.growthRewards)
			{
				if (growthReward2.population > 0)
				{
					list6.Add(ResourceUtils.GetResourceEntryText(ResourceManager.Type.Population, growthReward2.population));
				}
				if (growthReward2.score > 0)
				{
					list6.Add(ResourceUtils.GetResourceEntryText(ResourceManager.Type.Score, growthReward2.score));
				}
			}
			arg2 = string.Join(", ", list6);
		}
		foreach (ImprovementAbility.Type improvementAbility in buildingData.improvementAbilities)
		{
			if (improvementAbility != ImprovementAbility.Type.Expand && improvementAbility != ImprovementAbility.Type.Consumed && improvementAbility != ImprovementAbility.Type.Network)
			{
				list.Add(Localization.Get(improvementAbility.GetDisplayName(), Localization.Get(buildingData.displayName), buildingData.growthRate, arg2));
			}
		}
		if (buildingData.adjacencyRequirements != null && buildingData.adjacencyRequirements.Count > 0)
		{
			foreach (AdjacencyRequirements adjacencyRequirement in buildingData.adjacencyRequirements)
			{
				if (adjacencyRequirement.improvement != null && adjacencyRequirement.improvement.type != ImprovementData.Type.None)
				{
					list.Add(Localization.Get("building.restriction.near", Localization.Get(adjacencyRequirement.improvement.displayName)));
				}
				else if (adjacencyRequirement.resource != null && adjacencyRequirement.resource.type != ResourceData.Type.None)
				{
					list.Add(Localization.Get("building.restriction.near", Localization.Get(adjacencyRequirement.resource.displayName)));
				}
				else if (adjacencyRequirement.terrain != null && adjacencyRequirement.terrain.type != TerrainData.Type.None)
				{
					list.Add(Localization.Get("building.restriction.near", Localization.Get(adjacencyRequirement.terrain.displayName)));
				}
			}
		}
		if (buildingData.terrainRequirements != null && buildingData.terrainRequirements.Count > 0)
		{
			foreach (TerrainRequirements terrainRequirement2 in buildingData.terrainRequirements)
			{
				if (terrainRequirement2.resource != null && terrainRequirement2.resource.type != ResourceData.Type.None)
				{
					list.Add(Localization.Get("building.restriction.on", Localization.Get(terrainRequirement2.resource.displayName)));
				}
			}
		}
		return string.Join(" ", list);
	}

	public static bool IsStarProductionType(ImprovementData.Type type)
	{
		if (type != ImprovementData.Type.Sanctuary)
		{
			return type == ImprovementData.Type.CustomsHouse;
		}
		return true;
	}
}
