using UnityEngine;

public static class CameraExtensions
{
	public static Bounds OrthographicBounds(this Camera camera)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = NativeHelpers.Screen();
		float num = ((Vector2Int)(ref val)).x;
		val = NativeHelpers.Screen();
		float num2 = num / (float)((Vector2Int)(ref val)).y;
		float num3 = camera.orthographicSize * 2f;
		return new Bounds(((Component)camera).transform.position, new Vector3(num3 * num2, num3, 0f));
	}
}
