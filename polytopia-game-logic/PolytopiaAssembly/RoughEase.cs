using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RoughEase
{
	public enum Taper
	{
		None,
		Out,
		In,
		Both
	}

	internal class EasePoint
	{
		public float time;

		public float gap;

		public float value;

		public float change;

		public EasePoint next;

		public EasePoint prev;

		public EasePoint(float time, float value, EasePoint next)
		{
			this.time = time;
			this.value = value;
			if (next != null)
			{
				this.next = next;
				next.prev = this;
				change = next.value - value;
				gap = next.time - time;
			}
		}
	}

	protected static Dictionary<string, RoughEase> cachedEases = new Dictionary<string, RoughEase>();

	private EasePoint first;

	private EasePoint last;

	private bool restrictMinAndMax;

	public RoughEase(float strength = 1f, uint points = 20u, bool restrictMinAndMax = false, EaseFunction templateEase = null, Taper taper = Taper.None, bool randomize = true, string name = "")
	{
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(name))
		{
			cachedEases.Add(name, this);
		}
		this.restrictMinAndMax = restrictMinAndMax;
		List<Vector2> list = new List<Vector2>();
		strength *= 0.4f;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		int num4 = (int)points;
		while (--num4 > -1)
		{
			num = (randomize ? Random.value : (1f / (float)points * (float)num4));
			num2 = ((templateEase != null) ? templateEase.Invoke(num, 1f, 0f, 0f) : num);
			if (taper == Taper.None)
			{
				num3 = strength;
			}
			else if (taper == Taper.Out)
			{
				float num5 = 1f - num;
				num3 = num5 * num5 * strength;
			}
			else if (taper == Taper.In)
			{
				num3 = num * num * strength;
			}
			else if (taper == Taper.Both && num < 0.5f)
			{
				float num6 = num * 2f;
				num3 = num6 * num6 * 0.5f * strength;
			}
			else if (taper == Taper.Both && num >= 0.5f)
			{
				float num7 = (1f - num) * 2f;
				num3 = num7 * num7 * 0.5f * strength;
			}
			num2 = (randomize ? (num2 + (Random.value * num3 - num3 * 0.5f)) : ((num4 % 2 != 0) ? (num2 - num3 * 0.5f) : (num2 + num3 * 0.5f)));
			if (restrictMinAndMax)
			{
				num2 = Mathf.Max(0f, Mathf.Min(num2, 1f));
			}
			list.Add(new Vector2(num, num2));
		}
		list.Sort((Vector2 x, Vector2 y) => x.x.CompareTo(y.x));
		first = (last = new EasePoint(1f, 1f, null));
		num4 = (int)points;
		while (--num4 > -1)
		{
			first = new EasePoint(list[num4].x, list[num4].y, first);
		}
		first = new EasePoint(0f, 0f, (first.time != 0f) ? first : first.next);
	}

	public static EaseFunction Create(float strength = 1f, uint points = 20u, bool restrictMinAndMax = false, EaseFunction templateEase = null, Taper taper = Taper.None, bool randomize = true, string name = "")
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (!HaveCachedEase(name))
		{
			return new EaseFunction(new RoughEase(strength, points, restrictMinAndMax, templateEase, taper, randomize, name).Ease);
		}
		return ByName(name);
	}

	public static bool HaveCachedEase(string name)
	{
		return cachedEases.ContainsKey(name);
	}

	public static EaseFunction ByName(string name)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		if (HaveCachedEase(name))
		{
			return new EaseFunction(cachedEases[name].Ease);
		}
		return null;
	}

	public float Ease(float time, float duration, float unusedOvershoot, float unusedPeriod)
	{
		float num = time / duration;
		EasePoint next;
		if (num < 0.5f)
		{
			next = first;
			while (next.time <= num)
			{
				next = next.next;
			}
			next = next.next;
		}
		else
		{
			next = last;
			while (next.time >= num)
			{
				next = next.prev;
			}
		}
		float num2 = 0f + (next.value + Math.Abs(num - next.time) / next.gap * next.change);
		if (restrictMinAndMax)
		{
			num2 = Mathf.Max(0f, Mathf.Min(num2, 1f));
		}
		return num2;
	}
}
