using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("UI/Polytopia Scroll Rect", 37)]
public class PolytopiaScrollRect : ScrollRect, IPointerClickHandler, IEventSystemHandler
{
	public bool routeToParent;

	public bool globalGamepadScroll;

	public bool allowGamepadScroll = true;

	public RectTransform gamepadScrollGroupParent;

	private bool wasScrolling;

	private bool gamepadScroll;

	private const int SCROLL_GRACE_FRAMES = 2;

	private int lastScrollFrame = -2;

	private Vector2 accumulatedScrollDelta;

	private Vector2 positionOffset;

	public Action onClicked;

	public Action onDragStarted;

	public Action onDragEnded;

	protected override void Awake()
	{
		((UIBehaviour)this).Awake();
	}

	private bool IsProbablyTouchScroll(Vector2 scrollDelta)
	{
		return false;
	}

	public override void OnScroll(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Vector2 scrollDelta = eventData.scrollDelta;
		if (!NativeHelpers.IsTrackpadInMomentumPhase())
		{
			if (!wasScrolling && IsProbablyTouchScroll(scrollDelta))
			{
				((ScrollRect)this).inertia = true;
				PointerEventData val = new PointerEventData(EventSystem.current);
				val.button = (InputButton)0;
				accumulatedScrollDelta = Vector2.zero;
				val.position = accumulatedScrollDelta;
				((ScrollRect)this).OnInitializePotentialDrag(val);
				((ScrollRect)this).OnBeginDrag(val);
				wasScrolling = true;
			}
			else if (!wasScrolling || gamepadScroll)
			{
				((ScrollRect)this).OnScroll(eventData);
			}
		}
	}

	protected void Update()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		if (!((UIBehaviour)this).IsActive())
		{
			return;
		}
		Vector2 val = Vector2.zero;
		RectTransform transform = (RectTransform)(Object.op_Implicit((Object)(object)gamepadScrollGroupParent) ? ((object)gamepadScrollGroupParent) : ((object)(RectTransform)((Component)this).transform));
		_ = gamepadScroll;
		if (allowGamepadScroll && PolytopiaInput.isTrackingOmnicursor && (globalGamepadScroll || PolytopiaInput.Omnicursor.IsCursorAffixedToChildOf(transform)))
		{
			val = InputManager.gamepadInputManager.GetSecondaryStick();
			gamepadScroll = true;
			((ScrollRect)this).movementType = (MovementType)2;
			if (!((ScrollRect)this).horizontal)
			{
				val.x = 0f;
			}
			if (!((ScrollRect)this).vertical)
			{
				val.y = 0f;
			}
			if (((Vector2)(ref val)).sqrMagnitude > 1E-07f)
			{
				((ScrollRect)this).inertia = false;
			}
		}
		else
		{
			((ScrollRect)this).movementType = (MovementType)1;
		}
		if (((Vector2)(ref val)).sqrMagnitude < 0.0001f)
		{
			val = Input.mouseScrollDelta;
			gamepadScroll = false;
			if (!((ScrollRect)this).horizontal)
			{
				val.x = 0f;
			}
			if (!((ScrollRect)this).vertical)
			{
				val.y = 0f;
			}
			if (((Vector2)(ref val)).sqrMagnitude > 1E-07f)
			{
				((ScrollRect)this).inertia = true;
			}
		}
		if (gamepadScroll && !wasScrolling)
		{
			PointerEventData val2 = new PointerEventData(EventSystem.current);
			val2.button = (InputButton)0;
			accumulatedScrollDelta = Vector2.zero;
			val2.position = accumulatedScrollDelta;
			((ScrollRect)this).OnInitializePotentialDrag(val2);
			((ScrollRect)this).OnBeginDrag(val2);
			wasScrolling = true;
		}
		bool flag = NativeHelpers.IsTrackpadInMomentumPhase() && Vector2.Dot(CalculateNormalizedOffset(), new Vector2(0f - val.x, val.y)) > 0f;
		bool flag2 = (((Vector2)(ref val)).sqrMagnitude > 1E-07f || NativeHelpers.IsTrackpadInNormalPhase()) && !flag;
		if (flag2)
		{
			lastScrollFrame = Time.frameCount;
		}
		else if (Time.frameCount - lastScrollFrame < 2)
		{
			flag2 = true;
		}
		ScrollAsDrag(flag2, val);
	}

	private void ScrollAsDrag(bool isScrolling, Vector2 scrollDelta)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (wasScrolling)
		{
			PointerEventData val = new PointerEventData(EventSystem.current);
			val.button = (InputButton)0;
			if (!isScrolling)
			{
				((ScrollRect)this).OnEndDrag(val);
				wasScrolling = false;
			}
			if (isScrolling)
			{
				accumulatedScrollDelta += new Vector2(scrollDelta.x, 0f - scrollDelta.y) * ((ScrollRect)this).scrollSensitivity;
				val.position = accumulatedScrollDelta;
				((ScrollRect)this).OnDrag(val);
			}
		}
	}

	private Vector2 CalculateNormalizedOffset()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		float num = ((((ScrollRect)this).horizontal && (((ScrollRect)this).horizontalNormalizedPosition < 0f || ((ScrollRect)this).horizontalNormalizedPosition > 1f)) ? ((ScrollRect)this).horizontalNormalizedPosition : 0f);
		float num2 = ((((ScrollRect)this).vertical && (((ScrollRect)this).verticalNormalizedPosition < 0f || ((ScrollRect)this).verticalNormalizedPosition > 1f)) ? ((ScrollRect)this).verticalNormalizedPosition : 0f);
		return new Vector2(num, num2);
	}

	public override void OnInitializePotentialDrag(PointerEventData eventData)
	{
		if (routeToParent)
		{
			ExecuteEvents.ExecuteHierarchy<IInitializePotentialDragHandler>(((Component)((Component)this).transform.parent).gameObject, (BaseEventData)(object)eventData, ExecuteEvents.initializePotentialDrag);
		}
		((ScrollRect)this).OnInitializePotentialDrag(eventData);
	}

	public override void OnDrag(PointerEventData eventData)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		eventData.position = GetInputPosition(eventData);
		if (routeToParent)
		{
			ExecuteEvents.ExecuteHierarchy<IDragHandler>(((Component)((Component)this).transform.parent).gameObject, (BaseEventData)(object)eventData, ExecuteEvents.dragHandler);
		}
		((ScrollRect)this).OnDrag(eventData);
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		positionOffset = Vector2.zero;
		eventData.position = GetInputPosition(eventData);
		if (routeToParent)
		{
			ExecuteEvents.ExecuteHierarchy<IBeginDragHandler>(((Component)((Component)this).transform.parent).gameObject, (BaseEventData)(object)eventData, ExecuteEvents.beginDragHandler);
		}
		((ScrollRect)this).OnBeginDrag(eventData);
		onDragStarted?.Invoke();
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		eventData.position = GetInputPosition(eventData);
		if (routeToParent)
		{
			ExecuteEvents.ExecuteHierarchy<IEndDragHandler>(((Component)((Component)this).transform.parent).gameObject, (BaseEventData)(object)eventData, ExecuteEvents.endDragHandler);
		}
		((ScrollRect)this).OnEndDrag(eventData);
		positionOffset = Vector2.zero;
		onDragEnded?.Invoke();
	}

	public Vector2 GetInputPosition(PointerEventData eventData)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (Input.touchCount <= 1)
		{
			return eventData.position + positionOffset;
		}
		return InputManager.GetInputPosition() + positionOffset;
	}

	public void OffsetContent(Vector2 offset)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		RectTransform content = ((ScrollRect)this).content;
		content.anchoredPosition += offset;
		positionOffset += offset * UICanvasScalerHelper.GetUIScale();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		onClicked?.Invoke();
	}
}
