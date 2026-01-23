using System.IO;

public class PoisonUnitAction : ActionBase
{
	public WorldCoordinates Origin { get; private set; }

	public WorldCoordinates Target { get; private set; }

	public PoisonUnitAction()
	{
	}

	public PoisonUnitAction(byte playerId, WorldCoordinates origin, WorldCoordinates target)
		: base(playerId)
	{
		Origin = origin;
		Target = target;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Origin);
		TileData tile2 = state.Map.GetTile(Target);
		_ = tile.unit;
		tile2.unit?.AddEffect(UnitEffect.Poisoned);
	}

	public override ActionType GetActionType()
	{
		return ActionType.Poison;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Origin.Serialize(writer, version);
		Target.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Origin = new WorldCoordinates(reader, version);
		Target = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Origin: {Origin}, Target: {Target})";
	}
}
