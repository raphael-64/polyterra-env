using System.IO;
using Polytopia.Data;

public class ResearchCommand : CommandBase
{
	public TechData.Type Type { get; private set; }

	public ResearchCommand()
	{
	}

	public ResearchCommand(byte playerId, TechData.Type type)
		: base(playerId)
	{
		Type = type;
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
			validationError = CommandBase.VALIDATION_ERROR_MISSING_TECH_DATA;
			return false;
		}
		if (!state.GameLogicData.GetUnlockableTech(playerState).Contains(data))
		{
			validationError = CommandBase.VALIDATION_ERROR_TECH_NOT_UNLOCKABLE;
			return false;
		}
		if (!playerState.CanAfford(state, data))
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_AFFORD;
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		if (state.TryGetPlayer(base.PlayerId, out var playerState) && state.GameLogicData.TryGetData(Type, out var data))
		{
			state.ActionStack.Add(new ResearchAction(base.PlayerId, Type, state.GameLogicData.GetTechPrice(data, playerState, state)));
		}
	}

	public override CommandType GetCommandType()
	{
		return CommandType.Research;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write((ushort)Type);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Type = (TechData.Type)reader.ReadUInt16();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Type: {Type})";
	}
}
