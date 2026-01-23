using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using UnityEngine;

public class NewGameIntent : Intent
{
	public NewGameIntent(Uri uri)
		: base(uri)
	{
	}

	public override bool CanBeQueuedAfterIntent(Intent intent)
	{
		return !(intent is NewGameIntent);
	}

	public override async Task HandleAsync()
	{
		await DeepLinkNewGame();
		Log.Info("[DeepLinking] processed intent.", Array.Empty<object>());
		base.State = ProcessState.Processed;
	}

	private Task DeepLinkNewGame()
	{
		Dictionary<string, string> queryMap = base.QueryMap;
		GameSettings gameSettings = new GameSettings();
		gameSettings.SetUnlockedTribes(GameManager.GetPurchaseManager().GetUnlockedTribes());
		gameSettings.BaseGameMode = ParseGameMode(queryMap);
		gameSettings.Difficulty = ParseDifficulty(queryMap);
		gameSettings.OpponentCount = ParseIntVariable("opponents", queryMap, 1);
		GameManager.StartingTribe = ParseTribe(queryMap);
		GameManager.PreliminaryGameSettings = gameSettings;
		TaskCompletionSource<bool> createGameCompletionSource = new TaskCompletionSource<bool>();
		UIBlackFader.FadeIn(0.5f, async delegate
		{
			if (Object.op_Implicit((Object)(object)UIManager.Instance) && UIManager.Instance.type == UIManager.Type.Ingame)
			{
				PopupManager.RemoveAllPopups();
				ResultScreen.Hide();
				while (UIManager.Instance.CurrentScreen != UIConstants.Screens.Hud && UIManager.Instance.IsScreenInStack(UIConstants.Screens.Hud))
				{
					UIManager.Instance.OnBack();
				}
			}
			DOTween.KillAll(false);
			await GameManager.Instance.CreateSinglePlayerGame();
			createGameCompletionSource.SetResult(result: true);
		});
		return createGameCompletionSource.Task;
	}

	private GameMode ParseGameMode(Dictionary<string, string> queryVariables)
	{
		if (queryVariables.TryGetValue("gamemode", out var value) && Enum.TryParse<GameMode>(value, ignoreCase: true, out var result))
		{
			return result;
		}
		return GameMode.Perfection;
	}

	private TribeData.Type ParseTribe(Dictionary<string, string> queryVariables)
	{
		if (queryVariables.TryGetValue("tribe", out var value) && Enum.TryParse<TribeData.Type>(value, ignoreCase: true, out var result))
		{
			return result;
		}
		return TribeData.Type.Xinxi;
	}

	private GameSettings.Difficulties ParseDifficulty(Dictionary<string, string> queryVariables)
	{
		if (queryVariables.TryGetValue("difficulty", out var value) && Enum.TryParse<GameSettings.Difficulties>(value, ignoreCase: true, out var result))
		{
			return result;
		}
		return GameSettings.Difficulties.Normal;
	}

	private int ParseIntVariable(string key, Dictionary<string, string> queryVariables, int defaultValue = 0)
	{
		queryVariables.TryGetValue(key, out var value);
		int result = -1;
		if (value != null)
		{
			int.TryParse(value, out result);
		}
		if (queryVariables.TryGetValue(key, out var value2) && int.TryParse(value2, out var result2))
		{
			return result2;
		}
		return defaultValue;
	}
}
