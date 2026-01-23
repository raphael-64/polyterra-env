using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class UISunriseBg : MonoBehaviour
{
	[SerializeField]
	protected CanvasGroup cnvsGrp;

	[SerializeField]
	protected GameObject starContainer;

	protected Tween fadeTween;

	public void Init()
	{
		cnvsGrp.alpha = 0f;
		((Component)this).gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		WorldEvents.OnSunriseVisibility += OnSunriseVisibility;
	}

	private void OnDisable()
	{
		WorldEvents.OnSunriseVisibility -= OnSunriseVisibility;
	}

	private void OnSunriseVisibility(bool showing)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		TweenUtils.KillTween(fadeTween);
		if (showing)
		{
			((Component)this).gameObject.SetActive(true);
			fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(cnvsGrp.DOFade(1f, SunriseBg.SUNRISE_TIME), (Ease)2), (TweenCallback)delegate
			{
				starContainer.SetActive(false);
			});
		}
		else
		{
			starContainer.SetActive(true);
			fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(cnvsGrp.DOFade(0f, SunriseBg.SUNRISE_TIME), (Ease)3), (TweenCallback)delegate
			{
				((Component)this).gameObject.SetActive(false);
			});
		}
	}
}
