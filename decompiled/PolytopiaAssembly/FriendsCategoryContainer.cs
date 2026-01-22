using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendsCategoryContainer : UIBasicComponent
{
	[Flags]
	public enum DisableFlags
	{
		AlreadyFriend = 1,
		FriendRequestSent = 2,
		PickedForMatch = 4,
		Unmigrated = 8
	}

	[SerializeField]
	protected TMPLocalizer header;

	[SerializeField]
	protected RectTransform grid;

	[Header("Prefabs")]
	public PlayerButton playerButtonPrefab;

	[HideInInspector]
	public Action<PlayerData> selectCallback;

	[EnumFlag]
	public DisableFlags disableFlags;

	private PlayerButton[] buttons;

	private PlayerData[] data;

	public void ClearButtons()
	{
		if (buttons != null)
		{
			PlayerButton[] array = buttons;
			for (int i = 0; i < array.Length; i++)
			{
				Object.DestroyImmediate((Object)(object)((Component)array[i]).gameObject);
			}
			buttons = null;
		}
	}

	public void SetData(PlayerData[] data, string headerKey, bool shouldSortData = true)
	{
		this.data = data;
		header.Key = headerKey;
		((Component)header).gameObject.SetActive(headerKey != null);
		if (shouldSortData)
		{
			Array.Sort(this.data, new FriendSort());
		}
		int num = this.data.Length;
		ClearButtons();
		buttons = new PlayerButton[num];
		for (int i = 0; i < num; i++)
		{
			PlayerData playerData = this.data[i];
			PlayerButton playerButton = Object.Instantiate<PlayerButton>(playerButtonPrefab, (Transform)(object)grid);
			playerButton.UpdateScrollerOnHighlight = true;
			playerButton.id = i;
			playerButton.OnClicked += OnButtonClicked;
			UpdateButton(playerButton, playerData);
			buttons[i] = playerButton;
		}
	}

	public void RefreshItems()
	{
		int num = data.Length;
		for (int i = 0; i < num; i++)
		{
			PlayerData playerData = data[i];
			PlayerButton playerButton = buttons[i];
			UpdateButton(playerButton, playerData);
			playerButton.Refresh();
		}
	}

	private void UpdateButton(PlayerButton button, PlayerData data)
	{
		button.SetFriendData(data);
		button.PlayerButtonEnable = true;
		if ((disableFlags & DisableFlags.PickedForMatch) != 0 && GameManager.PreliminaryGameSettings.HavePlayer(data.profile.id))
		{
			button.PlayerButtonEnable = false;
		}
		if ((disableFlags & DisableFlags.AlreadyFriend) != 0 && data.state == PlayerData.State.Accepted)
		{
			button.PlayerButtonEnable = false;
		}
		if ((disableFlags & DisableFlags.FriendRequestSent) != 0 && data.state == PlayerData.State.SentRequest)
		{
			button.PlayerButtonEnable = false;
		}
		if ((disableFlags & DisableFlags.Unmigrated) != 0 && !data.profile.lastLoginDate.HasValue)
		{
			button.PlayerButtonEnable = false;
		}
	}

	public Selectable GetButton(int idx)
	{
		if (idx >= 0 && idx < buttons.Length)
		{
			return (Selectable)(object)buttons[idx].button;
		}
		return null;
	}

	protected void OnButtonClicked(int id, BaseEventData eventData)
	{
		selectCallback?.Invoke(data[id]);
	}
}
