using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameSetupGameNameRow : UIBasicComponent
{
	[SerializeField]
	protected TMPLocalizer header;

	[SerializeField]
	protected UIPlainButton gameNameButton;

	[HideInInspector]
	public Action<string> OnValueChanged;

	private bool hasSearchedForScrollRectHighlightHelper;

	private ScrollRectHighlightHelper scrollRectHighlightHelper;

	protected string value = "None";

	protected int gameNameChanges;

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

	public bool IsSelected => (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)gameNameButton).gameObject;

	private void OnEnable()
	{
		gameNameButton.OnClicked += OnClicked;
	}

	private void OnDisable()
	{
		gameNameButton.OnClicked -= OnClicked;
		gameNameChanges = 0;
	}

	private void OnClicked(int id, BaseEventData eventData = null)
	{
		UpdateGameName();
	}

	public void UpdateGameName()
	{
		if (gameNameChanges < 50)
		{
			gameNameChanges++;
		}
		value = PolyLanguage.MakeGameName(gameNameChanges >= 50);
		UpdateGameNameLabel();
		OnValueChanged?.Invoke(value);
	}

	private void UpdateGameNameLabel()
	{
		gameNameButton.text = value;
	}

	public string GetValue()
	{
		return value;
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
