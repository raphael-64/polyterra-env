using System;
using UnityEngine;
using UnityEngine.UI;

public class UIGradient : BaseMeshEffect
{
	public struct Matrix2x3
	{
		public float m00;

		public float m01;

		public float m02;

		public float m10;

		public float m11;

		public float m12;

		public Matrix2x3(float m00, float m01, float m02, float m10, float m11, float m12)
		{
			this.m00 = m00;
			this.m01 = m01;
			this.m02 = m02;
			this.m10 = m10;
			this.m11 = m11;
			this.m12 = m12;
		}

		public static Vector2 operator *(Matrix2x3 m, Vector2 v)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			float num = m.m00 * v.x - m.m01 * v.y + m.m02;
			float num2 = m.m10 * v.x + m.m11 * v.y + m.m12;
			return new Vector2(num, num2);
		}
	}

	public Color m_color1 = Color.white;

	public Color m_color2 = Color.white;

	[Range(-180f, 180f)]
	public float m_angle;

	public bool m_ignoreRatio = true;

	public bool m_raycstTarget = true;

	public override void ModifyMesh(VertexHelper vh)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			Rect rect = ((BaseMeshEffect)this).graphic.rectTransform.rect;
			Vector2 dir = RotationDir(m_angle);
			if (!m_ignoreRatio)
			{
				dir = CompensateAspectRatio(rect, dir);
			}
			Matrix2x3 matrix2x = LocalPositionMatrix(rect, dir);
			UIVertex val = default(UIVertex);
			for (int i = 0; i < vh.currentVertCount; i++)
			{
				vh.PopulateUIVertex(ref val, i);
				Vector2 val2 = matrix2x * Vector2.op_Implicit(val.position);
				ref Color32 color = ref val.color;
				color = Color32.op_Implicit(Color32.op_Implicit(color) * Color.Lerp(m_color2, m_color1, val2.y));
				vh.SetUIVertex(val, i);
			}
			((BaseMeshEffect)this).graphic.raycastTarget = m_raycstTarget;
		}
	}

	public void SetColor(Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (color.r != m_color1.r || color.g != m_color1.g || color.b != m_color1.b)
		{
			m_color1 = new Color(color.r, color.g, color.b, m_color1.a);
			m_color2 = new Color(color.r, color.g, color.b, m_color2.a);
			((BaseMeshEffect)this).graphic.SetVerticesDirty();
		}
	}

	public static Matrix2x3 LocalPositionMatrix(Rect rect, Vector2 dir)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		float x = dir.x;
		float y = dir.y;
		Vector2 min = ((Rect)(ref rect)).min;
		Vector2 size = ((Rect)(ref rect)).size;
		float num = 0.5f;
		float num2 = min.x / size.x + num;
		float num3 = min.y / size.y + num;
		float m = x / size.x;
		float m2 = y / size.y;
		float m3 = 0f - (num2 * x - num3 * y - num);
		float m4 = y / size.x;
		float m5 = x / size.y;
		float m6 = 0f - (num2 * y + num3 * x - num);
		return new Matrix2x3(m, m2, m3, m4, m5, m6);
	}

	private Vector2 RotationDir(float angle)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float num = angle * ((float)Math.PI / 180f);
		float num2 = Mathf.Cos(num);
		float num3 = Mathf.Sin(num);
		return new Vector2(num2, num3);
	}

	private Vector2 CompensateAspectRatio(Rect rect, Vector2 dir)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Rect)(ref rect)).height / ((Rect)(ref rect)).width;
		dir.x *= num;
		return ((Vector2)(ref dir)).normalized;
	}
}
