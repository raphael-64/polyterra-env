using System.IO;

public class KillUnitAction : ActionBase
{
	public WorldCoordinates Coordinates { get; private set; }

	public KillUnitAction()
	{
	}

	public KillUnitAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Coordinates);
		ActionUtils.KillUnit(gameState, tile);
	}

	public override ActionType GetActionType()
	{
		return ActionType.KillUnit;
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
