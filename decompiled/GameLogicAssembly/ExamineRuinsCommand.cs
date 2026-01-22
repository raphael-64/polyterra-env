using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class ExamineRuinsCommand : CommandBase
{
	public WorldCoordinates Coordinates { get; private set; }

	public ExamineRuinsCommand()
	{
	}

	public ExamineRuinsCommand(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		if (!PassesBasicValidation(state, out validationError))
		{
			return false;
		}
		if (state.Version < 27)
		{
			return true;
		}
		if (base.PlayerId == byte.MaxValue)
		{
			return false;
		}
		if (state.Version >= 51)
		{
			TileData tile = state.Map.GetTile(Coordinates);
			if (tile.improvement == null || tile.improvement.type != ImprovementData.Type.Ruin)
			{
				return false;
			}
		}
		return true;
	}

	public override bool NeedServerConfirmation()
	{
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		TileData tile = state.Map.GetTile(Coordinates);
		RuinsReward ruinsReward = GetRuinsReward(state, base.PlayerId, tile);
		state.ActionStack.Add(new ExamineRuinsAction(base.PlayerId, ruinsReward, Coordinates));
		if (tile.unit != null && tile.unit.HasEffect(UnitEffect.Invisible))
		{
			state.ActionStack.Add(new RevealAction(base.PlayerId, Coordinates));
		}
	}

	private RuinsReward GetRuinsReward(GameState state, byte playerId, TileData tile)
	{
		List<RuinsReward> list = new List<RuinsReward>
		{
			RuinsReward.Resources,
			RuinsReward.PopulationGrowth
		};
		state.TryGetPlayer(playerId, out var playerState);
		if (state.Version < 50)
		{
			if (state.CurrentTurn >= 5)
			{
				state.GameLogicData.TryGetData(playerState.tribe, out var data);
				state.GameLogicData.TryGetData(UnitData.Type.Giant, out var data2);
				UnitData unit = state.GameLogicData.GetOverride(data2, data);
				if (!tile.IsWater || unit.HasAbility(UnitAbility.Type.Swim))
				{
					list.Add(RuinsReward.SuperUnit);
				}
				else if (data.type == TribeData.Type.Elyrion)
				{
					list.Add(RuinsReward.Seamonster);
				}
				else if (state.Version < 40 || state.GameLogicData.HasUnlockedMovementOnTerrain(playerState, TerrainData.Type.Water))
				{
					list.Add(RuinsReward.Battleship);
				}
			}
		}
		else if (!tile.IsWater)
		{
			list.Add(RuinsReward.Swordsman);
		}
		else if (state.GameLogicData.HasUnlockedMovementOnTerrain(playerState, TerrainData.Type.Water))
		{
			list.Add(RuinsReward.Battleship);
		}
		int num = 0;
		List<TileData> area = state.Map.GetArea(tile.coordinates, 1, allowDiagonal: true, includeCenter: false);
		for (int i = 0; i < area.Count; i++)
		{
			if (area[i].CanBeAccessedByPlayer(state, playerState))
			{
				num++;
			}
		}
		if (state.Version < 43 || num > 0)
		{
			foreach (TileData item in state.Map.GetArea(tile.coordinates, 2, allowDiagonal: true, includeCenter: false))
			{
				if (!item.GetExplored(playerId))
				{
					list.Add(RuinsReward.Explorer);
					break;
				}
			}
		}
		List<TechData> unlockableTech = state.GameLogicData.GetUnlockableTech(playerState);
		if (unlockableTech != null && unlockableTech.Count > 0)
		{
			list.Add(RuinsReward.FreeTech);
		}
		return list[state.RandomHash.Range(0, list.Count, tile.coordinates.X, tile.coordinates.Y)];
	}

	public override CommandType GetCommandType()
	{
		return CommandType.ExamineRuins;
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
