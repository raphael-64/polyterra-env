public static class CommandEvents
{
	public delegate void OnCommandExecutedEvent(CommandBase command);

	public static event OnCommandExecutedEvent OnCommandExecuted;

	public static void CommandExecuted(CommandBase command)
	{
		CommandEvents.OnCommandExecuted?.Invoke(command);
	}
}
