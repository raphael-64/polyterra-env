using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class SunriseBg : MonoBehaviour
{
	public static float SUNRISE_TIME = 15f;

	[SerializeField]
	protected Renderer sunriseRenderer;

	protected Material sunriseMaterial;

	protected static SunriseBg instance;

	protected Tween gradientFadeTween;

	protected bool showing;

	public static bool Showing
	{
		get
		{
			if (Object.op_Implicit((Object)(object)instance))
			{
				return instance.showing;
			}
			return false;
		}
	}

	private void Awake()
	{
		instance = this;
		sunriseMaterial = sunriseRenderer.material;
		sunriseMaterial.SetFloat("_Alpha", 0f);
		sunriseRenderer.enabled = false;
	}

	private void OnDestroy()
	{
		TweenUtils.KillTween(gradientFadeTween);
		instance = null;
	}

	protected void ShowInternal()
	{
		showing = true;
		TweenUtils.KillTween(gradientFadeTween);
		sunriseRenderer.enabled = true;
		gradientFadeTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(ShortcutExtensions.DOFloat(sunriseMaterial, 1f, "_Alpha", SUNRISE_TIME), (Ease)2);
		WorldEvents.SunriseVisibility(showing);
	}

	protected void HideInternal()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		showing = false;
		TweenUtils.KillTween(gradientFadeTween);
		gradientFadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(ShortcutExtensions.DOFloat(sunriseMaterial, 0f, "_Alpha", SUNRISE_TIME), (Ease)3), (TweenCallback)delegate
		{
			sunriseRenderer.enabled = false;
		});
		WorldEvents.SunriseVisibility(showing);
	}

	public static void Show()
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			instance.ShowInternal();
		}
	}

	public static void Hide()
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			instance.HideInternal();
		}
	}

	public static void ToggleVisibility()
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			if (instance.showing)
			{
				instance.HideInternal();
			}
			else
			{
				instance.ShowInternal();
			}
		}
	}

	public static void Refresh()
	{
	}
}
