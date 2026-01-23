using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(VertexColorTest))]
public class VertexColorCompanion : MonoBehaviour
{
	[Range(0f, 36f)]
	public int testVertex;

	protected int lastVert;

	private void Update()
	{
		if (testVertex != lastVert)
		{
			lastVert = testVertex;
			((Graphic)((Component)this).GetComponent<VertexColorTest>()).SetAllDirty();
		}
	}
}
