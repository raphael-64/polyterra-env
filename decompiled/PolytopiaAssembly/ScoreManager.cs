using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polytopia.Data;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;

public class ScoreManager
{
	private static TribeRatingsViewModel cachedRatings;

	public static bool TryGetTribeScore(TribeData.Type tribe, out int score)
	{
		score = -1;
		if (cachedRatings == null)
		{
			cachedRatings = CacheManager.GetCachedTribeRatingsViewModel();
		}
		if (cachedRatings == null || cachedRatings.Ratings == null)
		{
			return false;
		}
		if (cachedRatings.Ratings.TryGetValue((int)tribe, out var value) && value.Score.HasValue)
		{
			score = (int)value.Score.Value;
			return true;
		}
		return false;
	}

	public static bool TryGetTribeRating(TribeData.Type tribe, out int rating)
	{
		rating = -1;
		if (cachedRatings == null)
		{
			cachedRatings = CacheManager.GetCachedTribeRatingsViewModel();
		}
		if (cachedRatings == null || cachedRatings.Ratings == null)
		{
			return false;
		}
		if (cachedRatings.Ratings.TryGetValue((int)tribe, out var value) && value.Rating.HasValue)
		{
			rating = (int)value.Rating.Value;
			return true;
		}
		return false;
	}

	public static TopTribe GetTopTribe()
	{
		if (cachedRatings == null)
		{
			cachedRatings = CacheManager.GetCachedTribeRatingsViewModel();
		}
		if (cachedRatings == null || cachedRatings.Ratings == null)
		{
			return new TopTribe(0u, TribeData.Type.None);
		}
		uint num = 0u;
		TribeData.Type tribeType = TribeData.Type.None;
		foreach (KeyValuePair<int, TribeRatingViewModel> rating in cachedRatings.Ratings)
		{
			if (rating.Value.Score.HasValue)
			{
				uint value = rating.Value.Score.Value;
				if (value > num)
				{
					num = value;
					tribeType = (TribeData.Type)rating.Value.TribeType;
				}
			}
		}
		return new TopTribe(num, tribeType);
	}

	public static uint GetTopScore()
	{
		uint num = 0u;
		if (cachedRatings == null)
		{
			cachedRatings = CacheManager.GetCachedTribeRatingsViewModel();
		}
		if (cachedRatings == null || cachedRatings.Ratings == null)
		{
			return num;
		}
		foreach (KeyValuePair<int, TribeRatingViewModel> rating in cachedRatings.Ratings)
		{
			if (rating.Value.Score.HasValue)
			{
				uint value = rating.Value.Score.Value;
				if (value > num)
				{
					num = value;
				}
			}
		}
		return num;
	}

	public static uint GetTopRating()
	{
		uint num = 0u;
		if (cachedRatings == null)
		{
			cachedRatings = CacheManager.GetCachedTribeRatingsViewModel();
		}
		if (cachedRatings == null || cachedRatings.Ratings == null)
		{
			return num;
		}
		foreach (KeyValuePair<int, TribeRatingViewModel> rating in cachedRatings.Ratings)
		{
			if (rating.Value.Rating.HasValue)
			{
				uint value = rating.Value.Rating.Value;
				if (value > num)
				{
					num = value;
				}
			}
		}
		return num;
	}

	public static async void SetTribeScore(TribeData.Type tribe, uint score, bool force = false)
	{
		Log.Verbose("[ScoreManager] Set tribe score for {0}: {1}", new object[2] { tribe, score });
		if (cachedRatings == null)
		{
			cachedRatings = CacheManager.GetCachedTribeRatingsViewModel();
		}
		if (cachedRatings == null)
		{
			cachedRatings = new TribeRatingsViewModel();
		}
		if (cachedRatings.Ratings == null)
		{
			cachedRatings.Ratings = new Dictionary<int, TribeRatingViewModel>();
		}
		bool flag = false;
		if (cachedRatings.Ratings.TryGetValue((int)tribe, out var tribeRatingViewModel))
		{
			if (tribeRatingViewModel.Score.HasValue && (tribeRatingViewModel.Score.Value > score || force))
			{
				Log.Verbose("[ScoreManager] Rating ({0}) was lower than current value ({2}, has value: {1})", new object[3]
				{
					score,
					tribeRatingViewModel.Score.HasValue,
					tribeRatingViewModel.Score.Value
				});
			}
			else
			{
				cachedRatings.Ratings[(int)tribe].Score = score;
				flag = true;
				Log.Verbose("[ScoreManager] Updated score for tribe {0}: {1}", new object[2] { tribe, score });
			}
		}
		else
		{
			tribeRatingViewModel = new TribeRatingViewModel
			{
				TribeType = (int)tribe,
				Score = score
			};
			cachedRatings.Ratings.Add((int)tribe, tribeRatingViewModel);
			flag = true;
			Log.Verbose("[ScoreManager] Added score for tribe {0}: {1}", new object[2] { tribe, score });
		}
		if (flag)
		{
			await CacheManager.CacheTribeRatingsViewModel(cachedRatings);
			if (await UploadTribeRating(tribeRatingViewModel))
			{
				Log.Verbose("[ScoreManager] Uploaded score for tribe {0}: {1}", new object[2] { tribe, score });
			}
		}
	}

	public static async void SetTribeRating(TribeData.Type tribe, uint rating, bool force = false)
	{
		Log.Verbose("[ScoreManager] Set tribe rating for {0}: {1}", new object[2] { tribe, rating });
		if (cachedRatings == null)
		{
			cachedRatings = CacheManager.GetCachedTribeRatingsViewModel();
		}
		if (cachedRatings == null)
		{
			cachedRatings = new TribeRatingsViewModel();
		}
		if (cachedRatings.Ratings == null)
		{
			cachedRatings.Ratings = new Dictionary<int, TribeRatingViewModel>();
		}
		bool flag = false;
		if (cachedRatings.Ratings.TryGetValue((int)tribe, out var tribeRatingViewModel))
		{
			if (tribeRatingViewModel.Rating.HasValue && (tribeRatingViewModel.Rating.Value > rating || force))
			{
				Log.Verbose("[ScoreManager] Rating ({0}) was lower than current value ({2}, has value: {1})", new object[3]
				{
					rating,
					tribeRatingViewModel.Rating.HasValue,
					tribeRatingViewModel.Rating.Value
				});
			}
			else
			{
				cachedRatings.Ratings[(int)tribe].Rating = rating;
				flag = true;
				Log.Verbose("[ScoreManager] Updated rating for tribe {0}: {1}", new object[2] { tribe, rating });
			}
		}
		else
		{
			tribeRatingViewModel = new TribeRatingViewModel
			{
				TribeType = (int)tribe,
				Rating = rating
			};
			cachedRatings.Ratings.Add((int)tribe, tribeRatingViewModel);
			flag = true;
			Log.Verbose("[ScoreManager] Added rating for tribe {0}: {1}", new object[2] { tribe, rating });
		}
		if (flag)
		{
			await CacheManager.CacheTribeRatingsViewModel(cachedRatings);
			if (await UploadTribeRating(tribeRatingViewModel))
			{
				Log.Verbose("[ScoreManager] Uploaded rating for tribe {0}: {1}", new object[2] { tribe, rating });
			}
		}
	}

	private static async Task<bool> UploadTribeRating(TribeRatingViewModel tribeRatingViewModel)
	{
		if (tribeRatingViewModel == null)
		{
			return false;
		}
		UploadTribeRatingBindingModel uploadTribeRatingBindingModel = new UploadTribeRatingBindingModel();
		uploadTribeRatingBindingModel.Entries = new Dictionary<int, TribeRatingViewModel> { { tribeRatingViewModel.TribeType, tribeRatingViewModel } };
		ServerResponse<ResponseViewModel> serverResponse = ((!PolytopiaBackendAdapter.Instance.IsConnected) ? (await PolytopiaBackendAdapter.Instance.UploadTribeRatingHttp(uploadTribeRatingBindingModel)) : (await PolytopiaBackendAdapter.Instance.UploadTribeRating(uploadTribeRatingBindingModel)));
		if (!serverResponse.Success)
		{
			Log.Warning("[ScoreManager] Failed to upload tribe ratings", Array.Empty<object>());
			return false;
		}
		return true;
	}

	public static async Task<bool> ClearScoresAndRatings()
	{
		NetworkUtils.ShowLoader();
		ServerResponse<ResponseViewModel> serverResponse = ((!PolytopiaBackendAdapter.Instance.IsConnected) ? (await PolytopiaBackendAdapter.Instance.ClearTribeRatingsHttp()) : (await PolytopiaBackendAdapter.Instance.ClearTribeRatings()));
		if (serverResponse.Success)
		{
			NetworkUtils.HideLoader();
			cachedRatings = null;
			CacheManager.ClearTribeRatingsViewModelCache();
			return true;
		}
		NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(serverResponse.ErrorCode));
		return false;
	}

	public static async Task<bool> SyncTribeRatings()
	{
		cachedRatings = CacheManager.GetCachedTribeRatingsViewModel();
		if (cachedRatings == null)
		{
			cachedRatings = new TribeRatingsViewModel();
		}
		bool uploadSuccess = false;
		if (cachedRatings.Ratings != null)
		{
			UploadTribeRatingBindingModel uploadTribeRatingBindingModel = new UploadTribeRatingBindingModel
			{
				Entries = new Dictionary<int, TribeRatingViewModel>()
			};
			foreach (KeyValuePair<int, TribeRatingViewModel> rating in cachedRatings.Ratings)
			{
				uploadTribeRatingBindingModel.Entries.Add(rating.Key, new TribeRatingViewModel
				{
					TribeType = rating.Key,
					Rating = rating.Value.Rating,
					Score = rating.Value.Score
				});
			}
			ServerResponse<ResponseViewModel> serverResponse = ((!PolytopiaBackendAdapter.Instance.IsConnected) ? (await PolytopiaBackendAdapter.Instance.UploadTribeRatingHttp(uploadTribeRatingBindingModel)) : (await PolytopiaBackendAdapter.Instance.UploadTribeRating(uploadTribeRatingBindingModel)));
			uploadSuccess = serverResponse.Success;
		}
		ServerResponse<TribeRatingsViewModel> serverResponse2 = ((!PolytopiaBackendAdapter.Instance.IsConnected) ? (await PolytopiaBackendAdapter.Instance.GetTribeRatingsHttp()) : (await PolytopiaBackendAdapter.Instance.GetTribeRatings()));
		if (serverResponse2.Success)
		{
			Log.Verbose("[ScoreManager] Successfully downloaded ratings", Array.Empty<object>());
			if (!uploadSuccess && cachedRatings.Ratings != null)
			{
				Log.Verbose("[ScoreManager] Upload failed, update local data with online data...", Array.Empty<object>());
				int num = 0;
				foreach (KeyValuePair<int, TribeRatingViewModel> rating2 in serverResponse2.Data.Ratings)
				{
					if (cachedRatings.Ratings.TryGetValue(rating2.Key, out var value))
					{
						if (value.Score.HasValue && rating2.Value.Score.HasValue && rating2.Value.Score.Value > value.Score.Value)
						{
							Log.Verbose("[ScoreManager] - Updated cached score ({0}) with downloaded score ({1})", new object[2]
							{
								value.Score.Value,
								rating2.Value.Score.Value
							});
							cachedRatings.Ratings[rating2.Key].Score = rating2.Value.Score;
							num++;
						}
						if (value.Rating.HasValue && rating2.Value.Rating.HasValue && rating2.Value.Rating.Value > value.Rating.Value)
						{
							Log.Verbose("[ScoreManager] - Updated cached rating ({0}) with downloaded rating ({1})", new object[2]
							{
								value.Rating.Value,
								rating2.Value.Rating.Value
							});
							cachedRatings.Ratings[rating2.Key].Rating = rating2.Value.Rating.Value;
							num++;
						}
					}
					else if (rating2.Value.Score.HasValue || rating2.Value.Rating.HasValue)
					{
						cachedRatings.Ratings.Add(rating2.Key, rating2.Value);
						if (rating2.Value.Score.HasValue)
						{
							Log.Verbose("[ScoreManager] - Added new score value ({0})", new object[1] { rating2.Value.Score.Value });
							num++;
						}
						if (rating2.Value.Score.HasValue)
						{
							Log.Verbose("[ScoreManager] - Added new rating value ({0})", new object[1] { rating2.Value.Rating.Value });
							num++;
						}
					}
				}
				Log.Verbose("[ScoreManager] Updated {0} values", new object[1] { num });
			}
			else
			{
				Log.Verbose("[ScoreManager] Local values updated", Array.Empty<object>());
				cachedRatings = serverResponse2.Data;
			}
		}
		else
		{
			Log.Verbose("[ScoreManager] Failed to download rating values", Array.Empty<object>());
		}
		await CacheManager.CacheTribeRatingsViewModel(cachedRatings);
		Log.Verbose("[ScoreManager] Successfully synced tribe ratings", Array.Empty<object>());
		return true;
	}

	public static async Task SyncNumSingleplayerGames(int? serverValue)
	{
		int localValue = CacheManager.GetCachedNumSingleplayerGames();
		if (!serverValue.HasValue || localValue > serverValue)
		{
			UploadNumSingleplayerGamesBindingModel model = new UploadNumSingleplayerGamesBindingModel
			{
				Count = localValue
			};
			ServerResponse<ResponseViewModel> serverResponse = ((!PolytopiaBackendAdapter.Instance.IsConnected) ? (await PolytopiaBackendAdapter.Instance.UploadNumSingleplayerGamesHttp(model)) : (await PolytopiaBackendAdapter.Instance.UploadNumSingleplayerGames(model)));
			if (serverResponse.Success)
			{
				await CacheManager.CacheNumSinglePlayerGames(localValue);
			}
		}
		else
		{
			await CacheManager.CacheNumSinglePlayerGames(serverValue.Value);
		}
	}

	public static int GetRatingLevel(int rating)
	{
		if (rating >= ScoreSheet.ratingLimits[2])
		{
			return 3;
		}
		if (rating >= ScoreSheet.ratingLimits[1])
		{
			return 2;
		}
		if (rating >= ScoreSheet.ratingLimits[0])
		{
			return 1;
		}
		return 0;
	}

	public static int GetScoreLevel(int score)
	{
		if (score >= ScoreSheet.scoreLimits[2])
		{
			return 3;
		}
		if (score >= ScoreSheet.scoreLimits[1])
		{
			return 2;
		}
		if (score >= ScoreSheet.scoreLimits[0])
		{
			return 1;
		}
		return 0;
	}

	public static int GetTimeGoal(GameState gameState)
	{
		return gameState.Settings.OpponentCount * 10;
	}

	public static float GetTimeScore(GameState gameState)
	{
		return (float)Math.Round(Math.Min(100.0, (double)GetTimeGoal(gameState) / (double)(int)Math.Max(gameState.CurrentTurn, 1u) * 100.0));
	}

	public static float GetBattleScore(PlayerState playerState)
	{
		return (float)Math.Round((double)(playerState.kills + 1) / (double)(playerState.kills + playerState.casualities + 1) * 100.0);
	}

	public static float GetWipeOutScore(GameState gameState, PlayerState playerState)
	{
		if (gameState.Settings.OpponentCount <= 0)
		{
			return 0f;
		}
		return (float)Math.Round((float)playerState.wipeOuts / (float)gameState.Settings.OpponentCount * 100f);
	}

	public static float GetDifficultyScore(GameState gameState)
	{
		return (float)Math.Round(((float)GameSettings.HandicapFromDifficulty(gameState.Settings.Difficulty) + 1f) / 5f * 100f);
	}
}
