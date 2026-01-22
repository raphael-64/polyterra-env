using System.IO;

public class BoostAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public BoostAction()
	{
	}

	public BoostAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null && tile.unit != null)
		{
			tile.unit.AddEffect(UnitEffect.Boosted);
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Boost;
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
