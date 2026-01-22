using NaughtyAttributes;
using UnityEngine;

public class UISafeAreaPreventer : UIBasicComponent
{
	public enum SafeAreaPreventFlags
	{
		None = 0,
		Left = 1,
		Top = 2,
		Right = 4,
		Bottom = 8,
		All = -1
	}

	[EnumFlags]
	public SafeAreaPreventFlags preventType;

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
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		Vector2 offsetMax = base.rectTransform.offsetMax;
		Vector2 offsetMin = base.rectTransform.offsetMin;
		if ((preventType & SafeAreaPreventFlags.Left) != SafeAreaPreventFlags.None)
		{
			offsetMin.x = 0f - ScreenManager.SafeLeft;
		}
		if ((preventType & SafeAreaPreventFlags.Top) != SafeAreaPreventFlags.None)
		{
			offsetMax.y = ScreenManager.SafeTop;
		}
		if ((preventType & SafeAreaPreventFlags.Right) != SafeAreaPreventFlags.None)
		{
			offsetMax.x = ScreenManager.SafeRight;
		}
		if ((preventType & SafeAreaPreventFlags.Bottom) != SafeAreaPreventFlags.None)
		{
			offsetMin.y = 0f - ScreenManager.SafeBottom;
		}
		base.rectTransform.offsetMax = offsetMax;
		base.rectTransform.offsetMin = offsetMin;
	}
}
