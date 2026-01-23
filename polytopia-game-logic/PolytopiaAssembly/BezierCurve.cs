using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BezierCurve : MonoBehaviour
{
	public class BezierPath
	{
		public List<Vector3> pathPoints;

		private int segments;

		public int pointCount = 50;

		public BezierPath()
		{
			pathPoints = new List<Vector3>();
		}

		public BezierPath(int segmentCount)
		{
			pathPoints = new List<Vector3>();
			pointCount = segmentCount;
		}

		public void DeletePath()
		{
			pathPoints.Clear();
		}

		private Vector3 BezierPathCalculation(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			float num = t * t;
			float num2 = t * num;
			float num3 = 1f - t;
			float num4 = num3 * num3;
			return num3 * num4 * p0 + 3f * num4 * t * p1 + 3f * num3 * num * p2 + num2 * p3;
		}

		public void CreateCurve(Vector3[] controlPoints)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			segments = controlPoints.Length / 3;
			for (int i = 0; i < controlPoints.Length - 3; i += 3)
			{
				Vector3 p = controlPoints[i];
				Vector3 p2 = controlPoints[i + 1];
				Vector3 p3 = controlPoints[i + 2];
				Vector3 p4 = controlPoints[i + 3];
				if (i == 0)
				{
					pathPoints.Add(BezierPathCalculation(p, p2, p3, p4, 0f));
				}
				for (int j = 0; j < pointCount / segments; j++)
				{
					float t = 1f / (float)(pointCount / segments - 1) * (float)j;
					Vector3 val = default(Vector3);
					val = BezierPathCalculation(p, p2, p3, p4, t);
					pathPoints.Add(val);
				}
			}
		}
	}

	public Transform startPoint;

	public Transform startHandle;

	public Transform endHandle;

	public Transform endPoint;

	[Space]
	public Material material;

	public int segmentCount = 100;

	public float lineWidth = 0.25f;

	[Header("Debug")]
	public bool drawGizmos;

	[ToggleButton]
	public bool updateCurve;

	private BezierPath path;

	private LineRenderer line;

	private void Start()
	{
		DrawCurve();
	}

	private void Update()
	{
		if (updateCurve)
		{
			DrawCurve();
		}
	}

	public static BakedLine GenerateLineWithPath(Vector3[] positions, Transform parentTransform, Material material, int segmentCount = 8)
	{
		BezierPath bezierPath = new BezierPath(segmentCount);
		bezierPath.CreateCurve(positions);
		BakedLine bakedLine = CreateLine(parentTransform, material);
		SetPathOnLine(bakedLine.LineRenderer, bezierPath);
		return bakedLine;
	}

	private static BakedLine CreateLine(Transform parentTransform, Material material)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("line");
		val.transform.parent = parentTransform;
		val.transform.rotation = parentTransform.rotation;
		val.transform.localPosition = Vector3.zero;
		LineRenderer val2 = val.AddComponent<LineRenderer>();
		val2.textureMode = (LineTextureMode)1;
		val2.alignment = (LineAlignment)1;
		val2.numCapVertices = 4;
		((Renderer)val2).material = material;
		val2.useWorldSpace = false;
		((Renderer)val2).sortingLayerID = MeshCache.TERRAIN_LAYER_ID;
		MeshRenderer val3 = val.AddComponent<MeshRenderer>();
		((Renderer)val3).sharedMaterial = material;
		((Renderer)val3).shadowCastingMode = (ShadowCastingMode)0;
		((Renderer)val3).lightProbeUsage = (LightProbeUsage)0;
		((Renderer)val3).reflectionProbeUsage = (ReflectionProbeUsage)0;
		((Renderer)val3).sortingLayerID = MeshCache.TERRAIN_LAYER_ID;
		BakedLine bakedLine = val.AddComponent<BakedLine>();
		bakedLine.LineRenderer = val2;
		bakedLine.MeshRenderer = val3;
		bakedLine.MeshFilter = val.AddComponent<MeshFilter>();
		return bakedLine;
	}

	private static void SetPathOnLine(LineRenderer line, BezierPath path, float lineWidth = 0.04f)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		line.startWidth = lineWidth;
		line.endWidth = lineWidth;
		line.positionCount = path.pathPoints.Count;
		for (int i = 0; i < line.positionCount; i++)
		{
			Vector3 val = path.pathPoints[i];
			line.SetPosition(i, val);
		}
	}

	private void UpdatePath()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] controlPoints = (Vector3[])(object)new Vector3[4] { startPoint.localPosition, startHandle.localPosition, endHandle.localPosition, endPoint.localPosition };
		path.DeletePath();
		path.CreateCurve(controlPoints);
	}

	public void DrawCurve()
	{
		if (path == null)
		{
			path = new BezierPath(segmentCount);
		}
		else
		{
			path.pointCount = segmentCount;
		}
		UpdatePath();
		if ((Object)(object)line == (Object)null)
		{
			line = CreateLine(((Component)this).transform, material).LineRenderer;
		}
		SetPathOnLine(line, path);
	}

	private void OnDrawGizmos()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		if (!drawGizmos)
		{
			return;
		}
		bool flag = true;
		Gizmos.color = Color.green;
		if ((Object)(object)startPoint != (Object)null)
		{
			Gizmos.DrawWireSphere(startPoint.position, 0.2f);
		}
		else
		{
			flag = false;
		}
		Gizmos.color = new Color(0f, 0.5f, 0f);
		if ((Object)(object)startHandle != (Object)null)
		{
			Gizmos.DrawWireSphere(startHandle.position, 0.2f);
		}
		else
		{
			flag = false;
		}
		Gizmos.color = new Color(0.5f, 0f, 0f);
		if ((Object)(object)endHandle != (Object)null)
		{
			Gizmos.DrawWireSphere(endHandle.position, 0.2f);
		}
		else
		{
			flag = false;
		}
		Gizmos.color = Color.red;
		if ((Object)(object)endPoint != (Object)null)
		{
			Gizmos.DrawWireSphere(endPoint.position, 0.2f);
		}
		else
		{
			flag = false;
		}
		if (flag)
		{
			if (path == null)
			{
				path = new BezierPath(segmentCount);
			}
			UpdatePath();
			for (int i = 1; i <= path.pointCount; i++)
			{
				Vector3 val = path.pathPoints[i - 1];
				Vector3 val2 = path.pathPoints[i];
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(val, val2);
			}
		}
	}
}
