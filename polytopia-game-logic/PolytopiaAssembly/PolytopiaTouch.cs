using UnityEngine;

public class PolytopiaTouch
{
	private const float TAP_TIME = 0.4f;

	public TouchPhase phase;

	public int fingerId;

	public Vector2 position;

	public Vector2 deltaPosition;

	public Vector2 startPosition;

	public float startTime;

	public float maxSquareDistance;

	public static PolytopiaTouch Create(int fingerId, Vector2 position)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return new PolytopiaTouch
		{
			fingerId = fingerId,
			startPosition = position,
			startTime = Time.realtimeSinceStartup
		};
	}

	public void UpdateWithTouch(Touch touch)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		UpdateWithData(((Touch)(ref touch)).phase, ((Touch)(ref touch)).position, ((Touch)(ref touch)).deltaPosition);
	}

	public void UpdateWithData(TouchPhase phase, Vector2 position, Vector2 deltaPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		this.phase = phase;
		this.position = position;
		this.deltaPosition = deltaPosition;
		float num = maxSquareDistance;
		Vector2 val = position - startPosition;
		maxSquareDistance = Mathf.Max(num, ((Vector2)(ref val)).sqrMagnitude);
	}

	public bool IsValidTap()
	{
		float num = Mathf.Pow(ScalingUtils.ScaledMinTapThreshold(), 2f);
		float num2 = Mathf.Pow(ScalingUtils.ScaledMaxTapThreshold(), 2f);
		if (maxSquareDistance > num2)
		{
			return false;
		}
		float num3 = (maxSquareDistance - num) / (num2 - num);
		return Time.realtimeSinceStartup - startTime <= Mathf.Lerp(0.4f, 0.060000002f, num3);
	}

	public bool IsValidLongPress(float tapTime = 0.4f)
	{
		if (IsValidTap())
		{
			return false;
		}
		float num = Mathf.Pow(ScalingUtils.ScaledDragThreshold(), 2f);
		if (maxSquareDistance > num)
		{
			return false;
		}
		return Time.realtimeSinceStartup - startTime >= tapTime;
	}

	public override string ToString()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		return $"{GetType()}, id {fingerId}, phase {phase}, position {position}, startposition {startPosition}, deltaposition {deltaPosition}, starttime {startTime}, maxsquaredistance {maxSquareDistance}";
	}
}
