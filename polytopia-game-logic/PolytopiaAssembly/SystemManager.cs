using UnityEngine;

public class SystemManager
{
	public enum Platforms
	{
		Unknown,
		PC,
		iOS,
		Android
	}

	public static Vector2 CURSOR_HOTSPOT = new Vector2(19f, 13f);

	protected static UIIconData.CursorData.CursorStyles currentCursorStyle = UIIconData.CursorData.CursorStyles.Default;

	protected static int hoverCounter = 0;

	public static bool IsMobile => Application.isMobilePlatform;

	public static bool IsTesla => false;

	public static bool IsSwitch => false;

	public static bool IsSteam
	{
		get
		{
			if (!IsMobile && !IsTesla)
			{
				return !IsSwitch;
			}
			return false;
		}
	}

	public static bool IsAndroid => false;

	public static bool IsIOS => false;

	public static Platforms Platform => Platforms.PC;

	public static string GetUpdateDealer()
	{
		return "Steam";
	}

	public static bool ShouldShowQuitButton()
	{
		return !IsMobile;
	}

	public static bool ShouldShowMultiplayerButton()
	{
		return true;
	}

	public static bool ShouldShowCustomLanguageOption()
	{
		return true;
	}

	public static bool ShouldShowLocalPlayerNameInput()
	{
		return true;
	}

	public static bool ShouldUseMobileUI()
	{
		return IsMobile;
	}

	public static bool ShouldUseTouchInterface()
	{
		return IsMobile;
	}

	public static bool ShouldShowMouseCursor()
	{
		return !IsMobile;
	}

	public static bool IsTouchScreenKeyboardSupported()
	{
		return TouchScreenKeyboard.isSupported;
	}

	public static void IncreaseHoveredCounter()
	{
		hoverCounter++;
		RefreshCursor();
	}

	public static void DecreaseHoveredCounter()
	{
		hoverCounter--;
		if (hoverCounter < 0)
		{
			hoverCounter = 0;
		}
		RefreshCursor();
	}

	public static void ResetHoveredCounter()
	{
		hoverCounter = 0;
		RefreshCursor();
	}

	protected static void RefreshCursor()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		UIIconData.CursorData.CursorStyles cursorStyles = ((hoverCounter > 0) ? UIIconData.CursorData.CursorStyles.Hover : UIIconData.CursorData.CursorStyles.Default);
		if (cursorStyles != currentCursorStyle)
		{
			if (!IsMobile)
			{
				Cursor.SetCursor(UIManager.IconData.GetCursor(cursorStyles), CURSOR_HOTSPOT, (CursorMode)0);
			}
			currentCursorStyle = cursorStyles;
		}
	}
}
