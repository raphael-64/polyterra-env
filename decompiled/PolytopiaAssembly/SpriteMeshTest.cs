using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteMeshTest : MonoBehaviour
{
	public RectTransform target;

	public float targetSize = 100f;

	public RectTransform resultImg;

	public Vector2 verticeOffset = Vector2.zero;

	public Vector3 positionOffset = Vector3.zero;

	public bool centerImage = true;

	[ToggleButton]
	public bool refresh;

	private List<Vector3> points = new List<Vector3>();

	public void Start()
	{
		UpdateSize();
	}

	private void UpdateSize()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		Image[] componentsInChildren = ((Component)target).GetComponentsInChildren<Image>();
		Bounds val = default(Bounds);
		float uI_PIXELS_PER_UNIT = UIConstants.UI_PIXELS_PER_UNIT;
		points.Clear();
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			Image val2 = componentsInChildren[i];
			Sprite sprite = val2.sprite;
			Vector2[] vertices = sprite.vertices;
			Log.Verbose("Image pivot : {0}", new object[1] { sprite.pivot });
			Log.Verbose("Image textureRectOffset : {0}", new object[1] { sprite.textureRectOffset });
			object[] array = new object[1];
			Bounds bounds = sprite.bounds;
			array[0] = ((Bounds)(ref bounds)).size.y;
			Log.Verbose("Image bounds.size.y : {0}", array);
			Rect rect = sprite.rect;
			Vector2 val3 = -(((Rect)(ref rect)).size / 2f - sprite.pivot);
			rect = sprite.rect;
			Vector2 val4 = val3 / ((Rect)(ref rect)).size - (((Graphic)val2).rectTransform.pivot - new Vector2(0.5f, 0.5f));
			bounds = sprite.bounds;
			Vector2 val5 = Vector2.op_Implicit(((Bounds)(ref bounds)).size) * val4;
			object[] array2 = new object[1];
			rect = sprite.rect;
			array2[0] = ((Rect)(ref rect)).size;
			Log.Verbose("imgSprite.rect.size: {0}", array2);
			Log.Verbose("pivotMultiplier: {0}", new object[1] { val4 });
			Log.Verbose("pivotOffset: {0}", new object[1] { val5.y });
			Log.Verbose("image.rectTransform.pivot: {0}", new object[1] { ((Graphic)val2).rectTransform.pivot });
			Vector2[] array3 = vertices;
			for (int j = 0; j < array3.Length; j++)
			{
				Vector3 val6 = Vector2.op_Implicit((array3[j] + val5 + verticeOffset) * uI_PIXELS_PER_UNIT);
				Vector3 val7 = ((Transform)target).InverseTransformPoint(((Component)val2).transform.position + positionOffset) + val6;
				((Bounds)(ref val)).Encapsulate(val7);
				points.Add(val7);
			}
		}
		resultImg.anchoredPosition = Vector2.op_Implicit(((Bounds)(ref val)).center);
		resultImg.sizeDelta = Vector2.op_Implicit(((Bounds)(ref val)).size);
		if (centerImage)
		{
			float num2 = targetSize / Mathf.Max(((Bounds)(ref val)).size.x, ((Bounds)(ref val)).size.y);
			((Transform)target).localScale = Vector3.one * num2;
			target.anchoredPosition = Vector2.op_Implicit(-(((Bounds)(ref val)).center * num2));
		}
	}

	private void Update()
	{
		if (refresh)
		{
			UpdateSize();
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (points.Count <= 0)
		{
			return;
		}
		Gizmos.color = Color.red;
		foreach (Vector3 point in points)
		{
			Gizmos.DrawSphere(((Component)this).transform.position + point, 0.5f);
		}
	}
}
