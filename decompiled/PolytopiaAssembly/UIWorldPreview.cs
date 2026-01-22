using System.Collections;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.UI;

public class UIWorldPreview : MonoBehaviour
{
	[SerializeField]
	private RectTransform tilesParent;

	[SerializeField]
	private UIWorldPreviewData worldPreviewData;

	[Header("Prefabs")]
	[SerializeField]
	private UITile tilePrefab;

	private TribeData tribeData;

	private TribeData mixTribeData;

	private SkinType skinType;

	private Vector2 size = Vector2.zero;

	private List<UITile> tiles = new List<UITile>();

	public RectTransform RectTransform
	{
		get
		{
			Transform transform = ((Component)this).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	private void OnRectTransformDimensionsChange()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			((MonoBehaviour)this).StartCoroutine(WaitForDimensionsChange());
		}
		IEnumerator WaitForDimensionsChange()
		{
			yield return null;
			Vector2 val = size;
			Rect rect = RectTransform.rect;
			if (val != ((Rect)(ref rect)).size && tribeData != null)
			{
				rect = RectTransform.rect;
				size = ((Rect)(ref rect)).size;
				UpdateTiles();
				SetPreview();
			}
		}
	}

	public void SetPreview(TribeData tribeData, SkinType skinType, TribeData mixTribeData)
	{
		this.tribeData = tribeData;
		this.skinType = skinType;
		this.mixTribeData = mixTribeData;
		UpdateTiles();
		SetPreview();
	}

	private void SetPreview()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		if (size.x == 0f || size.y == 0f || tribeData == null)
		{
			return;
		}
		SpriteAtlasManager spriteAtlasManager = GameManager.GetSpriteAtlasManager();
		int tribe = ((mixTribeData != null) ? mixTribeData.style : tribeData.style);
		skinType.GetName();
		int climate = tribeData.climate;
		foreach (UICityPosition cityPosition in worldPreviewData.CityPositions)
		{
			if (TryGetTile(cityPosition.position, out var uiTile))
			{
				int num = cityPosition.level;
				if (tribeData.type == TribeData.Type.Luxidoor)
				{
					num += 2;
				}
				((Component)uiTile.UICityRenderer).gameObject.SetActive(true);
				uiTile.UICityRenderer.CreateCity(tribe, num, isCapital: false, skinType);
			}
		}
		foreach (UITile tile in tiles)
		{
			if (TryGetData(tile.Position, out var uiTile2))
			{
				tile.SetData(uiTile2, tribeData, skinType, mixTribeData);
			}
			else
			{
				tile.SetData(worldPreviewData.DefaultData, tribeData, skinType, mixTribeData);
			}
		}
		SpriteAddress[] spriteAddresses = new SpriteAddress[2]
		{
			SpriteData.GetTileSpriteAddress(TerrainData.Type.Mountain, skinType.GetName()),
			SpriteData.GetTileSpriteAddress(TerrainData.Type.Mountain, climate)
		};
		spriteAtlasManager.LoadSprite(spriteAddresses, delegate(string atlasName, string spriteName, Sprite sprite)
		{
			foreach (UITile tile2 in tiles)
			{
				tile2.Mountain.sprite = sprite;
				((Graphic)tile2.Mountain).SetNativeSize();
			}
		});
		SpriteAddress[] spriteAddresses2 = new SpriteAddress[2]
		{
			SpriteData.GetTileSpriteAddress(TerrainData.Type.Forest, skinType.GetName()),
			SpriteData.GetTileSpriteAddress(TerrainData.Type.Forest, climate)
		};
		spriteAtlasManager.LoadSprite(spriteAddresses2, delegate(string atlasName, string spriteName, Sprite sprite)
		{
			foreach (UITile tile3 in tiles)
			{
				tile3.Forest.sprite = sprite;
				((Graphic)tile3.Forest).SetNativeSize();
			}
		});
	}

	private bool TryGetData(Vector2Int position, out UITileData uiTile)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		worldPreviewData.TryGetData(position, tribeData.type, out uiTile);
		return uiTile != null;
	}

	private bool TryGetTile(Vector2Int position, out UITile uiTile)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return Object.op_Implicit((Object)(object)(uiTile = tiles.Find((UITile x) => x.Position == position)));
	}

	private void UpdateTiles()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		Vector2 sizeDelta = tilePrefab.RectTransform.sizeDelta;
		int num = 1 + 2 * (int)Mathf.Ceil(size.x * 0.5f / sizeDelta.x);
		int num2 = 1 + 2 * (int)Mathf.Ceil(size.y * 0.667f / (sizeDelta.y * 0.3f));
		float num3 = (float)(-((num - 1) / 2)) * sizeDelta.x;
		float num4 = 1.8f * sizeDelta.y;
		int num5 = -(num - 1) / 2;
		int num6 = (num2 - 1) / 2;
		int num7 = 0;
		for (int i = 0; i < num2; i++)
		{
			if (i % 2 == 0)
			{
				num3 -= sizeDelta.x * 0.5f;
			}
			for (int j = 0; j < num; j++)
			{
				UITile tile = GetTile(num7);
				tile.RectTransform.anchoredPosition = new Vector2(num3, num4);
				((Transform)tile.RectTransform).SetAsLastSibling();
				((Object)tile).name = $"UITile_{num5}_{num6}";
				tile.SetPosition(new Vector2Int(num5, num6));
				num3 += sizeDelta.x;
				num5++;
				num7++;
			}
			num4 -= sizeDelta.y * 0.315f;
			num3 = (float)(-((num - 1) / 2)) * sizeDelta.x;
			num6--;
			num5 = -(num - 1) / 2;
		}
		for (int k = num7; k < tiles.Count; k++)
		{
			((Object)tiles[k]).name = "UITile unused";
			((Component)tiles[k]).gameObject.SetActive(false);
		}
		foreach (UITile tile2 in tiles)
		{
			tile2.HideAll();
		}
	}

	private UITile GetTile(int index)
	{
		if (tiles.Count > index)
		{
			((Component)tiles[index]).gameObject.SetActive(true);
			return tiles[index];
		}
		UITile uITile = Object.Instantiate<UITile>(tilePrefab, (Transform)(object)tilesParent);
		tiles.Add(uITile);
		return uITile;
	}
}
