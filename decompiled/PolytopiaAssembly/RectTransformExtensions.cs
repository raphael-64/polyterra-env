using UnityEngine;

public static class RectTransformExtensions
{
	private static Vector3[] CORNERS = (Vector3[])(object)new Vector3[4];

	public static bool Overlaps(this RectTransform a, RectTransform b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Rect val = a.WorldRect();
		return ((Rect)(ref val)).Overlaps(b.WorldRect());
	}

	public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Rect val = a.WorldRect();
		return ((Rect)(ref val)).Overlaps(b.WorldRect(), allowInverse);
	}

	public static Rect WorldRect(this RectTransform rectTransform)
	{
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.GetWorldCorners(CORNERS);
		float num = Mathf.Min(Mathf.Min(CORNERS[0].x, CORNERS[1].x), Mathf.Min(CORNERS[2].x, CORNERS[3].x));
		float num2 = Mathf.Max(Mathf.Max(CORNERS[0].x, CORNERS[1].x), Mathf.Max(CORNERS[2].x, CORNERS[3].x));
		float num3 = Mathf.Min(Mathf.Min(CORNERS[0].y, CORNERS[1].y), Mathf.Min(CORNERS[2].y, CORNERS[3].y));
		float num4 = Mathf.Max(Mathf.Max(CORNERS[0].y, CORNERS[1].y), Mathf.Max(CORNERS[2].y, CORNERS[3].y));
		return Rect.MinMaxRect(num, num3, num2, num4);
	}

	public static Rect ScreenRect(this RectTransform rectTransform, Canvas canvas)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if ((int)canvas.renderMode == 0)
		{
			return rectTransform.WorldRect();
		}
		Camera worldCamera = canvas.worldCamera;
		rectTransform.GetWorldCorners(CORNERS);
		CORNERS[0] = worldCamera.WorldToScreenPoint(CORNERS[0]);
		CORNERS[1] = worldCamera.WorldToScreenPoint(CORNERS[1]);
		CORNERS[2] = worldCamera.WorldToScreenPoint(CORNERS[2]);
		CORNERS[3] = worldCamera.WorldToScreenPoint(CORNERS[3]);
		float num = Mathf.Min(Mathf.Min(CORNERS[0].x, CORNERS[1].x), Mathf.Min(CORNERS[2].x, CORNERS[3].x));
		float num2 = Mathf.Max(Mathf.Max(CORNERS[0].x, CORNERS[1].x), Mathf.Max(CORNERS[2].x, CORNERS[3].x));
		float num3 = Mathf.Min(Mathf.Min(CORNERS[0].y, CORNERS[1].y), Mathf.Min(CORNERS[2].y, CORNERS[3].y));
		float num4 = Mathf.Max(Mathf.Max(CORNERS[0].y, CORNERS[1].y), Mathf.Max(CORNERS[2].y, CORNERS[3].y));
		return Rect.MinMaxRect(num, num3, num2, num4);
	}
}
