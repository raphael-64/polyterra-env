using System;
using System.Collections.Generic;
using System.Linq;

namespace Polytopia.Data;

public static class SkinTypeExtensions
{
	public static string GetName(this SkinType skinType)
	{
		return skinType.ToString().ToLower();
	}

	public static string GetSkinNameKey()
	{
		return "TribeSkins/skinName";
	}

	public static string GetLocalizationKey(this SkinType skinType)
	{
		return "TribeSkins/tribeskins." + skinType.GetName();
	}

	public static string GetLocalizationDescriptionKey(this SkinType skinType)
	{
		return "TribeSkins/tribeskins." + skinType.GetName() + ".description";
	}

	public static List<SkinType> GetAllSkinsTypes()
	{
		return Enum.GetValues(typeof(SkinType)).Cast<SkinType>().ToList();
	}

	public static bool CanUseInCurrentGameVersion(this SkinType skinType, int version)
	{
		switch (skinType)
		{
		case SkinType.Default:
		case SkinType.Test:
			return true;
		case SkinType.Ranger:
		case SkinType.Baerion:
		case SkinType.Skeleton:
			return version >= 86;
		default:
			return false;
		}
	}

	public static bool IsLocked(this SkinType skinType)
	{
		if (skinType == SkinType.Ninja || (uint)(skinType - 4) <= 4u || skinType == SkinType.Test)
		{
			return true;
		}
		return false;
	}
}
