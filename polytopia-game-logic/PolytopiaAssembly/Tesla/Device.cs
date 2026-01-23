using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tesla;

internal abstract class Device<TEvent> : NativeWrapper where TEvent : struct
{
	private IntPtr _driver;

	private IntPtr _evBuf;

	private List<TEvent> _evCache;

	protected IntPtr driver => _driver;

	public IList<TEvent> Events => _evCache;

	internal Device(IntPtr driverPtr, IntPtr evBuf)
	{
		_driver = driverPtr;
		_evBuf = evBuf;
		_evCache = new List<TEvent>();
		if (GetEventSize() != Marshal.SizeOf(typeof(TEvent)))
		{
			throw new Exception(string.Format("Device {2}: expected event of size {0} but got {1}", GetEventSize(), Marshal.SizeOf(typeof(TEvent)), GetType().Name));
		}
	}

	protected override void ReleaseNativeResources()
	{
		if (_evBuf != IntPtr.Zero)
		{
			DestroyEventBuffer(_evBuf);
			_evBuf = IntPtr.Zero;
		}
	}

	protected abstract void DestroyEventBuffer(IntPtr evBuf);

	protected abstract IntPtr GetEventData(IntPtr evBuf);

	protected abstract uint UpdateEvents(IntPtr evBuf);

	protected abstract uint GetEventSize();

	internal uint Update()
	{
		_evCache.Clear();
		uint num = UpdateEvents(_evBuf);
		if (num == 0)
		{
			return 0u;
		}
		IntPtr eventData = GetEventData(_evBuf);
		long num2 = Marshal.SizeOf(typeof(TEvent));
		for (long num3 = 0L; num3 < num; num3++)
		{
			_evCache.Add((TEvent)Marshal.PtrToStructure((IntPtr)((long)eventData + num3 * num2), typeof(TEvent)));
		}
		return num;
	}
}
