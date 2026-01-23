using UnityEngine;

public class ToggleButtonAttribute : PropertyAttribute
{
	public readonly float width;

	public readonly float height;

	public readonly bool rightAligned;

	public ToggleButtonAttribute()
	{
	}

	public ToggleButtonAttribute(float width = 0f, float height = 0f, bool rightAligned = false)
	{
		this.width = width;
		this.height = height;
		this.rightAligned = rightAligned;
	}
}
