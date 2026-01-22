using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class MixerOverlay : UIBasicComponent
{
	[SerializeField]
	protected Image flash;

	[SerializeField]
	protected int minMix = 100;

	[SerializeField]
	protected int maxMix = 400;

	protected bool showing;

	protected int mixCounter;

	protected List<MixerSpark> sparks = new List<MixerSpark>();

	protected RectTransform source;

	protected RectTransform target;

	protected Action mixComplete;

	public void Show(RectTransform source, RectTransform target, Action mixComplete)
	{
		this.source = source;
		this.target = target;
		this.mixComplete = mixComplete;
		mixCounter = 0;
		showing = true;
		((Component)flash).gameObject.SetActive(false);
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		showing = false;
		((Component)this).gameObject.SetActive(false);
		foreach (MixerSpark spark in sparks)
		{
			if (spark.IsUsed)
			{
				spark.Kill();
			}
		}
	}

	public void ShowFlash()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		((Graphic)flash).color = Color.white;
		((Component)flash).gameObject.SetActive(true);
		TweenSettingsExtensions.OnComplete<TweenerCore<Color, Color, ColorOptions>>(flash.DOFade(0f, 2f), (TweenCallback)delegate
		{
			((Component)flash).gameObject.SetActive(false);
		});
		AudioManager.PlaySFX(SFXTypes.MergeTribe);
	}

	private void Update()
	{
		mixCounter++;
		if ((float)mixCounter > (float)minMix + Random.value * (float)maxMix)
		{
			MixerSpark pooledObject = ObjectPool.GetPooledObject<MixerSpark>("MixerSpark");
			pooledObject.Setup(base.rectTransform, source, target);
			sparks.Add(pooledObject);
			AudioManager.PlaySFX(SFXTypes.Snap);
		}
		if (mixCounter > minMix + maxMix)
		{
			mixComplete?.Invoke();
		}
	}
}
