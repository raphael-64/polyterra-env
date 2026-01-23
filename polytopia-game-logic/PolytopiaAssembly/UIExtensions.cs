using UnityEngine;

public static class UIExtensions
{
	public static void SetWidth(this RectTransform rectTransform, float width)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector2 sizeDelta = rectTransform.sizeDelta;
		sizeDelta.x = width;
		rectTransform.sizeDelta = sizeDelta;
	}

	public static void SetHeight(this RectTransform rectTransform, float height)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector2 sizeDelta = rectTransform.sizeDelta;
		sizeDelta.y = height;
		rectTransform.sizeDelta = sizeDelta;
	}

	public static float GetWidth(this RectTransform rectTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return rectTransform.sizeDelta.x;
	}

	public static float GetHeight(this RectTransform rectTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return rectTransform.sizeDelta.y;
	}

	public static void SetAnchoredY(this RectTransform rectTransform, float y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.y = y;
		rectTransform.anchoredPosition = anchoredPosition;
	}

	public static void SetAnchoredX(this RectTransform rectTransform, float x)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.x = x;
		rectTransform.anchoredPosition = anchoredPosition;
	}

	public static Rect GetWorldRect(this RectTransform rectTransform, float scale = 1f)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Vector2 val = Vector2.op_Implicit(array[0]);
		Rect rect = rectTransform.rect;
		float num = scale * ((Rect)(ref rect)).size.x;
		rect = rectTransform.rect;
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(num, scale * ((Rect)(ref rect)).size.y);
		return new Rect(val, val2);
	}
}
