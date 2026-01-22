using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(ScrollRect))]
public class ScrollRectHighlightHelper : UIBasicComponent
{
	public float scrollTime = 0.2f;

	public bool log;

	private ScrollRect m_scrollRect;

	public RectOffset scrollInset;

	private Tween horizontalScrollTween;

	private Tween verticalScrollTween;

	private ScrollRect ScrollRect
	{
		get
		{
			if ((Object)(object)m_scrollRect == (Object)null)
			{
				m_scrollRect = ((Component)this).GetComponent<ScrollRect>();
			}
			return m_scrollRect;
		}
	}

	private void OnDisable()
	{
		TweenUtils.KillTween(horizontalScrollTween);
		TweenUtils.KillTween(verticalScrollTween);
	}

	public void ScrollToObject(RectTransform target)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0585: Unknown result type (might be due to invalid IL or missing references)
		//IL_058a: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this == (Object)null || (Object)(object)target == (Object)null || !((Component)target).gameObject.activeSelf)
		{
			Log.Warning("Trying to scroll to object that's not active", Array.Empty<object>());
			return;
		}
		Rect rect = target.rect;
		Rect worldRect = target.GetWorldRect(UIManager.Canvas.scaleFactor);
		Vector2 val = Vector2.op_Implicit(((Transform)base.rectTransform).InverseTransformPoint(Vector2.op_Implicit(((Rect)(ref worldRect)).center)));
		Rect rect2 = base.rectTransform.rect;
		Rect rect3;
		if (ScrollRect.vertical)
		{
			float num = val.y + ((Rect)(ref rect)).height * 0.5f;
			float num2 = val.y - ((Rect)(ref rect)).height * 0.5f;
			float num3 = ((Rect)(ref rect2)).center.y + ((Rect)(ref rect2)).height * 0.5f;
			float num4 = ((Rect)(ref rect2)).center.y - ((Rect)(ref rect2)).height * 0.5f;
			bool flag = false;
			float num5 = 0f;
			LogMessage("ScrollRectHighlightHelper :: ScrollToObject :: targetTop: {0}", num);
			LogMessage("ScrollRectHighlightHelper :: ScrollToObject :: targetBottom: {0}", num2);
			LogMessage("ScrollRectHighlightHelper :: ScrollToObject :: thisTop: {0}", num3);
			LogMessage("ScrollRectHighlightHelper :: ScrollToObject :: thisBottom: {0}", num4);
			if (((Rect)(ref rect2)).height - (float)scrollInset.top - (float)scrollInset.bottom < ((Rect)(ref rect)).height || num > num3 - (float)scrollInset.top)
			{
				flag = true;
				float num6 = 0f - ((Transform)ScrollRect.content).InverseTransformPoint(Vector2.op_Implicit(((Rect)(ref worldRect)).max)).y;
				rect3 = ScrollRect.content.rect;
				float num7 = num6 + ((Rect)(ref rect3)).max.y - (float)scrollInset.top;
				rect3 = ScrollRect.content.rect;
				num5 = 1f - Mathf.Min(Mathf.Max(num7 / Mathf.Max(1f, ((Rect)(ref rect3)).height - ((Rect)(ref rect2)).height), 0f), 1f);
				LogMessage("ScrollRectHighlightHelper :: rtTopInContent: {0}", num7);
			}
			else if (num2 < num4 + (float)scrollInset.bottom)
			{
				flag = true;
				float num8 = 0f - ((Transform)ScrollRect.content).InverseTransformPoint(Vector2.op_Implicit(((Rect)(ref worldRect)).min)).y;
				rect3 = ScrollRect.content.rect;
				float num9 = num8 + ((Rect)(ref rect3)).max.y - ((Rect)(ref rect2)).height + (float)scrollInset.bottom;
				rect3 = ScrollRect.content.rect;
				num5 = 1f - Mathf.Min(Mathf.Max(num9 / Mathf.Max(1f, ((Rect)(ref rect3)).height - ((Rect)(ref rect2)).height), 0f), 1f);
				LogMessage("ScrollRectHighlightHelper :: rtBottomInContent: {0}", num9);
			}
			if (flag)
			{
				LogMessage("ScrollRectHighlightHelper :: ScrollToObject :: verticalNormalizedPosition: {0}", num5);
				if (scrollTime > 0f)
				{
					verticalScrollTween = (Tween)(object)ScrollRect.DOVerticalNormalizedPos(num5, scrollTime);
				}
				else
				{
					ScrollRect.verticalNormalizedPosition = num5;
				}
			}
		}
		if (!ScrollRect.horizontal)
		{
			return;
		}
		float num10 = val.x + ((Rect)(ref rect)).width * 0.5f;
		float num11 = val.x - ((Rect)(ref rect)).width * 0.5f;
		float num12 = ((Rect)(ref rect2)).center.x + ((Rect)(ref rect2)).width * 0.5f;
		float num13 = ((Rect)(ref rect2)).center.x - ((Rect)(ref rect2)).width * 0.5f;
		bool flag2 = false;
		float num14 = 0f;
		LogMessage("ScrollRectHighlightHelper :: ScrollToObject :: targetRight: {0}", num10);
		LogMessage("ScrollRectHighlightHelper :: ScrollToObject :: targetLeft: {0}", num11);
		LogMessage("ScrollRectHighlightHelper :: ScrollToObject :: thisRight: {0}", num12);
		LogMessage("ScrollRectHighlightHelper :: ScrollToObject :: thisLeft: {0}", num13);
		if (((Rect)(ref rect2)).width - (float)scrollInset.right - (float)scrollInset.left < ((Rect)(ref rect)).width || num10 > num12 - (float)scrollInset.right)
		{
			flag2 = true;
			float num15 = 0f - ((Transform)ScrollRect.content).InverseTransformPoint(Vector2.op_Implicit(((Rect)(ref worldRect)).max)).x;
			rect3 = ScrollRect.content.rect;
			float num16 = num15 + ((Rect)(ref rect3)).max.x - (float)scrollInset.right;
			rect3 = ScrollRect.content.rect;
			num14 = 1f - Mathf.Min(Mathf.Max(num16 / (((Rect)(ref rect3)).width - ((Rect)(ref rect2)).width), 0f), 1f);
			LogMessage("ScrollRectHighlightHelper :: rtRightInContent: {0}", num16);
		}
		else if (num11 < num13 + (float)scrollInset.left)
		{
			flag2 = true;
			float num17 = 0f - ((Transform)ScrollRect.content).InverseTransformPoint(Vector2.op_Implicit(((Rect)(ref worldRect)).min)).x;
			rect3 = ScrollRect.content.rect;
			float num18 = num17 + ((Rect)(ref rect3)).max.x - ((Rect)(ref rect2)).width + (float)scrollInset.left;
			rect3 = ScrollRect.content.rect;
			num14 = 1f - Mathf.Min(Mathf.Max(num18 / (((Rect)(ref rect3)).width - ((Rect)(ref rect2)).width), 0f), 1f);
			LogMessage("ScrollRectHighlightHelper :: rtLeftInContent: {0}", num18);
		}
		if (flag2)
		{
			LogMessage("ScrollRectHighlightHelper :: ScrollToObject :: horizontalNormalizedPosition: {0}", num14);
			if (scrollTime > 0f)
			{
				horizontalScrollTween = (Tween)(object)ScrollRect.DOHorizontalNormalizedPos(num14, scrollTime);
			}
			else
			{
				ScrollRect.horizontalNormalizedPosition = num14;
			}
		}
	}

	public void OnScrollerChanged(Vector2 pos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		LogMessage("ScrollRectHighlightHelper :: pos: {0}", pos.y);
	}

	protected void LogMessage(string format, params object[] args)
	{
		if (log)
		{
			Log.Verbose(format, args);
		}
	}
}
