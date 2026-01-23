using IchiGamepad;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ButtonSprite : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private ButtonSpriteHub _hub;

	[Header("Button settings")]
	[SerializeField]
	private ButtonSpriteMode _mode = ButtonSpriteMode.Borderless;

	[SerializeField]
	[LogicalButtonDropdown]
	private ulong _button;

	public RectTransform rectTransform => ((Component)this).GetComponent<RectTransform>();

	public Image icon => _icon;

	private void Awake()
	{
		UpdateButtonSprite();
		if (Backend.IsInitialized)
		{
			Backend.GetInstance().OnControllerMappingChanged += UpdateButtonSprite;
		}
	}

	private void UpdateButtonSprite()
	{
		if (!Object.op_Implicit((Object)(object)_hub) || !Object.op_Implicit((Object)(object)_icon))
		{
			return;
		}
		if (!Backend.IsInitialized)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		((Component)this).gameObject.SetActive(!Backend.GetInstance().IsKeyboard);
		Sprite sprite = _hub.GetSprite(_mode, (LogicalButton)_button);
		if ((Object)(object)_icon.sprite != (Object)(object)sprite)
		{
			_icon.sprite = sprite;
		}
	}

	private void OnDestroy()
	{
		if (Backend.IsInitialized)
		{
			Backend.GetInstance().OnControllerMappingChanged -= UpdateButtonSprite;
		}
	}

	private void OnValidate()
	{
		_icon = ((Component)this).GetComponent<Image>();
	}
}
