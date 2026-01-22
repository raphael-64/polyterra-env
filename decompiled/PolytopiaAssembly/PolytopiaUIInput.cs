using UnityEngine;
using UnityEngine.EventSystems;

public class PolytopiaUIInput : BaseInput
{
	public override bool mousePresent => PolytopiaInput.mousePresent;

	public override Vector2 mousePosition => PolytopiaInput.mousePosition;

	public override bool touchSupported => PolytopiaInput.touchSupported;

	public override int touchCount => PolytopiaInput.touchCount;

	public override bool GetMouseButtonDown(int button)
	{
		return PolytopiaInput.GetMouseButtonDown(button);
	}

	public override bool GetMouseButtonUp(int button)
	{
		return PolytopiaInput.GetMouseButtonUp(button);
	}

	public override bool GetMouseButton(int button)
	{
		return PolytopiaInput.GetMouseButton(button);
	}

	public override Touch GetTouch(int index)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return PolytopiaInput.GetTouch(index);
	}
}
