using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

public class CenterNotification : UIBasicComponent
{
	[SerializeField]
	protected float showTime = 2f;

	[SerializeField]
	protected CanvasGroup cnvsGrp;

	[SerializeField]
	protected TextMeshProUGUI header;

	[SerializeField]
	protected TextMeshProUGUI message;

	[HideInInspector]
	public Action hideCallback;

	protected bool showing;

	public string Header
	{
		set
		{
			((TMP_Text)header).text = value;
		}
	}

	public string Message
	{
		set
		{
			((Component)message).gameObject.SetActive(!string.IsNullOrEmpty(value));
			((TMP_Text)message).text = value;
		}
	}

	public bool Showing
	{
		get
		{
			return showing;
		}
		protected set
		{
			showing = value;
		}
	}

	public override void Init()
	{
		base.Init();
		((Component)this).gameObject.SetActive(false);
	}

	public void Show(float hideDelay = -1f)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		if (hideDelay <= 0f)
		{
			hideDelay = showTime;
		}
		Showing = true;
		((Component)this).gameObject.SetActive(true);
		cnvsGrp.alpha = 0f;
		cnvsGrp.DOFade(1f, 0.1f);
		TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<float, float, FloatOptions>>(cnvsGrp.DOFade(0f, 0.6f), hideDelay), (TweenCallback)delegate
		{
			((Component)this).gameObject.SetActive(false);
			Showing = false;
			hideCallback?.Invoke();
		});
	}
}
