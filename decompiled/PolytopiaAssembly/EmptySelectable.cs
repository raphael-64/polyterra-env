using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EmptySelectable : Selectable, ISelectHandler, IEventSystemHandler
{
	public bool m_updateScroller;

	protected RectTransform m_rectTransform;

	protected ScrollRectHighlightHelper m_scrollRectHighlightHelper;

	public virtual bool UpdateScrollerOnHighlight
	{
		get
		{
			return m_updateScroller;
		}
		set
		{
			m_updateScroller = value;
			if (m_updateScroller)
			{
				m_scrollRectHighlightHelper = ((Component)this).GetComponentInParent<ScrollRectHighlightHelper>();
				m_updateScroller = (Object)(object)m_scrollRectHighlightHelper != (Object)null;
			}
			else
			{
				m_scrollRectHighlightHelper = null;
			}
		}
	}

	public RectTransform rectTransform
	{
		get
		{
			if ((Object)(object)m_rectTransform == (Object)null)
			{
				m_rectTransform = ((Component)this).GetComponent<RectTransform>();
			}
			return m_rectTransform;
		}
	}

	protected override void Awake()
	{
		UpdateScrollerOnHighlight = m_updateScroller;
	}

	public override void OnSelect(BaseEventData eventData)
	{
		((Selectable)this).OnSelect(eventData);
		if (UpdateScrollerOnHighlight)
		{
			m_scrollRectHighlightHelper.ScrollToObject(rectTransform);
		}
	}
}
