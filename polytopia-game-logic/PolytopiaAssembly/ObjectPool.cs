using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
	[Serializable]
	public class PoolData
	{
		public GameObject prefab;

		public int spawnCount = 1;

		public PooledObjectData.SpawnSettings spawnSettings;
	}

	public class PooledObjectData
	{
		public enum SpawnSettings
		{
			None,
			CreateIfNull
		}

		public List<GameObject> pooledObjects = new List<GameObject>();

		public List<IPooledObject> pooledObjectIFs = new List<IPooledObject>();

		public SpawnSettings spawnSetting;

		public string id = "None";

		private int currIdx;

		private GameObject prefab;

		public GameObject Prefab
		{
			set
			{
				prefab = value;
			}
		}

		public GameObject PoolObject(GameObject prefab)
		{
			this.prefab = prefab;
			GameObject val = Object.Instantiate<GameObject>(prefab);
			val.SetActive(false);
			((Object)val).name = id;
			IPooledObject component = val.GetComponent<IPooledObject>();
			if (component != null)
			{
				pooledObjectIFs.Add(component);
			}
			else
			{
				pooledObjects.Add(val);
			}
			val.transform.SetParent(((Component)instance).transform);
			UIBasicComponent component2 = val.GetComponent<UIBasicComponent>();
			if ((Object)(object)component2 != (Object)null)
			{
				component2.Init();
			}
			return val;
		}

		public void ReturnObject(GameObject pooledObject)
		{
			currIdx--;
			if (currIdx < 0)
			{
				currIdx = 0;
			}
			pooledObject.SetActive(false);
			pooledObject.transform.SetParent(((Component)instance).transform, false);
			IPooledObject component = pooledObject.GetComponent<IPooledObject>();
			if (component != null)
			{
				component.IsUsed = false;
			}
		}

		public void ReturnObject(IPooledObject pooledObject)
		{
			GameObject gameObject = pooledObject.gameObject;
			gameObject.SetActive(false);
			gameObject.transform.SetParent(((Component)instance).transform, false);
			pooledObject.IsUsed = false;
		}

		public T GetPooledObject<T>() where T : class, IPooledObject
		{
			int count = pooledObjectIFs.Count;
			for (int i = 0; i < count; i++)
			{
				IPooledObject pooledObject = pooledObjectIFs[i];
				if (!pooledObject.IsUsed)
				{
					return pooledObject as T;
				}
			}
			if (spawnSetting == SpawnSettings.CreateIfNull)
			{
				return PoolObject(prefab).GetComponent<T>();
			}
			return null;
		}

		public GameObject GetPooledObject()
		{
			int count = pooledObjects.Count;
			if (count > 0 && currIdx < count)
			{
				for (int i = currIdx; i < count; i++)
				{
					if (!pooledObjects[i].activeSelf)
					{
						IPooledObject component = pooledObjects[i].GetComponent<IPooledObject>();
						if (component == null)
						{
							return pooledObjects[i];
						}
						if (!component.IsUsed)
						{
							return pooledObjects[i];
						}
					}
				}
			}
			if (spawnSetting == SpawnSettings.CreateIfNull)
			{
				return PoolObject(prefab);
			}
			return null;
		}
	}

	[SerializeField]
	protected PoolData[] startData;

	public static ObjectPool instance;

	protected static Dictionary<string, PooledObjectData> pooledObjects = new Dictionary<string, PooledObjectData>();

	private void Awake()
	{
		if ((Object)(object)instance != (Object)null && Object.op_Implicit((Object)(object)((Component)instance).gameObject))
		{
			Object.Destroy((Object)(object)((Component)instance).gameObject);
		}
		instance = this;
		pooledObjects.Clear();
		int num = startData.Length;
		for (int i = 0; i < num; i++)
		{
			PoolData poolData = startData[i];
			string name = ((Object)poolData.prefab).name;
			PooledObjectData pooledObjectData = new PooledObjectData();
			pooledObjectData.spawnSetting = poolData.spawnSettings;
			pooledObjectData.id = name;
			pooledObjects.Add(name, pooledObjectData);
			int spawnCount = startData[i].spawnCount;
			for (int j = 0; j < spawnCount; j++)
			{
				pooledObjectData.PoolObject(poolData.prefab);
			}
		}
	}

	private void OnDestroy()
	{
		instance = null;
		pooledObjects.Clear();
	}

	public static T GetPooledObject<T>(string id) where T : class, IPooledObject
	{
		if (pooledObjects.TryGetValue(id, out var value))
		{
			return value.GetPooledObject<T>();
		}
		return null;
	}

	public static GameObject GetPooledObject(string id)
	{
		if (pooledObjects.TryGetValue(id, out var value))
		{
			return value.GetPooledObject();
		}
		return null;
	}

	public static T GetPooledObject<T>(GameObject prefab, PooledObjectData.SpawnSettings spawnSettings = PooledObjectData.SpawnSettings.CreateIfNull) where T : IPooledObject
	{
		return GetPooledObject(prefab, spawnSettings).GetComponent<T>();
	}

	public static GameObject GetPooledObject(GameObject prefab, PooledObjectData.SpawnSettings spawnSettings = PooledObjectData.SpawnSettings.CreateIfNull)
	{
		string name = ((Object)prefab).name;
		pooledObjects.TryGetValue(name, out var value);
		if (value == null)
		{
			value = new PooledObjectData();
			value.spawnSetting = spawnSettings;
			value.id = ((Object)prefab).name;
			value.Prefab = prefab;
			pooledObjects.Add(name, value);
		}
		return value.GetPooledObject();
	}

	public static void ReturnObject(GameObject pooledObject)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			string name = ((Object)pooledObject).name;
			if (pooledObjects.TryGetValue(name, out var value))
			{
				value.ReturnObject(pooledObject);
				return;
			}
			Log.Error("Trying to return pooled object with no pool, object name : {0} :: name {1}", new object[2]
			{
				((Object)pooledObject).name,
				name
			});
		}
	}

	public static void ReturnObject(IPooledObject pooledObject)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			string name = ((Object)pooledObject.gameObject).name;
			if (pooledObjects.TryGetValue(name, out var value))
			{
				value.ReturnObject(pooledObject);
				return;
			}
			Log.Error("Trying to return pooled object with no pool, object name : {0} :: name {1}", new object[2]
			{
				((Object)pooledObject.gameObject).name,
				name
			});
		}
	}
}
