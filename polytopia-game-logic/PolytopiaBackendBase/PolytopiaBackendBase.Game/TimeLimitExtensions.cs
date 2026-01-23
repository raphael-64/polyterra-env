using System;

namespace PolytopiaBackendBase.Game;

public static class TimeLimitExtensions
{
	public const int COUNT = 5;

	public static int GetRandomInSeconds(int randomInt)
	{
		return ((TimeLimit)(randomInt % 5 + 1)).ToSeconds();
	}

	public static int ToSeconds(this TimeLimit timeLimit)
	{
		return timeLimit switch
		{
			TimeLimit.VeryShort => 300, 
			TimeLimit.Short => 900, 
			TimeLimit.Medium => 3600, 
			TimeLimit.Long => 86400, 
			TimeLimit.VeryLong => 604800, 
			_ => 0, 
		};
	}

	public static TimeSpan ToTimeSpan(this TimeLimit timeLimit)
	{
		return TimeSpan.FromSeconds(timeLimit.ToSeconds());
	}

	public static TimeLimit TimeLimitFromSeconds(int seconds)
	{
		return seconds switch
		{
			300 => TimeLimit.VeryShort, 
			900 => TimeLimit.Short, 
			3600 => TimeLimit.Medium, 
			86400 => TimeLimit.Long, 
			604800 => TimeLimit.VeryLong, 
			0 => TimeLimit.None, 
			_ => TimeLimit.Live, 
		};
	}
}
