using UnityEngine;

public class ScreenManager : MonoBehaviour
{
	[Info]
	[SerializeField]
	protected string info = "No Info";

	protected Vector2 lastScreenSize;

	protected Rect lastSafeArea;

	protected FullScreenMode lastFullScreenMode;

	protected static float screenSize;

	protected static bool isBigScreen;

	private const string PREFS_LAST_SCREEN_RATIO = "LastScreenRatio";

	private bool _fullScreen;

	private int manualScreenWidth = 1920;

	public static float SafeLeft
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (UIManager.Exists)
			{
				Rect safeArea = GetSafeArea();
				Vector2Int val = NativeHelpers.Screen();
				return (float)((Vector2Int)(ref val)).x * (((Rect)(ref safeArea)).position.x / (float)Screen.width);
			}
			return 0f;
		}
	}

	public static float SafeTop
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (UIManager.Exists)
			{
				Rect safeArea = GetSafeArea();
				Vector2Int val = NativeHelpers.Screen();
				return (float)((Vector2Int)(ref val)).y * (1f - (((Rect)(ref safeArea)).position.y + ((Rect)(ref safeArea)).size.y) / (float)Screen.height);
			}
			return 0f;
		}
	}

	public static float SafeRight
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (UIManager.Exists)
			{
				Rect safeArea = GetSafeArea();
				Vector2Int val = NativeHelpers.Screen();
				return (float)((Vector2Int)(ref val)).x * (1f - (((Rect)(ref safeArea)).position.x + ((Rect)(ref safeArea)).size.x) / (float)Screen.width);
			}
			return 0f;
		}
	}

	public static float SafeBottom
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (UIManager.Exists)
			{
				Rect safeArea = GetSafeArea();
				Vector2Int val = NativeHelpers.Screen();
				return (float)((Vector2Int)(ref val)).y * (((Rect)(ref safeArea)).position.y / (float)Screen.height);
			}
			return 0f;
		}
	}

	public static float SafeHeight
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (UIManager.Exists)
			{
				Rect safeArea = GetSafeArea();
				Vector2Int val = NativeHelpers.Screen();
				return (float)((Vector2Int)(ref val)).y * (((Rect)(ref safeArea)).size.y / (float)Screen.height);
			}
			return 0f;
		}
	}

	public static float SafeWidth
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (UIManager.Exists)
			{
				Rect safeArea = GetSafeArea();
				Vector2Int val = NativeHelpers.Screen();
				return (float)((Vector2Int)(ref val)).x * (((Rect)(ref safeArea)).size.x / (float)Screen.width);
			}
			return 0f;
		}
	}

	public static bool IsBigScreen => isBigScreen;

	private void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		UpdateLastSafeArea();
		UpdateLastScreenSize();
		_fullScreen = Screen.fullScreen;
		Vector2Int val = NativeHelpers.Screen();
		manualScreenWidth = ((Vector2Int)(ref val)).x;
	}

	private void Update()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (_fullScreen != Screen.fullScreen)
		{
			if (_fullScreen)
			{
				int num = manualScreenWidth;
				float num2 = 1f * (float)manualScreenWidth;
				Resolution currentResolution = Screen.currentResolution;
				float num3 = num2 * (float)((Resolution)(ref currentResolution)).height;
				currentResolution = Screen.currentResolution;
				Screen.SetResolution(num, Mathf.RoundToInt(num3 / (float)((Resolution)(ref currentResolution)).width), Screen.fullScreen);
			}
			else
			{
				Screen.SetResolution(Display.displays[Camera.main.targetDisplay].systemWidth, Display.displays[Camera.main.targetDisplay].systemHeight, Screen.fullScreen);
			}
			_fullScreen = Screen.fullScreen;
		}
		if (Screen.fullScreenMode != lastFullScreenMode)
		{
			lastFullScreenMode = Screen.fullScreenMode;
			ScalingUtils.SetDirty();
			SystemEvents.FullscreenChanged(lastFullScreenMode);
		}
		if (GetSafeArea() != lastSafeArea)
		{
			UpdateLastSafeArea();
			SystemEvents.SafeAreaChanged(lastSafeArea);
		}
		Vector2Int val = NativeHelpers.Screen();
		if ((float)((Vector2Int)(ref val)).x == lastScreenSize.x)
		{
			val = NativeHelpers.Screen();
			if ((float)((Vector2Int)(ref val)).y == lastScreenSize.y)
			{
				return;
			}
		}
		UpdateLastScreenSize();
		ScalingUtils.SetDirty();
		SystemEvents.ScreenSizeChanged(lastScreenSize);
	}

	private void UpdateLastScreenSize()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		lastScreenSize = Vector2Int.op_Implicit(NativeHelpers.Screen());
		if (!_fullScreen)
		{
			Vector2Int val = NativeHelpers.Screen();
			manualScreenWidth = ((Vector2Int)(ref val)).x;
		}
	}

	private void UpdateLastSafeArea()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		lastSafeArea = GetSafeArea();
	}

	public static Rect GetSafeArea()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Screen.safeArea;
	}

	public static float GetScreenWidth()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = NativeHelpers.Screen();
		return ((Vector2Int)(ref val)).x;
	}

	public static float GetScreenHeight()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = NativeHelpers.Screen();
		return ((Vector2Int)(ref val)).y;
	}

	protected void RefreshInfo()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		info = $"safeArea: {GetSafeArea()}\nSafeLeft: {SafeLeft}\nSafeTop: {SafeTop}\nSafeRight: {SafeRight}\nSafeBottom: {SafeBottom}\nSafeHeight: {SafeHeight}\nSafeWidth: {SafeWidth}";
	}
}
