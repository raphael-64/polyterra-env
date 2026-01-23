using System;
using System.Collections.Generic;
using PolytopiaBackendBase.Game;

public class GameOverReaction : ReactionBase
{
	private readonly GameOverAction action;

	public GameOverReaction(GameOverAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (GameManager.Client.IsSpectating)
		{
			onComplete();
			return;
		}
		if (GameManager.GameState.Settings.GameType == GameType.SinglePlayer)
		{
			int num = PolytopiaPlayerPrefs.GetInt("LocalPlayedGames");
			num++;
			PolytopiaPlayerPrefs.SetInt("LocalPlayedGames", num);
			PolytopiaPlayerPrefs.Save();
		}
		if (GameManager.GameState.Settings.rules.TurnLimit > 0)
		{
			onComplete();
			return;
		}
		string description = GetDescription();
		if (!string.IsNullOrEmpty(description))
		{
			IconPopup iconPopup = PopupManager.GetIconPopup();
			iconPopup.Header = GetHeader();
			iconPopup.Description = description;
			iconPopup.spriteHandle.Request(SpriteData.GetHeadSpriteAddresses(GameManager.GameState, GameManager.LocalPlayer));
			iconPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					onComplete();
				})
			};
			iconPopup.Show();
		}
		else
		{
			onComplete();
		}
	}

	private string GetHeader()
	{
		return GameManager.GameState.Settings.RulesGameMode switch
		{
			GameMode.Perfection => Localization.Get("gamemode.perfection"), 
			GameMode.Domination => Localization.Get("gamemode.domination"), 
			GameMode.Glory => Localization.Get("gamemode.glory"), 
			GameMode.Might => Localization.Get("gamemode.might"), 
			_ => "Game Over", 
		};
	}

	private string GetDescription()
	{
		if (GameManager.GameState.TryGetPlayer(action.WinningPlayerId, out var playerState))
		{
			GameSettings settings = GameManager.GameState.Settings;
			List<PlayerState> playersSortedByRankForMultiplayerResults = GameManager.GameState.GetPlayersSortedByRankForMultiplayerResults();
			bool flag = GameManager.LocalPlayer.Id == playersSortedByRankForMultiplayerResults[0].Id;
			switch (settings.RulesGameMode)
			{
			case GameMode.Perfection:
				if (flag)
				{
					return Localization.Get("gamemode.perfection.win");
				}
				if (GameManager.GameState.CurrentTurn < settings.rules.TurnLimit)
				{
					return Localization.Get("gamemode.death");
				}
				return Localization.Get("gamemode.perfection.loss");
			case GameMode.Domination:
				if (flag)
				{
					return Localization.Get("gamemode.domination.win");
				}
				return Localization.Get("gamemode.domination.loss");
			case GameMode.Glory:
				if (GameStateUtils.CountRealAlivePlayers(GameManager.GameState) > 1)
				{
					return Localization.Get("gamemode.glory.win", settings.rules.ScoreLimit, playerState.GetLocalizedTribeName(GameManager.GameState));
				}
				if (flag)
				{
					return Localization.Get("gamemode.might.win.last.human");
				}
				return null;
			case GameMode.Might:
				if (flag)
				{
					bool flag2 = GameStateUtils.SecondLastPlayerResigned(GameManager.GameState);
					int num = GameStateUtils.CountAlivePlayers(GameManager.GameState);
					if (!flag2 && num > 1)
					{
						return Localization.Get("gamemode.might.win.last.human");
					}
					return Localization.Get(flag2 ? "gamemode.might.win.no.opponents" : "gamemode.might.win");
				}
				return null;
			case GameMode.Custom:
			case GameMode.Sandbox:
				return Localization.Get("endscreen.winner", playerState.UserName);
			}
		}
		return "Uh-Oh, gamemode not registered";
	}
}
