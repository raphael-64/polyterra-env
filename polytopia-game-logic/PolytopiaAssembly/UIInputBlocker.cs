using NaughtyAttributes;
using UnityEngine;

public class UIInputBlocker : UIBasicComponent
{
	protected int count;

	protected static UIInputBlocker instance;

	[SerializeField]
	[Info]
	private string info;

	public override void Init()
	{
		base.Init();
		instance = this;
		InternalResetBlockerCount();
		info = $"Count: {count}";
	}

	private void OnDestroy()
	{
		instance = null;
	}

	[Button("Increase Spinner Count")]
	protected void InternalIncreasBlockerCount()
	{
		count++;
		((Component)this).gameObject.SetActive(true);
		info = $"Count: {count}";
	}

	[Button("Decrease Spinner Count")]
	protected void InternalDecreaseBlockerCount()
	{
		count--;
		if (count <= 0)
		{
			InternalResetBlockerCount();
		}
		info = $"Count: {count}";
	}

	[Button("Reset Spinner Count")]
	protected void InternalResetBlockerCount()
	{
		count = 0;
		((Component)this).gameObject.SetActive(false);
		info = $"Count: {count}";
	}

	public static void IncreaseBlockerCount()
	{
		if ((Object)(object)instance != (Object)null)
		{
			instance.InternalIncreasBlockerCount();
		}
	}

	public static void DecreaseBlockerCount()
	{
		if ((Object)(object)instance != (Object)null)
		{
			instance.InternalDecreaseBlockerCount();
		}
	}

	public static void ResetBlockerrCount()
	{
		if ((Object)(object)instance != (Object)null)
		{
			instance.InternalResetBlockerCount();
		}
	}
}
