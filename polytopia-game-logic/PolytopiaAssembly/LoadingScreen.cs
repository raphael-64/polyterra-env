using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class LoadingScreen : UIScreenBase
{
	public CanvasGroup cnvsGrp;

	public float minShowTime = 2f;

	private bool interupted;

	private float showTime;

	private Tween tween;

	public override void Show(bool instant = false)
	{
		base.Show(instant);
		showTime = Time.time;
		GameEvents.OnMapLoaded -= OnMapLoaded;
		GameEvents.OnMapLoaded += OnMapLoaded;
	}

	protected void OnDestroy()
	{
		GameEvents.OnMapLoaded -= OnMapLoaded;
	}

	public override void ShowScreen(UIConstants.Screens screen, bool instant = false)
	{
		base.ShowScreen(screen, instant);
		if (screen != screenType)
		{
			interupted = true;
		}
	}

	private void OnMapLoaded()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		float num = 0f;
		float num2 = Time.time - showTime;
		if (num2 < minShowTime)
		{
			num = minShowTime - num2;
		}
		Log.Verbose("LoadingScreen :: delay: {0}", new object[1] { num });
		if (tween == null)
		{
			tween = (Tween)(object)TweenSettingsExtensions.SetDelay<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(cnvsGrp.DOFade(0f, 1f), new TweenCallback(FadeComplete)), num);
		}
	}

	public override void Hide(bool instant = false)
	{
		if (tween == null)
		{
			base.Hide();
		}
	}

	private void FadeComplete()
	{
		if (!interupted)
		{
			base.Hide();
		}
		else
		{
			showState = ShowStates.None;
			base.Hide();
		}
		tween = null;
		interupted = false;
		UIEvents.LoadingScreenHidden();
	}
}
