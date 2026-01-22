using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Challengermode;
using PolytopiaBackendBase.Challengermode.Data;
using PullToRefresh;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TournamentListScreen : UIScreenBase
{
	[Header("Tournament List Screen")]
	[SerializeField]
	protected UIRefreshControl refresher;

	[SerializeField]
	protected RectTransform container;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected LayoutGroup containerLayoutGroup;

	[Header("Prefabs")]
	[SerializeField]
	protected TournamentInfoRow tournamentInfoRowPrefab;

	[SerializeField]
	protected HeaderRow headerRowPrefab;

	[SerializeField]
	protected MultiplayerInfoRow infoRowPrefab;

	[SerializeField]
	protected ButtonRow buttonRowPrefab;

	[SerializeField]
	protected CMAccountInfoRow accountInfoRowPrefab;

	[SerializeField]
	protected CMConnectInfoRow accountConnectInfoRowPrefab;

	[SerializeField]
	protected LabelButtonRow labelButtonRowPrefab;

	protected List<UIBasicButton> rows = new List<UIBasicButton>();

	protected List<GameObject> otherRows = new List<GameObject>();

	protected List<IListCellNavigation> navigableCells = new List<IListCellNavigation>();

	public override void Show(bool instant = false)
	{
		base.Show(instant);
		AddLoadingMessage();
		if (PolytopiaBackendAdapter.Instance.IsConnected && GameManager.IsNetworkEnabled())
		{
			Reload(forceReload: true);
		}
		OnScreenUpdated();
		if (rows.Count > 0)
		{
			PolytopiaInput.Omnicursor.AffixToUIElement(((Component)rows[0].button).GetComponent<RectTransform>());
		}
	}

	protected void OnEnable()
	{
		StartSceneBg.Bright = false;
		BackendEvents.OnBackendConnectionChanged += OnBackendConnectionChangedAsync;
		BackendEvents.OnAccountLinked += OnAccountLinked;
		TournamentManager.OnTournamentUpdated += OnTournamentUpdated;
	}

	protected void OnDisable()
	{
		BackendEvents.OnBackendConnectionChanged -= OnBackendConnectionChangedAsync;
		BackendEvents.OnAccountLinked -= OnAccountLinked;
		TournamentManager.OnTournamentUpdated -= OnTournamentUpdated;
	}

	private void OnBackendConnectionChangedAsync(ConnectionStatus status)
	{
		switch (status)
		{
		case ConnectionStatus.Connected:
		case ConnectionStatus.Reconnected:
			Reload(forceReload: true);
			break;
		case ConnectionStatus.None:
		case ConnectionStatus.Connecting:
		case ConnectionStatus.Reconnecting:
		case ConnectionStatus.ConnectionFailed:
		case ConnectionStatus.Disconnected:
		case ConnectionStatus.SocialConnected:
			break;
		}
	}

	private void OnTournamentUpdated(TournamentViewModel tournamentViewModel)
	{
		if (!((Object)(object)this == (Object)null))
		{
			BuildListAsync();
		}
	}

	private void OnAccountLinked(bool success)
	{
		if (success)
		{
			Reload();
		}
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}

	public async void Reload(bool forceReload = false)
	{
		await ReloadAsync(forceReload);
	}

	public async Task ReloadAsync(bool forceReload = false)
	{
		if (GameManager.IsNetworkEnabled())
		{
			await LoadTournamentsAsync(forceReload);
		}
	}

	protected async Task LoadTournamentsAsync(bool forceReload = false)
	{
		if (!PolytopiaBackendAdapter.Instance.IsConnected)
		{
			return;
		}
		if (forceReload)
		{
			NetworkUtils.ShowLoader(Localization.Get("onlineview.reloading"), 0);
			await GameManager.GetTournamentManager().UpdateTournaments();
			NetworkUtils.HideLoader();
		}
		else
		{
			NetworkUtils.ShowLoader(Localization.Get("onlineview.loading.title"));
			if (!GameManager.GetTournamentManager().HasLoadedCache)
			{
				await GameManager.GetTournamentManager().UpdateTournaments();
			}
			NetworkUtils.HideLoader();
		}
		BuildListAsync();
	}

	private async void BuildListAsync()
	{
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		ChallengermodeConnectionStatus connectionStatus = await GameManager.GetTournamentManager().GetConnectionStatus();
		List<TournamentViewModel> list = await GameManager.GetTournamentManager().GetTournaments();
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		List<TournamentViewModel> list2 = new List<TournamentViewModel>();
		List<TournamentViewModel> list3 = new List<TournamentViewModel>();
		List<TournamentViewModel> list4 = new List<TournamentViewModel>();
		List<TournamentViewModel> list5 = new List<TournamentViewModel>();
		list.Sort(SortTournamentSummaryByStartDate);
		foreach (TournamentViewModel item in list)
		{
			bool flag = item.PersonalViewModel != null && item.PersonalViewModel.HasConfirmed;
			bool flag2 = item.PersonalViewModel != null && item.PersonalViewModel.HasSignedUp;
			bool flag3 = item.PersonalViewModel != null && item.PersonalViewModel.PariticipationComplete;
			bool flag4 = item.ScheduledStartTime.HasValue && item.ScheduledStartTime.Value < DateTime.UtcNow;
			if (item.State == TournamentState.Running)
			{
				if (flag)
				{
					list2.Add(item);
				}
			}
			else if (item.State == TournamentState.Concluded || item.State == TournamentState.Completed)
			{
				if (flag3)
				{
					list5.Add(item);
				}
			}
			else if (flag2)
			{
				list3.Add(item);
			}
			else if (!flag4)
			{
				list4.Add(item);
			}
		}
		ClearList();
		if (connectionStatus == null)
		{
			AddCMConnectInfoRow(Localization.Get("challengermode.connect.connectionfailed"));
		}
		else if (connectionStatus.IsConnected)
		{
			UserViewModel userViewModel = PolytopiaBackendAdapter.Instance?.ClientUserData?.User?.CmUserData;
			if (userViewModel != null)
			{
				AddCMAccountInfoRow(userViewModel.UserName, userViewModel.OverviewUrl, userViewModel.PictureUrl);
			}
		}
		else if (connectionStatus.IsAnotherAccountConnected)
		{
			AddCMConnectInfoRow(Localization.Get("challengermode.connect.alreadyconnected"));
			AddButtonRow(Localization.Get("challengermode.buttons.reconnectaccount"), OnChallengerModeReconnectButton);
		}
		else if (connectionStatus.IsRefreshtokenExpired)
		{
			AddCMConnectInfoRow(Localization.Get("challengermode.connect.tokenexpired"));
			AddButtonRow(Localization.Get("challengermode.buttons.connectaccount"), OnChallengerModeConnectButton);
		}
		else
		{
			AddCMConnectInfoRow(Localization.Get("onlineview.tournaments.info.description"));
			AddButtonRow(Localization.Get("challengermode.buttons.connectaccount"), OnChallengerModeConnectButton);
		}
		if (list.Count == 0)
		{
			AddInfoRow(null, Localization.Get("onlineview.tournaments.info.notournaments"));
		}
		else
		{
			if (list2.Count > 0)
			{
				AddHeader("onlineview.tournaments.ongoing", "-{0}-");
				AddTournamentRows(list2);
			}
			if (list3.Count > 0)
			{
				AddHeader("onlineview.tournaments.joined", "-{0}-");
				AddTournamentRows(list3);
			}
			if (list4.Count > 0)
			{
				AddHeader("onlineview.tournaments.available", "-{0}-");
				AddTournamentRows(list4);
			}
			if (list5.Count > 0)
			{
				AddHeader("onlineview.tournaments.ended", "-{0}-");
				AddTournamentRows(list5);
			}
		}
		AddLabelButtonRow(Localization.Get("onlineview.tournaments.info.description.faq"), Localization.Get("onlineview.tournaments.info.description.faq.button"), delegate
		{
			NativeHelpers.OpenURL("https://support.challengermode.com/en/game-specific/polytopia-tournament-faq");
		});
		UpdateNavigation();
		Log.Verbose("TournamentListScreen refreshed", Array.Empty<object>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(container);
	}

	private async Task RefreshTournamentsAsync()
	{
		if (PolytopiaBackendAdapter.Instance.IsConnected)
		{
			await ReloadAsync(forceReload: true);
			NetworkUtils.ShowLoader(Localization.Get("onlineview.uptodate.tournaments"), 0, 2f);
		}
		refresher.EndRefreshing();
	}

	private void OnChallengerModeConnectButton(ButtonRow buttonRow)
	{
		buttonRow.buttonComp.ButtonEnabled = false;
		PopupManager.ShowAccountConnectionPopup(delegate
		{
			buttonRow.buttonComp.ButtonEnabled = true;
		});
	}

	private async void OnChallengerModeReconnectButton(ButtonRow buttonRow)
	{
		buttonRow.buttonComp.ButtonEnabled = false;
		await GameManager.GetTournamentManager().ConnectChallengermodeAccount(reconnect: true);
		await RefreshTournamentsAsync();
		if ((Object)(object)buttonRow != (Object)null)
		{
			buttonRow.buttonComp.ButtonEnabled = true;
		}
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
		NetworkUtils.HideLoader();
	}

	public async void OnRefreshTournaments()
	{
		await RefreshTournamentsAsync();
	}

	private void ClearList()
	{
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		foreach (UIBasicButton row in rows)
		{
			Object.Destroy((Object)(object)((Component)row).gameObject);
		}
		foreach (GameObject otherRow in otherRows)
		{
			Object.Destroy((Object)(object)otherRow);
		}
		rows.Clear();
		otherRows.Clear();
		navigableCells.Clear();
	}

	private void UpdateNavigation()
	{
		UIUtils.UpdateNavigationOnListCells(navigableCells, (Selectable)(object)backButton.button, null);
		if (navigableCells.Count > 0)
		{
			_ = navigableCells[navigableCells.Count - 1];
		}
		else
			_ = null;
	}

	private void AddTournamentRows(List<TournamentViewModel> tournamentList)
	{
		for (int i = 0; i < tournamentList.Count; i++)
		{
			AddTournamentRow(tournamentList[i]);
		}
	}

	private void AddTournamentRow(TournamentViewModel tournament)
	{
		TournamentInfoRow tournamentInfoRow = Object.Instantiate<TournamentInfoRow>(tournamentInfoRowPrefab, (Transform)(object)container);
		tournamentInfoRow.SetData(tournament);
		if (rows.Count == 0)
		{
			CurrentSelectable = (Selectable)(object)tournamentInfoRow.button;
		}
		rows.Add(tournamentInfoRow);
		navigableCells.Add(tournamentInfoRow);
	}

	protected void AddHeader(string headerKey, string format = null)
	{
		HeaderRow headerRow = Object.Instantiate<HeaderRow>(headerRowPrefab, (Transform)(object)container);
		headerRow.label.format = format;
		headerRow.label.Key = headerKey;
		otherRows.Add(((Component)headerRow).gameObject);
	}

	protected void AddInfoRow(string headerText, string descriptionText, UnityAction<string, string> linkCallback = null)
	{
		MultiplayerInfoRow multiplayerInfoRow = Object.Instantiate<MultiplayerInfoRow>(infoRowPrefab, (Transform)(object)container);
		if (string.IsNullOrEmpty(headerText))
		{
			((Component)multiplayerInfoRow.header).gameObject.SetActive(false);
		}
		else
		{
			((TMP_Text)multiplayerInfoRow.header).text = headerText;
		}
		if (string.IsNullOrEmpty(descriptionText))
		{
			((Component)multiplayerInfoRow.description).gameObject.SetActive(false);
		}
		else
		{
			((TMP_Text)multiplayerInfoRow.description).text = descriptionText;
		}
		if (linkCallback != null)
		{
			multiplayerInfoRow.DescriptionLinkCallback = linkCallback;
		}
		otherRows.Add(((Component)multiplayerInfoRow).gameObject);
	}

	protected void AddLabelButtonRow(string headerText, string buttonText, UIButtonBase.ButtonAction action)
	{
		LabelButtonRow labelButtonRow = Object.Instantiate<LabelButtonRow>(labelButtonRowPrefab, (Transform)(object)container);
		((TMP_Text)labelButtonRow.header).text = headerText;
		labelButtonRow.button.text = buttonText;
		labelButtonRow.button.OnClicked += action;
		otherRows.Add(((Component)labelButtonRow).gameObject);
		navigableCells.Add(labelButtonRow);
	}

	protected ButtonRow AddButtonRow(string buttonText, Action<ButtonRow> action)
	{
		ButtonRow buttonRow = Object.Instantiate<ButtonRow>(buttonRowPrefab, (Transform)(object)container);
		buttonRow.buttonComp.text = buttonText;
		buttonRow.buttonComp.OnClicked += delegate
		{
			action(buttonRow);
		};
		otherRows.Add(((Component)buttonRow).gameObject);
		navigableCells.Add(buttonRow);
		return buttonRow;
	}

	protected void AddCMConnectInfoRow(string description)
	{
		CMConnectInfoRow cMConnectInfoRow = Object.Instantiate<CMConnectInfoRow>(accountConnectInfoRowPrefab, (Transform)(object)container);
		((TMP_Text)cMConnectInfoRow.description).text = description;
		otherRows.Add(((Component)cMConnectInfoRow).gameObject);
	}

	protected void AddCMAccountInfoRow(string header, string overviewUrl, string imageUrl)
	{
		CMAccountInfoRow cMAccountInfoRow = Object.Instantiate<CMAccountInfoRow>(accountInfoRowPrefab, (Transform)(object)container);
		((TMP_Text)cMAccountInfoRow.header).text = header;
		if (!string.IsNullOrEmpty(overviewUrl))
		{
			cMAccountInfoRow.OnClicked += delegate
			{
				NativeHelpers.OpenURL(overviewUrl);
			};
		}
		cMAccountInfoRow.LoadProfileImage(imageUrl);
		otherRows.Add(((Component)cMAccountInfoRow).gameObject);
		navigableCells.Add(cMAccountInfoRow);
	}

	protected void AddLoadingMessage()
	{
		ClearList();
		AddInfoRow(Localization.Get("onlineview.tournaments.info.header"), Localization.Get("onlineview.tournaments.loading"));
	}

	private static int SortTournamentSummaryByStartDate(TournamentViewModel a, TournamentViewModel b)
	{
		if (a == null || b == null)
		{
			return 0;
		}
		DateTime utcNow = DateTime.UtcNow;
		DateTime dateTime = a.ScheduledStartTime ?? a.DateCreated;
		DateTime obj = b.ScheduledStartTime ?? b.DateCreated;
		TimeSpan timeSpan = dateTime - utcNow;
		TimeSpan value = obj - utcNow;
		return timeSpan.CompareTo(value);
	}
}
