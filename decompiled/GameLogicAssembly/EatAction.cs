using System.IO;
using Polytopia.Data;

public class EatAction : ActionBase
{
	public uint Eater { get; private set; }

	public WorldCoordinates Coordinates { get; private set; }

	public EatAction()
	{
	}

	public EatAction(byte playerId, uint eater)
		: base(playerId)
	{
		Eater = eater;
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 40)
		{
			ExecuteV40(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	public void ExecuteDefault(GameState state)
	{
		if (!state.TryGetUnit(Eater, out var unit))
		{
			return;
		}
		TileData tileData = null;
		GridDirection gridDirection = unit.direction.Opposite();
		for (int i = 0; i < GridDirections.COUNT; i++)
		{
			int num = (i + 1) / 2 * ((i % 2 == 0) ? 1 : (-1));
			GridDirection direction = (GridDirection)((int)(gridDirection + num + GridDirections.COUNT) % GridDirections.COUNT);
			TileData tile = state.Map.GetTile(unit.coordinates + direction.ToCoordinates());
			if (tile != null && tile.unit == null && state.GetPath(unit.coordinates, tile.coordinates, 1, unit) != null)
			{
				tileData = tile;
				break;
			}
		}
		if (tileData != null && state.GameLogicData.TryGetData(UnitData.Type.Segment, out var data) && state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			Coordinates = tileData.coordinates;
			UnitState unitState = ActionUtils.TrainUnit(state, playerState, tileData, data);
			unitState.leader = Eater;
			unit.follower = unitState.id;
			ActionUtils.ExploreFromTile(state, playerState, tileData, data.GetSightRange(), shouldUseActions: true);
			ActionUtils.CheckSurroundingArea(state, playerState.Id, tileData);
			state.ActionStack.Add(new IncreaseScoreAction(playerState.Id, (int)ScoreSheet.GetUnitScore(data), tileData.coordinates));
		}
	}

	public void ExecuteV40(GameState state)
	{
		if (!state.TryGetUnit(Eater, out var unit))
		{
			return;
		}
		TileData tileData = null;
		GridDirection gridDirection = unit.direction.Opposite();
		for (int i = 0; i < GridDirections.COUNT; i++)
		{
			int num = (i + 1) / 2 * ((i % 2 == 0) ? 1 : (-1));
			GridDirection direction = (GridDirection)((int)(gridDirection + num + GridDirections.COUNT) % GridDirections.COUNT);
			TileData tile = state.Map.GetTile(unit.coordinates + direction.ToCoordinates());
			if (tile != null && tile.unit == null && state.GetPath(unit.coordinates, tile.coordinates, 1, unit) != null)
			{
				tileData = tile;
				break;
			}
		}
		if (tileData != null && state.GameLogicData.TryGetData(UnitData.Type.Segment, out var data) && state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			Coordinates = tileData.coordinates;
			UnitState unitState = ActionUtils.TrainUnit(state, playerState, tileData, data);
			unitState.leader = Eater;
			unit.follower = unitState.id;
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Eat;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write((ushort)Eater);
		Coordinates.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Eater = reader.ReadUInt16();
		Coordinates = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Type: {Eater}, Coordinates: {Coordinates})";
	}
}
