using UnityEngine;

public class SettingsToggleGroup : UIBasicComponent
{
	public RectTransform topList;

	public RectTransform middleList;

	public RectTransform middleRowDivider;

	public RectTransform bottomList;

	[Header("Toggle Containers")]
	public SettingsToggleContainer soundEffectsContainer;

	public SettingsToggleContainer ambienceContainer;

	public SettingsToggleContainer tribeMusicContainer;

	public SettingsToggleContainer suggestionsContainer;

	public SettingsToggleContainer infoOnBuidContainer;

	public SettingsToggleContainer confirmTurnContainer;

	[Header("PC Specific")]
	public SettingsToggleContainer fullscreenContainer;

	public SettingsToggleContainer compactUIContainer;

	public SettingsToggleContainer antiAliasContainer;

	private void OnEnable()
	{
		soundEffectsContainer.Button.OnValueChanged += OnSFXValueChanged;
		ambienceContainer.Button.OnValueChanged += OnAmbiencealueChanged;
		tribeMusicContainer.Button.OnValueChanged += OnTribeMusicValueChanged;
		suggestionsContainer.Button.OnValueChanged += OnSuggestionValueChanged;
		infoOnBuidContainer.Button.OnValueChanged += OnInfoValueChanged;
		confirmTurnContainer.Button.OnValueChanged += OnConfirmTurnValueChanged;
		SystemEvents.OnFullscreenChanged += OnFullscreenChanged;
		fullscreenContainer.Button.OnValueChanged += OnFullscreenValueChanged;
		antiAliasContainer.Button.OnValueChanged += OnAntiAliasValueChanged;
		compactUIContainer.Button.OnValueChanged += OnCompactUIValueChanged;
		SetupContainers();
	}

	private void OnDisable()
	{
		soundEffectsContainer.Button.OnValueChanged -= OnSFXValueChanged;
		ambienceContainer.Button.OnValueChanged -= OnAmbiencealueChanged;
		tribeMusicContainer.Button.OnValueChanged -= OnTribeMusicValueChanged;
		suggestionsContainer.Button.OnValueChanged -= OnSuggestionValueChanged;
		infoOnBuidContainer.Button.OnValueChanged -= OnInfoValueChanged;
		confirmTurnContainer.Button.OnValueChanged -= OnConfirmTurnValueChanged;
		SystemEvents.OnFullscreenChanged -= OnFullscreenChanged;
		fullscreenContainer.Button.OnValueChanged -= OnFullscreenValueChanged;
		compactUIContainer.Button.OnValueChanged -= OnCompactUIValueChanged;
	}

	private void SetupContainers()
	{
		soundEffectsContainer.Value = SettingsUtils.SoundEffects;
		ambienceContainer.Value = SettingsUtils.Ambience;
		tribeMusicContainer.Value = SettingsUtils.TribeMusic;
		suggestionsContainer.Value = SettingsUtils.Suggestions;
		infoOnBuidContainer.Value = SettingsUtils.InfoOnBuild;
		confirmTurnContainer.Value = SettingsUtils.ConfirmTurn;
		((Component)middleList).gameObject.SetActive(true);
		((Component)middleRowDivider).gameObject.SetActive(true);
		((Component)compactUIContainer).gameObject.SetActive(false);
		compactUIContainer.Value = SettingsUtils.UseCompactUI;
		antiAliasContainer.Value = SettingsUtils.UseAntiAliasing;
		fullscreenContainer.Value = Screen.fullScreen;
	}

	private void OnSFXValueChanged(bool value)
	{
		SettingsUtils.SoundEffects = value;
	}

	private void OnAmbiencealueChanged(bool value)
	{
		SettingsUtils.Ambience = value;
	}

	private void OnTribeMusicValueChanged(bool value)
	{
		SettingsUtils.TribeMusic = value;
	}

	private void OnSuggestionValueChanged(bool value)
	{
		SettingsUtils.Suggestions = value;
	}

	private void OnInfoValueChanged(bool value)
	{
		SettingsUtils.InfoOnBuild = value;
	}

	private void OnConfirmTurnValueChanged(bool value)
	{
		SettingsUtils.ConfirmTurn = value;
	}

	private void OnFullscreenValueChanged(bool value)
	{
		Screen.fullScreen = value;
	}

	private void OnFullscreenChanged(FullScreenMode mode)
	{
		fullscreenContainer.Value = Screen.fullScreen;
	}

	private void OnCompactUIValueChanged(bool value)
	{
		SettingsUtils.UseCompactUI = value;
	}

	private void OnAntiAliasValueChanged(bool value)
	{
		SettingsUtils.UseAntiAliasing = value;
		GraphicsUtils.SetMSAAEnabled(SettingsUtils.UseAntiAliasing);
	}
}
