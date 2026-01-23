using System;
using UnityEngine;

public class UnityLogger : Logger
{
	public void LogCallback(string logEntry, string stackTrace, LogType logType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)logType != 2)
		{
			if ((int)logType == 4)
			{
				Application.Quit();
			}
		}
		else if (logEntry.Contains("Failed to create RenderTexture"))
		{
			GraphicsUtils.ReduceGraphicsSettings();
		}
	}

	public override void LogSpam(string format, object[] args)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)base.LogLevel >= 5)
		{
			Debug.LogFormat((LogType)3, (LogOption)((int)base.StackTraceLevel < 5), (Object)null, format, args);
		}
	}

	public override void LogVerbose(string format, object[] args)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)base.LogLevel >= 4)
		{
			Debug.LogFormat((LogType)3, (LogOption)((int)base.StackTraceLevel < 4), (Object)null, format, args);
		}
	}

	public override void LogInfo(string format, object[] args)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)base.LogLevel >= 3)
		{
			Debug.LogFormat((LogType)3, (LogOption)((int)base.StackTraceLevel < 3), (Object)null, format, args);
		}
	}

	public override void LogWarning(string format, object[] args)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)base.LogLevel >= 2)
		{
			Debug.LogFormat((LogType)2, (LogOption)((int)base.StackTraceLevel < 2), (Object)null, format, args);
		}
	}

	public override void LogError(string format, object[] args)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)base.LogLevel >= 1)
		{
			Debug.LogFormat((LogType)0, (LogOption)((int)base.StackTraceLevel < 1), (Object)null, format, args);
		}
	}

	public override void LogException(Exception exception)
	{
		Debug.LogException(exception);
	}
}
