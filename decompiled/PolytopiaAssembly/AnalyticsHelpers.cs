using System;
using System.Collections.Generic;
using Polytopia.Data;

public static class AnalyticsHelpers
{
	private static float currency;

	public static void AddCurrency(float amount)
	{
		currency += amount;
	}

	public static void SendCurrencyEvent()
	{
		if (currency != 0f)
		{
			GameManager.GetAnalyticsManager().SendEvent("earn_virtual_currency", new Dictionary<string, object>
			{
				{ "value", currency },
				{ "virtual_currency_name", "stars" },
				{
					"game_id",
					GameManager.Client.CurrentGameId
				}
			});
			currency = 0f;
		}
	}

	public static void SendGameStartEvent(Guid gameId, GameSettings settings, TribeData.Type? tribe)
	{
		GameManager.GetAnalyticsManager().SendEvent("game_start", new Dictionary<string, object>
		{
			{ "game_id", gameId },
			{
				"game_mode",
				settings.BaseGameMode.ToString().ToLowerInvariant().Replace("custom", "creative")
			},
			{ "game_type", settings.GameType },
			{ "opponents", settings.OpponentCount },
			{ "difficulty", settings.Difficulty },
			{ "map_type", settings.mapPreset },
			{ "map_size", settings.MapSize },
			{ "tribe", tribe }
		});
	}
}
