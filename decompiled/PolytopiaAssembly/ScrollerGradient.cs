using UnityEngine;

public class ScrollerGradient : UIBasicComponent
{
	[SerializeField]
	protected RectTransform solidTop;

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
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		solidTop.SetHeight(ScreenManager.SafeTop);
		Vector2 offsetMax = base.rectTransform.offsetMax;
		offsetMax.x = ScreenManager.SafeRight;
		base.rectTransform.offsetMax = offsetMax;
		Vector2 offsetMin = base.rectTransform.offsetMin;
		offsetMin.x = 0f - ScreenManager.SafeLeft;
		base.rectTransform.offsetMin = offsetMin;
	}
}
