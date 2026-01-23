using System;

namespace Tesla;

internal class Devices : NativeWrapper
{
	private IntPtr _devices;

	public SteeringWheelDevice steeringWheel { get; private set; }

	public ScrollWheelDevice leftScrollWheel { get; private set; }

	public ScrollWheelDevice rightScrollWheel { get; private set; }

	public TouchDevice touchScreen { get; private set; }

	private bool WantsDevice(WantedDevices wantedDevices, WantedDevices device)
	{
		return (wantedDevices & device) == device;
	}

	public Devices(string deviceNamePrefix = "tesla", WantedDevices wantedDevices = WantedDevices.all)
	{
		_devices = API.TeslaDevices_Create(deviceNamePrefix, (uint)wantedDevices);
		if (WantsDevice(wantedDevices, WantedDevices.touchScreen))
		{
			touchScreen = new TouchDevice(API.TeslaDevices_GetTouchScreen(_devices));
		}
		if (WantsDevice(wantedDevices, WantedDevices.steeringWheel))
		{
			steeringWheel = new SteeringWheelDevice(API.TeslaDevices_GetSteeringWheel(_devices));
		}
		if (WantsDevice(wantedDevices, WantedDevices.leftScrollWheel))
		{
			leftScrollWheel = new ScrollWheelDevice(API.TeslaDevices_GetLeftScrollWheel(_devices));
		}
		if (WantsDevice(wantedDevices, WantedDevices.rightScrollWheel))
		{
			rightScrollWheel = new ScrollWheelDevice(API.TeslaDevices_GetRightScrollWheel(_devices));
		}
	}

	protected override void ReleaseNativeResources()
	{
		if (_devices != IntPtr.Zero)
		{
			API.TeslaDevices_Destroy(_devices);
			_devices = IntPtr.Zero;
		}
	}

	public override void Dispose(bool disposing)
	{
		if (touchScreen != null)
		{
			touchScreen.Dispose(disposing);
		}
		if (steeringWheel != null)
		{
			steeringWheel.Dispose(disposing);
		}
		if (leftScrollWheel != null)
		{
			leftScrollWheel.Dispose(disposing);
		}
		if (rightScrollWheel != null)
		{
			rightScrollWheel.Dispose(disposing);
		}
		base.Dispose(disposing);
	}

	public void Update()
	{
		if (touchScreen != null)
		{
			touchScreen.Update();
		}
		if (steeringWheel != null)
		{
			steeringWheel.Update();
		}
		if (leftScrollWheel != null)
		{
			leftScrollWheel.Update();
		}
		if (rightScrollWheel != null)
		{
			rightScrollWheel.Update();
		}
	}
}
