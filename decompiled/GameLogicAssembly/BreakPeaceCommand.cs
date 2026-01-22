using System.Collections.Generic;
using System.IO;

public class BreakPeaceCommand : CommandBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public byte OpponentId { get; protected set; }

	public BreakPeaceCommand()
	{
	}

	public BreakPeaceCommand(byte playerId, byte opponentId, WorldCoordinates coordinates)
		: base(playerId)
	{
		OpponentId = opponentId;
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		if (!PassesBasicValidation(state, out validationError))
		{
			return false;
		}
		state.TryGetPlayer(base.PlayerId, out var playerState);
		state.TryGetPlayer(OpponentId, out var playerState2);
		if (!playerState.HasPeaceWith(OpponentId))
		{
			validationError = CommandBase.VALIDATION_ERROR_INVALID_DIPLOMACY_STATE;
			return false;
		}
		if (!playerState.KnowsPlayer(OpponentId) || !playerState2.IsAlive(state))
		{
			validationError = CommandBase.VALIDATION_ERROR_INVALID_DIPLOMACY_TARGET;
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		state.ActionStack.Add(new UpdateTransportConnectionAction(base.PlayerId, new List<byte> { base.PlayerId, OpponentId }));
		state.ActionStack.Add(new UpdateRoutesAction(base.PlayerId));
		state.ActionStack.Add(new UpdateRoutesAction(OpponentId));
		state.ActionStack.Add(new BreakPeaceAction(base.PlayerId, OpponentId));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.BreakPeace;
	}

	public override bool ShouldAlwaysAskForConfirmation()
	{
		return true;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
		writer.Write(OpponentId);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
		OpponentId = reader.ReadByte();
	}
}
