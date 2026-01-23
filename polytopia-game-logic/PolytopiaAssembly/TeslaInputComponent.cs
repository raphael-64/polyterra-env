using Tesla;
using UnityEngine;
using UnityEngine.EventSystems;

public class TeslaInputComponent : MonoBehaviour
{
	public bool touchScreen = true;

	public bool leftScrollWheel;

	public bool rightScrollWheel;

	public bool steeringWheelAndBrakePedal;

	[SerializeField]
	private TeslaUIInput teslaInput;

	private bool isDone;

	private void Start()
	{
		WantedDevices wantedDevices = (WantedDevices)0;
		if (touchScreen)
		{
			wantedDevices |= WantedDevices.touchScreen;
		}
		if (steeringWheelAndBrakePedal)
		{
			wantedDevices |= WantedDevices.steeringWheel;
		}
		if (leftScrollWheel)
		{
			wantedDevices |= WantedDevices.leftScrollWheel;
		}
		if (rightScrollWheel)
		{
			wantedDevices |= WantedDevices.rightScrollWheel;
		}
		Input.Initialize(wantedDevices);
	}

	private void Update()
	{
		if (!isDone)
		{
			EventSystem current = EventSystem.current;
			if ((Object)(object)((current != null) ? current.currentInputModule : null) != (Object)null)
			{
				EventSystem.current.currentInputModule.inputOverride = (BaseInput)(object)teslaInput;
				isDone = true;
			}
		}
		Input.PumpEvents();
	}
}
