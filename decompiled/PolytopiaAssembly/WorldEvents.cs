public class WorldEvents
{
	public delegate void OnCityCapturedEvent(WorldCoordinates coordinates);

	public delegate void OnSunriseVisibilityEvent(bool showing);

	public static event OnCityCapturedEvent OnCityCaptured;

	public static event OnSunriseVisibilityEvent OnSunriseVisibility;

	public static void CityCaptured(WorldCoordinates coordinates)
	{
		WorldEvents.OnCityCaptured?.Invoke(coordinates);
	}

	public static void SunriseVisibility(bool showing)
	{
		WorldEvents.OnSunriseVisibility?.Invoke(showing);
	}
}
