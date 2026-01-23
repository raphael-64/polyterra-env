using System;
using System.Collections.Generic;
using PolytopiaBackendBase.Game;
using UnityEngine;

public class ClientActionManager : ActionManager
{
	public const int SIMULATION_STEPS_PER_FRAME = 100;

	protected Queue<ReactionBase> reactionQueue = new Queue<ReactionBase>();

	protected ReactionBase currentReaction;

	protected bool isPaused;

	protected bool isReacting;

	protected bool isRecapping;

	protected bool isSimulating;

	protected bool isHolding;

	protected override string LOG_PREFIX => "<color=#63d863>[CAM]</color>";

	public ushort LastSeenCommand { get; set; }

	public bool IsRecap => isRecapping;

	public override bool IsProcessing
	{
		get
		{
			if (base.IsProcessing || IsRecap)
			{
				return !IsPaused;
			}
			return false;
		}
	}

	public bool IsSimulating
	{
		get
		{
			return isSimulating;
		}
		set
		{
			isSimulating = value;
		}
	}

	public bool IsPaused => isPaused;

	public ClientActionManager(GameState state)
		: base(state)
	{
	}

	public override void Reset(GameState state)
	{
		reactionQueue.Clear();
		base.Reset(state);
	}

	public static bool CanExecuteCommand(CommandBase command, GameState gameState)
	{
		if (!GameManager.Client.IsWaitingForCommand)
		{
			return command.IsValid(gameState);
		}
		return false;
	}

	private void ExecuteReaction(ReactionBase reaction)
	{
		reaction.Execute(delegate
		{
			Log.Spam("Ending reaction {0}, (frame {1})", new object[2]
			{
				reaction,
				Time.frameCount
			});
			if (currentReaction != reaction)
			{
				Log.Error("Mismatching reaction. Expected {0}, was {1}", new object[2] { currentReaction, reaction });
			}
			else
			{
				currentReaction = null;
				Update();
			}
		});
	}

	protected override void Update()
	{
		try
		{
			if (isAborting)
			{
				if (IsRecap)
				{
					EndRecap();
				}
				OnExecutionAborted();
			}
			else
			{
				if (isPaused || isSimulating || isHolding)
				{
					return;
				}
				if (currentReaction != null)
				{
					Log.Info("Reaction {0} still playing, update aborted", new object[1] { currentReaction });
					return;
				}
				bool flag = IsWaitingForCommandTrigger(gameState.CurrentPlayer) && ActionManager.USE_COMMAND_TRIGGER;
				if (reactionQueue != null && reactionQueue.Count > 0)
				{
					if (currentReaction == null)
					{
						ReactionBase reaction = reactionQueue.Dequeue();
						if (HotSeatOverlay.IsActive && !IsRecap && !(reaction is PassPlayerReaction) && !(reaction is EndMatchReaction))
						{
							Update();
							return;
						}
						currentReaction = reaction;
						Log.Spam("{0} {2,-24}\t: {1} ({3}) frame (frame {4})", new object[5]
						{
							LOG_PREFIX,
							reaction,
							string.Format("{0}Executing reaction", IsRecap ? "(R) " : ""),
							reactionQueue.Count + 1,
							Time.frameCount
						});
						if (reaction.ShouldFocusCamera())
						{
							ReactionUtils.CameraFocusIfExplored(GameManager.LocalPlayer.Id, reaction.GetCameraFocusCoordinates(), reaction.ShouldNudgeToCenter(), reaction.GetCameraFocusSpeed(), delegate
							{
								ExecuteReaction(reaction);
							});
						}
						else
						{
							ExecuteReaction(reaction);
						}
					}
					else
					{
						ReactionBase reactionBase = reactionQueue.Peek();
						Log.Error("tried to start {2} while {0} still playing (frame {1})", new object[3]
						{
							currentReaction,
							Time.frameCount,
							reactionBase
						});
					}
					return;
				}
				if (gameState.ActionStack != null && gameState.ActionStack.Count > 0 && !gameState.WaitForCommand && !flag)
				{
					int num = gameState.ActionStack.Count - 1;
					ActionBase actionBase = gameState.ActionStack[num];
					if (actionBase.IsValid(gameState))
					{
						Log.Spam("{0} {2,-24}\t: {1} ({3})", new object[4]
						{
							LOG_PREFIX,
							actionBase,
							string.Format("{0}Executing action", IsRecap ? "(R) " : ""),
							num + 1
						});
						actionBase.Execute(gameState);
						if (GameManager.debugShouldSkipReactions && !IsRecap)
						{
							MapRenderer.Current.Refresh();
							ResourceEvents.RefreshWallets(GameManager.LocalPlayer.Id);
						}
						else
						{
							ReactionBase reaction2 = actionBase.GetReaction();
							if (reaction2 != null)
							{
								reactionQueue.Enqueue(reaction2);
							}
						}
					}
					else
					{
						Log.Spam("{0} {2,-24}\t: {1} ({3})", new object[4]
						{
							LOG_PREFIX,
							actionBase,
							string.Format("{0}Action is invalid", IsRecap ? "(R) " : ""),
							num + 1
						});
					}
					gameState.ActionStack.RemoveAt(num);
					Update();
					return;
				}
				bool flag2 = LastSeenCommand < gameState.CommandStack.Count;
				if ((flag2 || flag) && ShouldInterruptCommand != null && ShouldInterruptCommand())
				{
					if (isRecapping)
					{
						EndRecap();
					}
					StopProcessing();
					OnInterruptCommand?.Invoke();
				}
				else if (flag2)
				{
					CommandBase commandBase = gameState.CommandStack[LastSeenCommand++];
					if (LastSeenCommand > gameState.LastProcessedCommand)
					{
						gameState.LastProcessedCommand = LastSeenCommand;
					}
					Log.Spam("{0} {2,-24}\t: {1} ({3})", new object[4]
					{
						LOG_PREFIX,
						commandBase,
						string.Format("{0}Executing command", IsRecap ? "(R) " : ""),
						gameState.CommandStack.Count - LastSeenCommand + 1
					});
					AnalyticsManager.SetCrashMetaData("last_executed_command", gameState.LastProcessedCommand.ToString());
					AnalyticsManager.SetCrashMetaData("last_executed_command_frame", Time.frameCount.ToString());
					commandBase.Execute(gameState);
					GameManager.Client.SetLastSeenCommand(LastSeenCommand);
					GameEvents.CommandExecuted();
					Update();
				}
				else
				{
					if (isRecapping)
					{
						EndRecap();
					}
					StopProcessing();
					if (flag)
					{
						CommandTriggerUIUtils.TryShowNextCommandTrigger();
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("{0} Update Failed, reason: {1}", new object[2]
			{
				LOG_PREFIX,
				ex.ToString()
			});
			if (GameManager.GameState == null || (Object)(object)MapRenderer.Current == (Object)null)
			{
				Log.Error("State not initialized properly, exiting game (gamestate {0}, maprenderer {1}", new object[2]
				{
					GameManager.GameState,
					MapRenderer.Current
				});
				GameManager.ReturnToMenu();
				return;
			}
			currentReaction = null;
			if (isRecapping)
			{
				EndRecap();
			}
			StopProcessing();
			GameManager.Client.ResetGameState(StateUpdateReason.StateReset);
		}
	}

	public void SimulateCommand()
	{
		if (isAborting)
		{
			Log.Error("Trying to SimulateCommand while aborting", Array.Empty<object>());
			return;
		}
		CommandBase commandBase = gameState.CommandStack[LastSeenCommand++];
		if (!commandBase.IsValid(gameState, out var validationError))
		{
			Log.Error("Trying to simulate an invalid command {0} ({1})", new object[2] { commandBase, validationError });
			if (gameState.TryGetPendingCommandTrigger(gameState.CurrentPlayer, out var _))
			{
				throw new Exception("Can't continue simulation if we have commands invalid due to command triggers");
			}
		}
		Log.Spam("{0} {2,-24}\t: {1} ({4} of {5}, {3} commands remaining)", new object[6]
		{
			LOG_PREFIX,
			commandBase,
			"Simulating command",
			gameState.CommandStack.Count - LastSeenCommand + 1,
			LastSeenCommand,
			gameState.CommandStack.Count
		});
		commandBase.Execute(gameState);
	}

	public void Pause()
	{
		if (isAborting)
		{
			Log.Error("Trying to Pause while aborting", Array.Empty<object>());
			return;
		}
		Log.Verbose("{0} Pausing at Command {1}/{2}", new object[3]
		{
			LOG_PREFIX,
			LastSeenCommand,
			(base.GameState != null && base.GameState.CommandStack != null) ? base.GameState.CommandStack.Count : 0
		});
		isHolding = false;
		isPaused = true;
		currentReaction = null;
	}

	public void Resume()
	{
		if (isAborting)
		{
			Log.Error("Trying to Resume while aborting", Array.Empty<object>());
			return;
		}
		Log.Verbose("{0} Resuming from Command {1}/{2}", new object[3]
		{
			LOG_PREFIX,
			LastSeenCommand,
			(base.GameState != null && base.GameState.CommandStack != null) ? base.GameState.CommandStack.Count : 0
		});
		GameManager.CancelDelay();
		if (LastSeenCommand <= gameState.CommandStack.Count - 1)
		{
			StartRecap();
		}
		else if (isRecapping)
		{
			EndRecap();
		}
		isHolding = false;
		isPaused = false;
		processing = true;
		InputManager.DisableInput(InputManager.InputType.Map);
		Update();
	}

	public bool IsWaitingForCommandTrigger(byte playerId)
	{
		CommandTrigger trigger;
		return gameState.TryGetPendingCommandTrigger(playerId, out trigger);
	}

	protected override void StartProcessing()
	{
		if (isSimulating)
		{
			Log.Error("Can't start processing while simulating", Array.Empty<object>());
			return;
		}
		StopProcessing();
		Log.Verbose("{0} Starting processing...", new object[1] { LOG_PREFIX });
		processing = true;
		InputManager.DisableInput(InputManager.InputType.Map);
		OnStartedProcessing?.Invoke();
	}

	public override void StopProcessing()
	{
		if (processing)
		{
			Log.Verbose("{0} Finished processing", new object[1] { LOG_PREFIX });
			processing = false;
			InputManager.EnableInput(InputManager.InputType.Map);
			OnFinishedProcessing?.Invoke();
		}
	}

	public override void AbortExecution(Action OnAbortedProcessing)
	{
		isAborting = true;
		if (IsProcessing)
		{
			Log.Info("{0} Aborting processing...", new object[1] { LOG_PREFIX });
			base.OnAbortedProcessing = OnAbortedProcessing;
		}
		else
		{
			currentReaction = null;
			OnAbortedProcessing?.Invoke();
		}
	}

	protected void OnExecutionAborted()
	{
		Log.Info("{0} Aborted processing", new object[1] { LOG_PREFIX });
		processing = false;
		currentReaction = null;
		isHolding = false;
		InputManager.EnableInput(InputManager.InputType.Map);
		OnAbortedProcessing?.Invoke();
	}

	protected void StartRecap()
	{
		Log.Verbose("{0} Recap started...", new object[1] { LOG_PREFIX });
		isRecapping = true;
		OnStartedRecap?.Invoke();
		InputManager.DisableInput(InputManager.InputType.Map);
		if (GameManager.Client.IsReplay)
		{
			GameEvents.ReplayStarted();
		}
		else
		{
			GameEvents.RecapStarted();
		}
		NotificationManager.UpdateIngameAlert();
	}

	public void EndRecap()
	{
		if (isRecapping)
		{
			Log.Verbose("{0} Recap ended", new object[1] { LOG_PREFIX });
			isRecapping = false;
			InputManager.ResetInputBlocker();
			InputManager.BlockInputIfShowingLoadingScreen();
			if (GameManager.Client.IsReplay)
			{
				GameEvents.ReplayEnded();
			}
			else
			{
				GameEvents.RecapEnded();
			}
			NotificationManager.UpdateIngameAlert();
			StartProcessing();
		}
	}

	public void HoldExecution(bool value)
	{
		isHolding = value;
		if (!isHolding)
		{
			Update();
		}
	}
}
