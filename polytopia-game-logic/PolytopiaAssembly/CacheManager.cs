using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polytopia.IO;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

public static class CacheManager
{
	private static T GetCachedItem<T>(string path)
	{
		try
		{
			if (PolytopiaFile.Exists(path))
			{
				return JsonConvert.DeserializeObject<T>(PolytopiaFile.ReadAllText(path));
			}
			return default(T);
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to read cache at {0} with error {1}", new object[2]
			{
				path,
				ex.ToString()
			});
			return default(T);
		}
	}

	public static PolytopiaUserViewModel GetCachedUserViewModel()
	{
		return GetCachedItem<PolytopiaUserViewModel>(Paths.GetUserProfileCachePath());
	}

	public static VersioningViewModel GetCachedVersioningViewModel()
	{
		return GetCachedItem<VersioningViewModel>(Paths.GetVersioningCachePath());
	}

	public static TribeRatingsViewModel GetCachedTribeRatingsViewModel()
	{
		return GetCachedItem<TribeRatingsViewModel>(Paths.GetTribeRatingsCachePath());
	}

	public static List<NewsItem> GetCachedNews()
	{
		return GetCachedItem<List<NewsItem>>(Paths.GetNewsCachePath());
	}

	public static int GetCachedNumSingleplayerGames()
	{
		return GetCachedItem<int>(Paths.GetNumSingleplayerGamesCachePath());
	}

	private static async Task CacheItem<T>(T item, string path)
	{
		try
		{
			if (PolytopiaFile.Exists(path))
			{
				PolytopiaFile.Delete(path);
			}
			string value = JsonConvert.SerializeObject((object)item);
			using StreamWriter writer = new StreamWriter(PolytopiaFile.Open(path, FileMode.Create));
			await writer.WriteAsync(value);
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to cache data at {0} with error {1}", new object[2]
			{
				path,
				ex.ToString()
			});
		}
	}

	public static async Task CacheUserProfileData(PolytopiaUserViewModel polytopiaUserViewModel)
	{
		await CacheItem(polytopiaUserViewModel, Paths.GetUserProfileCachePath());
	}

	public static async Task CacheVersioningViewModel(VersioningViewModel versioningViewModel)
	{
		await CacheItem(versioningViewModel, Paths.GetVersioningCachePath());
	}

	public static async Task CacheTribeRatingsViewModel(TribeRatingsViewModel tribeRatingsViewModel)
	{
		await CacheItem(tribeRatingsViewModel, Paths.GetTribeRatingsCachePath());
	}

	public static async Task CacheNumSinglePlayerGames(int count)
	{
		await CacheItem(count, Paths.GetNumSingleplayerGamesCachePath());
	}

	public static async Task CacheNews(List<NewsItem> newsItems)
	{
		await CacheItem(newsItems, Paths.GetNewsCachePath());
	}

	public static void ClearVersioningViewModelCache()
	{
		string versioningCachePath = Paths.GetVersioningCachePath();
		if (PolytopiaFile.Exists(versioningCachePath))
		{
			PolytopiaFile.Delete(versioningCachePath);
		}
	}

	public static void ClearTribeRatingsViewModelCache()
	{
		string tribeRatingsCachePath = Paths.GetTribeRatingsCachePath();
		if (PolytopiaFile.Exists(tribeRatingsCachePath))
		{
			PolytopiaFile.Delete(tribeRatingsCachePath);
		}
	}
}
