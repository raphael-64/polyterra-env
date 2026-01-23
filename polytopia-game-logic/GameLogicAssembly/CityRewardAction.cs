using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class CityRewardAction : ActionBase
{
	public CityReward Reward { get; private set; }

	public WorldCoordinates Coordinates { get; private set; }

	public CityRewardAction()
	{
	}

	public CityRewardAction(byte playerId, CityReward reward, WorldCoordinates coordinates)
		: base(playerId)
	{
		Reward = reward;
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile == null || tile.improvement == null || !state.TryGetPlayer(base.PlayerId, out var _))
		{
			return;
		}
		tile.improvement.AddReward(Reward);
		switch (Reward)
		{
		case CityReward.Park:
			state.ActionStack.Add(new ModifyScoreAction(base.PlayerId, Coordinates, 250));
			state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, 250, Coordinates, 50));
			break;
		case CityReward.Workshop:
			state.ActionStack.Add(new ModifyProductionAction(base.PlayerId, 1, Coordinates));
			break;
		case CityReward.Explorer:
			state.ActionStack.Add(new ScoutMoveAction(base.PlayerId, state.GetNextUnitId(), 15u, state.RandomHash.GetHash(tile.coordinates.X, tile.coordinates.Y), Coordinates, new List<WorldCoordinates>()));
			break;
		case CityReward.BorderGrowth:
			AddBorderGrowthActions(state, tile);
			break;
		case CityReward.SuperUnit:
			ActionUtils.TrainUnitOnOccupiedSpace(state, base.PlayerId, UnitData.Type.Giant, tile);
			break;
		case CityReward.Resources:
		{
			for (uint num = 0u; num < 5; num++)
			{
				int num2 = 40;
				if (num == 4)
				{
					num2 += 150;
				}
				state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, num2));
			}
			break;
		}
		case CityReward.PopulationGrowth:
		{
			for (int i = 0; i < 3; i++)
			{
				state.ActionStack.Add(new IncreasePopulationAction(base.PlayerId, Coordinates, Coordinates));
			}
			break;
		}
		case CityReward.TutorialExplorer:
			state.ActionStack.Add(new ScoutMoveAction(base.PlayerId, state.GetNextUnitId(), 15u, state.RandomHash.GetHash(tile.coordinates.X, tile.coordinates.Y), Coordinates, new List<WorldCoordinates>(), ScoutMoveAction.ScoutBehaviourType.GoToNearestVillage));
			break;
		}
	}

	private void AddBorderGrowthActions(GameState state, TileData tile)
	{
		tile.improvement.borderSize++;
		if (state.Version < 13)
		{
			AddSubAction(new RuleAreaAction(base.PlayerId, tile.coordinates));
		}
		else
		{
			AddSubAction(new ExpandCityAction(base.PlayerId, tile.coordinates));
		}
		AddSubAction(new UpdateRoutesAction(base.PlayerId));
		List<byte> list = new List<byte>(state.PlayerCount - 1);
		foreach (PlayerState playerState2 in state.PlayerStates)
		{
			if (playerState2.Id != base.PlayerId && playerState2.Id != byte.MaxValue)
			{
				list.Add(playerState2.Id);
			}
		}
		AddSubAction(new UpdateTransportConnectionAction(base.PlayerId, list));
		if (state.TryGetPlayer(base.PlayerId, out var playerState) && playerState.tribe == TribeData.Type.Polaris)
		{
			AddSubAction(new FreezeAreaAction(base.PlayerId, tile.coordinates, tile.improvement.borderSize, freezeUnits: false, onlyOwnedTiles: true));
		}
		CommitSubActionsToStack(state.ActionStack);
	}

	public override ActionType GetActionType()
	{
		return ActionType.CityReward;
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
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates}, Reward: {Reward})";
	}
}
