using UnityEngine;

public static class TransformExtensions
{
	public static string FullName(this Transform transform)
	{
		Transform val = transform;
		string text = ((Object)transform).name;
		for (int i = 0; i < 100; i++)
		{
			val = val.parent;
			if ((Object)(object)val == (Object)null)
			{
				return text;
			}
			text = ((object)val)?.ToString() + "/" + text;
		}
		return text;
	}
}
