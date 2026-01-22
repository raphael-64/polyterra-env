using System;
using NaughtyAttributes;
using UnityEngine;

public class UIPositionTester : UIBasicComponent
{
	public RectTransform target;

	public float scaleFactor = -1f;

	[Info]
	public string info = "info";

	private void Start()
	{
		Calculate();
	}

	[Button("Recalculate")]
	public void Calculate()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		float num = ((scaleFactor > 0f) ? scaleFactor : UIManager.Canvas.scaleFactor);
		Rect rect = target.rect;
		Rect worldRect = target.GetWorldRect(num);
		Vector2 val = Vector2.op_Implicit(((Transform)base.rectTransform).InverseTransformPoint(Vector2.op_Implicit(((Rect)(ref worldRect)).center)));
		float num2 = val.y + ((Rect)(ref rect)).height * 0.5f;
		float num3 = val.y - ((Rect)(ref rect)).height * 0.5f;
		Rect rect2 = base.rectTransform.rect;
		Rect worldRect2 = base.rectTransform.GetWorldRect(num);
		float num4 = ((Rect)(ref rect2)).center.y + ((Rect)(ref rect2)).height * 0.5f;
		float num5 = ((Rect)(ref rect2)).center.y - ((Rect)(ref rect2)).height * 0.5f;
		string text = $"ScaleFactor:{num}%\nTarget Rect: {RectStr(rect)}\nTarget World Rect: {RectStr(worldRect)}\ntargetPosInMe: {val}\ntargetBottom: {num3}\ntargetTop: {num2}\n\nThis Rect: {RectStr(rect2)}\nThis World Rect: {RectStr(worldRect2)}\nthisBottom:{num5}\nthisTop: {num4}\n\n{CheckIsTargetOutsideTop(num2, num4)}\n{CheckIsTargetOutsideBottom(num3, num5)}\n";
		info = text;
		Log.Verbose(info, Array.Empty<object>());
	}

	protected string RectStr(Rect rect)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		return $"pos x:{((Rect)(ref rect)).position.x}, y:{((Rect)(ref rect)).position.y} :: center x:{((Rect)(ref rect)).center.x}, y:{((Rect)(ref rect)).center.y} :: size width:{((Rect)(ref rect)).size.x}, height:{((Rect)(ref rect)).size.y}";
	}

	protected string CheckIsTargetOutsideTop(float targetTop, float viewportTop)
	{
		if (!(targetTop > viewportTop))
		{
			return "<color=#00FF00>Target inside Top</color>";
		}
		return $"<color=#FF0000>Target outside Top</color>, offset:{viewportTop - targetTop}";
	}

	protected string CheckIsTargetOutsideBottom(float targetBottom, float viewportBottom)
	{
		if (!(targetBottom < viewportBottom))
		{
			return "<color=#00FF00>Target inside Bottom</color>";
		}
		return $"<color=#FF0000>Target outside Bottom</color>, offset:{viewportBottom - targetBottom}";
	}
}
