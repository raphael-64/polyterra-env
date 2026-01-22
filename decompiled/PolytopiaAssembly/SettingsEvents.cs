public static class SettingsEvents
{
	public delegate void OnSettingsUpdatedEvent(SettingsUtils.SettingsType type);

	public static event OnSettingsUpdatedEvent OnSettingsUpdated;

	public static void SettingsUpdated(SettingsUtils.SettingsType type)
	{
		SettingsEvents.OnSettingsUpdated?.Invoke(type);
	}
}
