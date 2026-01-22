using System.IO;
using Polytopia.Data;

public class PeaceTreatyCommand : CommandBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public byte OpponentId { get; protected set; }

	public PeaceTreatyCommand()
	{
	}

	public PeaceTreatyCommand(byte playerId, byte opponentId, WorldCoordinates coordinates)
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
		if (!state.GameLogicData.IsUnlocked(PlayerAbility.Type.PeaceTreaty, playerState))
		{
			validationError = CommandBase.VALIDATION_ERROR_NOT_UNLOCKED;
			return false;
		}
		state.TryGetPlayer(OpponentId, out var playerState2);
		if (playerState.HasPeaceWith(OpponentId) || playerState2.HasMessage(DiplomacyMessageType.PeaceRequest, playerState.Id))
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
		state.ActionStack.Add(new PeaceTreatyAction(base.PlayerId, OpponentId));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.PeaceTreaty;
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
