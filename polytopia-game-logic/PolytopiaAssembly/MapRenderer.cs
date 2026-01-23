using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Polytopia.Data;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
	public const string LOG_PREFIX = "<color=#dce54d>[MapRenderer]</color>";

	public const float TILE_WIDTH = 0.9622f;

	public const float TILE_HEIGHT = 0.576f;

	public const float TILE_WIDTH_HALF = 0.4811f;

	public const float TILE_HEIGHT_HALF = 0.288f;

	public const float TILE_HORIZONTAL_OFFSET = 0f;

	public const float TILE_VERTICAL_OFFSET = -0.223f;

	public const int DEPTH_INCREASE_PER_ROW = 100;

	public const int BORDERS_BACK_SORT_OFFSET = 0;

	public const int TERRAIN_SORT_OFFSET = 1;

	public const int TRANSPORT_SORT_OFFSET = 2;

	public const int WORLD_OBJECT_SORT_OFFSET = 2;

	public const int TERRAIN_FEATURE_SORT_OFFSET = 3;

	public const int RESOURCES_OUTLINE_SORT_OFFSET = 4;

	public const int RESOURCES_SORT_OFFSET = 5;

	public const int HOUSES_SORT_OFFSET = 6;

	public const int WALLS_SORT_OFFSET = 97;

	public const int BUILDINGS_SORT_OFFSET = 98;

	public const int BORDERS_FRONT_SORT_OFFSET = 99;

	private static MapRenderer instance;

	[Header("Components")]
	[SerializeField]
	protected Tile tilePrefab;

	[SerializeField]
	protected Material tileMaterial;

	[Header("Settings")]
	[Header("MapGeneration")]
	[Range(11f, 64f)]
	[SerializeField]
	private ushort mapSize = 11;

	[SerializeField]
	private MapGeneratorSettings mapSettings;

	private Tile[] renderedTiles;

	private List<Unit> renderedUnits = new List<Unit>();

	private Coroutine findMapWithPrerequisit;

	private long generationCount;

	private MaterialPropertyBlock tilePropertyBlock;

	public static MapRenderer Current => instance;

	public bool IsRendered
	{
		get
		{
			if (renderedTiles != null)
			{
				return renderedTiles.Length != 0;
			}
			return false;
		}
	}

	public List<Unit> RenderedUnits => renderedUnits;

	private void Awake()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)instance))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		instance = this;
		DebugConsole.AddCommand("map_clear", new CommandDelegate(CmdClearMap), "Clear the current map");
		DebugConsole.AddCommand("map_reveal", new CommandDelegate(CmdRevealMap), "Reveal the entire map");
		DebugConsole.AddCommand("map_refresh", new CommandDelegate(CmdRefreshMap), "Refresh the current map");
		DebugConsole.AddCommand("map_generate", new CommandDelegate(CmdMapGenerateGame), "Generate a map from a seed if given");
		DebugConsole.AddCommand("map_seed", new CommandDelegate(CmdMapSeedGame), "Get current map seed");
		if (GameManager.GameState?.Settings != null)
		{
			mapSettings = GameManager.GameState.Settings.GetMapGeneratorSettings();
			mapSize = (ushort)GameManager.GameState.Settings.MapSize;
		}
		tilePropertyBlock = GameManager.GetMeshCache().GetOrCreateMaterialPropertyBlock("TerrainFeatures");
	}

	private void OnDestroy()
	{
		DebugConsole.RemoveCommand("map_clear");
		DebugConsole.RemoveCommand("map_reveal");
		DebugConsole.RemoveCommand("map_refresh");
		DebugConsole.RemoveCommand("map_generate");
		DebugConsole.RemoveCommand("map_seed");
		instance = null;
	}

	private void Update()
	{
	}

	public void LateUpdate()
	{
		GameManager.GetSpriteRendererManager().Update();
		if (renderedTiles == null)
		{
			return;
		}
		int num = 100;
		for (int i = 0; i < renderedTiles.Length; i++)
		{
			Tile tile = renderedTiles[i];
			if (tile.isDirty && num-- > 0)
			{
				tile.BatchSprites();
			}
			else if (tile.isDirty)
			{
				tile.Unbatch();
			}
		}
	}

	public void RenderMap(MapData mapData)
	{
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		Stopwatch stopwatch = Stopwatch.StartNew();
		stopwatch.Start();
		ClearUnits();
		Log.Verbose("{0} Render map ({1}x{2}) tilecount {3}", new object[4]
		{
			"<color=#dce54d>[MapRenderer]</color>",
			mapData.Width,
			mapData.Height,
			mapData.Tiles.Length
		});
		bool flag = false;
		if (renderedTiles == null || renderedTiles.Length != mapData.Tiles.Length)
		{
			if (renderedTiles != null)
			{
				Tile[] array = renderedTiles;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].DestroyInstance();
				}
			}
			renderedTiles = new Tile[mapData.Tiles.Length];
			flag = true;
		}
		for (int j = 0; j < mapData.Tiles.Length; j++)
		{
			if ((Object)(object)renderedTiles[j] == (Object)null)
			{
				flag = true;
				Tile tile = Object.Instantiate<Tile>(tilePrefab, LevelManager.TileHolder);
				renderedTiles[j] = tile;
				tile.SetMaterial(tileMaterial);
				tile.SetMaterialPropertyBlock(tilePropertyBlock);
			}
			TileData tileData = mapData.Tiles[j];
			Tile obj = renderedTiles[j];
			((Object)obj).name = $"Tile {tileData.coordinates}";
			obj.Data = tileData;
			obj.Depth = mapData.Height - (tileData.coordinates.X + tileData.coordinates.Y) * 100;
			obj.Position = Vector2.op_Implicit(tileData.coordinates.ToPosition());
			obj.Render();
		}
		stopwatch.Stop();
		Log.Verbose("{0} Map rendered in {1} seconds", new object[2]
		{
			"<color=#dce54d>[MapRenderer]</color>",
			(float)stopwatch.ElapsedMilliseconds / 1000f
		});
		if (flag)
		{
			InputEvents.SelectionCleared();
		}
		else
		{
			LevelManager.GetClientInteraction()?.UpdateAfterMapRefresh();
		}
	}

	public void Refresh()
	{
		RefreshUnitPositions();
		if (renderedTiles != null)
		{
			Log.Verbose("{0} Refreshing map...", new object[1] { "<color=#dce54d>[MapRenderer]</color>" });
			for (int i = 0; i < GameManager.GameState.Map.Tiles.Length; i++)
			{
				renderedTiles[i].Render();
			}
		}
		LevelManager.GetClientInteraction()?.UpdateAfterMapRefresh();
	}

	public void RefreshUnitPositions()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (renderedUnits == null || renderedUnits.Count == 0)
		{
			return;
		}
		foreach (Unit renderedUnit in renderedUnits)
		{
			Tile tileInstance = GetTileInstance(renderedUnit.State.coordinates);
			if ((Object)(object)tileInstance.Unit != (Object)(object)renderedUnit)
			{
				renderedUnit.Tile = tileInstance;
				tileInstance.Unit = renderedUnit;
				((Component)renderedUnit).transform.position = Vector2.op_Implicit(renderedUnit.State.coordinates.ToPosition());
			}
		}
	}

	public void Clear()
	{
		if (renderedTiles != null)
		{
			Log.Info("{0} Clearing map...", new object[1] { "<color=#dce54d>[MapRenderer]</color>" });
			for (int i = 0; i < renderedTiles.Length; i++)
			{
				Tile tile = renderedTiles[i];
				if (Object.op_Implicit((Object)(object)tile))
				{
					tile.DestroyInstance();
				}
			}
			renderedTiles = null;
		}
		ClearUnits();
	}

	private void ClearUnits()
	{
		if (renderedUnits != null)
		{
			for (int i = 0; i < renderedUnits.Count; i++)
			{
				renderedUnits[i--].Destroy();
			}
			renderedUnits.Clear();
		}
	}

	public Tile GetTileInstance(WorldCoordinates coordinates)
	{
		if (GameManager.GameState == null || GameManager.GameState.Map == null || renderedTiles == null)
		{
			return null;
		}
		int tileIndex = GameManager.GameState.Map.GetTileIndex(coordinates);
		if (tileIndex == -1 || tileIndex >= renderedTiles.Length)
		{
			return null;
		}
		return renderedTiles[tileIndex];
	}

	public Unit GetUnitInstance(uint id)
	{
		foreach (Unit renderedUnit in renderedUnits)
		{
			if (renderedUnit.State.id == id)
			{
				return renderedUnit;
			}
		}
		return null;
	}

	public void AddUnit(Unit unit)
	{
		renderedUnits.Add(unit);
	}

	public void RemoveUnit(Unit unit)
	{
		renderedUnits.Remove(unit);
	}

	public Bounds GetWorldBounds()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.GameState == null || GameManager.GameState.Map == null)
		{
			return new Bounds(Vector3.zero, Vector3.one);
		}
		float num = (float)(int)GameManager.GameState.Map.Width * 0.9622f;
		float num2 = (float)(int)GameManager.GameState.Map.Height * 0.576f;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(num, num2 + 0.576f, 0f);
		return new Bounds(((Component)this).transform.position + new Vector3(0f, (num2 - 0.576f) * 0.5f, 0f), val);
	}

	public void CmdRevealMap(string[] args)
	{
		Log.Info("{0} Revealing map...", new object[1] { "<color=#dce54d>[MapRenderer]</color>" });
		if (GameManager.GameState.Map != null)
		{
			for (int i = 0; i < GameManager.GameState.Map.Tiles.Length; i++)
			{
				GameManager.GameState.Map.Tiles[i].SetExplored(GameManager.LocalPlayer.Id, explored: true);
			}
			Refresh();
		}
	}

	private void CmdRefreshMap(string[] args)
	{
		Refresh();
	}

	private void CmdClearMap(string[] args)
	{
		Clear();
	}

	private void CmdMapGenerateGame(string[] args)
	{
		if (args.Length == 1)
		{
			int.TryParse(args[0], out var result);
			GenerateMapFromConsole(result);
			Clear();
			RenderMap(GameManager.Client.GameState.Map);
			CmdRevealMap(null);
		}
	}

	private void CmdMapSeedGame(string[] args)
	{
		DebugConsole.Write("Map Seed: {0}", new object[1] { GameManager.GameState.Seed });
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)GameManager.Instance != (Object)null && GameManager.GameState != null && GameManager.GameState.Map != null)
		{
			Gizmos.color = Color.red;
			DebugDraw.BoundsGizmo(GetWorldBounds());
		}
	}

	private IEnumerator CalculateDistanceProbability()
	{
		int generationCount = 1000;
		MapGeneratorSettings mapSettings = GameManager.GameState.Settings.GetMapGeneratorSettings();
		ushort mapSize = (ushort)GameManager.GameState.Settings.MapSize;
		int[] buckets = new int[mapSize];
		while (generationCount-- > 0)
		{
			yield return (object)new WaitForFixedUpdate();
			int value = Random.Range(int.MinValue, int.MaxValue);
			GenerateMap(mapSettings, mapSize, value);
			GameManager.GameState.TryGetPlayer(1, out var playerState);
			GameManager.GameState.TryGetPlayer(2, out var playerState2);
			int num = MapDataExtensions.ChebyshevDistance(playerState.startTile, playerState2.startTile);
			buckets[num]++;
		}
		Clear();
		RenderMap(GameManager.Client.GameState.Map);
		CmdRevealMap(null);
		string text = "Result: \n";
		for (int i = 0; i < buckets.Length; i++)
		{
			text += $"{i}: {buckets[i]}\n";
		}
		Log.Verbose(text, Array.Empty<object>());
		yield return null;
	}

	private IEnumerator FindMapWithPrerequisit(Func<bool> resultCondition)
	{
		generationCount = 0L;
		int num = 0;
		bool flag = false;
		MapGeneratorSettings mapSettings = GameManager.GameState.Settings.GetMapGeneratorSettings();
		ushort mapSize = (ushort)GameManager.GameState.Settings.MapSize;
		while (!flag)
		{
			yield return (object)new WaitForFixedUpdate();
			generationCount++;
			num = Random.Range(int.MinValue, int.MaxValue);
			GenerateMap(mapSettings, mapSize, num);
			flag = resultCondition();
		}
		Clear();
		RenderMap(GameManager.Client.GameState.Map);
		CmdRevealMap(null);
		Debug.Log((object)"<color=yellow>New Map Generated</color>");
		Debug.Log((object)$"<color=yellow>Found Generation Condition: {flag}</color>");
		Debug.Log((object)$"<color=yellow>Seed: {num}</color>");
		Debug.Log((object)$"<color=yellow>Generations made: {generationCount}</color>");
		yield return null;
	}

	private bool CheckForShallowWaterThatShouldBeOcean()
	{
		for (int i = 0; i < GameManager.GameState.Map.Tiles.Length; i++)
		{
			TileData tileData = GameManager.GameState.Map.Tiles[i];
			if (tileData.HasResource(ResourceData.Type.Fish) && tileData.altitude != -1)
			{
				return true;
			}
			if (tileData.terrain != TerrainData.Type.Water)
			{
				continue;
			}
			List<TileData> area = GameManager.GameState.Map.GetArea(tileData.coordinates, 1, allowDiagonal: false, includeCenter: false);
			bool flag = false;
			for (int j = 0; j < area.Count; j++)
			{
				if (area[j].altitude > 0)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckForCloseRuins()
	{
		for (int i = 0; i < GameManager.GameState.Map.Tiles.Length; i++)
		{
			TileData tileData = GameManager.GameState.Map.Tiles[i];
			if (!tileData.HasImprovement(ImprovementData.Type.Ruin))
			{
				continue;
			}
			List<TileData> area = GameManager.GameState.Map.GetArea(tileData.coordinates, 2, allowDiagonal: true, includeCenter: false);
			for (int j = 0; j < area.Count; j++)
			{
				if (area[j].HasImprovement(ImprovementData.Type.Ruin))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CheckForMissingWaterTilesKickoo()
	{
		List<TileData> area = GameManager.GameState.Map.GetArea(GameManager.LocalPlayer.startTile, 1, allowDiagonal: true);
		int num = 0;
		foreach (TileData item in area)
		{
			if (item.terrain == TerrainData.Type.Water)
			{
				num++;
			}
		}
		if (num < 2)
		{
			return true;
		}
		return false;
	}

	private bool CheckForWhaleInWater()
	{
		TileData[] tiles = GameManager.GameState.Map.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (tileData.IsWater && tileData.HasResource(ResourceData.Type.Whale) && tileData.terrain == TerrainData.Type.Water)
			{
				return true;
			}
		}
		return false;
	}

	private void GenerateMapFromConsole(int seed)
	{
		GenerateMap(GameManager.GameState.Settings.GetMapGeneratorSettings(), (ushort)GameManager.GameState.Settings.MapSize, seed);
	}

	private void GenerateMapFromSerialized()
	{
		GenerateMap(mapSettings, mapSize);
	}

	private void GenerateMap(MapGeneratorSettings mapSettings, ushort mapSize, int? seed = null)
	{
		GameManager.GameState.Map = new MapData(mapSize, mapSize);
		MapGenerator mapGenerator = new MapGenerator();
		if (seed.HasValue)
		{
			GameManager.GameState.Seed = seed.Value;
			mapGenerator.GenerateWithSeed(seed.Value, GameManager.GameState, mapSettings);
		}
		else
		{
			mapGenerator.Generate(GameManager.GameState, mapSettings);
		}
	}
}
