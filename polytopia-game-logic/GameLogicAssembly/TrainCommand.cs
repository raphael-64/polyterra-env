using System.IO;
using Polytopia.Data;

public class TrainCommand : CommandBase
{
	public UnitData.Type Type { get; private set; }

	public WorldCoordinates Coordinates { get; private set; }

	public TrainCommand()
	{
	}

	public TrainCommand(byte playerId, UnitData.Type type, WorldCoordinates coordinates)
		: base(playerId)
	{
		Type = type;
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		if (!PassesBasicValidation(state, out validationError))
		{
			return false;
		}
		if (!state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			validationError = CommandBase.VALIDATION_ERROR_MISSING_PLAYER;
			return false;
		}
		if (!state.GameLogicData.TryGetData(Type, out var data))
		{
			validationError = CommandBase.VALIDATION_ERROR_MISSING_UNIT_DATA;
			return false;
		}
		if (!playerState.CanAfford(data))
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_AFFORD;
			return false;
		}
		if (!playerState.CanTrainUnit(state, Type))
		{
			validationError = CommandBase.VALIDATION_ERROR_NOT_UNLOCKED;
			return false;
		}
		if (!CommandValidation.HasUnitTerrain(state, Coordinates, data))
		{
			validationError = CommandBase.VALIDATION_ERROR_UNIT_LOCKED;
			return false;
		}
		if (CommandValidation.HasUnit(state, Coordinates))
		{
			validationError = CommandBase.VALIDATION_ERROR_TILE_OCCUPIED;
			return false;
		}
		if (!data.HasAbility(UnitAbility.Type.Independent) && !data.HasAbility(UnitAbility.Type.Agent) && !CommandValidation.CanCitySupportUnit(state, Coordinates))
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_SUPPORT_MORE_UNITS;
			return false;
		}
		if (data.HasAbility(UnitAbility.Type.Agent) && CommandValidation.HasTempleWithinRange(state, Coordinates, 1))
		{
			validationError = CommandBase.VALIDATION_ERROR_WITHIN_TEMPLE_RANGE;
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		if (state.GameLogicData.TryGetData(Type, out var data))
		{
			state.ActionStack.Add(new TrainAction(base.PlayerId, Type, Coordinates, data.cost));
		}
	}

	public override CommandType GetCommandType()
	{
		return CommandType.Train;
	}

	public override bool ShouldAskForConfirmation()
	{
		return true;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write((ushort)Type);
		Coordinates.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Type = (UnitData.Type)reader.ReadUInt16();
		Coordinates = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Type: {Type}, Coordinates {Coordinates})";
	}
}
