using System.IO;

public class WipePlayerEndAction : ActionBase
{
	public byte TargetPlayerId { get; protected set; }

	public WipePlayerEndAction()
	{
	}

	public WipePlayerEndAction(byte playerId, byte targetPlayerId)
		: base(playerId)
	{
		TargetPlayerId = targetPlayerId;
	}

	public override void Execute(GameState state)
	{
	}

	public override ActionType GetActionType()
	{
		return ActionType.WipePlayerEnd;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(TargetPlayerId);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		TargetPlayerId = reader.ReadByte();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, TargetPlayerId: {TargetPlayerId})";
	}
}
