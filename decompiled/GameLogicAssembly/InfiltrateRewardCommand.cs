using System.IO;
using Polytopia.Data;

public class InfiltrateRewardCommand : CommandBase
{
	public CityReward Reward { get; private set; }

	public WorldCoordinates Coordinates { get; private set; }

	public InfiltrateRewardCommand()
	{
	}

	public InfiltrateRewardCommand(byte playerId, CityReward reward, WorldCoordinates coordinates)
		: base(playerId)
	{
		Reward = reward;
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState gameState, out string validationError)
	{
		if (gameState.CurrentPlayer != base.PlayerId)
		{
			validationError = CommandBase.VALIDATION_ERROR_PLAYER_MISMATCH;
			return false;
		}
		if (!gameState.TryGetPendingCommandTrigger(base.PlayerId, out var trigger) && trigger.type == CommandTriggerType.Infiltrate)
		{
			validationError = CommandBase.VALIDATION_ERROR_MISSING_COMMAND_TRIGGER;
			return false;
		}
		if (VersionManager.GameVersion == 60)
		{
			gameState.TryGetPlayer(base.PlayerId, out var playerState);
			if (!playerState.CanAfford(CityRewardData.GetRewardCost(Reward)))
			{
				validationError = CommandBase.VALIDATION_ERROR_CANT_AFFORD;
				return false;
			}
		}
		validationError = null;
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		if (ActionManager.USE_COMMAND_TRIGGER)
		{
			state.PopPendingCommandTrigger(base.PlayerId, CommandTriggerType.Infiltrate);
		}
		else
		{
			state.ClearWaitAction();
		}
		state.ActionStack.Add(new InfiltrationRewardAction(base.PlayerId, Reward, Coordinates, UnitData.Type.Cloak));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.InfiltrateReward;
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
