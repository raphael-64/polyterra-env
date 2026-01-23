using DG.Tweening;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HudButtonBar : MonoBehaviour
{
	public CanvasGroup canvasGroup;

	public RectTransform rectTransform;

	public GameObject raycastBlocker;

	public HorizontalLayoutGroup layoutGroup;

	[Header("Buttons")]
	public UIRoundButton menuButton;

	public UIRoundButton statsButton;

	public UIRoundButton techTreeButton;

	public UIRoundButton nextTurnButton;

	[Header("Widgets")]
	public PositionDisplay positionDisplay;

	private int oldSelectedButton = -1;

	private UIRoundButton[] buttonArray;

	public bool blockRefreshingStatsButton;

	public bool blockRefreshingTechTreeButton;

	public bool blockRefreshingNextTurnButton;

	private HudScreen hudScreen;

	private bool isRightAligned;

	private bool forceToRight;

	public bool IsShowing { get; private set; }

	public void Init(HudScreen hudScreen)
	{
		this.hudScreen = hudScreen;
		statsButton.buttonActive = true;
		techTreeButton.buttonActive = true;
		buttonArray = new UIRoundButton[4] { menuButton, statsButton, techTreeButton, nextTurnButton };
		Show();
		Update();
	}

	public void DeInit()
	{
		statsButton.buttonActive = false;
		techTreeButton.buttonActive = false;
	}

	public void MoveSelectedButton(int offset)
	{
		if (oldSelectedButton < 0)
		{
			SetSelectedButton((offset > 0) ? (buttonArray.Length - 1) : 0);
		}
		else
		{
			SetSelectedButton((oldSelectedButton + buttonArray.Length + offset) % buttonArray.Length);
		}
	}

	public void SetSelectedButton(int selectedButton)
	{
		if (selectedButton >= 0 && selectedButton < buttonArray.Length)
		{
			EventSystem.current.SetSelectedGameObject(((Component)buttonArray[selectedButton]).gameObject);
			PolytopiaInput.Omnicursor.AffixToUIElement(buttonArray[selectedButton].rectTransform);
			UIManager.Instance.GetCurrentScreen().CurrentSelectable = null;
		}
		oldSelectedButton = selectedButton;
	}

	private void OnWaitingForServer(bool isWaiting)
	{
		RefreshNextTurnButton();
	}

	public bool IsRightAligned()
	{
		return isRightAligned;
	}

	private void Update()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (hudScreen.InteractionBar.type == InteractionBar.Type.PcBar)
		{
			Rect val = hudScreen.InteractionBar.rectTransform.WorldRect();
			Rect val2 = rectTransform.WorldRect();
			Rect val3 = menuButton.rectTransform.WorldRect();
			Rect val4 = nextTurnButton.rectTransform.WorldRect();
			float num = ((Rect)(ref val4)).xMax - ((Rect)(ref val3)).xMin;
			float num2 = ((Rect)(ref val2)).center.x - num / 2f;
			if ((!isRightAligned && ((Rect)(ref val)).xMax > num2 - 50f) || forceToRight)
			{
				RectOffset padding = ((LayoutGroup)layoutGroup).padding;
				padding.right = 30;
				((LayoutGroup)layoutGroup).padding = padding;
				((LayoutGroup)layoutGroup).childAlignment = (TextAnchor)2;
				isRightAligned = true;
				forceToRight = false;
			}
			else if (isRightAligned && ((Rect)(ref val)).xMax < num2 - 50f)
			{
				RectOffset padding2 = ((LayoutGroup)layoutGroup).padding;
				padding2.right = 0;
				((LayoutGroup)layoutGroup).padding = padding2;
				((LayoutGroup)layoutGroup).childAlignment = (TextAnchor)1;
				isRightAligned = false;
			}
		}
	}

	private void OnEnable()
	{
		GameEvents.OnMatchStart += OnMatchStart;
		GameEvents.OnMatchResumed += OnMatchResumed;
		GameEvents.OnStartedProcessing += OnStartedProcessing;
		GameEvents.OnFinishedProcessing += OnFinishedProcessing;
		GameEvents.OnRecapEnded += OnFinishedProcessing;
		GameEvents.OnReplayEnded += OnFinishedProcessing;
		GameEvents.OnWaitingForServer += OnWaitingForServer;
		GameEvents.OnPassPlayer += OnPassPlayer;
		GameEvents.OnTurnEnded += OnTurnEnded;
		UIEvents.OnForceRefreshHud += RefreshComponents;
		SettingsEvents.OnSettingsUpdated += OnSettingsUpdated;
		UIEvents.OnPopupStackChanged += OnPopupStackChanged;
		if (SettingsUtils.UseCompactUI)
		{
			StartListeningToInputEvents();
		}
		RefreshComponents();
	}

	private void OnDisable()
	{
		GameEvents.OnMatchStart -= OnMatchStart;
		GameEvents.OnMatchResumed -= OnMatchResumed;
		GameEvents.OnStartedProcessing -= OnStartedProcessing;
		GameEvents.OnFinishedProcessing -= OnFinishedProcessing;
		GameEvents.OnRecapEnded -= OnFinishedProcessing;
		GameEvents.OnReplayEnded -= OnFinishedProcessing;
		GameEvents.OnWaitingForServer -= OnWaitingForServer;
		GameEvents.OnPassPlayer -= OnPassPlayer;
		GameEvents.OnTurnEnded -= OnTurnEnded;
		UIEvents.OnForceRefreshHud -= RefreshComponents;
		SettingsEvents.OnSettingsUpdated -= OnSettingsUpdated;
		UIEvents.OnPopupStackChanged -= OnPopupStackChanged;
		if (SettingsUtils.UseCompactUI)
		{
			StopListeningToInputEvents();
		}
	}

	private void OnSettingsUpdated(SettingsUtils.SettingsType type)
	{
		if (type == SettingsUtils.SettingsType.UseCompactUI)
		{
			if (SettingsUtils.UseCompactUI)
			{
				StartListeningToInputEvents();
				return;
			}
			Hide();
			StopListeningToInputEvents();
		}
	}

	private void StartListeningToInputEvents()
	{
		InputEvents.OnTileSelected += OnTileSelected;
		InputEvents.OnBuildingSelected += OnBuildingSelected;
		InputEvents.OnUnitSelected += OnUnitSelected;
		InputEvents.OnSelectionCleared += OnSelectionCleared;
	}

	private void StopListeningToInputEvents()
	{
		InputEvents.OnTileSelected -= OnTileSelected;
		InputEvents.OnBuildingSelected -= OnBuildingSelected;
		InputEvents.OnUnitSelected -= OnUnitSelected;
		InputEvents.OnSelectionCleared -= OnSelectionCleared;
	}

	private void OnFinishedProcessing()
	{
		RefreshNextTurnButton();
		RefreshTechTreeButton();
	}

	private void OnStartedProcessing()
	{
		RefreshNextTurnButton();
	}

	private void OnSelectionCleared()
	{
		Show();
	}

	private void OnUnitSelected(Unit unit)
	{
		if ((Object)(object)unit != (Object)null)
		{
			Hide();
		}
	}

	private void OnBuildingSelected(Building building)
	{
		if ((Object)(object)building != (Object)null)
		{
			Hide();
		}
	}

	private void OnTileSelected(Tile tile)
	{
		if ((Object)(object)tile != (Object)null)
		{
			Hide();
		}
	}

	private void OnMatchStart()
	{
		RefreshComponents();
	}

	private void OnMatchResumed()
	{
		RefreshComponents();
	}

	private void OnPassPlayer()
	{
		RefreshComponents();
	}

	private void OnTurnEnded()
	{
		RefreshComponents();
	}

	public void OnStyleUpdated()
	{
		RefreshStatsButton();
	}

	public void RefreshComponents()
	{
		RefreshStatsButton();
		RefreshPositionDisplay();
		RefreshTechTreeButton();
		RefreshNextTurnButton();
	}

	private void RefreshStatsButton()
	{
		if (!blockRefreshingStatsButton && GameManager.LocalPlayer.GetTribeStyle(GameManager.GameState) > 0)
		{
			statsButton.iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandleCallback)
			{
				statsButton.SetFaceIcon(spriteHandleCallback.sprite);
			});
			statsButton.iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddresses(GameManager.GameState, GameManager.LocalPlayer));
		}
	}

	private void RefreshTechTreeButton()
	{
		if (blockRefreshingTechTreeButton)
		{
			return;
		}
		PlayerState localPlayer = GameManager.LocalPlayer;
		if (localPlayer == null)
		{
			return;
		}
		bool buttonActive = false;
		if (GameManager.GameState.GameLogicData.TryGetData(localPlayer.tribe, out var data))
		{
			foreach (TechData item in GameManager.GameState.GameLogicData.GetAllTechForTribe(data))
			{
				if (GameManager.GameState.GameLogicData.IsUnlockable(item.type, localPlayer) && localPlayer.CanAfford(GameManager.GameState, item))
				{
					buttonActive = true;
					break;
				}
			}
		}
		techTreeButton.buttonActive = buttonActive;
	}

	private void RefreshPositionDisplay()
	{
		positionDisplay.Init();
		if (blockRefreshingStatsButton)
		{
			((Component)positionDisplay).gameObject.SetActive(false);
		}
	}

	private void RefreshNextTurnButton()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		if (blockRefreshingNextTurnButton || GameManager.Client == null || GameManager.GameState == null)
		{
			return;
		}
		if (GameManager.Client.IsSpectating)
		{
			if (GameManager.Client.IsReplay && GameManager.GameState.TryGetWinner(out var _))
			{
				nextTurnButton.bgColors.activeColor = ColorConstants.green;
				nextTurnButton.buttonActive = true;
				nextTurnButton.Key = "world.turn.finish";
				nextTurnButton.SetSprite(UIManager.IconData.GetSprite("EndTurn"), nativeSize: false);
			}
			else
			{
				nextTurnButton.bgColors.activeColor = ColorConstants.blue;
				nextTurnButton.buttonActive = true;
				nextTurnButton.SetSprite(UIManager.IconData.GetSprite("Exit"), nativeSize: false);
				nextTurnButton.Key = "world.turn.exit";
			}
			return;
		}
		if (GameManager.Client.IsWaitingForCommand)
		{
			nextTurnButton.ButtonEnabled = false;
			((Selectable)nextTurnButton.button).interactable = false;
		}
		else
		{
			nextTurnButton.ButtonEnabled = true;
			((Selectable)nextTurnButton.button).interactable = true;
		}
		PlayerState winner2;
		if (!GameManager.IsPlayerLocal(GameManager.GameState.CurrentPlayer) && GameManager.GameState.CurrentState != GameState.State.Ended)
		{
			if (GameManager.GameState.Settings.GameType != GameType.SinglePlayer && GameManager.GameState.Settings.GameType != GameType.PassAndPlay)
			{
				nextTurnButton.buttonActive = true;
				nextTurnButton.SetSprite(UIManager.IconData.GetSprite("Exit"), nativeSize: false);
				nextTurnButton.Key = "world.turn.exit";
				return;
			}
			nextTurnButton.buttonActive = false;
		}
		else if (GameManager.GameState.TryGetWinner(out winner2))
		{
			nextTurnButton.bgColors.activeColor = ColorConstants.green;
			nextTurnButton.buttonActive = true;
			nextTurnButton.Key = "world.turn.finish";
			nextTurnButton.SetSprite(UIManager.IconData.GetSprite("EndTurn"), nativeSize: false);
			return;
		}
		nextTurnButton.Key = "world.turn.end";
		nextTurnButton.buttonActive = !GameManager.LocalPlayer.AnyUnitCanPerformAction(GameManager.GameState);
		nextTurnButton.SetSprite(UIManager.IconData.GetSprite("EndTurn"), nativeSize: false);
	}

	public void Show()
	{
		IsShowing = true;
		canvasGroup.DOFade(1f, 0.2f);
	}

	public void Hide()
	{
		IsShowing = false;
		canvasGroup.DOFade(0f, 0.2f);
	}

	private void OnPopupStackChanged()
	{
		raycastBlocker.SetActive(PopupManager.IsUnskippablePopupShowing());
	}
}
