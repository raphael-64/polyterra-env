using System.IO;

public class PromoteAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public PromoteAction()
	{
	}

	public PromoteAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		switch (state.Version)
		{
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
			ExecuteV17(state);
			break;
		case 18:
		case 19:
		case 20:
		case 21:
			ExecuteV18(state);
			break;
		default:
			ExecuteV22(state);
			break;
		}
	}

	private void ExecuteV22(GameState state)
	{
		UnitState unit = state.Map.GetTile(Coordinates).unit;
		if (unit.passengerUnit != null)
		{
			unit.passengerUnit.promotionLevel++;
		}
		else
		{
			unit.promotionLevel++;
		}
		unit.health = (ushort)unit.GetMaxHealth(state);
	}

	private void ExecuteV18(GameState state)
	{
		UnitState unitState = state.Map.GetTile(Coordinates).unit;
		if (unitState.passengerUnit != null)
		{
			unitState = unitState.passengerUnit;
		}
		unitState.promotionLevel++;
		unitState.health = (ushort)unitState.GetMaxHealth(state);
	}

	private void ExecuteV17(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		tile.unit.promotionLevel++;
		tile.unit.health = (ushort)tile.unit.GetMaxHealth(state);
	}

	public override ActionType GetActionType()
	{
		return ActionType.Promote;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
	}
}
