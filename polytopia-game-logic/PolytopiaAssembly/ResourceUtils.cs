using System.Collections.Generic;
using Polytopia.Data;

public static class ResourceUtils
{
	public static string GetName(ResourceManager.Type type)
	{
		return type switch
		{
			ResourceManager.Type.Score => "price.points", 
			ResourceManager.Type.Currency => "price.stars", 
			ResourceManager.Type.Population => "price.population", 
			ResourceManager.Type.Production => "None", 
			_ => "None", 
		};
	}

	public static string GetResourceEntryText(ResourceManager.Type type, int amount)
	{
		return type switch
		{
			ResourceManager.Type.Score => string.Format("{0} {1}", amount, Localization.Get(PluralizeKey("price.points", amount))), 
			ResourceManager.Type.Currency => string.Format("{0} {1}", amount, Localization.Get(PluralizeKey("price.stars", amount))), 
			ResourceManager.Type.Population => string.Format("{0} {1}", amount, Localization.Get(PluralizeKey("price.population", amount))), 
			ResourceManager.Type.Production => "None", 
			_ => "None", 
		};
	}

	public static string GetResourceEntryText(List<Rewards> data)
	{
		List<string> list = new List<string>();
		int count = data.Count;
		for (int i = 0; i < count; i++)
		{
			Rewards rewards = data[i];
			if (rewards.currency > 0)
			{
				list.Add(string.Format("{0} {1}", rewards.currency, Localization.Get(PluralizeKey("price.stars", rewards.currency))));
			}
			else if (rewards.population > 0)
			{
				list.Add(string.Format("{0} {1}", rewards.population, Localization.Get(PluralizeKey("price.population", rewards.population))));
			}
			else if (rewards.score > 0)
			{
				list.Add(string.Format("{0} {1}", rewards.score, Localization.Get(PluralizeKey("price.points", rewards.score))));
			}
		}
		return string.Join(", ", list);
	}

	private static string PluralizeKey(string key, int amount)
	{
		return string.Format("{0}{1}", key, (amount > 1) ? ".plural" : "");
	}
}
