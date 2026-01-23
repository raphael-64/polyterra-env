using System.Collections.Generic;
using Newtonsoft.Json;

namespace Polytopia.Data;

public class TribeData
{
	public enum BonusEnum
	{
		None,
		CityLevel3,
		Spores
	}

	public enum CategoryEnum
	{
		Hidden,
		Human,
		Special
	}

	public enum PriceTierEnum
	{
		Tier0,
		Tier1,
		Tier2,
		Tier3
	}

	public enum Type
	{
		None,
		Nature,
		Aimo,
		Aquarion,
		Bardur,
		Elyrion,
		Hoodrick,
		Imperius,
		Kickoo,
		Luxidoor,
		Oumaji,
		Quetzali,
		Vengir,
		Xinxi,
		Yadakk,
		Zebasi,
		Polaris,
		Cymanti
	}

	public int idx;

	public int color;

	public int style;

	public int climate;

	public string language;

	public BonusEnum bonus;

	[JsonConverter(typeof(StringIDToObjectConverter<UnitData, UnitData.Type>))]
	public UnitData startingUnit;

	public CategoryEnum category;

	public PriceTierEnum priceTier;

	public List<SkinType> skins = new List<SkinType>();

	[JsonConverter(typeof(DictionaryWithStringIDsConverter<float, TerrainData.Type>))]
	public Dictionary<TerrainData.Type, float> terrainModifier = new Dictionary<TerrainData.Type, float>();

	[JsonConverter(typeof(DictionaryWithStringIDsConverter<float, ResourceData.Type>))]
	public Dictionary<ResourceData.Type, float> resourceModifier = new Dictionary<ResourceData.Type, float>();

	[JsonConverter(typeof(StringIDsToObjectsConverter<TechData, TechData.Type>))]
	public List<TechData> startingTech = new List<TechData>();

	[JsonConverter(typeof(StringIDsToObjectsConverter<ResourceData, ResourceData.Type>))]
	public List<ResourceData> startingResource = new List<ResourceData>();

	[JsonConverter(typeof(StringIDsToEnumsConverter<TribeAbility.Type>))]
	public List<TribeAbility.Type> tribeAbilities = new List<TribeAbility.Type>();

	[JsonConverter(typeof(DictionaryWithStringIDKeyValuesConverter<TechData, TechData.Type>))]
	public Dictionary<TechData.Type, TechData> techOverrides = new Dictionary<TechData.Type, TechData>();

	[JsonConverter(typeof(DictionaryWithStringIDKeyValuesConverter<UnitData, UnitData.Type>))]
	public Dictionary<UnitData.Type, UnitData> unitOverrides = new Dictionary<UnitData.Type, UnitData>();

	[JsonConverter(typeof(DictionaryWithStringIDKeyValuesConverter<ImprovementData, ImprovementData.Type>))]
	public Dictionary<ImprovementData.Type, ImprovementData> improvementOverrides = new Dictionary<ImprovementData.Type, ImprovementData>();

	public string displayName => $"tribes.{type.GetName()}";

	public string description => $"tribes.{type.GetName()}.info";

	public string description2 => $"tribes.{type.GetName()}.info2";

	public Type type => (Type)idx;
}
