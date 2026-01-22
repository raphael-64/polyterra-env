public class UIEvents
{
	public delegate void OnLoadingScreenHiddenEvent();

	public delegate void OnQuickActionsOpenEvent(WorldCoordinates coordinates, bool open);

	public delegate void OnScreenOpenEvent(UIConstants.Screens screen);

	public delegate void OnScreenCloseEvent(UIConstants.Screens screen);

	public delegate void OnAutoCameraFocusEnabledEvent(bool enabled);

	public delegate void OnInteractionBarOpenEvent(bool open);

	public delegate void OnPopupStackChangedEvent();

	public delegate void OnForceRefreshHudEvent();

	public static event OnLoadingScreenHiddenEvent OnLoadingScreenHidden;

	public static event OnQuickActionsOpenEvent OnQuickActionsOpen;

	public static event OnScreenOpenEvent OnScreenOpen;

	public static event OnScreenCloseEvent OnScreenClose;

	public static event OnAutoCameraFocusEnabledEvent OnAutoCameraFocusEnabled;

	public static event OnInteractionBarOpenEvent OnInteractionBarOpen;

	public static event OnPopupStackChangedEvent OnPopupStackChanged;

	public static event OnForceRefreshHudEvent OnForceRefreshHud;

	public static void LoadingScreenHidden()
	{
		UIEvents.OnLoadingScreenHidden?.Invoke();
	}

	public static void QuickActionsOpen(WorldCoordinates coordinates, bool open)
	{
		UIEvents.OnQuickActionsOpen?.Invoke(coordinates, open);
	}

	public static void ScreenOpen(UIConstants.Screens screen)
	{
		UIEvents.OnScreenOpen?.Invoke(screen);
	}

	public static void ScreenClose(UIConstants.Screens screen)
	{
		UIEvents.OnScreenClose?.Invoke(screen);
	}

	public static void AutoCameraFocusEnabled(bool enabled)
	{
		UIEvents.OnAutoCameraFocusEnabled?.Invoke(enabled);
	}

	public static void InteractionBarOpen(bool open)
	{
		UIEvents.OnInteractionBarOpen?.Invoke(open);
	}

	public static void PopupStackChanged()
	{
		UIEvents.OnPopupStackChanged?.Invoke();
	}

	public static void ForceRefreshHud()
	{
		UIEvents.OnForceRefreshHud?.Invoke();
	}
}
