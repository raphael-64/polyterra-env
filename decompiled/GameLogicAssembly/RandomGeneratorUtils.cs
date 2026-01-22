using System;

public static class RandomGeneratorUtils
{
	public static float Value(this Random rng)
	{
		return (float)(rng.NextDouble() * 1.0);
	}

	public static float Range(this Random rng, float min, float max)
	{
		return (float)(rng.NextDouble() * (double)(max - min) + (double)min);
	}

	public static int Range(this Random rng, int max)
	{
		return rng.Next(max);
	}

	public static int Range(this Random rng, int min, int max)
	{
		return rng.Next(min, max);
	}
}
