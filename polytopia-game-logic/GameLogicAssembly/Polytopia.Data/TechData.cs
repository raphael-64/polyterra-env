using System.Collections.Generic;
using Newtonsoft.Json;

namespace Polytopia.Data;

public class TechData
{
	public enum Type
	{
		Basic = 0,
		Riding = 1,
		FreeSpirit = 2,
		Chivalry = 3,
		Roads = 4,
		Trade = 5,
		Organization = 6,
		Shields = 7,
		Farming = 8,
		Construction = 9,
		Fishing = 10,
		Whaling = 11,
		Aquatism = 12,
		Sailing = 13,
		Navigation = 14,
		Hunting = 15,
		Forestry = 16,
		Mathematics = 17,
		Archery = 18,
		Spiritualism = 19,
		Climbing = 20,
		Meditation = 21,
		Philosophy = 22,
		Mining = 23,
		Smithery = 24,
		FreeDiving = 25,
		Spearing = 26,
		Riding2 = 27,
		ForestMagic = 28,
		WaterMagic = 29,
		Frostwork = 30,
		PolarWarfare = 31,
		Polarism = 32,
		Oceanology = 33,
		Shock = 35,
		Recycling = 36,
		Hydrology = 37,
		Diplomacy = 38
	}

	public int idx;

	public int cost;

	[JsonConverter(typeof(StringIDsToObjectsConverter<TechData, Type>))]
	public List<TechData> techUnlocks = new List<TechData>();

	[JsonConverter(typeof(StringIDsToObjectsConverter<ImprovementData, ImprovementData.Type>))]
	public List<ImprovementData> improvementUnlocks = new List<ImprovementData>();

	[JsonConverter(typeof(StringIDsToObjectsConverter<UnitData, UnitData.Type>))]
	public List<UnitData> unitUnlocks = new List<UnitData>();

	[JsonConverter(typeof(StringIDsToEnumsConverter<PlayerAbility.Type>))]
	public List<PlayerAbility.Type> abilityUnlocks = new List<PlayerAbility.Type>();

	[JsonConverter(typeof(StringIDsToObjectsConverter<TaskData, TaskData.Type>))]
	public List<TaskData> taskUnlocks = new List<TaskData>();

	[JsonConverter(typeof(DictionaryWithStringIDsConverter<int, TerrainData.Type>))]
	public Dictionary<TerrainData.Type, int> movementUnlocks = new Dictionary<TerrainData.Type, int>();

	[JsonConverter(typeof(DictionaryWithStringIDsConverter<int, TerrainData.Type>))]
	public Dictionary<TerrainData.Type, int> defenceBonusUnlocks = new Dictionary<TerrainData.Type, int>();

	public string displayName => $"technology.names.{type.GetName()}";

	public Type type => (Type)idx;
}
