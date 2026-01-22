using System;
using System.Runtime.InteropServices;

namespace Tesla;

public class PointerOutputValue<T> : NativeWrapper
{
	private IntPtr _ptr;

	public IntPtr address => _ptr;

	public T value => Marshal.PtrToStructure<T>(_ptr);

	public PointerOutputValue()
	{
		_ptr = Marshal.AllocHGlobal(IntPtr.Size);
	}

	protected override void ReleaseNativeResources()
	{
		Marshal.FreeHGlobal(_ptr);
	}
}
