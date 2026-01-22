using System.IO;

public class PeaceTreatyAction : ActionBase
{
	public byte OpponentId { get; protected set; }

	public PeaceTreatyAction()
	{
	}

	public PeaceTreatyAction(byte playerId, byte opponentId)
		: base(playerId)
	{
		OpponentId = opponentId;
	}

	public override void Execute(GameState state)
	{
		state.TryGetPlayer(OpponentId, out var playerState);
		playerState.SendMessage(DiplomacyMessageType.PeaceRequest, base.PlayerId);
	}

	public override ActionType GetActionType()
	{
		return ActionType.PeaceTreaty;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(OpponentId);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		OpponentId = reader.ReadByte();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, OpponentId: {OpponentId})";
	}
}
