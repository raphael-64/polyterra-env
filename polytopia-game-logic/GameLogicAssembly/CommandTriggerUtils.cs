using System;
using System.Collections.Generic;
using Polytopia.Data;

public static class CommandTriggerUtils
{
	public static bool TryGetTriggerCommand(GameState gameState, out CommandBase command)
	{
		command = null;
		if (gameState.TryGetPendingCommandTrigger(gameState.CurrentPlayer, out var trigger))
		{
			switch (trigger.type)
			{
			case CommandTriggerType.CityLevelUp:
			{
				if (!gameState.GameLogicData.TryGetData(ImprovementData.Type.City, out var data))
				{
					return false;
				}
				TileData tile2 = gameState.Map.GetTile(trigger.coordinates);
				CityReward[] cityRewardsForLevel = data.GetCityRewardsForLevel(tile2.improvement.level - 1);
				CityReward reward2 = AI.ChooseCityReward(gameState, tile2, cityRewardsForLevel);
				if (!new CityRewardCommand(gameState.CurrentPlayer, reward2, tile2.coordinates).IsValid(gameState))
				{
					return false;
				}
				command = new CityRewardCommand(gameState.CurrentPlayer, reward2, tile2.coordinates);
				return true;
			}
			case CommandTriggerType.PeaceRequest:
			{
				bool accepted = AI.ShouldAcceptPeace(gameState, gameState.CurrentPlayer, trigger.opponentId);
				command = new PeaceRequestResponseCommand(gameState.CurrentPlayer, trigger.opponentId, accepted);
				return true;
			}
			case CommandTriggerType.Infiltrate:
			{
				TileData tile = gameState.Map.GetTile(trigger.coordinates);
				CityReward reward = AI.ChooseInfiltrationReward(gameState, tile, CityRewardData.infiltrateRewards);
				command = new InfiltrateRewardCommand(gameState.CurrentPlayer, reward, tile.coordinates);
				return true;
			}
			default:
				throw new Exception("Not implemented");
			}
		}
		return false;
	}

	public static TileData GetCityTileThatShouldLevelUp(List<TileData> cityTiles)
	{
		foreach (TileData cityTile in cityTiles)
		{
			if (cityTile.improvement.ShouldLevelUp())
			{
				return cityTile;
			}
		}
		return null;
	}
}
