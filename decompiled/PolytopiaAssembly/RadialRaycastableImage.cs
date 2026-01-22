using UnityEngine;
using UnityEngine.UI;

public class RadialRaycastableImage : Image
{
	[SerializeField]
	private float _outerRadius = 0.6f;

	[SerializeField]
	private float _innerRadius = 0.1f;

	public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!((Image)this).IsRaycastLocationValid(screenPoint, eventCamera))
		{
			return false;
		}
		Rect val = ((Graphic)this).rectTransform.WorldRect();
		Vector2 val2 = screenPoint - ((Rect)(ref val)).center;
		float num;
		for (num = Mathf.Atan2(val2.y, val2.x) * 57.29578f - ((Transform)((Graphic)this).rectTransform).eulerAngles.z - 90f; num < 0f; num += 360f)
		{
		}
		float width = ((Rect)(ref val)).width;
		if (num >= (1f - ((Image)this).fillAmount) * 360f && ((Vector2)(ref val2)).magnitude <= _outerRadius * width)
		{
			return ((Vector2)(ref val2)).magnitude >= _innerRadius * width;
		}
		return false;
	}
}
