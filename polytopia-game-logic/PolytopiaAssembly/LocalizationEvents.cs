public static class LocalizationEvents
{
	public delegate void OnLanguageChangedEvent(Localization.Languages language);

	public delegate void OnLocalizationUpdatedEvent();

	public static event OnLanguageChangedEvent OnLanguageChanged;

	public static event OnLocalizationUpdatedEvent OnLocalizationUpdated;

	public static void LanguageChanged(Localization.Languages language)
	{
		LocalizationEvents.OnLanguageChanged?.Invoke(language);
	}

	public static void LocalizationUpdated()
	{
		LocalizationEvents.OnLocalizationUpdated?.Invoke();
	}
}
