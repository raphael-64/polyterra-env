using System;
using System.Diagnostics;
using UnityEngine;

public class FPSOverlay : MonoBehaviour
{
	[Flags]
	protected enum Visibility
	{
		FramesPerSecond = 1,
		FrameTime = 2,
		Graph = 4,
		Histogram = 8
	}

	private const int BUFFER_SIZE = 128;

	private const int HISTORY_SIZE = 50;

	private const int TEXT_AVERAGE_FRAMES = 30;

	private Stopwatch stopwatch;

	private long stopwatchFrequency;

	private long lastFrameTick;

	private float[][] data = new float[2][]
	{
		new float[128],
		new float[128]
	};

	private float[] history = new float[50];

	private float[] frametimeHistory = new float[128];

	private float accumulatedFPS;

	private float accumulatedFrametime;

	private float currentFPS;

	private float currentFrametime;

	private static Color[] colors = (Color[])(object)new Color[2]
	{
		Color32.op_Implicit(new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue)),
		Color32.op_Implicit(new Color32((byte)0, byte.MaxValue, (byte)0, byte.MaxValue))
	};

	public bool IsEnabled => Config.showFPS.IntValue >= 1;

	private void Start()
	{
		stopwatch = new Stopwatch();
		stopwatchFrequency = Stopwatch.Frequency;
		stopwatch.Start();
		lastFrameTick = stopwatch.ElapsedTicks;
	}

	private void Update()
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		if (IsEnabled)
		{
			long elapsedTicks = stopwatch.ElapsedTicks;
			float num = (float)((elapsedTicks - lastFrameTick) * 1000) / (float)stopwatchFrequency;
			lastFrameTick = elapsedTicks;
			float num2 = 1f / Time.deltaTime;
			history[Time.frameCount % history.Length] = num2;
			int num3 = 0;
			int num4 = DebugOverlay.Width - 10;
			accumulatedFrametime += num;
			accumulatedFPS += num2;
			if (Time.frameCount % 30 == 0)
			{
				currentFrametime = accumulatedFrametime / 30f;
				currentFPS = accumulatedFPS / 30f;
				accumulatedFrametime = (accumulatedFPS = 0f);
			}
			if (Config.showFPS.IntValue >= 1)
			{
				DebugOverlay.DrawText((float)num4, (float)num3, Color.yellow, "FPS " + (int)currentFPS, Array.Empty<object>());
				num3++;
			}
			if (Config.showFPS.IntValue >= 2)
			{
				DebugOverlay.DrawText((float)num4, (float)num3, Color.white, (int)currentFrametime + "ms", Array.Empty<object>());
				num3++;
			}
			if (Config.showFPS.IntValue >= 3)
			{
				float num5 = Time.deltaTime * 1000f;
				int num6 = Time.frameCount % data[0].Length;
				data[0][num6] = 0f - Mathf.Min(0f, num - num5);
				data[1][num6] = Mathf.Max(0f, num - num5);
				AnalyzeData(data[0], out var _, out var _, out var _, out var _);
				DebugOverlay.DrawRect((float)num4, (float)num3, 10f, 2f, new Color(0f, 0f, 0f, 0.3f));
				DebugOverlay.DrawGraph((float)num4, (float)num3, 10f, 2f, data, Time.frameCount, colors, -1f);
				num3 += 2;
			}
			if (Config.showFPS.IntValue >= 4)
			{
				int num7 = Time.frameCount % frametimeHistory.Length;
				frametimeHistory[num7] = num;
				AnalyzeData(frametimeHistory, out var _, out var _, out var _, out var maxValue2);
				DebugOverlay.DrawRect((float)num4, (float)num3, 10f, 2f, new Color(0f, 0f, 0f, 0.3f));
				DebugOverlay.DrawHistogram((float)num4, (float)num3, 10f, 2f, frametimeHistory, Time.frameCount, Color.red, maxValue2);
				num3 += 2;
			}
		}
	}

	private void AnalyzeData(float[] data, out float mean, out float variance, out float minValue, out float maxValue)
	{
		float num = 0f;
		float num2 = 0f;
		minValue = float.MaxValue;
		maxValue = float.MinValue;
		int num3 = data.Length;
		for (int i = 0; i < num3; i++)
		{
			float num4 = data[i];
			num += num4;
			minValue = ((num4 < minValue) ? num4 : minValue);
			maxValue = ((num4 > maxValue) ? num4 : maxValue);
		}
		mean = num / (float)num3;
		for (int j = 0; j < num3; j++)
		{
			float num5 = data[j] - mean;
			num2 += num5 * num5;
		}
		variance = num2 / (float)(num3 - 1);
	}

	private void CmdShowFPS(string[] args)
	{
		if (args != null && args.Length != 0)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] == "0" || args[i] == "false")
				{
					((Behaviour)this).enabled = false;
					break;
				}
				if (args[i] == "1" || args[i] == "true")
				{
					((Behaviour)this).enabled = true;
				}
			}
		}
		else
		{
			((Behaviour)this).enabled = !((Behaviour)this).enabled;
		}
	}
}
