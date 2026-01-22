using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.UI;

public class NewsScreen : UIScreenBase
{
	public class NewsData
	{
		public List<NewsItemData> news;
	}

	public class NewsItemData
	{
		public string id;

		public long date;

		public string body;

		public string image;

		public string link;
	}

	public const int MAX_NEWS_ITEMS = 25;

	[Header("New Screen")]
	[SerializeField]
	protected RectTransform container;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[Header("Prefabs")]
	[SerializeField]
	protected NewsListItem newsItemPrefab;

	private List<NewsItem> newsData;

	protected NewsListItem[] items;

	protected bool foundValidSelectable;

	public int NewsCount
	{
		get
		{
			if (newsData != null && newsData.Count > 0)
			{
				return newsData.Count;
			}
			return 0;
		}
	}

	public override async void Show(bool instant = false)
	{
		StartSceneBg.Bright = false;
		base.Show(instant);
		GameManager.GetAnalyticsManager().SendEvent("news", new Dictionary<string, object> { 
		{
			"badge",
			(GameManager.UnreadNewsCount > 0) ? "on" : "off"
		} });
		if (newsData == null)
		{
			await LoadNewsData();
		}
		else if (items != null)
		{
			for (int i = 0; i < items.Length; i++)
			{
				items[i].ReloadImage();
			}
			if (items.Length != 0)
			{
				PolytopiaInput.Omnicursor.AffixToUIElement(items[0].rectTransform);
			}
		}
	}

	public override void Hide(bool instant = false)
	{
		base.Hide(instant);
		if (items != null)
		{
			for (int i = 0; i < items.Length; i++)
			{
				items[i].Clear();
			}
		}
	}

	private async Task LoadNewsData()
	{
		NetworkUtils.ShowLoader();
		ServerResponse<NewsObject> serverResponse = await PolytopiaBackendAdapter.Instance.GetNews(null);
		if (serverResponse.Success)
		{
			newsData = serverResponse.Data.News;
			await CacheManager.CacheNews(newsData);
			NetworkUtils.HideLoader();
		}
		else
		{
			NetworkUtils.ShowLoaderError(Localization.Get("news.error.failed"));
			Log.Warning("Failed to load news: {0} {1}", new object[2] { serverResponse.ErrorMessage, serverResponse.ErrorCode });
			newsData = CacheManager.GetCachedNews();
		}
		if (newsData != null)
		{
			PopulateData();
			if (items.Length != 0)
			{
				PolytopiaInput.Omnicursor.AffixToUIElement(items[0].rectTransform);
			}
			PolytopiaPlayerPrefs.SetString("polytopia_last_seen_news", DateTime.UtcNow.ToString("O"));
			PolytopiaPlayerPrefs.Save();
			GameManager.UnreadNewsCount = 0;
		}
	}

	private void PopulateData()
	{
		if (newsData == null || newsData.Count <= 0)
		{
			return;
		}
		int num = Mathf.Min(newsData.Count, 25);
		items = new NewsListItem[num];
		for (int i = 0; i < num; i++)
		{
			NewsListItem newsListItem = Object.Instantiate<NewsListItem>(newsItemPrefab, (Transform)(object)container);
			newsListItem.SetData(newsData[i]);
			if (!foundValidSelectable && newsListItem.ButtonEnabled)
			{
				foundValidSelectable = true;
				UINavigationManager.Select((Selectable)(object)newsListItem.button);
			}
			newsListItem.UpdateScrollerOnHighlight = true;
			items[i] = newsListItem;
		}
		PolytopiaPlayerPrefs.SetInt("viewedNewsCount", num);
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}
}
