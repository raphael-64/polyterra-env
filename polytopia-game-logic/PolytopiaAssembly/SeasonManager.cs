using System;
using Polytopia.Data;
using UnityEngine;

public class SeasonManager
{
	private static DateTime cachedNow;

	private static float lastCacheTime;

	private const float REFRESH_INTERVAL = 60f;

	public static DateTime Now()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!(realtimeSinceStartup - lastCacheTime >= 60f))
		{
			_ = cachedNow;
			if (lastCacheTime != 0f)
			{
				goto IL_0036;
			}
		}
		lastCacheTime = realtimeSinceStartup;
		cachedNow = DateTime.UtcNow;
		goto IL_0036;
		IL_0036:
		return cachedNow;
	}

	public static bool IsHalloween()
	{
		return Now().Month == 11;
	}

	public static bool IsChristmas()
	{
		DateTime dateTime = Now();
		if (dateTime.Month != 12 || dateTime.Day < 22)
		{
			if (dateTime.Month == 1)
			{
				return dateTime.Day <= 5;
			}
			return false;
		}
		return true;
	}

	public static bool ShouldPlayerHaveVengirHalloweenHeads(GameState gameState, PlayerState player)
	{
		if (IsHalloween() && player.tribe == TribeData.Type.Vengir)
		{
			return GameLogicData.HasPlayerBuiltUniqueImprovement(gameState, player, ImprovementData.Type.Monument5);
		}
		return false;
	}

	public static bool ShouldShowVengirHalloweenHeadsAfterBuildAction(GameState state, BuildAction action)
	{
		if (IsHalloween() && state.TryGetPlayer(action.PlayerId, out var playerState) && playerState.tribe == TribeData.Type.Vengir)
		{
			return action.Type == ImprovementData.Type.Monument5;
		}
		return false;
	}

	public static bool TryGetHeadVariant(PlayerState player, out SkinType seasonalSkin)
	{
		seasonalSkin = SkinType.Default;
		if (ShouldPlayerHaveVengirHalloweenHeads(GameManager.GameState, player))
		{
			seasonalSkin = SkinType.Skeleton;
			return true;
		}
		return false;
	}
}
