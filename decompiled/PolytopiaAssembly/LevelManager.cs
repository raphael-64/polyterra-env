using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	private static LevelManager instance;

	[SerializeField]
	protected Transform tileHolder;

	[SerializeField]
	protected Transform unitHolder;

	[SerializeField]
	private ClientInteraction clientInteraction;

	public static Transform TileHolder => instance.tileHolder;

	public static Transform UnitHolder => instance.unitHolder;

	private void Start()
	{
		if (!Object.op_Implicit((Object)(object)instance))
		{
			instance = this;
			Log.Info("[LevelManager] Start", Array.Empty<object>());
			GameManager.Instance.OnLevelLoaded();
			StartAmbience();
		}
	}

	public static void PreloadAtlases(Action completion)
	{
		List<string> atlasesUsedInGame = new List<string>();
		atlasesUsedInGame.Add("TerrainBuildings");
		atlasesUsedInGame.Add("TerrainFeatures");
		atlasesUsedInGame.Add("Units");
		atlasesUsedInGame.Add("Overlays");
		GameManager.GetSpriteAtlasManager().PreloadSpriteAtlases(atlasesUsedInGame, 10, delegate(bool success)
		{
			if (!success)
			{
				throw new Exception(string.Format("Failed to load atlases used in game {0}", string.Join(", ", atlasesUsedInGame)));
			}
			completion?.Invoke();
		});
	}

	public static ClientInteraction GetClientInteraction()
	{
		return instance.clientInteraction;
	}

	private void StartAmbience()
	{
	}
}
