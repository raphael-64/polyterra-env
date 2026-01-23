using System.IO;

public class DisconnectCityAction : ActionBase
{
	public WorldCoordinates Coordinates { get; private set; }

	public DisconnectCityAction()
	{
	}

	public DisconnectCityAction(byte playerId, WorldCoordinates coordinates)
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
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(tile.improvement.connectedToCapitalOfPlayer, out var playerState);
		TileData tile2 = state.Map.GetTile(playerState.startTile);
		state.ActionStack.Add(new DecreasePopulationAction(base.PlayerId, tile2.coordinates, 60));
		state.ActionStack.Add(new DecreasePopulationAction(base.PlayerId, tile.coordinates, 60));
		tile.improvement.connectedToCapitalOfPlayer = 0;
	}

	public override ActionType GetActionType()
	{
		return ActionType.DisconnectCity;
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
