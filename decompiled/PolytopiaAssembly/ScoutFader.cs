using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class ScoutFader : MonoBehaviour
{
	[SerializeField]
	protected SpriteRenderer[] unitSpriteRenderers;

	[SerializeField]
	protected SpriteRenderer[] outlineRenderers;

	[SerializeField]
	protected float unitSpriteEndFade;

	[SerializeField]
	protected float outlineEndFade;

	[SerializeField]
	protected float fadeTime = 1f;

	private List<Tween> tweens = new List<Tween>();

	private MaterialPropertyBlock propBlock;

	private float currentOverlayStrength;

	private float OverlayStrength
	{
		get
		{
			return currentOverlayStrength;
		}
		set
		{
			currentOverlayStrength = value;
			SpriteRenderer[] array = unitSpriteRenderers;
			foreach (SpriteRenderer val in array)
			{
				if (!((Object)(object)val == (Object)null))
				{
					((Renderer)val).GetPropertyBlock(propBlock);
					propBlock.SetFloat("_OverlayStrength", currentOverlayStrength);
					((Renderer)val).SetPropertyBlock(propBlock);
				}
			}
		}
	}

	private void OnEnable()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		propBlock = new MaterialPropertyBlock();
		tweens.Add((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLoops<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => OverlayStrength), (DOSetter<float>)delegate(float x)
		{
			OverlayStrength = x;
		}, unitSpriteEndFade, fadeTime), -1, (LoopType)1), (Ease)10));
		SpriteRenderer[] array = outlineRenderers;
		foreach (SpriteRenderer target in array)
		{
			tweens.Add((Tween)(object)TweenSettingsExtensions.SetLoops<TweenerCore<Color, Color, ColorOptions>>(target.DOFade(outlineEndFade, fadeTime), -1, (LoopType)1));
		}
	}

	private void OnDisable()
	{
		foreach (Tween tween in tweens)
		{
			TweenUtils.KillTween(tween);
		}
	}
}
