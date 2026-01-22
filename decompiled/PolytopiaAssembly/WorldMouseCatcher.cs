using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldMouseCatcher : MonoBehaviour
{
	[SerializeField]
	protected Camera cam;

	[SerializeField]
	protected float scaleMultiplier = 1f;

	protected static WorldMouseCatcher instance;

	protected Bounds cameraBounds;

	protected Coroutine sunriseWaiter;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		RefreshLayout();
	}

	private void OnDestroy()
	{
		instance = null;
	}

	private void OnEnable()
	{
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
	}

	private void OnDisable()
	{
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
	}

	protected void OnScreenSizeChanged(Vector2 screenSize)
	{
		RefreshLayout();
	}

	protected void RefreshLayout()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		cameraBounds = cam.OrthographicBounds();
		((Component)this).transform.localScale = new Vector3(((Bounds)(ref cameraBounds)).size.x * scaleMultiplier, ((Bounds)(ref cameraBounds)).size.y * scaleMultiplier, 1f);
	}

	public static void StartSunriseTimer()
	{
		instance.StartSunriseTimerInternal();
	}

	private void StartSunriseTimerInternal()
	{
		KillWaiter();
		if (InputManager.IsEnabled(InputManager.InputType.Map) && !EventSystem.current.IsPointerOverGameObject())
		{
			sunriseWaiter = ((MonoBehaviour)this).StartCoroutine(DelaySunrise());
		}
	}

	public static void StopSunriseTimer()
	{
		instance.StopSunriseTimerInternal();
	}

	private void StopSunriseTimerInternal()
	{
		if (sunriseWaiter != null)
		{
			KillWaiter();
		}
	}

	private IEnumerator DelaySunrise()
	{
		yield return (object)new WaitForSeconds(SunriseBg.SUNRISE_TIME);
		Log.Verbose("WorldMouseCatcher :: Toggle Sunrise", Array.Empty<object>());
		SunriseBg.ToggleVisibility();
	}

	private void KillWaiter()
	{
		if (sunriseWaiter != null)
		{
			((MonoBehaviour)this).StopCoroutine(sunriseWaiter);
		}
		sunriseWaiter = null;
	}

	public static void Refresh()
	{
		if (Object.op_Implicit((Object)(object)instance) && ((Component)instance).gameObject.activeSelf)
		{
			instance.RefreshLayout();
		}
	}
}
