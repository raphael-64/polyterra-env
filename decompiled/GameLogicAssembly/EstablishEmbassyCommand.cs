using System.IO;
using Polytopia.Data;

public class EstablishEmbassyCommand : CommandBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public byte OpponentId { get; protected set; }

	public EstablishEmbassyCommand()
	{
	}

	public EstablishEmbassyCommand(byte playerId, byte opponentId, WorldCoordinates coordinates)
		: base(playerId)
	{
		OpponentId = opponentId;
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState gameState, out string validationError)
	{
		if (!PassesBasicValidation(gameState, out validationError))
		{
			return false;
		}
		if (!gameState.Map.GetTile(Coordinates).ContainsOpponentCityOrUnit(base.PlayerId, OpponentId))
		{
			validationError = CommandBase.VALIDATION_ERROR_INVALID_DIPLOMACY_TARGET;
			return false;
		}
		gameState.TryGetPlayer(base.PlayerId, out var playerState);
		if (playerState.Currency < gameState.GameLogicData.DiplomacyData.embassyCost)
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_AFFORD;
			return false;
		}
		if (!gameState.GameLogicData.IsUnlocked(PlayerAbility.Type.Embassy, playerState))
		{
			validationError = CommandBase.VALIDATION_ERROR_NOT_UNLOCKED;
			return false;
		}
		gameState.TryGetPlayer(OpponentId, out var playerState2);
		if (gameState.Version < 80)
		{
			if (playerState.HasEmbassyWith(playerState2) || !playerState2.OwnsTheirCapital(gameState) || playerState2.HasWarWith(playerState, gameState))
			{
				validationError = CommandBase.VALIDATION_ERROR_INVALID_DIPLOMACY_STATE;
				return false;
			}
		}
		else if (playerState.HasEmbassyWith(playerState2) || !playerState2.OwnsTheirCapital(gameState))
		{
			validationError = CommandBase.VALIDATION_ERROR_INVALID_DIPLOMACY_STATE;
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		state.ActionStack.Add(new EstablishEmbassyAction(base.PlayerId, OpponentId));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.EstablishEmbassy;
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
