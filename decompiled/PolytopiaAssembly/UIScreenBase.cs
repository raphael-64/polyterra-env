using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScreenBase : UIBasicComponent, ISelectableContainer
{
	public enum ShowStates
	{
		None,
		Showing,
		Hidden
	}

	public UIConstants.Screens screenType;

	[SerializeField]
	protected bool disableGameInputOnShow;

	protected Selectable currentSelectable;

	[SerializeField]
	protected Selectable defaultSelectable;

	[SerializeField]
	protected bool playTribeMusic;

	[SerializeField]
	protected UIButtonBase backButton;

	[SerializeField]
	protected RectTransform omnicursorDefaultAffix;

	protected bool hasNullFallbackSelectable;

	protected Selectable fallbackSelectable;

	protected ShowStates showState;

	public virtual bool Showing => ShowState == ShowStates.Showing;

	public virtual ShowStates ShowState
	{
		get
		{
			return showState;
		}
		set
		{
			showState = value;
		}
	}

	public virtual Selectable CurrentSelectable
	{
		get
		{
			return currentSelectable;
		}
		set
		{
			currentSelectable = value;
		}
	}

	public virtual Selectable DefaultSelectable
	{
		get
		{
			return defaultSelectable;
		}
		set
		{
			defaultSelectable = value;
		}
	}

	public virtual void Show(bool instant = false)
	{
		if (ShowState == ShowStates.Showing)
		{
			return;
		}
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
		((Component)this).gameObject.SetActive(true);
		if (playTribeMusic)
		{
			AudioSource audioSource = AudioManager.GetAudioSource(AudioManager.AudioSourceTypes.TribeMusic);
			audioSource.volume = 0f;
			audioSource.clip = AudioManager.GetTribeMusic(GameManager.LocalPlayer.tribe.GetName(), GameManager.LocalPlayer.skinType.GetName());
			if (!audioSource.isPlaying)
			{
				audioSource.Play();
			}
			AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 1f, 0.6f, (Ease)1);
		}
		if (Object.op_Implicit((Object)(object)omnicursorDefaultAffix))
		{
			PolytopiaInput.Omnicursor.AffixToUIElement(omnicursorDefaultAffix);
		}
		GameManager.GetAnalyticsManager().SendEvent("screen_view", new Dictionary<string, object> { { "screen_name", screenType } });
		SubscribeButtonsEvents();
		UIEvents.ScreenOpen(screenType);
	}

	public virtual void ShowCompleted()
	{
		UINavigationManager.Select(GetCurrentSelectableOrFallback());
	}

	public async void OnLayoutComplete(Action callback)
	{
		if (callback != null && !((Object)(object)this == (Object)null))
		{
			await new WaitForEndOfFrame();
			if (callback != null && !((Object)(object)this == (Object)null))
			{
				callback();
			}
		}
	}

	public virtual void Hide(bool instant = false)
	{
		if (ShowState != ShowStates.Hidden)
		{
			PolytopiaInput.Omnicursor.RestoreTileAffix();
			SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
			((Component)this).gameObject.SetActive(false);
			ShowState = ShowStates.Hidden;
			if (playTribeMusic)
			{
				AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 0f, 0.6f, (Ease)1);
			}
			UnsubscribeButtonsEvents();
			UIEvents.ScreenClose(screenType);
		}
	}

	public virtual void OnButtonUp(InputManager.Buttons button)
	{
		if (button == InputManager.Buttons.Cancel && !PopupManager.PopupShowing)
		{
			OnBack();
		}
	}

	public virtual void OnBack()
	{
		UIManager.Instance.OnBack();
	}

	public virtual void ShowScreen(UIConstants.Screens screen, bool instant = false)
	{
		SetShowing(screen == screenType, instant);
	}

	public virtual void SetDeepLinkData(UIDeepLinkData data)
	{
	}

	public void SetShowing(bool value, bool instant)
	{
		if ((value && ShowState != ShowStates.Showing) || (!value && ShowState != ShowStates.Hidden))
		{
			if (value)
			{
				Show(instant);
				ShowCompleted();
				OnLayoutComplete(OnScreenUpdated);
			}
			else
			{
				Hide(instant);
			}
			ShowState = (value ? ShowStates.Showing : ShowStates.Hidden);
		}
	}

	private void OnScreenSizeChanged(Vector2 screenSize)
	{
		OnLayoutComplete(OnScreenUpdated);
	}

	public virtual void OnScreenUpdated()
	{
	}

	public virtual Selectable GetCurrentSelectableOrFallback()
	{
		if (!UINavigationManager.IsValidSelectable(CurrentSelectable))
		{
			return GetDefaultSelectableOrFallback();
		}
		return CurrentSelectable;
	}

	public virtual Selectable GetDefaultSelectableOrFallback()
	{
		if (UINavigationManager.IsValidSelectable(DefaultSelectable))
		{
			return DefaultSelectable;
		}
		if (!hasNullFallbackSelectable && !UINavigationManager.IsValidSelectable(fallbackSelectable))
		{
			fallbackSelectable = UINavigationManager.FindFirstNavigableSelectable(((Component)this).transform);
			hasNullFallbackSelectable = !UINavigationManager.IsValidSelectable(fallbackSelectable);
		}
		return fallbackSelectable;
	}

	protected virtual void SubscribeButtonsEvents()
	{
		if (!((Object)(object)backButton == (Object)null))
		{
			backButton.OnClicked += BackButtonOnClicked;
		}
	}

	protected virtual void UnsubscribeButtonsEvents()
	{
		if (!((Object)(object)backButton == (Object)null))
		{
			backButton.OnClicked -= BackButtonOnClicked;
		}
	}

	private void BackButtonOnClicked(int id, BaseEventData eventdata)
	{
		OnBack();
	}
}
