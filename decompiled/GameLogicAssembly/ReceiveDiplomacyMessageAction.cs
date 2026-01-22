using System;
using System.IO;

public class ReceiveDiplomacyMessageAction : ActionBase
{
	public byte SenderId { get; protected set; }

	public DiplomacyMessageType MessageType { get; protected set; }

	public ReceiveDiplomacyMessageAction()
	{
	}

	public ReceiveDiplomacyMessageAction(byte playerId, byte senderId, DiplomacyMessageType messageType)
		: base(playerId)
	{
		SenderId = senderId;
		MessageType = messageType;
	}

	public override bool IsValid(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		return playerState.HasMessage(MessageType, SenderId);
	}

	public override void Execute(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		if (playerState.TryRemoveMessage(MessageType, SenderId))
		{
			switch (MessageType)
			{
			case DiplomacyMessageType.PeaceRequest:
			{
				CommandTrigger commandTrigger = new CommandTrigger
				{
					playerId = base.PlayerId,
					type = CommandTriggerType.PeaceRequest,
					opponentId = SenderId
				};
				state.AddPendingCommandTrigger(commandTrigger);
				break;
			}
			default:
				throw new Exception("Not implemented");
			case DiplomacyMessageType.EstablishEmbassy:
				break;
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.ReceiveDiplomacyMessage;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(SenderId);
		writer.Write((short)MessageType);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		SenderId = reader.ReadByte();
		MessageType = (DiplomacyMessageType)reader.ReadInt16();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, SenderId: {SenderId}, MessageType {MessageType})";
	}
}
