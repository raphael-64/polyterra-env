using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollRectCallbackHelper : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
	public delegate void ScrollRectEvent(PointerEventData eventData);

	public event ScrollRectEvent OnDragBegin;

	public event ScrollRectEvent OnDragging;

	public event ScrollRectEvent OnDragEnd;

	public event ScrollRectEvent OnScrolled;

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		this.OnDragBegin?.Invoke(eventData);
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		this.OnDragging?.Invoke(eventData);
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		this.OnDragEnd?.Invoke(eventData);
	}

	void IScrollHandler.OnScroll(PointerEventData eventData)
	{
		this.OnScrolled?.Invoke(eventData);
	}
}
