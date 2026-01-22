using System;
using System.Collections.Generic;

namespace PolytopiaBackendBase.Game;

public class CommandResult
{
	public int GameStateHash { get; set; }

	public bool PreviouslyReadyToStart { get; set; }

	public bool ReadyToStart { get; set; }

	public List<PolytopiaCommandViewModel> ExecutedCommands { get; set; }

	public int PreviousGameStateHash { get; set; }

	public byte[] CurrentGameStateData { get; set; }

	public byte[] GameStateDataBeforeAi { get; set; }

	public List<CommandResultEvent> Events { get; set; }

	public Guid? CurrentUserId { get; set; }

	public Guid? FirstHumanPlayerId { get; set; }

	public int CurrentTurnNumber { get; set; }

	public bool Success { get; set; }

	public bool IdempotentSuccess { get; set; }

	public Exception Exception { get; set; }

	public StateUpdateReason? PushReason { get; set; }

	public static CommandResult UnsuccessfulResult(Exception exception = null)
	{
		return new CommandResult
		{
			Success = false,
			Exception = exception
		};
	}

	public static CommandResult IdempotentSuccessResult()
	{
		return new CommandResult
		{
			Success = true,
			IdempotentSuccess = true
		};
	}
}
