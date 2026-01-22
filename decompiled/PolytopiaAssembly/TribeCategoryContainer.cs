using System;
using System.Collections.Generic;
using Polytopia.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TribeCategoryContainer : UIBasicComponent
{
	public TMPLocalizer header;

	public TMPLocalizer description;

	public GridLayoutGroup grid;

	public LayoutElement gridLayoutElement;

	[Header("Prefabs")]
	public PlayerButton tribeButtonPrefab;

	[NonSerialized]
	public Action<TribeData.Type, TribeData.Type, PlayerButton> selectCallback;

	public Action<TribeData> longClickCallback;

	private List<TribeData> tribesData;

	private RectTransform m_gridRectTr;

	private PlayerButton[] buttons;

	public override void Init()
	{
		base.Init();
	}

	private void OnEnable()
	{
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
	}

	private void OnDisable()
	{
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
	}

	public void SetCategory(TribeData.CategoryEnum category)
	{
		string text = $"tribepicker.categories.{category.ToString().ToLowerInvariant()}tribes";
		if (Localization.HaveKey(text))
		{
			header.Key = text;
			string key = $"{text}.description";
			if (Localization.HaveKey(key))
			{
				description.Key = key;
				((Component)description).gameObject.SetActive(true);
			}
			else
			{
				((Component)description).gameObject.SetActive(false);
			}
		}
		else
		{
			((TMP_Text)header.TextComponent).text = "UNKNOWN";
		}
	}

	public void SetTribesData(List<TribeData> tribes)
	{
		UpdateGridLayout();
		tribesData = tribes;
		int count = tribesData.Count;
		buttons = new PlayerButton[count];
		for (int i = 0; i < count; i++)
		{
			TribeData tribeData = tribesData[i];
			PlayerButton playerButton = Object.Instantiate<PlayerButton>(tribeButtonPrefab, ((Component)grid).transform);
			playerButton.SetTribeData(tribeData);
			playerButton.id = i;
			playerButton.OnClicked += OnTribeClicked;
			playerButton.OnLongClick += OnTribeLongClick;
			playerButton.UpdateScrollerOnHighlight = true;
			buttons[i] = playerButton;
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
	}

	public Selectable GetButton(int idx)
	{
		if (idx >= 0 && idx < buttons.Length)
		{
			return (Selectable)(object)buttons[idx].button;
		}
		return null;
	}

	private void OnTribeClicked(int idx, BaseEventData eventData)
	{
		selectCallback?.Invoke(tribesData[idx].type, TribeData.Type.None, buttons[idx]);
	}

	private void OnTribeLongClick(TribeData tribe)
	{
		longClickCallback?.Invoke(tribe);
	}

	private void OnScreenSizeChanged(Vector2 screenSize)
	{
		UpdateGridLayout();
	}

	private void UpdateGridLayout()
	{
		float num = ((Screen.width > 600 && Screen.width > Screen.height) ? 444 : 328);
		if (num != gridLayoutElement.preferredWidth)
		{
			gridLayoutElement.preferredWidth = num;
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
		}
	}

	public void RefreshItems()
	{
		PlayerButton[] array = buttons;
		foreach (PlayerButton playerButton in array)
		{
			playerButton.Refresh();
			SkinType selectedSkin = GameManager.PreliminaryGameSettings.GetSelectedSkin(playerButton.Tribe.type);
			playerButton.SetSkin(selectedSkin);
		}
	}
}
