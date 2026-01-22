using System;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
	[Serializable]
	public class UnitPrefabData
	{
		public UnitData.Type type;

		public Unit prefab;
	}

	[Serializable]
	public class UnitOverridePrefabData
	{
		public OverrideCondition overrideCondition;

		public UnitData.Type type;

		public Unit prefab;
	}

	[Serializable]
	public class WeaponPrefabData
	{
		public UnitData.WeaponEnum type;

		public WeaponGFX prefab;
	}

	[Serializable]
	public class ImprovementPrefabData
	{
		public ImprovementData.Type type;

		public Building prefab;
	}

	[Serializable]
	public class ResourcePrefabData
	{
		public ResourceData.Type type;

		public Resource prefab;
	}

	[Serializable]
	public class GFXPrefabData
	{
		public GFXType type;

		public GameObject prefab;
	}

	public UnitPrefabData[] unitPrefabs;

	public UnitOverridePrefabData[] unitOverridePrefabs;

	public WeaponPrefabData[] weaponPrefabs;

	public ImprovementPrefabData[] improvementPrefabs;

	public ResourcePrefabData[] resourcePrefabs;

	public GFXPrefabData[] gfxPrefabs;

	protected static PrefabManager instance;

	protected static Dictionary<UnitData.Type, Unit> units = new Dictionary<UnitData.Type, Unit>();

	protected static Dictionary<OverrideCondition, Dictionary<UnitData.Type, Unit>> unitOverrides = new Dictionary<OverrideCondition, Dictionary<UnitData.Type, Unit>>();

	protected static Dictionary<UnitData.WeaponEnum, WeaponGFX> weapons = new Dictionary<UnitData.WeaponEnum, WeaponGFX>();

	protected static Dictionary<ImprovementData.Type, Building> improvements = new Dictionary<ImprovementData.Type, Building>();

	protected static Dictionary<ResourceData.Type, Resource> resources = new Dictionary<ResourceData.Type, Resource>();

	protected static Dictionary<GFXType, GameObject> gfx = new Dictionary<GFXType, GameObject>();

	public void Awake()
	{
		instance = this;
		SetupData();
	}

	private void SetupData()
	{
		UnitPrefabData[] array = unitPrefabs;
		foreach (UnitPrefabData unitPrefabData in array)
		{
			units.Add(unitPrefabData.type, unitPrefabData.prefab);
		}
		UnitOverridePrefabData[] array2 = unitOverridePrefabs;
		foreach (UnitOverridePrefabData unitOverridePrefabData in array2)
		{
			if (!unitOverrides.TryGetValue(unitOverridePrefabData.overrideCondition, out var value))
			{
				value = new Dictionary<UnitData.Type, Unit>();
				unitOverrides[unitOverridePrefabData.overrideCondition] = value;
			}
			value.Add(unitOverridePrefabData.type, unitOverridePrefabData.prefab);
		}
		WeaponPrefabData[] array3 = weaponPrefabs;
		foreach (WeaponPrefabData weaponPrefabData in array3)
		{
			weapons.Add(weaponPrefabData.type, weaponPrefabData.prefab);
		}
		ImprovementPrefabData[] array4 = improvementPrefabs;
		foreach (ImprovementPrefabData improvementPrefabData in array4)
		{
			improvements.Add(improvementPrefabData.type, improvementPrefabData.prefab);
		}
		ResourcePrefabData[] array5 = resourcePrefabs;
		foreach (ResourcePrefabData resourcePrefabData in array5)
		{
			resources.Add(resourcePrefabData.type, resourcePrefabData.prefab);
		}
		GFXPrefabData[] array6 = gfxPrefabs;
		foreach (GFXPrefabData gFXPrefabData in array6)
		{
			gfx.Add(gFXPrefabData.type, gFXPrefabData.prefab);
		}
	}

	public static Unit GetPrefab(UnitData.Type type, OverrideCondition overrideCondition)
	{
		if (unitOverrides.TryGetValue(overrideCondition, out var value) && value.TryGetValue(type, out var value2))
		{
			return value2;
		}
		return null;
	}

	public static Unit GetPrefab(UnitData.Type type)
	{
		if (SeasonManager.IsChristmas())
		{
			Unit prefab = GetPrefab(type, OverrideCondition.Christmas);
			if ((Object)(object)prefab != (Object)null)
			{
				return prefab;
			}
		}
		if (units.ContainsKey(type))
		{
			return units[type];
		}
		Log.Warning("Couldn't find prefab for type: {0}", new object[1] { type });
		return units[UnitData.Type.None];
	}

	public static WeaponGFX GetPrefab(UnitData.WeaponEnum type)
	{
		if (weapons.ContainsKey(type))
		{
			return weapons[type];
		}
		Log.Warning("Couldn't find prefab for type: {0}", new object[1] { type });
		return null;
	}

	public static Building GetPrefab(ImprovementData.Type type)
	{
		if (improvements.TryGetValue(type, out var value))
		{
			return value;
		}
		Log.Warning("Couldn't find prefab for type: {0}", new object[1] { type });
		return null;
	}

	public static Resource GetPrefab(ResourceData.Type type)
	{
		if (resources.TryGetValue(type, out var value))
		{
			return value;
		}
		Log.Warning("Couldn't find prefab for type: {0}", new object[1] { type });
		return null;
	}

	public static GameObject GetPrefab(GFXType type)
	{
		if (gfx.ContainsKey(type))
		{
			return gfx[type];
		}
		Log.Warning("Couldn't find prefab for type: {0}", new object[1] { type });
		return null;
	}

	public static bool HavePrefab(UnitData.Type type)
	{
		return units.ContainsKey(type);
	}

	public static bool HavePrefab(UnitData.WeaponEnum type)
	{
		return weapons.ContainsKey(type);
	}

	public static bool HavePrefab(ImprovementData.Type type)
	{
		return improvements.ContainsKey(type);
	}

	public static bool HavePrefab(ResourceData.Type type)
	{
		return resources.ContainsKey(type);
	}

	public static bool HavePrefab(GFXType type)
	{
		return gfx.ContainsKey(type);
	}
}
