using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using PolytopiaBackendBase.Game;
using UnityEngine;

public class NotificationManager : UIBasicComponent
{
	public static Color NETWORK_ALERT_COLOR = Color32.op_Implicit(new Color32((byte)0, (byte)153, byte.MaxValue, (byte)204));

	public static Color NETWORK_ERROR_COLOR = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)51, (byte)0, (byte)204));

	public float endPosY = -90f;

	public float startOffsetY = -100f;

	public float alertPosY = -70f;

	public float networkAlertYPos = -55f;

	public bool ignorePlayer;

	public RectTransform notificationContainer;

	[Header("Prefabs")]
	public NotificationBase basicNotificationPrefab;

	public DiplomacyNotification diplomacyNotificationPrefab;

	public TopAlert topAlertPrefab;

	public TopAlert networkAlertPrefab;

	public CenterNotification centerNotificationPrefab;

	[Header("Debug")]
	public bool testMode;

	public GameObject testButton;

	protected static NotificationManager instance;

	protected List<NotificationBase> notificationList = new List<NotificationBase>();

	protected Dictionary<string, Stack<NotificationBase>> notificationCache = new Dictionary<string, Stack<NotificationBase>>();

	protected TopAlert topAlert;

	protected TopAlert networkAlert;

	protected CenterNotification centerNotification;

	protected Coroutine hideAlertCoroutine;

	protected Coroutine hideNetworkAlertCoroutine;

	protected float currentAlertPosition;

	protected float currentNetworkAlertPosition;

	protected bool silenceAlerts;

	protected bool ignoreNotifications;

	protected PollAnimation<float> hideAnimation;

	private const int USERNAME_CHARACTER_LIMIT = 20;

	public override void Init()
	{
		base.Init();
		((Component)this).gameObject.SetActive(false);
		CreateTopAlert();
		CreateNetworkAlert();
		CreateCenterNotification();
		instance = this;
		testButton.SetActive(testMode);
		BackendEvents.OnPlayersStatusesUpdated += UpdatePlayerStatus;
		currentAlertPosition = alertPosY;
		currentNetworkAlertPosition = networkAlertYPos;
	}

	private void OnDestroy()
	{
		instance = null;
		BackendEvents.OnPlayersStatusesUpdated -= UpdatePlayerStatus;
	}

	private void Update()
	{
		if (hideAnimation == null || !((Object)(object)notificationContainer != (Object)null))
		{
			return;
		}
		float easedProgress = hideAnimation.GetEasedProgress();
		float y = Mathf.LerpUnclamped(hideAnimation.startValue, hideAnimation.targetValue, easedProgress);
		notificationContainer.SetAnchoredY(y);
		if (hideAnimation.GetProgress() >= 1f)
		{
			if (notificationList.Count == 0)
			{
				notificationContainer.SetAnchoredY(0f);
			}
			hideAnimation = null;
		}
	}

	private void CreateTopAlert()
	{
		topAlert = Object.Instantiate<TopAlert>(topAlertPrefab, (Transform)(object)base.rectTransform);
		topAlert.rectTransform.SetAnchoredY(alertPosY);
		topAlert.Hide();
	}

	private void CreateNetworkAlert()
	{
		networkAlert = Object.Instantiate<TopAlert>(networkAlertPrefab, (Transform)(object)base.rectTransform);
		networkAlert.rectTransform.SetAnchoredY(networkAlertYPos);
		networkAlert.rectTransform.SetHeight(35f);
		networkAlert.Hide();
	}

	private void CreateCenterNotification()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		centerNotification = Object.Instantiate<CenterNotification>(centerNotificationPrefab, (Transform)(object)base.rectTransform);
		centerNotification.rectTransform.anchoredPosition = Vector2.zero;
		centerNotification.hideCallback = UpdateVisibility;
		centerNotification.Init();
	}

	[Button("Test Notification")]
	public void OnTestNotification()
	{
		if (Random.value > 0.5f)
		{
			Notify("Test notification message...");
		}
		else
		{
			Notify("Test notification message with longer text. This will hopefully row break.", "Entering Village!", UIManager.IconData.GetSprite("Resource"));
		}
	}

	[Button("Test 2 Notification")]
	public void OnTestTwoNotifications()
	{
		OnTestNotification();
		OnTestNotification();
	}

	protected void UpdateVisibility()
	{
		if (notificationList.Count == 0 && !topAlert.Showing && !centerNotification.Showing && !networkAlert.Showing)
		{
			((Component)this).gameObject.SetActive(false);
		}
		else
		{
			((Component)this).gameObject.SetActive(true);
		}
	}

	public void AddNotificationToQueue(NotificationBase notification)
	{
		if (ignoreNotifications)
		{
			return;
		}
		float showPosY = GetBaseYPosition();
		if (notificationList.Count > 0)
		{
			foreach (NotificationBase notification2 in notificationList)
			{
				if (notification2.state == NotificationBase.State.Showing || notification2.state == NotificationBase.State.Hiding)
				{
					showPosY = notification2.showPosY - (notification2.GetTotalHeightOfContent() + 10f);
				}
			}
		}
		notificationList.Add(notification);
		notification.showPosY = showPosY;
		notification.Show();
		UpdateVisibility();
	}

	public void NotificationStartHiding(NotificationBase notification)
	{
	}

	public void NotificationHidden(NotificationBase notification)
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		float num = notification.GetTotalHeightOfContent() + 10f;
		if (!notificationCache.ContainsKey(notification.notificationId))
		{
			notificationCache.Add(notification.notificationId, new Stack<NotificationBase>());
		}
		notificationCache[notification.notificationId].Push(notification);
		notification.ResetNotification();
		notificationList.Remove(notification);
		if (notificationList.Count == 0)
		{
			notificationContainer.SetAnchoredY(0f);
		}
		else if (((Component)this).gameObject.activeInHierarchy && ((Component)this).gameObject.activeSelf && (Object)(object)notificationContainer != (Object)null)
		{
			float num2 = ((hideAnimation != null) ? hideAnimation.targetValue : notificationContainer.anchoredPosition.y);
			hideAnimation = new PollAnimation<float>(0.2f, notificationContainer.anchoredPosition.y, num2 + num, Easing.InOutSine);
		}
		else
		{
			notificationContainer.SetAnchoredY(notificationContainer.anchoredPosition.y + num);
		}
		UpdateVisibility();
	}

	public bool IsNotificationQueued(NotificationBase notification)
	{
		return notificationList.Contains(notification);
	}

	private void ShowAlertInternal(string message, float showTime = 0f, bool showTurnTimer = false)
	{
		if (!silenceAlerts)
		{
			topAlert.ShowTimer = showTurnTimer;
			topAlert.Message = message;
			topAlert.Show(instant: true);
			UpdateVisibility();
			UpdatePositions();
			if (showTime > 0f)
			{
				hideAlertCoroutine = ((MonoBehaviour)this).StartCoroutine(DelayHideAlert(showTime));
			}
		}
	}

	private void ShowNetworkAlertInternal(string message, Color color, bool instant = false, float showTime = 0f)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		networkAlert.Message = message;
		networkAlert.SetBackgroundColor(color);
		networkAlert.Show(instant);
		UpdateVisibility();
		UpdatePositions();
		if (showTime > 0f)
		{
			hideNetworkAlertCoroutine = ((MonoBehaviour)this).StartCoroutine(DelayHideNetworkAlert(showTime));
		}
	}

	private void UpdatePositions()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		float y = currentAlertPosition;
		float y2 = currentNetworkAlertPosition;
		if (networkAlert.Showing && topAlert.rectTransform.Overlaps(networkAlert.rectTransform))
		{
			y = networkAlert.rectTransform.anchoredPosition.y - networkAlert.rectTransform.GetHeight();
		}
		topAlert.rectTransform.SetAnchoredY(y);
		networkAlert.rectTransform.SetAnchoredY(y2);
	}

	private void HideAlertInternal()
	{
		topAlert.Hide(instant: true, OnAlertHidden);
		if (hideAlertCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(hideAlertCoroutine);
		}
	}

	private void HideNetworkAlertInternal(bool instant = false)
	{
		networkAlert.Hide(instant, OnAlertHidden);
		if (hideNetworkAlertCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(hideNetworkAlertCoroutine);
		}
	}

	private void OnAlertHidden()
	{
		UpdateVisibility();
		UpdatePositions();
	}

	private IEnumerator DelayHideAlert(float delay)
	{
		yield return (object)new WaitForSeconds(delay);
		HideAlert();
	}

	private IEnumerator DelayHideNetworkAlert(float delay)
	{
		yield return (object)new WaitForSecondsRealtime(delay);
		HideNetworkAlert();
	}

	public static void Notify(string message, string title = null, object icon = null, PlayerState reciever = null)
	{
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		if (reciever == null && !instance.ignorePlayer)
		{
			reciever = GameManager.LocalPlayer;
		}
		if (instance.ignorePlayer || reciever == GameManager.LocalPlayer)
		{
			NotificationBase basicNotification = GetBasicNotification();
			basicNotification.Title = title;
			basicNotification.Message = message;
			if (icon is Sprite)
			{
				basicNotification.IconSprite = (Sprite)((icon is Sprite) ? icon : null);
			}
			else if (icon is RectTransform)
			{
				basicNotification.IconContent = (RectTransform)((icon is RectTransform) ? icon : null);
			}
			basicNotification.Show();
		}
	}

	public static void Notify(string message, string title, PlayerState player, PlayerState otherPlayer, DiplomacyGraphics.Type diplomacyGraphicsType, PlayerState receiver = null)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			if (receiver == null && !instance.ignorePlayer)
			{
				receiver = GameManager.LocalPlayer;
			}
			if (instance.ignorePlayer || receiver == GameManager.LocalPlayer)
			{
				DiplomacyNotification diplomacyNotification = GetDiplomacyNotification();
				diplomacyNotification.Title = title;
				diplomacyNotification.Message = message;
				diplomacyNotification.SetData(player, otherPlayer, diplomacyGraphicsType);
				diplomacyNotification.Show();
			}
		}
	}

	public static void Alert(string message, float showTime = 0f, bool showTurnTimer = false)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.ShowAlertInternal(message, showTime, showTurnTimer);
		}
	}

	public static void NetworkAlert(string message, bool instant = false, float showTime = 0f)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)instance == (Object)null))
		{
			instance.ShowNetworkAlertInternal(message, NETWORK_ALERT_COLOR, instant, showTime);
		}
	}

	public static void NetworkAlert(string message, Color color, bool instant = false, float showTime = 0f)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)instance == (Object)null))
		{
			instance.ShowNetworkAlertInternal(message, color, instant, showTime);
		}
	}

	public static void HideAlert()
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.HideAlertInternal();
		}
	}

	public static void HideNetworkAlert(bool instant = false)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.HideNetworkAlertInternal(instant);
		}
	}

	public static void SetAlertPosition(float position)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.currentAlertPosition = position;
			instance.UpdatePositions();
		}
	}

	public static void SetNetworkAlertPosition(float position)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.currentNetworkAlertPosition = position;
			instance.UpdatePositions();
		}
	}

	public static void ResetAlertPosition()
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.currentAlertPosition = instance.alertPosY;
			instance.UpdatePositions();
		}
	}

	public static void ResetNetworkAlertPosition()
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.currentNetworkAlertPosition = instance.networkAlertYPos;
			instance.UpdatePositions();
		}
	}

	public static void IgnoreNotifications(bool ignore)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.ignoreNotifications = ignore;
		}
	}

	public static void SetSilenceAlerts(bool silenced)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.silenceAlerts = silenced;
		}
	}

	public static void ShowCenterNotification(string header, string message = null, float hideDelay = -1f)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.centerNotification.Header = header;
			instance.centerNotification.Message = message;
			instance.centerNotification.Show(hideDelay);
			instance.UpdateVisibility();
		}
	}

	public static NotificationBase GetBasicNotification()
	{
		if ((Object)(object)instance == (Object)null)
		{
			return null;
		}
		string text = "basicPopup";
		NotificationBase notificationBase;
		if (!instance.notificationCache.ContainsKey(text) || instance.notificationCache[text].Count == 0)
		{
			notificationBase = Object.Instantiate<NotificationBase>(instance.basicNotificationPrefab, (Transform)(object)instance.notificationContainer);
			notificationBase.notificationId = text;
			notificationBase.notificationManager = instance;
			notificationBase.showPosY = instance.GetBaseYPosition();
			notificationBase.startOffsetY = instance.startOffsetY;
			notificationBase.Init();
		}
		else
		{
			notificationBase = instance.notificationCache[text].Pop();
		}
		((Transform)notificationBase.rectTransform).SetAsLastSibling();
		return notificationBase;
	}

	public static DiplomacyNotification GetDiplomacyNotification()
	{
		if ((Object)(object)instance == (Object)null)
		{
			return null;
		}
		string text = "diplomacyPopup";
		DiplomacyNotification diplomacyNotification;
		if (!instance.notificationCache.ContainsKey(text) || instance.notificationCache[text].Count == 0)
		{
			diplomacyNotification = Object.Instantiate<DiplomacyNotification>(instance.diplomacyNotificationPrefab, (Transform)(object)instance.notificationContainer);
			diplomacyNotification.notificationId = text;
			diplomacyNotification.notificationManager = instance;
			diplomacyNotification.showPosY = instance.endPosY;
			diplomacyNotification.startOffsetY = instance.startOffsetY;
			diplomacyNotification.Init();
		}
		else
		{
			diplomacyNotification = instance.notificationCache[text].Pop() as DiplomacyNotification;
		}
		((Transform)diplomacyNotification.rectTransform).SetAsLastSibling();
		return diplomacyNotification;
	}

	public static void UpdateIngameAlert()
	{
		if (GameManager.GameState.CurrentState != GameState.State.Ended && GameManager.GameState.CurrentPlayer != byte.MaxValue && !GameManager.Client.IsReplay && !GameManager.IsPlayerViewing(GameManager.GameState.CurrentPlayer) && GameManager.GameState.TryGetPlayer(GameManager.GameState.CurrentPlayer, out var playerState))
		{
			if (GameManager.GameState.Settings.GameType == GameType.Multiplayer || GameManager.GameState.Settings.GameType == GameType.Competitive)
			{
				string text = playerState.UserName;
				if (text.Length > 20)
				{
					text = $"{playerState.UserName.Truncate(20)}..";
				}
				if (playerState.AccountId.HasValue && AccountManager.IsPlayerPlayingCurrentGame(playerState.AccountId.ToString()))
				{
					Alert(Localization.Get("world.turn.waiting.playing", text), 0f, showTurnTimer: true);
				}
				else
				{
					Alert(Localization.Get("world.turn.waiting", text), 0f, showTurnTimer: true);
				}
			}
			else if (GameManager.LocalPlayer.KnowsPlayer(GameManager.GameState.CurrentPlayer))
			{
				Alert(Localization.Get("world.turn.waiting", playerState.UserName));
			}
			else
			{
				Alert(Localization.Get("world.turn.waiting.unknown"));
			}
		}
		else
		{
			HideAlert();
		}
	}

	private float GetBaseYPosition()
	{
		if (GameManager.Client != null && GameManager.Client.IsReplay)
		{
			return endPosY - 130f;
		}
		return endPosY;
	}

	private static void UpdatePlayerStatus()
	{
		if (GameManager.Client != null && GameManager.Client.ActionManager != null && GameManager.Client.CurrentGameId.HasValue && !(GameManager.Client.CurrentGameId == Guid.Empty))
		{
			UpdateIngameAlert();
		}
	}
}
