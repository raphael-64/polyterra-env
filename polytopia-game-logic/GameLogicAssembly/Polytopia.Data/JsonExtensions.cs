using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Polytopia.Data;

public static class JsonExtensions
{
	public static void Populate<T>(this JToken value, T target, JsonSerializerSettings settings = null) where T : class
	{
		JsonReader val = value.CreateReader();
		try
		{
			JsonSerializer.CreateDefault(settings).Populate(val, (object)target);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
