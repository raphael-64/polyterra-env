using UnityEngine;

public static class MapExtensions
{
	public static Vector2 ToPosition(this WorldCoordinates coordinates)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2((float)(coordinates.X - coordinates.Y) * 0.4811f, (float)(coordinates.X + coordinates.Y) * 0.288f);
	}

	public static WorldCoordinates ToWorldCoordinates(this Vector2 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		float num = position.x / 0.4811f + 0f;
		float num2 = position.y / 0.288f + -0.223f;
		int x = Mathf.RoundToInt((num + num2) * 0.5f);
		int y = Mathf.RoundToInt((num2 - num) * 0.5f);
		return new WorldCoordinates(x, y);
	}

	public static WorldCoordinates ScreenToWorldCoordinates(this Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		int x = Mathf.RoundToInt((position.x / 0.4811f + position.y / 0.288f) / 2f);
		int y = Mathf.RoundToInt((position.y / 0.288f + (0f - position.x / 0.4811f)) / 2f);
		return new WorldCoordinates(x, y);
	}
}
