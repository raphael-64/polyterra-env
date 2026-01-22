using System.IO;

public class DestroyEmbassyAction : ActionBase
{
	public byte OpponentId { get; protected set; }

	public DestroyEmbassyAction()
	{
	}

	public DestroyEmbassyAction(byte playerId, byte opponentId)
		: base(playerId)
	{
		OpponentId = opponentId;
	}

	public override bool IsValid(GameState state)
	{
		state.TryGetPlayer(OpponentId, out var playerState);
		return playerState.GetRelation(base.PlayerId).EmbassyLevel > 0;
	}

	public override void Execute(GameState state)
	{
		state.TryGetPlayer(OpponentId, out var playerState);
		playerState.GetRelation(base.PlayerId).EmbassyLevel = 0;
	}

	public override ActionType GetActionType()
	{
		return ActionType.DestroyEmbassy;
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
