using System.Collections.Generic;
using UnityEngine;

namespace Tesla;

public class Input
{
	public const int MaxTouches = 10;

	private WantedDevices _initializedDevices;

	private Devices _teslaDevices;

	private int _touchCount;

	private bool[] _touchStates = new bool[10];

	private Touch[] _touchCache = new Touch[10];

	private int[] _touchMap = new int[10];

	private ScrollWheelButtons _leftSWButtonsDown;

	private ScrollWheelButtons _rightSWButtonsDown;

	private int _leftSWMovement;

	private int _rightSWMovement;

	private float _wheelAngle;

	private bool _brakePressed;

	private bool _pasEnabled;

	private static Input _instance;

	private static Input instance
	{
		get
		{
			return _instance;
		}
		set
		{
			_instance = value;
		}
	}

	public static bool powerAssistedSteeringEnabled
	{
		get
		{
			if (instance != null)
			{
				return instance._pasEnabled;
			}
			return false;
		}
	}

	public static bool brakePressed
	{
		get
		{
			if (instance != null)
			{
				return instance._brakePressed;
			}
			return false;
		}
	}

	public static float steeringWheelAngle
	{
		get
		{
			if (instance != null)
			{
				return instance._wheelAngle;
			}
			return 0f;
		}
	}

	public static ScrollWheelButtons leftScrollWheelButtonsDown
	{
		get
		{
			if (instance != null)
			{
				return instance._leftSWButtonsDown;
			}
			return ScrollWheelButtons.left;
		}
	}

	public static ScrollWheelButtons rightScrollWheelButtonsDown
	{
		get
		{
			if (instance != null)
			{
				return instance._rightSWButtonsDown;
			}
			return ScrollWheelButtons.left;
		}
	}

	public static int leftScrollWheelMovement
	{
		get
		{
			if (instance != null)
			{
				return instance._leftSWMovement;
			}
			return 0;
		}
	}

	public static int rightScrollWheelMovement
	{
		get
		{
			if (instance != null)
			{
				return instance._rightSWMovement;
			}
			return 0;
		}
	}

	public static int touchCount
	{
		get
		{
			if (instance != null)
			{
				return instance._touchCount;
			}
			return 0;
		}
	}

	public static IEnumerable<Touch> touches
	{
		get
		{
			if (instance == null)
			{
				yield break;
			}
			int i = 0;
			while (i < 10)
			{
				if (instance._touchStates[i])
				{
					yield return instance._touchCache[i];
				}
				int num = i + 1;
				i = num;
			}
		}
	}

	public static Touch GetTouch(int index)
	{
		if (instance != null && index >= 0 && index < instance._touchCount)
		{
			return instance._touchCache[instance._touchMap[index]];
		}
		return new Touch
		{
			fingerId = -1
		};
	}

	public static WantedDevices Initialize(WantedDevices wantedDevices)
	{
		if (instance == null)
		{
			instance = new Input(wantedDevices);
		}
		return instance._initializedDevices;
	}

	public static void PumpEvents()
	{
		if (instance != null)
		{
			instance.PumpEventsInternal();
		}
	}

	private Input(WantedDevices wantedDevices)
	{
	}

	private void PumpEventsInternal()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Invalid comparison between Unknown and I4
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		if (_teslaDevices == null)
		{
			return;
		}
		_teslaDevices.Update();
		if (_teslaDevices.touchScreen != null)
		{
			int num = 0;
			for (int i = 0; i < 10; i++)
			{
				if (num >= _touchCount)
				{
					break;
				}
				if (_touchStates[i])
				{
					if ((int)_touchCache[i].phase == 3)
					{
						_touchStates[i] = false;
						_touchCount--;
					}
					else
					{
						_touchMap[num++] = i;
						_touchCache[i].phase = (TouchPhase)2;
					}
				}
			}
			if (_touchCount != num)
			{
				Debug.LogError((object)$"Touch count ({_touchCount}) doesn't equal number of valid touches ({num})");
			}
			foreach (TouchDeviceEvent @event in _teslaDevices.touchScreen.Events)
			{
				if ((bool)@event.down && !_touchStates[@event.id])
				{
					if (_touchCount != 10)
					{
						_touchStates[@event.id] = true;
						_touchCache[@event.id].fingerId = @event.id;
						_touchCache[@event.id].phase = (TouchPhase)0;
						_touchCache[@event.id].position = new Vector2((float)@event.x, (float)(Screen.height - @event.y - 1));
						_touchMap[_touchCount++] = @event.id;
					}
				}
				else if (!@event.down && _touchStates[@event.id])
				{
					_touchCache[@event.id].phase = (TouchPhase)3;
				}
				else if ((bool)@event.down)
				{
					_touchCache[@event.id].position = new Vector2((float)@event.x, (float)(Screen.height - @event.y - 1));
					if ((int)_touchCache[@event.id].phase == 2)
					{
						_touchCache[@event.id].phase = (TouchPhase)1;
					}
				}
			}
		}
		if (_teslaDevices.steeringWheel != null)
		{
			foreach (SteeringWheelDeviceEvent event2 in _teslaDevices.steeringWheel.Events)
			{
				Debug.Log((object)$"steering wheel event: movement:{event2.wheelAngle} brake:{event2.brakePressed}");
				_wheelAngle = (float)event2.wheelAngle;
				_brakePressed = event2.brakePressed;
				_pasEnabled = event2.powerAssistedSteeringEnabled;
			}
		}
		if (_teslaDevices.leftScrollWheel != null)
		{
			_leftSWMovement = 0;
			_leftSWButtonsDown = ScrollWheelButtons.left;
			foreach (ScrollWheelDeviceEvent event3 in _teslaDevices.leftScrollWheel.Events)
			{
				Debug.Log((object)$"scroll wheel event: movement:{event3.scrollMovement} buttons:{event3.buttons}");
				_leftSWMovement += event3.scrollMovement;
				if (event3.leftButtonDown)
				{
					_leftSWButtonsDown |= ScrollWheelButtons.left;
				}
				if (event3.rightButtonDown)
				{
					_leftSWButtonsDown |= ScrollWheelButtons.right;
				}
				if (event3.middleButtonDown)
				{
					_leftSWButtonsDown |= ScrollWheelButtons.middle;
				}
			}
		}
		if (_teslaDevices.rightScrollWheel == null)
		{
			return;
		}
		_rightSWMovement = 0;
		_rightSWButtonsDown = ScrollWheelButtons.left;
		foreach (ScrollWheelDeviceEvent event4 in _teslaDevices.rightScrollWheel.Events)
		{
			Debug.Log((object)$"scroll wheel event: movement:{event4.scrollMovement} buttons:{event4.buttons}");
			_rightSWMovement += event4.scrollMovement;
			if (event4.leftButtonDown)
			{
				_rightSWButtonsDown |= ScrollWheelButtons.left;
			}
			if (event4.rightButtonDown)
			{
				_rightSWButtonsDown |= ScrollWheelButtons.right;
			}
			if (event4.middleButtonDown)
			{
				_rightSWButtonsDown |= ScrollWheelButtons.middle;
			}
		}
	}
}
