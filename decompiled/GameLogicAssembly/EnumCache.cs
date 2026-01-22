using System;
using System.Collections.Generic;

public static class EnumCache<T> where T : struct, IComparable, IFormattable, IConvertible
{
	private static Dictionary<T, string> nameMap;

	private static Dictionary<string, T> typeMap;

	static EnumCache()
	{
		nameMap = new Dictionary<T, string>();
		typeMap = new Dictionary<string, T>();
		foreach (T value in Enum.GetValues(typeof(T)))
		{
			string text = value.ToString().ToLowerInvariant();
			nameMap.Add(value, text);
			typeMap.Add(text, value);
		}
	}

	public static void AddMapping(string name, T value)
	{
		string text = name.ToLowerInvariant();
		if (!nameMap.TryGetValue(value, out var value2))
		{
			nameMap.Add(value, text);
			return;
		}
		if (value2 != name)
		{
			throw new Exception($"Mismatching names {text} and {value2} for value {value}");
		}
		if (!typeMap.TryGetValue(text, out var value3))
		{
			typeMap.Add(text, value);
		}
		else if (value3.CompareTo(value) == 0)
		{
			throw new Exception($"Mismatching values {value} and {value3} for name {text}");
		}
	}

	public static string GetName(T type)
	{
		if (!nameMap.TryGetValue(type, out var value))
		{
			throw new Exception($"Missing name for value {type}");
		}
		return value;
	}

	public static bool TryGetName(T type, out string value)
	{
		return nameMap.TryGetValue(type, out value);
	}

	public static T GetType(string name)
	{
		if (!typeMap.TryGetValue(name, out var value))
		{
			throw new Exception($"Missing enum value for string {name}");
		}
		return value;
	}

	public static bool TryGetType(string name, out T type)
	{
		return typeMap.TryGetValue(name, out type);
	}
}
