using System;
using System.Collections.Generic;

namespace Tesla;

internal static class CallBackLUT<T>
{
	private static Dictionary<IntPtr, AsyncResult<T>> objMap = new Dictionary<IntPtr, AsyncResult<T>>();

	private static ulong counter = 0uL;

	public static IntPtr Add(AsyncResult<T> result)
	{
		lock (objMap)
		{
			IntPtr intPtr = (IntPtr)(long)counter++;
			objMap[intPtr] = result;
			return intPtr;
		}
	}

	public static void Set(IntPtr context, T result)
	{
		lock (objMap)
		{
			if (!objMap.ContainsKey(context))
			{
				throw new InvalidOperationException("Key not found");
			}
			objMap[context].Set(result);
			objMap.Remove(context);
		}
	}
}
