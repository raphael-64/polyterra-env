using System.IO;

public class CityRewardCommand : CommandBase
{
	public CityReward Reward { get; private set; }

	public WorldCoordinates Coordinates { get; private set; }

	public CityRewardCommand()
	{
	}

	public CityRewardCommand(byte playerId, CityReward reward, WorldCoordinates coordinates)
		: base(playerId)
	{
		Reward = reward;
		Coordinates = coordinates;
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

	public override bool NeedServerConfirmation()
	{
		return Reward == CityReward.Explorer;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		if (ActionManager.USE_COMMAND_TRIGGER)
		{
			state.PopPendingCommandTrigger(base.PlayerId, CommandTriggerType.CityLevelUp);
		}
		else
		{
			state.ClearWaitAction();
		}
		state.ActionStack.Add(new CityRewardAction(base.PlayerId, Reward, Coordinates));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.CityReward;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
		writer.Write((ushort)Reward);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
		Reward = (CityReward)reader.ReadUInt16();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Reward: {Reward}, Coordinates: {Coordinates})";
	}
}
