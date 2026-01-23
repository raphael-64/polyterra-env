using PolytopiaBackendBase.Game;

public class StartMatchAction : ActionBase
{
	public StartMatchAction()
	{
	}

	public StartMatchAction(byte playerId)
		: base(playerId)
	{
	}

	public override void Execute(GameState gameState)
	{
		if (gameState.PlayerStates != null && gameState.PlayerStates.Count > 0)
		{
			for (int i = 0; i < gameState.PlayerStates.Count; i++)
			{
				PlayerState playerState = gameState.PlayerStates[i];
				if (gameState.GameLogicData.TryGetData(playerState.tribe, out var data))
				{
					if (data.startingTech == null || data.startingTech.Count == 0)
					{
						continue;
					}
					for (int j = 0; j < data.startingTech.Count; j++)
					{
						ActionUtils.LearnTech(gameState, playerState, data.startingTech[j].type, 0, shouldUseActions: false);
					}
				}
				if (gameState.Settings.RulesGameMode == GameMode.Tutorial)
				{
					gameState.ActionStack.Add(new IncreaseCurrencyAction(playerState.Id, playerState.startTile, 6, 40));
				}
			}
		}
		gameState.CurrentState = GameState.State.Started;
	}

	public override ActionType GetActionType()
	{
		return ActionType.StartMatch;
	}
}
