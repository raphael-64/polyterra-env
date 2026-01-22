using UnityEngine;
using UnityEngine.UI;

public class PopupButtonContainer : MonoBehaviour
{
	public UITextButton buttonPrefab;

	[Header("Button Styles")]
	public UIButtonBase.ColorStates leftButtonBgColors = new UIButtonBase.ColorStates
	{
		defaultColor = new Color(1f, 1f, 1f, 0.8f),
		hoverColor = new Color(0.4f, 0.4f, 0.4f, 0.8f),
		highlightedColor = new Color(1f, 1f, 1f, 0.8f),
		highlightedHoverColor = new Color(1f, 1f, 1f, 0.8f),
		disabledColor = new Color(1f, 0.2f, 0f, 0.298f)
	};

	public UIButtonBase.ColorStates leftButtonLabelColors = new UIButtonBase.ColorStates
	{
		defaultColor = Color.black,
		hoverColor = Color.white,
		highlightedColor = Color.black,
		highlightedHoverColor = Color.black,
		disabledColor = new Color(1f, 1f, 1f, 0.298f)
	};

	public UIButtonBase.ButtonAction hideCallback;

	public bool autoSelectAtStart = true;

	protected RectTransform m_rectTransform;

	protected UITextButton[] buttons;

	protected int startSelection = -1;

	protected bool buttonAnimationsEnabled = true;

	protected bool buttonsEnabled = true;

	public virtual bool ButtonsEnabled
	{
		set
		{
			UITextButton[] array = buttons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ButtonEnabled = buttonsEnabled;
			}
		}
	}

	public virtual bool ButtonAnimationsEnabled
	{
		get
		{
			return buttonAnimationsEnabled;
		}
		set
		{
			buttonAnimationsEnabled = value;
			UITextButton[] array = buttons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AnimationsEnabled = buttonAnimationsEnabled;
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

	public UITextButton[] Buttons => buttons;

	public Selectable StartSelection
	{
		get
		{
			if (buttons == null || buttons.Length == 0)
			{
				return null;
			}
			if (startSelection > -1 && startSelection < buttons.Length)
			{
				return (Selectable)(object)buttons[startSelection].button;
			}
			return null;
		}
	}

	public void SetButtonData(PopupBase.PopupButtonData[] buttonData)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		int num = buttonData.Length;
		buttons = new UITextButton[num];
		Vector2 val = default(Vector2);
		for (int i = 0; i < num; i++)
		{
			UITextButton uITextButton = Object.Instantiate<UITextButton>(buttonPrefab, ((Component)this).transform);
			((Vector2)(ref val))._002Ector((num == 1) ? 0.5f : ((float)(i / (num - 1))), 0.5f);
			uITextButton.rectTransform.anchorMin = val;
			uITextButton.rectTransform.anchorMax = val;
			uITextButton.rectTransform.pivot = val;
			uITextButton.rectTransform.anchoredPosition = Vector2.zero;
			uITextButton.Key = buttonData[i].text;
			((Object)uITextButton).name = $"PopupButton_{uITextButton.text}";
			uITextButton.id = buttonData[i].id;
			if (buttonData[i].closesPopup)
			{
				uITextButton.OnClicked += hideCallback;
			}
			if (buttonData[i].callback != null)
			{
				uITextButton.OnClicked += buttonData[i].callback;
			}
			if (buttonData[i].customColorStates != null)
			{
				uITextButton.BgColorStates = buttonData[i].customColorStates;
			}
			buttons[i] = uITextButton;
			if (buttonData[i].state == PopupBase.PopupButtonData.States.Selected)
			{
				startSelection = i;
			}
			else if (buttonData[i].state == PopupBase.PopupButtonData.States.Disabled)
			{
				uITextButton.ButtonEnabled = false;
			}
			buttons[i].AnimationsEnabled = buttonAnimationsEnabled;
		}
		if (num >= 2 && buttonData[0].customColorStates == null)
		{
			buttons[0].LabelColorStates = new UIButtonBase.ColorStates(leftButtonLabelColors);
			buttons[0].BgColorStates = new UIButtonBase.ColorStates(leftButtonBgColors);
		}
		if (startSelection >= 0)
		{
			PolytopiaInput.Omnicursor.AffixToUIElement(((Component)buttons[startSelection]).GetComponent<RectTransform>());
		}
		((Component)this).gameObject.SetActive(true);
	}

	public void ResetContainer()
	{
		if (buttons == null)
		{
			return;
		}
		int num = buttons.Length;
		for (int i = 0; i < num; i++)
		{
			UITextButton uITextButton = buttons[i];
			if (!((Object)(object)uITextButton == (Object)null))
			{
				uITextButton.AnimationsEnabled = false;
				Object.Destroy((Object)(object)((Component)uITextButton).gameObject);
			}
		}
		startSelection = -1;
	}

	public void DisableButtonAnimations()
	{
		ButtonAnimationsEnabled = false;
	}
}
