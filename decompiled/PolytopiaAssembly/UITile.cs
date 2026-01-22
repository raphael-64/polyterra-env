using Polytopia.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITile : MonoBehaviour
{
	public RectTransform RectTransform
	{
		get
		{
			Transform transform = ((Component)this).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	public TerrainData.Type TerrainType { get; private set; }

	[field: SerializeField]
	public Vector2Int Position { get; set; }

	[field: SerializeField]
	public Image Tile { get; private set; }

	[field: SerializeField]
	public Image Mountain { get; private set; }

	[field: SerializeField]
	public Image Forest { get; private set; }

	[field: SerializeField]
	public Image Resource { get; private set; }

	[field: SerializeField]
	public Image Animal { get; private set; }

	[field: SerializeField]
	public Image Improvement { get; private set; }

	[field: SerializeField]
	public RectTransform UnitParent { get; private set; }

	[field: SerializeField]
	public TextMeshProUGUI DebugText { get; private set; }

	[field: SerializeField]
	public UICityRenderer UICityRenderer { get; private set; }

	private UIUnitRenderer UIUnitRenderer { get; set; }

	public unsafe void SetPosition(Vector2Int position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Position = position;
		((TMP_Text)DebugText).text = ((object)(*(Vector2Int*)(&position))/*cast due to .constrained prefix*/).ToString();
	}

	public void HideAll()
	{
		((Component)Mountain).gameObject.SetActive(false);
		((Component)Forest).gameObject.SetActive(false);
		((Component)Resource).gameObject.SetActive(false);
		((Component)Animal).gameObject.SetActive(false);
		((Component)UICityRenderer).gameObject.SetActive(false);
		((Component)Improvement).gameObject.SetActive(false);
		DestroyUnitRenderer();
	}

	public void SetData(UITileData tileData, TribeData tribeData, SkinType skinType, TribeData mixTribeData)
	{
		SpriteAtlasManager atlas = GameManager.GetSpriteAtlasManager();
		int climate = tribeData.climate;
		TribeData visibleTribeData = mixTribeData ?? tribeData;
		LoadTerrain();
		LoadResources();
		LoadUnits();
		LoadImprovements();
		void LoadImprovements()
		{
			if (tileData.improvementType != ImprovementData.Type.None)
			{
				SpriteAddress[] buildingSpriteAddresses = SpriteData.GetBuildingSpriteAddresses(tileData.improvementType, skinType, climate);
				atlas.LoadSprite(buildingSpriteAddresses, delegate(Sprite sprite)
				{
					((Component)Improvement).gameObject.SetActive(true);
					Improvement.sprite = sprite;
					((Graphic)Improvement).SetNativeSize();
				});
			}
		}
		void LoadResources()
		{
			SpriteAddress[] array = new SpriteAddress[0];
			Image image = ((tileData.resourceType == ResourceData.Type.Game) ? Animal : Resource);
			ResourceData.Type resourceType = tileData.resourceType;
			if (resourceType == ResourceData.Type.Game)
			{
				array = SpriteData.GetAddresses(PickerType.Animal, climate, skinType);
			}
			else if (resourceType == ResourceData.Type.Fruit)
			{
				array = SpriteData.GetAddresses(PickerType.Fruit, climate, skinType);
			}
			else if (resourceType != ResourceData.Type.None)
			{
				array = new SpriteAddress[1] { SpriteData.GetResourceSpriteAddress(tileData.resourceType, climate) };
				if (resourceType == ResourceData.Type.Spores)
				{
					Debug.LogWarning((object)$"Load: {array[0]}");
				}
			}
			atlas.LoadSprite(array, delegate(Sprite sprite)
			{
				if (resourceType == ResourceData.Type.Spores)
				{
					Debug.LogWarning((object)$"Loaded: {resourceType}");
				}
				((Component)image).gameObject.SetActive(true);
				image.sprite = sprite;
				((Graphic)image).SetNativeSize();
			});
		}
		void LoadTerrain()
		{
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			TerrainType = tileData.terrainType;
			TerrainData.Type terrain = ((TerrainType == TerrainData.Type.Water || TerrainType == TerrainData.Type.Ocean || TerrainType == TerrainData.Type.Ice) ? TerrainType : TerrainData.Type.Field);
			SpriteAddress[] spriteAddresses = new SpriteAddress[2]
			{
				SpriteData.GetTileSpriteAddress(terrain, skinType.GetName()),
				SpriteData.GetTileSpriteAddress(terrain, climate)
			};
			atlas.LoadSprite(spriteAddresses, delegate(string atlasName, string spriteName, Sprite sprite)
			{
				Tile.sprite = sprite;
				((Graphic)Tile).SetNativeSize();
			});
			if (TerrainType == TerrainData.Type.Water || TerrainType == TerrainData.Type.Ocean)
			{
				RectTransform.SetAnchoredY(RectTransform.anchoredPosition.y - 3f);
			}
			((Component)Mountain).gameObject.SetActive(TerrainType == TerrainData.Type.Mountain);
			((Component)Forest).gameObject.SetActive(TerrainType == TerrainData.Type.Forest);
		}
		void LoadUnits()
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			DestroyUnitRenderer();
			if (tileData.unitType != UnitData.Type.None)
			{
				UIUnitRenderer = UIUtils.GetUIUnitRenderer(tileData.unitType, visibleTribeData, skinType);
				((Transform)UIUnitRenderer.rectTransform).SetParent((Transform)(object)UnitParent, false);
				UIUnitRenderer.rectTransform.anchoredPosition = Vector2.zero;
			}
		}
	}

	private void DestroyUnitRenderer()
	{
		if ((Object)(object)UIUnitRenderer != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)UIUnitRenderer).gameObject);
			UIUnitRenderer = null;
		}
	}
}
