using UnityEngine;

public class ColorUtil
{
	public static Color ClearWhite = new Color(1f, 1f, 1f, 0f);

	public static Color SetAlphaOnColor(Color sourceColor, float alpha)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		sourceColor.a = alpha;
		return sourceColor;
	}

	public static Color ColorFromInt(uint col)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		return Color32.op_Implicit(new Color32
		{
			b = (byte)(col & 0xFF),
			g = (byte)((col >> 8) & 0xFF),
			r = (byte)((col >> 16) & 0xFF),
			a = byte.MaxValue
		});
	}

	public static uint ColorToInt(Color col)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		return ((uint)(col.r * 255f) << 24) + ((uint)(col.g * 255f) << 16) + ((uint)(col.b * 255f) << 8) + (uint)(col.a * 255f);
	}

	public static string ColorToHex(Color col)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return ColorToInt(col).ToString("X8");
	}
}
