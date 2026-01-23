using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UISimpleBox : Graphic
{
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		Vector2 zero = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		zero.x = 0f;
		zero.y = 0f;
		zero2.x = 1f;
		zero2.y = 1f;
		zero.x -= ((Graphic)this).rectTransform.pivot.x;
		zero.y -= ((Graphic)this).rectTransform.pivot.y;
		zero2.x -= ((Graphic)this).rectTransform.pivot.x;
		zero2.y -= ((Graphic)this).rectTransform.pivot.y;
		ref float x = ref zero.x;
		float num = x;
		Rect rect = ((Graphic)this).rectTransform.rect;
		x = num * ((Rect)(ref rect)).width;
		ref float y = ref zero.y;
		float num2 = y;
		rect = ((Graphic)this).rectTransform.rect;
		y = num2 * ((Rect)(ref rect)).height;
		ref float x2 = ref zero2.x;
		float num3 = x2;
		rect = ((Graphic)this).rectTransform.rect;
		x2 = num3 * ((Rect)(ref rect)).width;
		ref float y2 = ref zero2.y;
		float num4 = y2;
		rect = ((Graphic)this).rectTransform.rect;
		y2 = num4 * ((Rect)(ref rect)).height;
		vh.Clear();
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.position = Vector2.op_Implicit(new Vector2(zero.x, zero.y));
		simpleVert.color = Color32.op_Implicit(((Graphic)this).color);
		vh.AddVert(simpleVert);
		simpleVert.position = Vector2.op_Implicit(new Vector2(zero.x, zero2.y));
		simpleVert.color = Color32.op_Implicit(((Graphic)this).color);
		vh.AddVert(simpleVert);
		simpleVert.position = Vector2.op_Implicit(new Vector2(zero2.x, zero2.y));
		simpleVert.color = Color32.op_Implicit(((Graphic)this).color);
		vh.AddVert(simpleVert);
		simpleVert.position = Vector2.op_Implicit(new Vector2(zero2.x, zero.y));
		simpleVert.color = Color32.op_Implicit(((Graphic)this).color);
		vh.AddVert(simpleVert);
		vh.AddTriangle(0, 1, 2);
		vh.AddTriangle(2, 3, 0);
	}
}
