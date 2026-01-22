using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPLocalizer : UIBasicComponent
{
	public delegate void OnTextUpdatedEvent();

	[Tooltip("String id")]
	public string localizationKey = string.Empty;

	[Tooltip("Example: -{0}- which would result in -VALUE-")]
	public string format = string.Empty;

	[SerializeField]
	private bool shouldAutoSizeAsian;

	[SerializeField]
	private bool shouldWrapNonAsian;

	protected TextMeshProUGUI m_textComponent;

	protected string[] arguments;

	protected bool textOverridden;

	protected float originalFontSize;

	public bool ShouldAutoSizeAsian
	{
		get
		{
			return shouldAutoSizeAsian;
		}
		set
		{
			shouldAutoSizeAsian = value;
			UpdateOverflowStrategy();
		}
	}

	public bool ShouldWrapNonAsian
	{
		get
		{
			return shouldWrapNonAsian;
		}
		set
		{
			shouldWrapNonAsian = value;
			UpdateOverflowStrategy();
		}
	}

	public string Key
	{
		get
		{
			return localizationKey;
		}
		set
		{
			localizationKey = value;
			textOverridden = false;
			UpdateText();
		}
	}

	public string Text
	{
		get
		{
			return ((TMP_Text)TextComponent).text;
		}
		set
		{
			((TMP_Text)TextComponent).text = value;
			textOverridden = true;
		}
	}

	public string[] Arguments
	{
		get
		{
			return arguments;
		}
		set
		{
			arguments = value;
			UpdateText();
		}
	}

	public TextMeshProUGUI TextComponent
	{
		get
		{
			if ((Object)(object)m_textComponent == (Object)null)
			{
				m_textComponent = ((Component)this).GetComponent<TextMeshProUGUI>();
			}
			return m_textComponent;
		}
	}

	public Vector2 PreferedValues
	{
		get
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			TextMeshProUGUI textComponent = TextComponent;
			string text = ((TMP_Text)TextComponent).text;
			Rect rect = ((TMP_Text)TextComponent).rectTransform.rect;
			float width = ((Rect)(ref rect)).width;
			rect = ((TMP_Text)TextComponent).rectTransform.rect;
			return ((TMP_Text)textComponent).GetPreferredValues(text, width, ((Rect)(ref rect)).height);
		}
	}

	public event OnTextUpdatedEvent OnTextUpdated;

	private void OnEnable()
	{
		UpdateText();
		LocalizationEvents.OnLocalizationUpdated += OnLocalizationUpdated;
	}

	private void OnDisable()
	{
		LocalizationEvents.OnLocalizationUpdated -= OnLocalizationUpdated;
	}

	private void OnLocalizationUpdated()
	{
		UpdateText();
	}

	public void UpdateText()
	{
		if (Key != string.Empty && !textOverridden)
		{
			if (string.IsNullOrEmpty(format))
			{
				if (Arguments == null || Arguments.Length == 0)
				{
					((TMP_Text)TextComponent).text = Localization.Get(Key);
				}
				else
				{
					TextMeshProUGUI textComponent = TextComponent;
					string key = Key;
					object[] args = Arguments;
					((TMP_Text)textComponent).text = Localization.Get(key, args);
				}
			}
			else
			{
				((TMP_Text)TextComponent).text = string.Format(format, Localization.Get(Key));
			}
			this.OnTextUpdated?.Invoke();
		}
		UpdateOverflowStrategy();
	}

	private void UpdateOverflowStrategy()
	{
		bool flag = Localization.IsAsianLanguage();
		if (ShouldAutoSizeAsian)
		{
			if (!((TMP_Text)TextComponent).enableAutoSizing && flag)
			{
				originalFontSize = ((TMP_Text)TextComponent).fontSize;
			}
			bool num = ((TMP_Text)TextComponent).enableAutoSizing && !flag;
			((TMP_Text)TextComponent).enableAutoSizing = flag;
			if (num && originalFontSize != 0f)
			{
				((TMP_Text)TextComponent).fontSize = originalFontSize;
			}
		}
		if (ShouldWrapNonAsian)
		{
			((TMP_Text)TextComponent).enableWordWrapping = !flag;
		}
	}
}
