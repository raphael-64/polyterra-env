using UnityEngine;
using UnityEngine.UI;

public class CityProgressBarGraphics : Image
{
	protected Color m_fillColor = Color.white;

	protected int[] m_fields;

	protected bool m_inverseFields;

	public int[] Fields
	{
		get
		{
			return m_fields;
		}
		set
		{
			m_fields = value;
			((Graphic)this).SetVerticesDirty();
		}
	}

	public Color FillColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_fillColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_fillColor = value;
			((Graphic)this).SetVerticesDirty();
		}
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		((Image)this).OnPopulateMesh(toFill);
		if (m_fields == null || m_fields.Length == 0)
		{
			return;
		}
		for (int i = 0; i < toFill.currentVertCount; i++)
		{
			UIVertex simpleVert = UIVertex.simpleVert;
			toFill.PopulateUIVertex(ref simpleVert, i);
			Color val = Color32.op_Implicit(simpleVert.color);
			if (IsFieldVertex(i))
			{
				val = FillColor;
			}
			simpleVert.color = Color32.op_Implicit(val);
			toFill.SetUIVertex(simpleVert, i);
		}
	}

	private bool IsFieldVertex(int idx)
	{
		for (int i = 0; i < m_fields.Length; i++)
		{
			int num = m_fields[i] * 4;
			if (idx >= num && idx < num + 4)
			{
				return true;
			}
		}
		return false;
	}
}
