using System.Collections.Generic;
using System.IO;

public class PeaceRequestResponseCommand : CommandBase
{
	public byte OpponentId { get; protected set; }

	public bool Accepted { get; protected set; }

	public PeaceRequestResponseCommand()
	{
	}

	public PeaceRequestResponseCommand(byte playerId, byte opponentId, bool accepted)
		: base(playerId)
	{
		OpponentId = opponentId;
		Accepted = accepted;
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		if (state.CurrentPlayer != base.PlayerId)
		{
			validationError = CommandBase.VALIDATION_ERROR_PLAYER_MISMATCH;
			return false;
		}
		if (!state.TryGetPendingCommandTrigger(base.PlayerId, out var trigger) && trigger.type == CommandTriggerType.CityLevelUp)
		{
			validationError = CommandBase.VALIDATION_ERROR_MISSING_COMMAND_TRIGGER;
			return false;
		}
		validationError = null;
		return true;
	}

	public override void Execute(GameState state)
	{
		state.PopPendingCommandTrigger(base.PlayerId, CommandTriggerType.PeaceRequest);
		if (Accepted)
		{
			state.ActionStack.Add(new UpdateTransportConnectionAction(base.PlayerId, new List<byte> { base.PlayerId, OpponentId }));
			state.ActionStack.Add(new UpdateRoutesAction(base.PlayerId));
			state.ActionStack.Add(new UpdateRoutesAction(OpponentId));
		}
		state.ActionStack.Add(new PeaceRequestResponseAction(base.PlayerId, OpponentId, Accepted));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.PeaceRequestResponse;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(OpponentId);
		writer.Write(Accepted);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		OpponentId = reader.ReadByte();
		Accepted = reader.ReadBoolean();
	}
}
