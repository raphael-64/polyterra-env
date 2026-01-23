using System;
using PolytopiaBackendBase.Game;

public static class GameTypeUtils
{
	public static bool ShouldAllowChangingDisabledTribes(GameType gameType, Guid? gameOwnerId)
	{
		switch (gameType)
		{
		case GameType.Multiplayer:
			if (gameOwnerId.HasValue)
			{
				Guid playerAccountId = AccountManager.PlayerAccountId;
				Guid? guid = gameOwnerId;
				return playerAccountId == guid;
			}
			return false;
		case GameType.Competitive:
			return false;
		default:
			return true;
		}
	}
}
