using System.IO;
using Polytopia.Data;

public class TrainAction : ActionBase
{
	public UnitData.Type Type { get; private set; }

	public WorldCoordinates Coordinates { get; private set; }

	public int Cost { get; private set; }

	public WorldCoordinates LookCoordinates { get; private set; } = WorldCoordinates.NULL_COORDINATES;

	public TrainAction()
	{
	}

	public TrainAction(byte playerId, UnitData.Type type, WorldCoordinates coordinates, int cost)
		: base(playerId)
	{
		Type = type;
		Coordinates = coordinates;
		Cost = cost;
	}

	public TrainAction(byte playerId, UnitData.Type type, WorldCoordinates coordinates, int cost, WorldCoordinates lookCoordinates)
		: base(playerId)
	{
		Type = type;
		Coordinates = coordinates;
		Cost = cost;
		LookCoordinates = lookCoordinates;
	}

	public override void Execute(GameState gameState)
	{
		if (gameState.Version < 28)
		{
			ExecuteV27(gameState);
		}
		else if (gameState.Version < 60)
		{
			ExecuteV28(gameState);
		}
		else
		{
			ExecuteDefault(gameState);
		}
	}

	public void ExecuteDefault(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Coordinates);
		if (tile != null && gameState.GameLogicData.TryGetData(Type, out var data) && gameState.TryGetPlayer(base.PlayerId, out var playerState))
		{
			playerState.Currency -= Cost;
			AddSubAction(new IncreaseScoreAction(base.PlayerId, (int)ScoreSheet.GetUnitScore(data), tile.coordinates));
			if (tile.unit != null && tile.unit.owner == base.PlayerId)
			{
				ActionUtils.TrainUnitOnOccupiedSpace(gameState, base.PlayerId, Type, tile);
			}
			else
			{
				UnitState unitState = ActionUtils.TrainUnit(gameState, playerState, tile, data);
				if (LookCoordinates != WorldCoordinates.NULL_COORDINATES)
				{
					unitState.SetUnitDirection(unitState.coordinates, LookCoordinates);
				}
			}
			if (tile.IsWater && !data.HasAbility(UnitAbility.Type.Swim) && !data.HasAbility(UnitAbility.Type.Fly))
			{
				AddSubAction(new EmbarkAction(base.PlayerId, tile.coordinates));
			}
			ActionUtils.ExploreFromTile(gameState, playerState, tile, data.GetSightRange(), shouldUseActions: true);
			ActionUtils.CheckSurroundingArea(gameState, base.PlayerId, tile);
		}
		CommitSubActionsToStack(gameState.ActionStack);
	}

	public void ExecuteV27(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null && state.GameLogicData.TryGetData(Type, out var data) && state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			playerState.Currency -= Cost;
			AddSubAction(new IncreaseScoreAction(base.PlayerId, (int)ScoreSheet.GetUnitScore(data), tile.coordinates));
			if (tile.unit != null && tile.unit.owner == base.PlayerId)
			{
				ActionUtils.TrainUnitOnOccupiedSpace(state, base.PlayerId, Type, tile);
			}
			else
			{
				ActionUtils.TrainUnit(state, playerState, tile, data);
			}
			ActionUtils.ExploreFromTile(state, playerState, tile, data.GetSightRange(), shouldUseActions: true);
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
		}
		CommitSubActionsToStack(state.ActionStack);
	}

	public void ExecuteV28(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null && state.GameLogicData.TryGetData(Type, out var data) && state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			playerState.Currency -= Cost;
			if (tile.unit != null && tile.unit.owner == base.PlayerId)
			{
				ActionUtils.TrainUnitOnOccupiedSpace(state, base.PlayerId, Type, tile);
			}
			else
			{
				AddSubAction(new IncreaseScoreAction(base.PlayerId, (int)ScoreSheet.GetUnitScore(data), tile.coordinates));
				ActionUtils.TrainUnit(state, playerState, tile, data);
			}
			ActionUtils.ExploreFromTile(state, playerState, tile, data.GetSightRange(), shouldUseActions: true);
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
		}
		CommitSubActionsToStack(state.ActionStack);
	}

	public override ActionType GetActionType()
	{
		return ActionType.Train;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		if (version < 60)
		{
			Serialize59(writer, version);
		}
		else
		{
			SerializeDefault(writer, version);
		}
	}

	private void Serialize59(BinaryWriter writer, int version)
	{
		writer.Write((ushort)Type);
		Coordinates.Serialize(writer, version);
		writer.Write(Cost);
	}

	private void SerializeDefault(BinaryWriter writer, int version)
	{
		writer.Write((ushort)Type);
		Coordinates.Serialize(writer, version);
		writer.Write(Cost);
		LookCoordinates.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		if (version < 60)
		{
			Deserialize59(reader, version);
		}
		else
		{
			DeserializeDefault(reader, version);
		}
	}

	private void Deserialize59(BinaryReader reader, int version)
	{
		Type = (UnitData.Type)reader.ReadUInt16();
		Coordinates = new WorldCoordinates(reader, version);
		Cost = reader.ReadInt32();
	}

	private void DeserializeDefault(BinaryReader reader, int version)
	{
		Type = (UnitData.Type)reader.ReadUInt16();
		Coordinates = new WorldCoordinates(reader, version);
		Cost = reader.ReadInt32();
		LookCoordinates = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Type: {Type}, Coordinates: {Coordinates}, Cost: {Cost})";
	}
}
