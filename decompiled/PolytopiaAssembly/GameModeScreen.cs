using System.Collections.Generic;
using DG.Tweening;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameModeScreen : UIScreenBase
{
	[Header("Game Mode Screen")]
	[SerializeField]
	protected RectTransform verticalList;

	[SerializeField]
	protected RectTransform[] horizontalLists;

	[SerializeField]
	protected GamemodeButton[] buttons;

	[SerializeField]
	protected int hMaxNum = 2;

	[Space(10f)]
	[SerializeField]
	protected UILabelButton tutorialButton;

	public static void StartTutorial()
	{
		SettingsUtils.IsFirstTime = false;
		GameSettings gameSettings = new GameSettings();
		gameSettings.BaseGameMode = GameMode.Tutorial;
		gameSettings.SetUnlockedTribes(GameManager.GetPurchaseManager().GetUnlockedTribes());
		gameSettings.mapPreset = MapPreset.Lakes;
		GameManager.StartingTribe = TribeData.Type.Imperius;
		GameManager.StartingTribeMix = TribeData.Type.None;
		GameManager.StartingSkin = SkinType.Default;
		GameManager.PreliminaryGameSettings = gameSettings;
		GameManager.PreliminaryGameSettings.OpponentCount = 1;
		GameManager.PreliminaryGameSettings.Difficulty = GameSettings.Difficulties.Easy;
		UIBlackFader.FadeIn(0.5f, async delegate
		{
			DOTween.KillAll(false);
			await GameManager.Instance.CreateSinglePlayerGame();
		});
	}

	public override void Init()
	{
		base.Init();
		GamemodeButton[] array = buttons;
		foreach (GamemodeButton gamemodeButton in array)
		{
			switch (gamemodeButton.GetGameMode())
			{
			case GameMode.Perfection:
				gamemodeButton.OnClicked += OnPerfection;
				break;
			case GameMode.Domination:
				gamemodeButton.OnClicked += OnDomination;
				break;
			case GameMode.Custom:
				gamemodeButton.OnClicked += OnCustom;
				break;
			}
		}
		tutorialButton.OnClicked += TutorialButton_OnClicked;
	}

	public override void Show(bool instant = false)
	{
		GameManager.Client?.Reset();
		base.Show(instant);
		StartSceneBg.Bright = true;
	}

	private void OnGameMode(GameMode gameMode)
	{
		string settingsNameFromModes = GameSettingsExtensions.GetSettingsNameFromModes(GameType.SinglePlayer, gameMode);
		GameSettingsExtensions.TryLoadFromDisk(out var settings, settingsNameFromModes);
		GameManager.PreliminaryGameSettings = settings;
		GameManager.PreliminaryGameSettings.BaseGameMode = gameMode;
		UIManager.Instance.ShowScreen(UIConstants.Screens.TribeSelector);
	}

	public override void OnScreenUpdated()
	{
		UpdateListLayout();
	}

	private void UpdateListLayout()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = NativeHelpers.Screen();
		if (((Vector2Int)(ref val)).x > 600)
		{
			val = NativeHelpers.Screen();
			int x = ((Vector2Int)(ref val)).x;
			val = NativeHelpers.Screen();
			if (x > ((Vector2Int)(ref val)).y)
			{
				int num = 0;
				int num2 = 0;
				int num3 = buttons.Length;
				for (int i = 0; i < num3; i++)
				{
					((Component)horizontalLists[num2]).gameObject.SetActive(true);
					((Transform)buttons[i].rectTransform).SetParent((Transform)(object)horizontalLists[num2]);
					num++;
					if (num >= hMaxNum)
					{
						num = 0;
						num2++;
					}
				}
				goto IL_00f9;
			}
		}
		for (int j = 0; j < buttons.Length; j++)
		{
			GamemodeButton obj = buttons[j];
			((Transform)obj.rectTransform).SetParent((Transform)(object)verticalList, false);
			((Transform)obj.rectTransform).SetSiblingIndex(j);
		}
		RectTransform[] array = horizontalLists;
		for (int k = 0; k < array.Length; k++)
		{
			((Component)array[k]).gameObject.SetActive(false);
		}
		goto IL_00f9;
		IL_00f9:
		LayoutRebuilder.ForceRebuildLayoutImmediate(verticalList);
	}

	public void OnPerfection(int id, BaseEventData eventData = null)
	{
		if (!GameManager.GetVersioningInfoHolder().IsHighscoreEnabled(out var message))
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("versioning.highscores.title");
			basicPopup.Description = (string.IsNullOrEmpty(message) ? Localization.Get("versioning.highscores") : message);
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup.Show();
		}
		OnGameMode(GameMode.Perfection);
		GameManager.GetAnalyticsManager().SendEvent("game_mode_choose", new Dictionary<string, object> { { "game_mode", "perfection" } });
	}

	public void OnDomination(int id, BaseEventData eventData = null)
	{
		OnGameMode(GameMode.Domination);
		GameManager.GetAnalyticsManager().SendEvent("game_mode_choose", new Dictionary<string, object> { { "game_mode", "domination" } });
	}

	public void OnCustom(int id, BaseEventData eventData = null)
	{
		OnGameMode(GameMode.Custom);
		GameManager.GetAnalyticsManager().SendEvent("game_mode_choose", new Dictionary<string, object> { { "game_mode", "creative" } });
	}

	private void TutorialButton_OnClicked(int id, BaseEventData eventData = null)
	{
		StartTutorial();
	}
}
