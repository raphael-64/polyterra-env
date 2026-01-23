using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polytopia.Data;

public class InfiltrationRewardAction : ActionBase
{
	public CityReward Reward { get; private set; }

	public WorldCoordinates Coordinates { get; private set; }

	public UnitData.Type UnitType { get; private set; }

	public InfiltrationRewardAction()
	{
	}

	public InfiltrationRewardAction(byte playerId, CityReward reward, WorldCoordinates coordinates, UnitData.Type type)
		: base(playerId)
	{
		Reward = reward;
		Coordinates = coordinates;
		UnitType = type;
	}

	public override void Execute(GameState gameState)
	{
		if (gameState.Version < 80)
		{
			ExecuteV60(gameState);
		}
		else if (gameState.Version < 83)
		{
			ExecuteV82(gameState);
		}
		else
		{
			ExecuteDefault(gameState);
		}
	}

	private void ExecuteDefault(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Coordinates);
		if (tile == null || tile.improvement == null || !gameState.TryGetPlayer(base.PlayerId, out var playerState))
		{
			return;
		}
		ushort level = tile.improvement.level;
		gameState.TryGetPlayer(tile.owner, out var _);
		int amount = tile.CalculateRawProduction(gameState);
		gameState.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, tile.coordinates, amount, 10));
		tile.improvement.AddEffect(ImprovementEffect.robbed);
		if (tile.unit != null)
		{
			UnitState unitState = new UnitState();
			unitState.type = UnitData.Type.Dagger;
			unitState.health = (ushort)unitState.GetMaxHealth(gameState);
			unitState.coordinates = Coordinates;
			BattleResults battleResults = BattleHelpers.GetBattleResults(gameState, unitState, tile.unit);
			ActionUtils.PerformAttack(gameState, base.PlayerId, Coordinates, Coordinates, battleResults.attackDamage);
		}
		int num = 0;
		int num2 = Math.Min((int)level, 5);
		List<TileData> list = new List<TileData>();
		foreach (TileData item in gameState.Map.GetArea(Coordinates, tile.improvement.borderSize, allowDiagonal: true))
		{
			if (item.rulingCityCoordinates == tile.coordinates && item.unit == null && item.CanBeAccessedByPlayer(gameState, playerState))
			{
				list.Add(item);
			}
		}
		list.Sort((TileData a, TileData b) => Evaluate(b).CompareTo(Evaluate(a)));
		foreach (TileData item2 in list)
		{
			if (num >= num2)
			{
				break;
			}
			gameState.ActionStack.Add(new TrainAction(base.PlayerId, UnitData.Type.Dagger, item2.coordinates, 0, Coordinates));
			num++;
		}
		float Evaluate(TileData tileToEvaluate)
		{
			float num3 = 0f;
			uint hash = gameState.RandomHash.GetHash(tileToEvaluate.coordinates.X, tileToEvaluate.coordinates.Y, (int)gameState.CurrentTurn);
			num3 += gameState.RandomHash.Range(0f, 0.1f, (int)hash);
			if (tileToEvaluate.HasImprovement(ImprovementData.Type.City))
			{
				num3 += 2f;
			}
			if (tileToEvaluate.IsWater)
			{
				num3 -= 1.6f;
			}
			if (tileToEvaluate.GetResource(gameState, base.PlayerId) != null)
			{
				num3 += 0.5f;
			}
			if (playerState.GetDefenceBonus(tileToEvaluate.terrain, gameState) > 1)
			{
				num3 += 1f;
			}
			return num3;
		}
	}

	private void ExecuteV82(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Coordinates);
		if (tile == null || tile.improvement == null || !gameState.TryGetPlayer(base.PlayerId, out var playerState))
		{
			return;
		}
		switch (Reward)
		{
		case CityReward.StealResources:
		{
			int improvementLevel = ActionUtils.UpdateImprovementLevel(gameState, base.PlayerId, tile);
			int num = tile.CalculateWork(gameState, improvementLevel);
			for (uint num2 = 0u; num2 < num; num2++)
			{
				gameState.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, 40));
			}
			tile.improvement.AddEffect(ImprovementEffect.robbed);
			break;
		}
		case CityReward.Poison:
		{
			TileData[] areaSorted = gameState.Map.GetAreaSorted(Coordinates, 1, allowDiagonal: true);
			gameState.TryGetPlayer(base.PlayerId, out var _);
			TileData[] array = areaSorted;
			foreach (TileData tileData in array)
			{
				if (tileData.unit != null)
				{
					gameState.ActionStack.Add(new PoisonUnitAction(base.PlayerId, tileData.coordinates, tileData.coordinates));
					gameState.ActionStack.Add(new AttackAction(base.PlayerId, tileData.coordinates, tileData.coordinates, 20, shouldMoveToTarget: false, AttackAction.AnimationType.None, 0));
				}
			}
			break;
		}
		case CityReward.Rebellion:
		{
			tile.improvement.AddEffect(ImprovementEffect.robbed);
			int val = ActionUtils.UpdateImprovementLevel(gameState, base.PlayerId, tile);
			int num3 = 0;
			int num4 = Math.Min(val, 5);
			List<TileData> list2 = new List<TileData>();
			foreach (TileData item in gameState.Map.GetArea(Coordinates, tile.improvement.borderSize, allowDiagonal: true, includeCenter: false))
			{
				if (item.rulingCityCoordinates == tile.coordinates && item.unit == null && item.CanBeAccessedByPlayer(gameState, playerState))
				{
					list2.Add(item);
				}
			}
			list2.Sort((TileData a, TileData b) => Evaluate(b).CompareTo(Evaluate(a)));
			{
				foreach (TileData item2 in list2)
				{
					if (num3 >= num4)
					{
						break;
					}
					gameState.ActionStack.Add(new TrainAction(base.PlayerId, UnitData.Type.Dagger, item2.coordinates, 0, Coordinates));
					num3++;
				}
				break;
			}
		}
		case CityReward.StealTech:
		{
			if (tile.owner == 0 || !gameState.TryGetPlayer(tile.owner, out var playerState2))
			{
				break;
			}
			List<TechData> unlockableTech = gameState.GameLogicData.GetUnlockableTech(playerState);
			List<TechData.Type> list = new List<TechData.Type>();
			foreach (TechData item3 in unlockableTech)
			{
				if (playerState2.HasTech(item3.type))
				{
					list.Add(item3.type);
				}
			}
			if (list.Count > 0)
			{
				TechData.Type type = list[gameState.RandomHash.Range(0, list.Count, Coordinates.X, Coordinates.Y)];
				gameState.ActionStack.Add(new ResearchAction(base.PlayerId, type, 0));
			}
			else
			{
				Log.Verbose("[felix] No tech was found!", Array.Empty<object>());
			}
			break;
		}
		case CityReward.Spy:
			break;
		}
		float Evaluate(TileData tileToEvaluate)
		{
			float num5 = 0f;
			uint hash = gameState.RandomHash.GetHash(tileToEvaluate.coordinates.X, tileToEvaluate.coordinates.Y, (int)gameState.CurrentTurn);
			num5 += gameState.RandomHash.Range(0f, 0.1f, (int)hash);
			if (tileToEvaluate.IsWater)
			{
				num5 -= 1f;
			}
			if (tileToEvaluate.GetResource(gameState, base.PlayerId) != null)
			{
				num5 += 0.5f;
			}
			if (tileToEvaluate.improvement != null)
			{
				num5 -= 0.5f;
			}
			if (playerState.GetDefenceBonus(tileToEvaluate.terrain, gameState) > 1)
			{
				num5 += 1f;
			}
			return num5;
		}
	}

	private void ExecuteV60(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Coordinates);
		if (tile == null || tile.improvement == null || !gameState.TryGetPlayer(base.PlayerId, out var playerState))
		{
			return;
		}
		if (VersionManager.GameVersion == 60)
		{
			int rewardCost = CityRewardData.GetRewardCost(Reward);
			playerState.Currency -= rewardCost;
		}
		switch (Reward)
		{
		case CityReward.StealResources:
		{
			int improvementLevel = ActionUtils.UpdateImprovementLevel(gameState, base.PlayerId, tile);
			int num = tile.CalculateWork(gameState, improvementLevel);
			for (uint num2 = 0u; num2 < num; num2++)
			{
				gameState.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, 40));
			}
			tile.improvement.AddEffect(ImprovementEffect.robbed);
			break;
		}
		case CityReward.Poison:
		{
			TileData[] areaSorted = gameState.Map.GetAreaSorted(Coordinates, 1, allowDiagonal: true);
			gameState.TryGetPlayer(base.PlayerId, out var _);
			TileData[] array = areaSorted;
			foreach (TileData tileData in array)
			{
				if (tileData.unit != null)
				{
					gameState.ActionStack.Add(new PoisonUnitAction(base.PlayerId, tileData.coordinates, tileData.coordinates));
					gameState.ActionStack.Add(new AttackAction(base.PlayerId, tileData.coordinates, tileData.coordinates, 20, shouldMoveToTarget: false, AttackAction.AnimationType.None, 0));
				}
			}
			break;
		}
		case CityReward.Rebellion:
		{
			int val = ActionUtils.UpdateImprovementLevel(gameState, base.PlayerId, tile);
			int num3 = 0;
			int num4 = Math.Min(val, 4);
			List<TileData> list2 = new List<TileData>();
			foreach (TileData item in gameState.Map.GetArea(Coordinates, 1, allowDiagonal: true, includeCenter: false))
			{
				if (item.unit == null && item.CanBeAccessedByPlayer(gameState, playerState))
				{
					list2.Add(item);
				}
			}
			{
				foreach (TileData item2 in list2.OrderBy((TileData x) => x.IsWater))
				{
					if (num3 >= num4)
					{
						break;
					}
					gameState.ActionStack.Add(new TrainAction(base.PlayerId, UnitData.Type.Dagger, item2.coordinates, 0, Coordinates));
					num3++;
				}
				break;
			}
		}
		case CityReward.StealTech:
		{
			if (tile.owner == 0 || !gameState.TryGetPlayer(tile.owner, out var playerState2))
			{
				break;
			}
			List<TechData> unlockableTech = gameState.GameLogicData.GetUnlockableTech(playerState);
			List<TechData.Type> list = new List<TechData.Type>();
			foreach (TechData item3 in unlockableTech)
			{
				if (playerState2.HasTech(item3.type))
				{
					list.Add(item3.type);
				}
			}
			if (list.Count > 0)
			{
				TechData.Type type = list[gameState.RandomHash.Range(0, list.Count, Coordinates.X, Coordinates.Y)];
				gameState.ActionStack.Add(new ResearchAction(base.PlayerId, type, 0));
			}
			else
			{
				Log.Verbose("[felix] No tech was found!", Array.Empty<object>());
			}
			break;
		}
		case CityReward.Spy:
			break;
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.InfiltrationReward;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
		writer.Write((ushort)Reward);
		if (version >= 80)
		{
			writer.Write((ushort)UnitType);
		}
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
		Reward = (CityReward)reader.ReadUInt16();
		if (version >= 80)
		{
			UnitType = (UnitData.Type)reader.ReadUInt16();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates}, Reward: {Reward})";
	}
}
