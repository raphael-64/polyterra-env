using System;

namespace Tesla;

internal class SteeringWheelDevice : Device<SteeringWheelDeviceEvent>
{
	internal SteeringWheelDevice(IntPtr driver)
		: base(driver, API.TeslaSteeringDeviceEventBuffer_Create())
	{
	}

	protected override void DestroyEventBuffer(IntPtr evBuf)
	{
		API.TeslaSteeringDeviceEventBuffer_Destroy(evBuf);
	}

	protected override IntPtr GetEventData(IntPtr evBuf)
	{
		return API.TeslaSteeringDeviceEventBuffer_GetEventData(evBuf);
	}

	protected override uint GetEventSize()
	{
		return API.TeslaSteeringDeviceEventBuffer_GetEventSize();
	}

	protected override uint UpdateEvents(IntPtr evBuf)
	{
		return API.TeslaDeviceDriverSteeringWheel_UpdateEvents(base.driver, evBuf);
	}
}
