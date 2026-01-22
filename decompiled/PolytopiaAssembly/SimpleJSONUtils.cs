using SimpleJSON;
using UnityEngine;

public static class SimpleJSONUtils
{
	public static JSONNode FindMatchingNode(JSONArray array, string key, string value)
	{
		JSONNode.Enumerator enumerator = array.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode jSONNode = enumerator.Current;
			if (jSONNode[key] == (object)value)
			{
				return jSONNode;
			}
		}
		return null;
	}

	public static int GetNextIndex(JSONArray array)
	{
		int num = -1;
		JSONNode.Enumerator enumerator = array.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode jSONNode = enumerator.Current;
			num = Mathf.Max(num, jSONNode["idx"].AsInt);
		}
		return num + 1;
	}

	public static void RemoveFromStraightList(JSONArray array, string value)
	{
		JSONNode jSONNode = null;
		JSONNode.Enumerator enumerator = array.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode jSONNode2 = enumerator.Current;
			JSONNode.Enumerator enumerator2 = jSONNode2.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				if (enumerator2.Current.Value == (object)value)
				{
					jSONNode = jSONNode2;
					break;
				}
			}
		}
		if (jSONNode != null)
		{
			array.Remove(jSONNode);
		}
	}

	public static void AddToStraightList(JSONArray array, string value)
	{
		JSONNode jSONNode = null;
		JSONNode.Enumerator enumerator = array.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode jSONNode2 = enumerator.Current;
			JSONNode.Enumerator enumerator2 = jSONNode2.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				if (enumerator2.Current.Value == (object)value)
				{
					jSONNode = jSONNode2;
				}
			}
		}
		if (jSONNode == null)
		{
			JSONObject jSONObject = new JSONObject();
			jSONObject[""] = value;
			array.Add(jSONObject);
		}
	}
}
