using System.IO;

public class ModifyScoreAction : ActionBase
{
	public short Amount { get; protected set; }

	public WorldCoordinates Target { get; protected set; }

	public ModifyScoreAction()
	{
	}

	public ModifyScoreAction(byte playerId, WorldCoordinates target, short amount)
		: base(playerId)
	{
		Amount = amount;
		Target = target;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Target);
		tile.improvement.baseScore = (ushort)(tile.improvement.baseScore + Amount);
	}

	public override ActionType GetActionType()
	{
		return ActionType.ModifyScore;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(Amount);
		Target.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Amount = reader.ReadInt16();
		Target = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Amount: {Amount}, Source: {Target})";
	}
}
