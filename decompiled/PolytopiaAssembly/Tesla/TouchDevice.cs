using System;

namespace Tesla;

internal class TouchDevice : Device<TouchDeviceEvent>
{
	internal TouchDevice(IntPtr driver)
		: base(driver, API.TeslaTouchDeviceEventBuffer_Create())
	{
	}

	protected override void DestroyEventBuffer(IntPtr evBuf)
	{
		API.TeslaTouchDeviceEventBuffer_Destroy(evBuf);
	}

	protected override IntPtr GetEventData(IntPtr evBuf)
	{
		return API.TeslaTouchDeviceEventBuffer_GetEventData(evBuf);
	}

	protected override uint GetEventSize()
	{
		return API.TeslaTouchDeviceEventBuffer_GetEventSize();
	}

	protected override uint UpdateEvents(IntPtr evBuf)
	{
		return API.TeslaDeviceDriverTouch_UpdateEvents(base.driver, evBuf);
	}

	public void SetSize(int width, int height)
	{
		API.TeslaDeviceDriverTouch_SetSize(base.driver, width, height);
	}
}
