using UnityEngine;

public static class SystemEvents
{
	public delegate void OnScreenSizeChangedEvent(Vector2 screenSize);

	public delegate void OnSafeAreaChangedEvent(Rect safeArea);

	public delegate void OnFullscreenChangedEvent(FullScreenMode mode);

	public delegate void OnPurchaseManagerInitializedEvent(bool success, string errorKey);

	public delegate void OnPurchaseManagerUpdatedEvent();

	public delegate void OnMultiplayerEnabledUpdatedEvent();

	public static event OnScreenSizeChangedEvent OnScreenSizeChanged;

	public static event OnSafeAreaChangedEvent OnSafeAreaChanged;

	public static event OnFullscreenChangedEvent OnFullscreenChanged;

	public static event OnPurchaseManagerInitializedEvent OnPurchaseManagerInitialized;

	public static event OnPurchaseManagerUpdatedEvent OnPurchaseManagerUpdated;

	public static event OnMultiplayerEnabledUpdatedEvent OnMultiplayerEnabledUpdated;

	public static void ScreenSizeChanged(Vector2 screenSize)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		SystemEvents.OnScreenSizeChanged?.Invoke(screenSize);
	}

	public static void SafeAreaChanged(Rect safeArea)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		SystemEvents.OnSafeAreaChanged?.Invoke(safeArea);
	}

	public static void FullscreenChanged(FullScreenMode mode)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		SystemEvents.OnFullscreenChanged?.Invoke(mode);
	}

	public static void PurchaseManagerInitialized(bool success, string errorKey)
	{
		SystemEvents.OnPurchaseManagerInitialized?.Invoke(success, errorKey);
	}

	public static void PurchaseManagerUpdated()
	{
		SystemEvents.OnPurchaseManagerUpdated?.Invoke();
	}

	public static void MultiplayerEnabledUpdated()
	{
		SystemEvents.OnMultiplayerEnabledUpdated?.Invoke();
	}
}
