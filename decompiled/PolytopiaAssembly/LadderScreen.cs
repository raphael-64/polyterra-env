using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Challengermode.Data;
using PullToRefresh;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LadderScreen : UIScreenBase
{
	[Header("List")]
	[SerializeField]
	protected UIRefreshControl refresher;

	[SerializeField]
	protected RectTransform container;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected LayoutGroup containerLayoutGroup;

	[Header("Multiplayer Selection Screen Connection")]
	[SerializeField]
	protected MultiplayerSelectionScreen multiplayerSelectionScreen;

	[Header("Prefabs")]
	[SerializeField]
	protected LadderDetails ladderDetailsPrefab;

	[SerializeField]
	protected HeaderRow headerRowPrefab;

	[SerializeField]
	protected MultiplayerInfoRow infoRowPrefab;

	[SerializeField]
	protected ButtonRow buttonRowPrefab;

	protected List<GameObject> rows = new List<GameObject>();

	protected List<IListCellNavigation> navigableCells = new List<IListCellNavigation>();

	protected LadderViewModel currentLadderData;

	protected bool isUpcoming;

	public override void Show(bool instant = false)
	{
		multiplayerSelectionScreen.Show(instant: true);
		multiplayerSelectionScreen.UpdateScreenSelectionListSelectedIndex(UIConstants.Screens.LadderScreen);
		base.Show(instant);
		if (PolytopiaBackendAdapter.Instance.IsConnected && GameManager.IsNetworkEnabled())
		{
			LoadAsync();
		}
		else
		{
			AddStartMessages();
		}
		OnScreenUpdated();
	}

	public override void Hide(bool instant = false)
	{
		base.Hide(instant);
		multiplayerSelectionScreen.Hide();
	}

	private void OnEnable()
	{
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChangedAsync;
		LadderManager.OnLadderStateUpdated += OnLadderStateUpdated;
	}

	private void OnDisable()
	{
		BackendEvents.OnBackendConnectionChanged -= OnBackendConnectionChangedAsync;
		LadderManager.OnLadderStateUpdated -= OnLadderStateUpdated;
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

	private void OnLadderStateUpdated(LadderViewModel ladder)
	{
		if (currentLadderData != null && currentLadderData.Id == ladder.Id)
		{
			currentLadderData = ladder;
			BuildList();
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
			((TMP_Text)multiplayerInfoRow3.description).text = Localization.Get("onlineview.ladder.loading");
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
		MultiplayerInfoRow multiplayerInfoRow = Object.Instantiate<MultiplayerInfoRow>(infoRowPrefab, (Transform)(object)container);
		rows.Add(((Component)multiplayerInfoRow).gameObject);
		return multiplayerInfoRow;
	}

	protected ButtonRow AddButtonRow()
	{
		ButtonRow buttonRow = Object.Instantiate<ButtonRow>(buttonRowPrefab, (Transform)(object)container);
		rows.Add(((Component)buttonRow).gameObject);
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
		((TMP_Text)multiplayerInfoRow.description).text = Localization.Get("onlineview.ladder.loading");
		GameManager.GetLoginManager().Login(silent: false);
		await new WaitForUpdate();
	}

	public async void LoadAsync()
	{
		if (GameManager.IsNetworkEnabled())
		{
			await LoadLadderStartAsync();
			await multiplayerSelectionScreen.UpdateFriendsBadgeAsync();
		}
	}

	public async Task ReloadAsync(bool forceReload = false)
	{
		if (GameManager.IsNetworkEnabled())
		{
			await LoadLadderRefreshAsync(forceReload);
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

	public async void Reload()
	{
		await ReloadAsync(forceReload: true);
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

	private async Task RefreshLadderAsync()
	{
		if (PolytopiaBackendAdapter.Instance.IsConnected)
		{
			await ReloadAsync(forceReload: true);
			NetworkUtils.ShowLoader(Localization.Get("onlineview.uptodate.ladders"), 0, 2f);
		}
		refresher.EndRefreshing();
	}

	public async void OnRefreshLadder()
	{
		await RefreshLadderAsync();
	}

	protected async Task LoadLadderAsync(bool forceReload = false)
	{
		Log.Info("Loading ladders...", Array.Empty<object>());
		NetworkUtils.HideLoader();
		if (rows.Count == 0)
		{
			MultiplayerInfoRow multiplayerInfoRow = AddInfoRow();
			((TMP_Text)multiplayerInfoRow.header).text = Localization.Get("onlineview.nogames.intro", AccountManager.Alias);
			((TMP_Text)multiplayerInfoRow.description).text = Localization.Get("onlineview.ladder.loading");
		}
		Guid? ladderId = await GameManager.GetLadderManager().GetRecurringLadderId();
		isUpcoming = false;
		if (!ladderId.HasValue)
		{
			ladderId = await GameManager.GetLadderManager().GetUpcomingLadderId();
			isUpcoming = true;
		}
		if (ladderId.HasValue)
		{
			if (forceReload)
			{
				await GameManager.GetLadderManager().UpdateLadderData(ladderId.Value);
			}
			currentLadderData = await GameManager.GetLadderManager().GetLadderData(ladderId.Value);
			Log.Info("currentLadderData, laddername {0}", new object[1] { currentLadderData.Name });
		}
		else
		{
			Log.Info("Could not find recurring ladder id", Array.Empty<object>());
		}
		BuildList();
	}

	protected async Task LoadLadderStartAsync()
	{
		if (PolytopiaBackendAdapter.Instance.IsConnected && GameManager.IsNetworkEnabled())
		{
			NetworkUtils.ShowLoader(Localization.Get("onlineview.loading.title"));
			await LoadLadderAsync(forceReload: true);
		}
	}

	protected async Task LoadLadderRefreshAsync(bool forceReload)
	{
		if (PolytopiaBackendAdapter.Instance.IsConnected && GameManager.IsNetworkEnabled())
		{
			NetworkUtils.ShowLoader(Localization.Get("onlineview.reloading"), 0);
			await LoadLadderAsync(forceReload);
		}
	}

	private void BuildList()
	{
		if (!((Object)(object)this == (Object)null))
		{
			ClearList();
			if (currentLadderData == null)
			{
				MultiplayerInfoRow multiplayerInfoRow = AddInfoRow();
				((Component)multiplayerInfoRow.header).gameObject.SetActive(false);
				((TMP_Text)multiplayerInfoRow.description).text = Localization.Get("onlineview.noladders");
				rows.Add(((Component)multiplayerInfoRow).gameObject);
			}
			else
			{
				AddLadderDetails(currentLadderData, isUpcoming);
			}
			CurrentSelectable = multiplayerSelectionScreen.ScreenSelectionList.GetCurrentSelectable();
			UpdateNavigation();
		}
	}

	protected void AddHeader(string headerKey, string format = null)
	{
		HeaderRow headerRow = Object.Instantiate<HeaderRow>(headerRowPrefab, (Transform)(object)container);
		headerRow.label.format = format;
		headerRow.label.Key = headerKey;
		rows.Add(((Component)headerRow).gameObject);
	}

	protected void AddHeader(string header)
	{
		HeaderRow headerRow = Object.Instantiate<HeaderRow>(headerRowPrefab, (Transform)(object)container);
		headerRow.label.Text = header;
		rows.Add(((Component)headerRow).gameObject);
	}

	private void AddLadderDetails(LadderViewModel data, bool isUpcoming = false)
	{
		LadderDetails ladderDetails = Object.Instantiate<LadderDetails>(ladderDetailsPrefab, (Transform)(object)container);
		ladderDetails.SetData(data, isUpcoming);
		rows.Add(((Component)ladderDetails).gameObject);
	}

	protected void ClearList()
	{
		foreach (GameObject row in rows)
		{
			Object.Destroy((Object)(object)row);
		}
		rows.Clear();
		navigableCells.Clear();
	}
}
