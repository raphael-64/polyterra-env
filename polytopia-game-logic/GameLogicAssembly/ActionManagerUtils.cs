public static class ActionManagerUtils
{
	public static bool PerformAllQueuedActions(GameState gameState)
	{
		int num = 1000;
		while (SimulateAction(gameState) && num-- > 0)
		{
		}
		return num > 0;
	}

	public static bool SimulateAction(GameState gameState)
	{
		if (gameState.ActionStack == null || gameState.ActionStack.Count == 0)
		{
			return false;
		}
		if (gameState.TryGetPendingCommandTrigger(gameState.CurrentPlayer, out var _))
		{
			return false;
		}
		int num = gameState.ActionStack.Count - 1;
		ActionBase actionBase = gameState.ActionStack[num];
		if (actionBase.IsValid(gameState))
		{
			Log.Spam("{0} {2,-24}\t: {1} ({3})", new object[4] { "ActionManager", actionBase, "Simulating action", num });
			actionBase.Execute(gameState);
		}
		else
		{
			Log.Spam("{0} {2,-24}\t: {1} ({3})", new object[4] { "ActionManager", actionBase, "Sim Action is invalid", num });
		}
		gameState.ActionStack.RemoveAt(num);
		return true;
	}
}
