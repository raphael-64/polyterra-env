using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITextButton : UIBasicButton
{
	public delegate void ButtonSizeChange(float newSize);

	[Header("Text Button")]
	public Image icon;

	[SerializeField]
	protected RectTransform iconContainer;

	[SerializeField]
	protected TMPLocalizer labelLocalizer;

	public bool autoSize = true;

	[Header("Colors")]
	[SerializeField]
	protected ColorStates labelColorStates = new ColorStates
	{
		defaultColor = new Color(1f, 1f, 1f, 1f),
		hoverColor = new Color(0f, 0f, 0f, 1f),
		highlightedColor = new Color(0f, 0f, 0f, 1f),
		highlightedHoverColor = new Color(0f, 0f, 0f, 1f),
		disabledColor = new Color(1f, 1f, 1f, 0.502f)
	};

	[SerializeField]
	protected float margin = 12f;

	public string text
	{
		get
		{
			return ((TMP_Text)labelLocalizer.TextComponent).text;
		}
		set
		{
			((TMP_Text)labelLocalizer.TextComponent).text = value;
			UpdateSize();
		}
	}

	public float FontSize
	{
		get
		{
			return ((TMP_Text)labelLocalizer.TextComponent).fontSize;
		}
		set
		{
			((TMP_Text)labelLocalizer.TextComponent).fontSize = value;
		}
	}

	public float MaxFontSize
	{
		get
		{
			return ((TMP_Text)labelLocalizer.TextComponent).fontSizeMax;
		}
		set
		{
			((TMP_Text)labelLocalizer.TextComponent).fontSizeMax = value;
		}
	}

	public ColorStates BGColors
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

	public string Key
	{
		get
		{
			return labelLocalizer.Key;
		}
		set
		{
			labelLocalizer.Key = value;
		}
	}

	public override bool ButtonEnabled
	{
		get
		{
			return base.ButtonEnabled;
		}
		set
		{
			base.ButtonEnabled = value;
			UpdateColors();
		}
	}

	public override bool Highlighted
	{
		get
		{
			return base.Highlighted;
		}
		set
		{
			base.Highlighted = value;
			UpdateColors();
		}
	}

	public virtual ColorStates LabelColorStates
	{
		get
		{
			return labelColorStates;
		}
		set
		{
			labelColorStates = value;
		}
	}

	public event ButtonSizeChange OnSizeChanged;

	public override void Awake()
	{
		base.Awake();
		if (labelLocalizer.Key == string.Empty)
		{
			text = ((TMP_Text)labelLocalizer.TextComponent).text;
		}
		UpdateColors();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		labelLocalizer.OnTextUpdated += OnLabelLocalizerUpdated;
		UpdateSize();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		labelLocalizer.OnTextUpdated -= OnLabelLocalizerUpdated;
	}

	private void OnLabelLocalizerUpdated()
	{
		UpdateSize();
	}

	public virtual void UpdateSize()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (autoSize)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(labelLocalizer.rectTransform);
			Vector2 val = (((Object)(object)iconContainer != (Object)null) ? iconContainer.sizeDelta : Vector2.zero);
			Vector2 sizeDelta = base.rectTransform.sizeDelta;
			sizeDelta.x = Mathf.Max(((TMP_Text)labelLocalizer.TextComponent).preferredWidth + margin * 2f + val.x, sizeDelta.y);
			base.rectTransform.sizeDelta = sizeDelta;
			this.OnSizeChanged?.Invoke(sizeDelta.x);
		}
	}

	public override void UpdateColors()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateColors();
		if ((Object)(object)labelLocalizer != (Object)null)
		{
			((Graphic)labelLocalizer.TextComponent).color = GetColorForState(labelColorStates);
		}
	}
}
