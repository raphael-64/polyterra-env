using System.Collections.Generic;
using Polytopia.Data;

public static class DataExtensions
{
	public static bool Contains(this List<TechData> list, TechData.Type tech)
	{
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].type == tech)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool Contains(this List<ImprovementData> list, ImprovementData.Type improvement)
	{
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].type == improvement)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool Contains(this List<UnitData> list, UnitData.Type unit)
	{
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].type == unit)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool Contains(this List<TaskData> list, TaskData.Type task)
	{
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].type == task)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool Contains(this List<TerrainRequirements> list, TerrainData.Type terrain)
	{
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].terrain.type == terrain)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool Contains(this List<TerrainRequirements> list, ResourceData.Type resource)
	{
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].resource.type == resource)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool Contains(this List<AdjacencyRequirements> list, TerrainData.Type terrain)
	{
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].terrain.type == terrain)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool Contains(this List<AdjacencyRequirements> list, ImprovementData.Type improvement)
	{
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].improvement.type == improvement)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool Contains(this List<AdjacencyImprovements> list, ImprovementData.Type improvement)
	{
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				AdjacencyImprovements adjacencyImprovements = list[i];
				if (adjacencyImprovements.improvement != null && adjacencyImprovements.improvement.type == improvement)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool Contains(this List<TerrainData> list, TerrainData.Type terrain)
	{
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].type == terrain)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static UnitData GetUnit(this List<Creates> list)
	{
		if (list != null && list.Count > 0)
		{
			foreach (Creates item in list)
			{
				if (item.unit != null && item.unit.type != UnitData.Type.None)
				{
					return item.unit;
				}
			}
		}
		return null;
	}

	public static ResourceData GetResource(this List<Creates> list)
	{
		if (list != null && list.Count > 0)
		{
			foreach (Creates item in list)
			{
				if (item.resource != null && item.resource.type != ResourceData.Type.None)
				{
					return item.resource;
				}
			}
		}
		return null;
	}

	public static TerrainData GetTerrain(this List<Creates> list)
	{
		if (list != null && list.Count > 0)
		{
			foreach (Creates item in list)
			{
				if (item.terrain != null)
				{
					return item.terrain;
				}
			}
		}
		return null;
	}

	public static int GetPopulation(this List<Rewards> list)
	{
		int num = 0;
		if (list != null && list.Count > 0)
		{
			foreach (Rewards item in list)
			{
				num += item.population;
			}
		}
		return num;
	}

	public static int GetPopulation(this List<GrowthRewards> list)
	{
		int num = 0;
		if (list != null && list.Count > 0)
		{
			foreach (GrowthRewards item in list)
			{
				num += item.population;
			}
		}
		return num;
	}

	public static int GetCurrency(this List<Rewards> list)
	{
		int num = 0;
		if (list != null && list.Count > 0)
		{
			foreach (Rewards item in list)
			{
				num += item.currency;
			}
		}
		return num;
	}

	public static int GetScore(this List<Rewards> list)
	{
		int num = 0;
		if (list != null && list.Count > 0)
		{
			foreach (Rewards item in list)
			{
				num += item.score;
			}
		}
		return num;
	}

	public static int GetScore(this List<GrowthRewards> list)
	{
		int num = 0;
		if (list != null && list.Count > 0)
		{
			foreach (GrowthRewards item in list)
			{
				num += item.score;
			}
		}
		return num;
	}

	public static int CalculateRewardScore(this List<Rewards> list)
	{
		int num = 0;
		if (list != null && list.Count > 0)
		{
			foreach (Rewards item in list)
			{
				num += item.population;
				num += item.currency;
				num += item.score;
			}
		}
		return num;
	}

	public static bool IsType(this TechData data, TechData.Type type)
	{
		if (data != null)
		{
			return data.type == type;
		}
		return false;
	}

	public static bool IsType(this UnitData data, UnitData.Type type)
	{
		if (data != null)
		{
			return data.type == type;
		}
		return false;
	}

	public static bool IsType(this ImprovementData data, ImprovementData.Type type)
	{
		if (data != null)
		{
			return data.type == type;
		}
		return false;
	}

	public static bool IsType(this ResourceData data, ResourceData.Type type)
	{
		if (data != null)
		{
			return data.type == type;
		}
		return false;
	}
}
