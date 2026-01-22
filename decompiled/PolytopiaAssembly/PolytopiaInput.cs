using System;
using UnityEngine;

public static class PolytopiaInput
{
	private enum InputMode
	{
		MouseOrTouch,
		Omnicursor
	}

	public static readonly OmnicursorController Omnicursor = new OmnicursorController();

	private static InputMode _inputMode = InputMode.MouseOrTouch;

	private static int _lastUpdateCheckFrame = -1;

	private static Vector3 _lastUpdateMousePosition = Input.mousePosition;

	public static bool isTrackingOmnicursor
	{
		get
		{
			if (_inputMode == InputMode.Omnicursor && !Omnicursor.IsPrimaryButtonHeld)
			{
				return !Omnicursor.IsPrimaryButtonUp;
			}
			return false;
		}
	}

	public static Vector2 mousePosition
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			UpdateInputMode();
			return (Vector2)(_inputMode switch
			{
				InputMode.MouseOrTouch => Vector2.op_Implicit(Input.mousePosition), 
				InputMode.Omnicursor => Omnicursor.SimulatedMousePosition, 
				_ => throw new Exception($"Unknown input mode {_inputMode}"), 
			});
		}
	}

	public static bool touchSupported => Input.touchSupported;

	public static Touch[] touches
	{
		get
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			UpdateInputMode();
			switch (_inputMode)
			{
			case InputMode.MouseOrTouch:
				return Input.touches;
			case InputMode.Omnicursor:
				if (Omnicursor.IsPrimaryButtonHeld || Omnicursor.IsPrimaryButtonUp)
				{
					return (Touch[])(object)new Touch[1] { GetTouch(0) };
				}
				return (Touch[])(object)new Touch[0];
			default:
				throw new Exception($"Unknown input mode {_inputMode}");
			}
		}
	}

	public static int touchCount
	{
		get
		{
			UpdateInputMode();
			switch (_inputMode)
			{
			case InputMode.MouseOrTouch:
				return Input.touchCount;
			case InputMode.Omnicursor:
				if (!Omnicursor.IsPrimaryButtonHeld && !Omnicursor.IsPrimaryButtonUp)
				{
					return 0;
				}
				return 1;
			default:
				throw new Exception($"Unknown input mode {_inputMode}");
			}
		}
	}

	public static bool mousePresent => Input.mousePresent;

	public static bool GetMouseButtonDown(int button)
	{
		UpdateInputMode();
		switch (_inputMode)
		{
		case InputMode.MouseOrTouch:
			return Input.GetMouseButtonDown(button);
		case InputMode.Omnicursor:
			if (button == 0)
			{
				return Omnicursor.IsPrimaryButtonDown;
			}
			return false;
		default:
			throw new Exception($"Unknown input mode {_inputMode}");
		}
	}

	public static bool GetMouseButtonUp(int button)
	{
		UpdateInputMode();
		switch (_inputMode)
		{
		case InputMode.MouseOrTouch:
			return Input.GetMouseButtonUp(button);
		case InputMode.Omnicursor:
			if (button == 0)
			{
				return Omnicursor.IsPrimaryButtonUp;
			}
			return false;
		default:
			throw new Exception($"Unknown input mode {_inputMode}");
		}
	}

	public static bool GetMouseButton(int button)
	{
		UpdateInputMode();
		switch (_inputMode)
		{
		case InputMode.MouseOrTouch:
			return Input.GetMouseButton(button);
		case InputMode.Omnicursor:
			if (button == 0)
			{
				return Omnicursor.IsPrimaryButtonHeld;
			}
			return false;
		default:
			throw new Exception($"Unknown input mode {_inputMode}");
		}
	}

	public static Touch GetTouch(int index)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		UpdateInputMode();
		switch (_inputMode)
		{
		case InputMode.MouseOrTouch:
			return Input.GetTouch(index);
		case InputMode.Omnicursor:
			if (index == 0 && (Omnicursor.IsPrimaryButtonHeld || Omnicursor.IsPrimaryButtonUp))
			{
				Touch result = default(Touch);
				((Touch)(ref result)).position = Omnicursor.SimulatedMousePosition;
				if (Omnicursor.IsPrimaryButtonDown)
				{
					((Touch)(ref result)).phase = (TouchPhase)0;
				}
				else if (Omnicursor.IsPrimaryButtonUp)
				{
					((Touch)(ref result)).phase = (TouchPhase)3;
				}
				else
				{
					((Touch)(ref result)).phase = (TouchPhase)1;
				}
				return result;
			}
			return default(Touch);
		default:
			throw new Exception($"Unknown input mode {_inputMode}");
		}
	}

	private static void UpdateInputMode()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (_lastUpdateCheckFrame == Time.frameCount || InputManager.gamepadInputManager == null)
		{
			return;
		}
		_lastUpdateCheckFrame = Time.frameCount;
		switch (_inputMode)
		{
		case InputMode.MouseOrTouch:
			if (!InputManager.gamepadInputManager.IsAnyButtonHeldDown())
			{
				Vector2 val2 = InputManager.gamepadInputManager.GetPrimaryStick();
				if (!(((Vector2)(ref val2)).sqrMagnitude > 0.25f))
				{
					val2 = InputManager.gamepadInputManager.GetSecondaryStick();
					if (!(((Vector2)(ref val2)).sqrMagnitude > 0.25f))
					{
						if (Object.op_Implicit((Object)(object)InputSystemCursorOverride.INSTANCE))
						{
							Cursor.visible = SystemManager.ShouldShowMouseCursor();
							((Component)InputSystemCursorOverride.INSTANCE).gameObject.SetActive(false);
						}
						break;
					}
				}
			}
			_inputMode = InputMode.Omnicursor;
			break;
		case InputMode.Omnicursor:
			if (!Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1) && !Input.GetMouseButtonUp(1) && Input.touchCount <= 0)
			{
				Vector3 val = _lastUpdateMousePosition - Input.mousePosition;
				if (!(((Vector3)(ref val)).sqrMagnitude > 10f))
				{
					if (Object.op_Implicit((Object)(object)InputSystemCursorOverride.INSTANCE))
					{
						Cursor.visible = false;
						((Component)InputSystemCursorOverride.INSTANCE).gameObject.SetActive(true);
					}
					break;
				}
			}
			_inputMode = InputMode.MouseOrTouch;
			break;
		}
		_lastUpdateMousePosition = Input.mousePosition;
	}
}
