using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupBase : UIBasicComponent, ISelectableContainer
{
	public class PopupButtonData
	{
		public enum States
		{
			None,
			Selected,
			Disabled,
			Alternative
		}

		public int id;

		public string text;

		public UIButtonBase.ButtonAction callback;

		public States state;

		public bool closesPopup = true;

		public Action stateCheck;

		public UIButtonBase.ColorStates customColorStates;

		public PopupButtonData()
		{
		}

		public PopupButtonData(string text, States state = States.None, UIButtonBase.ButtonAction callback = null, int id = -1, bool closesPopup = true, UIButtonBase.ColorStates customColorStates = null)
		{
			this.text = text;
			this.callback = callback;
			this.id = id;
			this.state = state;
			this.closesPopup = closesPopup;
			this.customColorStates = customColorStates;
		}
	}

	public Action HideCallback;

	[Header("Popup Base")]
	[SerializeField]
	protected TextMeshProUGUI header;

	[SerializeField]
	protected TextMeshProUGUI description;

	public bool fullscreenVariant;

	[Header("Scrolling")]
	[SerializeField]
	protected bool scrollingEnabled;

	[ShowIf("scrollingEnabled")]
	[SerializeField]
	protected RectTransform content;

	[ShowIf("scrollingEnabled")]
	[SerializeField]
	protected ScrollRect scrollRect;

	[ShowIf("scrollingEnabled")]
	[Tooltip("These items will only be shown if the popup needs scrolling")]
	[SerializeField]
	protected GameObject[] scrollingItems;

	[NonSerialized]
	public PopupManager popupManager;

	[NonSerialized]
	public string popupId;

	[NonSerialized]
	public string identifier;

	[NonSerialized]
	public bool forceScrollerBg;

	[NonSerialized]
	public Vector2 appearTransitionOrigin;

	protected bool autoCapitalizeHeader = true;

	protected UnityAction<string, string> descriptionLinkCallback;

	protected RectTransform m_scrollRectTransform;

	protected Selectable currentSelectable;

	[SerializeField]
	protected Selectable defaultSelectable;

	protected bool hasNullFallbackSelectable;

	protected Selectable fallbackSelectable;

	protected Tween scaleTween;

	public PopupScroller scroller { get; protected set; }

	public bool IsUnskippable { get; set; }

	public virtual string Header
	{
		get
		{
			return ((TMP_Text)header).text;
		}
		set
		{
			if (AutoCapitalizeHeader)
			{
				((TMP_Text)header).text = LocalizationUtils.CapitalizeString(value);
			}
			else
			{
				((TMP_Text)header).text = value;
			}
		}
	}

	public virtual string Description
	{
		get
		{
			if ((Object)(object)description == (Object)null)
			{
				return string.Empty;
			}
			return ((TMP_Text)description).text;
		}
		set
		{
			if (!((Object)(object)description == (Object)null))
			{
				((TMP_Text)description).text = value;
			}
		}
	}

	public virtual bool AutoCapitalizeHeader
	{
		get
		{
			return autoCapitalizeHeader;
		}
		set
		{
			autoCapitalizeHeader = value;
		}
	}

	public virtual UnityAction<string, string> DescriptionLinkCallback
	{
		get
		{
			return descriptionLinkCallback;
		}
		set
		{
			descriptionLinkCallback = value;
			TMPLinkHelper tMPLinkHelper2 = default(TMPLinkHelper);
			if (descriptionLinkCallback != null)
			{
				TMPLinkHelper tMPLinkHelper = ((Component)description).GetComponent<TMPLinkHelper>();
				if ((Object)(object)tMPLinkHelper == (Object)null)
				{
					tMPLinkHelper = ((Component)description).gameObject.AddComponent<TMPLinkHelper>();
					if (tMPLinkHelper.OnClick == null)
					{
						tMPLinkHelper.OnClick = new TMPLinkHelper.TMPLinkEvent();
					}
				}
				((UnityEvent<string, string>)tMPLinkHelper.OnClick).AddListener(descriptionLinkCallback);
			}
			else if ((Object)(object)description != (Object)null && ((Component)description).TryGetComponent<TMPLinkHelper>(ref tMPLinkHelper2))
			{
				Object.Destroy((Object)(object)tMPLinkHelper2);
			}
		}
	}

	public virtual Selectable CurrentSelectable
	{
		get
		{
			return currentSelectable;
		}
		set
		{
			currentSelectable = value;
		}
	}

	public virtual Selectable DefaultSelectable
	{
		get
		{
			return defaultSelectable;
		}
		set
		{
			defaultSelectable = value;
		}
	}

	private RectTransform ScrollRectTransform
	{
		get
		{
			if ((Object)(object)m_scrollRectTransform == (Object)null)
			{
				m_scrollRectTransform = ((Component)scrollRect).GetComponent<RectTransform>();
			}
			return m_scrollRectTransform;
		}
	}

	public override void Init()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public bool IsShowing()
	{
		if ((Object)(object)((Component)this).gameObject != (Object)null)
		{
			return ((Component)this).gameObject.activeSelf;
		}
		return false;
	}

	public virtual void Show(float delay)
	{
		((MonoBehaviour)this).Invoke("Show", delay);
	}

	public virtual void Show()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = NativeHelpers.Screen();
		float num = (float)((Vector2Int)(ref val)).x * 0.5f;
		val = NativeHelpers.Screen();
		Show(new Vector2(num, (float)((Vector2Int)(ref val)).y * 0.5f));
	}

	public virtual void Show(Vector2 origin)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)this == (Object)null))
		{
			UINavigationManager.Select(null);
			if ((Object)(object)popupManager != (Object)null)
			{
				popupManager.AddPopupToStack(this);
			}
			appearTransitionOrigin = origin;
			AudioManager.PlaySFX(SFXTypes.Popup);
			((Component)this).gameObject.SetActive(true);
			if (fullscreenVariant)
			{
				((Transform)base.rectTransform).localScale = Vector3.one;
				OnShowComplete();
			}
			else
			{
				float num = 0.3f;
				scroller.content.AnimateFrom(Vector2.op_Implicit(((Transform)base.rectTransform).InverseTransformPoint(Vector2.op_Implicit(origin))), num);
				((Transform)base.rectTransform).localScale = Vector3.zero;
				scaleTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)base.rectTransform, 1f, num), (Ease)27, 2f), new TweenCallback(OnShowComplete));
			}
			if (scrollingEnabled)
			{
				((MonoBehaviour)this).StartCoroutine(RefreshHeight());
			}
		}
	}

	protected virtual void OnShowComplete()
	{
		UINavigationManager.Select(GetCurrentSelectableOrFallback());
	}

	protected virtual void OnDisable()
	{
		TweenUtils.KillTween(scaleTween, complete: true);
		scaleTween = null;
	}

	public virtual void Hide()
	{
		if (IsShowing())
		{
			((MonoBehaviour)this).CancelInvoke("Show");
			if ((Object)(object)((Component)this).gameObject != (Object)null)
			{
				((Component)this).gameObject.SetActive(false);
			}
			if ((Object)(object)popupManager != (Object)null)
			{
				popupManager.RemoveLastPopupFromStack(this);
			}
			HideCallback?.Invoke();
			if (Object.op_Implicit((Object)(object)InfoButtonManager.Instance))
			{
				InfoButtonManager.Instance.DestroyInfoButtons(this);
			}
		}
	}

	public virtual void SetParent(PopupScroller scroller)
	{
		this.scroller = scroller;
		this.scroller.bgEnabled = (UIManager.Instance.CurrentScreen != UIConstants.Screens.Hud && UIManager.Instance.CurrentScreen != UIConstants.Screens.TechTree) || forceScrollerBg;
		((Transform)base.rectTransform).SetParent((Transform)(object)scroller.content.rectTransform);
	}

	public virtual void OnHide(int id, BaseEventData eventData)
	{
		Hide();
	}

	public virtual void ReturnFocus()
	{
	}

	public virtual void ResetPopup()
	{
		if (scrollingEnabled)
		{
			scrollRect.verticalNormalizedPosition = 1f;
		}
		DescriptionLinkCallback = null;
		Description = string.Empty;
		CurrentSelectable = null;
		forceScrollerBg = false;
	}

	protected IEnumerator RefreshHeight(Action OnComplete = null)
	{
		yield return (object)new WaitForEndOfFrame();
		bool flag = content.GetHeight() > (ScreenManager.SafeHeight - 20f) * UICanvasScalerHelper.GetInvertedUIScale();
		if (!fullscreenVariant)
		{
			float height = Mathf.Min(content.GetHeight(), (ScreenManager.SafeHeight - 20f) * UICanvasScalerHelper.GetInvertedUIScale());
			base.rectTransform.SetHeight(height);
			base.rectTransform.SetWidth(flag ? 422 : 402);
		}
		content.offsetMax = new Vector2((float)(flag ? (-20) : 0), 0f);
		scrollRect.vertical = flag;
		if ((Object)(object)scrollRect.verticalScrollbar != (Object)null)
		{
			((Component)scrollRect.verticalScrollbar).gameObject.SetActive(flag);
		}
		((Behaviour)scrollRect).enabled = flag;
		content.SetAnchoredY(0f);
		if (scrollingItems != null && scrollingItems.Length != 0)
		{
			GameObject[] array = scrollingItems;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(flag);
			}
		}
		OnComplete?.Invoke();
	}

	public virtual void OnAvailableAreaChanged()
	{
	}

	public void SetDescriptionFontSize(float size)
	{
		((TMP_Text)description).fontSize = size;
	}

	public void SetTribeInfoButtons(TextType textType)
	{
		TextMeshProUGUI val = ((textType == TextType.Header) ? header : description);
		if (((TMP_Text)val).textInfo != null)
		{
			((TMP_Text)val).textInfo.Clear();
		}
		TribeInfoButtonData tribeInfoButtonData = new TribeInfoButtonData
		{
			adjustPosition = (textType == TextType.Header)
		};
		InfoButtonManager.Instance.SetTribeInfoButtons(this, val, tribeInfoButtonData);
	}

	protected virtual Selectable GetStandardDefault()
	{
		return null;
	}

	public virtual Selectable GetCurrentSelectableOrFallback()
	{
		if (!UINavigationManager.IsValidSelectable(CurrentSelectable))
		{
			return GetDefaultSelectableOrFallback();
		}
		return CurrentSelectable;
	}

	public virtual Selectable GetDefaultSelectableOrFallback()
	{
		if (UINavigationManager.IsValidSelectable(DefaultSelectable))
		{
			return DefaultSelectable;
		}
		Selectable standardDefault = GetStandardDefault();
		if (UINavigationManager.IsValidSelectable(standardDefault))
		{
			return standardDefault;
		}
		if (!hasNullFallbackSelectable && !UINavigationManager.IsValidSelectable(fallbackSelectable))
		{
			fallbackSelectable = UINavigationManager.FindFirstNavigableSelectable(((Component)this).transform);
			hasNullFallbackSelectable = !UINavigationManager.IsValidSelectable(fallbackSelectable);
		}
		return fallbackSelectable;
	}
}
