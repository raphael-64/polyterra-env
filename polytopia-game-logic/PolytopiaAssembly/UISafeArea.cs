using UnityEngine;

public class UISafeArea : UIBasicComponent
{
	[SerializeField]
	protected bool ConformX = true;

	[SerializeField]
	protected bool ConformY = true;

	private RectTransform Panel;

	private Rect LastSafeArea = new Rect(0f, 0f, 0f, 0f);

	private void OnEnable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		SystemEvents.OnSafeAreaChanged += OnSafeAreaChanged;
		Refresh(ScreenManager.GetSafeArea());
	}

	private void OnDisable()
	{
		SystemEvents.OnSafeAreaChanged -= OnSafeAreaChanged;
	}

	private void OnSafeAreaChanged(Rect safeArea)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Refresh(safeArea);
	}

	private void Refresh(Rect safeArea)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (safeArea != LastSafeArea)
		{
			ApplySafeArea(safeArea);
		}
	}

	private void ApplySafeArea(Rect r)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		LastSafeArea = r;
		Vector2Int val;
		if (!ConformX)
		{
			((Rect)(ref r)).x = 0f;
			val = NativeHelpers.Screen();
			((Rect)(ref r)).width = ((Vector2Int)(ref val)).x;
		}
		if (!ConformY)
		{
			((Rect)(ref r)).y = 0f;
			val = NativeHelpers.Screen();
			((Rect)(ref r)).height = ((Vector2Int)(ref val)).y;
		}
		Vector2 position = ((Rect)(ref r)).position;
		Vector2 anchorMax = ((Rect)(ref r)).position + ((Rect)(ref r)).size;
		ref float x = ref position.x;
		float num = x;
		val = NativeHelpers.Screen();
		x = num / (float)((Vector2Int)(ref val)).x;
		ref float y = ref position.y;
		float num2 = y;
		val = NativeHelpers.Screen();
		y = num2 / (float)((Vector2Int)(ref val)).y;
		ref float x2 = ref anchorMax.x;
		float num3 = x2;
		val = NativeHelpers.Screen();
		x2 = num3 / (float)((Vector2Int)(ref val)).x;
		ref float y2 = ref anchorMax.y;
		float num4 = y2;
		val = NativeHelpers.Screen();
		y2 = num4 / (float)((Vector2Int)(ref val)).y;
		base.rectTransform.anchorMin = position;
		base.rectTransform.anchorMax = anchorMax;
	}
}
