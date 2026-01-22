using System.IO;
using Polytopia.Data;

public class ImprovementLevelUpAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public ImprovementLevelUpAction()
	{
	}

	public ImprovementLevelUpAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.improvement == null || tile.improvement.type == ImprovementData.Type.None)
		{
			return false;
		}
		if (ActionUtils.CalculateImprovementLevel(state, tile) <= tile.improvement.level)
		{
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 40)
		{
			ExecuteV1(state);
		}
		else if (state.Version <= 42)
		{
			ExecuteV42(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	public void ExecuteDefault(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.improvement == null)
		{
			return;
		}
		tile.improvement.level++;
		if (!state.GameLogicData.TryGetData(tile.improvement.type, out var data) || data.growthRewards == null || data.growthRewards.Count <= 0)
		{
			return;
		}
		foreach (GrowthRewards growthReward in data.growthRewards)
		{
			if (growthReward.population > 0 && tile.rulingCityCoordinates != WorldCoordinates.NULL_COORDINATES)
			{
				for (int i = 0; i < growthReward.population; i++)
				{
					state.ActionStack.Add(new IncreasePopulationAction(base.PlayerId, tile.coordinates, tile.rulingCityCoordinates, 60));
				}
			}
			if (growthReward.score > 0)
			{
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, growthReward.score, tile.coordinates));
			}
		}
	}

	public void ExecuteV42(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.improvement == null)
		{
			return;
		}
		tile.improvement.level++;
		if (!state.GameLogicData.TryGetData(tile.improvement.type, out var data) || data.growthRewards == null || data.growthRewards.Count <= 0)
		{
			return;
		}
		foreach (GrowthRewards growthReward in data.growthRewards)
		{
			if (growthReward.population > 0 && tile.rulingCityCoordinates != WorldCoordinates.NULL_COORDINATES)
			{
				for (int i = 0; i < growthReward.population; i++)
				{
					state.ActionStack.Add(new IncreasePopulationAction(base.PlayerId, tile.coordinates, tile.rulingCityCoordinates, 60));
				}
			}
			if (growthReward.score > 0)
			{
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, growthReward.score / 2, tile.coordinates));
			}
		}
	}

	public void ExecuteV1(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.improvement == null)
		{
			return;
		}
		tile.improvement.level++;
		if (!state.GameLogicData.TryGetData(tile.improvement.type, out var data) || data.rewards == null || data.rewards.Count <= 0)
		{
			return;
		}
		foreach (Rewards reward in data.rewards)
		{
			if (reward.population > 0 && !data.HasAbility(ImprovementAbility.Type.Patina))
			{
				for (int i = 0; i < reward.population; i++)
				{
					state.ActionStack.Add(new IncreasePopulationAction(base.PlayerId, tile.coordinates, tile.rulingCityCoordinates, 60));
				}
			}
			if (reward.score > 0)
			{
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, reward.score / 2, tile.coordinates));
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.ImprovementLevelUp;
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
