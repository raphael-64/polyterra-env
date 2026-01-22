using System.IO;

public class ReselectAction : ActionBase
{
	public uint UnitId { get; private set; }

	public ReselectAction()
	{
	}

	public ReselectAction(byte playerId, uint unitId)
		: base(playerId)
	{
		UnitId = unitId;
	}

	public override void Execute(GameState state)
	{
	}

	public override ActionType GetActionType()
	{
		return ActionType.Reselect;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(UnitId);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		UnitId = reader.ReadUInt32();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, UnitId: {UnitId})";
	}
}
