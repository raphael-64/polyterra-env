using PolytopiaBackendBase.Game;

public class GameModeUtils
{
	public static string GetTitle(GameMode gameMode)
	{
		return gameMode switch
		{
			GameMode.None => "gamemode.none", 
			GameMode.Perfection => "gamemode.perfection", 
			GameMode.Domination => "gamemode.domination", 
			GameMode.Glory => "gamemode.glory", 
			GameMode.Might => "gamemode.might", 
			GameMode.Custom => "gamemode.custom", 
			GameMode.Sandbox => "gamemode.sandbox", 
			GameMode.Tutorial => "gamemode.tutorial", 
			_ => string.Empty, 
		};
	}

	public static string GetDescription(GameMode gameMode)
	{
		return gameMode switch
		{
			GameMode.None => "gamemode.none.description", 
			GameMode.Perfection => "gamemode.perfection.description", 
			GameMode.Domination => "gamemode.domination.description", 
			GameMode.Glory => "gamemode.glory.description", 
			GameMode.Might => "gamemode.might.description", 
			GameMode.Custom => "gamemode.custom.description", 
			GameMode.Sandbox => "gamemode.sandbox.description", 
			_ => string.Empty, 
		};
	}

	public static string GetDifficultyName(GameSettings.Difficulties difficulty)
	{
		return difficulty switch
		{
			GameSettings.Difficulties.Easy => "gamesettings.difficulty.easy", 
			GameSettings.Difficulties.Normal => "gamesettings.difficulty.normal", 
			GameSettings.Difficulties.Hard => "gamesettings.difficulty.hard", 
			GameSettings.Difficulties.Crazy => "gamesettings.difficulty.crazy", 
			GameSettings.Difficulties.Frozen => "gamesettings.difficulty.frozen", 
			_ => string.Empty, 
		};
	}

	public static string GetWinMessage(GameMode gameMode)
	{
		switch (gameMode)
		{
		case GameMode.Perfection:
			return "gamemode.perfection.win";
		case GameMode.Domination:
			return "gamemode.domination.win";
		case GameMode.Glory:
			if (GameStateUtils.CountRealAlivePlayers(GameManager.GameState) <= 1)
			{
				return "gamemode.glory.win.no.opponents";
			}
			return "gamemode.glory.win";
		case GameMode.Might:
			if (!GameStateUtils.SecondLastPlayerResigned(GameManager.GameState))
			{
				return "gamemode.might.win";
			}
			return "gamemode.might.win.no.opponents";
		case GameMode.Custom:
			return string.Empty;
		default:
			return string.Empty;
		}
	}

	public static string GetLooseMessage(GameMode gameMode)
	{
		return gameMode switch
		{
			GameMode.Perfection => "gamemode.perfection.loss", 
			GameMode.Domination => "gamemode.domination.loss", 
			GameMode.Sandbox => "gamemode.domination.loss", 
			GameMode.Custom => string.Empty, 
			_ => string.Empty, 
		};
	}

	public static bool HighscoreEnabled(GameMode gameMode)
	{
		return gameMode switch
		{
			GameMode.Perfection => true, 
			_ => false, 
		};
	}
}
