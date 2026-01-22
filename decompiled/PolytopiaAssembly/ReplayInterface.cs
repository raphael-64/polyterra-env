using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReplayInterface : MonoBehaviour
{
	[SerializeField]
	protected RectTransform rectTransform;

	public Timeline timeline;

	public UIRoundButton viewmodeSelectButton;

	public UIRoundButton autoFocusButton;

	private SelectViewmodePopup selectViewmodePopup;

	private void OnEnable()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		SubscribeButtonsEvents();
		SetupViewmodeButton();
		SetupAutoFocusButton();
		SystemEvents.OnSafeAreaChanged += OnSafeAreaChanged;
		GameEvents.OnTurnStarted += OnTurnStarted;
		UIEvents.OnScreenOpen += OnScreenOpen;
		UIEvents.OnScreenClose += OnScreenClose;
		UIEvents.OnAutoCameraFocusEnabled += OnAutoCameraFocusEnabled;
		UIEvents.OnForceRefreshHud += OnForceRefreshHud;
		Timeline obj = timeline;
		obj.onSeekComplete = (Action)Delegate.Combine(obj.onSeekComplete, new Action(OnSeekComplete));
		RefreshSafeArea(ScreenManager.GetSafeArea());
		OnAutoCameraFocusEnabled(CameraController.GetIsAutoFocusEnabled());
	}

	private void OnDisable()
	{
		UnsubscribeButtonsEvents();
		SystemEvents.OnSafeAreaChanged -= OnSafeAreaChanged;
		GameEvents.OnTurnStarted -= OnTurnStarted;
		UIEvents.OnScreenOpen -= OnScreenOpen;
		UIEvents.OnScreenClose -= OnScreenClose;
		UIEvents.OnAutoCameraFocusEnabled -= OnAutoCameraFocusEnabled;
		UIEvents.OnForceRefreshHud -= OnForceRefreshHud;
		Timeline obj = timeline;
		obj.onSeekComplete = (Action)Delegate.Remove(obj.onSeekComplete, new Action(OnSeekComplete));
	}

	private void Update()
	{
		if (InputManager.GetKeyUp((KeyCode)112))
		{
			ShowViewModePopup();
		}
		if (InputManager.GetKeyUp((KeyCode)32))
		{
			if (GameManager.Client.ActionManager.IsPaused)
			{
				timeline.Play();
			}
			else
			{
				timeline.Pause();
			}
		}
		if (InputManager.GetKeyUp((KeyCode)105))
		{
			timeline.JumpBack();
		}
		else if (InputManager.GetKeyUp((KeyCode)111))
		{
			timeline.JumpForward();
		}
	}

	public void SetData(GameState gameState)
	{
		timeline.SetData(gameState);
		UpdateButton();
	}

	private void OnTurnStarted()
	{
		UpdateButton();
	}

	private void OnForceRefreshHud()
	{
		UpdateButton();
	}

	private void OnSafeAreaChanged(Rect safeArea)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		RefreshSafeArea(safeArea);
	}

	private void RefreshSafeArea(Rect safeArea)
	{
		rectTransform.SetAnchoredY(0f - ScreenManager.SafeTop * UICanvasScalerHelper.GetInvertedUIScale());
	}

	private void OnScreenOpen(UIConstants.Screens screen)
	{
		Log.Verbose("Show screen: {0}", new object[1] { screen });
		switch (screen)
		{
		case UIConstants.Screens.TechTree:
			((Component)timeline).gameObject.SetActive(false);
			((Component)viewmodeSelectButton).gameObject.SetActive(false);
			((Component)autoFocusButton).gameObject.SetActive(false);
			GameManager.Client.ActionManager.Pause();
			break;
		case UIConstants.Screens.IngameMenu:
		case UIConstants.Screens.StatsScreen:
			GameManager.Client.ActionManager.Pause();
			break;
		}
	}

	private void OnScreenClose(UIConstants.Screens screen)
	{
		Log.Verbose("Hide screen: {0}", new object[1] { screen });
		switch (screen)
		{
		case UIConstants.Screens.TechTree:
			((Component)timeline).gameObject.SetActive(true);
			((Component)viewmodeSelectButton).gameObject.SetActive(true);
			((Component)autoFocusButton).gameObject.SetActive(true);
			if (!timeline.isUserPaused)
			{
				GameManager.Client.ActionManager.Resume();
			}
			break;
		case UIConstants.Screens.IngameMenu:
		case UIConstants.Screens.StatsScreen:
			if (!timeline.isUserPaused)
			{
				GameManager.Client.ActionManager.Resume();
			}
			break;
		}
	}

	private void OnAutoCameraFocusEnabled(bool enabled)
	{
		((Component)autoFocusButton).gameObject.SetActive(!enabled);
	}

	public void OnViewModeButtonClicked(int id, BaseEventData eventData = null)
	{
		ShowViewModePopup();
	}

	public void OnAutoFocusButtonClicked(int id, BaseEventData eventData = null)
	{
		CameraController.SetIsAutoFocusEnabled(value: true);
	}

	private void ShowViewModePopup()
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)selectViewmodePopup != (Object)null) || !selectViewmodePopup.IsShowing())
		{
			selectViewmodePopup = PopupManager.GetSelectViewmodePopup();
			selectViewmodePopup.Header = Localization.Get("replay.viewmode.header");
			selectViewmodePopup.SetData(GameManager.GameState);
			SelectViewmodePopup obj = selectViewmodePopup;
			obj.onAutoClicked = (UIButtonBase.ButtonAction)Delegate.Combine(obj.onAutoClicked, new UIButtonBase.ButtonAction(OnAutoClicked));
			SelectViewmodePopup obj2 = selectViewmodePopup;
			obj2.onPlayerClicked = (UIButtonBase.ButtonAction)Delegate.Combine(obj2.onPlayerClicked, new UIButtonBase.ButtonAction(OnPlayerClicked));
			selectViewmodePopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.None, delegate
				{
					ClosePopup();
				})
			};
			selectViewmodePopup.Show(Vector2.op_Implicit(((Transform)viewmodeSelectButton.rectTransform).position));
		}
	}

	public void OnSeekComplete()
	{
		UpdateButton();
	}

	private void ClosePopup()
	{
		if ((Object)(object)selectViewmodePopup != (Object)null)
		{
			selectViewmodePopup.Hide();
		}
	}

	public void SetupViewmodeButton()
	{
		viewmodeSelectButton.ShowLabel = false;
	}

	public void SetupAutoFocusButton()
	{
		autoFocusButton.ShowLabel = false;
	}

	public void UpdateButton()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.Client == null || !(GameManager.Client is ReplayClient replayClient))
		{
			return;
		}
		PlayerState playerState = null;
		if (replayClient.doAutoSwitchPlayers)
		{
			replayClient.GameState.TryGetPlayer(replayClient.GameState.CurrentPlayer, out playerState);
		}
		else
		{
			replayClient.GameState.TryGetPlayer((byte)replayClient.currentViewingPlayer, out playerState);
		}
		if (playerState != null && playerState.Id != byte.MaxValue)
		{
			viewmodeSelectButton.rectTransform.sizeDelta = new Vector2(75f, 75f);
			viewmodeSelectButton.iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandleCallback)
			{
				viewmodeSelectButton.SetFaceIcon(spriteHandleCallback.sprite);
			});
			viewmodeSelectButton.iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddresses(replayClient.GameState, playerState));
			((Component)viewmodeSelectButton.Outline).gameObject.SetActive(false);
			((Graphic)viewmodeSelectButton.BG).color = playerState.GetPlayerColor(GameManager.GameState);
		}
	}

	private void OnAutoClicked(int id, BaseEventData eventData = null)
	{
		if (GameManager.Client != null && GameManager.Client is ReplayClient replayClient)
		{
			replayClient.doAutoSwitchPlayers = true;
			MapRenderer.Current.Refresh();
			UpdateButton();
		}
	}

	private void OnPlayerClicked(int id, BaseEventData eventData = null)
	{
		if (GameManager.Client != null && GameManager.Client is ReplayClient replayClient)
		{
			replayClient.doAutoSwitchPlayers = false;
			replayClient.currentViewingPlayer = id;
			MapRenderer.Current.Refresh();
			UpdateButton();
		}
	}

	protected void SubscribeButtonsEvents()
	{
		viewmodeSelectButton.OnClicked += OnViewModeButtonClicked;
		autoFocusButton.OnClicked += OnAutoFocusButtonClicked;
	}

	protected void UnsubscribeButtonsEvents()
	{
		viewmodeSelectButton.OnClicked -= OnViewModeButtonClicked;
		autoFocusButton.OnClicked -= OnAutoFocusButtonClicked;
	}
}
