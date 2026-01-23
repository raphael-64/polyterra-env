using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIIconButton : UIBasicButton
{
	[SerializeField]
	protected Image iconHolder;

	[SerializeField]
	protected TMPLocalizer labelLocalizer;

	public string text
	{
		get
		{
			return ((TMP_Text)labelLocalizer.TextComponent).text;
		}
		set
		{
			((TMP_Text)labelLocalizer.TextComponent).text = value;
		}
	}

	public Sprite Icon
	{
		get
		{
			return iconHolder.sprite;
		}
		set
		{
			iconHolder.sprite = value;
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
}
