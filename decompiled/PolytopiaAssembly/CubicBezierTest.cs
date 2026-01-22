using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using NaughtyAttributes;
using UnityEngine;

public class CubicBezierTest : MonoBehaviour
{
	public RectTransform start;

	public RectTransform startCenterHandle;

	public RectTransform centerStartHandle;

	public RectTransform center;

	public RectTransform centerEndHandle;

	public RectTransform endCenterHandle;

	public RectTransform end;

	public RectTransform target;

	[Space]
	public float handleAngleOffset;

	[Space]
	public float animTime = 1f;

	public float centerOffset = 0.15f;

	public float lookAheadOffset = 0.01f;

	public PathType pathType = (PathType)1;

	public PathMode pathMode = (PathMode)3;

	public int pathResolution = 10;

	public Ease pathEase = (Ease)1;

	private Vector3 lastStartPos;

	private Vector3 lastEndPos;

	public float lastHandleAngleOffset;

	public float lastCenterOffset = 0.15f;

	[Button("Run")]
	public void RunAnimation()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		((Transform)target).position = ((Transform)start).position;
		Vector3[] waypoints = GetWaypoints();
		TweenSettingsExtensions.SetLookAt(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Path, PathOptions>>(ShortcutExtensions.DOPath((Transform)(object)target, waypoints, animTime, pathType, pathMode, pathResolution, (Color?)Color.blue), pathEase), lookAheadOffset, (Vector3?)null, (Vector3?)null);
	}

	private void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (lastStartPos != ((Transform)start).position || lastEndPos != ((Transform)end).position || lastHandleAngleOffset != handleAngleOffset || lastCenterOffset != centerOffset)
		{
			Vector3[] waypoints = GetWaypoints();
			((Transform)startCenterHandle).position = waypoints[1];
			((Transform)centerStartHandle).position = waypoints[2];
			((Transform)center).position = waypoints[0];
			((Transform)centerEndHandle).position = waypoints[4];
			((Transform)endCenterHandle).position = waypoints[5];
			((Transform)target).position = ((Transform)start).position;
			lastStartPos = ((Transform)start).position;
			lastEndPos = ((Transform)end).position;
			lastHandleAngleOffset = handleAngleOffset;
			lastCenterOffset = centerOffset;
		}
	}

	private Vector3[] GetWaypoints()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = new Vector3[6];
		float num = Vector3.Distance(((Transform)start).position, ((Transform)end).position);
		Vector3 val = Vector3.Lerp(((Transform)start).position, ((Transform)end).position, 0.5f);
		val.y += num * centerOffset;
		float num2 = AngleInDeg(((Transform)start).position, ((Transform)end).position);
		float num3 = 1f - Mathf.Abs(num2) / 90f;
		float num4 = handleAngleOffset * (centerOffset / 0.15f) * num3;
		array[0] = val;
		array[1] = ((Transform)start).position + GetHandlePosition(((Transform)start).position, val, num4);
		array[2] = val + GetHandlePosition(val, ((Transform)start).position, 0f - num4);
		array[3] = ((Transform)end).position;
		array[4] = val + GetHandlePosition(val, ((Transform)end).position, num4);
		array[5] = ((Transform)end).position + GetHandlePosition(((Transform)end).position, val, 0f - num4);
		return (Vector3[])(object)array;
	}

	private Vector3 GetHandlePosition(Vector3 from, Vector3 to, float angleOffset)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		float num = AngleInRad(from, to) + angleOffset;
		return new Vector3(Mathf.Cos(num), Mathf.Sin(num), 0f) * (Vector3.Distance(from, to) * 0.33333f);
	}

	public static float AngleInRad(Vector3 vec1, Vector3 vec2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Atan2(vec2.y - vec1.y, vec2.x - vec1.x);
	}

	public static float AngleInDeg(Vector3 vec1, Vector3 vec2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return AngleInRad(vec1, vec2) * 180f / (float)Math.PI;
	}
}
