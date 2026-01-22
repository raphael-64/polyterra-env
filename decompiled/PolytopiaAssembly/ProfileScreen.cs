using System;
using System.Collections.Generic;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PullToRefresh;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProfileScreen : UIScreenBase
{
	[Header("Profile Page")]
	[SerializeField]
	protected VerticalLayoutGroup playerInfoList;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected UIRefreshControl refresher;

	[SerializeField]
	protected PlayerButton playerButton;

	[SerializeField]
	protected UIButtonBase editButton;

	[SerializeField]
	protected UIButtonBase faqButton;

	[Header("Prefabs")]
	[SerializeField]
	protected StatsRow statsRowPrefab;

	protected List<StatsRow> infoRows = new List<StatsRow>();

	private bool isInitialized;

	private void Start()
	{
		AddInfoRow("mplayerstats.alias", AccountManager.Alias);
		AddInfoRow("mplayerstats.friends", AccountManager.GetFriendCount().ToString());
		AddInfoRow("mplayerstats.games", AccountManager.UserModel?.NumMultiplayergames?.ToString() ?? "0");
		AddInfoRow("mplayerstats.rating", AccountManager.UserModel?.MultiplayerRating?.ToString() ?? "0");
		AddInfoRow("mplayerstats.gameversion", VersionManager.GameVersion.ToString());
		playerButton.SetAvatarState(AccountManager.AvatarState);
		isInitialized = true;
	}

	private void OnEnable()
	{
		BackendEvents.OnRefreshUser += OnRefreshUser;
		if (isInitialized)
		{
			UpdateValues();
		}
	}

	private void OnDisable()
	{
		BackendEvents.OnRefreshUser -= OnRefreshUser;
	}

	private void OnRefreshUser(PolytopiaUserViewModel user)
	{
		if (user.PolytopiaId == AccountManager.PlayerAccountId)
		{
			UpdateValues();
		}
	}

	private void AddInfoRow(string headerKey, string value)
	{
		StatsRow statsRow = Object.Instantiate<StatsRow>(statsRowPrefab, ((Component)playerInfoList).transform);
		statsRow.StatsNameKey = headerKey;
		statsRow.StatsValue = value;
		statsRow.Description = string.Empty;
		((Behaviour)statsRow.button).enabled = false;
		infoRows.Add(statsRow);
	}

	private void UpdateValues()
	{
		infoRows[0].StatsValue = AccountManager.Alias;
		infoRows[1].StatsValue = AccountManager.GetFriendCount().ToString();
		infoRows[2].StatsValue = AccountManager.UserModel?.NumMultiplayergames?.ToString() ?? "0";
		infoRows[3].StatsValue = AccountManager.UserModel?.MultiplayerRating?.ToString() ?? "0";
		playerButton.SetAvatarState(AccountManager.AvatarState);
	}

	public async void OnRefresh()
	{
		await PolytopiaBackendAdapter.Instance.EnsureAuthenticatedAsync();
		if (PolytopiaBackendAdapter.Instance.IsAuthenticated)
		{
			NetworkUtils.ShowLoader(Localization.Get("mplayerstats.reloading"));
			Log.Verbose("Refresh player info", Array.Empty<object>());
			ServerResponse<PolytopiaToken> serverResponse = await PolytopiaBackendAdapter.Instance.WhoAmI();
			if (!serverResponse.Success)
			{
				NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(serverResponse));
				refresher.EndRefreshing();
			}
			else
			{
				UpdateValues();
				NetworkUtils.ShowLoader(Localization.Get("mplayerstats.uptodate"), 0, 2f);
				refresher.EndRefreshing();
			}
		}
		else
		{
			NetworkUtils.ShowLoaderError(Localization.Get("mplayerstats.loaderror"));
			refresher.EndRefreshing();
		}
	}

	public void OnRefreshTrigger()
	{
		NetworkUtils.ShowLoader(Localization.Get("onlineview.reloading.release"), 0);
	}

	public void OnRefreshCancelled()
	{
		NetworkUtils.HideLoader();
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}

	public void EditAvatar()
	{
		UpdateAvatarPopup avatarPopup = PopupManager.GetUpdateAvatarPopup();
		avatarPopup.Header = Localization.Get("updateavatar.title");
		avatarPopup.buttonData = new PopupBase.PopupButtonData[2]
		{
			new PopupBase.PopupButtonData("buttons.back"),
			new PopupBase.PopupButtonData("updateavatar.save", PopupBase.PopupButtonData.States.Selected, async delegate
			{
				await avatarPopup.UpdateAvatar();
				playerButton.SetAvatarState(AccountManager.AvatarState);
			})
		};
		avatarPopup.SetData(AccountManager.AvatarState);
		avatarPopup.Show();
	}

	public void ShowFaq()
	{
		NativeHelpers.OpenURL("https://polytopia.io/support/");
	}

	protected override void SubscribeButtonsEvents()
	{
		base.SubscribeButtonsEvents();
		playerButton.OnClicked += PlayerButtonOnClicked;
		editButton.OnClicked += EditButtonOnClicked;
		faqButton.OnClicked += FaqButtonOnClicked;
	}

	protected override void UnsubscribeButtonsEvents()
	{
		base.UnsubscribeButtonsEvents();
		playerButton.OnClicked -= PlayerButtonOnClicked;
		editButton.OnClicked -= EditButtonOnClicked;
		faqButton.OnClicked -= FaqButtonOnClicked;
	}

	private void PlayerButtonOnClicked(int id, BaseEventData eventdata)
	{
		EditAvatar();
	}

	private void EditButtonOnClicked(int id, BaseEventData eventdata)
	{
		EditAvatar();
	}

	private void FaqButtonOnClicked(int id, BaseEventData eventdata)
	{
		ShowFaq();
	}
}
