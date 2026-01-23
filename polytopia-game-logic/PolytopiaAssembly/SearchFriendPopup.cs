using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class SearchFriendPopup : BasicPopup
{
	[Header("Search Friend Popup")]
	[SerializeField]
	protected TMP_InputField inputfield;

	[SerializeField]
	protected RectTransform scrollerContainer;

	[SerializeField]
	protected RectTransform resultScrollRect;

	[SerializeField]
	protected VerticalLayoutGroup resultListLayoutGroup;

	[SerializeField]
	protected FriendsCategoryContainer friendsCategory;

	[SerializeField]
	protected RectTransform noResultContainer;

	protected bool isSearching;

	protected LayoutElement scrollerLayoutElement;

	private PlayerData[] searchResultData;

	private LayoutElement ScrollerLayoutElement
	{
		get
		{
			if ((Object)(object)scrollerLayoutElement == (Object)null)
			{
				scrollerLayoutElement = ((Component)scrollerContainer).GetComponent<LayoutElement>();
			}
			return scrollerLayoutElement;
		}
	}

	public override void Init()
	{
		base.Init();
		((UnityEvent<string>)(object)inputfield.onSubmit).AddListener((UnityAction<string>)OnInputDone);
	}

	public override void Show()
	{
		buttonData = new PopupButtonData[2]
		{
			new PopupButtonData("buttons.back"),
			new PopupButtonData("friendlist.new.Search", PopupButtonData.States.Disabled, OnSearch, -1, closesPopup: false)
		};
		Header = Localization.Get("friendlist.new.title");
		Description = Localization.Get("friendlist.new.info.alias");
		((Component)noResultContainer).gameObject.SetActive(false);
		((Component)scrollerContainer).gameObject.SetActive(false);
		base.Show();
		UINavigationManager.Select((Selectable)(object)inputfield);
		CurrentSelectable = (Selectable)(object)inputfield;
		friendsCategory.selectCallback = OnShowFriendPopup;
		friendsCategory.disableFlags = FriendsCategoryContainer.DisableFlags.AlreadyFriend | FriendsCategoryContainer.DisableFlags.FriendRequestSent | FriendsCategoryContainer.DisableFlags.Unmigrated;
	}

	protected override void OnShowComplete()
	{
		base.OnShowComplete();
		DefaultSelectable = null;
	}

	public void OnShowFriendPopup(PlayerData friend)
	{
		FriendInfoPopup friendInfoPopup = PopupManager.GetFriendInfoPopup(friend.profile.id, friend.GetName(), friend.state, friend.profile.avatarState, friend);
		friendInfoPopup.OnFriendActionComplete = OnFriendActionComplete;
		friendInfoPopup.Show();
	}

	private void OnFriendActionComplete(FriendActions action, bool success)
	{
		if (success && action == FriendActions.Add)
		{
			Hide();
		}
	}

	public override void OnAvailableAreaChanged()
	{
		RefreshScrollerHeight();
	}

	public override void ResetPopup()
	{
		base.ResetPopup();
		ClearList();
		inputfield.text = string.Empty;
	}

	public void OnInputChanged(string value)
	{
		Buttons[1].ButtonEnabled = !string.IsNullOrEmpty(inputfield.text) && inputfield.text.Length >= 2;
	}

	public async void OnInputDone(string value)
	{
		if (!isSearching)
		{
			await LoadSearchResult();
		}
	}

	private async void OnSearch(int id, BaseEventData eventData)
	{
		if (!isSearching)
		{
			await LoadSearchResult();
		}
	}

	private async Task LoadSearchResult()
	{
		ClearList();
		if (!string.IsNullOrEmpty(inputfield.text) && inputfield.text.Length >= 2)
		{
			isSearching = true;
			Log.Verbose("Should search friend : {0}", new object[1] { inputfield.text });
			SearchUsersBindingModel model = new SearchUsersBindingModel
			{
				SearchString = inputfield.text,
				AllowCrossPlay = true
			};
			NetworkUtils.ShowLoader();
			Buttons[1].ButtonEnabled = false;
			ServerResponseList<PolytopiaFriendViewModel> serverResponseList = await PolytopiaBackendAdapter.Instance.SearchUsers(model);
			NetworkUtils.HideLoader();
			isSearching = false;
			if ((Object)(object)this == (Object)null || !((Component)this).gameObject.activeSelf)
			{
				return;
			}
			List<PolytopiaFriendViewModel> list = null;
			if (serverResponseList.Success)
			{
				list = serverResponseList.Data;
			}
			else
			{
				PopupManager.ShowErrorPopup(Localization.GetErrorMessage(serverResponseList));
			}
			Buttons[1].ButtonEnabled = true;
			if (list != null && list.Count > 0)
			{
				Log.Verbose("result : {0}", new object[1] { list.Count });
				((Component)noResultContainer).gameObject.SetActive(false);
				((Component)scrollerContainer).gameObject.SetActive(true);
				searchResultData = PlayerDataUtils.PlayerDataFromFriendViewModels(list);
				friendsCategory.SetData(searchResultData, null);
				RefreshScrollerHeight();
				UINavigationManager.Select(friendsCategory.GetButton(0));
			}
			else
			{
				UINavigationManager.Select((Selectable)(object)Buttons[1].button);
				((Component)scrollerContainer).gameObject.SetActive(false);
				((Component)noResultContainer).gameObject.SetActive(true);
			}
		}
		if (((Component)this).gameObject.activeInHierarchy)
		{
			((MonoBehaviour)this).StartCoroutine(DelayFixNavigation());
		}
	}

	public void Refresh(PlayerData[] friendDatas)
	{
		bool flag = false;
		if (searchResultData == null || searchResultData.Length == 0)
		{
			return;
		}
		List<PlayerData> list = new List<PlayerData>(searchResultData.Length);
		for (int i = 0; i < searchResultData.Length; i++)
		{
			PlayerData playerData = searchResultData[i];
			bool flag2 = false;
			foreach (PlayerData playerData2 in friendDatas)
			{
				if (playerData.profile.id == playerData2.profile.id)
				{
					flag = true;
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				list.Add(playerData);
			}
		}
		if (flag)
		{
			((Component)scrollerContainer).gameObject.SetActive(true);
			searchResultData = list.ToArray();
			friendsCategory.SetData(searchResultData, null);
			RefreshScrollerHeight();
		}
	}

	private void RefreshScrollerHeight()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)resultListLayoutGroup).transform;
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)(object)((transform is RectTransform) ? transform : null));
		float num = 80f;
		float num2 = 20f;
		Transform transform2 = ((Component)resultListLayoutGroup).transform;
		float num3 = ((RectTransform)((transform2 is RectTransform) ? transform2 : null)).sizeDelta.y;
		float num4 = 0f - scrollerContainer.anchoredPosition.y + num3;
		float num5 = Mathf.Min((ScreenManager.SafeHeight - num) * UICanvasScalerHelper.GetInvertedUIScale(), PopupManager.GetHeight() - num2);
		bool flag = num4 > num5;
		if (flag)
		{
			num3 = num5 - (0f - scrollerContainer.anchoredPosition.y);
		}
		resultScrollRect.offsetMax = new Vector2((float)(flag ? (-20) : 0), 0f);
		ScrollerLayoutElement.minHeight = num3;
		Log.Verbose($"SearchFriendPopup scroller yPos: {scrollerContainer.anchoredPosition.y}, prefered Height: {num4}, available Height: {num5}, needScroll: {flag}, newHeight: {num3}", Array.Empty<object>());
	}

	private IEnumerator DelayFixNavigation()
	{
		yield return (object)new WaitForEndOfFrame();
		UIUtils.SetExplicitNavigation(base.rectTransform);
	}

	private void ClearList()
	{
		friendsCategory.ClearButtons();
		((Component)noResultContainer).gameObject.SetActive(false);
		((Component)scrollerContainer).gameObject.SetActive(false);
		ScrollerLayoutElement.minHeight = 0f;
	}
}
