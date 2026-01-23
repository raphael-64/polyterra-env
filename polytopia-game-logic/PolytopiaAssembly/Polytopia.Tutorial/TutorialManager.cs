using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Polytopia.Tutorial;

public class TutorialManager : MonoBehaviour
{
	private const string TUTORIAL_TASK_NAME = "tutorial.task.";

	private const float MARGIN = 14f;

	private const float DEFAULT_Y_OFFSET = -70f;

	[SerializeField]
	private TaskPanel taskPanel;

	[SerializeField]
	private List<TutorialTask> tutorialTasks = new List<TutorialTask>();

	private HudButtonBar buttonBar;

	private HintIcon hintIcon;

	private TutorialTask harvestTask;

	private TutorialTask trainTask;

	private TutorialTask nextTurnTask;

	private TutorialTask citiesTask;

	private TutorialTask technologyTask;

	private TutorialTask meetTribeTask;

	private TutorialTask currentTask;

	private bool tutorialCompleted;

	private bool waitForPopup;

	private bool firstCheck = true;

	private bool isLeftAligned;

	private bool isTopAligned;

	private Sequence taskPanelAnimationSequence;

	private void Start()
	{
		GameState gameState = GameManager.GameState;
		PlayerState playerState = GameManager.LocalPlayer;
		if (gameState.Settings.BaseGameMode != GameMode.Tutorial)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		for (int i = 0; i < gameState.PlayerStates.Count; i++)
		{
			gameState.PlayerStates[i].blockTrainUnits = true;
		}
		buttonBar = (UIManager.Instance.GetScreen(UIConstants.Screens.Hud) as HudScreen).buttonBar;
		buttonBar.blockRefreshingNextTurnButton = true;
		buttonBar.blockRefreshingStatsButton = true;
		buttonBar.blockRefreshingTechTreeButton = true;
		((Component)buttonBar.positionDisplay).gameObject.SetActive(false);
		buttonBar.nextTurnButton.BlockButton = true;
		buttonBar.techTreeButton.BlockButton = true;
		buttonBar.statsButton.BlockButton = true;
		NotificationManager.IgnoreNotifications(ignore: true);
		(UIManager.Instance.GetScreen(UIConstants.Screens.TechTree) as TechView).OnItemsRefreshed += TechView_OnItemsRefreshed;
		ClientActionManager actionManager = GameManager.Client.ActionManager;
		actionManager.OnFinishedProcessing = (Action)Delegate.Combine(actionManager.OnFinishedProcessing, new Action(ActionManager_OnFinishedProcessing));
		RewardPopup.OnDataSet += RewardPopup_OnDataSet;
		taskPanel.OnClicked += TaskPanel_OnClicked;
		GenerateTasks();
		CheckButtons();
		void GenerateTasks()
		{
			int tribeClimate = playerState.GetTribeClimate(gameState);
			Sprite sprite = UIManager.IconData.GetSprite("EndTurn");
			Sprite sprite2 = UIManager.IconData.GetSprite("Village");
			harvestTask = new TutorialTask
			{
				taskType = TutorialTask.TaskType.IncreaseCapitalLevel,
				targetValue = 2,
				descriptionKey = "harvestfruit"
			};
			tutorialTasks.Add(harvestTask);
			SpriteData.GetResourceSprite(ResourceData.Type.Fruit, tribeClimate, delegate(string atlasName, string spriteName, Sprite sprite3)
			{
				SetSprite(harvestTask, sprite3);
			});
			trainTask = new TutorialTask
			{
				taskType = TutorialTask.TaskType.TrainUnit,
				targetValue = 1,
				descriptionKey = "trainunit",
				unitType = UnitData.Type.Warrior
			};
			tutorialTasks.Add(trainTask);
			nextTurnTask = new TutorialTask
			{
				taskType = TutorialTask.TaskType.TurnNumber,
				targetValue = 1,
				descriptionKey = "endTurn",
				sprite = sprite
			};
			tutorialTasks.Add(nextTurnTask);
			citiesTask = new TutorialTask
			{
				taskType = TutorialTask.TaskType.NumberOfCities,
				targetValue = 2,
				descriptionKey = "capturevillage",
				sprite = sprite2
			};
			tutorialTasks.Add(citiesTask);
			technologyTask = new TutorialTask
			{
				taskType = TutorialTask.TaskType.HaveTechnology,
				techType = TechData.Type.Hunting,
				descriptionKey = "unlockTech"
			};
			tutorialTasks.Add(technologyTask);
			SpriteData.GetResourceSprite(ResourceData.Type.Game, tribeClimate, delegate(string atlasName, string spriteName, Sprite sprite3)
			{
				SetSprite(technologyTask, sprite3);
			});
		}
		static void SetSprite(TutorialTask tutorialTask, Sprite sprite)
		{
			tutorialTask.sprite = sprite;
		}
	}

	private void Update()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		if (!isLeftAligned && buttonBar.IsRightAligned())
		{
			RectTransform rectTransform = taskPanel.rectTransform;
			rectTransform.anchorMin = new Vector2(0f, 1f);
			rectTransform.anchorMax = new Vector2(0f, 1f);
			rectTransform.pivot = new Vector2(0f, 1f);
			rectTransform.SetAnchoredX(14f);
			isLeftAligned = true;
			taskPanel.SetYOffset(-70f);
			isTopAligned = false;
		}
		else if (isLeftAligned && !buttonBar.IsRightAligned())
		{
			RectTransform rectTransform2 = taskPanel.rectTransform;
			rectTransform2.anchorMin = new Vector2(0.5f, 1f);
			rectTransform2.anchorMax = new Vector2(0.5f, 1f);
			rectTransform2.pivot = new Vector2(0.5f, 1f);
			_ = rectTransform2.anchoredPosition;
			rectTransform2.SetAnchoredX(0f);
			isLeftAligned = false;
			taskPanel.SetYOffset(-70f);
			isTopAligned = false;
		}
		bool flag = UIManager.Instance.GetCurrentScreen() is TechView;
		((Component)taskPanel).gameObject.SetActive(currentTask != null && !flag);
		if (isLeftAligned)
		{
			float num = (taskPanel.rectTransform.sizeDelta.x + 28f) * UICanvasScalerHelper.GetUIScale();
			RectTransform rectTransform3 = ResourceBar.ScoreContainer.rectTransform;
			bool flag2 = num < ((Transform)rectTransform3).position.x - rectTransform3.sizeDelta.x * 0.5f * UICanvasScalerHelper.GetUIScale();
			if (!isTopAligned && flag2)
			{
				taskPanel.SetYOffset(-14f);
				isTopAligned = true;
			}
			else if (isTopAligned && !flag2)
			{
				taskPanel.SetYOffset(-70f);
				isTopAligned = false;
			}
		}
	}

	private void OnDestroy()
	{
		ClearStaticEvents();
	}

	private void FinishTutorial()
	{
		if (!tutorialCompleted)
		{
			tutorialCompleted = true;
			NotificationManager.IgnoreNotifications(ignore: false);
			AnimateTaskPanel(null);
			ClientActionManager actionManager = GameManager.Client.ActionManager;
			actionManager.OnFinishedProcessing = (Action)Delegate.Remove(actionManager.OnFinishedProcessing, new Action(ActionManager_OnFinishedProcessing));
			ClearStaticEvents();
			if (!firstCheck)
			{
				IconPopup iconPopup = PopupManager.GetIconPopup();
				iconPopup.sprite = UIManager.IconData.GetSprite("GM_Perfection");
				iconPopup.Header = Localization.Get("tutorial.completed.title");
				iconPopup.Description = Localization.Get("tutorial.completed.description");
				iconPopup.buttonData = new PopupBase.PopupButtonData[1]
				{
					new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
				};
				iconPopup.Show();
				((Component)this).gameObject.SetActive(false);
			}
		}
	}

	private void CheckButtons()
	{
		bool flag = trainTask.isCompleted && harvestTask.isCompleted;
		if (flag && buttonBar.blockRefreshingNextTurnButton)
		{
			AnimateButton(((Component)buttonBar.nextTurnButton).transform);
			buttonBar.blockRefreshingNextTurnButton = !flag;
			buttonBar.nextTurnButton.BlockButton = !flag;
		}
		bool isCompleted = citiesTask.isCompleted;
		if (isCompleted && buttonBar.blockRefreshingTechTreeButton)
		{
			AnimateButton(((Component)buttonBar.techTreeButton).transform);
			buttonBar.blockRefreshingTechTreeButton = !isCompleted;
			buttonBar.techTreeButton.BlockButton = !isCompleted;
		}
		bool isCompleted2 = technologyTask.isCompleted;
		if (isCompleted2 && buttonBar.blockRefreshingStatsButton)
		{
			AnimateButton(((Component)buttonBar.statsButton).transform);
			buttonBar.blockRefreshingStatsButton = !isCompleted2;
			buttonBar.statsButton.BlockButton = !isCompleted2;
			((Component)buttonBar.positionDisplay).gameObject.SetActive(isCompleted2);
		}
		buttonBar.RefreshComponents();
		void AnimateButton(Transform transform)
		{
			if (!firstCheck)
			{
				Sequence obj = DOTween.Sequence();
				TweenSettingsExtensions.Append(obj, (Tween)(object)ShortcutExtensions.DOScale(transform, 1.4f, 0.15f));
				TweenSettingsExtensions.Append(obj, (Tween)(object)ShortcutExtensions.DOScale(transform, 1f, 0.22f));
			}
		}
	}

	private void ClearStaticEvents()
	{
		RewardPopup.OnDataSet -= RewardPopup_OnDataSet;
	}

	private void HideTechView()
	{
		TechView techView = UIManager.Instance.GetScreen(UIConstants.Screens.TechTree) as TechView;
		if (techView.Showing)
		{
			techView.OnBack();
			techView.OnItemsRefreshed -= TechView_OnItemsRefreshed;
			techView.RefreshTechItems();
		}
	}

	private void AnimateTaskPanel(Action onComplete)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		if (taskPanelAnimationSequence != null)
		{
			TweenExtensions.Kill((Tween)(object)taskPanelAnimationSequence, false);
		}
		taskPanelAnimationSequence = DOTween.Sequence();
		TweenSettingsExtensions.AppendCallback(taskPanelAnimationSequence, new TweenCallback(taskPanel.SetTaskCompleted));
		TweenSettingsExtensions.InsertCallback(taskPanelAnimationSequence, 1f, (TweenCallback)delegate
		{
			taskPanel.Hide(onComplete);
		});
	}

	private void ActionManager_OnFinishedProcessing()
	{
		PlayerState localPlayer = GameManager.LocalPlayer;
		GameState gameState = GameManager.GameState;
		int num = localPlayer.CountUnits(gameState);
		uint currentTurn = gameState.CurrentTurn;
		int cities = localPlayer.cities;
		WorldCoordinates currentCapitalCoordinates = localPlayer.GetCurrentCapitalCoordinates(gameState);
		TileData tile = gameState.Map.GetTile(currentCapitalCoordinates);
		ImprovementData.Type type = tile.improvement.type;
		ushort level = tile.improvement.level;
		for (int i = 0; i < tutorialTasks.Count; i++)
		{
			TutorialTask tutorialTask = tutorialTasks[i];
			if (tutorialTask.isCompleted)
			{
				continue;
			}
			bool flag = false;
			switch (tutorialTask.taskType)
			{
			case TutorialTask.TaskType.IncreaseCapitalLevel:
				localPlayer.blockTrainUnits = !(tutorialTask.isCompleted = type == ImprovementData.Type.City && level >= tutorialTask.targetValue);
				break;
			case TutorialTask.TaskType.TrainUnit:
				flag = num >= tutorialTask.targetValue;
				tutorialTask.isCompleted = flag;
				break;
			case TutorialTask.TaskType.TurnNumber:
				flag = currentTurn >= tutorialTask.targetValue;
				tutorialTask.isCompleted = flag;
				break;
			case TutorialTask.TaskType.NumberOfCities:
				flag = cities >= tutorialTask.targetValue;
				tutorialTask.isCompleted = flag;
				break;
			case TutorialTask.TaskType.HaveTechnology:
				flag = localPlayer.HasTech(tutorialTask.techType);
				if (!tutorialTask.isCompleted && flag)
				{
					HideTechView();
					foreach (PlayerState playerState2 in gameState.PlayerStates)
					{
						playerState2.blockTrainUnits = false;
					}
				}
				tutorialTask.isCompleted = flag;
				break;
			case TutorialTask.TaskType.MeetTribe:
				if ((tutorialTask.isCompleted = localPlayer.knownPlayers.Count >= tutorialTask.targetValue) && (Object)(object)hintIcon != (Object)null)
				{
					hintIcon.Hide();
				}
				break;
			}
			CheckButtons();
		}
		TutorialTask currentTask = tutorialTasks.FirstOrDefault((TutorialTask x) => !x.isCompleted);
		if (currentTask == null)
		{
			FinishTutorial();
		}
		else if (this.currentTask != currentTask || waitForPopup)
		{
			if (this.currentTask == null)
			{
				SetTaskData();
			}
			else
			{
				this.currentTask = currentTask;
				AnimateTaskPanel(SetTaskData);
			}
			firstCheck = false;
		}
		void SetTaskData()
		{
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			waitForPopup = false;
			this.currentTask = currentTask;
			string tutorialHeader = string.Format("{0} {1}/{2}", Localization.Get("gamemode.tutorial"), tutorialTasks.IndexOf(currentTask) + 1, tutorialTasks.Count);
			taskPanel.SetTutorialHeader(tutorialHeader);
			taskPanel.SetDescription("tutorial.task." + currentTask.descriptionKey);
			if (currentTask.taskType == TutorialTask.TaskType.TrainUnit)
			{
				if ((Object)(object)currentTask.sprite == (Object)null)
				{
					PlayerState localPlayer2 = GameManager.LocalPlayer;
					UnitData value = gameState.GameLogicData.units.FirstOrDefault((KeyValuePair<UnitData.Type, UnitData> x) => x.Key == currentTask.unitType).Value;
					taskPanel.SetIcon(value, localPlayer2);
				}
			}
			else if (currentTask.taskType == TutorialTask.TaskType.MeetTribe)
			{
				PlayerState playerState = gameState.PlayerStates.FirstOrDefault((PlayerState x) => x != GameManager.LocalPlayer);
				if ((Object)(object)currentTask.sprite == (Object)null)
				{
					UnitData value2 = gameState.GameLogicData.units.FirstOrDefault((KeyValuePair<UnitData.Type, UnitData> x) => x.Key == currentTask.unitType).Value;
					taskPanel.SetIcon(value2, playerState);
				}
				WorldCoordinates currentCapitalCoordinates2 = playerState.GetCurrentCapitalCoordinates(gameState);
				hintIcon = UIWorldIconContainer.GetHintIcon(currentCapitalCoordinates2);
				hintIcon.WorldOffset = new Vector3(0f, 0.2f, 0f);
				hintIcon.Callback = HintIcon_OnClicked;
				hintIcon.Type = HintIcon.IconTypes.Target;
				CameraController.Instance.CenterOnPosition(currentCapitalCoordinates2.ToPosition(), 0.8f, ShowHint);
			}
			else
			{
				taskPanel.SetIcon(currentTask.sprite);
			}
			switch (currentTask.taskType)
			{
			case TutorialTask.TaskType.Default:
				UIManager.Instance.SetTutorialCommandType(CommandType.None);
				break;
			case TutorialTask.TaskType.IncreaseCapitalLevel:
				UIManager.Instance.SetTutorialCommandType(CommandType.Build);
				break;
			case TutorialTask.TaskType.TrainUnit:
				UIManager.Instance.SetTutorialCommandType(CommandType.Train);
				break;
			case TutorialTask.TaskType.TurnNumber:
				UIManager.Instance.SetTutorialCommandType(CommandType.EndTurn);
				break;
			case TutorialTask.TaskType.NumberOfCities:
				UIManager.Instance.SetTutorialCommandType(CommandType.Move);
				break;
			case TutorialTask.TaskType.HaveTechnology:
				UIManager.Instance.SetTutorialCommandType(CommandType.Research);
				break;
			case TutorialTask.TaskType.MeetTribe:
				UIManager.Instance.SetTutorialCommandType(CommandType.None);
				break;
			default:
				UIManager.Instance.SetTutorialCommandType(CommandType.None);
				break;
			}
			taskPanel.Show();
		}
		void ShowHint()
		{
			hintIcon.Show();
		}
	}

	private void HintIcon_OnClicked()
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		GameState gameState = GameManager.GameState;
		IconPopup iconPopup = PopupManager.GetIconPopup();
		iconPopup.Header = Localization.Get("world.suggestion.title");
		iconPopup.Description = Localization.Get("tutorial.suggestion.description");
		iconPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		PlayerState playerState = gameState.PlayerStates.FirstOrDefault((PlayerState x) => x != GameManager.LocalPlayer);
		UIUnitRenderer uIUnitRenderer = UIUtils.GetUIUnitRenderer(gameState.GameLogicData.units.FirstOrDefault((KeyValuePair<UnitData.Type, UnitData> x) => x.Key == currentTask.unitType).Value, playerState);
		((Transform)uIUnitRenderer.rectTransform).SetParent((Transform)(object)iconPopup.iconContainer, false);
		UIUtils.FitImageContentInParent(uIUnitRenderer.rectTransform);
		iconPopup.Show(InputManager.GetInputPosition());
		if (Object.op_Implicit((Object)(object)hintIcon) && hintIcon.Coordinates != WorldCoordinates.NULL_COORDINATES)
		{
			CameraController.Instance.CenterOnPosition(hintIcon.Coordinates.ToPosition(), 0.5f);
		}
	}

	private void RewardPopup_OnDataSet(RewardPopup rewardPopup)
	{
		if (!nextTurnTask.isCompleted)
		{
			if (taskPanelAnimationSequence != null)
			{
				TweenExtensions.Kill((Tween)(object)taskPanelAnimationSequence, false);
			}
			waitForPopup = true;
			rewardPopup.SetRewards(GameManager.LocalPlayer, new CityReward[1] { CityReward.TutorialExplorer });
		}
	}

	private void TaskPanel_OnClicked(int id, BaseEventData eventData = null)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (currentTask != null)
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("tutorial.task." + currentTask.descriptionKey + ".title");
			basicPopup.Description = Localization.Get("tutorial.task." + currentTask.descriptionKey + ".description");
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup.Show(InputManager.GetInputPosition());
		}
	}

	private void TechView_OnItemsRefreshed(TechView techView)
	{
		if (!tutorialCompleted && !technologyTask.isCompleted)
		{
			techView.ForceBlockTechnologies(new List<TechData.Type> { technologyTask.techType });
		}
		else
		{
			techView.OnItemsRefreshed -= TechView_OnItemsRefreshed;
		}
	}
}
