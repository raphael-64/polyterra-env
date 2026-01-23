using System.IO;
using Polytopia.Data;

public class BuildCommand : CommandBase
{
	public ImprovementData.Type Type { get; private set; }

	public WorldCoordinates Coordinates { get; private set; }

	public BuildCommand()
	{
	}

	public BuildCommand(byte playerId, ImprovementData.Type type, WorldCoordinates coordinates)
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
			validationError = CommandBase.VALIDATION_ERROR_MISSING_IMPROVEMENT_DATA;
			return false;
		}
		if (!playerState.CanAfford(data))
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_AFFORD;
			return false;
		}
		if (!state.GameLogicData.IsUnlocked(data.type, playerState))
		{
			validationError = CommandBase.VALIDATION_ERROR_NOT_UNLOCKED;
			return false;
		}
		if (!state.GameLogicData.CanBuild(state, state.Map.GetTile(Coordinates), playerState, data))
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_BUILD;
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		state.ActionStack.Add(new BuildAction(base.PlayerId, Type, Coordinates));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.Build;
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
		Type = (ImprovementData.Type)reader.ReadUInt16();
		Coordinates = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Type: {Type}, Coordinates: {Coordinates})";
	}
}
