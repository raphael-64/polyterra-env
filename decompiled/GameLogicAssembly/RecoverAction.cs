using System.IO;

public class RecoverAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public RecoverAction()
	{
	}

	public RecoverAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null)
		{
			tile.unit.attacked = true;
			tile.unit.moved = true;
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Recover;
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
