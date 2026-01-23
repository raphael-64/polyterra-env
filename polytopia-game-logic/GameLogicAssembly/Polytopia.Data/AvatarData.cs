using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Polytopia.Data;

public class AvatarData : IPolytopiaDataRoot
{
	public Dictionary<AvatarCategory.Type, AvatarCategory> avatarCategories = new Dictionary<AvatarCategory.Type, AvatarCategory>();

	public Dictionary<AvatarPart.Type, AvatarPart> avatarParts = new Dictionary<AvatarPart.Type, AvatarPart>();

	public Dictionary<ColorPalette.Type, ColorPalette> colorPalettes = new Dictionary<ColorPalette.Type, ColorPalette>();

	private void AddAvatarPlaceholders(JObject rootObject)
	{
		Dictionary<AvatarCategory.Type, AvatarCategory> dict = avatarCategories;
		JToken obj = rootObject["avatarCategory"];
		ParseUtils.AddPlaceholdersForDictionary(dict, (JObject)(object)((obj is JObject) ? obj : null));
		Dictionary<AvatarPart.Type, AvatarPart> dict2 = avatarParts;
		JToken obj2 = rootObject["avatarPart"];
		ParseUtils.AddPlaceholdersForDictionary(dict2, (JObject)(object)((obj2 is JObject) ? obj2 : null));
		Dictionary<ColorPalette.Type, ColorPalette> dict3 = colorPalettes;
		JToken obj3 = rootObject["colorPalette"];
		ParseUtils.AddPlaceholdersForDictionary(dict3, (JObject)(object)((obj3 is JObject) ? obj3 : null));
	}

	private void ParseAvatarObjects(JObject rootObject)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		JsonSerializerSettings val = new JsonSerializerSettings();
		val.Context = new StreamingContext(StreamingContextStates.All, this);
		Dictionary<AvatarCategory.Type, AvatarCategory> dict = avatarCategories;
		JToken obj = rootObject["avatarCategory"];
		ParseUtils.ParseObjectsForDictionary(dict, (JObject)(object)((obj is JObject) ? obj : null), val);
		Dictionary<AvatarPart.Type, AvatarPart> dict2 = avatarParts;
		JToken obj2 = rootObject["avatarPart"];
		ParseUtils.ParseObjectsForDictionary(dict2, (JObject)(object)((obj2 is JObject) ? obj2 : null), val);
		Dictionary<ColorPalette.Type, ColorPalette> dict3 = colorPalettes;
		JToken obj3 = rootObject["colorPalette"];
		ParseUtils.ParseObjectsForDictionary(dict3, (JObject)(object)((obj3 is JObject) ? obj3 : null), val);
	}

	public bool TryGetDataGeneric<T, S>(S enumValue, out T result)
	{
		Type typeFromHandle = typeof(Dictionary<S, T>);
		FieldInfo[] fields = typeof(AvatarData).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType == typeFromHandle)
			{
				return (fieldInfo.GetValue(this) as Dictionary<S, T>).TryGetValue(enumValue, out result);
			}
		}
		result = default(T);
		return false;
	}

	public void Parse(string jsonData)
	{
		JObject rootObject = JObject.Parse(jsonData);
		AddAvatarPlaceholders(rootObject);
		ParseAvatarObjects(rootObject);
	}

	public bool TryGetData(AvatarPart.Type type, out AvatarPart data)
	{
		return avatarParts.TryGetValue(type, out data);
	}

	public bool TryGetData(AvatarCategory.Type type, out AvatarCategory data)
	{
		return avatarCategories.TryGetValue(type, out data);
	}

	public bool TryGetData(ColorPalette.Type type, out ColorPalette data)
	{
		return colorPalettes.TryGetValue(type, out data);
	}
}
