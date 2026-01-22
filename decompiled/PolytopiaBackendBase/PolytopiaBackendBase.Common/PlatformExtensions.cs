namespace PolytopiaBackendBase.Common;

public static class PlatformExtensions
{
	public static bool IsMobileProduction(this Platform platform)
	{
		if (platform != Platform.Android)
		{
			return platform == Platform.Ios;
		}
		return true;
	}

	private static bool IsMobileAlpha(this Platform platform)
	{
		if (platform != Platform.AndroidAlpha)
		{
			return platform == Platform.IosAlpha;
		}
		return true;
	}

	public static bool IsMobile(this Platform platform)
	{
		if (!platform.IsMobileProduction())
		{
			return platform.IsMobileAlpha();
		}
		return true;
	}

	public static bool IsComputer(this Platform platform)
	{
		if (platform != Platform.Steam)
		{
			return platform == Platform.Tesla;
		}
		return true;
	}

	public static bool IsCrossPlay(Platform platform1, Platform platform2)
	{
		if (platform1 != platform2 && ((!platform1.IsMobileProduction() && platform1 != Platform.Tesla) || (!platform2.IsMobileProduction() && platform2 != Platform.Tesla)))
		{
			if (platform1.IsMobileAlpha())
			{
				return platform2.IsMobileAlpha();
			}
			return false;
		}
		return true;
	}

	public static bool IsCompatible(Platform platform1, Platform platform2)
	{
		return platform1.IsMobileAlpha() == platform2.IsMobileAlpha();
	}
}
