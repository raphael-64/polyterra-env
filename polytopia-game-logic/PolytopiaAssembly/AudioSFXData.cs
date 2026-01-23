using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "AudioSFXData", menuName = "AudioSFXData", order = 9003)]
public class AudioSFXData : ScriptableObject
{
	[Serializable]
	public class AudioClipData
	{
		public SFXTypes id;

		public AudioClip clip;
	}

	public List<AudioClipData> SFX;

	protected Dictionary<SFXTypes, int> SfxMap = new Dictionary<SFXTypes, int>();

	public void Init()
	{
		SfxMap.Clear();
		int count = SFX.Count;
		for (int i = 0; i < count; i++)
		{
			AudioClipData audioClipData = SFX[i];
			SfxMap.Add(audioClipData.id, i);
		}
	}

	public AudioClip GetClip(SFXTypes id)
	{
		if (id == SFXTypes.None || id == SFXTypes.Silence)
		{
			return null;
		}
		if (SfxMap.TryGetValue(id, out var value))
		{
			return SFX[value].clip;
		}
		Log.Error("AudioSFXData :: GetCip :: Couldn't find clip with id : {0}", new object[1] { id.ToString() });
		return null;
	}
}
