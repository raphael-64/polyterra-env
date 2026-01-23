using System;
using System.Diagnostics;

public class LogicTimer
{
	public const float FRAMES_PER_SECOND = 30f;

	public const float FIXED_DELTA = 1f / 30f;

	private double accumulator;

	private long lastTime;

	private readonly Stopwatch stopwatch;

	private readonly Action action;

	public float LerpAlpha => (float)accumulator / (1f / 30f);

	public LogicTimer(Action action)
	{
		stopwatch = new Stopwatch();
		this.action = action;
	}

	public void Start()
	{
		lastTime = 0L;
		accumulator = 0.0;
		stopwatch.Start();
	}

	public void Stop()
	{
		stopwatch.Stop();
	}

	public void Update()
	{
		long elapsedTicks = stopwatch.ElapsedTicks;
		accumulator += (double)(elapsedTicks - lastTime) / (double)Stopwatch.Frequency;
		for (lastTime = elapsedTicks; accumulator >= 0.03333333507180214; accumulator -= 0.03333333507180214)
		{
			action();
		}
	}
}
