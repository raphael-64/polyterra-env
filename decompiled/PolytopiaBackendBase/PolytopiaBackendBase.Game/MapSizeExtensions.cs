namespace PolytopiaBackendBase.Game;

public static class MapSizeExtensions
{
	public const int NONE = 0;

	public const int TINY = 11;

	public const int SMALL = 14;

	public const int NORMAL = 16;

	public const int LARGE = 18;

	public const int HUGE = 20;

	public const int MASSIVE = 30;

	public const int COUNT = 5;

	public static int ToMapWidth(this MapSize mapSize)
	{
		return mapSize switch
		{
			MapSize.Tiny => 11, 
			MapSize.Small => 14, 
			MapSize.Normal => 16, 
			MapSize.Large => 18, 
			MapSize.Huge => 20, 
			MapSize.Massive => 30, 
			_ => 0, 
		};
	}

	public static MapSize MapSizeFromInt(int value)
	{
		return value switch
		{
			11 => MapSize.Tiny, 
			14 => MapSize.Small, 
			16 => MapSize.Normal, 
			18 => MapSize.Large, 
			20 => MapSize.Huge, 
			30 => MapSize.Massive, 
			_ => MapSize.None, 
		};
	}
}
