using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRoundButton : UIButtonBase
{
	[Header("Round Button")]
	[SerializeField]
	protected Image bg;

	[SerializeField]
	protected Image shine;

	[SerializeField]
	protected Image outline;

	[SerializeField]
	protected RectTransform iconContainer;

	[SerializeField]
	protected RectTransform textBg;

	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected TMPLocalizer label;

	[SerializeField]
	protected CanvasGroup cnvsGrp;

	[SerializeField]
	protected ResourceWidget resourceWidget;

	[SerializeField]
	protected bool tintIcon;

	[Space(10f)]
	[SerializeField]
	protected RectTransform iconAndTextContainer;

	[SerializeField]
	protected Image smallIcon;

	[SerializeField]
	protected TextMeshProUGUI textInContainer;

	[SerializeField]
	private float faceIconSizeMultiplier = 3f;

	[Header("Colors")]
	public RoundButtonColorStates bgColors;

	public RoundButtonColorStates outlineColors;

	protected bool m_buttonActive;

	protected bool m_buttonExpensive;

	protected float m_iconMaxSize = -1f;

	protected RectTransform m_iconContent;

	protected float m_cost = -1f;

	protected bool m_showLabelBackground;

	protected bool m_labelShowOnHover;

	protected bool m_showLabel = true;

	[NonSerialized]
	public SpriteHandle iconSpriteHandle = new SpriteHandle();

	public Sprite sprite
	{
		get
		{
			return icon.sprite;
		}
		set
		{
			SetSprite(value);
		}
	}

	public RectTransform iconContent
	{
		get
		{
			return m_iconContent;
		}
		set
		{
			if ((Object)(object)value != (Object)null)
			{
				m_iconContent = value;
				((Transform)value).SetParent((Transform)(object)iconContainer, false);
				UIUtils.FitImageContentInParent(value);
				((Component)icon).gameObject.SetActive(false);
			}
			else if ((Object)(object)m_iconContent != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)m_iconContent).gameObject);
				m_iconContent = null;
			}
		}
	}

	public string text
	{
		get
		{
			return label.Text;
		}
		set
		{
			label.Text = value;
			if (ShowTextBackground)
			{
				UpdateTextBgSize();
			}
		}
	}

	public string Key
	{
		get
		{
			return label.Key;
		}
		set
		{
			label.Key = value;
			if (ShowTextBackground)
			{
				UpdateTextBgSize();
			}
		}
	}

	public bool ShowTextBackground
	{
		get
		{
			return m_showLabelBackground;
		}
		set
		{
			m_showLabelBackground = value;
			label.rectTransform.SetAnchoredY(m_showLabelBackground ? (-10f) : (-3.5f));
			UpdateTextBgSize();
			UpdateLabelVisibility();
		}
	}

	public bool shineActive
	{
		get
		{
			if ((Object)(object)shine == (Object)null)
			{
				return false;
			}
			return ((Component)shine).gameObject.activeSelf;
		}
		set
		{
			if (!((Object)(object)shine == (Object)null))
			{
				((Component)shine).gameObject.SetActive(value);
			}
		}
	}

	public bool ShowLabel
	{
		get
		{
			return m_showLabel;
		}
		set
		{
			m_showLabel = value;
			((Component)label).gameObject.SetActive(m_showLabel);
		}
	}

	public bool LabelShowOnHover
	{
		get
		{
			return m_labelShowOnHover;
		}
		set
		{
			m_labelShowOnHover = value;
			UpdateTextBgSize();
			UpdateLabelVisibility();
		}
	}

	public TMPLocalizer Label => label;

	public bool buttonActive
	{
		get
		{
			return m_buttonActive;
		}
		set
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			m_buttonActive = value;
			shineActive = m_buttonActive;
			((Graphic)outline).color = outlineColors.GetColorForState(m_buttonActive, Highlighted);
			((Graphic)bg).color = bgColors.GetColorForState(m_buttonActive, Highlighted);
			Highlighted = Highlighted;
		}
	}

	public bool BlockButton
	{
		get
		{
			return m_buttonActive;
		}
		set
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			m_buttonActive = !value;
			shineActive = m_buttonActive;
			m_buttonEnabled = m_buttonActive;
			((Component)icon).gameObject.SetActive(m_buttonActive);
			((Component)label.TextComponent).gameObject.SetActive(m_buttonActive);
			((Graphic)bg).color = bgColors.GetColorForState(m_buttonActive, Highlighted);
			Highlighted = Highlighted;
		}
	}

	public bool buttonExpensive
	{
		get
		{
			return m_buttonExpensive;
		}
		set
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			m_buttonExpensive = value;
			if (value)
			{
				outlineColors.deactiveColor = ColorConstants.red;
				bgColors.deactiveColor = ColorConstants.blue;
			}
			else
			{
				outlineColors.deactiveColor = ColorConstants.gray;
				bgColors.deactiveColor = Color.black;
			}
			((Graphic)outline).color = outlineColors.GetColorForState(m_buttonActive, Highlighted);
			((Graphic)bg).color = bgColors.GetColorForState(m_buttonActive, Highlighted);
			Highlighted = Highlighted;
		}
	}

	public float iconMaxSize
	{
		get
		{
			return m_iconMaxSize;
		}
		set
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			m_iconMaxSize = value;
			if (m_iconMaxSize > -1f)
			{
				iconContainer.sizeDelta = Vector2.one * m_iconMaxSize;
			}
		}
	}

	public float Cost
	{
		get
		{
			return m_cost;
		}
		set
		{
			m_cost = value;
			((Component)resourceWidget).gameObject.SetActive(m_cost >= 0f);
			resourceWidget.Amount = m_cost;
		}
	}

	public Image BG => bg;

	public Image Outline => outline;

	public CanvasGroup CanvasGroup => cnvsGrp;

	public override void Awake()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			sprite = spriteHandle.sprite;
		});
		m_iconMaxSize = Mathf.Min(iconContainer.sizeDelta.x, iconContainer.sizeDelta.y);
		shineActive = buttonActive;
		_ = label.TextComponent;
		label.ShouldWrapNonAsian = true;
		label.ShouldAutoSizeAsian = true;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (ShowTextBackground)
		{
			UpdateTextBgSize();
		}
		UpdateLabelVisibility();
	}

	private void UpdateTextBgSize()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).gameObject.activeInHierarchy && m_showLabelBackground)
		{
			((TMP_Text)label.TextComponent).ForceMeshUpdate(false, false);
			Bounds textBounds = ((TMP_Text)label.TextComponent).textBounds;
			Vector2 val = Vector2.op_Implicit(((Bounds)(ref textBounds)).size);
			textBg.SetWidth(val.x + 10f);
			textBg.SetHeight(val.y + 9f);
		}
	}

	private void UpdateLabelVisibility()
	{
		if (m_labelShowOnHover)
		{
			bool flag = false;
			if (buttonState == ButtonStates.Over || Highlighted || SystemManager.ShouldUseTouchInterface() || PolytopiaInput.Omnicursor.IsCursorAffixedToChildOf(((Component)this).GetComponent<RectTransform>()))
			{
				flag = true;
			}
			((Component)label).gameObject.SetActive(flag);
			((Component)textBg).gameObject.SetActive(m_showLabelBackground && flag);
			if (m_showLabelBackground && flag)
			{
				UpdateTextBgSize();
			}
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		OnPointerEnterAnimation();
		UpdateLabelVisibility();
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		OnPointerExitAnimation();
		UpdateLabelVisibility();
	}

	public override void OnSelect(BaseEventData data)
	{
		base.OnSelect(data);
		UpdateLabelVisibility();
	}

	public override void OnDeselect(BaseEventData data)
	{
		base.OnDeselect(data);
		UpdateLabelVisibility();
	}

	public override void CancelTweens()
	{
		base.CancelTweens();
		hoverTween = null;
	}

	public new void OnPointerEnterAnimation()
	{
		CancelTweens();
		if ((Object)(object)hoverObject != (Object)null && AnimationsEnabled)
		{
			hoverTween = (Tween)(object)ShortcutExtensions.DOScale((Transform)(object)hoverObject, 1.1f, 0.2f);
		}
	}

	public new void OnPointerExitAnimation()
	{
		CancelTweens();
		if ((Object)(object)hoverObject != (Object)null && AnimationsEnabled)
		{
			hoverTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)hoverObject, 1f, 0.3f), (Ease)27, 5f);
		}
	}

	public void SetFaceIcon(Sprite faceIcon)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		icon.sprite = faceIcon;
		icon.useSpriteMesh = true;
		((Graphic)icon).SetNativeSize();
		Vector2 sizeDelta = ((Graphic)icon).rectTransform.sizeDelta;
		((Graphic)icon).rectTransform.sizeDelta = sizeDelta * faceIconSizeMultiplier;
		RectTransform obj = ((Graphic)icon).rectTransform;
		float x = faceIcon.pivot.x;
		Rect rect = faceIcon.rect;
		float num = x / ((Rect)(ref rect)).width;
		float y = faceIcon.pivot.y;
		rect = faceIcon.rect;
		obj.pivot = new Vector2(num, y / ((Rect)(ref rect)).height);
		((Component)icon).gameObject.SetActive(true);
	}

	public void SetSprite(Sprite sprite, bool nativeSize = true)
	{
		icon.sprite = sprite;
		bool flag = (Object)(object)icon.sprite != (Object)null;
		((Component)icon).gameObject.SetActive(flag);
		if (flag && nativeSize)
		{
			((Graphic)icon).SetNativeSize();
			UIUtils.FitImageContentInParent(((Graphic)icon).rectTransform);
		}
	}

	public void ShowIconAndTextContainer(Sprite sprite, string text)
	{
		smallIcon.sprite = sprite;
		((TMP_Text)textInContainer).text = text;
		((Component)iconAndTextContainer).gameObject.SetActive(true);
	}

	public void HideIconAndTextContainer()
	{
		((Component)iconAndTextContainer).gameObject.SetActive(false);
	}

	public void SetButtonBought()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Cost = -1f;
		((Component)Outline).gameObject.SetActive(false);
		bgColors.deactiveColor = ColorConstants.green;
		((Graphic)bg).color = bgColors.GetColorForState(m_buttonActive, Highlighted);
	}

	public void SetIconColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)icon).color = color;
	}
}
