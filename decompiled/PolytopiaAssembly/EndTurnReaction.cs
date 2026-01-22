using System;
using System.Collections.Generic;
using PolytopiaBackendBase.Game;
using UnityEngine;

public class EndTurnReaction : ReactionBase
{
	private readonly EndTurnAction action;

	public EndTurnReaction(EndTurnAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (GameManager.IsPlayerViewing(action.PlayerId) && !GameManager.Client.IsRecap)
		{
			GameManager.GetAnalyticsManager().SendEvent("turn_end", new Dictionary<string, object> { 
			{
				"game_id",
				GameManager.Client.CurrentGameId
			} });
		}
		if (!GameManager.Client.IsRecap)
		{
			NotificationManager.Alert(Localization.Get("wcontroller.turn.end"));
			if (GameManager.GameState.Settings.GameType == GameType.Multiplayer || GameManager.GameState.Settings.GameType == GameType.Competitive || GameManager.GameState.Settings.GameType == GameType.Matchmaking)
			{
				RewardPopup currentPopup = PopupManager.GetCurrentPopup<RewardPopup>();
				if ((Object)(object)currentPopup != (Object)null)
				{
					currentPopup.Hide();
				}
				if (GameManager.GameState.TryGetNextHumanPlayerIndex(GameManager.GameState.CurrentPlayerIndex, out var nextPlayerIndex))
				{
					byte id = GameManager.GameState.PlayerStates[nextPlayerIndex].Id;
					if (id != GameManager.LocalPlayer.Id && GameManager.IsPlayerViewing(action.PlayerId) && GameManager.GameState.TryGetPlayer(id, out var playerState))
					{
						NotificationManager.Notify(Localization.Get("wcontroller.turn.passed", playerState.UserName), Localization.Get("wcontroller.turn.passed.title"));
					}
				}
			}
		}
		GameEvents.TurnEnded();
		onComplete();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
