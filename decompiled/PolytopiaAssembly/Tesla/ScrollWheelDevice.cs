using System;

namespace Tesla;

internal class ScrollWheelDevice : Device<ScrollWheelDeviceEvent>
{
	internal ScrollWheelDevice(IntPtr driver)
		: base(driver, API.TeslaScrollDeviceEventBuffer_Create())
	{
	}

	protected override void DestroyEventBuffer(IntPtr evBuf)
	{
		API.TeslaScrollDeviceEventBuffer_Destroy(evBuf);
	}

	protected override IntPtr GetEventData(IntPtr evBuf)
	{
		return API.TeslaScrollDeviceEventBuffer_GetEventData(evBuf);
	}

	protected override uint GetEventSize()
	{
		return API.TeslaScrollDeviceEventBuffer_GetEventSize();
	}

	protected override uint UpdateEvents(IntPtr evBuf)
	{
		return API.TeslaDeviceDriverScrollWheel_UpdateEvents(base.driver, evBuf);
	}
}
