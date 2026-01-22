public static class StringUtils
{
	public static string ToLowerFirstChar(this string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return char.ToLower(input[0]) + input.Substring(1);
	}

	public static string Truncate(this string value, int maxLength)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		if (value.Length > maxLength)
		{
			return value.Substring(0, maxLength);
		}
		return value;
	}
}
