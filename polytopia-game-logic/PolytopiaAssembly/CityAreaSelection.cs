using System.Collections.Generic;
using UnityEngine;

public class CityAreaSelection : MonoBehaviour
{
	protected class LineData
	{
		public Vector3 from;

		public Vector3 to;

		public LineData()
		{
		}

		public LineData(Vector3 center, Vector3 fromOffset, Vector3 toOffset)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			from = center + fromOffset;
			to = center + toOffset;
		}
	}

	[SerializeField]
	protected LineRenderer linePrefab;

	protected List<LineRenderer> currentLines = new List<LineRenderer>();

	protected Queue<LineRenderer> cachedLines = new Queue<LineRenderer>();

	protected List<LineData> edgesData = new List<LineData>();

	protected Vector2 tileSize = new Vector2(0.966f, 0.585f);

	protected bool isownCity;

	public void Show(City city, int borderSize = 2)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		if (city.Owner == null)
		{
			return;
		}
		Vector2 val = tileSize * 0.5f;
		byte id = city.Owner.Id;
		isownCity = id == GameManager.LocalPlayer.Id;
		TileData[] areaSorted = GameManager.GameState.Map.GetAreaSorted(city.Tile.Coordinates, borderSize, allowDiagonal: true);
		int num = areaSorted.Length;
		edgesData.Clear();
		for (int i = 0; i < num; i++)
		{
			TileData tileData = areaSorted[i];
			Tile tileInstance = MapRenderer.Current.GetTileInstance(tileData.coordinates);
			if (tileInstance.Owner != null && tileInstance.Owner.Id == id && tileData.rulingCityCoordinates == city.Tile.Coordinates)
			{
				if (IsExposedEdge(city, tileInstance, GridDirection.N))
				{
					edgesData.Add(new LineData(tileInstance.VisualCenter, new Vector3(0f - val.x, 0f, 0f), new Vector3(0f, val.y, 0f)));
				}
				if (IsExposedEdge(city, tileInstance, GridDirection.E))
				{
					edgesData.Add(new LineData(tileInstance.VisualCenter, new Vector3(0f, val.y, 0f), new Vector3(val.x, 0f, 0f)));
				}
				if (IsExposedEdge(city, tileInstance, GridDirection.S))
				{
					edgesData.Add(new LineData(tileInstance.VisualCenter, new Vector3(val.x, 0f, 0f), new Vector3(0f, 0f - val.y, 0f)));
				}
				if (IsExposedEdge(city, tileInstance, GridDirection.W))
				{
					edgesData.Add(new LineData(tileInstance.VisualCenter, new Vector3(0f, 0f - val.y, 0f), new Vector3(0f - val.x, 0f, 0f)));
				}
			}
		}
		if (edgesData.Count > 0)
		{
			int count = edgesData.Count;
			for (int j = 0; j < count; j++)
			{
				AddLine(edgesData[j].from, edgesData[j].to);
			}
		}
		((Component)this).gameObject.SetActive(true);
	}

	private void AddLine(Vector3 from, Vector3 to)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		LineRenderer orCreateLine = GetOrCreateLine();
		((Component)orCreateLine).gameObject.SetActive(true);
		orCreateLine.useWorldSpace = true;
		orCreateLine.SetPositions((Vector3[])(object)new Vector3[2] { from, to });
		Color startColor = (orCreateLine.endColor = (isownCity ? ColorConstants.blue : ColorConstants.red));
		orCreateLine.startColor = startColor;
		currentLines.Add(orCreateLine);
	}

	private LineRenderer GetOrCreateLine()
	{
		if (cachedLines.Count > 0)
		{
			return cachedLines.Dequeue();
		}
		return Object.Instantiate<LineRenderer>(linePrefab, ((Component)this).transform);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
		foreach (LineRenderer currentLine in currentLines)
		{
			cachedLines.Enqueue(currentLine);
			((Component)currentLine).gameObject.SetActive(false);
		}
		currentLines.Clear();
	}

	private bool IsExposedEdge(City city, Tile tile, GridDirection direction)
	{
		byte id = city.Owner.Id;
		Tile neighbor = tile.GetNeighbor(direction);
		if ((Object)(object)neighbor == (Object)null || neighbor.Owner == null || neighbor.Owner.Id != id || neighbor.Data.rulingCityCoordinates != city.Tile.Coordinates)
		{
			return true;
		}
		return false;
	}
}
