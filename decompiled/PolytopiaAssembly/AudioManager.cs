using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public enum AudioSourceTypes
	{
		None,
		ThemeMusic,
		TribeMusic,
		Ambience,
		Ambience_1,
		Ambience_2,
		SFX,
		UI
	}

	[Serializable]
	public class AudioSourceData
	{
		public AudioSourceTypes id;

		public AudioSource audioSource;
	}

	[Serializable]
	public class TribeAudio
	{
		public TribeData.Type type;

		public AudioClip musicClip;

		public AudioClip ambienceClip;
	}

	public AudioMixer mixer;

	[SerializeField]
	protected AudioSourceData[] audioSourceData;

	[SerializeField]
	protected TribeAudio[] tribeAudioData;

	[SerializeField]
	protected AudioClip waterAmbience;

	[SerializeField]
	protected AudioSFXData SfxData;

	[SerializeField]
	protected float ambienceFadeInTime = 2.6f;

	[SerializeField]
	protected float ambienceFadeOutTime = 0.6f;

	[SerializeField]
	protected bool trackCameraMovement;

	protected static AudioManager instance;

	protected Dictionary<AudioSourceTypes, AudioSource> audioSources = new Dictionary<AudioSourceTypes, AudioSource>();

	protected Dictionary<AudioSourceTypes, Tween> audioFaders = new Dictionary<AudioSourceTypes, Tween>();

	protected Dictionary<TribeData.Type, int> tribeAudioMap = new Dictionary<TribeData.Type, int>();

	protected Dictionary<int, TribeData.Type> climateTribeMap = new Dictionary<int, TribeData.Type>();

	protected static Dictionary<string, float> maxVolumes = new Dictionary<string, float>();

	protected AudioSourceTypes currentAmbienceSource = AudioSourceTypes.Ambience_1;

	protected AudioSourceTypes fadeOutSource;

	protected int currentAmbienceClimate = -1;

	protected float ambienceUpdateDelay;

	protected AudioSourceTypes NextAmbienceSource => currentAmbienceSource switch
	{
		AudioSourceTypes.Ambience_1 => AudioSourceTypes.Ambience_2, 
		_ => AudioSourceTypes.Ambience_1, 
	};

	private void Awake()
	{
		instance = this;
		SetupMaxVolumes();
		SfxData.Init();
		SetupData();
		VolumeChanged();
	}

	private void OnEnable()
	{
		if (trackCameraMovement)
		{
			SetZoomLevel(CameraController.NormalizedZoom);
		}
	}

	private void OnDestroy()
	{
		foreach (Tween value in audioFaders.Values)
		{
			TweenUtils.KillTween(value, complete: true);
		}
	}

	protected void Update()
	{
		ambienceUpdateDelay -= Time.deltaTime;
		if (!trackCameraMovement || ambienceUpdateDelay > 0f || (Object)(object)MapRenderer.Current == (Object)null || GameManager.GameState == null)
		{
			return;
		}
		PlayerState localPlayer = GameManager.LocalPlayer;
		ambienceUpdateDelay = 0.5f;
		WorldCoordinates coordinates = CameraController.Coordinates;
		if (!(coordinates != WorldCoordinates.NULL_COORDINATES))
		{
			return;
		}
		Dictionary<int, float> dictionary = new Dictionary<int, float>();
		foreach (TileData item in GameManager.GameState.Map.GetArea(coordinates, 2, allowDiagonal: true))
		{
			if (item == null || !item.GetExplored(localPlayer.Id))
			{
				continue;
			}
			if (item.IsWater)
			{
				if (!dictionary.ContainsKey(0))
				{
					dictionary.Add(0, 0f);
				}
				dictionary[0] += 0.5f;
			}
			else
			{
				if (!dictionary.ContainsKey(item.climate))
				{
					dictionary.Add(item.climate, 0f);
				}
				dictionary[item.climate]++;
			}
		}
		int ambienceClimate = -1;
		float num = 0f;
		foreach (KeyValuePair<int, float> item2 in dictionary)
		{
			if (item2.Value > num)
			{
				num = item2.Value;
				ambienceClimate = item2.Key;
			}
		}
		SetAmbienceClimate(ambienceClimate);
	}

	protected void SetAmbienceClimate(int climate)
	{
		if (climate != currentAmbienceClimate && (!audioFaders.TryGetValue(fadeOutSource, out var value) || !TweenExtensions.IsActive(value) || !TweenExtensions.IsPlaying(value)))
		{
			AudioClip val = null;
			switch (climate)
			{
			case 0:
				val = waterAmbience;
				break;
			default:
				val = GetTribeAudio(climateTribeMap[climate]).ambienceClip;
				break;
			case -1:
				break;
			}
			fadeOutSource = currentAmbienceSource;
			FadeAudioSource(currentAmbienceSource, 0f, ambienceFadeOutTime, (Ease)2, delegate
			{
				fadeOutSource = AudioSourceTypes.None;
			});
			if ((Object)(object)val != (Object)null)
			{
				AudioSource audioSource = GetAudioSource(NextAmbienceSource);
				audioSource.clip = val;
				audioSource.time = Random.Range(0f, audioSource.clip.length);
				audioSource.Play();
				FadeAudioSource(NextAmbienceSource, 1f, ambienceFadeInTime, (Ease)3);
			}
			currentAmbienceSource = NextAmbienceSource;
			currentAmbienceClimate = climate;
		}
	}

	protected void SetupMaxVolumes()
	{
		float value = default(float);
		if (!maxVolumes.ContainsKey("MasterVolume") && instance.mixer.GetFloat("MasterVolume", ref value))
		{
			maxVolumes.Add("MasterVolume", value);
		}
		float value2 = default(float);
		if (!maxVolumes.ContainsKey("MusicVolume") && instance.mixer.GetFloat("MusicVolume", ref value2))
		{
			maxVolumes.Add("MusicVolume", value2);
		}
		float value3 = default(float);
		if (!maxVolumes.ContainsKey("SFXVolume") && instance.mixer.GetFloat("SFXVolume", ref value3))
		{
			maxVolumes.Add("SFXVolume", value3);
		}
		float value4 = default(float);
		if (!maxVolumes.ContainsKey("AmbienceVolume") && instance.mixer.GetFloat("AmbienceVolume", ref value4))
		{
			maxVolumes.Add("AmbienceVolume", value4);
		}
		float value5 = default(float);
		if (!maxVolumes.ContainsKey("AmbienceMasterVolume") && instance.mixer.GetFloat("AmbienceMasterVolume", ref value5))
		{
			maxVolumes.Add("AmbienceMasterVolume", value5);
		}
	}

	protected void SetupData()
	{
		AudioSourceData[] array = this.audioSourceData;
		foreach (AudioSourceData audioSourceData in array)
		{
			audioSources.Add(audioSourceData.id, audioSourceData.audioSource);
		}
		for (int j = 0; j < tribeAudioData.Length; j++)
		{
			TribeData.Type type = tribeAudioData[j].type;
			tribeAudioMap.Add(type, j);
			if (PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).TryGetData(type, out var data) && data.category != TribeData.CategoryEnum.Hidden)
			{
				climateTribeMap.Add(data.climate, type);
			}
		}
	}

	public static bool ShouldPlayTribeMusic()
	{
		if (SettingsUtils.TribeMusic)
		{
			return NativeHelpers.GetPlatformAudioState() == PlatformAudioState.MuteNone;
		}
		return false;
	}

	public static bool ShouldPlaySoundEffects()
	{
		if (SettingsUtils.SoundEffects)
		{
			return NativeHelpers.GetPlatformAudioState() != PlatformAudioState.MuteAll;
		}
		return false;
	}

	public static bool ShouldPlayAmbience()
	{
		if (SettingsUtils.Ambience)
		{
			return NativeHelpers.GetPlatformAudioState() != PlatformAudioState.MuteAll;
		}
		return false;
	}

	public static void VolumeChanged()
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.mixer.SetFloat("MasterVolume", GetLogarithmicValue(SettingsUtils.Volume, "MasterVolume"));
			instance.mixer.SetFloat("MusicVolume", GetLogarithmicValue(ShouldPlayTribeMusic() ? 1 : 0, "MusicVolume"));
			instance.mixer.SetFloat("SFXVolume", GetLogarithmicValue(ShouldPlaySoundEffects() ? 1 : 0, "SFXVolume"));
			instance.mixer.SetFloat("AmbienceVolume", GetLogarithmicValue(ShouldPlayAmbience() ? 1 : 0, "AmbienceVolume"));
		}
	}

	public static void SetZoomLevel(float value)
	{
		instance.mixer.SetFloat("AmbienceMasterVolume", GetLogarithmicValue(Mathf.Lerp(1f, 0.5f, value), "AmbienceMasterVolume"));
	}

	public static AudioSource GetAudioSource(AudioSourceTypes id)
	{
		if ((Object)(object)instance == (Object)null)
		{
			return null;
		}
		AudioSource val = null;
		if (instance.audioSources.ContainsKey(id))
		{
			val = instance.audioSources[id];
		}
		else if (id == AudioSourceTypes.Ambience && instance.currentAmbienceSource != AudioSourceTypes.None)
		{
			val = instance.audioSources[instance.currentAmbienceSource];
		}
		if ((Object)(object)val != (Object)null)
		{
			val.pitch = 1f;
		}
		return val;
	}

	public static void FadeAudioSource(AudioSourceTypes id, float to, float time = 0.6f, Ease easing = (Ease)1, Action onComplete = null)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		if (instance.audioSources.ContainsKey(id))
		{
			if (time > 0f)
			{
				Tween value = null;
				if (instance.audioFaders.TryGetValue(id, out value))
				{
					TweenUtils.KillTween(value);
				}
				value = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(instance.audioSources[id].DOFade(to, time), easing), (TweenCallback)delegate
				{
					onComplete?.Invoke();
				});
				if (!instance.audioFaders.ContainsKey(id))
				{
					instance.audioFaders.Add(id, value);
				}
				else
				{
					instance.audioFaders[id] = value;
				}
			}
			else
			{
				if (instance.audioFaders.TryGetValue(id, out var value2))
				{
					TweenUtils.KillTween(value2, complete: true);
				}
				instance.audioSources[id].volume = to;
				onComplete?.Invoke();
			}
		}
		else if (id == AudioSourceTypes.Ambience)
		{
			FadeAudioSource(instance.currentAmbienceSource, to, time, (Ease)1);
		}
	}

	public static TribeAudio GetTribeAudio(TribeData.Type type)
	{
		if (instance.tribeAudioMap.ContainsKey(type))
		{
			return instance.tribeAudioData[instance.tribeAudioMap[type]];
		}
		return null;
	}

	public static AudioClip GetTribeMusic(string tribe, string skin = null)
	{
		AudioClip val = null;
		string text = "Audio/music/citysong_" + tribe;
		Log.Verbose("[flx] Load sound {0}, ({1})", new object[2] { text, skin });
		if (skin != null)
		{
			val = Resources.Load<AudioClip>(text + "_" + skin);
		}
		if ((Object)(object)val == (Object)null)
		{
			val = Resources.Load<AudioClip>(text);
		}
		return val;
	}

	public static void PlaySFX(SFXTypes id, float volume = 1f, float pitch = 1f, float pan = 0f)
	{
		AudioSource audioSource = GetAudioSource(AudioSourceTypes.SFX);
		if (!((Object)(object)audioSource == (Object)null))
		{
			audioSource.pitch = pitch;
			if (volume >= 1f)
			{
				volume = 1f;
			}
			audioSource.PlayOneShot(instance.SfxData.GetClip(id), volume);
		}
	}

	public static void PlaySFXAtTile(SFXTypes id, WorldCoordinates coordinates, float volume = 1f, float pitch = 1f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = CameraController.Camera.WorldToScreenPoint(Vector2.op_Implicit(coordinates.ToPosition()));
		PlaySFX(id, volume, pitch, GetPanFromPosition(Vector2.op_Implicit(val)));
	}

	public static float GetPanFromPosition(Vector2 screenPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return (screenPosition.x / (float)Screen.width - 0.5f) * 2f;
	}

	public static float GetLogarithmicValue(float p, string mixerKey)
	{
		maxVolumes.TryGetValue(mixerKey, out var value);
		return Mathf.Min(Mathf.Log(Mathf.Max(p, 0.0001f)) * 20f, value);
	}

	public static float GetMixerFloat(string name)
	{
		float result = default(float);
		if ((Object)(object)instance != (Object)null && instance.mixer.GetFloat(name, ref result))
		{
			return result;
		}
		return -1f;
	}
}
