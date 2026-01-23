using System;
using System.Collections;
using System.Collections.Generic;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using UnityEngine;

public class AdvisorManager : MonoBehaviour
{
	public const string LOG_PREFIX = "<color=#B8FF00>[AM]</color>";

	protected const int suggestionLimit = 2;

	[SerializeField]
	[Info]
	protected string info = "None";

	protected Coroutine checkDelay;

	protected Dictionary<CommandType, int> completedSuggestions = new Dictionary<CommandType, int>();

	protected HintIcon hintIcon;

	protected CommandBase currentCommand;

	private bool isListeningToGameEvents;

	protected WorldCoordinates blockedCoordinates;

	protected bool areHintsBlocked;

	protected CommandType tutorialCommandType;

	private void OnEnable()
	{
		if (SettingsUtils.Suggestions)
		{
			ListenForGameEvents();
		}
		SettingsEvents.OnSettingsUpdated += OnSettingsUpdated;
	}

	private void OnSettingsUpdated(SettingsUtils.SettingsType type)
	{
		if (type == SettingsUtils.SettingsType.Suggestions)
		{
			if (SettingsUtils.Suggestions)
			{
				ListenForGameEvents();
				RestartChecker();
			}
			else
			{
				StopListenForGameEvents();
			}
		}
	}

	private void OnDisable()
	{
		StopListenForGameEvents();
		SettingsEvents.OnSettingsUpdated -= OnSettingsUpdated;
	}

	protected void ListenForGameEvents()
	{
		if (!isListeningToGameEvents)
		{
			isListeningToGameEvents = true;
			GameEvents.OnStateUpdated += OnStateUpdated;
			GameEvents.OnMatchStart += OnMatchStart;
			GameEvents.OnMatchResumed += OnMatchResumed;
			GameEvents.OnPassPlayer += OnPassPlayer;
			GameEvents.OnTurnEnded += OnTurnEnded;
			GameEvents.OnTurnStarted += OnTurnStarted;
			GameEvents.OnSessionEnded += OnSessionEnded;
			CommandEvents.OnCommandExecuted += OnCommandExecuted;
			UIEvents.OnQuickActionsOpen += OnQuickActionsOpen;
		}
	}

	protected void StopListenForGameEvents()
	{
		if (isListeningToGameEvents)
		{
			isListeningToGameEvents = false;
			HideHintIcon();
			KillCoroutine();
			UpdateInfo();
			GameEvents.OnStateUpdated -= OnStateUpdated;
			GameEvents.OnMatchStart -= OnMatchStart;
			GameEvents.OnMatchResumed -= OnMatchResumed;
			GameEvents.OnPassPlayer -= OnPassPlayer;
			GameEvents.OnTurnEnded -= OnTurnEnded;
			GameEvents.OnTurnStarted -= OnTurnStarted;
			GameEvents.OnSessionEnded -= OnSessionEnded;
			CommandEvents.OnCommandExecuted -= OnCommandExecuted;
			UIEvents.OnQuickActionsOpen -= OnQuickActionsOpen;
		}
	}

	protected void OnMatchStart()
	{
		RestartChecker();
	}

	protected void OnMatchResumed()
	{
		RestartChecker();
	}

	protected void OnStateUpdated()
	{
		RestartChecker();
	}

	private void OnTurnEnded()
	{
		PauseChecker();
	}

	private void OnTurnStarted()
	{
		RestartChecker();
	}

	private void OnPassPlayer()
	{
		RestartChecker();
	}

	private void OnSessionEnded()
	{
		PauseChecker();
	}

	private void OnCommandExecuted(CommandBase command)
	{
		if (GameManager.IsPlayerLocal(command.PlayerId))
		{
			CompleteSuggestion(command.GetCommandType());
		}
	}

	private void OnQuickActionsOpen(WorldCoordinates coordinates, bool open)
	{
		blockedCoordinates = (open ? coordinates : WorldCoordinates.NULL_COORDINATES);
		if (currentCommand != null && GetCoordinates(currentCommand) != WorldCoordinates.NULL_COORDINATES && GetCoordinates(currentCommand) == coordinates)
		{
			if (open)
			{
				HideHintIcon();
			}
			else if (currentCommand.IsValid(GameManager.GameState) && CanShowSuggestionForCommand(currentCommand))
			{
				ShowHintIcon();
			}
		}
	}

	protected void PauseChecker()
	{
		HideHintIcon();
		KillCoroutine();
	}

	protected void RestartChecker()
	{
		if (SettingsUtils.Suggestions)
		{
			HideHintIcon();
			KillCoroutine();
			if (GameManager.IsPlayerViewing(GameManager.GameState.CurrentPlayer) && (Object)(object)hintIcon == (Object)null)
			{
				checkDelay = ((MonoBehaviour)this).StartCoroutine(DelayCheckSuggestions(3f));
			}
			UpdateInfo();
		}
	}

	protected IEnumerator DelayCheckSuggestions(float delay = 0f)
	{
		yield return (object)new WaitForSeconds(delay);
		CheckSuggestions();
	}

	protected void CheckSuggestions()
	{
		if ((Object)(object)this == (Object)null || areHintsBlocked || GameManager.Client.IsSpectating)
		{
			return;
		}
		Log.Verbose("Checking for suggestions...", Array.Empty<object>());
		checkDelay = null;
		if (GameManager.GameState.Settings.RulesGameMode == GameMode.Tutorial)
		{
			currentCommand = AI.GetMove(GameManager.GameState, GameManager.LocalPlayer, tutorialCommandType);
			if (tutorialCommandType == CommandType.Research)
			{
				currentCommand = new ResearchCommand(GameManager.LocalPlayer.Id, TechData.Type.Hunting);
			}
		}
		else
		{
			currentCommand = AI.GetMove(GameManager.GameState, GameManager.LocalPlayer);
		}
		if (currentCommand != null && CanShowSuggestionForCommand(currentCommand))
		{
			ShowHintIcon();
		}
		UpdateInfo();
	}

	protected void ShowHintIcon()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		switch (currentCommand.GetCommandType())
		{
		case CommandType.Research:
			hintIcon = UIWorldIconContainer.GetHintIcon((UIManager.Instance.GetScreen(UIConstants.Screens.Hud) as HudScreen).buttonBar.techTreeButton.rectTransform);
			hintIcon.IconPosOffset = new Vector2(0f, 36f);
			break;
		case CommandType.EndTurn:
			hintIcon = UIWorldIconContainer.GetHintIcon((UIManager.Instance.GetScreen(UIConstants.Screens.Hud) as HudScreen).buttonBar.nextTurnButton.rectTransform);
			hintIcon.IconPosOffset = new Vector2(0f, 36f);
			break;
		default:
			hintIcon = UIWorldIconContainer.GetHintIcon(GetCoordinates(currentCommand));
			hintIcon.WorldOffset = new Vector3(0f, 0.2f, 0f);
			break;
		}
		hintIcon.Callback = HintClicked;
		hintIcon.Type = HintIcon.IconTypes.Suggestion;
		hintIcon.Show();
		AudioManager.PlaySFX(SFXTypes.Suggestion);
		UpdateInfo();
	}

	protected void HideHintIcon()
	{
		if ((Object)(object)hintIcon != (Object)null)
		{
			hintIcon.Hide();
			hintIcon = null;
			UpdateInfo();
		}
	}

	protected void KillCoroutine()
	{
		if (checkDelay != null)
		{
			((MonoBehaviour)this).StopCoroutine(checkDelay);
			checkDelay = null;
			UpdateInfo();
		}
	}

	protected void HintClicked()
	{
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		if (currentCommand == null)
		{
			return;
		}
		IconPopup iconPopup = PopupManager.GetIconPopup();
		iconPopup.Header = Localization.Get("world.suggestion.title");
		iconPopup.Description = string.Format("{0}\n{1}", Localization.Get("world.suggestion.message", currentCommand.GetInfo()), Localization.Get("world.suggestion.disable"));
		iconPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		switch (currentCommand.GetCommandType())
		{
		case CommandType.Build:
			iconPopup.spriteHandle.Request(SpriteData.GetBuildingSpriteAddresses((currentCommand as BuildCommand).Type, GameManager.LocalPlayer.skinType, GameManager.LocalPlayer.GetTribeData(GameManager.GameState).climate));
			break;
		case CommandType.Attack:
			iconPopup.sprite = UIManager.IconData.GetSprite("AttackTarget");
			break;
		case CommandType.Train:
		{
			TrainCommand trainCommand = currentCommand as TrainCommand;
			if (GameManager.GameState.GameLogicData.TryGetData(trainCommand.Type, out var data))
			{
				UIUnitRenderer uIUnitRenderer = UIUtils.GetUIUnitRenderer(data, GameManager.LocalPlayer);
				((Transform)uIUnitRenderer.rectTransform).SetParent((Transform)(object)iconPopup.iconContainer, false);
				UIUtils.FitImageContentInParent(uIUnitRenderer.rectTransform);
			}
			break;
		}
		case CommandType.Move:
			iconPopup.sprite = UIManager.IconData.GetSprite("MoveTarget");
			break;
		case CommandType.Research:
			iconPopup.sprite = UIManager.IconData.GetSprite("Tech");
			break;
		case CommandType.EndTurn:
			iconPopup.sprite = UIManager.IconData.GetSprite("EndTurn");
			break;
		}
		iconPopup.Show(InputManager.GetInputPosition());
		if (Object.op_Implicit((Object)(object)hintIcon) && hintIcon.Coordinates != WorldCoordinates.NULL_COORDINATES)
		{
			CameraController.Instance.CenterOnPosition(hintIcon.Coordinates.ToPosition(), 0.5f);
		}
		currentCommand = null;
		hintIcon = null;
		UpdateInfo();
	}

	protected void CompleteSuggestion(CommandType type)
	{
		if (IsSuggestableAction(type) && SettingsUtils.Suggestions)
		{
			bool flag = false;
			if (!completedSuggestions.ContainsKey(type))
			{
				completedSuggestions.Add(type, 0);
				flag = true;
			}
			else if (completedSuggestions[type] <= 2)
			{
				completedSuggestions[type]++;
				flag = true;
			}
			if (flag)
			{
				UpdateInfo();
			}
		}
	}

	protected bool CanShowSuggestionForCommand(CommandBase command)
	{
		if (blockedCoordinates != WorldCoordinates.NULL_COORDINATES && GetCoordinates(command) == blockedCoordinates)
		{
			return false;
		}
		if (GameManager.Client.ActionManager.IsProcessing)
		{
			return false;
		}
		CommandType commandType = command.GetCommandType();
		if (completedSuggestions.ContainsKey(commandType))
		{
			return completedSuggestions[commandType] <= 2;
		}
		return IsSuggestableAction(commandType);
	}

	protected bool IsSuggestableAction(CommandType type)
	{
		if (type != CommandType.Move && type != CommandType.Attack && type != CommandType.Research && type != CommandType.Train && type != CommandType.EndTurn)
		{
			return type == CommandType.Build;
		}
		return true;
	}

	protected WorldCoordinates GetCoordinates(CommandBase command)
	{
		return command.GetCommandType() switch
		{
			CommandType.Build => (command as BuildCommand).Coordinates, 
			CommandType.Attack => (command as AttackCommand).Origin, 
			CommandType.Train => (command as TrainCommand).Coordinates, 
			CommandType.Move => (command as MoveCommand).From, 
			_ => WorldCoordinates.NULL_COORDINATES, 
		};
	}

	protected void UpdateInfo()
	{
	}

	public void BlockHints()
	{
		areHintsBlocked = true;
		HideHintIcon();
	}

	public void UnblockHints()
	{
		areHintsBlocked = false;
		RestartChecker();
	}

	public void SetTutorialCommandType(CommandType commandType)
	{
		tutorialCommandType = commandType;
	}
}
