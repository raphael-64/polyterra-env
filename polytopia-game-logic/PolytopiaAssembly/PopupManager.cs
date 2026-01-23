using System;
using System.Collections.Generic;
using Polytopia.Data;
using UI.Popups;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupManager : UIBasicComponent
{
	public class BasicPopupData
	{
		public string header;

		public string description;

		public PopupBase.PopupButtonData[] buttonData;

		public BasicPopupData()
		{
		}

		public BasicPopupData(string header, string description, PopupBase.PopupButtonData[] buttonData = null)
		{
			this.header = header;
			this.description = description;
			this.buttonData = buttonData;
		}
	}

	public class IconPopupData
	{
		public string header;

		public string description;

		public Sprite sprite;

		public GameObject iconContent;

		public int cost;

		public PopupBase.PopupButtonData[] buttonData;

		public IconPopupData()
		{
		}

		public IconPopupData(string header, string description, Sprite sprite = null, int cost = -1, PopupBase.PopupButtonData[] buttonData = null, GameObject iconContent = null)
		{
			this.header = header;
			this.description = description;
			this.sprite = sprite;
			this.cost = cost;
			this.buttonData = buttonData;
			this.iconContent = iconContent;
		}
	}

	public class UnitPopupData
	{
		public UnitData unitData;

		public Unit unit;

		public PopupBase.PopupButtonData[] buttonData;

		public UnitPopupData()
		{
		}

		public UnitPopupData(UnitData unitData = null, PopupBase.PopupButtonData[] buttonData = null)
		{
			this.unitData = unitData;
			this.buttonData = buttonData;
		}

		public UnitPopupData(Unit unit = null, PopupBase.PopupButtonData[] buttonData = null)
		{
			this.unit = unit;
			this.buttonData = buttonData;
		}
	}

	public const string YOUR_TURN_POPUP_ID = "YOUR_TURN";

	public const string TURN_REMINDER_POPUP_ID = "TURN_REMINDER";

	public const string SKIP_NOTICE_POPUP_ID = "SKIP_NOTICE";

	[SerializeField]
	protected PopupScroller popupScrollerPrefab;

	[Header("Popups")]
	public BasicPopup basicPopupPrefab;

	public TechPopup techPopupPrefab;

	public IconPopup iconPopupPrefab;

	public DiplomacyPopup diplomacyPopupPrefab;

	public UnitPopup unitPopupPrefab;

	public SelectTribePopup selectTribePopup;

	public SelectTribePopup selectTribePopupFullscreen;

	public RewardPopup rewardPopupPrefab;

	public GameInfoPopup gameInfoPopupPrefab;

	public SearchFriendPopup searchFriendPopupPrefab;

	public CustomLanguagePopup customLanguagePopupPrefab;

	public UpdateAvatarPopup updateAvatarPopupPrefab;

	public UpdateHotseatPlayerPopup updateHotseatPlayerPopupPrefab;

	public FriendInfoPopup friendInfoPopupPrefab;

	public IconRewardPopup iconRewardPopup;

	public PlayerInfoPopup playerInfoPopup;

	public SelectViewmodePopup selectViewmodePopupPrefab;

	public DebugInfoPopup debugInfoPopup;

	public SharePopup sharePopupPrefab;

	public ChallengermodeConnectPopup challengermodeConnectPopupPrefab;

	public LobbyPopup lobbyPopupPrefab;

	public InvitePopup invitePopupPrefab;

	public TournamentInfoPopup tournamentInfoPopupPrefab;

	[Header("Debug")]
	public bool forceBigScreenPopups;

	protected Stack<PopupBase> popupStack = new Stack<PopupBase>();

	protected Dictionary<string, Stack<PopupBase>> popupCache = new Dictionary<string, Stack<PopupBase>>();

	protected Stack<PopupScroller> currentScrollers = new Stack<PopupScroller>();

	protected Stack<PopupScroller> cachedScrollers = new Stack<PopupScroller>();

	protected static PopupManager instance;

	protected Selectable lastSelection;

	protected InputManager.InputType lastInputTypes;

	protected bool subscribedToButtonEvent;

	public static Selectable LastSelection
	{
		get
		{
			return instance.lastSelection;
		}
		set
		{
			instance.lastSelection = value;
		}
	}

	public static bool PopupShowing
	{
		get
		{
			if ((Object)(object)instance != (Object)null)
			{
				return instance.popupStack.Count > 0;
			}
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		instance = this;
		RefreshActive();
		UIManager.Instance.ScreenKeyboardManager.OnTouchScreenHeightChanged += TouchScreenHeightChanged;
	}

	public override void DeInit()
	{
		base.DeInit();
		UIManager.Instance.ScreenKeyboardManager.OnTouchScreenHeightChanged -= TouchScreenHeightChanged;
	}

	public void AddPopupToStack(PopupBase popup)
	{
		if (popupStack.Count == 0)
		{
			if (Object.op_Implicit((Object)(object)EventSystem.current.currentSelectedGameObject))
			{
				LastSelection = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
			}
			if (popup.IsUnskippable)
			{
				lastInputTypes = InputManager.InputType.Map;
				InputManager.DisableInput(InputManager.InputType.Map);
			}
			if (popup.forceScrollerBg)
			{
				lastInputTypes |= InputManager.InputType.Camera;
				InputManager.DisableInput(InputManager.InputType.Camera);
			}
		}
		popup.SetParent(GetScroller());
		popupStack.Push(popup);
		RefreshActive();
		UIEvents.PopupStackChanged();
	}

	public static void RemoveAllPopups()
	{
		int count = instance.popupStack.Count;
		for (int i = 0; i < count; i++)
		{
			instance.popupStack.Peek().Hide();
		}
		UIEvents.PopupStackChanged();
	}

	public static void HideCurrentPopup()
	{
		PopupBase currentPopup = GetCurrentPopup();
		if ((Object)(object)currentPopup != (Object)null && !currentPopup.IsUnskippable)
		{
			SkipCurrentPopup();
		}
	}

	public void RemoveLastPopupFromStack(PopupBase popupToRemove)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		PopupBase popupBase = popupToRemove;
		PopupScroller popupScroller = popupBase.scroller;
		bool flag = false;
		if (popupStack.Count > 0 && (Object)(object)popupStack.Peek() == (Object)(object)popupToRemove)
		{
			popupBase = popupStack.Pop();
			flag = true;
		}
		else
		{
			Stack<PopupBase> stack = new Stack<PopupBase>();
			PopupBase[] array = new PopupBase[popupStack.Count];
			popupStack.CopyTo(array, 0);
			List<PopupBase> list = new List<PopupBase>(array);
			list.Reverse();
			foreach (PopupBase item in list)
			{
				if ((Object)(object)item != (Object)(object)popupBase)
				{
					stack.Push(item);
				}
				else
				{
					flag = true;
				}
			}
			popupStack = stack;
		}
		if (!flag)
		{
			return;
		}
		PolytopiaInput.Omnicursor.OverrideToPosition(popupToRemove.appearTransitionOrigin);
		if (!popupCache.ContainsKey(popupBase.popupId))
		{
			popupCache.Add(popupBase.popupId, new Stack<PopupBase>());
		}
		if (currentScrollers.Count > 0 && (Object)(object)currentScrollers.Peek() == (Object)(object)popupBase.scroller)
		{
			popupScroller = currentScrollers.Pop();
		}
		else
		{
			Stack<PopupScroller> stack2 = new Stack<PopupScroller>();
			PopupScroller[] array2 = new PopupScroller[currentScrollers.Count];
			currentScrollers.CopyTo(array2, 0);
			List<PopupScroller> list2 = new List<PopupScroller>(array2);
			list2.Reverse();
			foreach (PopupScroller item2 in list2)
			{
				if ((Object)(object)item2 != (Object)(object)popupBase.scroller)
				{
					stack2.Push(item2);
				}
			}
			currentScrollers = stack2;
		}
		cachedScrollers.Push(popupScroller);
		((Component)popupScroller).gameObject.SetActive(false);
		popupCache[popupBase.popupId].Push(popupBase);
		popupBase.ResetPopup();
		((Transform)popupBase.rectTransform).SetParent(((Component)this).transform);
		RefreshActive();
		if (popupStack.Count == 0)
		{
			Selectable suggestedSelection;
			if ((Object)(object)LastSelection != (Object)null && (Object)(object)((Component)LastSelection).gameObject != (Object)null)
			{
				UINavigationManager.Select(LastSelection);
			}
			else if (UINavigationManager.GetSuggestedSelectable(out suggestedSelection))
			{
				UINavigationManager.Select(suggestedSelection);
			}
			InputManager.EnableInput(lastInputTypes);
			lastInputTypes = (InputManager.InputType)0;
		}
		else
		{
			GetCurrentPopup().ReturnFocus();
		}
		UIEvents.PopupStackChanged();
	}

	protected void RefreshActive()
	{
		bool flag = popupStack.Count > 0;
		((Component)this).gameObject.SetActive(flag);
		if (flag)
		{
			if (!subscribedToButtonEvent)
			{
				InputEvents.OnButtonUp += OnButtonUp;
				subscribedToButtonEvent = true;
			}
		}
		else
		{
			InputEvents.OnButtonUp -= OnButtonUp;
			subscribedToButtonEvent = false;
		}
	}

	public void AdjustAvailableArea(float keyboardHeight)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		base.rectTransform.offsetMin = Vector2.up * keyboardHeight / UIManager.CanvasScaler.scaleFactor;
		base.rectTransform.offsetMax = Vector2.zero;
		foreach (PopupBase item in popupStack)
		{
			item.OnAvailableAreaChanged();
		}
	}

	public static float GetHeight()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = instance.rectTransform.rect;
		return ((Rect)(ref rect)).size.y;
	}

	private void OnButtonUp(InputManager.Buttons button)
	{
		if (button == InputManager.Buttons.Cancel && popupStack.Count > 0 && !popupStack.Peek().IsUnskippable)
		{
			SkipCurrentPopup();
		}
	}

	private static void SkipCurrentPopup()
	{
		bool flag = false;
		PopupBase currentPopup = GetCurrentPopup();
		if (currentPopup is BasicPopup)
		{
			BasicPopup basicPopup = currentPopup as BasicPopup;
			PopupBase.PopupButtonData[] buttonData = basicPopup.buttonData;
			if (buttonData != null && buttonData.Length != 0)
			{
				flag = true;
				basicPopup.OnHide(buttonData[0].id, null);
				buttonData[0].callback?.Invoke(buttonData[0].id);
			}
		}
		if (!flag)
		{
			currentPopup.Hide();
		}
		UIEvents.PopupStackChanged();
	}

	private void TouchScreenHeightChanged(float keyboardHeight)
	{
		AdjustAvailableArea(keyboardHeight);
	}

	protected PopupScroller GetScroller()
	{
		PopupScroller popupScroller = ((cachedScrollers.Count != 0) ? cachedScrollers.Pop() : Object.Instantiate<PopupScroller>(popupScrollerPrefab, ((Component)this).transform));
		if ((Object)(object)popupScroller != (Object)null)
		{
			((Component)popupScroller).gameObject.SetActive(true);
			((Transform)popupScroller.rectTransform).SetAsLastSibling();
			currentScrollers.Push(popupScroller);
		}
		return popupScroller;
	}

	public static bool IsPopupShowing<T>(string identifier = null)
	{
		foreach (PopupBase item in instance.popupStack)
		{
			if (((object)item).GetType() == typeof(T))
			{
				if (string.IsNullOrEmpty(identifier))
				{
					return true;
				}
				if (item.identifier == identifier)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsUnskippablePopupShowing()
	{
		foreach (PopupBase item in instance.popupStack)
		{
			if (item.IsUnskippable)
			{
				return true;
			}
		}
		return false;
	}

	public static PopupBase GetCurrentPopup()
	{
		if (!PopupShowing)
		{
			return null;
		}
		return instance.popupStack.Peek();
	}

	public static T GetCurrentPopup<T>(string identifier = null) where T : PopupBase
	{
		foreach (PopupBase item in instance.popupStack)
		{
			if (((object)item).GetType() == typeof(T))
			{
				if (string.IsNullOrEmpty(identifier))
				{
					return (T)item;
				}
				if (item.identifier == identifier)
				{
					return (T)item;
				}
			}
		}
		return null;
	}

	public static bool TryGetOpenPopup<T>(string identifier, out T openPopup) where T : PopupBase
	{
		openPopup = null;
		foreach (PopupBase item in instance.popupStack)
		{
			if (((object)item).GetType() == typeof(T) && !string.IsNullOrEmpty(identifier) && item.identifier == identifier)
			{
				openPopup = (T)item;
				return true;
			}
		}
		return false;
	}

	public static BasicPopup GetBasicPopup(BasicPopupData data)
	{
		BasicPopup basicPopup = GetBasicPopup();
		basicPopup.Header = data.header;
		basicPopup.Description = data.description;
		basicPopup.buttonData = data.buttonData;
		return basicPopup;
	}

	private static T GetPopup<T>(string popupId, T prefab) where T : PopupBase
	{
		T val;
		if (!instance.popupCache.ContainsKey(popupId) || instance.popupCache[popupId].Count == 0)
		{
			val = Object.Instantiate<T>(prefab, ((Component)instance).transform);
			val.popupId = popupId;
			val.popupManager = instance;
			val.Init();
		}
		else
		{
			val = instance.popupCache[popupId].Pop() as T;
			((Transform)val.rectTransform).SetParent(((Component)instance).transform);
		}
		val.identifier = null;
		((Transform)val.rectTransform).SetAsLastSibling();
		return val;
	}

	public static BasicPopup GetBasicPopup()
	{
		return GetPopup("basicPopup", instance.basicPopupPrefab);
	}

	public static FriendInfoPopup GetFriendInfoPopup()
	{
		return GetPopup("friendInfoPopup", instance.friendInfoPopupPrefab);
	}

	public static FriendInfoPopup GetFriendInfoPopup(Guid id, string name, PlayerData.State friendshipState, AvatarState avatarState = null, PlayerData playerData = null)
	{
		FriendInfoPopup popup = GetPopup("friendInfoPopup", instance.friendInfoPopupPrefab);
		popup.SetData(id, name, friendshipState, avatarState, playerData);
		return popup;
	}

	public static SharePopup GetSharePopup(Guid gameId)
	{
		SharePopup popup = GetPopup("sharePopup", instance.sharePopupPrefab);
		popup.SetData(gameId);
		return popup;
	}

	public static ChallengermodeConnectPopup GetChallengermodeConnectPopup()
	{
		return GetPopup("challengermodeConnectPopup", instance.challengermodeConnectPopupPrefab);
	}

	public static IconPopup GetIconPopup(IconPopupData data)
	{
		IconPopup iconPopup = GetIconPopup();
		iconPopup.Header = data.header;
		iconPopup.Description = data.description;
		if (Object.op_Implicit((Object)(object)data.sprite))
		{
			iconPopup.sprite = data.sprite;
		}
		else if (Object.op_Implicit((Object)(object)data.iconContent))
		{
			data.iconContent.transform.SetParent((Transform)(object)iconPopup.iconContainer, false);
			UIUtils.FitImageContentInParent(data.iconContent.GetComponent<RectTransform>());
			data.iconContent.gameObject.SetActive(true);
		}
		iconPopup.cost = data.cost;
		iconPopup.buttonData = data.buttonData;
		return iconPopup;
	}

	public static BasicPopup GetBlockingPopup(string id, string header, string description)
	{
		BasicPopup basicPopup = GetBasicPopup();
		basicPopup.identifier = id;
		basicPopup.Header = header;
		basicPopup.Description = description;
		basicPopup.IsUnskippable = true;
		basicPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("startmenu.quit", PopupBase.PopupButtonData.States.Selected, delegate
			{
				Application.Quit();
			}, -1, closesPopup: false)
		};
		return basicPopup;
	}

	public static IconPopup GetIconPopup()
	{
		return GetPopup("iconPopup", instance.iconPopupPrefab);
	}

	public static IconRewardPopup GetIconRewardPopup()
	{
		return GetPopup("iconRewardPopup", instance.iconRewardPopup);
	}

	public static DiplomacyPopup GetDiplomacyPopup()
	{
		return GetPopup("diplomacyPopup", instance.diplomacyPopupPrefab);
	}

	public static TechPopup GetTechPopup()
	{
		return GetPopup("techPopup", instance.techPopupPrefab);
	}

	public static SelectViewmodePopup GetSelectViewmodePopup()
	{
		return GetPopup("viewmodePopup", instance.selectViewmodePopupPrefab);
	}

	public static UnitPopup GetUnitPopup(UnitPopupData data)
	{
		UnitPopup unitPopup = GetUnitPopup();
		if (data.unitData != null)
		{
			unitPopup.UnitData = data.unitData;
		}
		if (Object.op_Implicit((Object)(object)data.unit))
		{
			unitPopup.Unit = data.unit;
		}
		unitPopup.buttonData = data.buttonData;
		return unitPopup;
	}

	public static UnitPopup GetUnitPopup()
	{
		return GetPopup("unitPopup", instance.unitPopupPrefab);
	}

	public static SelectTribePopup GetSelectTribePopup()
	{
		if ((Application.isMobilePlatform && !UICanvasScalerHelper.IsBigScreen()) || (instance.forceBigScreenPopups && Debug.isDebugBuild))
		{
			return GetPopup("selectTribePopupFullscreen", instance.selectTribePopupFullscreen);
		}
		return GetPopup("selectTribePopup", instance.selectTribePopup);
	}

	public static PlayerInfoPopup GetPlayerInfoPopup()
	{
		PlayerInfoPopup popup = GetPopup("playerInfoPopup", instance.playerInfoPopup);
		popup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		return popup;
	}

	public static RewardPopup GetRewardPopup()
	{
		return GetPopup("rewardPopup", instance.rewardPopupPrefab);
	}

	public static BasicPopup GetMoreGameInfoPopup()
	{
		return GetPopup("moreGameInfoPopup", instance.basicPopupPrefab);
	}

	public static BasicPopup GetMoreTournamentInfoPopup()
	{
		return GetPopup("moreTournamentInfoPopup", instance.basicPopupPrefab);
	}

	public static GameInfoPopup GetGameInfoPopup()
	{
		return GetPopup("gameInfoPopup", instance.gameInfoPopupPrefab);
	}

	public static LobbyPopup GetLobbyPopup()
	{
		return GetPopup("lobbyPopup", instance.lobbyPopupPrefab);
	}

	public static TournamentInfoPopup GetTournamentInfoPopup()
	{
		return GetPopup("tournamentInfoPopup", instance.tournamentInfoPopupPrefab);
	}

	public static InvitePopup GetInvitePopup()
	{
		return GetPopup("invitePopup", instance.invitePopupPrefab);
	}

	public static SearchFriendPopup GetSearchFriendPopup()
	{
		return GetPopup("searchFriendPopup", instance.searchFriendPopupPrefab);
	}

	public static CustomLanguagePopup GetCustomLanguagePopup()
	{
		return GetPopup("customLanguagePopup", instance.customLanguagePopupPrefab);
	}

	public static DebugInfoPopup GetDebugInfoPopup()
	{
		return GetPopup("debugInfoPopup", instance.debugInfoPopup);
	}

	public static UpdateAvatarPopup GetUpdateAvatarPopup()
	{
		return GetPopup("updateAvatarPopup", instance.updateAvatarPopupPrefab);
	}

	public static UpdateHotseatPlayerPopup GetUpdateHotseatPlayerPopup()
	{
		return GetPopup("updateHotseatPlayerPopup", instance.updateHotseatPlayerPopupPrefab);
	}

	public static void ShowBackendErrorPopup(string message, Exception exception)
	{
		if (!string.IsNullOrEmpty(message))
		{
			ShowBackendErrorPopup(message, exception?.InnerException?.Message ?? exception?.Message ?? "Unknown error");
		}
		else
		{
			ShowBackendErrorPopup(exception?.InnerException?.Message ?? exception?.Message ?? "Unknown error");
		}
	}

	public static void ShowBackendErrorPopup(string message, string error)
	{
		ShowBackendErrorPopup($"{message}\n\n{error}");
	}

	public static void ShowBackendErrorPopup(Exception exception)
	{
		ShowBackendErrorPopup(exception?.InnerException?.Message ?? exception?.Message ?? "Unknown error");
	}

	public static void ShowBackendErrorPopup(string message)
	{
		if (Application.isPlaying)
		{
			Log.Warning("Backend Error: {0}", new object[1] { message });
			BasicPopup basicPopup = GetBasicPopup();
			basicPopup.Header = "Backend Error";
			basicPopup.Description = message;
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup.Show();
			NotificationManager.HideAlert();
		}
	}

	public static void ShowErrorPopup(string message, Action action = null)
	{
		ShowErrorPopup(Localization.Get("misc.error.title"), message, action);
	}

	public static void ShowErrorPopup(string header, string message, Action action = null)
	{
		if (Application.isPlaying)
		{
			Log.Verbose("Show error popup: {0}: {1}", new object[2] { header, message });
			BasicPopup basicPopup = GetBasicPopup();
			basicPopup.Header = header;
			basicPopup.Description = message;
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					action?.Invoke();
				})
			};
			basicPopup.Show();
		}
	}

	public static void ShowAccountConnectionPopup(Action onComplete)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		ChallengermodeConnectPopup challengermodeConnectPopup = GetChallengermodeConnectPopup();
		challengermodeConnectPopup.Header = Localization.Get("challengermode.connect.header");
		challengermodeConnectPopup.Description = Localization.Get("challengermode.connect.description");
		challengermodeConnectPopup.buttonData = new PopupBase.PopupButtonData[2]
		{
			new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.None, delegate
			{
				onComplete();
			}),
			new PopupBase.PopupButtonData("buttons.connect", PopupBase.PopupButtonData.States.Selected, async delegate
			{
				await GameManager.GetTournamentManager().ConnectChallengermodeAccount();
				onComplete();
			})
		};
		challengermodeConnectPopup.Show(InputManager.GetInputPosition());
	}
}
