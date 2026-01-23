using System;
using System.Runtime.InteropServices;

namespace Tesla;

public static class NativeString
{
	public static string PointerToString(IntPtr ptr)
	{
		return Marshal.PtrToStringAnsi(ptr);
	}
}
