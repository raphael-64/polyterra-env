using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using NaughtyAttributes;
using UnityEngine;

public class UISpinner : UIBasicComponent
{
	[SerializeField]
	protected float showDelay = 1f;

	[SerializeField]
	private GameObject spinnerComponent;

	[SerializeField]
	private CanvasGroup spinnerCnvsGroup;

	[SerializeField]
	[Info]
	private string info;

	protected static UISpinner instance;

	protected int count;

	protected Coroutine showWaiter;

	protected Tween fadeTween;

	public override void Init()
	{
		base.Init();
		instance = this;
		spinnerCnvsGroup.alpha = 0f;
		InternalResetSpinnerCount();
		info = $"Count: {count}";
	}

	private void OnDestroy()
	{
		instance = null;
		TweenUtils.KillTween(fadeTween);
		if (showWaiter != null)
		{
			((MonoBehaviour)this).StopCoroutine(showWaiter);
			showWaiter = null;
		}
	}

	[Button("Increase Spinner Count")]
	protected void InternalIncreasSpinnerCount()
	{
		count++;
		if (showWaiter == null)
		{
			showWaiter = ((MonoBehaviour)this).StartCoroutine(DelayShowSpinner());
		}
		info = $"Count: {count}";
	}

	[Button("Decrease Spinner Count")]
	protected void InternalDecreaseSpinnerCount()
	{
		count--;
		if (count <= 0)
		{
			InternalResetSpinnerCount();
		}
		info = $"Count: {count}";
	}

	[Button("Reset Spinner Count")]
	protected void InternalResetSpinnerCount()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		count = 0;
		if (showWaiter != null)
		{
			((MonoBehaviour)this).StopCoroutine(showWaiter);
			showWaiter = null;
		}
		TweenUtils.KillTween(fadeTween);
		fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(spinnerCnvsGroup.DOFade(0f, 0.2f), (TweenCallback)delegate
		{
			spinnerComponent.SetActive(false);
		});
		info = $"Count: {count}";
	}

	protected IEnumerator DelayShowSpinner()
	{
		yield return (object)new WaitForSeconds(showDelay);
		spinnerComponent.SetActive(true);
		TweenUtils.KillTween(fadeTween);
		fadeTween = (Tween)(object)spinnerCnvsGroup.DOFade(1f, 0.2f);
	}

	public static void IncreaseSpinnerCount()
	{
		if ((Object)(object)instance != (Object)null)
		{
			instance.InternalIncreasSpinnerCount();
		}
	}

	public static void DecreaseSpinnerCount()
	{
		if ((Object)(object)instance != (Object)null)
		{
			instance.InternalDecreaseSpinnerCount();
		}
	}

	public static void ResetSpinnerCount()
	{
		if ((Object)(object)instance != (Object)null)
		{
			instance.InternalResetSpinnerCount();
		}
	}
}
