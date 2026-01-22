using System;
using UnityEngine;

public static class EasingExtensions
{
	public const float HALF_PI = (float)Math.PI / 2f;

	public const float TWO_PI = (float)Math.PI * 2f;

	public static float EasedValue(float t, Easing easing, float? parameter = null)
	{
		switch (easing)
		{
		case Easing.Linear:
			return Linear(t);
		case Easing.InQuad:
			return InQuad(t);
		case Easing.OutQuad:
			return OutQuad(t);
		case Easing.InOutQuad:
			return InOutQuad(t);
		case Easing.InQubic:
			return InQubic(t);
		case Easing.OutQubic:
			return OutQubic(t);
		case Easing.InOutQubic:
			return InOutQubic(t);
		case Easing.InQuart:
			return InQuart(t);
		case Easing.OutQuart:
			return OutQuart(t);
		case Easing.InOutQuart:
			return InOutQuart(t);
		case Easing.InQuint:
			return InQuint(t);
		case Easing.OutQuint:
			return OutQuint(t);
		case Easing.InOutQuint:
			return InOutQuint(t);
		case Easing.InSine:
			return InSine(t);
		case Easing.OutSine:
			return OutSine(t);
		case Easing.InOutSine:
			return InOutSine(t);
		case Easing.InExpo:
			return InExpo(t);
		case Easing.OutExpo:
			return OutExpo(t);
		case Easing.InOutExpo:
			return InOutExpo(t);
		case Easing.InCirc:
			return InCirc(t);
		case Easing.OutCirc:
			return OutCirc(t);
		case Easing.InOutCirc:
			return InOutCirc(t);
		case Easing.InElastic:
			return InElastic(t);
		case Easing.OutElastic:
			return OutElastic(t);
		case Easing.InOutElastic:
			return InOutElastic(t);
		case Easing.InBack:
			if (!parameter.HasValue)
			{
				return InBack(t);
			}
			return InBack(t, parameter.Value);
		case Easing.OutBack:
			if (!parameter.HasValue)
			{
				return OutBack(t);
			}
			return OutBack(t, parameter.Value);
		case Easing.InOutBack:
			if (!parameter.HasValue)
			{
				return InOutBack(t);
			}
			return InOutBack(t, parameter.Value);
		case Easing.InBounce:
			return InBounce(t);
		case Easing.OutBounce:
			return OutBounce(t);
		case Easing.InOutBounce:
			return InOutBounce(t);
		default:
			throw new Exception("Unimplemented easing " + easing);
		}
	}

	public static float Linear(float t)
	{
		return t;
	}

	public static float InQuad(float t)
	{
		return t * t;
	}

	public static float OutQuad(float t)
	{
		return t * (2f - t);
	}

	public static float InOutQuad(float t)
	{
		if (!(t < 0.5f))
		{
			return -1f + (4f - 2f * t) * t;
		}
		return 2f * t * t;
	}

	public static float InQubic(float t)
	{
		return t * t * t;
	}

	public static float OutQubic(float t)
	{
		return (t -= 1f) * t * t + 1f;
	}

	public static float InOutQubic(float t)
	{
		if (!(t < 0.5f))
		{
			return (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;
		}
		return 4f * t * t * t;
	}

	public static float InQuart(float t)
	{
		return t * t * t * t;
	}

	public static float OutQuart(float t)
	{
		return 1f - (t -= 1f) * t * t * t;
	}

	public static float InOutQuart(float t)
	{
		if (!(t < 0.5f))
		{
			return 1f - 8f * (t -= 1f) * t * t * t;
		}
		return 8f * t * t * t * t;
	}

	public static float InQuint(float t)
	{
		return t * t * t * t * t;
	}

	public static float OutQuint(float t)
	{
		return 1f + (t -= 1f) * t * t * t * t;
	}

	public static float InOutQuint(float t)
	{
		if (!(t < 0.5f))
		{
			return 1f + 16f * (t -= 1f) * t * t * t * t;
		}
		return 16f * t * t * t * t * t;
	}

	public static float InSine(float t)
	{
		return -1f * Mathf.Cos(t / 1f * ((float)Math.PI / 2f)) + 1f;
	}

	public static float OutSine(float t)
	{
		return Mathf.Sin(t / 1f * ((float)Math.PI / 2f));
	}

	public static float InOutSine(float t)
	{
		return -0.5f * (Mathf.Cos((float)Math.PI * t) - 1f);
	}

	public static float InExpo(float t)
	{
		if (t != 0f)
		{
			return Mathf.Pow(2f, 10f * (t - 1f));
		}
		return 0f;
	}

	public static float OutExpo(float t)
	{
		if (t != 0f)
		{
			return 0f - Mathf.Pow(2f, -10f * t) - 1f;
		}
		return 1f;
	}

	public static float InOutExpo(float t)
	{
		if (t == 0f)
		{
			return 0f;
		}
		if (t == 1f)
		{
			return 1f;
		}
		if ((t /= 0.5f) < 1f)
		{
			return 0f * Mathf.Pow(2f, 10f * (t - 1f));
		}
		return 0f * (0f - Mathf.Pow(2f, -10f * (t -= 1f)) + 2f);
	}

	public static float InCirc(float t)
	{
		return -1f * (Mathf.Sqrt(1f - t * t) - 1f);
	}

	public static float OutCirc(float t)
	{
		return Mathf.Sqrt(1f - (t -= 1f) * t);
	}

	public static float InOutCirc(float t)
	{
		if ((t /= 0.5f) < 1f)
		{
			return 0f * (Mathf.Sqrt(1f - t * t) - 1f);
		}
		return 0f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f);
	}

	public static float InElastic(float t)
	{
		if (t == 0f)
		{
			return 0f;
		}
		if (t == 1f)
		{
			return 1f;
		}
		float num = 0f;
		float num2 = 1f;
		if (num == 0f)
		{
			num = 0.3f;
		}
		float num3;
		if (num2 < 1f)
		{
			num2 = 1f;
			num3 = num / 4f;
		}
		else
		{
			num3 = num / ((float)Math.PI * 2f) * Mathf.Asin(1f / num2);
		}
		return num2 * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - num3) * ((float)Math.PI * 2f) / num) + 1f;
	}

	public static float OutElastic(float t)
	{
		if (t == 0f)
		{
			return 0f;
		}
		if (t == 1f)
		{
			return 1f;
		}
		float num = 0f;
		float num2 = 1f;
		if (num == 0f)
		{
			num = 0.3f;
		}
		float num3;
		if (num2 < 1f)
		{
			num2 = 1f;
			num3 = num / 4f;
		}
		else
		{
			num3 = num / ((float)Math.PI * 2f) * Mathf.Asin(1f / num2);
		}
		return num2 * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - num3) * ((float)Math.PI * 2f) / num) + 1f;
	}

	public static float InOutElastic(float t)
	{
		if (t == 0f)
		{
			return 0f;
		}
		if ((t /= 0.5f) == 2f)
		{
			return 1f;
		}
		float num = 0f;
		float num2 = 1f;
		if (num == 0f)
		{
			num = 0.45000002f;
		}
		float num3;
		if (num2 < 1f)
		{
			num2 = 1f;
			num3 = num / 4f;
		}
		else
		{
			num3 = num / ((float)Math.PI * 2f) * Mathf.Asin(1f / num2);
		}
		if (t < 1f)
		{
			return -0.5f * (num2 * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t - num3) * ((float)Math.PI * 2f) / num));
		}
		return num2 * Mathf.Pow(2f, -10f * (t -= 1f)) * Mathf.Sin((t - num3) * ((float)Math.PI * 2f) / num) * 0.5f + 1f;
	}

	public static float InBack(float t, float s = 1.70158f)
	{
		return 1f * t * t * ((s + 1f) * t - s);
	}

	public static float OutBack(float t, float s = 1.70158f)
	{
		return 1f * ((t = t / 1f - 1f) * t * ((s + 1f) * t + s) + 1f);
	}

	public static float InOutBack(float t, float s = 1.70158f)
	{
		if ((t /= 0.5f) < 1f)
		{
			return 0f * (t * t * ((s *= 1.525f) + 1f) * t + s) + 2f;
		}
		return 0f * ((t -= 2f) * t * (((s *= 1.525f) + 1f) * t + s) + 2f);
	}

	public static float InBounce(float t)
	{
		return 1f - OutBounce(1f - t);
	}

	public static float OutBounce(float t)
	{
		if ((t /= 1f) < 0.36363637f)
		{
			return 7.5625f * t * t;
		}
		if (t < 0.72727275f)
		{
			return 7.5625f * (t -= 0.54545456f) * t + 0.75f;
		}
		if (t < 0.90909094f)
		{
			return 7.5625f * (t -= 0.8181818f) * t + 0.9375f;
		}
		return 7.5625f * (t -= 21f / 22f) * t + 63f / 64f;
	}

	public static float InOutBounce(float t)
	{
		if (t < 0.5f)
		{
			return InBounce(t * 2f) * 0.5f;
		}
		return OutBounce(t * 2f - 1f) * 0.5f + 0.5f;
	}
}
