using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICityPlot : MonoBehaviour
{
	public int floors;

	public List<GameObject> houses = new List<GameObject>();

	[HideInInspector]
	public float scale = 1f;

	public float pixelsPerUnit = 1f;

	public void AddHouse(GameObject house)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Image component = house.GetComponent<Image>();
		houses.Add(house);
		house.transform.SetParent(((Component)this).transform);
		float num = 0f;
		float num2 = (float)floors * (20f * scale);
		Bounds bounds = component.sprite.bounds;
		float num3 = (num2 - ((Bounds)(ref bounds)).size.y) / pixelsPerUnit;
		house.transform.localPosition = new Vector3(num, num3);
		floors++;
	}

	public GameObject getHouse(int index)
	{
		if (index >= 0 && index < houses.Count)
		{
			return houses[index];
		}
		return null;
	}

	public SpriteRenderer GetHouseRenderer(int index)
	{
		if (index >= 0 && index < houses.Count)
		{
			return houses[index].GetComponent<SpriteRenderer>();
		}
		return null;
	}

	public void Clear()
	{
		if (houses.Count > 0)
		{
			int count = houses.Count;
			for (int i = 0; i < count; i++)
			{
				Object.Destroy((Object)(object)houses[i]);
			}
			houses.Clear();
		}
	}
}
