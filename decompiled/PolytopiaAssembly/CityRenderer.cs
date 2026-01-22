using System;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class CityRenderer : MonoBehaviour, ISpriteRendererProvider
{
	private int HOUSE_WORKSHOP = 6;

	private int HOUSE_CAPITAL = 7;

	private int HOUSE_PARK = 8;

	public bool buildAtStart;

	[SerializeField]
	protected Material material;

	[HideInInspector]
	public int sortOrder;

	[Info]
	[SerializeField]
	protected string info = "None";

	protected WorldCoordinates m_coordinates;

	protected int m_tribe = 1;

	protected int m_level = 1;

	protected byte m_isCapitalOf;

	protected bool m_haveWall;

	protected bool m_haveWorkshop;

	protected int m_parkCount;

	private List<byte> playerEmbassies = new List<byte>();

	private static int PIXELS_PER_UNIT = 1056;

	private static float SCALE = 5.097063f;

	private static Vector2 BASE_SIZE = new Vector2(393f, 235.8f);

	private GameObject city;

	private List<CityPlot> plots = new List<CityPlot>();

	private PolytopiaSpriteRenderer wall;

	private bool dataChanged = true;

	private bool isEnemyCity;

	private Random rng;

	private List<PolytopiaSpriteRenderer> spriteRenderers = new List<PolytopiaSpriteRenderer>();

	public SkinType SkinType { get; set; }

	public WorldCoordinates Coordinates
	{
		get
		{
			return m_coordinates;
		}
		set
		{
			if (value != m_coordinates)
			{
				dataChanged = true;
			}
			m_coordinates = value;
		}
	}

	public int Tribe
	{
		get
		{
			return m_tribe;
		}
		set
		{
			if (value != m_tribe)
			{
				dataChanged = true;
			}
			m_tribe = value;
		}
	}

	public int Level
	{
		get
		{
			return m_level;
		}
		set
		{
			if (value != m_level)
			{
				dataChanged = true;
			}
			m_level = value;
		}
	}

	public byte IsCapitalOf
	{
		get
		{
			return m_isCapitalOf;
		}
		set
		{
			if (value != m_isCapitalOf)
			{
				dataChanged = true;
			}
			m_isCapitalOf = value;
		}
	}

	public bool HaveWall
	{
		get
		{
			return m_haveWall;
		}
		set
		{
			if (value != m_haveWall)
			{
				dataChanged = true;
			}
			m_haveWall = value;
		}
	}

	public bool HaveWorkshop
	{
		get
		{
			return m_haveWorkshop;
		}
		set
		{
			if (value != m_haveWorkshop)
			{
				dataChanged = true;
			}
			m_haveWorkshop = value;
		}
	}

	public List<byte> PlayerEmbassies
	{
		get
		{
			return playerEmbassies;
		}
		set
		{
			if (playerEmbassies.Count != value.Count)
			{
				dataChanged = true;
			}
			else
			{
				for (int i = 0; i < playerEmbassies.Count; i++)
				{
					if (value[i] != playerEmbassies[i])
					{
						dataChanged = true;
						break;
					}
				}
			}
			playerEmbassies = value;
		}
	}

	public int ParkCount
	{
		get
		{
			return m_parkCount;
		}
		set
		{
			if (value != m_parkCount)
			{
				dataChanged = true;
			}
			m_parkCount = value;
		}
	}

	public bool IsEnemyCity
	{
		get
		{
			return isEnemyCity;
		}
		set
		{
			if (value != isEnemyCity)
			{
				dataChanged = true;
			}
			isEnemyCity = value;
		}
	}

	private void Start()
	{
		if (buildAtStart)
		{
			RefreshCity();
		}
	}

	public void SetVisible(bool visible)
	{
		for (int i = 0; i < plots.Count; i++)
		{
			CityPlot cityPlot = plots[i];
			for (int j = 0; j < cityPlot.houses.Count; j++)
			{
				((Component)cityPlot.houses[j]).gameObject.SetActive(visible);
			}
		}
		if ((Object)(object)wall != (Object)null)
		{
			((Component)wall).gameObject.SetActive(visible && HaveWall);
		}
	}

	public void RefreshCity()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0609: Unknown result type (might be due to invalid IL or missing references)
		if (!dataChanged)
		{
			return;
		}
		Clear();
		int hash = (int)GameManager.GameState.RandomHash.GetHash(m_coordinates.X, m_coordinates.Y);
		rng = new Random(hash);
		city = new GameObject();
		city.transform.SetParent(((Component)this).transform);
		((Object)city).name = "city";
		int currentIndex = 0;
		int num = 4;
		if (Level <= 0)
		{
			Level = 1;
		}
		if (Level >= 2)
		{
			num = 9;
		}
		if (Level >= 5)
		{
			num = 16;
		}
		int num2 = Mathf.RoundToInt(Mathf.Sqrt((float)num));
		float num3 = 50f * SCALE;
		int num4 = Mathf.RoundToInt(num3 * 4f) / PIXELS_PER_UNIT;
		int num5 = Mathf.RoundToInt(num3 * 8f * 0.3f) / PIXELS_PER_UNIT;
		Bounds val = default(Bounds);
		((Bounds)(ref val))._002Ector(Vector3.zero, new Vector3(BASE_SIZE.x / (float)PIXELS_PER_UNIT, BASE_SIZE.y / (float)PIXELS_PER_UNIT, 0f));
		int num6 = num2 * 2;
		for (int i = 0; i < num2 * 2; i++)
		{
			int num7 = Mathf.Min(i + 1, num2);
			int num8 = num7 - 1;
			int num9 = i - num8;
			while (num8 >= 0 && num9 < num7)
			{
				GameObject val2 = new GameObject
				{
					name = $"plot_{num8}_{num9}"
				};
				val2.transform.SetParent(city.transform);
				CityPlot cityPlot = val2.AddComponent<CityPlot>();
				cityPlot.floors = 0;
				cityPlot.pixelsPerUnit = PIXELS_PER_UNIT;
				cityPlot.scale = SCALE;
				float num10 = (float)(num8 - num9) * num3 / (float)PIXELS_PER_UNIT * 0.5f;
				float num11 = (float)(num9 + num8) * num3 / (float)PIXELS_PER_UNIT * 0.3f - 0.24f;
				((Bounds)(ref val)).Encapsulate(new Vector3(num10, num11, 0f));
				((Component)cityPlot).transform.localPosition = new Vector3(num10, num11);
				cityPlot.sortingOrder = sortOrder + 6 + num6 + (num2 * 2 - i - 1);
				val2.SetActive(false);
				plots.Add(cityPlot);
				num6--;
				num9++;
				num8--;
			}
		}
		CityPlot cityPlot2 = plots[0];
		plots.Reverse();
		float num12 = 0f - ((Bounds)(ref val)).min.x;
		num12 += ((float)num4 - ((Bounds)(ref val)).size.x) * 0.5f;
		float num13 = ((float)num5 - ((Bounds)(ref val)).size.y) * 0.5f;
		foreach (CityPlot plot in plots)
		{
			Vector3 localPosition = ((Component)plot).transform.localPosition;
			localPosition.x += num12 / (float)PIXELS_PER_UNIT;
			localPosition.y += num13 / (float)PIXELS_PER_UNIT;
			((Component)plot).transform.localPosition = localPosition;
		}
		city.transform.localPosition = new Vector3(0f, 0.02f + (0.15f - (float)(num2 - 2) * 0.075f), 0f);
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		for (int j = 1; j <= Level; j++)
		{
			if (j < 6)
			{
				list2.Add(j);
			}
		}
		int num14 = Level * 4;
		num14 += num14 * num14 / 10;
		for (int k = 1; k < num14; k++)
		{
			int index = rng.Range(0, list2.Count);
			int item = list2[index];
			list.Insert((list.Count > 0) ? rng.Range(0, list.Count - 1) : 0, item);
		}
		if (HaveWorkshop)
		{
			list.Add(HOUSE_WORKSHOP);
		}
		for (int l = 0; l < ParkCount; l++)
		{
			list.Add(HOUSE_PARK);
		}
		int tribe = Math.Max(1, Tribe);
		SkinType skinType = SkinType;
		while (list.Count > 0)
		{
			CityPlot nextRandomPlot = GetNextRandomPlot(ref currentIndex, num);
			PolytopiaSpriteRenderer house = GetHouse(tribe, list[0], skinType);
			nextRandomPlot.AddHouse(house);
			list.RemoveAt(0);
		}
		foreach (byte playerEmbassy in PlayerEmbassies)
		{
			CityPlot nextRandomPlot2 = GetNextRandomPlot(ref currentIndex, num - 1);
			PolytopiaSpriteRenderer embassy = GetEmbassy(playerEmbassy);
			((Component)nextRandomPlot2).gameObject.SetActive(true);
			nextRandomPlot2.AddHouse(embassy);
		}
		if (IsCapitalOf != 0)
		{
			if (GameManager.GameState.TryGetPlayer(IsCapitalOf, out var playerState))
			{
				tribe = playerState.GetTribeStyle(GameManager.GameState);
				skinType = playerState.skinType;
			}
			PolytopiaSpriteRenderer house2 = GetHouse(tribe, HOUSE_CAPITAL, skinType);
			cityPlot2.AddHouse(house2);
			((Component)cityPlot2).gameObject.SetActive(true);
		}
		wall = GetResource(new SpriteAddress("TerrainFeatures", "CityWallGFX"));
		((Component)wall).transform.parent = city.transform;
		float num15 = 0f;
		float num16 = (float)(num5 / PIXELS_PER_UNIT) - city.transform.localPosition.y;
		((Component)wall).transform.localPosition = new Vector3(num15, num16, 0f);
		wall.SortingLayer = MeshCache.TERRAIN_LAYER_ID;
		wall.SortingOrder = sortOrder + 97;
		((Component)wall).gameObject.SetActive(HaveWall);
		city.transform.localScale = Vector3.one;
		UpdateSpriteRenderers();
		dataChanged = false;
	}

	private CityPlot GetNextRandomPlot(ref int currentIndex, int size)
	{
		currentIndex += Mathf.CeilToInt(rng.Value() * 1.5f);
		if (currentIndex > size)
		{
			currentIndex = 1;
		}
		CityPlot cityPlot = plots[currentIndex - 1];
		((Component)cityPlot).gameObject.SetActive(true);
		return cityPlot;
	}

	private PolytopiaSpriteRenderer GetEmbassy(byte embassyPlayerId)
	{
		PolytopiaSpriteRenderer spriteRenderer = GetPooledSpriteRenderer();
		SpriteAddress spriteAddress = new SpriteAddress("TerrainFeatures", "embassy");
		SpriteAddress spriteAddress2 = new SpriteAddress("TerrainFeatures", "embassy-tint");
		GameManager.GetSpriteAtlasManager().LoadSprites(new SpriteAddress[2] { spriteAddress, spriteAddress2 }, delegate(string[] atlasNames, string[] spriteNames, Sprite[] sprites)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			GameManager.GameState.TryGetPlayer(embassyPlayerId, out var playerState);
			List<MeshDescription> sprites2 = new List<MeshDescription>
			{
				new MeshDescription(sprites[0], Color.white, 0),
				new MeshDescription(sprites[1], playerState.GetPlayerColor(GameManager.GameState), 0)
			};
			spriteRenderer.Sprites = sprites2;
		});
		return spriteRenderer;
	}

	private PolytopiaSpriteRenderer GetHouse(int tribe = 1, int type = 1, SkinType skinType = SkinType.Default)
	{
		PolytopiaSpriteRenderer polytopiaSpriteRenderer = null;
		if (type == HOUSE_WORKSHOP)
		{
			return GetResource(new SpriteAddress("TerrainFeatures", "House_Workshop"));
		}
		if (type == HOUSE_PARK)
		{
			return GetResource(new SpriteAddress("TerrainFeatures", "House_Park"));
		}
		return GetResource(SpriteData.GetHouseAddresses(type, tribe.ToString(), skinType));
	}

	private PolytopiaSpriteRenderer GetPooledSpriteRenderer()
	{
		PolytopiaSpriteRenderer pooledObject = ObjectPool.GetPooledObject<PolytopiaSpriteRenderer>("PolytopiaSpriteRenderer");
		pooledObject.IsUsed = true;
		((Renderer)pooledObject.meshRenderer).enabled = true;
		((Component)pooledObject).gameObject.SetActive(true);
		pooledObject.SharedMaterial = material;
		return pooledObject;
	}

	private PolytopiaSpriteRenderer GetResource(SpriteAddress spriteAddress)
	{
		PolytopiaSpriteRenderer spriteRenderer = GetPooledSpriteRenderer();
		TerrainMaterialHelper.SetSpriteSaturated(spriteRenderer, IsEnemyCity);
		GameManager.GetSpriteAtlasManager().LoadSprite(spriteAddress, delegate(string atlasName, string spriteName, Sprite sprite)
		{
			spriteRenderer.Sprite = sprite;
		});
		return spriteRenderer;
	}

	private PolytopiaSpriteRenderer GetResource(SpriteAddress[] spriteAddresses)
	{
		PolytopiaSpriteRenderer spriteRenderer = GetPooledSpriteRenderer();
		TerrainMaterialHelper.SetSpriteSaturated(spriteRenderer, IsEnemyCity);
		GameManager.GetSpriteAtlasManager().LoadSprite(spriteAddresses, delegate(string atlasName, string spriteName, Sprite sprite)
		{
			spriteRenderer.Sprite = sprite;
		});
		return spriteRenderer;
	}

	public void Clear()
	{
		if ((Object)(object)city != (Object)null)
		{
			int count = plots.Count;
			for (int i = 0; i < count; i++)
			{
				CityPlot cityPlot = plots[i];
				cityPlot.Clear();
				Object.Destroy((Object)(object)cityPlot);
			}
			plots.Clear();
			wall.ReturnToPool();
			Object.Destroy((Object)(object)city);
		}
	}

	private void UpdateSpriteRenderers()
	{
		spriteRenderers.Clear();
		for (int i = 0; i < plots.Count; i++)
		{
			_ = plots[i];
			spriteRenderers.AddRange(plots[i].houses);
		}
		if (HaveWall)
		{
			spriteRenderers.Add(wall);
		}
	}

	public List<PolytopiaSpriteRenderer> GetSpriteRenderers()
	{
		return spriteRenderers;
	}
}
