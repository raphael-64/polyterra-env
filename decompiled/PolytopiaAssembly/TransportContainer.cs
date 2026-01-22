using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class TransportContainer : MonoBehaviour, ISpriteRendererProvider
{
	public enum TransportType
	{
		Road,
		Route
	}

	[SerializeField]
	private Tile tile;

	[SerializeField]
	private Material material;

	private PolytopiaSpriteRenderer[] lines = new PolytopiaSpriteRenderer[8];

	private List<PolytopiaSpriteRenderer> spriteRenderers = new List<PolytopiaSpriteRenderer>(8);

	private bool visible;

	private int depth;

	public int Depth
	{
		get
		{
			return depth;
		}
		set
		{
			depth = value;
			PolytopiaSpriteRenderer[] array = lines;
			foreach (PolytopiaSpriteRenderer polytopiaSpriteRenderer in array)
			{
				if ((Object)(object)polytopiaSpriteRenderer != (Object)null)
				{
					polytopiaSpriteRenderer.SortingOrder = depth + 2;
				}
			}
		}
	}

	public void Render()
	{
		bool flag = false;
		for (int i = 0; i < lines.Length; i++)
		{
			GridDirection direction = (GridDirection)i;
			TransportType type;
			bool flag2 = ShouldShow(direction, out type);
			flag = flag || flag2;
			UpdateRoadInDirection(direction, type, flag2);
		}
		if (!flag && tile.Data.HasRoad && !tile.Data.HasImprovement(ImprovementData.Type.City) && !tile.Data.IsRouteOpener(GameManager.GameState))
		{
			UpdateRoadInDirection(GridDirection.S, TransportType.Road, visible);
		}
	}

	public List<PolytopiaSpriteRenderer> GetSpriteRenderers()
	{
		return spriteRenderers;
	}

	private PolytopiaSpriteRenderer CreateRoad(GridDirection direction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject();
		val.transform.parent = ((Component)this).transform;
		val.transform.localPosition = Vector3.zero;
		val.transform.localScale = Vector3.one;
		val.transform.localRotation = Quaternion.identity;
		PolytopiaSpriteRenderer polytopiaSpriteRenderer = val.AddComponent<PolytopiaSpriteRenderer>();
		polytopiaSpriteRenderer.SharedMaterial = material;
		polytopiaSpriteRenderer.SortingLayer = MeshCache.TERRAIN_LAYER_ID;
		polytopiaSpriteRenderer.SortingOrder = depth + 2;
		return polytopiaSpriteRenderer;
	}

	private void UpdateRoadInDirection(GridDirection direction, TransportType type, bool shouldShow)
	{
		PolytopiaSpriteRenderer line = lines[(int)direction];
		if ((Object)(object)line == (Object)null && !shouldShow)
		{
			return;
		}
		if ((Object)(object)line == (Object)null)
		{
			line = CreateRoad(direction);
			lines[(int)direction] = line;
			spriteRenderers.Add(line);
		}
		((Component)line).gameObject.SetActive(shouldShow);
		if (!shouldShow)
		{
			return;
		}
		string obj = ((type == TransportType.Road) ? "roads000" : "routes000");
		int num = (int)direction;
		string text = obj + num;
		if (!((Object)(object)line.Sprite == (Object)null) && !(((Object)line.Sprite).name != text))
		{
			return;
		}
		SpriteAddress spriteAddress = new SpriteAddress("TerrainFeatures", text);
		GameManager.GetSpriteAtlasManager().LoadSprite(spriteAddress, delegate(string atlasName, string loadedSpriteName, Sprite sprite)
		{
			if ((Object)(object)line != (Object)null && (Object)(object)line.Sprite != (Object)(object)sprite)
			{
				line.Sprite = sprite;
			}
		});
	}

	public void SetVisible(bool value)
	{
		visible = value;
		PolytopiaSpriteRenderer[] array = lines;
		foreach (PolytopiaSpriteRenderer polytopiaSpriteRenderer in array)
		{
			if (!((Object)(object)polytopiaSpriteRenderer == (Object)null))
			{
				((Component)polytopiaSpriteRenderer).gameObject.SetActive(visible);
			}
		}
	}

	private bool ShouldShow(GridDirection direction, out TransportType type)
	{
		if (tile.Data.HasRoad)
		{
			type = TransportType.Road;
		}
		else
		{
			type = TransportType.Route;
		}
		if (!Object.op_Implicit((Object)(object)tile) || !visible || tile.Data == null || (!tile.Data.hasRoute && !tile.Data.IsRouteOpener(GameManager.GameState) && !tile.Data.HasRoad))
		{
			return false;
		}
		TileData tileData = GameManager.GameState.Map.GetTile(tile.Data.coordinates + direction.ToCoordinates());
		if (tileData == null)
		{
			return false;
		}
		if (tile.Data.HasMatchingTransportPath(tileData, GameManager.GameState))
		{
			return true;
		}
		return false;
	}
}
