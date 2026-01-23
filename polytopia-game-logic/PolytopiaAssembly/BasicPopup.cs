using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BasicPopup : PopupBase
{
	[Header("Basic Popup")]
	[SerializeField]
	protected PopupButtonContainer buttonContainer;

	[SerializeField]
	protected UITextButton topButton;

	protected UIButtonBase.ButtonAction topButtonCallback;

	protected PopupButtonData[] m_buttonData;

	protected PopupButtonData topButtonData;

	protected PollAnimation<float> replayClickDelay;

	protected int replayClickAnimationStep;

	protected UIButtonBase replayClickButton;

	public virtual bool ButtonsEnabled
	{
		set
		{
			if ((Object)(object)buttonContainer != (Object)null)
			{
				buttonContainer.ButtonsEnabled = value;
			}
		}
	}

	public virtual bool ButtonAnimationsEnabled
	{
		get
		{
			if ((Object)(object)buttonContainer != (Object)null)
			{
				return buttonContainer.ButtonAnimationsEnabled;
			}
			return false;
		}
		set
		{
			if ((Object)(object)buttonContainer != (Object)null)
			{
				buttonContainer.ButtonAnimationsEnabled = value;
			}
		}
	}

	public virtual PopupButtonData[] buttonData
	{
		get
		{
			return m_buttonData;
		}
		set
		{
			m_buttonData = value;
			buttonContainer.SetButtonData(m_buttonData);
		}
	}

	public virtual PopupButtonData TopButtonData
	{
		set
		{
			if (!((Object)(object)topButton != (Object)null))
			{
				return;
			}
			topButtonData = value;
			if (value != null)
			{
				topButton.Key = value.text;
				((Object)topButton).name = $"PopupButton_{topButton.text}";
				topButton.id = value.id;
				if (value.closesPopup)
				{
					topButton.OnClicked += OnHide;
				}
				if (value.callback != null)
				{
					topButtonCallback = value.callback;
					topButton.OnClicked += topButtonCallback;
				}
				if (value.state == PopupButtonData.States.Disabled)
				{
					topButton.ButtonEnabled = false;
				}
			}
			((Component)topButton).gameObject.SetActive(value != null);
		}
	}

	public virtual UITextButton[] Buttons
	{
		get
		{
			if (!((Object)(object)buttonContainer != (Object)null))
			{
				return null;
			}
			return buttonContainer.Buttons;
		}
	}

	public virtual UITextButton TopButton => topButton;

	public override void Init()
	{
		base.Init();
		if ((Object)(object)buttonContainer != (Object)null)
		{
			buttonContainer.hideCallback = OnHide;
			((Component)buttonContainer).gameObject.SetActive(false);
		}
	}

	public void RefreshButtonState()
	{
		for (int i = 0; i < buttonData.Length; i++)
		{
			buttonData[i].stateCheck?.Invoke();
			Buttons[i].ButtonEnabled = buttonData[i].state != PopupButtonData.States.Disabled;
		}
	}

	protected override void OnShowComplete()
	{
		if (!UINavigationManager.IsValidSelectable(DefaultSelectable))
		{
			DefaultSelectable = GetStandardDefault();
		}
		base.OnShowComplete();
		if (!((Object)(object)this == (Object)null))
		{
			UIUtils.SetExplicitNavigation(base.rectTransform);
		}
	}

	public override void ReturnFocus()
	{
		base.ReturnFocus();
		UINavigationManager.Select(GetCurrentSelectableOrFallback());
	}

	public override void OnHide(int id, BaseEventData eventData)
	{
		ButtonsEnabled = false;
		ButtonAnimationsEnabled = false;
		base.OnHide(id, eventData);
	}

	public override void ResetPopup()
	{
		base.ResetPopup();
		if ((Object)(object)buttonContainer != (Object)null)
		{
			buttonContainer.ResetContainer();
		}
		TopButtonData = null;
		if ((Object)(object)topButton != (Object)null)
		{
			topButton.OnClicked -= OnHide;
			topButton.OnClicked -= topButtonCallback;
		}
	}

	protected virtual void Update()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.Client != null && GameManager.Client.IsSpectating && replayClickDelay != null && replayClickDelay.IsCompleted())
		{
			if (replayClickAnimationStep == 0)
			{
				replayClickButton.OnPointerEnterAnimation();
				replayClickDelay = new PollAnimation<float>(0.1f);
				replayClickAnimationStep++;
			}
			else if (replayClickAnimationStep == 1)
			{
				replayClickButton.OnPointerDownAnimation();
				replayClickDelay = new PollAnimation<float>(0.1f);
				AudioManager.PlaySFX(SFXTypes.Press, 1f, 1f, AudioManager.GetPanFromPosition(Vector2.op_Implicit(((Component)replayClickButton).transform.position)));
				replayClickAnimationStep++;
			}
			else if (replayClickAnimationStep == 2)
			{
				replayClickButton.OnPointerUpAnimation();
				replayClickDelay = new PollAnimation<float>(0.2f);
				replayClickAnimationStep++;
			}
			else if (replayClickAnimationStep == 3)
			{
				replayClickAnimationStep = 0;
				replayClickDelay = null;
				Hide();
			}
		}
	}

	public virtual void ReplayClickButton(int buttonIndex, Action onHideCallback = null, float delay = 0.8f)
	{
		if (buttonIndex < 0 || buttonIndex > Buttons.Length - 1)
		{
			Log.Warning("{0}.ReplayClickButton does not have a button index: {1}", new object[2]
			{
				((Object)this).name,
				buttonIndex
			});
		}
		else
		{
			ReplayClickButton(Buttons[buttonIndex], onHideCallback, delay);
		}
	}

	public virtual void ReplayClickButton(UIButtonBase button, Action onHideCallback = null, float delay = 0.8f)
	{
		replayClickButton = button;
		replayClickButton.AnimationsEnabled = true;
		replayClickAnimationStep = 0;
		replayClickDelay = new PollAnimation<float>(delay);
		HideCallback = onHideCallback;
	}

	public void RefreshButtons()
	{
		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].UpdateColors();
		}
	}

	public UITextButton GetButtonById(int id)
	{
		if (Buttons.Length != 0)
		{
			UITextButton[] buttons = Buttons;
			foreach (UITextButton uITextButton in buttons)
			{
				if (uITextButton.id == id)
				{
					return uITextButton;
				}
			}
		}
		return null;
	}

	protected override Selectable GetStandardDefault()
	{
		if ((Object)(object)buttonContainer != (Object)null && (Object)(object)buttonContainer.StartSelection != (Object)null)
		{
			return buttonContainer.StartSelection;
		}
		if (topButtonData != null && topButtonData.state == PopupButtonData.States.Disabled)
		{
			return (Selectable)(object)topButton.button;
		}
		if (Buttons != null && Buttons.Length != 0)
		{
			return (Selectable)(object)Buttons[0].button;
		}
		if (topButtonData != null)
		{
			return (Selectable)(object)topButton.button;
		}
		return null;
	}
}
