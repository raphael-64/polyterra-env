using System.Collections.Generic;
using Newtonsoft.Json;

namespace Polytopia.Data;

public class UnitData
{
	public enum WeaponEnum
	{
		None,
		Club,
		Sword,
		Arrow,
		Magic,
		Gun,
		Rock,
		Claw,
		FireBlow,
		Trident,
		IceArrow,
		Poison,
		Sting,
		Dagger
	}

	public enum Type
	{
		None,
		Scout,
		Warrior,
		Rider,
		Knight,
		Defender,
		Ship,
		Battleship,
		Catapult,
		Archer,
		MindBender,
		Swordsman,
		Giant,
		Bunny,
		Boat,
		Polytaur,
		Navalon,
		DragonEgg,
		BabyDragon,
		FireDragon,
		Amphibian,
		Tridention,
		Mooni,
		BattleSled,
		IceFortress,
		IceArcher,
		Crab,
		Gaami,
		Hexapod,
		Doomux,
		Phychi,
		Kiton,
		Exida,
		Centipede,
		Segment,
		Raychi,
		Shaman,
		Dagger,
		Cloak,
		Cloak_Boat,
		Pirate
	}

	public int idx;

	public bool hidden;

	public int cost;

	public int health;

	public int defence;

	public int movement;

	public WeaponEnum weapon;

	public int range;

	public int attack;

	public int promotionLimit;

	[JsonConverter(typeof(StringIDsToEnumsConverter<UnitAbility.Type>))]
	public List<UnitAbility.Type> unitAbilities = new List<UnitAbility.Type>();

	[JsonConverter(typeof(StringIDsToObjectsConverter<TerrainData, TerrainData.Type>))]
	public List<TerrainData> movementTerrain = new List<TerrainData>();

	[JsonConverter(typeof(StringIDToObjectConverter<UnitData, Type>))]
	public UnitData upgradesFrom;

	public int growthRate;

	public string displayName => $"unit.names.{type.GetName()}";

	public Type type => (Type)idx;
}
