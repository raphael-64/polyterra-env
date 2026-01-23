using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PullToRefresh;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReplaysScreen : UIScreenBase
{
	[Header("List")]
	[SerializeField]
	protected UIRefreshControl refresherGames;

	[SerializeField]
	protected RectTransform containerGames;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected LayoutGroup containerLayoutGroup;

	[Header("Multiplayer Selection Screen Connection")]
	[SerializeField]
	protected MultiplayerSelectionScreen multiplayerSelectionScreen;

	[Header("Prefabs")]
	[SerializeField]
	protected ReplayGameInfoRow replayGameInfoRowPrefab;

	[SerializeField]
	protected HeaderRow headerRowPrefab;

	[SerializeField]
	protected MultiplayerInfoRow infoRowPrefab;

	[SerializeField]
	protected ButtonRow buttonRowPrefab;

	protected List<UIBasicButton> rows = new List<UIBasicButton>();

	protected List<GameObject> otherRows = new List<GameObject>();

	protected List<IListCellNavigation> navigableCells = new List<IListCellNavigation>();

	private HeaderRow latestHeader;

	private HeaderRow favoritesHeader;

	private MultiplayerInfoRow noReplaysRow;

	public const int LATEST_GAMES_MAX = 5;

	private List<GameSummaryViewModel> favorites;

	private List<GameSummaryViewModel> latest;

	public override void Show(bool instant = false)
	{
		GameManager.Client?.Reset();
		multiplayerSelectionScreen.Show(instant: true);
		multiplayerSelectionScreen.UpdateScreenSelectionListSelectedIndex(UIConstants.Screens.ReplaysScreen);
		base.Show(instant);
		if (PolytopiaBackendAdapter.Instance.IsConnected && GameManager.IsNetworkEnabled())
		{
			Reload();
		}
		else
		{
			AddStartMessages();
		}
		OnScreenUpdated();
		if (rows.Count > 0)
		{
			PolytopiaInput.Omnicursor.AffixToUIElement(((Component)rows[0].button).GetComponent<RectTransform>());
		}
	}

	public override void Hide(bool instant = false)
	{
		base.Hide(instant);
		multiplayerSelectionScreen.Hide();
	}

	private void OnEnable()
	{
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChangedAsync;
	}

	private void OnDisable()
	{
		BackendEvents.OnBackendConnectionChanged -= OnBackendConnectionChangedAsync;
	}

	private async void OnBackendConnectionChangedAsync(ConnectionStatus status)
	{
		if (!((Object)(object)this == (Object)null))
		{
			await new WaitForUpdate();
			switch (status)
			{
			case ConnectionStatus.None:
			case ConnectionStatus.Connecting:
			case ConnectionStatus.Reconnecting:
			case ConnectionStatus.SocialConnected:
				AddStartMessages();
				break;
			case ConnectionStatus.Connected:
			case ConnectionStatus.Reconnected:
				await ReloadAsync(forceReload: true);
				break;
			case ConnectionStatus.ConnectionFailed:
			case ConnectionStatus.Disconnected:
				AddStartMessages();
				break;
			}
		}
	}

	private void AddStartMessages()
	{
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		ClearList();
		if (!GameManager.IsNetworkEnabled())
		{
			MultiplayerInfoRow multiplayerInfoRow = AddInfoRow();
			((TMP_Text)multiplayerInfoRow.header).text = Localization.Get("onlineview.nogames.intro", AccountManager.Alias);
			((TMP_Text)multiplayerInfoRow.description).text = Localization.Get("onlineview.loaderror");
			return;
		}
		switch (PolytopiaBackendAdapter.Instance.ConnectionStatus)
		{
		case ConnectionStatus.Connecting:
		case ConnectionStatus.Reconnecting:
		{
			MultiplayerInfoRow multiplayerInfoRow3 = AddInfoRow();
			((TMP_Text)multiplayerInfoRow3.header).text = Localization.Get("onlineview.nogames.intro", AccountManager.Alias);
			((TMP_Text)multiplayerInfoRow3.description).text = Localization.Get("onlineview.replays.loading");
			break;
		}
		case ConnectionStatus.ConnectionFailed:
		case ConnectionStatus.Disconnected:
		{
			MultiplayerInfoRow multiplayerInfoRow2 = AddInfoRow();
			((TMP_Text)multiplayerInfoRow2.header).text = Localization.Get("onlineview.nogames.intro", AccountManager.Alias);
			((TMP_Text)multiplayerInfoRow2.description).text = Localization.Get("onlineview.loaderror");
			AddButtonRow(Localization.Get("buttons.reconnect"), OnReconnectBackendAsync);
			break;
		}
		case ConnectionStatus.None:
		case ConnectionStatus.Connected:
		case ConnectionStatus.Reconnected:
			break;
		}
	}

	public void SetScreenPadding(int topPadding)
	{
		containerLayoutGroup.padding.top = topPadding;
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}

	protected MultiplayerInfoRow AddInfoRow()
	{
		MultiplayerInfoRow multiplayerInfoRow = Object.Instantiate<MultiplayerInfoRow>(infoRowPrefab, (Transform)(object)containerGames);
		otherRows.Add(((Component)multiplayerInfoRow).gameObject);
		return multiplayerInfoRow;
	}

	protected ButtonRow AddButtonRow()
	{
		ButtonRow buttonRow = Object.Instantiate<ButtonRow>(buttonRowPrefab, (Transform)(object)containerGames);
		otherRows.Add(((Component)buttonRow).gameObject);
		navigableCells.Add(buttonRow);
		return buttonRow;
	}

	protected ButtonRow AddButtonRow(string buttonText, UIButtonBase.ButtonAction action)
	{
		ButtonRow buttonRow = AddButtonRow();
		buttonRow.buttonComp.text = buttonText;
		buttonRow.buttonComp.OnClicked += action;
		return buttonRowPrefab;
	}

	public async void OnReconnectBackendAsync(int buttonId, BaseEventData eventData)
	{
		ClearList();
		MultiplayerInfoRow multiplayerInfoRow = AddInfoRow();
		((TMP_Text)multiplayerInfoRow.header).text = Localization.Get("onlineview.nogames.intro", AccountManager.Alias);
		((TMP_Text)multiplayerInfoRow.description).text = Localization.Get("onlineview.replays.loading");
		GameManager.GetLoginManager().Login(silent: false);
		await new WaitForUpdate();
	}

	public async Task ReloadAsync(bool forceReload = false)
	{
		if (GameManager.IsNetworkEnabled())
		{
			await LoadGamesAsync(forceReload);
			await multiplayerSelectionScreen.UpdateFriendsBadgeAsync();
		}
	}

	private void UpdateNavigation()
	{
		Transform transform = ((Component)multiplayerSelectionScreen).transform;
		UIUtils.SetExplicitNavigation((RectTransform)(object)((transform is RectTransform) ? transform : null), useCenter: true);
		UIUtils.UpdateNavigationOnListCells(navigableCells, multiplayerSelectionScreen.ScreenSelectionList.GetCurrentSelectable(), (Selectable)(object)multiplayerSelectionScreen.FriendsButton.button);
		IListCellNavigation listCellNavigation = ((navigableCells.Count > 0) ? navigableCells[navigableCells.Count - 1] : null);
		if (listCellNavigation != null)
		{
			UIUtils.SetSelectOnUp((Selectable)(object)multiplayerSelectionScreen.NewGameButton.button, listCellNavigation.GetMainSelectable());
			UIUtils.SetSelectOnUp((Selectable)(object)multiplayerSelectionScreen.FriendsButton.button, listCellNavigation.GetMainSelectable());
			UIUtils.SetSelectOnUp((Selectable)(object)multiplayerSelectionScreen.ProfileButton.button, listCellNavigation.GetMainSelectable());
		}
	}

	public async void Reload(bool forceReload = false)
	{
		await ReloadAsync(forceReload);
	}

	public void OnRefreshTrigger()
	{
		if (PolytopiaBackendAdapter.Instance.IsConnected)
		{
			NetworkUtils.ShowLoader(Localization.Get("onlineview.reloading.release"), 0);
		}
	}

	public void OnRefreshCancelled()
	{
		if (!multiplayerSelectionScreen.IsConnectingBannerShowing)
		{
			NetworkUtils.HideLoader();
		}
	}

	private async Task RefreshGamesAsync()
	{
		if (PolytopiaBackendAdapter.Instance.IsConnected)
		{
			await ReloadAsync(forceReload: true);
			NetworkUtils.ShowLoader(Localization.Get("onlineview.uptodate.replays"), 0, 2f);
		}
		refresherGames.EndRefreshing();
	}

	public async void OnRefreshGames()
	{
		await RefreshGamesAsync();
	}

	protected async Task LoadGamesAsync(bool forceReload = false)
	{
		if (rows.Count == 0 && otherRows.Count == 0)
		{
			MultiplayerInfoRow multiplayerInfoRow = AddInfoRow();
			((TMP_Text)multiplayerInfoRow.header).text = Localization.Get("onlineview.nogames.intro", AccountManager.Alias);
			((TMP_Text)multiplayerInfoRow.description).text = Localization.Get("onlineview.replays.loading");
		}
		if (forceReload)
		{
			NetworkUtils.ShowLoader(Localization.Get("onlineview.reloading"), 0);
			await GameManager.GetReplaysManager().UpdateReplays();
		}
		else
		{
			NetworkUtils.ShowLoader(Localization.Get("onlineview.loading.title"));
			if (!GameManager.GetReplaysManager().HasCachedReplays)
			{
				await GameManager.GetReplaysManager().UpdateReplays();
			}
		}
		NetworkUtils.HideLoader();
		BuildList();
	}

	private async void BuildList()
	{
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		latest = await GameManager.GetReplaysManager().GetRecentReplays();
		favorites = await GameManager.GetReplaysManager().GetFavoriteReplays();
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		ClearList();
		if (latest != null && latest.Count > 0)
		{
			latestHeader = Object.Instantiate<HeaderRow>(headerRowPrefab, (Transform)(object)containerGames);
			latestHeader.label.Text = Localization.Get("onlineview.lastgames", latest.Count);
			foreach (GameSummaryViewModel item in latest)
			{
				if (item == null)
				{
					continue;
				}
				bool favorited = false;
				foreach (GameSummaryViewModel favorite in favorites)
				{
					if (!(favorite.GameId != item.GameId))
					{
						favorited = true;
						break;
					}
				}
				AddGameRow(item, favorited);
			}
		}
		else
		{
			noReplaysRow = Object.Instantiate<MultiplayerInfoRow>(infoRowPrefab, (Transform)(object)containerGames);
			((TMP_Text)noReplaysRow.description).text = Localization.Get("onlineview.noreplays");
			((Component)noReplaysRow.header).gameObject.SetActive(false);
			otherRows.Add(((Component)noReplaysRow).gameObject);
		}
		if (favorites != null && favorites.Count > 0)
		{
			favoritesHeader = Object.Instantiate<HeaderRow>(headerRowPrefab, (Transform)(object)containerGames);
			favoritesHeader.label.Text = Localization.Get("onlineview.favorites", favorites.Count, 10);
			foreach (GameSummaryViewModel favorite2 in favorites)
			{
				if (favorite2 != null)
				{
					AddGameRow(favorite2, favorited: true);
				}
			}
		}
		CurrentSelectable = multiplayerSelectionScreen.ScreenSelectionList.GetCurrentSelectable();
		UpdateNavigation();
	}

	protected void AddGameRow(GameSummaryViewModel summary, bool favorited)
	{
		ReplayGameInfoRow replayGameInfoRow = Object.Instantiate<ReplayGameInfoRow>(replayGameInfoRowPrefab, (Transform)(object)containerGames);
		replayGameInfoRow.OnFavoritedAction = delegate
		{
			Reload();
		};
		replayGameInfoRow.SetData(summary, favorited);
		if (rows.Count == 0)
		{
			CurrentSelectable = (Selectable)(object)replayGameInfoRow.button;
		}
		rows.Add(replayGameInfoRow);
		navigableCells.Add(replayGameInfoRow);
	}

	protected void ClearList()
	{
		foreach (ReplayGameInfoRow row in rows)
		{
			Object.Destroy((Object)(object)((Component)row).gameObject);
		}
		foreach (GameObject otherRow in otherRows)
		{
			Object.Destroy((Object)(object)otherRow);
		}
		if (Object.op_Implicit((Object)(object)latestHeader))
		{
			Object.Destroy((Object)(object)((Component)latestHeader).gameObject);
		}
		if (Object.op_Implicit((Object)(object)favoritesHeader))
		{
			Object.Destroy((Object)(object)((Component)favoritesHeader).gameObject);
		}
		rows.Clear();
		navigableCells.Clear();
	}
}
