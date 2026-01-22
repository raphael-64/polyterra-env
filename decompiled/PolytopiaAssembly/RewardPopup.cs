using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardPopup : BasicPopup
{
	public enum PopupType
	{
		CityLevelUp,
		Infiltrate
	}

	protected Action<TileData, CityReward> callback;

	[Header("Reward Popup")]
	[SerializeField]
	protected RectTransform rewardButtonContainer;

	[SerializeField]
	protected List<UIRoundButton> rewardButtonsList = new List<UIRoundButton>();

	[SerializeField]
	protected UIRoundButton rewardButtonPrefab;

	private CityReward[] cityRewards;

	private TileData tile;

	public Action<TileData, CityReward> RewardChoosenCallback
	{
		set
		{
			callback = value;
		}
	}

	public static event Action<RewardPopup> OnDataSet;

	public override void Init()
	{
		base.Init();
		base.IsUnskippable = true;
	}

	protected override void OnShowComplete()
	{
		base.OnShowComplete();
		UIUtils.SetExplicitNavigation(base.rectTransform);
		UINavigationManager.Select((Selectable)(object)rewardButtonsList[0].button);
	}

	public void SetData(PlayerState playerState, TileData tile, CityReward[] rewards, PopupType type, bool isReplay = false)
	{
		this.tile = tile;
		switch (type)
		{
		case PopupType.CityLevelUp:
			Header = Localization.Get("world.reward.levelup", tile.improvement.name);
			Description = Localization.Get("wcontroller.building.upgrade.reward", tile.improvement.name, tile.improvement.level);
			break;
		case PopupType.Infiltrate:
			Header = Localization.Get("world.infiltrate.header");
			Description = Localization.Get("world.infiltrate.text", tile.improvement.name);
			break;
		}
		SetRewards(playerState, rewards, isReplay);
		RewardPopup.OnDataSet?.Invoke(this);
	}

	public void SetRewards(PlayerState playerState, CityReward[] rewards, bool isReplay = false)
	{
		cityRewards = rewards;
		rewardButtonsList.ForEach(delegate(UIRoundButton x)
		{
			((Component)x).gameObject.SetActive(false);
		});
		for (int num = 0; num < cityRewards.Length; num++)
		{
			if (num >= rewardButtonsList.Count)
			{
				GetRoundButton();
			}
			UIRoundButton uIRoundButton = rewardButtonsList[num];
			uIRoundButton.sprite = UIManager.IconData.GetSprite(rewards[num].ToString());
			uIRoundButton.text = LocalizationUtils.CapitalizeString(Localization.Get(CityRewardData.GetRewardName(rewards[num])));
			((Component)uIRoundButton).gameObject.SetActive(true);
			uIRoundButton.CanvasGroup.interactable = !isReplay;
			uIRoundButton.CanvasGroup.blocksRaycasts = !isReplay;
		}
	}

	public override void ResetPopup()
	{
		base.ResetPopup();
		rewardButtonsList.ForEach(delegate(UIRoundButton x)
		{
			x.Highlighted = false;
		});
	}

	public void ReplayClickButton(CityReward reward, Action onHideCallback = null, float delay = 0.8f)
	{
		ReplayClickButton((cityRewards[0] != reward) ? 1 : 0, onHideCallback, delay);
	}

	public override void ReplayClickButton(int buttonIndex, Action onHideCallback = null, float delay = 0.8f)
	{
		replayClickButton = rewardButtonsList[buttonIndex];
		replayClickAnimationStep = 0;
		replayClickDelay = new PollAnimation<float>(delay);
		HideCallback = onHideCallback;
	}

	private UIRoundButton GetRoundButton()
	{
		UIRoundButton uIRoundButton = Object.Instantiate<UIRoundButton>(rewardButtonPrefab, (Transform)(object)rewardButtonContainer);
		uIRoundButton.buttonActive = true;
		uIRoundButton.OnClicked += OnRewardButtonClicked;
		uIRoundButton.id = rewardButtonsList.Count;
		rewardButtonsList.Add(uIRoundButton);
		return uIRoundButton;
	}

	private void OnRewardButtonClicked(int id, BaseEventData eventData)
	{
		if (!GameManager.Client.ActionManager.IsWaitingForCommandTrigger(GameManager.Client.GetCurrentLocalPlayer().Id))
		{
			OnHide(id, eventData);
		}
		else if (!GameManager.Client.IsWaitingForCommand)
		{
			OnHide(id, eventData);
			callback?.Invoke(tile, cityRewards[id]);
		}
	}
}
