using System;
using Polytopia.Data;

public static class ScoreExtensions
{
	public static int CalculateImprovementScore(this GameState state, TileData tile)
	{
		if (tile.improvement == null || tile.owner == 0 || tile.owner == byte.MaxValue)
		{
			return 0;
		}
		ImprovementState improvement = tile.improvement;
		int num = improvement.baseScore;
		state.GameLogicData.TryGetData(improvement.type, out var data);
		if (improvement.type == ImprovementData.Type.City)
		{
			num += (improvement.level + 1) * ScoreSheet.cityLevelScore;
			num += improvement.xp * ScoreSheet.cityXPScore;
		}
		if (data.HasAbility(ImprovementAbility.Type.Patina))
		{
			num += GetPatinaScore(state, tile);
		}
		return num;
	}

	public static int GetPatinaScore(GameState state, TileData tile)
	{
		if (tile.improvement == null)
		{
			return 0;
		}
		if (state.GameLogicData.TryGetData(tile.improvement.type, out var data) && data.HasAbility(ImprovementAbility.Type.Patina) && state.GameLogicData.TryGetData(tile.improvement.type, out var data2))
		{
			int num = Math.Max((state.Version > 42) ? tile.improvement.level : (tile.improvement.level - 1), 0);
			int num2 = 0;
			if (state.Version > 42)
			{
				foreach (GrowthRewards growthReward in data.growthRewards)
				{
					num2 += growthReward.score;
				}
			}
			else
			{
				num2 = (int)data2.GetScoreReward();
			}
			if (state.Version <= 42)
			{
				num2 /= 2;
			}
			return num * num2;
		}
		return 0;
	}

	public static int CalculateLevelUpScore(this TileData tile)
	{
		return ScoreSheet.cityLevelScore - tile.improvement.level * ScoreSheet.cityXPScore;
	}

	public static float GetDifficultyScore(this GameState gameState)
	{
		return (float)Math.Round(((float)GameSettings.HandicapFromDifficulty(gameState.Settings.Difficulty) + 1f) / 5f * 100f);
	}
}
