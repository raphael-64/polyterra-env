using System;

public class EndMatchReaction : ReactionBase
{
	private readonly EndMatchAction action;

	public EndMatchReaction(EndMatchAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (GameManager.Client.IsReplay)
		{
			UIEvents.ForceRefreshHud();
			return;
		}
		bool localPlayerIsWinner = false;
		if (GameManager.GameState.TryGetWinner(out var winner))
		{
			localPlayerIsWinner = GameManager.LocalPlayer.Id == winner.Id;
		}
		ScoreDetails score = GameManager.LocalPlayer.GetScore(GameManager.GameState);
		GameManager.MatchEnded(localPlayerIsWinner, score, winner.Id);
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
