using Polytopia.Data;

public static class ResourceDataUtils
{
	public static int CalculateIncomeFor(GameState gameState, byte playerId)
	{
		int num = 0;
		gameState.TryGetPlayer(playerId, out var playerState);
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (tileData.improvement != null)
			{
				num += tileData.CalculateWork(gameState, playerState, tileData.improvement.level);
			}
		}
		return num;
	}

	public static uint GetCurrencyReward(this ImprovementState state, GameState gameState)
	{
		if (gameState.GameLogicData.TryGetData(state.type, out var data))
		{
			return data.GetCurrencyReward();
		}
		return 0u;
	}

	public static uint GetCurrencyReward(this ImprovementData data)
	{
		return (uint)data.rewards.GetCurrency();
	}

	public static uint GetPopulationReward(this ImprovementState state, GameState gameState)
	{
		if (gameState.GameLogicData.TryGetData(state.type, out var data))
		{
			return data.GetPopulationReward();
		}
		return 0u;
	}

	public static uint GetPopulationReward(this ImprovementData data)
	{
		return (uint)data.rewards.GetPopulation();
	}

	public static uint GetScoreReward(this ImprovementData data)
	{
		int num = 0;
		if (data.rewards != null && data.rewards.Count > 0)
		{
			foreach (Rewards reward in data.rewards)
			{
				if (reward.score > 0)
				{
					num += reward.score;
				}
			}
		}
		return (uint)num;
	}

	public static int GetCurrencyCost(this ImprovementState improvement, GameState gameState)
	{
		if (gameState.GameLogicData.TryGetData(improvement.type, out var data))
		{
			return data.GetCurrencyCost();
		}
		return 0;
	}

	public static int GetCurrencyCost(this ImprovementData data)
	{
		return data.cost;
	}
}
