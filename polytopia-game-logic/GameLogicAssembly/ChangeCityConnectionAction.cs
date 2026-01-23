using System.IO;
using Polytopia.Data;

public class ChangeCityConnectionAction : ActionBase
{
	public WorldCoordinates Coordinates { get; private set; }

	public ChangeCityConnectionAction()
	{
	}

	public ChangeCityConnectionAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state)
	{
		return base.IsValid(state);
	}

	public override void Execute(GameState state)
	{
		int version = state.Version;
		if ((uint)(version - 6) <= 18u)
		{
			ExecuteV24(state);
		}
		else
		{
			ExecuteV25(state);
		}
	}

	public void ExecuteV25(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		TileData tile2 = state.Map.GetTile(playerState.startTile);
		state.ActionStack.Add(new IncreasePopulationAction(playerState.Id, tile2.coordinates, tile2.coordinates, 60));
		state.TryGetPlayer(tile.improvement.connectedToCapitalOfPlayer, out var playerState2);
		TileData tile3 = state.Map.GetTile(playerState2.startTile);
		state.ActionStack.Add(new DecreasePopulationAction(playerState.Id, tile3.coordinates, 60));
		tile.improvement.connectedToCapitalOfPlayer = base.PlayerId;
		state.CheckTask(playerState, TaskData.Type.Network);
	}

	public void ExecuteV24(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		TileData tile2 = state.Map.GetTile(playerState.startTile);
		state.ActionStack.Add(new IncreasePopulationAction(playerState.Id, tile2.coordinates, tile2.coordinates, 60));
		state.TryGetPlayer(tile.improvement.connectedToCapitalOfPlayer, out var playerState2);
		TileData tile3 = state.Map.GetTile(playerState2.startTile);
		state.ActionStack.Add(new DecreasePopulationAction(playerState.Id, tile3.coordinates, 60));
		tile.improvement.connectedToCapitalOfPlayer = base.PlayerId;
	}

	public override ActionType GetActionType()
	{
		return ActionType.ChangeCityConnection;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		if (version >= 11)
		{
			Coordinates.Serialize(writer, version);
		}
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		if (version >= 11)
		{
			Coordinates = new WorldCoordinates(reader, version);
		}
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates})";
	}
}
