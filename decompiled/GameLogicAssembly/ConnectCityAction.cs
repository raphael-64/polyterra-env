using System.IO;
using Polytopia.Data;

public class ConnectCityAction : ActionBase
{
	public WorldCoordinates Coordinates { get; private set; }

	public ConnectCityAction()
	{
	}

	public ConnectCityAction(byte playerId, WorldCoordinates coordinates)
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
		state.TryGetPlayer(base.PlayerId, out var playerState);
		TileData tile = state.Map.GetTile(playerState.startTile);
		TileData tile2 = state.Map.GetTile(Coordinates);
		state.ActionStack.Add(new IncreasePopulationAction(base.PlayerId, tile.coordinates, tile.coordinates, 60));
		state.ActionStack.Add(new IncreasePopulationAction(base.PlayerId, tile2.coordinates, tile2.coordinates, 60));
		tile2.improvement.connectedToCapitalOfPlayer = base.PlayerId;
		state.CheckTask(playerState, TaskData.Type.Network);
	}

	public override ActionType GetActionType()
	{
		return ActionType.ConnectCity;
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
