using System.Collections.Generic;
using System.IO;

public class PeaceRequestResponseAction : ActionBase
{
	public byte SenderId { get; protected set; }

	public bool Accepted { get; protected set; }

	public PeaceRequestResponseAction()
	{
	}

	public PeaceRequestResponseAction(byte playerId, byte senderId, bool accepted)
		: base(playerId)
	{
		SenderId = senderId;
		Accepted = accepted;
	}

	public override void Execute(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		state.TryGetPlayer(SenderId, out var playerState2);
		if (Accepted)
		{
			DiplomacyRelation relation = playerState.GetRelation(SenderId);
			DiplomacyRelation relation2 = playerState2.GetRelation(base.PlayerId);
			relation.State = DiplomacyRelationState.Peace;
			relation2.State = DiplomacyRelationState.Peace;
			AddSubAction(new UpdateRoutesAction(base.PlayerId));
			AddSubAction(new UpdateRoutesAction(SenderId));
			AddSubAction(new UpdateTransportConnectionAction(base.PlayerId, new List<byte> { base.PlayerId, SenderId }));
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.PeaceRequestResponse;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(SenderId);
		writer.Write(Accepted);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		SenderId = reader.ReadByte();
		Accepted = reader.ReadBoolean();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, SenderId: {SenderId} Accepted {Accepted})";
	}
}
