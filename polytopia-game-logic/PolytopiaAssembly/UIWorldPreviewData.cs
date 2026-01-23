using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Polytopia.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "UIWorldPreviewData", menuName = "Data/UIWorldPreviewData", order = 0)]
public class UIWorldPreviewData : ScriptableObject
{
	[SerializeField]
	public List<UICityPosition> CityPositions = new List<UICityPosition>();

	[Space(10f)]
	[SerializeField]
	private Vector2Int offset;

	[field: SerializeField]
	public UITileData DefaultData { get; private set; }

	[field: SerializeField]
	public List<UITileData> DefaultDataList { get; private set; } = new List<UITileData>();

	[field: SerializeField]
	public List<PreviewData> PreviewOverrides { get; private set; } = new List<PreviewData>();

	public bool TryGetData(Vector2Int position, TribeData.Type tribeType, out UITileData uiTile)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		uiTile = PreviewOverrides.FirstOrDefault((PreviewData x) => x.tribeType == tribeType)?.TileDataList.FirstOrDefault((UITileData x) => x.Position == position);
		if (uiTile == null)
		{
			uiTile = DefaultDataList.FirstOrDefault((UITileData x) => x.Position == position);
		}
		return uiTile != null;
	}

	[Button(null)]
	public void Sort()
	{
		DefaultDataList = (from x in DefaultDataList
			orderby ((Vector2Int)(ref x.Position)).x, ((Vector2Int)(ref x.Position)).y
			select x).ToList();
		foreach (PreviewData previewOverride in PreviewOverrides)
		{
			previewOverride.TileDataList = (from x in previewOverride.TileDataList
				orderby ((Vector2Int)(ref x.Position)).x, ((Vector2Int)(ref x.Position)).y
				select x).ToList();
		}
	}

	[Button(null)]
	public void ApplyOffset()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		foreach (UITileData defaultData in DefaultDataList)
		{
			Vector2Int val = offset;
			if (((Vector2Int)(ref defaultData.Position)).y % 2 == 1)
			{
				((Vector2Int)(ref val)).x = ((Vector2Int)(ref val)).x - 1;
			}
			defaultData.Position += val;
		}
		foreach (PreviewData previewOverride in PreviewOverrides)
		{
			foreach (UITileData tileData in previewOverride.TileDataList)
			{
				Vector2Int val2 = offset;
				if (((Vector2Int)(ref tileData.Position)).y % 2 == 1)
				{
					((Vector2Int)(ref val2)).x = ((Vector2Int)(ref val2)).x - 1;
				}
				tileData.Position += val2;
			}
		}
	}
}
