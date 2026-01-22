using System;

namespace UnityEngine.UI;

[AddComponentMenu("Layout/Content Size Fitter+ (Polytopia)", 141)]
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class PolytopiaContentSizeFitter : ContentSizeFitter
{
	[NonSerialized]
	private RectTransform m_Rect;

	[SerializeField]
	private float m_MaxWidth = -1f;

	[SerializeField]
	private float m_MaxHeight = -1f;

	private RectTransform rectTransform
	{
		get
		{
			if ((Object)(object)m_Rect == (Object)null)
			{
				m_Rect = ((Component)this).GetComponent<RectTransform>();
			}
			return m_Rect;
		}
	}

	public float maxWidth
	{
		get
		{
			return m_MaxWidth;
		}
		set
		{
			m_MaxWidth = value;
		}
	}

	public float maxHeight
	{
		get
		{
			return m_MaxHeight;
		}
		set
		{
			m_MaxHeight = value;
		}
	}

	public override void SetLayoutHorizontal()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		((ContentSizeFitter)this).SetLayoutHorizontal();
		if (maxWidth > 0f)
		{
			if ((int)((ContentSizeFitter)this).horizontalFit == 1)
			{
				rectTransform.SetSizeWithCurrentAnchors((Axis)0, Mathf.Min(LayoutUtility.GetMinSize(m_Rect, 0), maxWidth));
			}
			else if ((int)((ContentSizeFitter)this).horizontalFit == 2)
			{
				rectTransform.SetSizeWithCurrentAnchors((Axis)0, Mathf.Min(LayoutUtility.GetPreferredSize(m_Rect, 0), maxWidth));
			}
		}
	}

	public override void SetLayoutVertical()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		((ContentSizeFitter)this).SetLayoutVertical();
		if (maxHeight > 0f)
		{
			if ((int)((ContentSizeFitter)this).verticalFit == 1)
			{
				rectTransform.SetSizeWithCurrentAnchors((Axis)1, Mathf.Min(LayoutUtility.GetMinSize(m_Rect, 1), maxHeight));
			}
			else if ((int)((ContentSizeFitter)this).verticalFit == 2)
			{
				rectTransform.SetSizeWithCurrentAnchors((Axis)1, Mathf.Min(LayoutUtility.GetPreferredSize(m_Rect, 1), maxHeight));
			}
		}
	}
}
