using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "PolytopiaDataHolder", menuName = "PolytopiaDataHolder", order = 10001)]
public class PolytopiaDataHolder : ScriptableObject, IPolytopiaDataProvider
{
	[Serializable]
	public class DataAsset
	{
		public int version;

		public string @namespace;

		public TextAsset data;
	}

	public DataAsset[] gameLogicDatas;

	public DataAsset avatarData;

	public TextAsset GetGameLogicTextAssetWithVersion(int version)
	{
		if (gameLogicDatas != null && gameLogicDatas.Length != 0)
		{
			for (int i = 0; i < gameLogicDatas.Length; i++)
			{
				if (gameLogicDatas[i].version == version)
				{
					return gameLogicDatas[i].data;
				}
			}
		}
		return null;
	}

	public string LoadGameLogicData(int version)
	{
		TextAsset gameLogicTextAssetWithVersion = GetGameLogicTextAssetWithVersion(version);
		if ((Object)(object)gameLogicTextAssetWithVersion == (Object)null)
		{
			return null;
		}
		return gameLogicTextAssetWithVersion.text;
	}

	public string LoadAvatarData(int version)
	{
		return avatarData.data.text;
	}
}
