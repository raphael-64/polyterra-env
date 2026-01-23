using System;
using Polytopia.Data;

public class ScoreSheet
{
	public static int tileValue = 20;

	public static int cityLevelScore = 50;

	public static int cityXPScore = 5;

	public static int exploreValue = 5;

	public static uint[] ratingLimits = new uint[3] { 20u, 70u, 95u };

	public static uint[] scoreLimits = new uint[3] { 3000u, 10000u, 50000u };

	public static float GetUnitScore(UnitData unitData)
	{
		return unitData.cost * 5;
	}

	public static float GetTechScore(GameState state, TechData.Type type)
	{
		if (state.GameLogicData.TryGetData(type, out var data))
		{
			return GetTechScore(data);
		}
		return 0f;
	}

	public static float GetTechScore(TechData tech)
	{
		if (tech == null)
		{
			return 0f;
		}
		return tech.cost * 100;
	}

	public static float GetDifficultyBonusMultiplier(GameSettings.Difficulties difficulty, int opponentCount)
	{
		return (float)Math.Round(((double)((float)GameSettings.HandicapFromDifficulty(difficulty) / 5f) + Math.Log(Math.Max(1, opponentCount)) / Math.Log(11.399999618530273)) * 100.0) / 100f;
	}
}
