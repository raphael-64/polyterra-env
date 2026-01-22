using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Polytopia.Data;

public class GameLogicData : IPolytopiaDataRoot
{
	public Dictionary<TribeData.Type, TribeData> tribes = new Dictionary<TribeData.Type, TribeData>();

	public Dictionary<TerrainData.Type, TerrainData> terrains = new Dictionary<TerrainData.Type, TerrainData>();

	public Dictionary<ResourceData.Type, ResourceData> resources = new Dictionary<ResourceData.Type, ResourceData>();

	public Dictionary<TaskData.Type, TaskData> tasks = new Dictionary<TaskData.Type, TaskData>();

	public Dictionary<ImprovementData.Type, ImprovementData> improvements = new Dictionary<ImprovementData.Type, ImprovementData>();

	public Dictionary<UnitData.Type, UnitData> units = new Dictionary<UnitData.Type, UnitData>();

	public Dictionary<TechData.Type, TechData> tech = new Dictionary<TechData.Type, TechData>();

	public DiplomacyData diplomacyData = new DiplomacyData();

	private HashSet<int> alienClimates = new HashSet<int>();

	public Dictionary<TribeData.Type, TribeData> AllTribeData => tribes;

	public Dictionary<TechData.Type, TechData> AllTechData => tech;

	public Dictionary<UnitData.Type, UnitData> AllUnitData => units;

	public Dictionary<ImprovementData.Type, ImprovementData> AllImprovementData => improvements;

	public Dictionary<ResourceData.Type, ResourceData> AllResourceData => resources;

	public Dictionary<TaskData.Type, TaskData> AllTaskData => tasks;

	public Dictionary<TerrainData.Type, TerrainData> AllTerrainData => terrains;

	public DiplomacyData DiplomacyData => diplomacyData;

	private void AddGameLogicPlaceholders(JObject rootObject)
	{
		Dictionary<TribeData.Type, TribeData> dict = tribes;
		JToken obj = rootObject["tribeData"];
		ParseUtils.AddPlaceholdersForDictionary(dict, (JObject)(object)((obj is JObject) ? obj : null));
		Dictionary<TerrainData.Type, TerrainData> dict2 = terrains;
		JToken obj2 = rootObject["terrainData"];
		ParseUtils.AddPlaceholdersForDictionary(dict2, (JObject)(object)((obj2 is JObject) ? obj2 : null));
		Dictionary<ResourceData.Type, ResourceData> dict3 = resources;
		JToken obj3 = rootObject["resourceData"];
		ParseUtils.AddPlaceholdersForDictionary(dict3, (JObject)(object)((obj3 is JObject) ? obj3 : null));
		Dictionary<TaskData.Type, TaskData> dict4 = tasks;
		JToken obj4 = rootObject["taskData"];
		ParseUtils.AddPlaceholdersForDictionary(dict4, (JObject)(object)((obj4 is JObject) ? obj4 : null));
		Dictionary<ImprovementData.Type, ImprovementData> dict5 = improvements;
		JToken obj5 = rootObject["improvementData"];
		ParseUtils.AddPlaceholdersForDictionary(dict5, (JObject)(object)((obj5 is JObject) ? obj5 : null));
		Dictionary<UnitData.Type, UnitData> dict6 = units;
		JToken obj6 = rootObject["unitData"];
		ParseUtils.AddPlaceholdersForDictionary(dict6, (JObject)(object)((obj6 is JObject) ? obj6 : null));
		Dictionary<TechData.Type, TechData> dict7 = tech;
		JToken obj7 = rootObject["techData"];
		ParseUtils.AddPlaceholdersForDictionary(dict7, (JObject)(object)((obj7 is JObject) ? obj7 : null));
		diplomacyData = new DiplomacyData();
	}

	private void ParseGameLogicObjects(JObject rootObject)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		JsonSerializerSettings val = new JsonSerializerSettings();
		val.Context = new StreamingContext(StreamingContextStates.All, this);
		Dictionary<TribeData.Type, TribeData> dict = tribes;
		JToken obj = rootObject["tribeData"];
		ParseUtils.ParseObjectsForDictionary(dict, (JObject)(object)((obj is JObject) ? obj : null), val);
		Dictionary<TerrainData.Type, TerrainData> dict2 = terrains;
		JToken obj2 = rootObject["terrainData"];
		ParseUtils.ParseObjectsForDictionary(dict2, (JObject)(object)((obj2 is JObject) ? obj2 : null), val);
		Dictionary<ResourceData.Type, ResourceData> dict3 = resources;
		JToken obj3 = rootObject["resourceData"];
		ParseUtils.ParseObjectsForDictionary(dict3, (JObject)(object)((obj3 is JObject) ? obj3 : null), val);
		Dictionary<TaskData.Type, TaskData> dict4 = tasks;
		JToken obj4 = rootObject["taskData"];
		ParseUtils.ParseObjectsForDictionary(dict4, (JObject)(object)((obj4 is JObject) ? obj4 : null), val);
		Dictionary<ImprovementData.Type, ImprovementData> dict5 = improvements;
		JToken obj5 = rootObject["improvementData"];
		ParseUtils.ParseObjectsForDictionary(dict5, (JObject)(object)((obj5 is JObject) ? obj5 : null), val);
		Dictionary<UnitData.Type, UnitData> dict6 = units;
		JToken obj6 = rootObject["unitData"];
		ParseUtils.ParseObjectsForDictionary(dict6, (JObject)(object)((obj6 is JObject) ? obj6 : null), val);
		Dictionary<TechData.Type, TechData> dict7 = tech;
		JToken obj7 = rootObject["techData"];
		ParseUtils.ParseObjectsForDictionary(dict7, (JObject)(object)((obj7 is JObject) ? obj7 : null), val);
		if (rootObject["diplomacyData"] is JObject)
		{
			rootObject["diplomacyData"].Populate(diplomacyData);
		}
	}

	public bool TryGetDataGeneric<T, S>(S enumValue, out T result)
	{
		Type typeFromHandle = typeof(Dictionary<S, T>);
		FieldInfo[] fields = typeof(GameLogicData).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType == typeFromHandle)
			{
				return (fieldInfo.GetValue(this) as Dictionary<S, T>).TryGetValue(enumValue, out result);
			}
		}
		result = default(T);
		return false;
	}

	public void Parse(string jsonData)
	{
		JObject rootObject = JObject.Parse(jsonData);
		AddGameLogicPlaceholders(rootObject);
		ParseGameLogicObjects(rootObject);
		CacheAlienClimates();
	}

	private void CacheAlienClimates()
	{
		foreach (KeyValuePair<TribeData.Type, TribeData> tribe in tribes)
		{
			if (tribe.Value.HasAbility(TribeAbility.Type.AlienClimate))
			{
				alienClimates.Add(tribe.Value.climate);
			}
		}
	}

	public void LoadDatabase(string path)
	{
		Log.Info("Loading database at: {0}", new object[1] { path });
		if (!string.IsNullOrEmpty(path) && File.Exists(path))
		{
			Parse(File.ReadAllText(path));
			Log.Info("[GameLogicData] Loaded database: {0}", new object[1] { path });
		}
	}

	public bool TryGetData(TribeData.Type id, out TribeData data)
	{
		if (id != TribeData.Type.None && tribes.TryGetValue(id, out var value))
		{
			data = value;
			return true;
		}
		data = null;
		return false;
	}

	public bool TryGetData(TechData.Type type, out TechData data)
	{
		if (tech.TryGetValue(type, out var value))
		{
			data = value;
			return true;
		}
		data = null;
		return false;
	}

	public bool TryGetData(UnitData.Type type, out UnitData data)
	{
		if (type != UnitData.Type.None && units.TryGetValue(type, out var value))
		{
			data = value;
			return true;
		}
		data = null;
		return false;
	}

	public bool TryGetData(ImprovementData.Type type, out ImprovementData data)
	{
		if (type != ImprovementData.Type.None && improvements.TryGetValue(type, out var value))
		{
			data = value;
			return true;
		}
		data = null;
		return false;
	}

	public bool TryGetData(ResourceData.Type type, out ResourceData data)
	{
		if (type != ResourceData.Type.None && resources.TryGetValue(type, out var value))
		{
			data = value;
			return true;
		}
		data = null;
		return false;
	}

	public bool TryGetData(TaskData.Type type, out TaskData data)
	{
		return tasks.TryGetValue(type, out data);
	}

	public bool TryGetData(TerrainData.Type type, out TerrainData data)
	{
		if (terrains.TryGetValue(type, out var value))
		{
			data = value;
			return true;
		}
		data = null;
		return false;
	}

	public List<TechData> GetAllTechForTribe(TribeData.Type tribe)
	{
		if (TryGetData(tribe, out var data))
		{
			return GetAllTechForTribe(data);
		}
		return null;
	}

	public List<TechData> GetAllTechForTribe(TribeData tribeData)
	{
		Queue<TechData> queue = new Queue<TechData>();
		List<TechData> list = new List<TechData>();
		TryGetData(TechData.Type.Basic, out var data);
		queue.Enqueue(data);
		int num = 200;
		while (queue.Count > 0 && num-- > 0)
		{
			TechData techData = GetOverride(queue.Dequeue(), tribeData);
			if (!list.Contains(techData))
			{
				list.Add(techData);
				queue.EnqueueRange(techData.techUnlocks);
			}
		}
		return list;
	}

	public TechData GetRequiredTech(TribeData tribeData, TechData.Type techType)
	{
		foreach (TechData item in GetAllTechForTribe(tribeData))
		{
			if (item.techUnlocks == null || item.techUnlocks.Count <= 0)
			{
				continue;
			}
			foreach (TechData techUnlock in item.techUnlocks)
			{
				if (techUnlock.type == techType)
				{
					return item;
				}
			}
		}
		return null;
	}

	public TechData GetRequiredTech(TribeData tribeData, ImprovementData.Type improvementType)
	{
		foreach (TechData item in GetAllTechForTribe(tribeData))
		{
			if (item.improvementUnlocks == null || item.improvementUnlocks.Count <= 0)
			{
				continue;
			}
			foreach (ImprovementData improvementUnlock in item.improvementUnlocks)
			{
				if (improvementUnlock.type == improvementType)
				{
					return item;
				}
			}
		}
		return null;
	}

	public TechData GetRequiredTech(TribeData tribeData, UnitData.Type unitType)
	{
		foreach (TechData item in GetAllTechForTribe(tribeData))
		{
			if (item.unitUnlocks == null || item.unitUnlocks.Count <= 0)
			{
				continue;
			}
			foreach (UnitData unitUnlock in item.unitUnlocks)
			{
				if (unitUnlock.type == unitType)
				{
					return item;
				}
			}
		}
		return null;
	}

	public TechData GetRequiredTech(TribeData tribeData, PlayerAbility.Type abilityType)
	{
		foreach (TechData item in GetAllTechForTribe(tribeData))
		{
			if (item.abilityUnlocks == null || item.abilityUnlocks.Count <= 0)
			{
				continue;
			}
			foreach (PlayerAbility.Type abilityUnlock in item.abilityUnlocks)
			{
				if (abilityUnlock == abilityType)
				{
					return item;
				}
			}
		}
		return null;
	}

	public TechData GetRequiredTech(TribeData tribeData, TaskData.Type taskType)
	{
		foreach (TechData item in GetAllTechForTribe(tribeData))
		{
			if (item.taskUnlocks == null || item.taskUnlocks.Count <= 0)
			{
				continue;
			}
			foreach (TaskData taskUnlock in item.taskUnlocks)
			{
				if (taskUnlock.type == taskType)
				{
					return item;
				}
			}
		}
		return null;
	}

	public TechData GetRequiredTech(TribeData tribeData, TerrainData.Type terrainType)
	{
		foreach (TechData item in GetAllTechForTribe(tribeData))
		{
			if (item.movementUnlocks != null && item.movementUnlocks.Count > 0 && item.movementUnlocks.ContainsKey(terrainType))
			{
				return item;
			}
		}
		return null;
	}

	public List<TechData> GetUnlockedTech(PlayerState player)
	{
		if (player != null && player.availableTech != null)
		{
			if (player.availableTech.Count == player.unlockedTechCache.Count)
			{
				bool flag = true;
				for (int i = 0; i < player.availableTech.Count; i++)
				{
					if (player.availableTech[i] != player.unlockedTechCache[i].type)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return player.unlockedTechCache;
				}
			}
			player.unlockedTechCache.Clear();
			GetUnlockedTechInternal(player, player.unlockedTechCache);
			return player.unlockedTechCache;
		}
		return null;
	}

	private void GetUnlockedTechInternal(PlayerState player, List<TechData> tech)
	{
		if (player == null || player.availableTech == null)
		{
			return;
		}
		for (int i = 0; i < player.availableTech.Count; i++)
		{
			if (TryGetData(player.availableTech[i], out var data))
			{
				tech.Add(data);
			}
		}
	}

	public List<UnitData> GetUnlockedUnits(PlayerState player, GameState state, bool shouldIncludeHidden)
	{
		TryGetData(player.tribe, out var data);
		List<TechData> unlockedTech = GetUnlockedTech(player);
		if (unlockedTech != null)
		{
			List<UnitData> list = new List<UnitData>();
			for (int i = 0; i < unlockedTech.Count; i++)
			{
				foreach (UnitData unitUnlock in unlockedTech[i].unitUnlocks)
				{
					UnitData unitData = GetOverride(unitUnlock, data);
					if (!shouldIncludeHidden && unitData.hidden)
					{
						continue;
					}
					if (unitData.HasAbility(UnitAbility.Type.Unique))
					{
						bool flag = false;
						for (int j = 0; j < state.Map.Tiles.Length; j++)
						{
							TileData tileData = state.Map.Tiles[j];
							if (tileData.unit != null && tileData.unit.owner == player.Id && tileData.unit.type == unitData.type)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							continue;
						}
					}
					list.Add(unitData);
				}
			}
			return list;
		}
		return null;
	}

	public List<UnitData> GetUnlockedUpgradesForUnit(PlayerState player, GameState state, UnitData baseUnit)
	{
		List<UnitData> list = new List<UnitData>();
		List<UnitData> unlockedUnits = GetUnlockedUnits(player, state, shouldIncludeHidden: true);
		if (unlockedUnits == null || unlockedUnits.Count == 0)
		{
			return list;
		}
		foreach (UnitData item in unlockedUnits)
		{
			if (item.upgradesFrom == baseUnit)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<ImprovementData> GetUnlockedImprovements(PlayerState player)
	{
		List<TechData> unlockedTech = GetUnlockedTech(player);
		if (unlockedTech != null && TryGetData(player.tribe, out var data))
		{
			List<ImprovementData> list = new List<ImprovementData>();
			for (int i = 0; i < unlockedTech.Count; i++)
			{
				for (int j = 0; j < unlockedTech[i].improvementUnlocks.Count; j++)
				{
					ImprovementData improvementData = GetOverride(unlockedTech[i].improvementUnlocks[j], data);
					if (!improvementData.hidden)
					{
						list.Add(improvementData);
					}
				}
			}
			if (player.tasks != null && player.tasks.Count > 0)
			{
				for (int k = 0; k < player.tasks.Count; k++)
				{
					if (player.tasks[k].IsCompleted && TryGetData(player.tasks[k].GetTaskType(), out var data2) && data2.improvementUnlocks != null && data2.improvementUnlocks.Count != 0)
					{
						list.AddRange(data2.improvementUnlocks);
					}
				}
			}
			return list;
		}
		return null;
	}

	public List<TerrainData> GetUnlockedMovements(PlayerState player)
	{
		List<TechData> unlockedTech = GetUnlockedTech(player);
		return GetMovementsWithUnlockedTeck(unlockedTech);
	}

	public List<TerrainData> GetMovementsWithUnlockedTeck(List<TechData> tech)
	{
		if (tech != null)
		{
			List<TerrainData> list = new List<TerrainData>();
			for (int i = 0; i < tech.Count; i++)
			{
				foreach (KeyValuePair<TerrainData.Type, int> movementUnlock in tech[i].movementUnlocks)
				{
					terrains.TryGetValue(movementUnlock.Key, out var value);
					list.Add(value);
				}
			}
			return list;
		}
		return null;
	}

	public bool HasUnlockedMovementOnTerrain(PlayerState player, TerrainData.Type terrainType)
	{
		return GetUnlockedMovements(player).Find((TerrainData x) => x.type == terrainType) != null;
	}

	public List<PlayerAbility.Type> GetUnlockedAbilities(PlayerState player)
	{
		List<TechData> unlockedTech = GetUnlockedTech(player);
		if (unlockedTech != null)
		{
			List<PlayerAbility.Type> list = new List<PlayerAbility.Type>();
			for (int i = 0; i < unlockedTech.Count; i++)
			{
				for (int j = 0; j < unlockedTech[i].abilityUnlocks.Count; j++)
				{
					list.Add(unlockedTech[i].abilityUnlocks[j]);
				}
			}
			return list;
		}
		return null;
	}

	public int GetUnlockedDefenceBonus(PlayerState player, TerrainData.Type terrain)
	{
		List<TechData> unlockedTech = GetUnlockedTech(player);
		if (unlockedTech != null)
		{
			for (int i = 0; i < unlockedTech.Count; i++)
			{
				if (unlockedTech[i].defenceBonusUnlocks != null && unlockedTech[i].defenceBonusUnlocks.Count != 0 && unlockedTech[i].defenceBonusUnlocks.TryGetValue(terrain, out var value))
				{
					return value;
				}
			}
		}
		return 1;
	}

	public List<TechData> GetUnlockableTech(PlayerState player)
	{
		if (TryGetData(player.tribe, out var data))
		{
			List<TechData> list = new List<TechData>();
			for (int i = 0; i < player.availableTech.Count; i++)
			{
				if (!TryGetData(player.availableTech[i], out var data2))
				{
					continue;
				}
				data2 = GetOverride(data2, data);
				foreach (TechData techUnlock in data2.techUnlocks)
				{
					TechData techData = GetOverride(techUnlock, data);
					if (!player.HasTech(techData.type) && !list.Contains(techData))
					{
						list.Add(techData);
					}
				}
			}
			return list;
		}
		return null;
	}

	public List<ImprovementData> GetUnlockableImprovements(PlayerState player)
	{
		List<ImprovementData> list = new List<ImprovementData>();
		foreach (TechData item in GetUnlockableTech(player))
		{
			if (item.improvementUnlocks == null || item.improvementUnlocks.Count <= 0)
			{
				continue;
			}
			foreach (ImprovementData improvementUnlock in item.improvementUnlocks)
			{
				list.Add(improvementUnlock);
			}
		}
		return list;
	}

	public List<ImprovementData> GetImprovementsForTile(GameState gameState, TileData tile, PlayerState player)
	{
		List<ImprovementData> list = new List<ImprovementData>();
		foreach (ImprovementData unlockedImprovement in GetUnlockedImprovements(player))
		{
			if (CanBuild(gameState, tile, player, unlockedImprovement))
			{
				list.Add(unlockedImprovement);
			}
		}
		foreach (ImprovementData unlockableImprovement in GetUnlockableImprovements(player))
		{
			if (CanBuild(gameState, tile, player, unlockableImprovement))
			{
				list.Add(unlockableImprovement);
			}
		}
		return list;
	}

	public bool IsUnlocked(TechData.Type type, PlayerState player)
	{
		return player.HasTech(type);
	}

	public bool IsUnlocked(UnitData.Type type, PlayerState player)
	{
		if (player != null && player.availableTech != null)
		{
			for (int i = 0; i < player.availableTech.Count; i++)
			{
				if (TryGetData(player.availableTech[i], out var data) && data.unitUnlocks.Contains(type))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsUnlocked(ImprovementData.Type type, PlayerState player)
	{
		if (player == null)
		{
			return false;
		}
		if (player.availableTech != null && TryGetData(player.tribe, out var data))
		{
			for (int i = 0; i < player.availableTech.Count; i++)
			{
				if (!TryGetData(player.availableTech[i], out var data2))
				{
					continue;
				}
				for (int j = 0; j < data2.improvementUnlocks.Count; j++)
				{
					if (data2.improvementUnlocks[j].type == type)
					{
						return true;
					}
					if (GetOverride(data2.improvementUnlocks[j], data).type == type)
					{
						return true;
					}
				}
			}
		}
		if (player.tasks != null && player.tasks.Count > 0)
		{
			for (int k = 0; k < player.tasks.Count; k++)
			{
				if (TryGetData(player.tasks[k].GetTaskType(), out var data3) && data3.improvementUnlocks.Contains(type))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsUnlocked(PlayerAbility.Type type, PlayerState player)
	{
		if (player != null && player.availableTech != null)
		{
			for (int i = 0; i < player.availableTech.Count; i++)
			{
				if (TryGetData(player.availableTech[i], out var data) && data.abilityUnlocks.Contains(type))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsUnlockable(TechData.Type type, PlayerState player)
	{
		if (TryGetData(player.tribe, out var data))
		{
			List<TechData> list = new List<TechData>();
			for (int i = 0; i < player.availableTech.Count; i++)
			{
				if (!TryGetData(player.availableTech[i], out var data2))
				{
					continue;
				}
				data2 = GetOverride(data2, data);
				foreach (TechData techUnlock in data2.techUnlocks)
				{
					TechData techData = GetOverride(techUnlock, data);
					if (!player.HasTech(techData.type) && !list.Contains(techData) && techData.type == type)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsUnlockable(UnitData.Type type, PlayerState player)
	{
		if (player == null)
		{
			return false;
		}
		foreach (TechData item in GetUnlockableTech(player))
		{
			if (item.unitUnlocks == null || item.unitUnlocks.Count <= 0)
			{
				continue;
			}
			foreach (UnitData unitUnlock in item.unitUnlocks)
			{
				if (unitUnlock.type == type)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsUnlockable(ImprovementData.Type type, PlayerState player)
	{
		if (player == null)
		{
			return false;
		}
		foreach (TechData item in GetUnlockableTech(player))
		{
			if (item.improvementUnlocks == null || item.improvementUnlocks.Count <= 0)
			{
				continue;
			}
			foreach (ImprovementData improvementUnlock in item.improvementUnlocks)
			{
				if (improvementUnlock.type == type)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsUnlockable(PlayerAbility.Type type, PlayerState player)
	{
		if (player == null)
		{
			return false;
		}
		foreach (TechData item in GetUnlockableTech(player))
		{
			if (item.abilityUnlocks == null || item.abilityUnlocks.Count <= 0)
			{
				continue;
			}
			foreach (PlayerAbility.Type abilityUnlock in item.abilityUnlocks)
			{
				if (abilityUnlock == type)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAlienClimate(int climate)
	{
		if (alienClimates == null || alienClimates.Count == 0)
		{
			return false;
		}
		return alienClimates.Contains(climate);
	}

	public TechData GetOverride(TechData techData, TribeData tribe)
	{
		if (tribe != null && tribe.techOverrides != null && tribe.techOverrides.Count > 0 && tribe.techOverrides.TryGetValue(techData.type, out var value))
		{
			return value;
		}
		return techData;
	}

	public ImprovementData GetOverride(ImprovementData improvementData, TribeData tribe)
	{
		if (tribe != null && tribe.improvementOverrides != null && tribe.improvementOverrides.Count > 0 && tribe.improvementOverrides.TryGetValue(improvementData.type, out var value))
		{
			return value;
		}
		return improvementData;
	}

	public ImprovementData GetOriginal(ImprovementData improvementData, TribeData tribe)
	{
		if (tribe != null && tribe.improvementOverrides != null && tribe.improvementOverrides.Count > 0)
		{
			foreach (KeyValuePair<ImprovementData.Type, ImprovementData> improvementOverride in tribe.improvementOverrides)
			{
				if (improvementOverride.Value == improvementData && TryGetData(improvementOverride.Key, out var data))
				{
					return data;
				}
			}
		}
		return improvementData;
	}

	public UnitData GetOverride(UnitData unitData, TribeData tribe)
	{
		if (tribe != null && tribe.unitOverrides != null && tribe.unitOverrides.Count > 0 && tribe.unitOverrides.TryGetValue(unitData.type, out var value))
		{
			return value;
		}
		return unitData;
	}

	public bool HaveValidTribesToPick(GameSettings settings)
	{
		foreach (KeyValuePair<TribeData.Type, TribeData> tribe in tribes)
		{
			if (!settings.IsTribeLocked(tribe.Key) && settings.IsTribeEnabled(tribe.Key))
			{
				return true;
			}
		}
		return false;
	}

	public List<TribeData> GetTribes(TribeData.CategoryEnum category)
	{
		TribeData[] array = tribes.Values.ToArray();
		List<TribeData> list = new List<TribeData>();
		TribeData[] array2 = array;
		foreach (TribeData tribeData in array2)
		{
			if (tribeData.category == category)
			{
				list.Add(tribeData);
			}
		}
		list.Sort((TribeData x, TribeData y) => x.style.CompareTo(y.style));
		return list;
	}

	public List<TribeData> GetAllTribes()
	{
		List<TribeData> list = new List<TribeData>();
		foreach (KeyValuePair<TribeData.Type, TribeData> tribe in tribes)
		{
			if (tribe.Key != TribeData.Type.None && tribe.Key != TribeData.Type.Nature)
			{
				list.Add(tribe.Value);
			}
		}
		list.Sort((TribeData x, TribeData y) => x.style.CompareTo(y.style));
		return list;
	}

	public List<TribeData.Type> GetAllTribeTypes()
	{
		return new List<TribeData.Type>(tribes.Keys.ToArray());
	}

	public List<ResourceData> GetAllResources()
	{
		List<ResourceData> list = new List<ResourceData>();
		foreach (KeyValuePair<ResourceData.Type, ResourceData> resource in resources)
		{
			list.Add(resource.Value);
		}
		return list;
	}

	public ImprovementData GetImprovementForResource(ResourceData.Type resource)
	{
		foreach (ImprovementData value in AllImprovementData.Values)
		{
			if (value.terrainRequirements == null)
			{
				continue;
			}
			foreach (TerrainRequirements terrainRequirement in value.terrainRequirements)
			{
				if (terrainRequirement.resource != null && terrainRequirement.resource.type != ResourceData.Type.None && terrainRequirement.resource.type == resource)
				{
					return value;
				}
			}
		}
		return null;
	}

	public ImprovementData GetActionableImprovementForResource(ResourceData.Type resource, PlayerState player)
	{
		List<ImprovementData> unlockedImprovements = GetUnlockedImprovements(player);
		if (unlockedImprovements == null || unlockedImprovements.Count == 0)
		{
			return null;
		}
		foreach (ImprovementData item in unlockedImprovements)
		{
			if (item.terrainRequirements == null)
			{
				continue;
			}
			foreach (TerrainRequirements terrainRequirement in item.terrainRequirements)
			{
				if (terrainRequirement.resource != null && terrainRequirement.resource.type != ResourceData.Type.None && terrainRequirement.resource.type == resource)
				{
					return item;
				}
			}
		}
		return null;
	}

	public TechData GetTechThatUnlocks(ImprovementData improvement, TribeData tribe)
	{
		ImprovementData original = GetOriginal(improvement, tribe);
		foreach (TechData value in AllTechData.Values)
		{
			if (value.improvementUnlocks.Contains(original))
			{
				return value;
			}
		}
		return null;
	}

	public TechData GetTechThatUnlocks(TerrainData.Type type)
	{
		foreach (TechData value in AllTechData.Values)
		{
			if (value.movementUnlocks.ContainsKey(type))
			{
				return value;
			}
		}
		return null;
	}

	public int GetTechPrice(TechData techData, PlayerState playerState, GameState state)
	{
		if (techData == null || techData.cost == 0)
		{
			return 0;
		}
		float num = 4f;
		num += (float)techData.cost;
		int cities = playerState.cities;
		num += (float)((cities - 1) * techData.cost);
		if (HasAbility(playerState, PlayerAbility.Type.Literacy))
		{
			float num2 = 0.66666f;
			if (state.Version < 40)
			{
				num2 = 0.8f;
			}
			num *= num2;
		}
		return (int)Math.Ceiling(num);
	}

	public bool HasAbility(PlayerState player, PlayerAbility.Type ability)
	{
		foreach (PlayerAbility.Type unlockedAbility in GetUnlockedAbilities(player))
		{
			if (unlockedAbility == ability)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsResourceRequiredByImprovement(ResourceData.Type type, ImprovementData improvement)
	{
		if (improvement.terrainRequirements != null && improvement.terrainRequirements.Count > 0)
		{
			for (int i = 0; i < improvement.terrainRequirements.Count; i++)
			{
				TerrainRequirements terrainRequirements = improvement.terrainRequirements[i];
				if (terrainRequirements.resource != null && terrainRequirements.resource.type == type)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanBuild(GameState gameState, TileData tile, PlayerState playerState, ImprovementData improvement)
	{
		if (improvement.hidden)
		{
			return false;
		}
		bool flag = improvement.type == ImprovementData.Type.Road;
		if ((tile.improvement != null && !flag) || (flag && tile.HasRoad))
		{
			return false;
		}
		if ((!flag && tile.owner != playerState.Id) || (flag && tile.owner != playerState.Id && tile.owner != 0))
		{
			return false;
		}
		UnitState unit = tile.GetUnit(gameState, playerState.Id);
		if (unit != null && unit.owner != playerState.Id && !playerState.HasPeaceWith(unit.owner))
		{
			return false;
		}
		if (improvement.HasAbility(ImprovementAbility.Type.Limited) && HasImprovementWithinCityBorders(gameState.Map, tile.rulingCityCoordinates, improvement.type))
		{
			return false;
		}
		if (improvement.HasAbility(ImprovementAbility.Type.Unique) && HasPlayerBuiltUniqueImprovement(gameState, playerState, improvement.type))
		{
			return false;
		}
		bool num = MeetsRequirement(tile, improvement.terrainRequirements);
		bool flag2 = MeetsRequirement(gameState.Map, tile, improvement.adjacencyRequirements);
		return num && flag2;
	}

	public bool MeetsRequirement(TileData tile, List<TerrainRequirements> requirements)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		if (requirements != null && requirements.Count > 0)
		{
			foreach (TerrainRequirements requirement in requirements)
			{
				if (requirement.resource != null && requirement.resource.type != ResourceData.Type.None)
				{
					flag4 = true;
					if (tile.resource != null && tile.resource.type == requirement.resource.type)
					{
						flag2 = true;
					}
				}
				if (requirement.terrain != null && requirement.terrain.type != TerrainData.Type.None)
				{
					flag3 = true;
					if (tile.terrain == requirement.terrain.type)
					{
						flag = true;
					}
				}
			}
		}
		if (!flag3 || flag)
		{
			return !flag4 || flag2;
		}
		return false;
	}

	public bool MeetsRequirement(MapData map, TileData tile, List<AdjacencyRequirements> requirements)
	{
		if (requirements != null && requirements.Count > 0)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			foreach (TileData tileNeighbor in map.GetTileNeighbors(tile.coordinates))
			{
				foreach (AdjacencyRequirements requirement in requirements)
				{
					if (!flag2 && requirement.improvement != null && tileNeighbor.improvement != null)
					{
						flag2 = tileNeighbor.improvement.type == requirement.improvement.type && tileNeighbor.owner == tile.owner;
					}
					if (!flag && requirement.terrain != null && requirement.terrain.type != TerrainData.Type.None && requirement.terrain.type != TerrainData.Type.None)
					{
						flag = tileNeighbor.terrain == requirement.terrain.type && tileNeighbor.owner == tile.owner;
					}
					if (!flag3 && requirement.resource != null && requirement.resource.type != ResourceData.Type.None && requirement.resource.type != ResourceData.Type.None)
					{
						flag3 = tileNeighbor.resource.type == requirement.resource.type && tileNeighbor.owner == tile.owner;
					}
				}
			}
			return flag || flag2 || flag3;
		}
		return true;
	}

	public bool HasImprovementWithinCityBorders(MapData map, WorldCoordinates cityCoordinates, ImprovementData.Type improvementType)
	{
		List<TileData> area = map.GetArea(cityCoordinates, 2, allowDiagonal: true, includeCenter: false);
		for (int i = 0; i < area.Count; i++)
		{
			TileData tileData = area[i];
			if (!(tileData.rulingCityCoordinates != cityCoordinates) && tileData.HasImprovement(improvementType))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HasPlayerUniqueImprovement(GameState gameState, PlayerState playerState, ImprovementData.Type improvementType)
	{
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (tileData.improvement != null && tileData.HasImprovement(improvementType) && tileData.improvement.founder == playerState.Id)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasPlayerBuiltUniqueImprovement(GameState gameState, PlayerState playerState, ImprovementData.Type improvementType)
	{
		if (gameState.Version <= 20)
		{
			return HasPlayerUniqueImprovement(gameState, playerState, improvementType);
		}
		if (playerState.builtUniqueImprovements == null)
		{
			return false;
		}
		return playerState.builtUniqueImprovements.Contains(improvementType);
	}
}
