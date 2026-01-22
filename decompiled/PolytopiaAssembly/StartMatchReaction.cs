using System;
using System.Collections.Generic;
using DG.Tweening;
using PolytopiaBackendBase.Game;
using UI.Popups;
using UnityEngine;

public class StartMatchReaction : ReactionBase
{
	private readonly StartMatchAction action;

	public StartMatchReaction(StartMatchAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		NotificationManager.HideAlert();
		MapRenderer.Current.Refresh();
		ResourceEvents.RefreshWallets(GameManager.LocalPlayer.Id);
		if (GameManager.Client.ActionManager.LastSeenCommand <= 1 && !GameManager.Client.IsReplay)
		{
			AudioSource audioSource = AudioManager.GetAudioSource(AudioManager.AudioSourceTypes.TribeMusic);
			audioSource.volume = 0f;
			audioSource.clip = AudioManager.GetTribeMusic(GameManager.LocalPlayer.tribe.GetName(), GameManager.LocalPlayer.skinType.GetName());
			if (!audioSource.isPlaying)
			{
				audioSource.Play();
			}
			AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 1f, 0.2f, (Ease)1);
			DoWelcomeCinematic(onComplete);
		}
		else
		{
			CameraController.Instance.CenterOnPosition(GameManager.LocalPlayer.startTile.ToPosition(), 0f);
			GameEvents.MatchStart();
			onComplete();
		}
	}

	private void StartGame(Action onComplete)
	{
		GameEvents.MatchStart();
		onComplete();
		AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 0f, 1f, (Ease)1);
		InputManager.EnableInput(InputManager.GameInputs);
	}

	private void DoWelcomeCinematic(Action onComplete)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		GameManager.GetAnalyticsManager().SendEvent("intro_popup_view", new Dictionary<string, object> { 
		{
			"game_id",
			GameManager.Client.CurrentGameId
		} });
		GameMode gameMode = GameManager.Client.GameState.Settings.RulesGameMode;
		InputManager.DisableInput(InputManager.GameInputs);
		CameraController.Instance.CenterOnPosition(GameManager.LocalPlayer.startTile.ToPosition() + new Vector2(0f, 1.5f), 0f);
		CameraController.Instance.CenterOnPosition(GameManager.LocalPlayer.startTile.ToPosition(), 6f, delegate
		{
			if (GameManager.Client != null && GameManager.Client.IsSpectating)
			{
				StartGame(onComplete);
			}
			else if (gameMode == GameMode.Tutorial)
			{
				ShowTutorialWelcomePopup();
			}
			else
			{
				ShowWelcomePopup();
			}
		});
		void Popup_OnRewardClicked()
		{
			StartGame(onComplete);
		}
		void ShowTutorialWelcomePopup()
		{
			IconRewardPopup iconRewardPopup = PopupManager.GetIconRewardPopup();
			iconRewardPopup.Header = Localization.Get("world.intro.title");
			iconRewardPopup.Description = GetDescription(gameMode);
			iconRewardPopup.SpriteHandle.Request(SpriteData.GetHeadSpriteAddresses(GameManager.GameState, GameManager.LocalPlayer));
			iconRewardPopup.RewardButton.sprite = UIManager.IconData.GetSprite("Resource");
			iconRewardPopup.RewardButton.text = "6";
			iconRewardPopup.OnRewardClicked = Popup_OnRewardClicked;
			iconRewardPopup.forceScrollerBg = true;
			iconRewardPopup.Show(0.3f);
		}
		void ShowWelcomePopup()
		{
			IconPopup iconPopup = PopupManager.GetIconPopup();
			iconPopup.Header = Localization.Get("world.intro.title");
			iconPopup.Description = GetDescription(gameMode);
			iconPopup.spriteHandle.Request(SpriteData.GetHeadSpriteAddresses(GameManager.GameState, GameManager.LocalPlayer));
			iconPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					StartGame(onComplete);
				})
			};
			iconPopup.forceScrollerBg = true;
			iconPopup.Show(0.3f);
		}
	}

	private string GetDescription(GameMode gameMode)
	{
		string text = string.Format("{0}\n\n", Localization.Get("world.intro.text", GameManager.LocalPlayer.GetLocalizedTribeName(GameManager.GameState)));
		switch (gameMode)
		{
		case GameMode.Perfection:
			text += Localization.Get("world.intro.objective", Localization.Get("gamemode.perfection.description"));
			break;
		case GameMode.Domination:
			text += Localization.Get("world.intro.objective", Localization.Get("gamemode.domination.description"));
			break;
		case GameMode.Glory:
			text += Localization.Get("world.intro.objective", Localization.Get("gamemode.glory.description", GameManager.Client.GameState.Settings.rules.ScoreLimit));
			break;
		case GameMode.Might:
			text += Localization.Get("world.intro.objective", Localization.Get("gamemode.might.description"));
			break;
		case GameMode.Tutorial:
			text += Localization.Get("gamemode.tutorial.description");
			break;
		}
		return text;
	}
}
