using System.IO;
using Polytopia.Data;

public class UpgradeAction : ActionBase
{
	public UnitData.Type Type { get; private set; }

	public WorldCoordinates Coordinates { get; private set; }

	public int Cost { get; private set; }

	public UpgradeAction()
	{
	}

	public UpgradeAction(byte playerId, UnitData.Type type, WorldCoordinates coordinates, int cost)
		: base(playerId)
	{
		Type = type;
		Coordinates = coordinates;
		Cost = cost;
	}

	public override void Execute(GameState state)
	{
		if (state.Version < 40)
		{
			Execute1(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	public void ExecuteDefault(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null && state.GameLogicData.TryGetData(Type, out var data) && state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			playerState.Currency -= Cost;
			UnitState unit = tile.unit;
			UnitState unitState = ActionUtils.TrainUnit(state, playerState, tile, data);
			unitState.home = unit.home;
			if (unit.passengerUnit != null)
			{
				unitState.passengerUnit = unit.passengerUnit;
				unitState.health = unit.health;
			}
			unitState.moved = unit.moved;
			unitState.attacked = unit.attacked;
			unitState.flipped = unit.flipped;
			unitState.direction = unit.direction;
			unitState.follower = unit.follower;
			unitState.id = unit.id;
			unitState.effects = unit.effects;
			ActionUtils.ExploreFromTile(state, playerState, tile, data.GetSightRange(), shouldUseActions: true);
		}
	}

	public void Execute1(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null && state.GameLogicData.TryGetData(Type, out var data) && state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			playerState.Currency -= Cost;
			UnitState unit = tile.unit;
			UnitState unitState = ActionUtils.TrainUnit(state, playerState, tile, data);
			unitState.home = unit.home;
			if (unit.passengerUnit != null)
			{
				unitState.passengerUnit = unit.passengerUnit;
				unitState.health = unit.health;
			}
			unitState.moved = unit.moved;
			unitState.attacked = unit.attacked;
			unitState.flipped = unit.flipped;
			unitState.direction = unit.direction;
			ActionUtils.ExploreFromTile(state, playerState, tile, data.GetSightRange(), shouldUseActions: true);
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Upgrade;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write((ushort)Type);
		Coordinates.Serialize(writer, version);
		writer.Write(Cost);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Type = (UnitData.Type)reader.ReadUInt16();
		Coordinates = new WorldCoordinates(reader, version);
		Cost = reader.ReadInt32();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Type: {Type}, Coordinates: {Coordinates}, Cost: {Cost})";
	}
}
