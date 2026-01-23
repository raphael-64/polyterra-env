using System.Collections.Generic;
using System.IO;

public class BuildRoadAction : ActionBase
{
	public List<TileData> newTransportPathTiles;

	public WorldCoordinates Coordinates { get; private set; }

	public BuildRoadAction()
	{
	}

	public BuildRoadAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		tile.HasRoad = true;
		newTransportPathTiles = new List<TileData>(1);
		newTransportPathTiles.Add(tile);
	}

	public override ActionType GetActionType()
	{
		return ActionType.BuildRoad;
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

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates})";
	}
}
