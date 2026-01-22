using System;
using System.Collections.Generic;
using DG.Tweening;
using Polytopia.Data;
using UI.Popups;
using UnityEngine;

public class MeetReaction : ReactionBase
{
	private readonly MeetAction action;

	public MeetReaction(MeetAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		PlayerState otherPlayerState;
		TribeData data;
		if (!GameManager.IsPlayerViewing(action.PlayerId))
		{
			onComplete();
		}
		else if (GameManager.GameState.TryGetPlayer(action.OtherPlayerId, out otherPlayerState) && GameManager.GameState.GameLogicData.TryGetData(otherPlayerState.tribe, out data))
		{
			if (GameManager.IsPlayerViewing(action.PlayerId) && !GameManager.Client.IsRecap)
			{
				GameManager.GetAnalyticsManager().SendEvent("meet_other_tribe_popup", new Dictionary<string, object> { 
				{
					"game_id",
					GameManager.Client.CurrentGameId
				} });
			}
			CameraController.Instance.CenterOnPosition(action.Coordinates.ToPosition(), 0.8f, delegate
			{
				if (GameManager.Client.IsSpectating)
				{
					onComplete();
				}
				else
				{
					AudioSource audioSource = AudioManager.GetAudioSource(AudioManager.AudioSourceTypes.TribeMusic);
					audioSource.volume = 0f;
					audioSource.clip = AudioManager.GetTribeMusic(otherPlayerState.tribe.GetName(), otherPlayerState.skinType.GetName());
					if (!audioSource.isPlaying)
					{
						audioSource.Play();
					}
					AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 1f, 0.2f, (Ease)1);
					if (GameManager.GameState.Version < 50)
					{
						IconPopup iconPopup = PopupManager.GetIconPopup();
						iconPopup.Header = Localization.Get("world.meet.tribe", otherPlayerState.GetLocalizedTribeName(GameManager.GameState));
						iconPopup.Description = GetDescription();
						iconPopup.spriteHandle.Request(SpriteData.GetHeadSpriteAddresses(GameManager.GameState, otherPlayerState));
						iconPopup.buttonData = new PopupBase.PopupButtonData[1]
						{
							new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
							{
								onComplete();
								AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 0f, 1f, (Ease)1);
							})
						};
						iconPopup.Show();
					}
					else
					{
						string linkedTribeNameWithSpace = otherPlayerState.GetLinkedTribeNameWithSpace(GameManager.GameState);
						IconRewardPopup iconRewardPopup = PopupManager.GetIconRewardPopup();
						iconRewardPopup.Header = Localization.Get("world.meet.tribe", linkedTribeNameWithSpace);
						iconRewardPopup.Description = GetDescription();
						iconRewardPopup.SetTribeInfoButtons(TextType.Header);
						iconRewardPopup.SetTribeInfoButtons(TextType.Description);
						iconRewardPopup.SpriteHandle.Request(SpriteData.GetHeadSpriteAddresses(GameManager.GameState, otherPlayerState));
						iconRewardPopup.RewardButton.sprite = null;
						iconRewardPopup.RewardButton.text = "";
						Sprite sprite = UIManager.IconData.GetSprite("Resource");
						string text = action.GetReward(GameManager.GameState).ToString();
						iconRewardPopup.RewardButton.ShowIconAndTextContainer(sprite, text);
						iconRewardPopup.OnRewardClicked = OnRewardClicked;
						GameManager.DelayCall(300, iconRewardPopup.Show);
					}
				}
			});
		}
		else
		{
			onComplete();
		}
		void OnRewardClicked()
		{
			onComplete();
			AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 0f, 1f, (Ease)1);
		}
	}

	private string GetDescription()
	{
		GameManager.GameState.TryGetPlayer(action.PlayerId, out var playerState);
		GameManager.GameState.TryGetPlayer(action.OtherPlayerId, out var playerState2);
		string text = "";
		bool flag = playerState2.GetAggression(playerState.Id, GameManager.GameState) > 0;
		if (!GameManager.GameState.Map.GetTile(action.Coordinates).GetExplored(action.PlayerId) || action.Coordinates == playerState.startTile)
		{
			text += Localization.Get("wcontroller.meet.tribe.distant", playerState2.GetLinkedTribeNameWithSpace(GameManager.GameState));
		}
		else
		{
			text += Localization.Get("wcontroller.meet.tribe.leader", playerState2.UserName);
			text = ((playerState2.score > playerState.score) ? ((!flag) ? (text + Localization.Get("wcontroller.meet.tribe.bigger.friendly")) : (text + Localization.Get("wcontroller.meet.tribe.bigger.hostile"))) : ((!flag) ? (text + Localization.Get("wcontroller.meet.tribe.smaller.friendly")) : (text + Localization.Get("wcontroller.meet.tribe.smaller.hostile"))));
		}
		if (!GameManager.GameState.Settings.rules.AllowTechSharing)
		{
			return text;
		}
		if (action.Tech != TechData.Type.Basic)
		{
			GameManager.GameState.GameLogicData.TryGetData(action.Tech, out var data);
			if (flag)
			{
				return text + "\n" + Localization.Get("wcontroller.meet.tribe.tech.hostile", Localization.Get(data.displayName));
			}
			return text + "\n" + Localization.Get("wcontroller.meet.tribe.tech.friendly", Localization.Get(data.displayName));
		}
		int reward = action.GetReward(GameManager.GameState);
		return text + "\n" + Localization.Get("wcontroller.meet.tribe.resource.boost", Localization.Get("wcontroller.tribe.level_" + reward / action.RewardMultiplier), reward);
	}

	public override string ToString()
	{
		return string.Format("{0} (Reward: {1})", GetType(), (action.Tech == TechData.Type.Basic) ? "Resources" : action.Tech.ToString());
	}
}
