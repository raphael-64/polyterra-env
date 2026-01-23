using System;

public static class MathUtils
{
	public static bool IsApproximately(float a, float b, float threshold)
	{
		if (a >= b - threshold)
		{
			return a <= b + threshold;
		}
		return false;
	}

	public static float Lerp(float a, float b, float interpolation)
	{
		float num = Math.Max(0f, Math.Min(1f, interpolation));
		return a * (1f - num) + b * num;
	}
}
