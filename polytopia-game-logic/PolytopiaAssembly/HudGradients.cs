using UnityEngine;

public class HudGradients : UIBasicComponent
{
	[SerializeField]
	protected RectTransform solidTop;

	[SerializeField]
	protected UIGradient fadeTop;

	[SerializeField]
	protected RectTransform solidBottom;

	[SerializeField]
	protected UIGradient fadeBottom;

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
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		solidTop.SetHeight(ScreenManager.SafeTop);
		solidBottom.SetHeight(ScreenManager.SafeBottom);
		Vector2 offsetMax = base.rectTransform.offsetMax;
		offsetMax.x = ScreenManager.SafeRight;
		base.rectTransform.offsetMax = offsetMax;
		Vector2 offsetMin = base.rectTransform.offsetMin;
		offsetMin.x = 0f - ScreenManager.SafeLeft;
		base.rectTransform.offsetMin = offsetMin;
	}

	public UIGradient GetTopFade()
	{
		return fadeTop;
	}
}
