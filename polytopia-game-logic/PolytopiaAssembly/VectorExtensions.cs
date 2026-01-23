using UnityEngine;

public static class VectorExtensions
{
	public static Vector2 ToVector2(this Vector3 input)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(input.x, input.y);
	}
}
