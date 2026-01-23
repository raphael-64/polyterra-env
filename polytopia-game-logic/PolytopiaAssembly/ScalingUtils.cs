using System.Collections.Generic;
using UnityEngine;

public static class ScalingUtils
{
	public const float BASE_DPI = 221f;

	public const float PIXELS_TO_UNIT = 1056f;

	private static bool isDirty = true;

	private static float dpiRatio = 1f;

	private static float? cachedDPI = null;

	private static Dictionary<string, float> DPIOverrides = new Dictionary<string, float>
	{
		{ "iPhone13,2", 460f },
		{ "iPhone13,3", 460f },
		{ "iPhone13,4", 458f }
	};

	public static void SetDirty()
	{
		isDirty = true;
	}

	public static float GetDPIRatio()
	{
		CalculateScalingValues();
		return dpiRatio;
	}

	public static float GetDPI()
	{
		if (!cachedDPI.HasValue)
		{
			cachedDPI = GetDPIInternal();
		}
		return cachedDPI.Value;
	}

	private static float GetDPIInternal()
	{
		float num = Screen.dpi;
		if (DPIOverrides.TryGetValue(SystemInfo.deviceModel, out var value))
		{
			num = value;
		}
		if (num == 0f)
		{
			num = 96f;
		}
		return num;
	}

	private static void CalculateScalingValues()
	{
		if (isDirty)
		{
			dpiRatio = GetDPI() / 221f;
			isDirty = false;
		}
	}

	public static float ScaledDragThreshold()
	{
		return 0.015f * GetDPI();
	}

	public static float ScaledMinTapThreshold()
	{
		return 0.1f * GetDPI();
	}

	public static float ScaledMaxTapThreshold()
	{
		return 0.25f * GetDPI();
	}
}
