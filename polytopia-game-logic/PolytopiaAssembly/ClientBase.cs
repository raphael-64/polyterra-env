using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polytopia.Data;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using UnityEngine;

public abstract class ClientBase
{
	private const string SAVE_TO_DISK_FAIL_POPUP_ID = "savetodiskfail";

	protected Guid gameId;

	protected GameState initialGameState;

	protected GameState currentGameState;

	protected GameState targetGameState;

	protected int targetGameStateCommandCount;

	protected bool isReady;

	protected bool isSessionEnded;

	public Action OnConnected;

	public Action OnDisconnected;

	public Action OnSessionOpened;

	public Action<StateUpdateReason> OnStateUpdated;

	public Action OnStartedProcessingActions;

	public Action OnFinishedProcessingActions;

	private int lastStateUpdateFrame = -1;

	private int stateUpdateCount;

	private const int MAX_STATE_UPDATES_PER_FRAME = 20;

	public virtual string LOG_PREFIX => "<color=#63d863>[CL]</color>";

	public ClientActionManager ActionManager { get; protected set; }

	public Guid? CurrentGameId => gameId;

	public virtual GameState GameState => currentGameState;

	public virtual bool IsReady => isReady;

	public virtual bool IsRecap
	{
		get
		{
			if (ActionManager == null)
			{
				return false;
			}
			return ActionManager.IsRecap;
		}
	}

	public virtual bool IsReplay => false;

	public virtual bool IsSpectating => false;

	public virtual bool HasLocalAI => true;

	public virtual bool IsWaitingForCommand
	{
		get
		{
			if (ActionManager != null && ActionManager.IsProcessing)
			{
				return !ActionManager.IsWaitingForCommandTrigger(GetCurrentLocalPlayer().Id);
			}
			return false;
		}
	}

	protected virtual GameState RecapGameState => initialGameState;

	protected ClientBase()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		DebugConsole.AddCommand("cl_savestate", new CommandDelegate(CmdSaveState), "Dump the current state to disk");
		DebugConsole.AddCommand("cl_tocommand", new CommandDelegate(CmdGoToCommand), "Go to a specific command in the state timeline");
		DebugConsole.AddCommand("cl_history", new CommandDelegate(CmdCommandHistory), "Print the command history");
		DebugConsole.AddCommand("cl_forcestate", new CommandDelegate(CmdForceStateUpdate), "Force a state update for debug purposes");
		Log.Info("{0} {1} ready", new object[2]
		{
			LOG_PREFIX,
			GetType()
		});
	}

	public virtual void Reset()
	{
		isReady = false;
		isSessionEnded = false;
		initialGameState = null;
		currentGameState = null;
		targetGameState = null;
		if (ActionManager != null)
		{
			ActionManager.OnStartedProcessing = null;
			ActionManager.OnFinishedProcessing = null;
			ActionManager.OnStartedRecap = null;
			ActionManager.ShouldInterruptCommand = null;
			ActionManager.OnInterruptCommand = null;
		}
		ActionManager = null;
	}

	public virtual void ResetGameState(StateUpdateReason reason)
	{
	}

	public virtual void Update()
	{
	}

	public abstract void Connect(Uri endpoint);

	public abstract void Disconnect();

	public abstract Task<CreateSessionResult> CreateSession(GameSettings settings, List<PlayerState> players);

	public abstract Task<bool> OpenSession(Guid gameId);

	public abstract void SaveSession(Guid gameId, bool showSaveErrorPopup = false);

	public abstract Guid[] GetSessions(long playerId);

	protected abstract void PrepareSession();

	public abstract void EndSession();

	public abstract bool IsPlayerLocal(byte playerId);

	public abstract PlayerState GetCurrentLocalPlayer();

	public abstract Task<bool> PickTribe(TribeData.Type tribeType, List<TribeData.Type> disabledTribes = null, SkinType skinType = SkinType.Default);

	public virtual Task UploadHighscore()
	{
		return Task.CompletedTask;
	}

	public abstract Task SendCommand(CommandBase command);

	public abstract Task<bool> ReceiveCommand(List<CommandBase> commands);

	public abstract ushort GetLastSeenCommand();

	public abstract void SetLastSeenCommand(ushort commandIndex);

	public virtual bool HasQueuedActions()
	{
		return ActionManager.IsProcessing;
	}

	protected virtual void SessionOpened()
	{
		isReady = true;
		OnSessionOpened?.Invoke();
	}

	protected virtual void OnStartedProcessing()
	{
		OnStartedProcessingActions?.Invoke();
	}

	protected virtual void OnFinishedProcessing()
	{
		if (ShouldApplyTargetState())
		{
			ApplyTargetState();
		}
		OnFinishedProcessingActions?.Invoke();
	}

	protected virtual void OnStartedReplay()
	{
	}

	protected virtual bool ShouldInterruptCommand()
	{
		return ShouldApplyTargetState();
	}

	protected virtual void OnInterruptCommand()
	{
		ApplyTargetState();
		ActionManager.Resume();
	}

	protected void StateUpdated(StateUpdateReason reason)
	{
		Log.Verbose("{0} State updated", new object[1] { LOG_PREFIX });
		OnStateUpdated?.Invoke(reason);
	}

	public bool HasTargetState()
	{
		return targetGameState != null;
	}

	public void ClearTargetState()
	{
		targetGameState = null;
	}

	public bool ShouldApplyTargetState()
	{
		if (HasTargetState())
		{
			return ActionManager.LastSeenCommand == targetGameStateCommandCount;
		}
		return false;
	}

	public void ApplyTargetState()
	{
		if (targetGameState != null)
		{
			targetGameState.CommandStack = GameState.CommandStack;
			if (targetGameState.CurrentState == GameState.State.Unknown)
			{
				targetGameState.CurrentState = GameState.CurrentState;
			}
			GameState gameState = targetGameState;
			int val = targetGameStateCommandCount;
			targetGameState = null;
			UpdateGameStateImmediate(SerializationHelpers.ToByteArray(gameState, gameState.Version), StateUpdateReason.ReplayFinished);
			ushort lastSeenCommand = (ushort)Math.Max(0, val);
			SetLastSeenCommand(lastSeenCommand);
			Log.Verbose("Setting target state. Should continue {0}", new object[1] { ActionManager.LastSeenCommand != gameState.CommandStack.Count });
		}
	}

	public void UpdateGameState(byte[] serializedGameState, StateUpdateReason reason)
	{
		SerializationHelpers.FromByteArray<GameState>(serializedGameState, out var result);
		UpdateGameState(result, reason);
	}

	public void UpdateGameState(GameState gameState, StateUpdateReason reason)
	{
		Log.Verbose("{0} Updating GameState... (Reason: {1})", new object[2] { LOG_PREFIX, reason });
		if (ActionManager != null && ActionManager.IsProcessing)
		{
			Log.Verbose("{0} State is processing, aborting action manager {0}...", new object[2]
			{
				LOG_PREFIX,
				GetHashCode()
			});
			ActionManager.AbortExecution(delegate
			{
				if (ActionManager.shouldResetAfterAbort)
				{
					ActionManager.shouldResetAfterAbort = false;
					Log.Verbose("ActionManager has been touched after abort, resetting state {0}", new object[1] { GetHashCode() });
					ResetGameState(StateUpdateReason.Unknown);
				}
				else
				{
					UpdateGameStateImmediate(gameState, reason);
				}
			});
		}
		else
		{
			UpdateGameStateImmediate(gameState, reason);
		}
	}

	public void UpdateGameStateImmediate(byte[] serializedGameState, StateUpdateReason reason)
	{
		SerializationHelpers.FromByteArray<GameState>(serializedGameState, out var result);
		UpdateGameStateImmediate(result, reason);
	}

	public void UpdateGameStateImmediate(GameState gameState, StateUpdateReason reason)
	{
		if (currentGameState != null && currentGameState.Seed != gameState.Seed)
		{
			Log.Error("Trying to update game state with state from a different game", Array.Empty<object>());
			return;
		}
		if (lastStateUpdateFrame != Time.frameCount)
		{
			lastStateUpdateFrame = Time.frameCount;
			stateUpdateCount = 0;
		}
		stateUpdateCount++;
		if (stateUpdateCount == 20)
		{
			return;
		}
		if (reason == StateUpdateReason.GameCreated || reason == StateUpdateReason.GameJoined)
		{
			AnalyticsManager.SetCrashMetaData("last_opened_game_frame", Time.frameCount.ToString());
		}
		AnalyticsManager.SetCrashMetaData("game_mode", gameState.Settings.BaseGameMode.ToString());
		AnalyticsManager.SetCrashMetaData("game_type", gameState.Settings.GameType.ToString());
		Log.Verbose("{0} Updating GameState... (Reason: {1})", new object[2] { LOG_PREFIX, reason });
		if (ActionManager != null && ActionManager.IsProcessing)
		{
			throw new Exception($"Can't do an immediate game state update while the actionmanager is processing, replaying {ActionManager.IsRecap}");
		}
		Log.Verbose("New gamestate has {0} pending actions and {1} pending command triggers", new object[2]
		{
			gameState.ActionStack?.Count,
			gameState.pendingCommandTriggers?.Count
		});
		currentGameState = gameState;
		ushort num = GetLastSeenCommand();
		switch (reason)
		{
		case StateUpdateReason.PlayerResigned:
		case StateUpdateReason.PlayerKicked:
		case StateUpdateReason.ValidCommand:
			if (ActionManager != null)
			{
				num = ActionManager.LastSeenCommand;
			}
			break;
		case StateUpdateReason.InvalidCommand:
		case StateUpdateReason.StateReset:
			num = gameState.LastProcessedCommand;
			PopupManager.RemoveAllPopups();
			break;
		case StateUpdateReason.GameJoined:
		case StateUpdateReason.ReplayFinished:
			num = gameState.LastProcessedCommand;
			break;
		}
		if (num > gameState.LastProcessedCommand)
		{
			num = gameState.LastProcessedCommand;
		}
		CreateOrResetActionManager(num);
		if (num < gameState.LastProcessedCommand && RecapGameState != null)
		{
			RewindToCommand(num, delegate(bool success)
			{
				if (success)
				{
					StateUpdated(reason);
					Log.Verbose("{0} State updated: {1}, position {2}/{3} (GameState: {4}, ActionManager: {5})", new object[6]
					{
						LOG_PREFIX,
						reason,
						ActionManager.LastSeenCommand,
						GameState.LastProcessedCommand,
						GameState.GetHashCode(),
						ActionManager.GetHashCode()
					});
				}
			});
		}
		else
		{
			StateUpdated(reason);
			Log.Verbose("{0} State updated: {1}, position {2}/{3} (GameState: {4}, ActionManager: {5})", new object[6]
			{
				LOG_PREFIX,
				reason,
				ActionManager.LastSeenCommand,
				GameState.LastProcessedCommand,
				GameState.GetHashCode(),
				ActionManager.GetHashCode()
			});
		}
	}

	public void RewindToCommand(ushort commandIndex, Action<bool> onComplete)
	{
		if (ActionManager.isAborting)
		{
			Log.Error("Cannot rewind while the action manager is aborting", Array.Empty<object>());
			onComplete?.Invoke(obj: false);
		}
		else if (ActionManager.IsProcessing)
		{
			Log.Verbose("{0} State is processing, aborting action manager {0}...", new object[2]
			{
				LOG_PREFIX,
				GetHashCode()
			});
			ActionManager.AbortExecution(delegate
			{
				if (ActionManager.shouldResetAfterAbort)
				{
					ActionManager.shouldResetAfterAbort = false;
					Log.Verbose("ActionManager has been touched after abort, resetting state {0}", new object[1] { GetHashCode() });
					ResetGameState(StateUpdateReason.Unknown);
					onComplete?.Invoke(obj: false);
				}
				else
				{
					RewindToCommandImmediate(commandIndex, onComplete);
				}
			});
		}
		else
		{
			RewindToCommandImmediate(commandIndex, onComplete);
		}
	}

	private void RewindToCommandImmediate(ushort commandIndex, Action<bool> onComplete)
	{
		Log.Info("{0} Rewinding to command {1}, (last processed command {2}, command stack count {3}, lastseencommand {4}, actionmanager lastseencommand {5})", new object[6]
		{
			LOG_PREFIX,
			commandIndex,
			currentGameState.LastProcessedCommand,
			currentGameState.CommandStack.Count,
			GetLastSeenCommand(),
			ActionManager.LastSeenCommand
		});
		PopupManager.RemoveAllPopups();
		InputManager.DisableInput(InputManager.GameInputs);
		if (HasTargetState())
		{
			ApplyTargetState();
		}
		GameState.State currentState = currentGameState.CurrentState;
		targetGameState = null;
		try
		{
			if (ActionManagerUtils.PerformAllQueuedActions(ActionManager.GameState))
			{
				SerializationHelpers.FromByteArray<GameState>(SerializationHelpers.ToByteArray(currentGameState, currentGameState.Version), out targetGameState);
				targetGameStateCommandCount = currentGameState.LastProcessedCommand;
			}
		}
		catch (Exception ex)
		{
			Log.Error("Failed to save target state {0}", new object[1] { ex });
			targetGameState = null;
		}
		SerializationHelpers.FromByteArray<GameState>(SerializationHelpers.ToByteArray(RecapGameState, RecapGameState.Version), out var result);
		byte[] commandStack = GameState.GetCommandStack();
		result.LoadCommandStack(commandStack);
		currentGameState = result;
		currentGameState.CurrentState = currentState;
		CreateOrResetActionManager(RecapGameState.LastProcessedCommand);
		GameManager.Simulate(ActionManager, commandIndex, delegate
		{
			if (ShouldApplyTargetState())
			{
				ApplyTargetState();
			}
			InputManager.EnableInput(InputManager.GameInputs);
			onComplete?.Invoke(obj: true);
		});
	}

	private void CreateOrResetActionManager(ushort lastSeenCommand)
	{
		if (ActionManager == null)
		{
			ActionManager = new ClientActionManager(GameState);
			ActionManager.OnFinishedProcessing = OnFinishedProcessing;
			ActionManager.OnStartedProcessing = OnStartedProcessing;
			ActionManager.OnInterruptCommand = OnInterruptCommand;
			ActionManager.ShouldInterruptCommand = ShouldInterruptCommand;
		}
		else
		{
			ActionManager.Reset(GameState);
		}
		ActionManager.LastSeenCommand = lastSeenCommand;
	}

	private void CmdSaveState(string[] args)
	{
		SaveSession(gameId);
	}

	private void CmdCommandHistory(string[] args)
	{
		if (GameState == null || GameState.CommandStack == null)
		{
			return;
		}
		for (int i = 0; i < GameState.CommandStack.Count; i++)
		{
			Log.Info("CMD{0:000}: {1}", new object[2]
			{
				i,
				GameState.CommandStack[i]
			});
		}
		if (GameState.ActionStack != null)
		{
			for (int j = 0; j < GameState.ActionStack.Count; j++)
			{
				Log.Info("ACT{0:000}: {1}", new object[2]
				{
					j,
					GameState.ActionStack[j]
				});
			}
		}
	}

	private void CmdGoToCommand(string[] args)
	{
		if (args == null || args.Length == 0)
		{
			DebugConsole.Write("Insufficient parameters, please enter a command to run from, eg: cl_replayfrom 0", Array.Empty<object>());
		}
		else
		{
			if (!ushort.TryParse(args[0], out var result))
			{
				return;
			}
			RewindToCommand(result, delegate(bool success)
			{
				if (success)
				{
					MapRenderer.Current.Clear();
					MapRenderer.Current.RenderMap(GameState.Map);
					ResourceEvents.RefreshWallets(GameManager.LocalPlayer.Id);
					ActionManager.Resume();
				}
			});
		}
	}

	public void CmdForceStateUpdate(string[] args)
	{
		Log.Info("Forcing state update at last seen command: {0}", new object[1] { ActionManager.LastSeenCommand });
		byte[] serializedGameState = SerializationHelpers.ToByteArray(GameState, GameState.Version);
		UpdateGameState(serializedGameState, StateUpdateReason.InvalidCommand);
	}

	private void CmdSendCommand(string[] args)
	{
		ReceiveCommand(new List<CommandBase>
		{
			new RecoverCommand(GameManager.LocalPlayer.Id, WorldCoordinates.NULL_COORDINATES)
		});
		PolytopiaBackendAdapter.Instance.SendCommand(new SendCommandBindingModel
		{
			Command = new PolytopiaCommandViewModel(CommandBase.ToByteArray(new RecoverCommand(GameManager.LocalPlayer.Id, WorldCoordinates.NULL_COORDINATES), currentGameState.Version)),
			GameId = CurrentGameId.Value
		});
	}

	protected void TriggerSaveToDiskFailedPopup(Action retryAction)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		if (!PopupManager.IsPopupShowing<BasicPopup>("savetodiskfail"))
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.identifier = "savetodiskfail";
			basicPopup.Header = Localization.Get("gamesaverbinary.unable.to.save.title");
			basicPopup.Description = Localization.Get("gamesaverbinary.unable.to.save");
			basicPopup.buttonData = new PopupBase.PopupButtonData[2]
			{
				new PopupBase.PopupButtonData(Localization.Get("buttons.exitgame"), PopupBase.PopupButtonData.States.None, delegate
				{
					GameManager.ReturnToMenu();
				}),
				new PopupBase.PopupButtonData(Localization.Get("buttons.tryagain"), PopupBase.PopupButtonData.States.Selected, delegate
				{
					basicPopup.Hide();
					retryAction?.Invoke();
				}, -1, closesPopup: false)
			};
			basicPopup.Buttons[0].BgColorStates.defaultColor = ColorConstants.red;
			basicPopup.Buttons[0].LabelColorStates.defaultColor = Color.white;
			basicPopup.IsUnskippable = true;
			basicPopup.Show();
		}
	}

	public virtual Task<ServerResponse<ResponseViewModel>> ResignCurrentGameAsync()
	{
		return null;
	}
}
