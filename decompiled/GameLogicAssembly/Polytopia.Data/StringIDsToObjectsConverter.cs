using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Polytopia.Data;

public class StringIDsToObjectsConverter<T, S> : JsonConverter where S : struct, IComparable, IFormattable, IConvertible
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
		List<T> list = new List<T>();
		string[] array = null;
		if ((int)reader.TokenType != 11 && (int)reader.TokenType == 2)
		{
			array = JToken.Load(reader).ToObject<string[]>();
		}
		if (array == null)
		{
			return list;
		}
		IPolytopiaDataRoot polytopiaDataRoot = serializer.Context.Context as IPolytopiaDataRoot;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			S type = EnumCache<S>.GetType(array2[i]);
			polytopiaDataRoot.TryGetDataGeneric<T, S>(type, out var result);
			list.Add(result);
		}
		return list;
	}

	public override bool CanConvert(Type objectType)
	{
		if (objectType.IsArray)
		{
			return objectType.GetElementType() == typeof(string);
		}
		return false;
	}
}
