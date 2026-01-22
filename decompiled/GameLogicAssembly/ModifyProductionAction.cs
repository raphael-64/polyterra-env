using System.IO;

public class ModifyProductionAction : ActionBase
{
	public short Amount { get; protected set; }

	public WorldCoordinates Source { get; protected set; }

	public ModifyProductionAction()
	{
	}

	public ModifyProductionAction(byte playerId, short amount, WorldCoordinates source)
		: base(playerId)
	{
		Amount = amount;
		Source = source;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Source);
		tile.improvement.production = (ushort)(tile.improvement.production + Amount);
	}

	public override ActionType GetActionType()
	{
		return ActionType.ModifyProduction;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(Amount);
		Source.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Amount = reader.ReadInt16();
		Source = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Amount: {Amount}, Source: {Source})";
	}
}
