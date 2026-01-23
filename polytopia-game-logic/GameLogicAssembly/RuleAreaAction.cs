using System.IO;

public class RuleAreaAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public RuleAreaAction()
	{
	}

	public RuleAreaAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		ActionUtils.RuleArea(state, playerState, tile, shouldUseActions: true);
	}

	public override ActionType GetActionType()
	{
		return ActionType.RuleArea;
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
