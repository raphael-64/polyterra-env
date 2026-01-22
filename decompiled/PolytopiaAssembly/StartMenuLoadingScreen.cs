using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuLoadingScreen : UIScreenBase
{
	[Header("Loading Screen")]
	[SerializeField]
	protected Image bg;

	[SerializeField]
	protected Color[] colorCycle;

	[SerializeField]
	protected float cycleTime;

	protected int cycleCount;

	protected Coroutine colorChangeDelay;

	private void OnEnable()
	{
		Init();
		if (StartupManager.IsReady)
		{
			OnStartupDone();
		}
		else
		{
			GameEvents.OnStartupDone += OnStartupDone;
		}
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)bg).color = colorCycle[cycleCount];
		colorChangeDelay = ((MonoBehaviour)this).StartCoroutine(DelayColorChange());
	}

	private IEnumerator DelayColorChange()
	{
		yield return (object)new WaitForSeconds(cycleTime);
		cycleCount++;
		if (cycleCount >= colorCycle.Length)
		{
			cycleCount = 0;
		}
		((Graphic)bg).color = colorCycle[cycleCount];
		colorChangeDelay = ((MonoBehaviour)this).StartCoroutine(DelayColorChange());
	}

	private void OnStartupDone()
	{
		GameEvents.OnStartupDone -= OnStartupDone;
		if (colorChangeDelay != null)
		{
			((MonoBehaviour)this).StopCoroutine(colorChangeDelay);
			colorChangeDelay = null;
		}
		UIManager.Instance.PopCurrentScreen();
		UIManager.Instance.ShowScreen(UIConstants.Screens.StartScreen);
	}

	public void StopCycle()
	{
		if (colorChangeDelay != null)
		{
			((MonoBehaviour)this).StopCoroutine(colorChangeDelay);
			colorChangeDelay = null;
		}
	}

	public void RestartCycle()
	{
		if (colorChangeDelay == null)
		{
			colorChangeDelay = ((MonoBehaviour)this).StartCoroutine(DelayColorChange());
		}
	}
}
