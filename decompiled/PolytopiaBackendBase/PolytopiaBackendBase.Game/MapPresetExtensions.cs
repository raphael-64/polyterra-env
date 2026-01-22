namespace PolytopiaBackendBase.Game;

public static class MapPresetExtensions
{
	public const int COUNT = 5;

	public static string GetLocalizationName(this MapPreset mapPreset)
	{
		return mapPreset switch
		{
			MapPreset.Dryland => "gamesettings.map.dryland", 
			MapPreset.Lakes => "gamesettings.map.lakes", 
			MapPreset.Continents => "gamesettings.map.continents", 
			MapPreset.Archipelago => "gamesettings.map.archipelago", 
			MapPreset.WaterWorld => "gamesettings.map.waterworld", 
			_ => "gamesettings.unknown", 
		};
	}
}
