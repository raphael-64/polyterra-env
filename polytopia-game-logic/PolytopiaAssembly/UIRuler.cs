using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class UIRuler : MonoBehaviour
{
	[Serializable]
	public struct HorizontalLineData
	{
		public enum horizontalLinePivot
		{
			Top,
			Center,
			Bottom
		}

		public horizontalLinePivot pivot;

		public int position;

		public Color color;

		public string info;
	}

	[Serializable]
	public struct VerticalLineData
	{
		public enum verticalLinePivot
		{
			Left,
			Center,
			Right
		}

		public verticalLinePivot pivot;

		public int position;

		public Color color;

		public string info;
	}

	public bool globalSpace;

	public bool drawRulers = true;

	public bool drawCenter;

	public bool drawInfo;

	public List<HorizontalLineData> horizontalLines;

	public List<VerticalLineData> verticalLines;

	public bool horizontalListExpanded = true;

	public bool verticalListExpanded = true;

	public RectOffset padding;
}
