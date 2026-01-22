using UnityEngine;

public class PollAnimation<T>
{
	public float startTime;

	public float duration;

	public Easing easing;

	public float? easingParameter;

	public T startValue;

	public T targetValue;

	public PollAnimation(float duration, Easing easing = Easing.Linear, float? easingParameter = null)
	{
		startTime = Time.time;
		this.duration = duration;
		this.easing = easing;
		this.easingParameter = easingParameter;
	}

	public PollAnimation(float duration, T startValue, T targetValue, Easing easing = Easing.Linear, float? easingParameter = null)
	{
		startTime = Time.time;
		this.duration = duration;
		this.startValue = startValue;
		this.targetValue = targetValue;
		this.easing = easing;
		this.easingParameter = easingParameter;
	}

	public float GetEasedProgress()
	{
		return EasingExtensions.EasedValue(GetProgress(), easing, easingParameter);
	}

	public float GetProgress()
	{
		return Mathf.Clamp01((Time.time - startTime) / duration);
	}

	public bool IsCompleted()
	{
		return Mathf.Approximately(GetProgress(), 1f);
	}
}
