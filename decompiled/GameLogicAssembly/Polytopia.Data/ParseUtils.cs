using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Polytopia.Data;

public static class ParseUtils
{
	public static void AddPlaceholdersForDictionary<T, S>(Dictionary<T, S> dict, JObject json) where T : struct, IComparable, IFormattable, IConvertible where S : class, new()
	{
		if (json == null)
		{
			return;
		}
		foreach (JProperty item in json.Properties())
		{
			if (!EnumCache<T>.TryGetType(item.Name, out var type) || !EnumCache<T>.TryGetName(type, out var _))
			{
				type = (T)(object)(int)item.Value[(object)"idx"];
				EnumCache<T>.AddMapping(item.Name, type);
			}
			dict[type] = new S();
		}
	}

	public static void ParseObjectsForDictionary<T, S>(Dictionary<T, S> dict, JObject json, JsonSerializerSettings settings = null) where T : struct, IComparable, IFormattable, IConvertible where S : class, new()
	{
		foreach (JProperty item in json.Properties())
		{
			T type = EnumCache<T>.GetType(item.Name);
			JsonExtensions.Populate(target: dict[type], value: item.Value, settings: settings);
		}
	}
}
