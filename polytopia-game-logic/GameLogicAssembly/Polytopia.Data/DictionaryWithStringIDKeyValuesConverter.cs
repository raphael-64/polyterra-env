using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Polytopia.Data;

public class DictionaryWithStringIDKeyValuesConverter<T, S> : JsonConverter where S : struct, IComparable, IFormattable, IConvertible
{
	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException("Unnecessary because CanWrite is false.");
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		Dictionary<S, T> dictionary = new Dictionary<S, T>();
		Dictionary<string, string> dictionary2 = null;
		if ((int)reader.TokenType != 11 && (int)reader.TokenType == 1)
		{
			dictionary2 = JToken.Load(reader).ToObject<Dictionary<string, string>>();
		}
		if (dictionary2 == null)
		{
			return dictionary;
		}
		IPolytopiaDataRoot polytopiaDataRoot = serializer.Context.Context as IPolytopiaDataRoot;
		foreach (KeyValuePair<string, string> item in dictionary2)
		{
			S type = EnumCache<S>.GetType(item.Key);
			S type2 = EnumCache<S>.GetType(item.Value);
			polytopiaDataRoot.TryGetDataGeneric<T, S>(type2, out var result);
			dictionary.Add(type, result);
		}
		return dictionary;
	}

	public override bool CanConvert(Type objectType)
	{
		return true;
	}
}
