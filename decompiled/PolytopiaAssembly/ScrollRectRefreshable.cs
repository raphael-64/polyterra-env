using PullToRefresh;
using UnityEngine.EventSystems;

public class ScrollRectRefreshable : PolytopiaScrollRect, IScrollable
{
	private bool _Dragging;

	public bool Dragging => _Dragging;

	public override void OnBeginDrag(PointerEventData eventData)
	{
		base.OnBeginDrag(eventData);
		_Dragging = true;
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		base.OnEndDrag(eventData);
		_Dragging = false;
	}
}
