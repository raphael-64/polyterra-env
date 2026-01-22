using System;
using Polytopia.Data;
using UnityEngine;

public class TerrainRenderer : MonoBehaviour
{
	[Serializable]
	public class WaterSpriteData
	{
		public Sprite defaultSprite;

		public Sprite cornerSprite;

		public Sprite leftSprite;

		public Sprite rightSprite;

		public Sprite GetSprite(Tile tile)
		{
			bool flag = Object.op_Implicit((Object)(object)tile.GetNeighbor(GridDirection.W));
			bool flag2 = Object.op_Implicit((Object)(object)tile.GetNeighbor(GridDirection.S));
			if (!flag && !flag2 && (Object)(object)cornerSprite != (Object)null)
			{
				return cornerSprite;
			}
			if (!flag && (Object)(object)leftSprite != (Object)null)
			{
				return leftSprite;
			}
			if (!flag2 && (Object)(object)rightSprite != (Object)null)
			{
				return rightSprite;
			}
			return defaultSprite;
		}
	}

	[SerializeField]
	private PolytopiaSpriteRenderer spriteRenderer;

	public WaterSpriteData waterSprites;

	public WaterSpriteData oceanSprites;

	public WaterSpriteData iceSprites;

	private bool isDesaturated;

	public PolytopiaSpriteRenderer SpriteRenderer => spriteRenderer;

	public void UpdateGraphics(Tile tile)
	{
		switch (tile.Data.terrain)
		{
		case TerrainData.Type.Water:
			spriteRenderer.Sprite = waterSprites.GetSprite(tile);
			break;
		case TerrainData.Type.Ocean:
			spriteRenderer.Sprite = oceanSprites.GetSprite(tile);
			break;
		case TerrainData.Type.Ice:
			spriteRenderer.Sprite = iceSprites.GetSprite(tile);
			break;
		default:
		{
			SpriteHandle spriteHandle = new SpriteHandle();
			spriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle2)
			{
				spriteRenderer.Sprite = spriteHandle2.sprite;
			});
			spriteHandle.Request(new SpriteAddress[2]
			{
				SpriteData.GetTileSpriteAddress(TerrainData.Type.Field, tile.SkinType.GetName()),
				SpriteData.GetTileSpriteAddress(TerrainData.Type.Field, tile.Climate.ToString())
			});
			break;
		}
		}
		bool flag = ShouldChangeSaturation(tile.Data.terrain) && tile.Owner != null && !GameManager.IsPlayerViewing(tile.Owner.Id);
		if (flag != isDesaturated)
		{
			TerrainMaterialHelper.SetSpriteSaturated(SpriteRenderer, flag);
			isDesaturated = flag;
		}
	}

	private bool ShouldChangeSaturation(TerrainData.Type terrainType)
	{
		switch (terrainType)
		{
		case TerrainData.Type.Water:
		case TerrainData.Type.Ocean:
			return false;
		default:
			return true;
		}
	}
}
