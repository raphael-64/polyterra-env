using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickActions : UIWorldIconBase
{
	public RectTransform buttonContainer;

	[SerializeField]
	protected float minRadius = 90f;

	[SerializeField]
	protected float angleOffset;

	[SerializeField]
	protected float minOuterRadius = 255f;

	[SerializeField]
	protected float minInnerRadius = 111f;

	[SerializeField]
	protected Image backgroundFill;

	[SerializeField]
	protected Image backgroundMask;

	[SerializeField]
	protected ButtonSprite buttonGlyphMoveLeft;

	[SerializeField]
	protected ButtonSprite buttonGlyphMoveRight;

	private List<UIRoundButton> buttons = new List<UIRoundButton>();

	private List<Tween> tweens = new List<Tween>();

	private Sequence sequence;

	private Vector2[] endPositions;

	private float radius;

	private Vector2 buttonGlyphMoveLeftEndPosition;

	private Vector2 buttonGlyphMoveRightEndPosition;

	private int oldSelectedButton = -1;

	public bool IsShowing;

	private bool waitingForMouseUp;

	private bool nextButtonHoverFromGamepad;

	public override bool KeepWorldPosition
	{
		get
		{
			return m_keepWorldPosition;
		}
		set
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			m_keepWorldPosition = value;
			lastCamPos = ((Component)CameraController.Camera).transform.position;
			CameraZoom = CameraController.CurrentZoom;
		}
	}

	public override Transform Target
	{
		get
		{
			return m_target;
		}
		set
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			m_target = value;
			lastCamPos = ((Component)CameraController.Camera).transform.position;
			CameraZoom = CameraController.CurrentZoom;
			Position = m_target.position;
		}
	}

	public override Vector3 Position
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_position;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			m_position = value;
			KeepWorldPosition = true;
			UIPosition = Vector2.op_Implicit(CameraController.Camera.WorldToScreenPoint(m_position + WorldOffset) * UICanvasScalerHelper.GetInvertedUIScale());
		}
	}

	protected override void OnDisable()
	{
		if (waitingForMouseUp)
		{
			waitingForMouseUp = false;
			InputManager.EnableInput(InputManager.InputType.Map);
		}
		base.OnDisable();
	}

	public void Show(bool instant = false)
	{
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Expected O, but got Unknown
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Expected O, but got Unknown
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Expected O, but got Unknown
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		base.Show();
		IsShowing = true;
		ClearTweens();
		RefreshButtons();
		int count = buttons.Count;
		((Component)this).gameObject.SetActive(count > 0);
		oldSelectedButton = 0;
		if (buttons.Count > 0)
		{
			PolytopiaInput.Omnicursor.AffixToUIElement(buttons[0].rectTransform);
		}
		if (count <= 0)
		{
			return;
		}
		if (instant)
		{
			for (int i = 0; i < count; i++)
			{
				buttons[i].rectTransform.anchoredPosition = endPositions[i];
				buttons[i].CanvasGroup.alpha = 1f;
			}
			if (buttons.Count > 1)
			{
				buttonGlyphMoveLeft.rectTransform.anchoredPosition = buttonGlyphMoveLeftEndPosition;
				((Graphic)buttonGlyphMoveLeft.icon).color = new Color(1f, 1f, 1f, 1f);
				buttonGlyphMoveRight.rectTransform.anchoredPosition = buttonGlyphMoveRightEndPosition;
				((Graphic)buttonGlyphMoveRight.icon).color = new Color(1f, 1f, 1f, 1f);
			}
			((Graphic)backgroundFill).color = ColorUtil.SetAlphaOnColor(((Graphic)backgroundFill).color, 0.4f);
			((Transform)((Graphic)backgroundFill).rectTransform).localScale = Vector3.one;
			UIUtils.SetExplicitNavigation(buttonContainer, useCenter: true, 2);
		}
		else
		{
			sequence = DOTween.Sequence();
			for (int j = 0; j < count; j++)
			{
				UIRoundButton button = buttons[j];
				button.ButtonEnabled = false;
				button.AnimationsEnabled = false;
				Navigation navigation = default(Navigation);
				((Navigation)(ref navigation)).mode = (Mode)4;
				((Selectable)button.button).navigation = navigation;
				Tween val = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(button.rectTransform.DOAnchorPos(endPositions[j], 0.2f), (Ease)27), 0.05f), (TweenCallback)delegate
				{
					button.AnimationsEnabled = true;
				});
				TweenSettingsExtensions.Join(sequence, val);
				tweens.Add(val);
				tweens.Add((Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(button.CanvasGroup.DOFade(1f, 0.2f), (Ease)27), (float)j * 0.05f), (TweenCallback)delegate
				{
					button.ButtonEnabled = true;
				}));
			}
			TweenSettingsExtensions.AppendCallback(sequence, (TweenCallback)delegate
			{
				UIUtils.SetExplicitNavigation(buttonContainer, useCenter: true, 2);
				if (LevelManager.GetClientInteraction().GetCurrentButtonInputMode() == ClientInteraction.ButtonInputMode.Menu)
				{
					ShowKeySelection(shouldShow: true);
				}
			});
			((Graphic)backgroundFill).color = ColorUtil.SetAlphaOnColor(((Graphic)backgroundFill).color, 0f);
			((Transform)((Graphic)backgroundFill).rectTransform).localScale = new Vector3(0.5f, 0.5f, 1f);
			((Transform)((Graphic)backgroundMask).rectTransform).localScale = new Vector3(0.5f, 0.5f, 1f);
			if (buttons.Count > 1)
			{
				tweens.Add((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(buttonGlyphMoveLeft.rectTransform.DOAnchorPos(buttonGlyphMoveLeftEndPosition, 0.2f), (Ease)27));
				tweens.Add((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(buttonGlyphMoveRight.rectTransform.DOAnchorPos(buttonGlyphMoveRightEndPosition, 0.2f), (Ease)27));
				tweens.Add((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(buttonGlyphMoveLeft.icon.DOColor(new Color(1f, 1f, 1f, 1f), 0.2f), (Ease)27));
				tweens.Add((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(buttonGlyphMoveRight.icon.DOColor(new Color(1f, 1f, 1f, 1f), 0.2f), (Ease)27));
			}
			tweens.Add((Tween)(object)backgroundFill.DOFade(0.4f, 0.2f));
			tweens.Add((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)((Graphic)backgroundFill).rectTransform, 1f, 0.2f), (Ease)27));
			tweens.Add((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)((Graphic)backgroundMask).rectTransform, 1f, 0.2f), (Ease)27));
		}
		UIEvents.QuickActionsOpen(m_coordinate, open: true);
	}

	public void Hide(bool instant = false)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		IsShowing = false;
		UIEvents.QuickActionsOpen(m_coordinate, open: false);
		PolytopiaInput.Omnicursor.RestoreTileAffix();
		Clear();
		((Component)this).gameObject.SetActive(false);
		buttonGlyphMoveLeft.rectTransform.anchoredPosition = Vector2.zero;
		buttonGlyphMoveRight.rectTransform.anchoredPosition = Vector2.zero;
		((Graphic)buttonGlyphMoveLeft.icon).color = new Color(1f, 1f, 1f, 0f);
		((Graphic)buttonGlyphMoveRight.icon).color = new Color(1f, 1f, 1f, 0f);
	}

	public void AddButton(UIRoundButton button)
	{
		buttons.Add(button);
		button.id = buttons.Count - 1;
		button.OnEnter += OnButtonHover;
	}

	private void OnButtonHover(int id, BaseEventData eventData)
	{
		if (!nextButtonHoverFromGamepad)
		{
			oldSelectedButton = id;
		}
		if (id < buttons.Count)
		{
			((Transform)buttons[id].rectTransform).SetAsLastSibling();
		}
		nextButtonHoverFromGamepad = false;
	}

	public bool HasButtons()
	{
		return buttons.Count > 0;
	}

	public void ShowKeySelection(bool shouldShow)
	{
		if (shouldShow && HasButtons())
		{
			UINavigationManager.Select((Selectable)(object)buttons[0].button);
		}
	}

	public void SortButtons()
	{
		if (buttons != null && buttons.Count != 0)
		{
			buttons.Sort(new ButtonSort());
		}
	}

	public void MoveSelectedButton(int offset)
	{
		if (buttons.Count != 0)
		{
			if (oldSelectedButton < 0)
			{
				SetSelectedButton((offset > 0) ? (buttons.Count - 1) : 0);
			}
			else
			{
				SetSelectedButton((oldSelectedButton + buttons.Count + offset) % buttons.Count);
			}
		}
	}

	public void SetSelectedButton(int index)
	{
		UIRoundButton uIRoundButton = buttons[index];
		nextButtonHoverFromGamepad = true;
		uIRoundButton.Highlighted = true;
		PolytopiaInput.Omnicursor.AffixToUIElement(uIRoundButton.rectTransform);
		UINavigationManager.Select(((Component)uIRoundButton).GetComponent<Selectable>());
		oldSelectedButton = index;
	}

	private void RefreshButtons()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		int count = buttons.Count;
		float num = 100f;
		if (count - 8 > 0)
		{
			float num2 = 10 * (count - 8);
			radius = minRadius + num2;
			num += num2;
		}
		else
		{
			radius = minRadius;
		}
		endPositions = (Vector2[])(object)new Vector2[count];
		buttonGlyphMoveLeftEndPosition = new Vector2(0f - num, 80f);
		buttonGlyphMoveRightEndPosition = new Vector2(num, 80f);
		float num3 = 1f;
		float num4 = angleOffset / 57.29578f;
		if (count > 4)
		{
			num3 = (float)Math.PI * -2f / (float)count;
		}
		else
		{
			num3 = -1f;
			num4 = angleOffset / 57.29578f - num3 / 2f * (float)(count - 1);
		}
		Vector2 val = default(Vector2);
		for (int i = 0; i < count; i++)
		{
			UIRoundButton uIRoundButton = buttons[i];
			((Component)uIRoundButton).gameObject.layer = ((Component)this).gameObject.layer;
			uIRoundButton.ShowTextBackground = true;
			uIRoundButton.LabelShowOnHover = true;
			uIRoundButton.OnDown += OnDown;
			uIRoundButton.OnUp += OnUp;
			((Vector2)(ref val))._002Ector(Mathf.Sin(num4) * radius, Mathf.Cos(num4) * (0f - radius));
			endPositions[i] = val;
			uIRoundButton.rectTransform.anchoredPosition = Vector2.zero;
			uIRoundButton.CanvasGroup.alpha = 0f;
			num4 += num3;
			buttons[i] = uIRoundButton;
		}
		UpdateBg();
	}

	private void OnDown(int id, BaseEventData eventData)
	{
		if (!waitingForMouseUp)
		{
			InputManager.DisableInput(InputManager.InputType.Map);
		}
		waitingForMouseUp = true;
	}

	private void OnUp(int id, BaseEventData eventData)
	{
		InputManager.EnableInput(InputManager.InputType.Map);
		waitingForMouseUp = false;
	}

	private void UpdateBg()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		float num = ((buttons.Count > 4) ? 360 : Mathf.Min(45 * buttons.Count + 10 * buttons.Count, 360));
		backgroundFill.fillAmount = num / 360f;
		((Transform)((Graphic)backgroundFill).rectTransform).localEulerAngles = new Vector3(0f, 0f, num * 0.5f);
		float num2 = minOuterRadius;
		float num3 = minInnerRadius;
		if (buttons.Count > 8)
		{
			float num4 = 316f;
			float num5 = 32f;
			float num6 = 12f / num4;
			num2 = (radius + num5) * 2f;
			num3 = (radius - num5) * 2f;
			num2 += num2 * num6;
			num3 -= num3 * num6;
		}
		((Graphic)backgroundFill).rectTransform.SetWidth(num2);
		((Graphic)backgroundFill).rectTransform.SetHeight(num2);
		((Graphic)backgroundMask).rectTransform.SetWidth(num3);
		((Graphic)backgroundMask).rectTransform.SetHeight(num3);
	}

	public void Clear()
	{
		ClearTweens();
		int count = buttons.Count;
		for (int i = 0; i < count; i++)
		{
			Object.Destroy((Object)(object)((Component)buttons[i]).gameObject);
		}
		buttons.Clear();
		endPositions = (Vector2[])(object)new Vector2[0];
	}

	private void ClearTweens()
	{
		if (sequence != null)
		{
			TweenExtensions.Kill((Tween)(object)sequence, false);
		}
		TweenUtils.KillTweens(tweens);
		DOTween.Kill((object)((Graphic)backgroundFill).rectTransform, false);
		DOTween.Kill((object)((Graphic)backgroundMask).rectTransform, false);
		DOTween.Kill((object)buttonGlyphMoveLeft.rectTransform, false);
		DOTween.Kill((object)buttonGlyphMoveRight.rectTransform, false);
		DOTween.Kill((object)buttonGlyphMoveLeft.icon, false);
		DOTween.Kill((object)buttonGlyphMoveRight.icon, false);
		tweens.Clear();
	}
}
