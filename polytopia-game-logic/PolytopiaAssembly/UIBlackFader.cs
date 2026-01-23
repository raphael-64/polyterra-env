using System;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIBlackFader : MonoBehaviour, ISelectableContainer
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static TweenCallback _003C_003E9__15_1;

		internal void _003CFadeInOut_003Eb__15_1()
		{
			instance.parentCanvas.SetActive(false);
			instance.isShowing = false;
			Log.Verbose("Hiding loader", Array.Empty<object>());
		}
	}

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private TMPLocalizer text;

	[SerializeField]
	private UITextButton backButton;

	[SerializeField]
	private GameObject parentCanvas;

	public static UIBlackFader instance;

	protected Tween fadeTween;

	protected Sequence fadeSequence;

	private Action backButtonCallback;

	private bool isShowing;

	public Selectable DefaultSelectable
	{
		get
		{
			if (((Component)backButton).gameObject.activeSelf)
			{
				return (Selectable)(object)backButton.button;
			}
			return null;
		}
		set
		{
		}
	}

	public Selectable CurrentSelectable
	{
		get
		{
			return DefaultSelectable;
		}
		set
		{
		}
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad((Object)(object)parentCanvas);
		instance = this;
		instance.backButton.OnClicked += OnBackButtonClicked;
		parentCanvas.SetActive(false);
	}

	private void OnBackButtonClicked(int id, BaseEventData eventData)
	{
		if (backButtonCallback != null)
		{
			backButtonCallback();
			backButtonCallback = null;
		}
		backButton.ButtonEnabled = false;
	}

	public static bool IsShowing()
	{
		if ((Object)(object)instance != (Object)null)
		{
			return instance.isShowing;
		}
		return false;
	}

	public static void UpdateCanvasInteraction()
	{
		if (!((Object)(object)instance == (Object)null))
		{
			bool interactable = InputManager.IsEnabled(InputManager.InputType.Loading);
			instance.canvasGroup.interactable = interactable;
		}
	}

	public static void FadeIn(float time = 0.5f, Action completeCallback = null, string textKey = "gamesettings.creatingworld", Action backButtonCallback = null)
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		if (IsShowing())
		{
			throw new Exception("Loader is already showing");
		}
		Log.Verbose("Showing loader", Array.Empty<object>());
		instance.isShowing = true;
		InputManager.DisableInputDuringLoadingScreen();
		bool shouldShowButton = backButtonCallback != null;
		instance.backButtonCallback = backButtonCallback;
		TweenUtils.KillTween(instance.fadeTween);
		instance.parentCanvas.SetActive(true);
		((Component)instance.text).gameObject.SetActive(false);
		((Component)instance.backButton).gameObject.SetActive(false);
		instance.text.Key = textKey;
		instance.fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(instance.canvasGroup.DOFade(1f, time), (TweenCallback)delegate
		{
			((Component)instance.text).gameObject.SetActive(!string.IsNullOrEmpty(textKey));
			((Component)instance.backButton).gameObject.SetActive(shouldShowButton);
			if (shouldShowButton && Object.op_Implicit((Object)(object)EventSystem.current))
			{
				UINavigationManager.Select((Selectable)(object)instance.backButton.button);
			}
			instance.backButton.ButtonEnabled = true;
			completeCallback?.Invoke();
		});
	}

	public static void FadeOut(float time = 0.5f, Action completeCallback = null)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		if (!IsShowing())
		{
			throw new Exception("Trying to fade out loader when it's not showing");
		}
		SystemEvents.SafeAreaChanged(ScreenManager.GetSafeArea());
		TweenUtils.KillTween(instance.fadeTween);
		instance.fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(instance.canvasGroup.DOFade(0f, time), (TweenCallback)delegate
		{
			instance.parentCanvas.SetActive(false);
			((Component)instance.text).gameObject.SetActive(false);
			((Component)instance.text).gameObject.SetActive(false);
			instance.isShowing = false;
			InputManager.EnableInputDuringLoadingScreen();
			Log.Verbose("Hiding loader", Array.Empty<object>());
			completeCallback?.Invoke();
		});
	}

	public static void FadeInOut(float time = 0.5f, Action middleCallback = null)
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Expected O, but got Unknown
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Expected O, but got Unknown
		if (IsShowing())
		{
			throw new Exception("Loader is already showing");
		}
		instance.isShowing = true;
		Log.Verbose("Showing loader", Array.Empty<object>());
		TweenUtils.KillTween((Tween)(object)instance.fadeSequence);
		Log.Verbose($"UIBlackFader :: FadeInOut :: instance: {instance}", Array.Empty<object>());
		instance.parentCanvas.SetActive(true);
		((Component)instance.text).gameObject.SetActive(false);
		((Component)instance.backButton).gameObject.SetActive(false);
		instance.fadeSequence = DOTween.Sequence();
		TweenSettingsExtensions.Append(instance.fadeSequence, (Tween)(object)instance.canvasGroup.DOFade(1f, time * 0.5f));
		TweenSettingsExtensions.AppendCallback(instance.fadeSequence, (TweenCallback)delegate
		{
			middleCallback?.Invoke();
		});
		TweenSettingsExtensions.Append(instance.fadeSequence, (Tween)(object)instance.canvasGroup.DOFade(0f, time * 0.5f));
		Sequence obj = instance.fadeSequence;
		object obj2 = _003C_003Ec._003C_003E9__15_1;
		if (obj2 == null)
		{
			TweenCallback val = delegate
			{
				instance.parentCanvas.SetActive(false);
				instance.isShowing = false;
				Log.Verbose("Hiding loader", Array.Empty<object>());
			};
			_003C_003Ec._003C_003E9__15_1 = val;
			obj2 = (object)val;
		}
		TweenSettingsExtensions.AppendCallback(obj, (TweenCallback)obj2);
	}

	public Selectable GetDefaultSelectableOrFallback()
	{
		if (!UINavigationManager.IsValidSelectable(DefaultSelectable))
		{
			return null;
		}
		return DefaultSelectable;
	}

	public Selectable GetCurrentSelectableOrFallback()
	{
		if (!UINavigationManager.IsValidSelectable(CurrentSelectable))
		{
			return GetDefaultSelectableOrFallback();
		}
		return CurrentSelectable;
	}
}
