using System.Collections.Generic;
using Polytopia.Data;

public class TechUtils
{
	public static string GetInfo(TechData data)
	{
		string[] array = new string[6] { "", "", "", "", "", "" };
		int num = 0;
		List<string> list = new List<string>();
		if (data.improvementUnlocks != null && data.improvementUnlocks.Count > 0)
		{
			List<string> list2 = new List<string>();
			foreach (ImprovementData improvementUnlock in data.improvementUnlocks)
			{
				list2.Add(Localization.Get(improvementUnlock.displayName));
			}
			list.Add(Localization.Get("technology.enables", array[num++], LocalizationUtils.GetPrettyList(list2)));
		}
		if (data.unitUnlocks != null && data.unitUnlocks.Count > 0)
		{
			List<string> list3 = new List<string>();
			foreach (UnitData unitUnlock in data.unitUnlocks)
			{
				if (unitUnlock.upgradesFrom != null)
				{
					list.Add(Localization.Get("technology.upgrade", array[num++], Localization.Get(unitUnlock.upgradesFrom.displayName), Localization.Get(unitUnlock.displayName)));
				}
				if (!unitUnlock.hidden)
				{
					list3.Add(Localization.Get(unitUnlock.displayName));
				}
			}
			if (list3.Count > 0)
			{
				list.Add(Localization.Get("technology.build", array[num++], LocalizationUtils.GetPrettyList(list3)));
			}
		}
		if (data.movementUnlocks != null && data.movementUnlocks.Count > 0)
		{
			List<string> list4 = new List<string>();
			foreach (KeyValuePair<TerrainData.Type, int> movementUnlock in data.movementUnlocks)
			{
				list4.Add(Localization.Get(movementUnlock.Key.GetDisplayName()));
			}
			list.Add(Localization.Get("technology.move", array[num++], LocalizationUtils.GetPrettyList(list4)));
		}
		if (data.defenceBonusUnlocks != null && data.defenceBonusUnlocks.Count > 0)
		{
			List<string> list5 = new List<string>();
			foreach (KeyValuePair<TerrainData.Type, int> defenceBonusUnlock in data.defenceBonusUnlocks)
			{
				list5.Add(Localization.Get(defenceBonusUnlock.Key.GetDisplayName()));
			}
			list.Add(Localization.Get("technology.defence", array[num++], LocalizationUtils.GetPrettyList(list5)));
		}
		if (data.taskUnlocks != null && data.taskUnlocks.Count > 0)
		{
			List<string> list6 = new List<string>();
			foreach (TaskData taskUnlock in data.taskUnlocks)
			{
				list6.Add(Localization.Get(taskUnlock.displayName));
			}
			list.Add(Localization.Get("technology.task", array[num++], LocalizationUtils.GetPrettyList(list6)));
		}
		if (data.abilityUnlocks != null && data.abilityUnlocks.Count > 0)
		{
			List<string> list7 = new List<string>();
			foreach (PlayerAbility.Type abilityUnlock in data.abilityUnlocks)
			{
				list7.Add(Localization.Get(abilityUnlock.GetDisplayName()));
			}
			list.Add(Localization.Get("technology.ability", array[num++], LocalizationUtils.GetPrettyList(list7)));
		}
		return string.Join("", list);
	}
}
