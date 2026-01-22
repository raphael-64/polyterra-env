using System.IO;
using Polytopia.Data;

public class ImprovementLevelDownAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	private ushort Amount { get; set; }

	public ImprovementLevelDownAction()
	{
	}

	public ImprovementLevelDownAction(byte playerId, WorldCoordinates coordinates, ushort amount = 1)
		: base(playerId)
	{
		Coordinates = coordinates;
		Amount = amount;
	}

	public override bool IsValid(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.improvement == null || tile.improvement.type == ImprovementData.Type.None)
		{
			return false;
		}
		if (ActionUtils.CalculateImprovementLevel(state, tile) >= tile.improvement.level)
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
		else
		{
			ExecuteDefault(state);
		}
	}

	public void ExecuteDefault(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.improvement == null || tile.improvement.level <= 0)
		{
			return;
		}
		tile.improvement.level -= Amount;
		if (!state.GameLogicData.TryGetData(tile.improvement.type, out var data) || data.growthRewards == null || data.growthRewards.Count <= 0)
		{
			return;
		}
		foreach (GrowthRewards growthReward in data.growthRewards)
		{
			if (growthReward.population > 0)
			{
				for (int i = 0; i < growthReward.population; i++)
				{
					state.ActionStack.Add(new DecreasePopulationAction(base.PlayerId, tile.rulingCityCoordinates, 200));
				}
			}
		}
	}

	public void ExecuteV1(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.improvement == null || tile.improvement.level <= 0)
		{
			return;
		}
		tile.improvement.level -= Amount;
		if (!state.GameLogicData.TryGetData(tile.improvement.type, out var data) || data.rewards == null || data.rewards.Count <= 0)
		{
			return;
		}
		foreach (Rewards reward in data.rewards)
		{
			if (reward.population > 0)
			{
				for (int i = 0; i < reward.population; i++)
				{
					state.ActionStack.Add(new DecreasePopulationAction(base.PlayerId, tile.rulingCityCoordinates, 200));
				}
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.ImprovementLevelDown;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		if (version < 40)
		{
			SerializeV1(writer, version);
		}
		else
		{
			SerializeDefault(writer, version);
		}
	}

	private void SerializeV1(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
	}

	private void SerializeDefault(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
		writer.Write(Amount);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		if (version < 40)
		{
			DeserializeV1(reader, version);
		}
		else
		{
			DeserializeDefault(reader, version);
		}
	}

	private void DeserializeV1(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
	}

	private void DeserializeDefault(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
		Amount = reader.ReadUInt16();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates})";
	}
}
