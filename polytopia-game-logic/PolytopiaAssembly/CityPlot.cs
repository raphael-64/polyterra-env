using System.Collections.Generic;
using UnityEngine;

public class CityPlot : MonoBehaviour
{
	public int floors;

	public int sortingOrder;

	public List<PolytopiaSpriteRenderer> houses = new List<PolytopiaSpriteRenderer>();

	[HideInInspector]
	public float scale = 1f;

	public float pixelsPerUnit = 1f;

	public void AddHouse(PolytopiaSpriteRenderer house)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		house.SortingLayer = MeshCache.TERRAIN_LAYER_ID;
		house.SortingOrder = sortingOrder + houses.Count;
		houses.Add(house);
		((Component)house).transform.SetParent(((Component)this).transform);
		float num = 0f;
		float num2 = (float)floors * (20f * scale);
		Bounds bounds = house.Bounds;
		float num3 = (num2 - ((Bounds)(ref bounds)).size.y) / pixelsPerUnit;
		((Component)house).transform.localPosition = new Vector3(num, num3);
		floors++;
	}

	public PolytopiaSpriteRenderer GetHouse(int index)
	{
		if (index >= 0 && index < houses.Count)
		{
			return houses[index];
		}
		return null;
	}

	public int GetHighestSortingOrder()
	{
		return sortingOrder + GetHouse(houses.Count - 1).SortingOrder;
	}

	public void Clear()
	{
		if (houses.Count > 0)
		{
			int count = houses.Count;
			for (int i = 0; i < count; i++)
			{
				houses[i].ReturnToPool();
			}
			houses.Clear();
		}
	}
}
