using Polytopia.Data;

public static class TribeExtensions
{
	public static TribeData.Type? GetType(string type)
	{
		if (EnumCache<TribeData.Type>.TryGetType(type, out var type2))
		{
			return type2;
		}
		return null;
	}

	public static string GetMixedTribeName(TribeData tribeData, TribeData mixTribeData)
	{
		string text = Localization.Get(mixTribeData.displayName);
		string text2 = Localization.Get(tribeData.displayName);
		return $"{text.Substring(0, 4)}{text2.Substring(text2.Length - 3)}";
	}
}
