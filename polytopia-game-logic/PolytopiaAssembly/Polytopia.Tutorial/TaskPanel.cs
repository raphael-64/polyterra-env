using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Polytopia.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Polytopia.Tutorial;

public class TaskPanel : UIButtonBase
{
	[Header("Task Panel")]
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private RectTransform containerRectTransform;

	[SerializeField]
	private Image background;

	[SerializeField]
	private RectTransform iconContainer;

	[SerializeField]
	private TMPLocalizer localizer;

	[SerializeField]
	private TextMeshProUGUI tutorialStep;

	private readonly float colorAlpha = 0.8f;

	private readonly float fadeAnimationTime = 0.5f;

	private readonly float scaleAnimationTime = 0.3f;

	private Sequence animationSequence;

	private float startYOffset;

	public override void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		startYOffset = base.rectTransform.anchoredPosition.y;
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SystemEvents.OnSafeAreaChanged += SystemEvents_OnSafeAreaChanged;
		RefreshSafeArea();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		SystemEvents.OnSafeAreaChanged -= SystemEvents_OnSafeAreaChanged;
	}

	public void Show(Action onComplete = null)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		SetBackgroundColor(ColorConstants.blue);
		((Component)this).gameObject.SetActive(true);
		canvasGroup.alpha = 1f;
		((Transform)containerRectTransform).localScale = Vector3.zero;
		TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)containerRectTransform, 1f, scaleAnimationTime), (Ease)27, 2f), new TweenCallback(OnShowComplete));
		AudioManager.PlaySFX(SFXTypes.Suggestion);
		void OnShowComplete()
		{
			canvasGroup.interactable = true;
			onComplete?.Invoke();
		}
	}

	public void Hide(Action onComplete = null)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		canvasGroup.interactable = false;
		TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(canvasGroup.DOFade(0f, fadeAnimationTime), (Ease)1), new TweenCallback(OnHideComplete));
		void OnHideComplete()
		{
			canvasGroup.interactable = false;
			((Component)this).gameObject.SetActive(false);
			onComplete?.Invoke();
		}
	}

	public void SetTaskCompleted()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		if (animationSequence != null)
		{
			TweenExtensions.Kill((Tween)(object)animationSequence, true);
		}
		animationSequence = DOTween.Sequence();
		TweenSettingsExtensions.Append(animationSequence, (Tween)(object)ShortcutExtensions.DOScale(((Component)this).transform, 1.1f, 0.1f));
		TweenSettingsExtensions.AppendCallback(animationSequence, new TweenCallback(SetComplete));
		TweenSettingsExtensions.Append(animationSequence, (Tween)(object)ShortcutExtensions.DOScale(((Component)this).transform, 1f, 0.15f));
		canvasGroup.interactable = false;
		void SetComplete()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			SetBackgroundColor(ColorConstants.green);
			AudioManager.PlaySFX(SFXTypes.TaskCompleted);
		}
	}

	public void SetIcon(UnitData unitData, PlayerState playerState)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (RectTransform item in (Transform)iconContainer)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
		UIUnitRenderer uIUnitRenderer = UIUtils.GetUIUnitRenderer(unitData, playerState);
		((Transform)uIUnitRenderer.rectTransform).SetParent((Transform)(object)iconContainer, false);
		UIUtils.FitImageContentInParent(uIUnitRenderer.rectTransform);
	}

	public void SetIcon(Sprite sprite)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (RectTransform item in (Transform)iconContainer)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
		if (Object.op_Implicit((Object)(object)sprite))
		{
			Image image = UIUtils.GetImage(sprite);
			((Transform)((Graphic)image).rectTransform).SetParent((Transform)(object)iconContainer, false);
			UIUtils.FitImageContentInParent(((Graphic)image).rectTransform);
		}
	}

	public void SetTutorialHeader(string value)
	{
		((TMP_Text)tutorialStep).text = value;
	}

	public void SetDescription(string key)
	{
		localizer.Key = key;
	}

	private void SetBackgroundColor(Color color)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		color.a = colorAlpha;
		((Graphic)background).color = color;
	}

	public void SetYOffset(float yOffset)
	{
		startYOffset = yOffset;
		RefreshSafeArea();
	}

	private void RefreshSafeArea()
	{
		base.rectTransform.SetAnchoredY(startYOffset - ScreenManager.SafeTop * UICanvasScalerHelper.GetInvertedUIScale());
	}

	private void SystemEvents_OnSafeAreaChanged(Rect safeArea)
	{
		RefreshSafeArea();
	}
}
