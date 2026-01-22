using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopAlert : UIBasicComponent
{
	private bool showTimer;

	[SerializeField]
	protected TextMeshProUGUI messageField;

	[SerializeField]
	protected TMPLocalizer messageFieldLocalizer;

	[SerializeField]
	protected Image background;

	[SerializeField]
	protected bool pulseBackground;

	[SerializeField]
	protected float startAlpha = 1f;

	private string message;

	private Tween tween;

	private Tween pulseTween;

	private Action onHiddenCallback;

	public bool ShowTimer
	{
		get
		{
			return showTimer;
		}
		set
		{
			if (GameVersionUtils.HideEsport)
			{
				showTimer = false;
			}
			else
			{
				showTimer = value;
			}
		}
	}

	public string Message
	{
		get
		{
			return messageFieldLocalizer.Text;
		}
		set
		{
			message = value;
			UpdateMessageFieldLocalizer();
		}
	}

	public string messageKey
	{
		get
		{
			return messageFieldLocalizer.Key;
		}
		set
		{
			messageFieldLocalizer.Key = value;
		}
	}

	public bool Showing => ((Component)this).gameObject.activeSelf;

	private void OnDestroy()
	{
		Tween obj = tween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		tween = null;
		Tween obj2 = pulseTween;
		if (obj2 != null)
		{
			TweenExtensions.Kill(obj2, false);
		}
		pulseTween = null;
	}

	public void Update()
	{
		if (ShowTimer)
		{
			UpdateMessageFieldLocalizer();
		}
	}

	public void Show(bool instant = false)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		UpdateMessageFieldLocalizer();
		if (instant || ((Component)this).gameObject.activeSelf || !((Component)this).gameObject.activeInHierarchy)
		{
			Tween obj = tween;
			if (obj != null)
			{
				TweenExtensions.Complete(obj);
			}
			((Component)this).gameObject.SetActive(true);
			((Transform)((Graphic)background).rectTransform).localScale = new Vector3(1f, 1f, 1f);
			OnShowComplete();
		}
		else if (tween != null && TweenExtensions.IsActive(tween) && TweenExtensions.IsPlaying(tween))
		{
			TweenSettingsExtensions.OnComplete<Tween>(tween, (TweenCallback)delegate
			{
				OnHideComplete();
				Show(instant);
			});
		}
		else
		{
			((Component)this).gameObject.SetActive(true);
			tween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(DOTween.To((DOGetter<Vector3>)(() => ((Transform)((Graphic)background).rectTransform).localScale), (DOSetter<Vector3>)delegate(Vector3 v)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				((Transform)((Graphic)background).rectTransform).localScale = v;
			}, new Vector3(1f, 1f, 1f), 0.1f), new TweenCallback(OnShowComplete));
		}
	}

	public void Hide(bool instant = false, Action callback = null)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		SetTextAlpha(0f);
		onHiddenCallback = callback;
		if (instant || !((Component)this).gameObject.activeInHierarchy)
		{
			Tween obj = tween;
			if (obj != null)
			{
				TweenExtensions.Complete(obj);
			}
			OnHideComplete();
		}
		else if (tween != null && TweenExtensions.IsActive(tween) && TweenExtensions.IsPlaying(tween))
		{
			TweenSettingsExtensions.OnComplete<Tween>(tween, (TweenCallback)delegate
			{
				OnShowComplete();
				Hide(instant);
			});
		}
		else
		{
			tween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(DOTween.To((DOGetter<Vector3>)(() => ((Transform)((Graphic)background).rectTransform).localScale), (DOSetter<Vector3>)delegate(Vector3 v)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				((Transform)((Graphic)background).rectTransform).localScale = v;
			}, new Vector3(1f, 0f, 1f), 0.1f), new TweenCallback(OnHideComplete));
		}
	}

	private void OnShowComplete()
	{
		SetTextAlpha(1f);
		if (pulseBackground && pulseTween == null)
		{
			pulseTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(TweenSettingsExtensions.SetLoops<TweenerCore<Color, Color, ColorOptions>>(background.DOFade(0.4f, 0.8f), -1, (LoopType)1), (Ease)7);
		}
	}

	private void OnHideComplete()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (pulseTween != null)
		{
			TweenExtensions.Kill(pulseTween, false);
			pulseTween = null;
		}
		((Graphic)background).color = ColorUtil.SetAlphaOnColor(((Graphic)background).color, startAlpha);
		((Component)this).gameObject.SetActive(false);
		((Transform)((Graphic)background).rectTransform).localScale = new Vector3(1f, 0f, 1f);
		onHiddenCallback?.Invoke();
		onHiddenCallback = null;
	}

	public void SetBackgroundColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)background).color = color;
	}

	public void SetTextAlpha(float alpha)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((Graphic)messageField).color;
		color.a = alpha;
		((Graphic)messageField).color = color;
	}

	private void UpdateMessageFieldLocalizer()
	{
		if (!ShowTimer || !GameManager.Client.CurrentGameId.HasValue)
		{
			messageFieldLocalizer.Text = message;
		}
		else if (!GameVersionUtils.HideEsport)
		{
			TimeSpan timeSpan = GameManager.GetRemoteGameDataManager().GetGameTimeLeftForCurrentPlayer(GameManager.Client.CurrentGameId.Value) ?? TimeSpan.Zero;
			if (timeSpan.TotalSeconds < 0.0)
			{
				timeSpan = TimeSpan.Zero;
			}
			string arg = TurnTimerContainer.FormatTimeSpanToString(timeSpan);
			string text = $"{message} ({arg})";
			messageFieldLocalizer.Text = text;
			((TMP_Text)messageFieldLocalizer.TextComponent).ForceMeshUpdate(false, false);
		}
	}
}
