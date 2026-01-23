using System;
using System.Collections.Generic;

public class ActionManager
{
	public delegate bool BoolDelegate();

	public static bool USE_COMMAND_TRIGGER = true;

	protected bool processing;

	public bool isAborting;

	public bool shouldResetAfterAbort;

	protected GameState gameState;

	public Action OnStartedProcessing;

	public Action OnFinishedProcessing;

	public Action OnStartedRecap;

	public BoolDelegate ShouldInterruptCommand;

	public Action OnInterruptCommand;

	protected Action OnAbortedProcessing;

	protected virtual string LOG_PREFIX => "<color=#ff7b7b>[SAM]</color>";

	public GameState GameState
	{
		get
		{
			return gameState;
		}
		set
		{
			gameState = value;
		}
	}

	public virtual bool IsProcessing => processing;

	public ActionManager(GameState state)
	{
		Log.Verbose("{0} Initialized", new object[1] { LOG_PREFIX });
		GameState = state;
	}

	public virtual void Reset(GameState state)
	{
		GameState = state;
		processing = false;
		isAborting = false;
	}

	public virtual bool ExecuteCommand(CommandBase command, out string error)
	{
		if (isAborting)
		{
			Log.Warning("Trying to execute command while aborting", Array.Empty<object>());
			shouldResetAfterAbort = true;
		}
		if (!command.IsValid(gameState, out var validationError))
		{
			error = $"Player {command.PlayerId} played an invalid command {command.ToString()} with error {validationError}";
			Log.Warning(error, Array.Empty<object>());
			return false;
		}
		gameState.CommandStack.Add(command);
		if (!processing)
		{
			StartProcessing();
			Update();
		}
		error = null;
		return true;
	}

	public virtual bool ExecuteCommands(List<CommandBase> commands)
	{
		if (isAborting)
		{
			Log.Warning("Trying to execute command while aborting", Array.Empty<object>());
			shouldResetAfterAbort = true;
		}
		for (int i = 0; i < commands.Count; i++)
		{
			gameState.CommandStack.Add(commands[i]);
		}
		if (!processing)
		{
			StartProcessing();
			Update();
		}
		return true;
	}

	protected virtual void Update()
	{
		CommandTrigger trigger;
		bool flag = gameState.TryGetPendingCommandTrigger(gameState.CurrentPlayer, out trigger) && USE_COMMAND_TRIGGER;
		if (gameState.ActionStack != null && gameState.ActionStack.Count > 0 && !gameState.WaitForCommand && !flag)
		{
			int num = gameState.ActionStack.Count - 1;
			ActionBase actionBase = gameState.ActionStack[num];
			if (actionBase.IsValid(gameState))
			{
				Log.Spam("{0} Executing action ({2}):\t{1}", new object[3]
				{
					LOG_PREFIX,
					actionBase,
					num + 1
				});
				actionBase.Execute(gameState);
			}
			else
			{
				Log.Spam("{0} Action is invalid ({2}):\t{1}", new object[3]
				{
					LOG_PREFIX,
					actionBase,
					num + 1
				});
			}
			gameState.ActionStack.RemoveAt(num);
			Update();
		}
		else if (gameState.LastProcessedCommand < gameState.CommandStack.Count)
		{
			CommandBase commandBase = gameState.CommandStack[gameState.LastProcessedCommand++];
			Log.Spam("{0} Executing command ({2}):\t{1}", new object[3]
			{
				LOG_PREFIX,
				commandBase,
				gameState.CommandStack.Count - gameState.LastProcessedCommand + 1
			});
			commandBase.Execute(gameState);
			Update();
		}
		else
		{
			Log.Verbose("{0} Finished processing", new object[1] { LOG_PREFIX });
			StopProcessing();
		}
	}

	public virtual void ForceUpdate()
	{
		Update();
	}

	public virtual void AbortExecution(Action OnAbortedProcessing)
	{
		isAborting = true;
		if (IsProcessing)
		{
			Log.Info("{0} Aborting processing...", new object[1] { LOG_PREFIX });
			this.OnAbortedProcessing = OnAbortedProcessing;
		}
		else
		{
			OnAbortedProcessing?.Invoke();
		}
	}

	protected virtual void StartProcessing()
	{
		StopProcessing();
		processing = true;
		OnStartedProcessing?.Invoke();
	}

	public virtual void StopProcessing()
	{
		if (processing)
		{
			processing = false;
			OnFinishedProcessing?.Invoke();
		}
	}
}
