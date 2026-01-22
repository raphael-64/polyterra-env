using Newtonsoft.Json;

public static class DataUtils
{
	public static bool IsValidJson<T>(this string strInput)
	{
		strInput = strInput.Trim();
		if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || (strInput.StartsWith("[") && strInput.EndsWith("]")))
		{
			try
			{
				JsonConvert.DeserializeObject<T>(strInput);
				return true;
			}
			catch
			{
				return false;
			}
		}
		return false;
	}
}
