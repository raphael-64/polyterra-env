using System.Collections.Generic;
using Polytopia.Data;

public class WorldContinent
{
	private List<WorldCoordinates> tiles;

	private readonly int climate;

	private readonly SkinType skinType;

	private float crop = 0.5f;

	private float fish = 0.5f;

	private float fruit = 0.5f;

	private float game = 0.5f;

	private float metal = 0.8f;

	private float whale = 0.8f;

	private float spores;

	private float water;

	private float ocean;

	private float field = 0.5f;

	private float mountain = 0.15f;

	private float forest = 0.4f;

	private float ice = 0.5f;

	public bool hasAlienClimate;

	public int LandTileCount;

	public int Climate => climate;

	public SkinType SkinType => skinType;

	public List<WorldCoordinates> Tiles
	{
		get
		{
			if (tiles == null)
			{
				tiles = new List<WorldCoordinates>();
			}
			return tiles;
		}
		set
		{
			tiles = value;
		}
	}

	public WorldContinent(PlayerState player, int version, bool requiresFallbackClimate = false)
	{
		if (PolytopiaDataManager.GetGameLogicData(VersionManager.GetGameLogicDataVersionFromGameVersion(version)).TryGetData(player.tribe, out var data))
		{
			hasAlienClimate = data.HasAbility(TribeAbility.Type.AlienClimate);
			climate = ((hasAlienClimate && requiresFallbackClimate) ? 2 : data.climate);
			skinType = ((!(hasAlienClimate && requiresFallbackClimate)) ? player.skinType : SkinType.Default);
			SetTerrainModifiers(data.terrainModifier);
			SetResourceModifiers(data.resourceModifier);
			Tiles.Add(player.startTile);
		}
	}

	public void SetTerrainModifiers(Dictionary<TerrainData.Type, float> modifiers)
	{
		if (modifiers == null || modifiers.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<TerrainData.Type, float> modifier in modifiers)
		{
			switch (modifier.Key)
			{
			case TerrainData.Type.Water:
				if (modifier.Value > 1f)
				{
					water = 0.2f * modifier.Value;
				}
				break;
			case TerrainData.Type.Ocean:
				if (modifier.Value > 1f)
				{
					ocean *= modifier.Value;
				}
				break;
			case TerrainData.Type.Field:
				field *= modifier.Value;
				break;
			case TerrainData.Type.Mountain:
				mountain *= modifier.Value;
				break;
			case TerrainData.Type.Forest:
				forest *= modifier.Value;
				break;
			case TerrainData.Type.Ice:
				ice *= modifier.Value;
				break;
			}
		}
	}

	public void SetResourceModifiers(Dictionary<ResourceData.Type, float> modifiers)
	{
		if (modifiers == null || modifiers.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<ResourceData.Type, float> modifier in modifiers)
		{
			switch (modifier.Key)
			{
			case ResourceData.Type.Game:
				game = modify(game, modifier.Value);
				break;
			case ResourceData.Type.Crop:
				crop = modify(crop, modifier.Value);
				break;
			case ResourceData.Type.Fish:
				fish = modify(fish, modifier.Value);
				break;
			case ResourceData.Type.Whale:
				whale = modify(whale, modifier.Value);
				break;
			case ResourceData.Type.Metal:
				metal = modify(metal, modifier.Value);
				break;
			case ResourceData.Type.Fruit:
				fruit = modify(fruit, modifier.Value);
				break;
			case ResourceData.Type.Spores:
				spores = modify(spores, modifier.Value);
				break;
			}
		}
	}

	private float modify(float defaultValue, float modifier)
	{
		if (defaultValue > 0f)
		{
			return defaultValue * modifier;
		}
		return modifier;
	}

	public float GetModifier(TerrainData.Type type)
	{
		return type switch
		{
			TerrainData.Type.Water => water, 
			TerrainData.Type.Ocean => ocean, 
			TerrainData.Type.Field => field, 
			TerrainData.Type.Mountain => mountain, 
			TerrainData.Type.Forest => forest, 
			TerrainData.Type.Ice => ice, 
			_ => 0f, 
		};
	}

	public void SetModifier(TerrainData.Type type, float value)
	{
		switch (type)
		{
		case TerrainData.Type.Water:
			water = value;
			break;
		case TerrainData.Type.Ocean:
			ocean = value;
			break;
		case TerrainData.Type.Field:
			field = value;
			break;
		case TerrainData.Type.Mountain:
			mountain = value;
			break;
		case TerrainData.Type.Forest:
			forest = value;
			break;
		case TerrainData.Type.Ice:
			ice = value;
			break;
		case TerrainData.Type.None:
			break;
		}
	}

	public float GetModifier(ResourceData.Type type)
	{
		return type switch
		{
			ResourceData.Type.Game => game, 
			ResourceData.Type.Crop => crop, 
			ResourceData.Type.Fish => fish, 
			ResourceData.Type.Whale => whale, 
			ResourceData.Type.Metal => metal, 
			ResourceData.Type.Fruit => fruit, 
			ResourceData.Type.Spores => spores, 
			_ => 0f, 
		};
	}

	public void SetModifier(ResourceData.Type type, float value)
	{
		switch (type)
		{
		case ResourceData.Type.Game:
			game = value;
			break;
		case ResourceData.Type.Crop:
			crop = value;
			break;
		case ResourceData.Type.Fish:
			fish = value;
			break;
		case ResourceData.Type.Whale:
			whale = value;
			break;
		case ResourceData.Type.Metal:
			metal = value;
			break;
		case ResourceData.Type.Fruit:
			fruit = value;
			break;
		case ResourceData.Type.Spores:
			spores = value;
			break;
		case ResourceData.Type.None:
			break;
		}
	}
}
