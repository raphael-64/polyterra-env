using System.IO;

public class HideAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public HideAction()
	{
	}

	public HideAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null && tile.unit != null)
		{
			tile.unit.moved = true;
			tile.unit.attacked = true;
			tile.unit.AddEffect(UnitEffect.Invisible);
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Hide;
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
		return $"{GetType()} (PlayerId: {base.PlayerId})";
	}
}
