using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;

public class ReplaysManager
{
	public const int MAX_REPLAYS = 10;

	public const int MAX_RECENT_REPLAYS = 5;

	private Task<ServerResponseList<GameSummaryViewModel>> updateRecentsTask;

	private Task<ServerResponseList<GameSummaryViewModel>> updateFavoritesTask;

	private List<GameSummaryViewModel> recentReplays;

	private List<GameSummaryViewModel> favoriteReplays;

	private GameSummaryViewModel currentReplaySummary;

	public bool HasCachedReplays
	{
		get
		{
			if (recentReplays == null)
			{
				return favoriteReplays != null;
			}
			return true;
		}
	}

	public async Task<bool> UpdateReplays()
	{
		bool flag = await UpdateRecentReplays();
		if (flag)
		{
			flag = await UpdateFavoriteReplays();
		}
		return flag;
	}

	private async Task<bool> UpdateRecentReplays()
	{
		if (updateRecentsTask == null || updateRecentsTask.IsCompleted)
		{
			updateRecentsTask = PolytopiaBackendAdapter.Instance.GetRecentGames(new RecentGamesBindingModel
			{
				Limit = 5
			});
		}
		ServerResponseList<GameSummaryViewModel> serverResponseList = await updateRecentsTask;
		if (!serverResponseList.Success)
		{
			NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(serverResponseList));
			return false;
		}
		recentReplays = new List<GameSummaryViewModel>();
		for (int i = 0; i < serverResponseList.Data.Count; i++)
		{
			if (IsReplayValid(serverResponseList.Data[i]))
			{
				recentReplays.Add(serverResponseList.Data[i]);
			}
		}
		return true;
	}

	private async Task<bool> UpdateFavoriteReplays()
	{
		if (updateFavoritesTask == null || updateFavoritesTask.IsCompleted)
		{
			updateFavoritesTask = PolytopiaBackendAdapter.Instance.GetSavedGames();
		}
		ServerResponseList<GameSummaryViewModel> serverResponseList = await updateFavoritesTask;
		if (!serverResponseList.Success)
		{
			NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(serverResponseList));
			return false;
		}
		favoriteReplays = new List<GameSummaryViewModel>();
		for (int i = 0; i < serverResponseList.Data.Count; i++)
		{
			if (IsReplayValid(serverResponseList.Data[i]))
			{
				favoriteReplays.Add(serverResponseList.Data[i]);
			}
		}
		return true;
	}

	public static bool IsReplayValid(GameSummaryViewModel gameSummaryViewModel)
	{
		GameStateSummary result;
		return SerializationHelpers.FromByteArray<GameStateSummary>(gameSummaryViewModel.GameSummaryData, out result);
	}

	public async Task<List<GameSummaryViewModel>> GetRecentReplays(bool forceUpdate = false)
	{
		if (recentReplays == null || forceUpdate)
		{
			await UpdateRecentReplays();
		}
		return recentReplays;
	}

	public async Task<List<GameSummaryViewModel>> GetFavoriteReplays(bool forceUpdate = false)
	{
		if (favoriteReplays == null || forceUpdate)
		{
			await UpdateFavoriteReplays();
		}
		return favoriteReplays;
	}

	public async Task<bool> IsFavoriteReplay(Guid gameId)
	{
		List<GameSummaryViewModel> list = await GetFavoriteReplays();
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].GameId == gameId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public GameSummaryViewModel GetCurrentReplaySummary()
	{
		return currentReplaySummary;
	}

	public void SetCurrentReplaySummary(GameSummaryViewModel summaryViewModel)
	{
		currentReplaySummary = summaryViewModel;
	}

	public async Task<ErrorCode?> SetCachedFavoriteReplay(GameSummaryViewModel summaryViewModel, bool save)
	{
		if (save && (favoriteReplays == null || favoriteReplays.Count >= 10))
		{
			return ErrorCode.FavouriteLimitReached;
		}
		ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.SaveGame(new SaveGameBindingModel
		{
			GameId = summaryViewModel.GameId,
			Save = save
		});
		if (!serverResponse.Success)
		{
			return serverResponse.ErrorCode;
		}
		if (save)
		{
			if (favoriteReplays == null)
			{
				favoriteReplays = new List<GameSummaryViewModel>();
			}
			favoriteReplays.Add(summaryViewModel);
		}
		else if (favoriteReplays != null && favoriteReplays.Contains(summaryViewModel))
		{
			favoriteReplays.Remove(summaryViewModel);
		}
		return null;
	}

	public bool IsReplay(Guid gameId)
	{
		if (recentReplays != null)
		{
			for (int i = 0; i < recentReplays.Count; i++)
			{
				if (recentReplays[i].GameId == gameId)
				{
					return true;
				}
			}
		}
		if (favoriteReplays != null)
		{
			for (int j = 0; j < favoriteReplays.Count; j++)
			{
				if (favoriteReplays[j].GameId == gameId)
				{
					return true;
				}
			}
		}
		return false;
	}
}
