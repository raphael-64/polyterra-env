using UnityEngine;

public static class GeometryUtils
{
	public static Vector2 ClosestPoint(this Rect rect, Vector2 position)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Bounds val = default(Bounds);
		((Bounds)(ref val))._002Ector(Vector2.op_Implicit(((Rect)(ref rect)).center), Vector2.op_Implicit(((Rect)(ref rect)).size));
		return Vector2.op_Implicit(((Bounds)(ref val)).ClosestPoint(Vector2.op_Implicit(position)));
	}

	public static Vector3 ClosestPointOnLine(Vector3 start, Vector3 end, Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = end - start;
		float magnitude = ((Vector3)(ref val)).magnitude;
		val /= magnitude;
		float num = Vector3.Dot(position - start, val);
		return start + val * Mathf.Clamp(num, 0f, magnitude);
	}
}
