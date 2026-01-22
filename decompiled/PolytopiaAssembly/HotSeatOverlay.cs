using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotSeatOverlay : UIScreenBase
{
	[SerializeField]
	protected Image bg;

	[SerializeField]
	protected TextMeshProUGUI nameLabel;

	[SerializeField]
	protected Image faceIcon;

	[SerializeField]
	protected GameObject contentContainer;

	[SerializeField]
	protected UIButtonBase startButton;

	protected static HotSeatOverlay instance;

	protected Tween fadeTween;

	protected PlayerState player;

	protected Action continueCallback;

	protected Action showCompleteCallback;

	protected Action hideCompleteCallback;

	protected Action exitCallback;

	private bool isActive;

	private SpriteHandle faceSpriteHandle = new SpriteHandle();

	public static PlayerState Player
	{
		set
		{
			instance.player = value;
		}
	}

	public static Action ContinueCallback
	{
		set
		{
			instance.continueCallback = value;
		}
	}

	public static Action ExitCallback
	{
		set
		{
			instance.exitCallback = value;
		}
	}

	public static Action ShowCompleteCallback
	{
		set
		{
			instance.showCompleteCallback = value;
		}
	}

	public static Action HideCompleteCallback
	{
		set
		{
			instance.hideCompleteCallback = value;
		}
	}

	public static bool IsActive
	{
		get
		{
			if ((Object)(object)instance == (Object)null)
			{
				return false;
			}
			return instance.isActive;
		}
	}

	public override void Init()
	{
		base.Init();
		faceSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetFaceIcon(spriteHandle.sprite);
		});
		instance = this;
		Hide(instant: true);
	}

	protected void ShowInternal(bool instant = false)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		TweenUtils.KillTween(fadeTween);
		if (player == null)
		{
			player = GameManager.LocalPlayer;
		}
		((TMP_Text)nameLabel).text = player.DisplayUserName;
		faceSpriteHandle.Request(SpriteData.GetHeadSpriteAddresses(GameManager.GameState, player));
		contentContainer.SetActive(false);
		((Component)this).gameObject.SetActive(true);
		if (instant)
		{
			((Graphic)bg).color = Color.black;
			if (!player.AutoPlay)
			{
				contentContainer.SetActive(true);
				UINavigationManager.Select((Selectable)(object)startButton.button);
			}
			OnShowComplete();
			return;
		}
		((Graphic)bg).color = Color.clear;
		fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Color, Color, ColorOptions>>(bg.DOFade(1f, 0.5f), (TweenCallback)delegate
		{
			if (!player.AutoPlay)
			{
				contentContainer.SetActive(true);
				UINavigationManager.Select((Selectable)(object)startButton.button);
			}
			OnShowComplete();
		});
	}

	private void SetFaceIcon(Sprite sprite)
	{
		faceIcon.sprite = sprite;
		faceIcon.preserveAspect = true;
		faceIcon.useSpriteMesh = true;
	}

	protected void HideInternal(bool instant = false)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		isActive = false;
		TweenUtils.KillTween(fadeTween);
		if (instant)
		{
			((Graphic)bg).color = Color.clear;
			contentContainer.SetActive(false);
			((Component)this).gameObject.SetActive(false);
			ContinueClicked();
			return;
		}
		((Component)this).gameObject.SetActive(true);
		contentContainer.SetActive(false);
		fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Color, Color, ColorOptions>>(bg.DOFade(0f, 0.5f), (TweenCallback)delegate
		{
			((Component)this).gameObject.SetActive(false);
			OnHideComplete();
			ResetScreen();
		});
	}

	protected void OnShowComplete()
	{
		isActive = true;
		showCompleteCallback?.Invoke();
		showCompleteCallback = null;
	}

	protected void OnHideComplete()
	{
		hideCompleteCallback?.Invoke();
		hideCompleteCallback = null;
	}

	public void ContinueClicked()
	{
		Action action = continueCallback;
		continueCallback = null;
		action?.Invoke();
	}

	public override void OnBack()
	{
		OnExitClicked();
	}

	public void OnExitClicked()
	{
		exitCallback?.Invoke();
		ResetScreen();
		UIManager.QueuedDeepLink = UIConstants.Screens.MultiplayerScreen;
		GameManager.ReturnToMenu();
	}

	protected void ResetScreen()
	{
		player = null;
		continueCallback = null;
		exitCallback = null;
		showCompleteCallback = null;
		hideCompleteCallback = null;
	}

	public override void Show(bool instant = false)
	{
		if (!instant && ((Component)instance).gameObject.activeSelf)
		{
			instant = true;
		}
		base.Show(instant);
		Log.Verbose("Show overlay for player {0} (instant: {1})", new object[2]
		{
			instance.player?.Id,
			instant
		});
		instance.ShowInternal(instant);
	}

	public override void Hide(bool instant = false)
	{
		Log.Verbose("Hide overlay for player {0} (instant: {1})", new object[2]
		{
			instance.player?.Id,
			instant
		});
		base.Hide(instant);
		instance.HideInternal(instant);
	}

	protected override void SubscribeButtonsEvents()
	{
		base.SubscribeButtonsEvents();
		startButton.OnClicked += StartButtonOnClicked;
	}

	protected override void UnsubscribeButtonsEvents()
	{
		base.UnsubscribeButtonsEvents();
		startButton.OnClicked -= StartButtonOnClicked;
	}

	private void StartButtonOnClicked(int id, BaseEventData eventdata)
	{
		ContinueClicked();
	}
}
