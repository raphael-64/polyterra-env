using System.Collections.Generic;
using UnityEngine;

public class CityStatusProgressBar : MonoBehaviour
{
	[Range(2f, 10f)]
	[SerializeField]
	protected int totalFields = 2;

	[Range(0f, 10f)]
	[SerializeField]
	protected int filledFields;

	[Range(0f, 10f)]
	[SerializeField]
	protected int dots;

	[SerializeField]
	protected float defaultScale = 2.25f;

	[SerializeField]
	protected float maxWidth = 1f;

	[SerializeField]
	protected Color baseColor;

	[SerializeField]
	protected Color fillColor;

	[SerializeField]
	protected Color negativeColor;

	[Header("Graphics")]
	[SerializeField]
	protected CityStatusProgressSegment segmentPrefab;

	protected List<CityStatusProgressSegment> segments = new List<CityStatusProgressSegment>();

	protected float minWidth = 0.59f;

	protected float centerFieldWidth = 0.27f;

	protected float dotsWidth = 0.27f;

	protected float spacing = 0.02f;

	protected Color lastfillColor;

	protected int lastFilledFields;

	protected int lastDots;

	public int TotalFields
	{
		get
		{
			return totalFields;
		}
		set
		{
			totalFields = value;
			UpdateFields();
		}
	}

	public int FilledFields
	{
		get
		{
			return filledFields;
		}
		set
		{
			filledFields = value;
			UpdateFields();
		}
	}

	public int Dots
	{
		get
		{
			return dots;
		}
		set
		{
			dots = value;
			UpdateFields();
		}
	}

	private void Start()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		lastfillColor = fillColor;
		TotalFields = totalFields;
	}

	public void UpdateFields()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Min(minWidth + (float)(totalFields - 2) * centerFieldWidth, maxWidth);
		for (int i = 0; i < totalFields; i++)
		{
			CityStatusProgressSegment cityStatusProgressSegment;
			if (i < segments.Count)
			{
				cityStatusProgressSegment = segments[i];
			}
			else
			{
				cityStatusProgressSegment = Object.Instantiate<CityStatusProgressSegment>(segmentPrefab, ((Component)this).transform, false);
				segments.Add(cityStatusProgressSegment);
			}
			if (i == 0)
			{
				cityStatusProgressSegment.SetType(CityStatusProgressSegment.Type.Left);
			}
			else if (i == totalFields - 1)
			{
				cityStatusProgressSegment.SetType(CityStatusProgressSegment.Type.Right);
			}
			else
			{
				cityStatusProgressSegment.SetType(CityStatusProgressSegment.Type.Middle);
			}
			float num2 = num / (float)totalFields;
			Color val = ((filledFields < 0) ? negativeColor : fillColor);
			int num3 = Mathf.Abs(filledFields);
			cityStatusProgressSegment.SetWidth(num2);
			cityStatusProgressSegment.SetPosition((float)i * num2 - num * 0.5f);
			cityStatusProgressSegment.SetColor((i < num3) ? val : baseColor);
			cityStatusProgressSegment.SetDot(i < dots, (i < num3) ? Color.white : Color.black);
			cityStatusProgressSegment.Render();
		}
		for (int j = totalFields; j < segments.Count; j++)
		{
			Object.Destroy((Object)(object)((Component)segments[j]).gameObject);
			segments.RemoveAt(j--);
		}
	}
}
