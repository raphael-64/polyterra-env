using UnityEngine;
using UnityEngine.EventSystems;

public class TeslaUIInput : BaseInput
{
	public override Vector2 mousePosition => TeslaInput.mousePosition;

	public override bool touchSupported => TeslaInput.touchSupported;

	public override int touchCount => TeslaInput.touchCount;

	public override bool GetMouseButtonDown(int button)
	{
		return TeslaInput.GetMouseButtonDown(button);
	}

	public override bool GetMouseButtonUp(int button)
	{
		return TeslaInput.GetMouseButtonUp(button);
	}

	public override bool GetMouseButton(int button)
	{
		return TeslaInput.GetMouseButton(button);
	}

	public override Touch GetTouch(int index)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return TeslaInput.GetTouch(index);
	}
}
