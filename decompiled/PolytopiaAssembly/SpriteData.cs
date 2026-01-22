using Polytopia.Data;
using UnityEngine;

public static class SpriteData
{
	public const string SPRITE_FOLDER = "Sprites/";

	public const string TILE_FOLDER = "Tile/";

	public const string TILE_FIELD = "ground";

	public const string TILE_MOUNTAIN = "mountain";

	public const string TILE_WATER = "water";

	public const string TILE_WATER_CORNER = "water_wall_left_wall_right";

	public const string TILE_WATER_LEFT = "water_wall_left";

	public const string TILE_WATER_RIGHT = "water_wall_right";

	public const string TILE_OCEAN = "ocean";

	public const string TILE_OCEAN_CORNER = "ocean_wall_left_wall_right";

	public const string TILE_OCEAN_LEFT = "ocean_wall_left";

	public const string TILE_OCEAN_RIGHT = "ocean_wall_right";

	public const string TILE_ICE = "ice";

	public const string TILE_UNKNOWN = "hidden";

	public const string RESOURCE_FOLDER = "Resource/";

	public const string FOREST_FOLDER = "Forest/";

	public const string RESOURCE_FOREST = "Forest";

	public const string RESOURCE_FRUIT = "fruit";

	public const string RESOURCE_GAME = "animal_";

	public const string RESOURCE_CROP = "crop";

	public const string RESOURCE_FISH = "fish";

	public const string RESOURCE_WHALE = "whale";

	public const string RESOURCE_METAL = "metal";

	public const string RESOURCE_SPORES = "spores";

	public const string IMPROVEMENT_FOLDER = "Building/";

	public const string IMPROVEMENT_BURN_FOREST = "Burn Forest";

	public const string IMPROVEMENT_BURN_SPORES = "Burn Spores";

	public const string IMPROVEMENT_CLEAR_FOREST = "Clear forest";

	public const string IMPROVEMENT_CUSTOMS_HOUSE = "Customs House_1";

	public const string IMPROVEMENT_FARM = "Farm";

	public const string IMPROVEMENT_FORGE = "Forge_1";

	public const string IMPROVEMENT_GROW_FOREST = "Grow Forest";

	public const string IMPROVEMENT_LUMBER_HUT = "Lumber Hut";

	public const string IMPROVEMENT_MINE = "Mine";

	public const string IMPROVEMENT_MONUMENT_1 = "Monument1";

	public const string IMPROVEMENT_MONUMENT_2 = "Monument2";

	public const string IMPROVEMENT_MONUMENT_3 = "Monument3";

	public const string IMPROVEMENT_MONUMENT_4 = "Monument4";

	public const string IMPROVEMENT_MONUMENT_5 = "Monument5";

	public const string IMPROVEMENT_MONUMENT_6 = "Monument6";

	public const string IMPROVEMENT_MONUMENT_7 = "Monument7";

	public const string IMPROVEMENT_PORT = "Port";

	public const string IMPROVEMENT_SANCTUARY = "sanctuary_1";

	public const string IMPROVEMENT_SAWMILL = "Sawmill_1";

	public const string IMPROVEMENT_WINDMILL = "Windmill_1";

	public const string IMPROVEMENT_ICE_PORT = "iceport";

	public const string IMPROVEMENT_ICE_BANK = "ice_bank_1";

	public const string IMPROVEMENT_TEMPLE = "Temple_1";

	public const string IMPROVEMENT_WATER_TEMPLE = "Water Temple_1";

	public const string IMPROVEMENT_MOUNTAIN_TEMPLE = "Mountain Temple_1";

	public const string IMPROVEMENT_FOREST_TEMPLE = "Forest Temple_1";

	public const string IMPROVEMENT_ICE_TEMPLE = "Ice Temple_1";

	public const string IMPROVEMENT_ROAD = "Road";

	public const string IMPROVEMENT_RUIN = "ruin";

	public const string IMPROVEMENT_PLACEHOLDER = "placeholder";

	public const string UNIT_ANIMALS = "animal";

	public const string UNIT_HEADS = "head";

	public const string UNIT_POLYTAUR_HEADS = "polytaur_2";

	public const string UNIT_ROOFS = "roof";

	public static SpriteAddress[] GetAddresses(PickerType pickerType, int tribeId, SkinType skinType)
	{
		return new SpriteAddress[2]
		{
			GetAddress(pickerType, skinType.GetName()),
			GetAddress(pickerType, tribeId.ToString())
		};
	}

	public static SpriteAddress GetAddress(PickerType pickerType, string skinID)
	{
		return pickerType switch
		{
			PickerType.Head => GetHeadSpriteAddress(skinID), 
			PickerType.Unit => new SpriteAddress("Units", skinID), 
			PickerType.Roof => new SpriteAddress("Units", "roof_" + skinID), 
			PickerType.PolytaurHead => new SpriteAddress("Units", "polytaur_2_" + skinID), 
			PickerType.Animal => GetResourceSpriteAddress(ResourceData.Type.Game, skinID), 
			PickerType.Fruit => GetResourceSpriteAddress(ResourceData.Type.Fruit, skinID), 
			PickerType.Forest => GetTileSpriteAddress(TerrainData.Type.Forest, skinID), 
			PickerType.Mountain => GetTileSpriteAddress(TerrainData.Type.Mountain, skinID), 
			PickerType.Monument_1 => GetBuildingSpriteAddress(ImprovementData.Type.Monument1, skinID), 
			PickerType.Monument_2 => GetBuildingSpriteAddress(ImprovementData.Type.Monument2, skinID), 
			PickerType.Monument_3 => GetBuildingSpriteAddress(ImprovementData.Type.Monument3, skinID), 
			PickerType.Monument_4 => GetBuildingSpriteAddress(ImprovementData.Type.Monument4, skinID), 
			PickerType.Monument_5 => GetBuildingSpriteAddress(ImprovementData.Type.Monument5, skinID), 
			PickerType.Monument_6 => GetBuildingSpriteAddress(ImprovementData.Type.Monument6, skinID), 
			PickerType.Monument_7 => GetBuildingSpriteAddress(ImprovementData.Type.Monument7, skinID), 
			_ => default(SpriteAddress), 
		};
	}

	public static SpriteAddress GetTileSpriteAddress(TerrainData.Type terrain, string skinId)
	{
		return terrain switch
		{
			TerrainData.Type.Water => new SpriteAddress("TerrainFeatures", "water"), 
			TerrainData.Type.Ocean => new SpriteAddress("TerrainFeatures", "ocean"), 
			TerrainData.Type.Field => new SpriteAddress("TerrainFeatures", "ground_" + skinId), 
			TerrainData.Type.Mountain => new SpriteAddress("TerrainFeatures", "mountain_" + skinId), 
			TerrainData.Type.Forest => new SpriteAddress("TerrainFeatures", "Forest_" + skinId), 
			TerrainData.Type.Ice => new SpriteAddress("TerrainFeatures", "ice"), 
			_ => default(SpriteAddress), 
		};
	}

	public static SpriteAddress GetTileSpriteAddress(TerrainData.Type terrain, int climate = 0)
	{
		if (climate == 0)
		{
			climate = 1;
		}
		return GetTileSpriteAddress(terrain, climate.ToString());
	}

	public static SpriteAddress GetResourceSpriteAddress(ResourceData.Type type, string skinId)
	{
		string text = "ResourceGFX_";
		return type switch
		{
			ResourceData.Type.Fruit => new SpriteAddress("TerrainFeatures", text + "fruit_" + skinId), 
			ResourceData.Type.Game => new SpriteAddress("TerrainFeatures", "animal_" + skinId), 
			ResourceData.Type.Crop => new SpriteAddress("TerrainFeatures", text + "crop"), 
			ResourceData.Type.Fish => new SpriteAddress("TerrainFeatures", text + "fish"), 
			ResourceData.Type.Whale => new SpriteAddress("TerrainFeatures", text + "whale"), 
			ResourceData.Type.Metal => new SpriteAddress("TerrainFeatures", text + "metal"), 
			ResourceData.Type.Spores => new SpriteAddress("TerrainFeatures", text + "spores"), 
			_ => default(SpriteAddress), 
		};
	}

	public static SpriteAddress GetResourceSpriteAddress(ResourceData.Type type, int climate = 0)
	{
		if (climate == 0)
		{
			Debug.LogWarning((object)$"Climat {type} is {climate}.");
			climate = 1;
		}
		return GetResourceSpriteAddress(type, climate.ToString());
	}

	public static void GetResourceSprite(ResourceData.Type type, int climate, SpriteCallback completion)
	{
		GameManager.GetSpriteAtlasManager().LoadSprite(GetResourceSpriteAddress(type, climate), completion);
	}

	public static SpriteAddress GetBuildingSpriteAddress(ImprovementData.Type type, int climate = 0)
	{
		if (climate == 0)
		{
			climate = 1;
		}
		return GetBuildingSpriteAddress(type, climate.ToString());
	}

	public static SpriteAddress GetBuildingSpriteAddress(ImprovementData.Type type, string skinId)
	{
		return type switch
		{
			ImprovementData.Type.Ruin => new SpriteAddress("TerrainFeatures", "ruin"), 
			ImprovementData.Type.Road => new SpriteAddress("UI", "Road"), 
			ImprovementData.Type.CustomsHouse => new SpriteAddress("TerrainBuildings", "Customs House_1"), 
			ImprovementData.Type.Farm => new SpriteAddress("TerrainFeatures", "Farm"), 
			ImprovementData.Type.Windmill => new SpriteAddress("TerrainBuildings", "Windmill_1"), 
			ImprovementData.Type.Fishing => GetResourceSpriteAddress(ResourceData.Type.Fish, skinId), 
			ImprovementData.Type.Port => new SpriteAddress("TerrainFeatures", "Port"), 
			ImprovementData.Type.Hunting => GetResourceSpriteAddress(ResourceData.Type.Game, skinId), 
			ImprovementData.Type.ClearForest => new SpriteAddress("UI", "Clear forest"), 
			ImprovementData.Type.BurnForest => new SpriteAddress("UI", "Burn Forest"), 
			ImprovementData.Type.Cultivate => new SpriteAddress("UI", "Burn Spores"), 
			ImprovementData.Type.HarvestSpores => new SpriteAddress("TerrainFeatures", "spores"), 
			ImprovementData.Type.LumberHut => new SpriteAddress("TerrainFeatures", "Lumber Hut"), 
			ImprovementData.Type.Sawmill => new SpriteAddress("TerrainBuildings", "Sawmill_1"), 
			ImprovementData.Type.GrowForest => new SpriteAddress("UI", "Grow Forest"), 
			ImprovementData.Type.HarvestFruit => GetResourceSpriteAddress(ResourceData.Type.Fruit, skinId), 
			ImprovementData.Type.WhaleHunting => GetResourceSpriteAddress(ResourceData.Type.Whale, skinId), 
			ImprovementData.Type.Temple => new SpriteAddress("TerrainBuildings", "Temple_1"), 
			ImprovementData.Type.ForestTemple => new SpriteAddress("TerrainBuildings", "Forest Temple_1"), 
			ImprovementData.Type.WaterTemple => new SpriteAddress("TerrainBuildings", "Water Temple_1"), 
			ImprovementData.Type.MountainTemple => new SpriteAddress("TerrainBuildings", "Mountain Temple_1"), 
			ImprovementData.Type.Mine => new SpriteAddress("TerrainFeatures", "Mine"), 
			ImprovementData.Type.Forge => new SpriteAddress("TerrainBuildings", "Forge_1"), 
			ImprovementData.Type.Monument1 => new SpriteAddress("TerrainBuildings", "Monument1_" + skinId), 
			ImprovementData.Type.Monument2 => new SpriteAddress("TerrainBuildings", "Monument2_" + skinId), 
			ImprovementData.Type.Monument3 => new SpriteAddress("TerrainBuildings", "Monument3_" + skinId), 
			ImprovementData.Type.Monument4 => new SpriteAddress("TerrainBuildings", "Monument4_" + skinId), 
			ImprovementData.Type.Monument5 => new SpriteAddress("TerrainBuildings", "Monument5_" + skinId), 
			ImprovementData.Type.Monument6 => new SpriteAddress("TerrainBuildings", "Monument6_" + skinId), 
			ImprovementData.Type.Monument7 => new SpriteAddress("TerrainBuildings", "Monument7_" + skinId), 
			ImprovementData.Type.EnchantAnimal => GetResourceSpriteAddress(ResourceData.Type.Game, skinId), 
			ImprovementData.Type.EnchantWhale => GetResourceSpriteAddress(ResourceData.Type.Whale, skinId), 
			ImprovementData.Type.Sanctuary => new SpriteAddress("TerrainBuildings", "sanctuary_1"), 
			ImprovementData.Type.Outpost => new SpriteAddress("TerrainFeatures", "iceport"), 
			ImprovementData.Type.IceBank => new SpriteAddress("TerrainBuildings", "ice_bank_1"), 
			ImprovementData.Type.IceTemple => new SpriteAddress("TerrainBuildings", "Ice Temple_1"), 
			ImprovementData.Type.Fungi => new SpriteAddress("TerrainBuildings", "fungi_2"), 
			ImprovementData.Type.Mycelium => new SpriteAddress("TerrainBuildings", "Mycelium_3"), 
			ImprovementData.Type.Clathrus => new SpriteAddress("TerrainBuildings", "clathrus_2"), 
			ImprovementData.Type.Algae => new SpriteAddress("TerrainBuildings", "algae"), 
			_ => new SpriteAddress("TerrainBuildings", "placeholder"), 
		};
	}

	public static SpriteAddress[] GetBuildingSpriteAddresses(ImprovementData.Type type, SkinType skinType, int climate = 0)
	{
		return new SpriteAddress[2]
		{
			GetBuildingSpriteAddress(type, skinType.GetName()),
			GetBuildingSpriteAddress(type, climate)
		};
	}

	public static void GetBuildingSprite(ImprovementData data, SkinType skinType, int climate, SpriteCallback completion)
	{
		if (climate == 0)
		{
			climate = 1;
		}
		GameManager.GetSpriteAtlasManager().LoadSprite(GetBuildingSpriteAddresses(data.type, skinType, climate), completion);
	}

	public static SpriteAddress GetUnitIconAddress(UnitData.Type type)
	{
		string text = "unknown";
		if (EnumCache<UnitData.Type>.TryGetName(type, out var value))
		{
			text = value;
		}
		return new SpriteAddress("UI", text + "_icon");
	}

	public static SpriteAddress GetUISpriteAddress(string name)
	{
		return new SpriteAddress("UI", name);
	}

	public static SpriteAddress GetWeaponGFXAddress(string spriteName, string skinId)
	{
		return new SpriteAddress("Overlays", spriteName + "_" + skinId);
	}

	public static SpriteAddress[] GetHouseAddresses(int type, string styleId, SkinType skinType)
	{
		if (skinType != SkinType.Default)
		{
			return new SpriteAddress[2]
			{
				new SpriteAddress("TerrainFeatures", $"House_{skinType.GetName()}_{type}"),
				new SpriteAddress("TerrainFeatures", $"House_{styleId}_{type}")
			};
		}
		return new SpriteAddress[1]
		{
			new SpriteAddress("TerrainFeatures", $"House_{styleId}_{type}")
		};
	}

	public static SpriteAddress[] GetHeadSpriteAddresses(GameState gameState, PlayerState playerState)
	{
		string name = playerState.skinType.GetName();
		if (SeasonManager.TryGetHeadVariant(playerState, out var seasonalSkin))
		{
			name = seasonalSkin.GetName();
		}
		return new SpriteAddress[2]
		{
			GetHeadSpriteAddress(name),
			GetHeadSpriteAddress(playerState.GetTribeStyle(gameState))
		};
	}

	public static SpriteAddress GetHeadSpriteAddress(TribeData.Type type)
	{
		if (PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).TryGetData(type, out var data))
		{
			return GetHeadSpriteAddress(data.style);
		}
		Log.Error("Failed to get head for type {0}, falling back to neutral", new object[1] { type });
		return GetHeadSpriteAddress("neutral");
	}

	public static SpriteAddress GetHeadSpriteAddress(int tribe)
	{
		return new SpriteAddress("Heads", "head_" + tribe);
	}

	public static SpriteAddress GetHeadSpriteAddress(string specialId)
	{
		return new SpriteAddress("Heads", "head_" + specialId);
	}

	public static void GetHeadSprite(GameState gameState, PlayerState playerState, SingleSpriteCallback completion)
	{
		GameManager.GetSpriteAtlasManager().LoadSprite(GetHeadSpriteAddresses(gameState, playerState), completion);
	}

	public static SpriteAddress GetAvatarPartSpriteAddress(string sprite)
	{
		return new SpriteAddress("Avatar", sprite);
	}

	public static void GetAvatarPartSprite(string sprite, SpriteCallback completion)
	{
		GameManager.GetSpriteAtlasManager().LoadSprite(GetAvatarPartSpriteAddress(sprite), completion);
	}

	public static SpriteAddress GetIconSpriteAddress(string sprite)
	{
		return new SpriteAddress("UI", sprite);
	}
}
