using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonBase : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	public enum ButtonStates
	{
		None,
		Over,
		Down
	}

	public delegate void ButtonAction(int id, BaseEventData eventData = null);

	[Serializable]
	public class ColorStates
	{
		public Color defaultColor = Color.white;

		public Color hoverColor = Color.white;

		public Color highlightedColor = Color.white;

		public Color highlightedHoverColor = Color.white;

		public Color disabledColor = Color.white;

		public void CopyFrom(ColorStates source)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			defaultColor = source.defaultColor;
			hoverColor = source.hoverColor;
			highlightedColor = source.highlightedColor;
			highlightedHoverColor = source.highlightedHoverColor;
			disabledColor = source.disabledColor;
		}

		public ColorStates()
		{
		}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)


		public ColorStates(ColorStates source)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			defaultColor = source.defaultColor;
			hoverColor = source.hoverColor;
			highlightedColor = source.highlightedColor;
			highlightedHoverColor = source.highlightedHoverColor;
			disabledColor = source.disabledColor;
		}
	}

	[HideInInspector]
	public ButtonStates buttonState;

	[Header("Button Base")]
	public RectTransform hoverObject;

	public RectTransform selectionObject;

	public bool debug;

	public int id;

	public bool m_updateScroller;

	protected RectTransform m_rectTransform;

	protected Button m_button;

	protected ScrollRectHighlightHelper m_scrollRectHighlightHelper;

	protected bool searchedForParentSelectableContainer;

	protected ISelectableContainer parentSelectableContainer;

	protected bool m_buttonEnabled = true;

	protected bool m_highlighted;

	protected bool m_animationsEnabled = true;

	protected bool m_canRegisterHover = true;

	protected bool m_hoverRegistered;

	protected bool m_blockClick;

	protected Vector2 selectionOrgOffsetMin;

	protected Vector2 selectionOrgOffsetMax;

	protected Tween pressTween;

	protected Tween hoverTween;

	protected List<Tween> selectionTweens = new List<Tween>();

	public virtual bool CanRegisterHover
	{
		get
		{
			if (ButtonEnabled)
			{
				return m_canRegisterHover;
			}
			return false;
		}
		set
		{
			m_canRegisterHover = value;
		}
	}

	public virtual bool ButtonEnabled
	{
		get
		{
			return m_buttonEnabled;
		}
		set
		{
			if ((Object)(object)button != (Object)null)
			{
				((Behaviour)button).enabled = value;
			}
			m_buttonEnabled = value;
		}
	}

	public virtual bool Highlighted
	{
		get
		{
			return m_highlighted;
		}
		set
		{
			m_highlighted = value;
			UpdateSelection();
		}
	}

	public virtual bool AnimationsEnabled
	{
		get
		{
			if (m_animationsEnabled)
			{
				return ButtonEnabled;
			}
			return false;
		}
		set
		{
			m_animationsEnabled = value;
			if (!m_animationsEnabled)
			{
				CancelTweens();
			}
		}
	}

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

	public Button button
	{
		get
		{
			if ((Object)(object)m_button == (Object)null && (Object)(object)this != (Object)null)
			{
				m_button = ((Component)this).GetComponent<Button>();
			}
			return m_button;
		}
	}

	public ISelectableContainer ParentSelectableContainer
	{
		get
		{
			if (parentSelectableContainer == null && !searchedForParentSelectableContainer)
			{
				parentSelectableContainer = ((Component)this).GetComponentInParent<ISelectableContainer>();
			}
			searchedForParentSelectableContainer = true;
			return parentSelectableContainer;
		}
	}

	public event ButtonAction OnDown;

	public event ButtonAction OnUp;

	public event ButtonAction OnClicked;

	public event ButtonAction OnClickedWhenDisabled;

	public event ButtonAction OnEnter;

	public event ButtonAction OnExit;

	public event ButtonAction OnSelected;

	public event ButtonAction OnDeselected;

	public virtual void Awake()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)selectionObject != (Object)null)
		{
			selectionOrgOffsetMin = selectionObject.offsetMin;
			selectionOrgOffsetMax = selectionObject.offsetMax;
		}
		UpdateScrollerOnHighlight = m_updateScroller;
	}

	public virtual void Start()
	{
	}

	protected virtual void OnEnable()
	{
		AnimationsEnabled = true;
		UINavigationManager.OnUINavigationTypeChanged += OnUINavigationTypeChanged;
		DebugLog("Enable");
	}

	protected virtual void OnDisable()
	{
		DebugLog("Disable");
		if (m_hoverRegistered)
		{
			SystemManager.DecreaseHoveredCounter();
			m_hoverRegistered = false;
		}
		Highlighted = false;
		AnimationsEnabled = false;
		UINavigationManager.OnUINavigationTypeChanged -= OnUINavigationTypeChanged;
		CancelTweens();
		ResetState();
	}

	protected virtual void OnDestroy()
	{
		DebugLog("Destroy");
		ButtonEnabled = false;
		AnimationsEnabled = false;
		UINavigationManager.OnUINavigationTypeChanged -= OnUINavigationTypeChanged;
		CancelTweens();
	}

	protected virtual void OnUINavigationTypeChanged(UINavigationManager.NavigationType navType)
	{
		UpdateSelection();
	}

	protected virtual void UpdateSelection()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)selectionObject != (Object)null))
		{
			return;
		}
		if (UINavigationManager.navigationType == UINavigationManager.NavigationType.Buttons)
		{
			CancelSelectionTweens();
			((Component)selectionObject).gameObject.SetActive(Highlighted);
			if (Highlighted)
			{
				selectionObject.offsetMin = Vector2.zero;
				selectionObject.offsetMax = Vector2.zero;
				selectionTweens.Add((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTween.To((DOGetter<Vector2>)(() => selectionObject.offsetMin), (DOSetter<Vector2>)delegate(Vector2 x)
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					selectionObject.offsetMin = x;
				}, selectionOrgOffsetMin, 0.2f), (Ease)27, 5f));
				selectionTweens.Add((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTween.To((DOGetter<Vector2>)(() => selectionObject.offsetMax), (DOSetter<Vector2>)delegate(Vector2 x)
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					selectionObject.offsetMax = x;
				}, selectionOrgOffsetMax, 0.2f), (Ease)27, 5f));
			}
		}
		else
		{
			CancelSelectionTweens();
			((Component)selectionObject).gameObject.SetActive(false);
		}
	}

	protected virtual void ResetState()
	{
		buttonState = ButtonStates.None;
		if (m_hoverRegistered)
		{
			m_hoverRegistered = false;
			SystemManager.DecreaseHoveredCounter();
		}
	}

	public void ResetEvents()
	{
		this.OnClicked = null;
	}

	public virtual void PointerDown(PointerEventData eventData)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (ButtonEnabled)
		{
			buttonState = ButtonStates.Down;
			this.OnDown?.Invoke(id, (BaseEventData)(object)eventData);
			OnPointerDownAnimation();
			AudioManager.PlaySFX(SFXTypes.Press, 1f, 1f, AudioManager.GetPanFromPosition(Vector2.op_Implicit(((Component)this).transform.position)));
			DebugLog("On Pointer Down");
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 0)
		{
			PointerDown(eventData);
		}
	}

	public virtual void PointerUp(PointerEventData eventData)
	{
		if (ButtonEnabled)
		{
			buttonState = ((this.OnClicked == null && button.onClick == null) ? ButtonStates.Over : ButtonStates.None);
			this.OnUp?.Invoke(id, (BaseEventData)(object)eventData);
			OnPointerUpAnimation();
			DebugLog("On Pointer Up");
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 0)
		{
			PointerUp(eventData);
		}
	}

	protected virtual void OnButtonClicked()
	{
		if (!ButtonEnabled)
		{
			this.OnClickedWhenDisabled?.Invoke(id);
			return;
		}
		this.OnClicked?.Invoke(id);
		DebugLog("On Button Click");
	}

	public virtual void PointerClick(PointerEventData eventData)
	{
		if (!m_blockClick)
		{
			if (!ButtonEnabled)
			{
				this.OnClickedWhenDisabled?.Invoke(id, (BaseEventData)(object)eventData);
				return;
			}
			this.OnClicked?.Invoke(id, (BaseEventData)(object)eventData);
			DebugLog("On Pointer Click");
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 0)
		{
			PointerClick(eventData);
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (ButtonEnabled)
		{
			buttonState = ButtonStates.Over;
			this.OnEnter?.Invoke(id, (BaseEventData)(object)eventData);
			if (CanRegisterHover && !m_hoverRegistered)
			{
				m_hoverRegistered = true;
				SystemManager.IncreaseHoveredCounter();
			}
			CancelTweens();
			if ((Object)(object)hoverObject != (Object)null && AnimationsEnabled)
			{
				pressTween = (Tween)(object)ShortcutExtensions.DOScale((Transform)(object)hoverObject, 1.05f, 0.2f);
			}
			DebugLog("On Pointer Enter");
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		if (ButtonEnabled)
		{
			buttonState = ButtonStates.None;
			this.OnExit?.Invoke(id, (BaseEventData)(object)eventData);
			if (m_hoverRegistered)
			{
				m_hoverRegistered = false;
				SystemManager.DecreaseHoveredCounter();
			}
			CancelTweens();
			if ((Object)(object)hoverObject != (Object)null && AnimationsEnabled)
			{
				pressTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)hoverObject, 1f, 0.3f), (Ease)27, 5f);
			}
			DebugLog("On Pointer Exit");
		}
	}

	public virtual void OnSubmit(BaseEventData eventData)
	{
		if (!ButtonEnabled)
		{
			this.OnClickedWhenDisabled?.Invoke(id, eventData);
			return;
		}
		this.OnClicked?.Invoke(id, eventData);
		DebugLog("On Submit");
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		if (ButtonEnabled)
		{
			Highlighted = true;
			if (eventData is AxisEventData)
			{
				ScrollToFocusObject();
			}
			if (ParentSelectableContainer != null)
			{
				ParentSelectableContainer.CurrentSelectable = (Selectable)(object)button;
			}
			UpdateSelection();
			this.OnSelected?.Invoke(id, eventData);
			DebugLog("On Select");
		}
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		if (ButtonEnabled)
		{
			Highlighted = false;
			this.OnDeselected?.Invoke(id, eventData);
			UpdateSelection();
			DebugLog("On Deselect");
		}
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		ExecuteEvents.ExecuteHierarchy<IInitializePotentialDragHandler>(((Component)((Component)this).transform.parent).gameObject, (BaseEventData)(object)eventData, ExecuteEvents.initializePotentialDrag);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		m_blockClick = true;
		ExecuteEvents.ExecuteHierarchy<IBeginDragHandler>(((Component)((Component)this).transform.parent).gameObject, (BaseEventData)(object)eventData, ExecuteEvents.beginDragHandler);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		m_blockClick = false;
		ExecuteEvents.ExecuteHierarchy<IEndDragHandler>(((Component)((Component)this).transform.parent).gameObject, (BaseEventData)(object)eventData, ExecuteEvents.endDragHandler);
	}

	public void OnDrag(PointerEventData eventData)
	{
		ExecuteEvents.ExecuteHierarchy<IDragHandler>(((Component)((Component)this).transform.parent).gameObject, (BaseEventData)(object)eventData, ExecuteEvents.dragHandler);
	}

	public void OnPointerEnterAnimation()
	{
		CancelTweens();
		if ((Object)(object)hoverObject != (Object)null && AnimationsEnabled)
		{
			hoverTween = (Tween)(object)ShortcutExtensions.DOScale((Transform)(object)hoverObject, 1.1f, 0.2f);
		}
	}

	public void OnPointerExitAnimation()
	{
		CancelTweens();
		if ((Object)(object)hoverObject != (Object)null && AnimationsEnabled)
		{
			hoverTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)hoverObject, 1f, 0.3f), (Ease)27, 5f);
		}
	}

	public void OnPointerDownAnimation()
	{
		CancelTweens();
		if ((Object)(object)hoverObject != (Object)null && AnimationsEnabled)
		{
			pressTween = (Tween)(object)ShortcutExtensions.DOScale((Transform)(object)hoverObject, 1.1f, 0.2f);
		}
	}

	public void OnPointerUpAnimation()
	{
		CancelTweens();
		if ((Object)(object)hoverObject != (Object)null && AnimationsEnabled)
		{
			pressTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)hoverObject, 1f, 0.3f), (Ease)27, 5f);
		}
	}

	public virtual void CancelTweens()
	{
		DebugLog("Cancel Tween");
		DOTween.Kill((object)hoverObject, true);
		DOTween.Kill((object)selectionObject, true);
		pressTween = null;
		hoverTween = null;
		CancelSelectionTweens();
	}

	public virtual void CancelSelectionTweens()
	{
		TweenUtils.KillTweens(selectionTweens, complete: true);
		selectionTweens.Clear();
	}

	public virtual void ScrollToFocusObject()
	{
		if (m_highlighted && m_updateScroller && (Object)(object)m_scrollRectHighlightHelper != (Object)null)
		{
			m_scrollRectHighlightHelper.ScrollToObject(rectTransform);
		}
	}

	protected virtual void DebugLog(string message)
	{
		if (debug)
		{
			Log.Verbose("{0} :: {1}", new object[2]
			{
				((Object)((Component)this).gameObject).name,
				message
			});
		}
	}
}
