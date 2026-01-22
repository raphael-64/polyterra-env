using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.UI;

public class UICityRenderer : MonoBehaviour
{
	public int tribe = 1;

	public int level = 1;

	public bool isCapital;

	public bool showWall;

	public bool buildAtStart;

	[HideInInspector]
	public int sortOrder;

	private static int PIXELS_PER_UNIT = 10;

	private static float SCALE = 5.097063f;

	private static Vector2 BASE_SIZE = new Vector2(393f, 235.8f);

	private GameObject city;

	private List<UICityPlot> plots = new List<UICityPlot>();

	private GameObject wall;

	private bool wallActive;

	public bool WallActive
	{
		get
		{
			if ((Object)(object)wall != (Object)null)
			{
				return wall.activeSelf;
			}
			return false;
		}
		set
		{
			wallActive = value;
			if ((Object)(object)wall != (Object)null)
			{
				wall.SetActive(wallActive);
			}
		}
	}

	public void CreateCity(int tribe, int level = 1, bool isCapital = false, SkinType skinType = SkinType.Default)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		Clear();
		city = new GameObject();
		city.transform.SetParent(((Component)this).transform);
		((Object)city).name = "city";
		int num = 0;
		int num2 = 4;
		if (level <= 0)
		{
			level = 1;
		}
		if (level >= 2)
		{
			num2 = 9;
		}
		if (level >= 5)
		{
			num2 = 16;
		}
		int num3 = Mathf.RoundToInt(Mathf.Sqrt((float)num2));
		int num4 = num3 - 1;
		int num5 = num3 - 1;
		float num6 = 50f * SCALE;
		int num7 = Mathf.RoundToInt(num6 * 4f) / PIXELS_PER_UNIT;
		int num8 = Mathf.RoundToInt(num6 * 8f * 0.3f) / PIXELS_PER_UNIT;
		Bounds val = default(Bounds);
		((Bounds)(ref val))._002Ector(Vector3.zero, new Vector3(BASE_SIZE.x / (float)PIXELS_PER_UNIT, BASE_SIZE.y / (float)PIXELS_PER_UNIT, 0f));
		while (num4 >= 0)
		{
			for (num5 = num3 - 1; num5 >= 0; num5--)
			{
				GameObject val2 = new GameObject
				{
					name = $"plot_{num4}_{num5}"
				};
				val2.transform.SetParent(city.transform);
				UICityPlot uICityPlot = val2.AddComponent<UICityPlot>();
				uICityPlot.floors = 0;
				uICityPlot.pixelsPerUnit = PIXELS_PER_UNIT;
				uICityPlot.scale = SCALE;
				float num9 = (float)(num4 - num5) * num6 / (float)PIXELS_PER_UNIT * 0.5f;
				float num10 = (float)(num5 + num4) * num6 / (float)PIXELS_PER_UNIT * 0.3f - 0.24f;
				((Bounds)(ref val)).Encapsulate(new Vector3(num9, num10, 0f));
				((Component)uICityPlot).transform.localPosition = new Vector3(num9, num10);
				val2.SetActive(false);
				plots.Add(uICityPlot);
			}
			num4--;
		}
		plots.Reverse();
		UICityPlot uICityPlot2 = plots[0];
		float num11 = 0f - ((Bounds)(ref val)).min.x;
		num11 += ((float)num7 - ((Bounds)(ref val)).size.x) * 0.5f;
		float num12 = ((float)num8 - ((Bounds)(ref val)).size.y) * 0.5f;
		foreach (UICityPlot plot in plots)
		{
			Vector3 localPosition = ((Component)plot).transform.localPosition;
			localPosition.x += num11 / (float)PIXELS_PER_UNIT;
			localPosition.y += num12 / (float)PIXELS_PER_UNIT;
			((Component)plot).transform.localPosition = localPosition;
		}
		city.transform.localPosition = new Vector3(0f, 0.02f + (0.15f - (float)(num3 - 2) * 0.075f), 0f);
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		for (int i = 1; i <= level; i++)
		{
			if (i < 6)
			{
				list2.Add(i);
			}
		}
		int num13 = level * 4;
		num13 += num13 * num13 / 10;
		for (int j = 1; j < num13; j++)
		{
			int index = Random.Range(0, list2.Count);
			int item = list2[index];
			list.Insert((list.Count > 0) ? Random.Range(0, list.Count - 1) : 0, item);
		}
		while (list.Count > 0)
		{
			num += Mathf.CeilToInt(Random.value * 1.5f);
			if (num > num2)
			{
				num = 1;
			}
			UICityPlot uICityPlot3 = plots[num - 1];
			((Component)uICityPlot3).gameObject.SetActive(true);
			GameObject house = GetHouse(tribe, list[0], skinType);
			uICityPlot3.AddHouse(house);
			list.RemoveAt(0);
		}
		if (isCapital)
		{
			GameObject house2 = GetHouse(tribe, 7, skinType);
			uICityPlot2.AddHouse(house2);
		}
		wall = GetResource(new SpriteAddress[1]
		{
			new SpriteAddress("TerrainFeatures", "CityWallGFX")
		});
		((Object)wall).name = "Wall";
		wall.transform.SetParent(city.transform);
		float num14 = 0f;
		float num15 = (float)(num8 / PIXELS_PER_UNIT) - city.transform.localPosition.y;
		wall.transform.localPosition = new Vector3(num14, num15, 0f);
		WallActive = wallActive;
		city.transform.localScale = Vector3.one;
	}

	private GameObject GetHouse(int tribe = 1, int type = 1, SkinType skinType = SkinType.Default)
	{
		GameObject resource = GetResource(SpriteData.GetHouseAddresses(type, tribe.ToString(), skinType));
		((Object)resource).name = $"House_{tribe}_{type}";
		return resource;
	}

	private GameObject GetResource(SpriteAddress[] spriteAddresses)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		GameObject val = new GameObject();
		Image image = val.AddComponent<Image>();
		image.useSpriteMesh = true;
		GameManager.GetSpriteAtlasManager().LoadSprite(spriteAddresses, delegate(string atlasName, string spriteName, Sprite sprite)
		{
			image.sprite = sprite;
			((Graphic)image).SetNativeSize();
		});
		return val;
	}

	public void Clear()
	{
		if ((Object)(object)city != (Object)null)
		{
			int count = plots.Count;
			for (int i = 0; i < count; i++)
			{
				plots[i].Clear();
				Object.Destroy((Object)(object)plots[i]);
			}
			plots.Clear();
			Object.Destroy((Object)(object)wall);
			Object.Destroy((Object)(object)city);
		}
	}
}
