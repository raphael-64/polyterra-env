using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICanvasScalerHelper : MonoBehaviour
{
	public UICanvasScaleData data;

	[Info]
	public string info = "";

	[SerializeField]
	private CanvasScaler canvasScaler;

	private static float invertedUIScale = 1f;

	private static float uiScale = 1f;

	private static float calculatedInvertedUIScale = 1f;

	private static float calculatedUIScale = 1f;

	private static bool isBigScreen;

	protected bool isDirty = true;

	public void Initialize()
	{
		CalculateUIScale();
	}

	private void OnEnable()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
		SystemEvents.OnSafeAreaChanged += OnSafeAreaChanged;
		DebugConsole.AddCommand("ui_globalscale", new CommandDelegate(CmdGlobalScale), "Global scale multiplier for the UI");
		CalculateUIScale();
	}

	private void OnDisable()
	{
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
		SystemEvents.OnSafeAreaChanged -= OnSafeAreaChanged;
		DebugConsole.RemoveCommand("ui_globalscale");
	}

	private void Update()
	{
		if (isDirty)
		{
			CalculateUIScale();
		}
	}

	private void OnScreenSizeChanged(Vector2 screenSize)
	{
		SetDirty();
	}

	private void OnSafeAreaChanged(Rect safeArea)
	{
		SetDirty();
	}

	[Button("Refresh")]
	private void CalculateUIScale()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = NativeHelpers.Screen();
		int x = ((Vector2Int)(ref val)).x;
		val = NativeHelpers.Screen();
		float num = (float)Mathf.Min(x, ((Vector2Int)(ref val)).y) / ScalingUtils.GetDPI();
		float num2 = ((SettingsUtils.ScaleLimit >= 0f) ? SettingsUtils.ScaleLimit : num);
		isBigScreen = GetIndexForLimit(num2) > 0;
		uiScale = ScalingUtils.GetDPIRatio() * GetScreenLimitMultiplier(num2) * data.globalUIScaleMultiplier;
		invertedUIScale = 1f / uiScale;
		calculatedUIScale = ScalingUtils.GetDPIRatio() * GetScreenLimitMultiplier(num) * data.globalUIScaleMultiplier;
		calculatedInvertedUIScale = 1f / calculatedUIScale;
		if ((Object)(object)EventSystem.current != (Object)null)
		{
			EventSystem.current.pixelDragThreshold = Mathf.RoundToInt(GetMinimumScrollThreshold());
		}
		bool num3 = uiScale != canvasScaler.scaleFactor;
		canvasScaler.scaleFactor = uiScale;
		if (num3)
		{
			val = NativeHelpers.Screen();
			float num4 = ((Vector2Int)(ref val)).x;
			val = NativeHelpers.Screen();
			SystemEvents.ScreenSizeChanged(new Vector2(num4, (float)((Vector2Int)(ref val)).y));
		}
		isDirty = false;
	}

	public void SetDirty()
	{
		isDirty = true;
	}

	public static float GetInvertedUIScale()
	{
		return invertedUIScale;
	}

	public static float GetUIScale()
	{
		return uiScale;
	}

	public static float GetMinimumScrollThreshold()
	{
		return Config.dragThreshold.FloatValue * calculatedUIScale;
	}

	public ScreenLimitData[] GetScreenLimitData()
	{
		return data.screenLimits;
	}

	public static bool IsBigScreen()
	{
		return isBigScreen;
	}

	public int GetIndexForLimit(float limit)
	{
		if (limit < 0f)
		{
			return -1;
		}
		int result = -1;
		for (int i = 0; i < data.screenLimits.Length; i++)
		{
			if (limit >= data.screenLimits[i].limit)
			{
				result = i;
			}
		}
		return result;
	}

	private float GetScreenLimitMultiplier(float minSize)
	{
		int indexForLimit = GetIndexForLimit(minSize);
		if (indexForLimit < 0)
		{
			return 1f;
		}
		return data.screenLimits[indexForLimit].scaleMultiplier;
	}

	private void CmdGlobalScale(string[] args)
	{
		if (float.TryParse(args[0], out var result))
		{
			data.globalUIScaleMultiplier = result;
			CalculateUIScale();
		}
	}
}
