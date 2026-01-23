using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Layout/Content Size Fitter", 141)]
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class PolyUIContentSizefitter : UIBehaviour, ILayoutSelfController, ILayoutController
{
	public enum FitMode
	{
		Unconstrained,
		MinSize,
		PreferredSize
	}

	[SerializeField]
	protected FitMode m_HorizontalFit;

	[ShowIf("HorizontalFitActive")]
	public float maxHorizontalSize = -1f;

	[SerializeField]
	protected FitMode m_VerticalFit;

	[ShowIf("VerticalFitActive")]
	public float maxVerticalSize = -1f;

	[NonSerialized]
	private RectTransform m_Rect;

	private DrivenRectTransformTracker m_Tracker;

	public FitMode horizontalFit
	{
		get
		{
			return m_HorizontalFit;
		}
		set
		{
			if (PolyUISetPropertyUtility.SetStruct(ref m_HorizontalFit, value))
			{
				SetDirty();
			}
		}
	}

	public FitMode verticalFit
	{
		get
		{
			return m_VerticalFit;
		}
		set
		{
			if (PolyUISetPropertyUtility.SetStruct(ref m_VerticalFit, value))
			{
				SetDirty();
			}
		}
	}

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

	private bool HorizontalFitActive()
	{
		return horizontalFit != FitMode.Unconstrained;
	}

	private bool VerticalFitActive()
	{
		return verticalFit != FitMode.Unconstrained;
	}

	protected PolyUIContentSizefitter()
	{
	}

	protected override void OnEnable()
	{
		((UIBehaviour)this).OnEnable();
		SetDirty();
	}

	protected override void OnDisable()
	{
		((DrivenRectTransformTracker)(ref m_Tracker)).Clear();
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		((UIBehaviour)this).OnDisable();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		SetDirty();
	}

	private void HandleSelfFittingAlongAxis(int axis)
	{
		FitMode fitMode = ((axis == 0) ? horizontalFit : verticalFit);
		if (fitMode == FitMode.Unconstrained)
		{
			((DrivenRectTransformTracker)(ref m_Tracker)).Add((Object)(object)this, rectTransform, (DrivenTransformProperties)0);
			return;
		}
		((DrivenRectTransformTracker)(ref m_Tracker)).Add((Object)(object)this, rectTransform, (DrivenTransformProperties)((axis == 0) ? 4096 : 8192));
		float num = ((axis == 0) ? maxHorizontalSize : maxVerticalSize);
		if (fitMode == FitMode.MinSize)
		{
			rectTransform.SetSizeWithCurrentAnchors((Axis)axis, (num < 0f) ? LayoutUtility.GetMinSize(m_Rect, axis) : Mathf.Min(LayoutUtility.GetMinSize(m_Rect, axis), num));
		}
		else
		{
			rectTransform.SetSizeWithCurrentAnchors((Axis)axis, (num < 0f) ? LayoutUtility.GetPreferredSize(m_Rect, axis) : Mathf.Min(LayoutUtility.GetPreferredSize(m_Rect, axis), num));
		}
	}

	public virtual void SetLayoutHorizontal()
	{
		((DrivenRectTransformTracker)(ref m_Tracker)).Clear();
		HandleSelfFittingAlongAxis(0);
	}

	public virtual void SetLayoutVertical()
	{
		HandleSelfFittingAlongAxis(1);
	}

	protected void SetDirty()
	{
		if (((UIBehaviour)this).IsActive())
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}
}
