using UnityEngine;

public static class ColorConstants
{
	public static Color blue = new Color(0f, 0.6f, 1f, 1f);

	public static Color green = new Color(0f, 0.8f, 0.2f, 1f);

	public static Color red = new Color(1f, 0.2f, 0f, 1f);

	public static Color yellow = new Color(1f, 0.8f, 0f, 1f);

	public static Color orange = new Color(1f, 0.6f, 0f, 1f);

	public static Color gray = new Color(0.4f, 0.4f, 0.4f, 1f);

	public static Color darkGray = new Color(0.2f, 0.2f, 0.2f, 1f);

	public static Color pink = new Color(0.9647f, 0.4078f, 0.6667f, 1f);

	public static Color glowColor = new Color(0.4f, 1f, 1f, 1f);

	public static UIButtonBase.ColorStates redButtonColorStates = new UIButtonBase.ColorStates
	{
		defaultColor = red,
		hoverColor = Color.white,
		highlightedColor = Color.blue,
		highlightedHoverColor = Color.blue,
		disabledColor = Color.red
	};
}
