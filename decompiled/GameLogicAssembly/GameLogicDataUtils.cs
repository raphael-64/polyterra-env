using System.Collections.Generic;
using Polytopia.Data;

public static class GameLogicDataUtils
{
	public static ProbabilityTable GetPickableTribes(List<TribeData> tribes, GameSettings settings, List<PlayerState> players, bool allowMirrorPick = false)
	{
		ProbabilityTable probabilityTable = new ProbabilityTable();
		foreach (TribeData tribe in tribes)
		{
			if (!settings.IsTribeEnabled(tribe.type) || (settings.IsAnyTribeUnlocked() && settings.IsTribeLocked(tribe.type)))
			{
				continue;
			}
			if (players != null)
			{
				bool flag = false;
				foreach (PlayerState player in players)
				{
					if (player.tribe == tribe.type)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					if (allowMirrorPick)
					{
						probabilityTable.Add((int)tribe.type, 0.01f);
					}
					continue;
				}
			}
			probabilityTable.Add((int)tribe.type, 1f);
		}
		if (probabilityTable.Count == 0 && !allowMirrorPick)
		{
			return GetPickableTribes(tribes, settings, players, allowMirrorPick: true);
		}
		probabilityTable.Normalize();
		return probabilityTable;
	}

	public static bool ContainsRequiredImprovement(this List<ImprovementData> list, ResourceData.Type resourceType)
	{
		if (resourceType == ResourceData.Type.None)
		{
			return false;
		}
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				ImprovementData improvementData = list[i];
				if (improvementData == null || improvementData.terrainRequirements == null || improvementData.terrainRequirements.Count == 0)
				{
					continue;
				}
				for (int j = 0; j < improvementData.terrainRequirements.Count; j++)
				{
					ResourceData resource = improvementData.terrainRequirements[j].resource;
					if (resource != null && resource.type != ResourceData.Type.None && resource.type == resourceType)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
