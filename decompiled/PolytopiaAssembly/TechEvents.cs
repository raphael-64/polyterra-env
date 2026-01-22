using Polytopia.Data;

public static class TechEvents
{
	public delegate void OnTechCompletedEvent(TechData tech);

	public delegate void OnRefreshAllTechEvent();

	public static event OnTechCompletedEvent OnTechCompleted;

	public static event OnRefreshAllTechEvent OnRefreshAllTech;

	public static void TechCompleted(TechData tech)
	{
		TechEvents.OnTechCompleted?.Invoke(tech);
		RefreshAllTech();
	}

	public static void RefreshAllTech()
	{
		TechEvents.OnRefreshAllTech?.Invoke();
	}
}
