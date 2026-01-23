using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polytopia.Data;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PullToRefresh;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreScreen : UIScreenBase
{
	public class HighscoreDeepLinkData : UIDeepLinkData
	{
		public TribeData.Type selectedTribe = TribeData.Type.Aimo;

		public HighscoreDeepLinkData()
		{
			screen = UIConstants.Screens.Highscore;
		}

		public HighscoreDeepLinkData(TribeData.Type selectedTribe)
		{
			screen = UIConstants.Screens.Highscore;
			this.selectedTribe = selectedTribe;
		}
	}

	[Header("Highscore")]
	[SerializeField]
	protected UIRefreshControl refresher;

	[SerializeField]
	protected UIHorizontalList horizontalList;

	[SerializeField]
	protected RectTransform content;

	[SerializeField]
	protected LayoutElement contentLayoutElement;

	[SerializeField]
	protected TextMeshProUGUI infoLabel;

	[Header("Prefabs")]
	[SerializeField]
	protected StatsRow statsRowPrefab;

	protected bool initializedInternally;

	protected bool isLoading;

	protected int selectedCategoryIndex;

	protected HighscoreViewModel[][] highScoreLists;

	protected Dictionary<int, int?> tribeMap = new Dictionary<int, int?>();

	protected List<StatsRow> currentRows = new List<StatsRow>();

	protected Stack<StatsRow> cachedRows = new Stack<StatsRow>();

	private bool isConnectingBannerShowing;

	public override async void Show(bool instant = false)
	{
		base.Show(instant);
		if (!initializedInternally)
		{
			Initialize();
		}
		else
		{
			horizontalList.RefreshScrollpositions();
		}
		CurrentSelectable = horizontalList.GetCurrentSelectable();
		PolytopiaInput.Omnicursor.AffixToUIElement(horizontalList.items[horizontalList.SelectedIndex].rectTransform);
		ClearList();
		ConnectionStatus connectionStatus = PolytopiaBackendAdapter.Instance.ConnectionStatus;
		if ((uint)(connectionStatus - 1) > 1u)
		{
			await LoadHighScoreData();
		}
		else
		{
			NetworkUtils.ShowLoader(Localization.Get("backend.connecting"), 0);
		}
		OnScreenUpdated();
	}

	public override void Hide(bool instant = false)
	{
		NetworkUtils.HideLoader();
		isConnectingBannerShowing = false;
		base.Hide(instant);
	}

	public override void SetDeepLinkData(UIDeepLinkData data)
	{
		base.SetDeepLinkData(data);
		HighscoreDeepLinkData highscoreDeepLinkData = data as HighscoreDeepLinkData;
		List<TribeData> allTribes = PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).GetAllTribes();
		int num = 0;
		int count = allTribes.Count;
		for (int i = 0; i < count; i++)
		{
			if (allTribes[i].type == highscoreDeepLinkData.selectedTribe)
			{
				num = i + 1;
				break;
			}
		}
		selectedCategoryIndex = num;
		if (!initializedInternally)
		{
			Initialize();
		}
	}

	private void OnEnable()
	{
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChanged;
		NotificationManager.SetNetworkAlertPosition(-154f);
		StartSceneBg.Bright = false;
	}

	private void OnDisable()
	{
		BackendEvents.OnBackendConnectionChanged -= OnBackendConnectionChanged;
		NotificationManager.ResetNetworkAlertPosition();
	}

	private async void OnBackendConnectionChanged(ConnectionStatus status)
	{
		switch (status)
		{
		case ConnectionStatus.None:
		case ConnectionStatus.Connecting:
		case ConnectionStatus.Reconnecting:
		case ConnectionStatus.SocialConnected:
			isConnectingBannerShowing = true;
			NetworkUtils.ShowLoader(Localization.Get("backend.connecting"), 0);
			break;
		case ConnectionStatus.Connected:
		case ConnectionStatus.Reconnected:
			if (isConnectingBannerShowing)
			{
				NetworkUtils.HideLoader();
				isConnectingBannerShowing = false;
			}
			await LoadHighScoreData();
			break;
		case ConnectionStatus.ConnectionFailed:
		case ConnectionStatus.Disconnected:
			((TMP_Text)infoLabel).text = Localization.Get("backend.connecting.failed");
			((Component)infoLabel).gameObject.SetActive(highScoreLists[selectedCategoryIndex] == null);
			break;
		}
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)contentLayoutElement != (Object)null)
		{
			contentLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}

	private void Initialize()
	{
		if (initializedInternally)
		{
			return;
		}
		List<string> list = new List<string>();
		list.Add(Localization.Get("highscore.alltribes"));
		tribeMap.Add(list.Count - 1, null);
		foreach (TribeData allTribe in PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).GetAllTribes())
		{
			list.Add(Localization.Get(allTribe.displayName));
			tribeMap.Add(list.Count - 1, (int)allTribe.type);
		}
		highScoreLists = new HighscoreViewModel[list.Count][];
		horizontalList.HeaderKey = "";
		horizontalList.SetData(list.ToArray(), selectedCategoryIndex);
		horizontalList.IndexSelectedCallback = OnhorizontalListChanged;
		((Component)infoLabel).gameObject.SetActive(false);
		initializedInternally = true;
	}

	private async Task LoadHighScoreData(bool refresh = false)
	{
		if (isLoading)
		{
			return;
		}
		Log.Info("Loading highscore data for {0}", new object[1] { tribeMap[selectedCategoryIndex] });
		ClearList();
		isLoading = true;
		if (highScoreLists[selectedCategoryIndex] == null || refresh)
		{
			Log.Verbose("Fetching highscores from server...", Array.Empty<object>());
			NetworkUtils.ShowLoader(Localization.Get("highscore.loading"), (!refresh) ? 10 : 0);
			TribeHighscoresBindingModel model = new TribeHighscoresBindingModel
			{
				TribeType = tribeMap[selectedCategoryIndex]
			};
			ServerResponseList<HighscoreViewModel> serverResponseList = ((!PolytopiaBackendAdapter.Instance.IsConnected) ? (await PolytopiaBackendAdapter.Instance.GetHighscoresHttp(model)) : (await PolytopiaBackendAdapter.Instance.GetHighscores(model)));
			ServerResponseList<HighscoreViewModel> serverResponseList2 = serverResponseList;
			NetworkUtils.HideLoader();
			if (ShowState != ShowStates.Showing)
			{
				isLoading = false;
				return;
			}
			if (!serverResponseList2.Success)
			{
				Log.Warning("Data failed to download: {0}", new object[1] { serverResponseList2.ErrorMessage });
				NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(serverResponseList2));
			}
			else
			{
				highScoreLists[selectedCategoryIndex] = serverResponseList2.Data.ToArray();
				if (refresh)
				{
					NetworkUtils.ShowLoader(Localization.Get("highscore.uptodate"), 0, 2f);
				}
			}
		}
		else
		{
			NetworkUtils.HideLoader();
		}
		int num = ((highScoreLists[selectedCategoryIndex] != null) ? highScoreLists[selectedCategoryIndex].Length : 0);
		bool flag = num > 0;
		((TMP_Text)infoLabel).text = Localization.Get("highscore.notavailable");
		((Component)infoLabel).gameObject.SetActive(!flag);
		for (int i = 0; i < num; i++)
		{
			HighscoreViewModel highscoreViewModel = highScoreLists[selectedCategoryIndex][i];
			TribeData.Type tribeType = (TribeData.Type)highscoreViewModel.TribeType;
			SerializationHelpers.FromByteArray<AvatarState>(highscoreViewModel.AvatarStateData, out var result);
			StatsRow statsRow = GetStatsRow();
			if (result == null)
			{
				statsRow.iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(tribeType));
			}
			else
			{
				statsRow.SetAvatarState(result);
			}
			string username = highscoreViewModel.Username;
			statsRow.StatsName = $"{i + 1}. {username}";
			statsRow.StatsValue = LocalizationUtils.FormatNumber(highscoreViewModel.Score);
			statsRow.BgColor = ColorConstants.blue;
			if (highscoreViewModel.PolytopiaUserId == AccountManager.PlayerAccountId)
			{
				statsRow.BgVisible = true;
			}
			else
			{
				statsRow.BgVisible = false;
			}
			((Transform)statsRow.rectTransform).SetSiblingIndex(i);
		}
		isLoading = false;
	}

	public void OnRefreshTrigger()
	{
		NetworkUtils.ShowLoader(Localization.Get("onlineview.reloading.release"), 0);
	}

	public void OnRefreshCancelled()
	{
		NetworkUtils.HideLoader();
	}

	public async void OnRefreshGames()
	{
		await LoadHighScoreData(refresh: true);
		refresher.EndRefreshing();
	}

	private StatsRow GetStatsRow()
	{
		StatsRow statsRow;
		if (cachedRows.Count == 0)
		{
			statsRow = Object.Instantiate<StatsRow>(statsRowPrefab, (Transform)(object)content);
			statsRow.Description = string.Empty;
		}
		else
		{
			statsRow = cachedRows.Pop();
			((Component)statsRow).gameObject.SetActive(true);
		}
		currentRows.Add(statsRow);
		return statsRow;
	}

	private void ClearList()
	{
		foreach (StatsRow currentRow in currentRows)
		{
			cachedRows.Push(currentRow);
			((Component)currentRow).gameObject.SetActive(false);
		}
		currentRows.Clear();
	}

	private async void OnhorizontalListChanged(int idx)
	{
		if (selectedCategoryIndex != idx)
		{
			selectedCategoryIndex = idx;
			await LoadHighScoreData();
		}
	}
}
