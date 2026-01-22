using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameSetupNameRow : UIBasicComponent
{
	[SerializeField]
	protected TMPLocalizer header;

	[SerializeField]
	protected TMP_InputField inputField;

	[HideInInspector]
	public Action<string> inputDoneCallback;

	private bool hasSearchedForScrollRectHighlightHelper;

	private ScrollRectHighlightHelper scrollRectHighlightHelper;

	public string HeaderKey
	{
		get
		{
			return header.Key;
		}
		set
		{
			header.Key = value;
		}
	}

	public string Name
	{
		get
		{
			return inputField.text;
		}
		set
		{
			inputField.text = value;
		}
	}

	public bool IsSelected => (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)inputField).gameObject;

	public void OnEditDone(string value)
	{
		inputDoneCallback?.Invoke(value);
	}

	public ScrollRectHighlightHelper GetScrollRectHighlightHelper()
	{
		if (!hasSearchedForScrollRectHighlightHelper && (Object)(object)scrollRectHighlightHelper == (Object)null)
		{
			scrollRectHighlightHelper = ((Component)this).GetComponentInParent<ScrollRectHighlightHelper>();
		}
		return scrollRectHighlightHelper;
	}

	public void OnSelect(string text)
	{
		if ((Object)(object)GetScrollRectHighlightHelper() != (Object)null)
		{
			scrollRectHighlightHelper.ScrollToObject(base.rectTransform);
		}
	}
}
