using System.IO;

public class FreezeTileAction : ActionBase
{
	public static int DEFAULT_TIME_MILLISECONDS = 50;

	public WorldCoordinates Coordinates { get; protected set; }

	public FreezeTileAction()
	{
	}

	public FreezeTileAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		return state.Map.GetTile(Coordinates).IsFreezable(state, playerState);
	}

	public override void Execute(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		TileData tile = state.Map.GetTile(Coordinates);
		ActionUtils.FreezeTile(state, playerState, tile);
	}

	public override ActionType GetActionType()
	{
		return ActionType.FreezeTile;
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
