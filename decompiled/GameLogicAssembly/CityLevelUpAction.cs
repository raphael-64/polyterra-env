using System.IO;
using Polytopia.Data;

public class CityLevelUpAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public CityLevelUpAction()
	{
	}

	public CityLevelUpAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (!tile.HasImprovement(ImprovementData.Type.City))
		{
			return;
		}
		state.TryGetPlayer(tile.owner, out var playerState);
		if (!tile.improvement.ShouldLevelUp())
		{
			return;
		}
		ActionUtils.LevelUpCity(state, playerState, tile);
		state.CheckTask(playerState, TaskData.Type.Metropolis);
		if (ActionManager.USE_COMMAND_TRIGGER)
		{
			return;
		}
		if (playerState.AutoPlay)
		{
			if (state.GameLogicData.TryGetData(ImprovementData.Type.City, out var data))
			{
				CityReward reward = data.GetCityRewardsForLevel(tile.improvement.level - 1)[0];
				state.CommandStack.Add(new CityRewardCommand(playerState.Id, reward, tile.coordinates));
			}
		}
		else
		{
			state.ActionStack.Add(new CityRewardPopupAction(playerState.Id, tile.coordinates));
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.CityLevelUp;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates})";
	}
}
