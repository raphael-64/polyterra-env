using System;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : UIScreenBase
{
	[Header("Settings")]
	[SerializeField]
	protected RectTransform container;

	[SerializeField]
	protected VerticalLayoutGroup verticalList;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected ScrollRect scrollRect;

	[Header("Prefabs")]
	[SerializeField]
	protected SettingsSliderContainer volumeSliderPrefab;

	[SerializeField]
	protected SettingsToggleGroup toggleGrouprPrefab;

	[SerializeField]
	protected UIHorizontalList horizontalListPrefab;

	[SerializeField]
	protected SettingsButtonContainer singleButtonContainerPrefab;

	[SerializeField]
	protected UICollapsableContainer collapsableContainerPrefab;

	[SerializeField]
	protected SettingsToggleContainer toggleContainerPrefab;

	protected float totalHeight;

	protected SettingsSliderContainer volumeSlider;

	protected UIHorizontalList languageSelector;

	protected UIHorizontalList scaleSelector;

	public override void Init()
	{
		CreateVolumeSlider();
		CreateToggleContainer();
		CreateAdvancedSettings();
		base.Init();
	}

	public override void Show(bool instant = false)
	{
		bool flag = false;
		float threshold = 1E-05f;
		if (!MathUtils.IsApproximately(AudioManager.GetMixerFloat("MasterVolume"), AudioManager.GetLogarithmicValue(SettingsUtils.Volume, "MasterVolume"), threshold))
		{
			flag = true;
		}
		else if (!MathUtils.IsApproximately(AudioManager.GetMixerFloat("MusicVolume"), AudioManager.GetLogarithmicValue(AudioManager.ShouldPlayTribeMusic() ? 1 : 0, "MusicVolume"), threshold))
		{
			flag = true;
		}
		else if (!MathUtils.IsApproximately(AudioManager.GetMixerFloat("SFXVolume"), AudioManager.GetLogarithmicValue(AudioManager.ShouldPlaySoundEffects() ? 1 : 0, "SFXVolume"), threshold))
		{
			flag = true;
		}
		else if (!MathUtils.IsApproximately(AudioManager.GetMixerFloat("AmbienceVolume"), AudioManager.GetLogarithmicValue(AudioManager.ShouldPlayAmbience() ? 1 : 0, "AmbienceVolume"), threshold))
		{
			flag = true;
		}
		if (flag)
		{
			Log.Warning("Found audio volume discrepancy", Array.Empty<object>());
			GameManager.GetAnalyticsManager().SendEvent("AudioSettingsDiscrepancy", new Dictionary<string, object>
			{
				{
					"MasterVolumeMixer",
					AudioManager.GetMixerFloat("MasterVolume")
				},
				{
					"MasterVolumeSettings",
					AudioManager.GetLogarithmicValue(SettingsUtils.Volume, "MasterVolume")
				},
				{
					"MusicVolumeMixer",
					AudioManager.GetMixerFloat("MusicVolume")
				},
				{
					"MusicVolumeSettings",
					AudioManager.GetLogarithmicValue(AudioManager.ShouldPlayTribeMusic() ? 1 : 0, "MusicVolume")
				},
				{
					"SFXVolumeMixer",
					AudioManager.GetMixerFloat("SFXVolume")
				},
				{
					"SFXVolumeSettings",
					AudioManager.GetLogarithmicValue(AudioManager.ShouldPlaySoundEffects() ? 1 : 0, "MusicVolume")
				},
				{
					"AmbienceVolumeMixer",
					AudioManager.GetMixerFloat("AmbienceVolume")
				},
				{
					"AmbienceVolumeSettings",
					AudioManager.GetLogarithmicValue(AudioManager.ShouldPlayAmbience() ? 1 : 0, "MusicVolume")
				}
			});
		}
		base.Show(instant);
		if ((Object)(object)volumeSlider != (Object)null)
		{
			volumeSlider.Highlighted = true;
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(container);
		OnLayoutComplete(OnScreenUpdated);
	}

	private void CreateVolumeSlider()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		volumeSlider = Object.Instantiate<SettingsSliderContainer>(volumeSliderPrefab, (Transform)(object)container);
		volumeSlider.HeaderKey = "settings.volume";
		volumeSlider.SliderValueChangedCallback = OnVolumeChanged;
		volumeSlider.SliderValue = SettingsUtils.Volume;
		volumeSlider.Arguments = new string[1] { Mathf.Round(volumeSlider.SliderValue * 100f).ToString() };
		volumeSlider.Highlighted = true;
		totalHeight += volumeSlider.rectTransform.sizeDelta.y;
	}

	private void OnVolumeChanged(float value)
	{
		volumeSlider.Arguments = new string[1] { Mathf.Round(value * 100f).ToString() };
		SettingsUtils.Volume = value;
	}

	private void CreateToggleContainer()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		SettingsToggleGroup settingsToggleGroup = Object.Instantiate<SettingsToggleGroup>(toggleGrouprPrefab, (Transform)(object)container);
		totalHeight += settingsToggleGroup.rectTransform.sizeDelta.y;
		omnicursorDefaultAffix = settingsToggleGroup.soundEffectsContainer.Button.rectTransform;
	}

	private void CreateLanguageList(Transform parent = null)
	{
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		bool num = SystemManager.ShouldShowCustomLanguageOption();
		List<string> list = new List<string> { "Automatic", "English", "Français", "Deutsch", "Italiano", "Português", "Русский", "Español", "日本語", "한국어" };
		List<int> list2 = new List<int> { 1, 3, 7, 9, 6, 4, 5, 8, 11, 12 };
		if (GameManager.GetPurchaseManager().IsTribeUnlocked(TribeData.Type.Elyrion))
		{
			list.Add("∑∫ỹriȱŋ");
			list2.Add(10);
		}
		if (num)
		{
			list.Add("Custom...");
			list2.Add(2);
		}
		languageSelector = Object.Instantiate<UIHorizontalList>(horizontalListPrefab, (Transform)(((object)parent) ?? ((object)container)));
		languageSelector.UpdateScrollerOnHighlight = true;
		languageSelector.HeaderKey = "settings.language";
		languageSelector.SetIds(list2.ToArray());
		languageSelector.SetData(list.ToArray());
		languageSelector.SelectId(SettingsUtils.Language, instant: true);
		languageSelector.IndexSelectedCallback = LanguageChangedCallback;
		totalHeight += languageSelector.rectTransform.sizeDelta.y;
	}

	private void LanguageChangedCallback(int index)
	{
		int? idForIndex = languageSelector.GetIdForIndex(index);
		if (!idForIndex.HasValue)
		{
			Log.Error("index {0} has no id", new object[1] { index });
		}
		else if (idForIndex.Value == 2)
		{
			CustomLanguagePopup customLanguagePopup = PopupManager.GetCustomLanguagePopup();
			customLanguagePopup.customFileAddedCallback = OnCustomFileLoaded;
			customLanguagePopup.closeCallback = OnCustomLanguagePopupClosed;
			customLanguagePopup.Show();
		}
		else
		{
			SettingsUtils.Language = idForIndex.Value;
			UINavigationManager.Select(languageSelector.GetCurrentSelectable());
		}
	}

	private void OnCustomFileLoaded(bool success)
	{
		if (success)
		{
			SettingsUtils.Language = 2;
		}
	}

	private void OnCustomLanguagePopupClosed()
	{
		Log.Verbose("SettingsScreen :: OnCustomLanguagePopupClosed: {0}", new object[1] { SettingsUtils.Language });
		languageSelector.SelectId(SettingsUtils.Language);
		int? indexForId = languageSelector.GetIndexForId(SettingsUtils.Language);
		if (!indexForId.HasValue)
		{
			Log.Error("id {0} does not exist in selector", new object[1] { SettingsUtils.Language });
		}
		else
		{
			UINavigationManager.Select(languageSelector.GetSelectable(indexForId.Value));
		}
	}

	private LayoutElement CreateVerticalSpacer(float height, Transform parent)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("Spacer");
		RectTransform val2 = val.AddComponent<RectTransform>();
		LayoutElement obj = val.AddComponent<LayoutElement>();
		((Transform)val2).SetParent(parent);
		((Transform)val2).localScale = Vector2.op_Implicit(Vector2.one);
		obj.minHeight = height;
		return obj;
	}

	private HorizontalLayoutGroup CreateHorizontalLayoutGroup(float spacing, Transform parent)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("Row");
		RectTransform val2 = val.AddComponent<RectTransform>();
		HorizontalLayoutGroup obj = val.AddComponent<HorizontalLayoutGroup>();
		((Transform)val2).SetParent(parent);
		((Transform)val2).localScale = Vector3.one;
		((HorizontalOrVerticalLayoutGroup)obj).childControlHeight = false;
		((HorizontalOrVerticalLayoutGroup)obj).childControlWidth = false;
		((HorizontalOrVerticalLayoutGroup)obj).childForceExpandHeight = false;
		((HorizontalOrVerticalLayoutGroup)obj).childForceExpandWidth = false;
		((LayoutGroup)obj).childAlignment = (TextAnchor)1;
		((HorizontalOrVerticalLayoutGroup)obj).spacing = spacing;
		return obj;
	}

	private SettingsToggleContainer CreateSettingsButtonContainer(string localizationKey, bool currentValue, UIToggleButton.ToggleValueChange callback, Transform parent)
	{
		SettingsToggleContainer settingsToggleContainer = Object.Instantiate<SettingsToggleContainer>(toggleContainerPrefab, parent);
		settingsToggleContainer.header.localizationKey = localizationKey;
		settingsToggleContainer.Button.Value = currentValue;
		settingsToggleContainer.Button.OnValueChanged += callback;
		return settingsToggleContainer;
	}

	private void CreateAdvancedSettings()
	{
		CreateVerticalSpacer(20f, (Transform)(object)container);
		UICollapsableContainer uICollapsableContainer = Object.Instantiate<UICollapsableContainer>(collapsableContainerPrefab, (Transform)(object)container);
		CreateLanguageList((Transform)(object)uICollapsableContainer.GetContentRectTransform());
		CreateVerticalSpacer(20f, (Transform)(object)uICollapsableContainer.GetContentRectTransform());
		CreateScaleSetting((Transform)(object)uICollapsableContainer.GetContentRectTransform());
	}

	private void CreateScaleSetting(Transform parent = null)
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		UICanvasScalerHelper canvasScalerHelper = UIManager.Instance.GetCanvasScalerHelper();
		ScreenLimitData[] screenLimitData = canvasScalerHelper.GetScreenLimitData();
		scaleSelector = Object.Instantiate<UIHorizontalList>(horizontalListPrefab, (Transform)(((object)parent) ?? ((object)container)));
		scaleSelector.UpdateScrollerOnHighlight = true;
		int num = screenLimitData.Length + 1;
		int[] array = new int[num];
		string[] array2 = new string[num];
		string[] data = new string[num];
		array[0] = -1;
		array2[0] = "settings.scale.auto";
		for (int i = 0; i < screenLimitData.Length; i++)
		{
			ScreenLimitData screenLimitData2 = screenLimitData[i];
			array[i + 1] = i;
			array2[i + 1] = screenLimitData2.localizationKey;
		}
		int indexForLimit = canvasScalerHelper.GetIndexForLimit(SettingsUtils.ScaleLimit);
		scaleSelector.HeaderKey = "settings.scale";
		scaleSelector.SetIds(array);
		scaleSelector.SetData(data);
		scaleSelector.SetLocalizationKeys(array2);
		scaleSelector.SelectId(indexForLimit, instant: true);
		scaleSelector.IndexSelectedCallback = OnScaleSettingChanged;
		totalHeight += scaleSelector.rectTransform.sizeDelta.y;
	}

	private void OnScaleSettingChanged(int index)
	{
		ScreenLimitData[] screenLimitData = UIManager.Instance.GetCanvasScalerHelper().GetScreenLimitData();
		int? idForIndex = scaleSelector.GetIdForIndex(index);
		if (!idForIndex.HasValue)
		{
			Log.Error("index {0} has no id", new object[1] { index });
			return;
		}
		float scaleLimit = -1f;
		if (idForIndex >= 0 && idForIndex < screenLimitData.Length)
		{
			scaleLimit = screenLimitData[idForIndex.Value].limit;
		}
		SettingsUtils.ScaleLimit = scaleLimit;
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}
}
