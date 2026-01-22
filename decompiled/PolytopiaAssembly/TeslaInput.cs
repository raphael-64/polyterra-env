using UnityEngine;

public static class TeslaInput
{
	public static Vector2 mousePosition => Vector2.op_Implicit(Input.mousePosition);

	public static bool touchSupported => Input.touchSupported;

	public static Touch[] touches => Input.touches;

	public static int touchCount => Input.touchCount;

	public static bool GetMouseButtonDown(int button)
	{
		return Input.GetMouseButtonDown(button);
	}

	public static bool GetMouseButtonUp(int button)
	{
		return Input.GetMouseButtonUp(button);
	}

	public static bool GetMouseButton(int button)
	{
		return Input.GetMouseButton(button);
	}

	public static Touch GetTouch(int index)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Input.GetTouch(index);
	}
}
