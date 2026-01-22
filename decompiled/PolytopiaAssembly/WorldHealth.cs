using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

public class WorldHealth : UIWorldIconBase
{
	public TextMeshProUGUI label;

	public CanvasGroup canvasGroup;

	[SerializeField]
	private float scaleDuration = 0.5f;

	[SerializeField]
	private float translationDuration = 0.4f;

	[SerializeField]
	private float intervalDuration;

	private Sequence tweenSequence;

	public float HealthValue
	{
		set
		{
			((TMP_Text)label).text = LocalizationUtils.FormatNumber(value);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ResetItem();
	}

	protected void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		((Transform)base.rectTransform).localScale = Vector3.one * CameraController.CurrentZoom;
	}

	protected override void InternalShow()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		base.InternalShow();
		KeepWorldPosition = true;
		((Transform)((TMP_Text)label).rectTransform).localScale = Vector3.zero;
		tweenSequence = DOTween.Sequence();
		TweenSettingsExtensions.Append(tweenSequence, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)((TMP_Text)label).rectTransform, 1f, scaleDuration), (Ease)27, 1.5f));
		TweenSettingsExtensions.AppendInterval(tweenSequence, intervalDuration);
		TweenSettingsExtensions.Append(tweenSequence, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetRelative<TweenerCore<Vector2, Vector2, VectorOptions>>(base.rectTransform.DOAnchorPosY(80f, translationDuration)), (Ease)8));
		if ((Object)(object)canvasGroup != (Object)null)
		{
			TweenSettingsExtensions.Join(tweenSequence, (Tween)(object)canvasGroup.DOFade(0f, 0.4f));
		}
		TweenSettingsExtensions.AppendCallback(tweenSequence, new TweenCallback(ShowAnimComplete));
	}

	private void ShowAnimComplete()
	{
		ReturnToPool();
	}

	public override void ResetItem()
	{
		base.ResetItem();
		if ((Object)(object)canvasGroup != (Object)null)
		{
			canvasGroup.alpha = 1f;
		}
		if (tweenSequence != null)
		{
			TweenExtensions.Kill((Tween)(object)tweenSequence, false);
		}
		DOTween.Kill((object)base.rectTransform, false);
		DOTween.Kill((object)((TMP_Text)label).rectTransform, false);
		DOTween.Kill((object)canvasGroup, false);
	}
}
