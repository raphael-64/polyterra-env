using System;
using Mock;
using PolytopiaBackendBase.Game;

[Serializable]
public class MapGeneratorSettings
{
	public const float DEFAULT_RICHNESS = 1f;

	public const float DEFAULT_WETNESS = 0.55f;

	public const int DEFAULT_SMOOTH_ITERATIONS = 3;

	public const float DEFAULT_PRE_TERRAIN_CITY_DENSITY = 0.3f;

	public const float DEFAULT_POST_TERRAIN_CITY_DENSITY = 0.6f;

	public const int DEFAULT_MIN_SUBURB_COUNT = 1;

	public const int DEFAULT_MAX_SUBURB_COUNT = 2;

	public const float DEFAULT_EMPTY_SPACE_VALUE = 0.5f;

	public const float DEFAULT_SHALLOW_PERCENT_OF_WATER = 0f;

	[Range(0f, 1f)]
	public float wetness = 0.55f;

	[Range(0f, 1f)]
	public float richness = 1f;

	[Range(0f, 10f)]
	public int smoothIterations = 3;

	[Range(0f, 1f)]
	public float preTerrainCityDensity = 0.3f;

	[Range(0f, 1f)]
	public float postTerrainCityDensity = 0.6f;

	[Range(0f, 5f)]
	public int minSuburbCount = 1;

	[Range(0f, 5f)]
	public int maxSuburbCount = 2;

	[Range(0f, 1f)]
	public float surroundingSpaceValue = 0.5f;

	[Range(0f, 1f)]
	public float shallowPercentOfWater;

	public int equalityIterations = 15;

	public float equalityLimit = 1.15f;

	public override string ToString()
	{
		return $"MapGeneratorSettings: wetness {wetness}, richness {richness}, smoothIterations {smoothIterations}";
	}

	public static MapGeneratorSettings CreateFromPreset(MapPreset mapPreset)
	{
		MapGeneratorSettings mapGeneratorSettings = new MapGeneratorSettings();
		switch (mapPreset)
		{
		case MapPreset.Dryland:
			mapGeneratorSettings.wetness = 0f;
			mapGeneratorSettings.smoothIterations = 0;
			mapGeneratorSettings.preTerrainCityDensity = 0f;
			mapGeneratorSettings.postTerrainCityDensity = 0.8f;
			mapGeneratorSettings.maxSuburbCount = 0;
			mapGeneratorSettings.minSuburbCount = 0;
			break;
		case MapPreset.Lakes:
			mapGeneratorSettings.wetness = 0.3f;
			mapGeneratorSettings.smoothIterations = 2;
			mapGeneratorSettings.surroundingSpaceValue = 1f;
			break;
		case MapPreset.Archipelago:
			mapGeneratorSettings.wetness = 0.65f;
			mapGeneratorSettings.smoothIterations = 0;
			break;
		case MapPreset.WaterWorld:
			mapGeneratorSettings.wetness = 0.85f;
			mapGeneratorSettings.smoothIterations = 2;
			mapGeneratorSettings.preTerrainCityDensity = 0.1f;
			mapGeneratorSettings.maxSuburbCount = 0;
			mapGeneratorSettings.minSuburbCount = 0;
			break;
		}
		return mapGeneratorSettings;
	}
}
