using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIBasicButton : UIButtonBase
{
	[Header("Basic Button")]
	[SerializeField]
	protected ColorStates bgColorStates = new ColorStates
	{
		defaultColor = new Color(0f, 0.6f, 1f, 0.8f),
		hoverColor = new Color(1f, 1f, 1f, 0.8f),
		highlightedColor = new Color(1f, 1f, 1f, 0.8f),
		highlightedHoverColor = new Color(1f, 1f, 1f, 0.8f),
		disabledColor = new Color(1f, 0.2f, 0f, 0.298f)
	};

	protected Image bg;

	public Color BgColor => GetColorForState(BgColorStates);

	public virtual Image BG
	{
		get
		{
			if ((Object)(object)bg == (Object)null)
			{
				bg = ((Component)hoverObject).GetComponent<Image>();
			}
			return bg;
		}
	}

	public virtual ColorStates BgColorStates
	{
		get
		{
			return bgColorStates;
		}
		set
		{
			bgColorStates = value;
			UpdateColors();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		UpdateColors();
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		UpdateColors();
	}

	public override void PointerClick(PointerEventData eventData)
	{
		base.PointerClick(eventData);
		UpdateColors();
	}

	public override void PointerUp(PointerEventData eventData)
	{
		base.PointerUp(eventData);
		UpdateColors();
	}

	public override void OnSelect(BaseEventData data)
	{
		base.OnSelect(data);
		UpdateColors();
	}

	public override void OnDeselect(BaseEventData data)
	{
		base.OnDeselect(data);
		UpdateColors();
	}

	protected override void OnUINavigationTypeChanged(UINavigationManager.NavigationType navType)
	{
		base.OnUINavigationTypeChanged(navType);
		UpdateColors();
	}

	public virtual void UpdateColors()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)hoverObject != (Object)null)
		{
			((Graphic)BG).color = BgColor;
		}
	}

	public Color GetColorForState(ColorStates colorState)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		switch (buttonState)
		{
		case ButtonStates.None:
			if (!ButtonEnabled)
			{
				return colorState.disabledColor;
			}
			if (Highlighted && UINavigationManager.navigationType == UINavigationManager.NavigationType.Buttons && (Object)(object)selectionObject == (Object)null)
			{
				return colorState.highlightedColor;
			}
			return colorState.defaultColor;
		case ButtonStates.Over:
		case ButtonStates.Down:
			if (!ButtonEnabled)
			{
				return colorState.disabledColor;
			}
			if (Highlighted && UINavigationManager.navigationType == UINavigationManager.NavigationType.Buttons && (Object)(object)selectionObject == (Object)null)
			{
				return colorState.highlightedHoverColor;
			}
			if (UINavigationManager.navigationType == UINavigationManager.NavigationType.Mouse)
			{
				return colorState.hoverColor;
			}
			return colorState.defaultColor;
		default:
			return colorState.defaultColor;
		}
	}
}
