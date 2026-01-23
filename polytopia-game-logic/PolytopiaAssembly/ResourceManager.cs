using System;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class ResourceManager
{
	public enum Type
	{
		Score,
		Currency,
		Population,
		Production
	}

	protected class LocalWallet
	{
		public byte playerId;

		public int score;

		public int currency;

		public LocalWallet()
		{
		}

		public LocalWallet(byte playerId)
		{
			this.playerId = playerId;
		}

		public void SyncWithState()
		{
			if (GameManager.GameState.TryGetPlayer(playerId, out var playerState))
			{
				score = (int)playerState.score;
				currency = playerState.Currency;
				Log.Verbose("Sync wallet with state for player : {0} was successful: Score: {1}, Currency: {2}", new object[3] { playerId, score, currency });
			}
			else
			{
				Log.Error("Failed to syn wallet with player: {0}, could not find player", new object[1] { playerId });
			}
		}
	}

	protected static Dictionary<byte, LocalWallet> localWallets = new Dictionary<byte, LocalWallet>();

	protected static bool loggingEnable = false;

	public static float LocalPlayerScore => GetResourceOfType(GameManager.LocalPlayer.Id, Type.Score);

	public static float LocalPlayerCurrency => GetResourceOfType(GameManager.LocalPlayer.Id, Type.Currency);

	public static void InitResources()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		DebugConsole.AddCommand("resources_togglelog", new CommandDelegate(CmdToggleLog), "Toggle logging of resource events");
		ResourceEvents.OnRefreshWallets += OnRefreshWallets;
	}

	public static void AddResourceOfTypeToResourceBar(byte playerId, Type type, float amount, WorldCoordinates from, Action onComplete = null, string reason = "None")
	{
		RectTransform val = null;
		switch (type)
		{
		case Type.Score:
			val = ResourceBar.ScoreContainer.rectTransform;
			break;
		case Type.Currency:
			val = ResourceBar.CurrencyContainer.rectTransform;
			break;
		}
		if (loggingEnable)
		{
			Log.Verbose("AddResourceOfTypeToResourceBar :: onComplete : {0}", new object[1] { onComplete != null });
		}
		if ((Object)(object)val != (Object)null)
		{
			AddResourceOfType(playerId, type, amount, new ResourceEvents.ResourceEventPositionData(from), new ResourceEvents.ResourceEventPositionData(val), onComplete, reason);
			return;
		}
		Log.Warning("Trying to add resources to the resource bar with unsupported type : {0}", new object[1] { type.ToString() });
	}

	public static void AddPopulationToCity(byte playerId, float amount, WorldCoordinates from, Building to, Action onResourcesAdded = null, Action onComplete = null, string reason = "None")
	{
		AddResourceOfType(playerId, Type.Population, amount, new ResourceEvents.ResourceEventPositionData(from), new ResourceEvents.ResourceEventPositionData(to.Tile.Coordinates), onComplete, reason, new ResourceEvents.ResourceEventResultData(to, onResourcesAdded, onComplete, reason));
	}

	public static void AddResourceOfType(byte playerId, Type type, float amount, WorldCoordinates from, Action onComplete = null, string reason = "None")
	{
		if (loggingEnable)
		{
			Log.Verbose("AddResourceOfTypeToResourceBar :: onComplete : {0}", new object[1] { onComplete != null });
		}
		AddResourceOfType(playerId, type, amount, new ResourceEvents.ResourceEventPositionData(from), null, onComplete, reason);
	}

	public static void AddResourceOfType(byte playerId, Type type, float amount, WorldCoordinates from, WorldCoordinates to, Action onComplete = null, string reason = "None")
	{
		AddResourceOfType(playerId, type, amount, new ResourceEvents.ResourceEventPositionData(from), new ResourceEvents.ResourceEventPositionData(to), onComplete, reason);
	}

	public static void AddResourceOfType(byte playerId, Type type, float amount, WorldCoordinates from, Building to, Action onComplete = null, string reason = "None")
	{
		AddResourceOfType(playerId, type, amount, new ResourceEvents.ResourceEventPositionData(from), new ResourceEvents.ResourceEventPositionData(to.Tile.Coordinates), onComplete, reason, new ResourceEvents.ResourceEventResultData(to, null, onComplete, reason));
	}

	public static void AddResourceOfType(byte playerId, Type type, float amount, ResourceEvents.ResourceEventPositionData from, ResourceEvents.ResourceEventPositionData to, Action onComplete = null, string reason = "None", ResourceEvents.ResourceEventResultData resultData = null)
	{
		if (loggingEnable)
		{
			Log.Verbose("AddResourceOfType :: onComplete : {0} :: amount : {1} :: type : {2}", new object[3]
			{
				onComplete != null,
				amount,
				type
			});
		}
		if (amount <= 0f)
		{
			onComplete?.Invoke();
			return;
		}
		bool flag = true;
		WorldIconContainer.ScoreElementTypes scoreElementType = WorldIconContainer.ScoreElementTypes.None;
		if (MapRenderer.Current.IsRendered && playerId == GameManager.LocalPlayer.Id)
		{
			switch (type)
			{
			case Type.Score:
				scoreElementType = WorldIconContainer.ScoreElementTypes.Label;
				if (resultData == null)
				{
					resultData = new ResourceEvents.ResourceEventResultData(ResourceEvents.ResourceEventResultData.ResultType.ReturnToResourceManager, onComplete, reason);
				}
				flag = false;
				break;
			case Type.Currency:
				scoreElementType = WorldIconContainer.ScoreElementTypes.Icon;
				if (resultData == null)
				{
					resultData = new ResourceEvents.ResourceEventResultData(ResourceEvents.ResourceEventResultData.ResultType.ReturnToResourceManager, onComplete, reason);
				}
				flag = false;
				break;
			case Type.Population:
				scoreElementType = WorldIconContainer.ScoreElementTypes.Icon;
				if (resultData == null)
				{
					resultData = new ResourceEvents.ResourceEventResultData(MapRenderer.Current.GetTileInstance(to.worldCoordinate).Improvement, null, onComplete, reason);
				}
				flag = false;
				break;
			case Type.Production:
				scoreElementType = WorldIconContainer.ScoreElementTypes.IconWithLabel;
				if (resultData == null)
				{
					resultData = new ResourceEvents.ResourceEventResultData(ResourceEvents.ResourceEventResultData.ResultType.UpdateIncome, onComplete, reason);
				}
				flag = false;
				break;
			}
		}
		if (loggingEnable)
		{
			Log.Verbose("AddResourceOfType :: addResourcesNow : {0} ", new object[1] { flag });
		}
		if (flag)
		{
			AddResourceOfType(playerId, type, amount, onComplete, reason);
		}
		else
		{
			ResourceEvents.AddResourceToUI(new ResourceEvents.ResourceEventData(playerId, type, scoreElementType, amount, from, to, resultData, reason));
		}
	}

	public static void AddResourceOfType(byte playerId, Type type, float amount, Action onComplete = null, string reason = "None")
	{
		if (amount <= 0f)
		{
			onComplete?.Invoke();
			return;
		}
		if (loggingEnable)
		{
			Log.Verbose("AddResourceOfType\nPlayer : {0}\nReason: {1}\nNew Resource :\nType : {2}\namount : {3}", new object[4] { playerId, reason, type, amount });
		}
		LocalWallet wallet = GetWallet(playerId);
		float total = 0f;
		switch (type)
		{
		case Type.Score:
			wallet.score += (int)amount;
			total = wallet.score;
			break;
		case Type.Currency:
			wallet.currency += (int)amount;
			total = wallet.currency;
			if (GameManager.IsPlayerViewing(playerId) && !GameManager.Client.IsRecap)
			{
				AnalyticsHelpers.AddCurrency(amount);
			}
			break;
		case Type.Production:
			IncomeChanged(playerId);
			break;
		}
		ResourceEvents.ResourceAdded(playerId, type, amount, total);
		if (loggingEnable)
		{
			Log.Verbose("AddResourceOfType :: onComplete valid : {0}", new object[1] { onComplete != null });
		}
		onComplete?.Invoke();
	}

	public static void RemoveResourceOfType(byte playerId, Type type, float amount, Action onComplete = null, string reason = "None")
	{
		if (amount <= 0f)
		{
			onComplete?.Invoke();
			return;
		}
		if (loggingEnable)
		{
			Log.Verbose("RemoveResourceOfType\nPlayer : {0}\nReason: {1}\nNew Resource :\nType : {2}\namount : {3}", new object[4] { playerId, reason, type, amount });
		}
		LocalWallet wallet = GetWallet(playerId);
		float total = 0f;
		switch (type)
		{
		case Type.Score:
			wallet.score -= (int)amount;
			total = wallet.score;
			break;
		case Type.Currency:
			wallet.currency -= (int)amount;
			total = wallet.currency;
			if (GameManager.IsPlayerViewing(playerId) && !GameManager.Client.IsRecap)
			{
				GameManager.GetAnalyticsManager().SendEvent("spend_virtual_currency", new Dictionary<string, object>
				{
					{ "value", amount },
					{
						"virtual_currency_name",
						type.ToString().ToLowerInvariant()
					},
					{
						"game_id",
						GameManager.Client.CurrentGameId
					}
				});
			}
			break;
		case Type.Production:
			IncomeChanged(playerId);
			break;
		}
		ResourceEvents.ResourceRemoved(playerId, type, amount, total);
		if (loggingEnable)
		{
			Log.Verbose("RemoveResourceOfType :: onComplete valid : {0}", new object[1] { onComplete != null });
		}
		onComplete?.Invoke();
	}

	public static void AddResources(byte playerId, List<Rewards> rewards, WorldCoordinates from, Action onResourcesAdded = null, Action onComplete = null, string reason = "None")
	{
		if (rewards != null && rewards.Count > 0)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (Rewards reward in rewards)
			{
				num += reward.population;
				num2 += reward.score;
				num3 += reward.currency;
			}
			if (num > 0)
			{
				TileData tile = GameManager.GameState.Map.GetTile(from);
				AddPopulationToCity(playerId, num, from, MapRenderer.Current.GetTileInstance(tile.rulingCityCoordinates).Improvement, onResourcesAdded, onComplete, reason);
			}
			else if (num3 > 0)
			{
				AddResourceOfTypeToResourceBar(playerId, Type.Currency, num3, from, onComplete, reason);
			}
			else if (num2 > 0)
			{
				AddResourceOfTypeToResourceBar(playerId, Type.Score, num2, from, onComplete, reason);
			}
			else
			{
				onComplete?.Invoke();
			}
		}
		else
		{
			onComplete?.Invoke();
		}
	}

	public static void AddResources(byte playerId, List<Rewards> rewards, Action onComplete = null, string reason = "None")
	{
		bool flag = false;
		foreach (Rewards reward in rewards)
		{
			if (reward.currency > 0)
			{
				AddResourceOfType(playerId, Type.Currency, reward.currency, onComplete, reason);
				flag = true;
			}
			if (reward.population > 0)
			{
				AddResourceOfType(playerId, Type.Population, reward.population, onComplete, reason);
				flag = true;
			}
			if (reward.score > 0)
			{
				AddResourceOfType(playerId, Type.Score, reward.score, onComplete, reason);
				flag = true;
			}
		}
		if (!flag)
		{
			onComplete?.Invoke();
		}
	}

	public static void IncomeChanged(byte playerId)
	{
		ResourceEvents.IncomeChanged(playerId);
	}

	public static float GetResourceOfType(byte playerId, Type type)
	{
		LocalWallet wallet = GetWallet(playerId);
		return type switch
		{
			Type.Score => wallet.score, 
			Type.Currency => wallet.currency, 
			_ => 0f, 
		};
	}

	public static bool HaveEnoughResources(byte playerId, Type type, float amount)
	{
		LocalWallet wallet = GetWallet(playerId);
		if (GameManager.GameState.TryGetPlayer(playerId, out var playerState))
		{
			switch (type)
			{
			case Type.Score:
				return amount <= (float)playerState.score;
			case Type.Currency:
				if (amount <= (float)playerState.Currency)
				{
					return amount <= (float)wallet.currency;
				}
				return false;
			}
		}
		return false;
	}

	protected static LocalWallet GetWallet(byte playerId)
	{
		if (!localWallets.TryGetValue(playerId, out var value))
		{
			value = new LocalWallet(playerId);
			localWallets.Add(playerId, value);
		}
		return value;
	}

	protected static bool DoesWalletExist(byte playerId)
	{
		return localWallets.ContainsKey(playerId);
	}

	private static void OnRefreshWallets(byte playerId)
	{
		AnalyticsHelpers.SendCurrencyEvent();
		GetWallet(playerId).SyncWithState();
		ResourceEvents.ResourceChanged(playerId);
	}

	protected static void CmdToggleLog(string[] args)
	{
		loggingEnable = !loggingEnable;
	}
}
