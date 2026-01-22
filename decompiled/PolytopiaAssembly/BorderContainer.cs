using System.Collections.Generic;
using UnityEngine;

public class BorderContainer : MonoBehaviour
{
	[SerializeField]
	private Tile tile;

	[SerializeField]
	private PolytopiaSpriteRenderer northBorder;

	[SerializeField]
	private PolytopiaSpriteRenderer eastBorder;

	[SerializeField]
	private PolytopiaSpriteRenderer southBorder;

	[SerializeField]
	private PolytopiaSpriteRenderer westBorder;

	private bool visible;

	private int depth;

	private List<PolytopiaSpriteRenderer> backSpriteRenderers;

	private List<PolytopiaSpriteRenderer> frontSpriteRenderers;

	public int Depth
	{
		get
		{
			return depth;
		}
		set
		{
			depth = value;
			northBorder.SortingOrder = depth;
			eastBorder.SortingOrder = depth;
			southBorder.SortingOrder = depth + 99;
			westBorder.SortingOrder = depth + 99;
		}
	}

	public void Render()
	{
		((Component)northBorder).gameObject.SetActive(ShowBorder(GridDirection.N));
		((Component)eastBorder).gameObject.SetActive(ShowBorder(GridDirection.E));
		((Component)southBorder).gameObject.SetActive(ShowBorder(GridDirection.S));
		((Component)westBorder).gameObject.SetActive(ShowBorder(GridDirection.W));
	}

	public List<PolytopiaSpriteRenderer> GetBackSpriteRenderers()
	{
		if (backSpriteRenderers == null)
		{
			backSpriteRenderers = new List<PolytopiaSpriteRenderer>();
			backSpriteRenderers.Add(northBorder);
			backSpriteRenderers.Add(eastBorder);
		}
		return backSpriteRenderers;
	}

	public List<PolytopiaSpriteRenderer> GetFrontSpriteRenderers()
	{
		if (frontSpriteRenderers == null)
		{
			frontSpriteRenderers = new List<PolytopiaSpriteRenderer>();
			frontSpriteRenderers.Add(southBorder);
			frontSpriteRenderers.Add(westBorder);
		}
		return frontSpriteRenderers;
	}

	public void SetColor(Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Color val = color * Color32.op_Implicit(new Color32((byte)128, (byte)128, (byte)128, byte.MaxValue));
		PolytopiaSpriteRenderer polytopiaSpriteRenderer = northBorder;
		Color color2 = (southBorder.Color = color);
		polytopiaSpriteRenderer.Color = color2;
		PolytopiaSpriteRenderer polytopiaSpriteRenderer2 = eastBorder;
		color2 = (westBorder.Color = val);
		polytopiaSpriteRenderer2.Color = color2;
	}

	public void SetVisible(bool value)
	{
		visible = value;
	}

	private bool ShowBorder(GridDirection direction)
	{
		if (!Object.op_Implicit((Object)(object)tile) || !visible || tile.Data == null || tile.Data.owner == 0)
		{
			return false;
		}
		TileData tileData = GameManager.GameState.Map.GetTile(tile.Data.coordinates + direction.ToCoordinates());
		if (tileData == null || (tileData != null && tileData.owner != tile.Data.owner))
		{
			return true;
		}
		return false;
	}
}
