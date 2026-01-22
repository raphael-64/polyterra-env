using System;

public static class EnumExtensions
{
	public static string GetName<T>(this T value) where T : struct, IComparable, IFormattable, IConvertible
	{
		return EnumCache<T>.GetName(value);
	}
}
