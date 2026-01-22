using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(VertexColorCompanion))]
public class VertexColorTest : Image
{
	[Range(0f, 36f)]
	public int testVertex;

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		((Image)this).OnPopulateMesh(toFill);
		VertexColorCompanion component = ((Component)this).GetComponent<VertexColorCompanion>();
		if (Object.op_Implicit((Object)(object)component))
		{
			testVertex = component.testVertex;
		}
		for (int i = 0; i < toFill.currentVertCount; i++)
		{
			UIVertex simpleVert = UIVertex.simpleVert;
			toFill.PopulateUIVertex(ref simpleVert, i);
			Vector3 val = simpleVert.position;
			Color val2 = Color32.op_Implicit(simpleVert.color);
			if (i >= testVertex && i < testVertex + 4)
			{
				val2 = Color.red;
				val += new Vector3(0f, 1f, 0f);
			}
			simpleVert.color = Color32.op_Implicit(val2);
			simpleVert.position = val;
			toFill.SetUIVertex(simpleVert, i);
		}
	}
}
