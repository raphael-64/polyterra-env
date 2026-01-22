using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Polytopia.Data;

public class StringIDToEnumConverter<S> : JsonConverter where S : struct, IComparable, IFormattable, IConvertible
{
	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException("Unnecessary because CanWrite is false.");
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		string text = null;
		if ((int)reader.TokenType != 11)
		{
			text = JToken.Load(reader).ToObject<string>();
		}
		if (text == null)
		{
			return null;
		}
		return EnumCache<S>.GetType(text);
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType.GetElementType() == typeof(string);
	}
}
