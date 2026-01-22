using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UDP.Common.MiniJSON;

namespace Network.BugReport;

public class BugReport : MonoBehaviour
{
	private class LogEntry
	{
		public string Message;

		public string StackTrace;

		public LogType LogType;

		public int Count;

		public LogEntry(string message, string stackTrace, LogType logType)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			Message = message;
			StackTrace = stackTrace;
			LogType = logType;
			Count = 1;
		}
	}

	private object _ThreadLock;

	private List<LogEntry> _StashedLogs;

	private Vector4 _TopRightCornerMargin;

	private int _PressCounter;

	private float _Timer;

	private Vector2? _PointerPos;

	private bool _ResetTouch;

	private void Awake()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		_ThreadLock = new object();
		_StashedLogs = new List<LogEntry>();
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		Application.logMessageReceivedThreaded += new LogCallback(OnLogReceived);
	}

	private void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_TopRightCornerMargin = default(Vector4);
		int width = Screen.width;
		int height = Screen.height;
		_TopRightCornerMargin.x = (float)width - (float)width * 0.1f;
		_TopRightCornerMargin.y = width;
		_TopRightCornerMargin.z = (float)height - (float)height * 0.1f;
		_TopRightCornerMargin.w = height;
	}

	private void Update()
	{
		GetPointerPos();
		ReduceTimer();
		CountPress();
		_PointerPos = null;
	}

	private void OnApplicationQuit()
	{
		BuildLogFile(asCurrentSession: false);
	}

	private void OnLogReceived(string message, string stacktrace, LogType type)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		lock (_ThreadLock)
		{
			if (_StashedLogs.Count > 0)
			{
				LogEntry logEntry = _StashedLogs[_StashedLogs.Count - 1];
				if (logEntry.LogType == type && logEntry.Message == message && logEntry.StackTrace == stacktrace)
				{
					logEntry.Count++;
					return;
				}
			}
			_StashedLogs.Add(new LogEntry(message, stacktrace, type));
		}
	}

	private void GetPointerPos()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetMouseButtonDown(0))
		{
			_PointerPos = Vector2.op_Implicit(Input.mousePosition);
		}
		if (Input.touchCount != 0 && !_ResetTouch)
		{
			_PointerPos = ((Touch)(ref Input.touches[0])).position;
			_ResetTouch = true;
		}
		if (Input.touchCount == 0)
		{
			_ResetTouch = false;
		}
	}

	private void ReduceTimer()
	{
		if (_Timer <= 0f)
		{
			return;
		}
		_Timer -= Time.deltaTime;
		if (!(_Timer > 0f))
		{
			if (_PressCounter == 5)
			{
				BuildLogFile(asCurrentSession: true);
				SendReport(currentSession: true);
			}
			else if (_PressCounter > 5)
			{
				SendReport(currentSession: false);
			}
			_PressCounter = 0;
		}
	}

	private bool TestPointerPosition(Vector2 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (pos.x < _TopRightCornerMargin.x || pos.x > _TopRightCornerMargin.y)
		{
			return false;
		}
		if (pos.y < _TopRightCornerMargin.z || pos.y > _TopRightCornerMargin.w)
		{
			return false;
		}
		return true;
	}

	private void CountPress()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (_PointerPos.HasValue && TestPointerPosition(_PointerPos.Value))
		{
			if (_Timer <= 0f)
			{
				_Timer = 2f;
				_PressCounter = 1;
			}
			else
			{
				_PressCounter++;
			}
		}
	}

	private void BuildLogFile(bool asCurrentSession)
	{
		string text = "";
		try
		{
			Hashtable hashtable = new Hashtable();
			AddPolytopiaInfo(hashtable);
			AddSystemInfo(hashtable);
			AddBatteryInfo(hashtable);
			AddUnityInfo(hashtable);
			AddDisplayInfo(hashtable);
			AddRuntimeInfo(hashtable);
			AddFeaturesInfo(hashtable);
			AddIOSInfo(hashtable);
			AddGraphicsInfo(hashtable);
			AddConsoleLogs(hashtable);
			text = Json.Serialize((object)hashtable);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
			return;
		}
		string text2 = (asCurrentSession ? "current" : "past");
		string path = "Logs_" + text2 + ".txt";
		path = Path.Combine(Application.persistentDataPath, path);
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		using StreamWriter streamWriter = new StreamWriter(path);
		streamWriter.Write(text);
		Debug.Log((object)("Logs saved on path " + path + "."));
	}

	private void SendReport(bool currentSession)
	{
	}

	private void AddPolytopiaInfo(Hashtable hashtable)
	{
		Hashtable hashtable2 = new Hashtable();
		hashtable2.Add("Semantic Version", VersionManager.SemanticVersion.ToString());
		hashtable2.Add("Game Version", VersionManager.GameVersion);
		hashtable2.Add("Game Logic Data Version", VersionManager.GameLogicDataVersion);
		hashtable.Add("Polytopia", hashtable2);
	}

	private void AddSystemInfo(Hashtable hashtable)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Hashtable hashtable2 = new Hashtable();
		hashtable2.Add("Utc Time", DateTime.UtcNow);
		hashtable2.Add("Operating System", SystemInfo.operatingSystem);
		hashtable2.Add("Device Type", SystemInfo.deviceType);
		hashtable2.Add("Device Model", SystemInfo.deviceModel);
		hashtable2.Add("CPU Type", SystemInfo.processorType);
		hashtable2.Add("CPU Count", SystemInfo.processorCount);
		hashtable2.Add("System Memory", GetBytesReadable((long)SystemInfo.systemMemorySize * 1024L * 1024));
		hashtable.Add("System", hashtable2);
	}

	private void AddBatteryInfo(Hashtable hashtable)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((int)SystemInfo.batteryStatus != 0)
		{
			Hashtable hashtable2 = new Hashtable();
			hashtable2.Add("Status", SystemInfo.batteryStatus);
			hashtable2.Add("Battery Level", SystemInfo.batteryLevel);
			hashtable.Add("Battery", hashtable2);
		}
	}

	private void AddUnityInfo(Hashtable hashtable)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		Hashtable hashtable2 = new Hashtable();
		hashtable2.Add("Version", Application.unityVersion);
		hashtable2.Add("Debug", Debug.isDebugBuild);
		hashtable2.Add("Unity Pro", Application.HasProLicense());
		hashtable2.Add("Genuine", (Application.genuine ? "Yes" : "No") + " (" + (Application.genuineCheckAvailable ? "Trusted" : "Untrusted") + ")");
		hashtable2.Add("System Language", Application.systemLanguage);
		hashtable2.Add("Platform", Application.platform);
		hashtable2.Add("Install Mode", Application.installMode);
		hashtable2.Add("Sandbox", Application.sandboxType);
		hashtable2.Add("IL2CPP", "No");
		hashtable2.Add("Application Version", Application.version);
		hashtable.Add("Unity", hashtable2);
	}

	private void AddDisplayInfo(Hashtable hashtable)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Hashtable hashtable2 = new Hashtable();
		hashtable2.Add("Resolution", Screen.width + "x" + Screen.height);
		hashtable2.Add("DPI", Screen.dpi);
		hashtable2.Add("Fullscreen", Screen.fullScreen);
		hashtable2.Add("Fullscreen Mode", Screen.fullScreenMode);
		hashtable2.Add("Orientation", Screen.orientation);
		hashtable.Add("Display", hashtable2);
	}

	private void AddRuntimeInfo(Hashtable hashtable)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		Hashtable hashtable2 = new Hashtable();
		hashtable2.Add("Play Time", Time.unscaledTime);
		hashtable2.Add("Level Play Time", Time.timeSinceLevelLoad);
		hashtable2.Add("Current Level", $"{((Scene)(ref activeScene)).name} (Index: {((Scene)(ref activeScene)).buildIndex})");
		hashtable2.Add("Quality Level", QualitySettings.names[QualitySettings.GetQualityLevel()] + " (" + QualitySettings.GetQualityLevel() + ")");
		hashtable.Add("Runtime", hashtable2);
	}

	private void AddFeaturesInfo(Hashtable hashtable)
	{
		Hashtable hashtable2 = new Hashtable();
		hashtable2.Add("Location", SystemInfo.supportsLocationService);
		hashtable2.Add("Accelerometer", SystemInfo.supportsAccelerometer);
		hashtable2.Add("Gyroscope", SystemInfo.supportsGyroscope);
		hashtable2.Add("Vibration", SystemInfo.supportsVibration);
		hashtable2.Add("Audio", SystemInfo.supportsAudio);
		hashtable.Add("Features", hashtable2);
	}

	private void AddIOSInfo(Hashtable hashtable)
	{
	}

	private void AddGraphicsInfo(Hashtable hashtable)
	{
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		Hashtable hashtable2 = new Hashtable();
		hashtable2.Add("Device Name", SystemInfo.graphicsDeviceName);
		hashtable2.Add("Device Vendor", SystemInfo.graphicsDeviceVendor);
		hashtable2.Add("Device Version", SystemInfo.graphicsDeviceVersion);
		hashtable2.Add("Graphics Memory", GetBytesReadable((long)SystemInfo.graphicsMemorySize * 1024L * 1024));
		hashtable2.Add("Max Tex Size", SystemInfo.maxTextureSize);
		hashtable.Add("Graphics - Device", hashtable2);
		Hashtable hashtable3 = new Hashtable();
		hashtable3.Add("UV Starts at top", SystemInfo.graphicsUVStartsAtTop);
		hashtable3.Add("Shader Level", SystemInfo.graphicsShaderLevel);
		hashtable3.Add("Multi Threaded", SystemInfo.graphicsMultiThreaded);
		hashtable3.Add("Hidden Service Removal (GPU)", SystemInfo.hasHiddenSurfaceRemovalOnGPU);
		hashtable3.Add("Uniform Array Indexing (Fragment Shaders)", SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders);
		hashtable3.Add("Shadows", SystemInfo.supportsShadows);
		hashtable3.Add("Raw Depth Sampling (Shadows)", SystemInfo.supportsRawShadowDepthSampling);
		hashtable3.Add("Motion Vectors", SystemInfo.supportsMotionVectors);
		hashtable3.Add("3D Textures", SystemInfo.supports3DTextures);
		hashtable3.Add("2D Array Textures", SystemInfo.supports2DArrayTextures);
		hashtable3.Add("3D Render Textures", SystemInfo.supports3DRenderTextures);
		hashtable3.Add("Cubemap Array Textures", SystemInfo.supportsCubemapArrayTextures);
		hashtable3.Add("Copy Texture Support", SystemInfo.copyTextureSupport);
		hashtable3.Add("Compute Shaders", SystemInfo.supportsComputeShaders);
		hashtable3.Add("Instancing", SystemInfo.supportsInstancing);
		hashtable3.Add("Hardware Quad Topology", SystemInfo.supportsHardwareQuadTopology);
		hashtable3.Add("32-bit index buffer", SystemInfo.supports32bitsIndexBuffer);
		hashtable3.Add("Sparse Textures", SystemInfo.supportsSparseTextures);
		hashtable3.Add("Render Target Count", SystemInfo.supportedRenderTargetCount);
		hashtable3.Add("Separated Render Targets Blend", SystemInfo.supportsSeparatedRenderTargetsBlend);
		hashtable3.Add("Multisampled Textures", SystemInfo.supportsMultisampledTextures);
		hashtable3.Add("Texture Wrap Mirror Once", SystemInfo.supportsTextureWrapMirrorOnce);
		hashtable3.Add("Reversed Z Buffer", SystemInfo.usesReversedZBuffer);
		hashtable.Add("Graphics - Features", hashtable3);
	}

	private void AddConsoleLogs(Hashtable hashtable)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < _StashedLogs.Count; i++)
		{
			list.Add(((object)Unsafe.As<LogType, LogType>(ref _StashedLogs[i].LogType)/*cast due to .constrained prefix*/).ToString() + ((_StashedLogs[i].Count > 1) ? $"({_StashedLogs[i].Count})" : ""));
			list.Add(_StashedLogs[i].Message);
			list.Add(_StashedLogs[i].StackTrace);
			list.Add("-----------");
		}
		hashtable.Add("Logs", list);
	}

	public static string GetBytesReadable(long i)
	{
		string text = ((i < 0) ? "-" : "");
		string text2;
		double num;
		if (i >= 1152921504606846976L)
		{
			text2 = "EB";
			num = i >> 50;
		}
		else if (i >= 1125899906842624L)
		{
			text2 = "PB";
			num = i >> 40;
		}
		else if (i >= 1099511627776L)
		{
			text2 = "TB";
			num = i >> 30;
		}
		else if (i >= 1073741824)
		{
			text2 = "GB";
			num = i >> 20;
		}
		else if (i >= 1048576)
		{
			text2 = "MB";
			num = i >> 10;
		}
		else
		{
			if (i < 1024)
			{
				return i.ToString(text + "0 B");
			}
			text2 = "KB";
			num = i;
		}
		return text + (num / 1024.0).ToString("0.### ") + text2;
	}
}
