using System;
using System.Collections.Generic;
using Polytopia.Data;

public static class ActionUtils
{
	public static void AddScore(GameState gameState, PlayerState playerState, int score)
	{
		playerState.score = (uint)(playerState.score + score);
		if (gameState.Settings.rules.ScoreLimit > 0 && playerState.score >= gameState.Settings.rules.ScoreLimit && gameState.CurrentState == GameState.State.Started && gameState.TryGetWinner(out var winner))
		{
			gameState.ActionStack.Add(new GameOverAction(playerState.Id, winner.Id));
		}
		if (!playerState.AutoPlay)
		{
			Log.Learn("score: {0}", new object[1] { playerState.score });
		}
	}

	public static void RemoveScore(PlayerState playerState, int score)
	{
		playerState.score = (uint)(playerState.score - score);
		if (!playerState.AutoPlay)
		{
			Log.Learn("score: {0}", new object[1] { playerState.score });
		}
	}

	public static ushort GetHealAmount(GameState gameState, TileData tileData)
	{
		ushort val = 20;
		if (gameState.TryGetPlayer(tileData.unit.owner, out var playerState) && (tileData.owner == tileData.unit.owner || playerState.HasPeaceWith(tileData.owner)))
		{
			val = 40;
		}
		if (gameState.Version < 83)
		{
			val = (ushort)((tileData.owner == tileData.unit.owner) ? 40u : 20u);
		}
		ushort val2 = (ushort)(tileData.unit.GetMaxHealth(gameState) - tileData.unit.health);
		return Math.Min(val, val2);
	}

	public static void LearnTech(GameState gameState, PlayerState playerState, TechData.Type type, int cost, bool shouldUseActions)
	{
		if (playerState.availableTech.Contains(type))
		{
			return;
		}
		playerState.availableTech.Add(type);
		playerState.Currency -= cost;
		if (shouldUseActions)
		{
			gameState.ActionStack.Add(new IncreaseScoreAction(playerState.Id, (int)ScoreSheet.GetTechScore(gameState, type), playerState.startTile));
		}
		else
		{
			AddScore(gameState, playerState, (int)ScoreSheet.GetTechScore(gameState, type));
		}
		if (gameState.GameLogicData.TryGetData(type, out var data))
		{
			if (data.taskUnlocks != null && data.taskUnlocks.Count > 0)
			{
				for (int i = 0; i < data.taskUnlocks.Count; i++)
				{
					EnableTask(gameState, playerState, data.taskUnlocks[i].type);
				}
			}
			if (data.abilityUnlocks.Contains(PlayerAbility.Type.CapitalVision))
			{
				TryRevealAllCapitals(gameState, playerState);
			}
		}
		gameState.CheckTask(playerState, TaskData.Type.Genius);
	}

	public static void TrainUnitOnOccupiedSpace(GameState gameState, byte playerId, UnitData.Type unitType, TileData tile)
	{
		gameState.TryGetPlayer(playerId, out var playerState);
		gameState.GameLogicData.TryGetData(playerState.tribe, out var data);
		gameState.GameLogicData.TryGetData(unitType, out var data2);
		UnitData unitData = gameState.GameLogicData.GetOverride(data2, data);
		gameState.ActionStack.Add(new TrainAction(playerId, unitData.type, tile.coordinates, 0));
		if (tile.unit != null && !((gameState.Version <= 8) ? TryPushUnit8(gameState, playerId, tile) : ((gameState.Version <= 17) ? TryPushUnit17(gameState, playerId, tile) : ((gameState.Version <= 19) ? TryPushUnit19(gameState, playerId, tile) : ((gameState.Version <= 26) ? TryPushUnit26(gameState, playerId, tile) : ((gameState.Version <= 42) ? TryPushUnit42(gameState, playerId, tile) : ((gameState.Version > 43) ? TryPushUnitDefault(gameState, playerId, tile) : TryPushUnit43(gameState, playerId, tile))))))))
		{
			if (gameState.Version < 27)
			{
				KillUnit(gameState, tile);
			}
			else
			{
				gameState.ActionStack.Add(new KillUnitAction(playerId, tile.coordinates));
			}
		}
	}

	public static bool TryPushUnitDefault(GameState gameState, byte playerId, TileData tile)
	{
		GridDirection gridDirection = ((playerId == tile.unit.owner) ? tile.unit.direction : tile.unit.direction.Opposite());
		if (tile.unit.HasFollower() && tile.unit.HasLeader())
		{
			GridDirection firstDirection = tile.unit.LeaderDirection(gameState);
			GridDirection secondDirection = tile.unit.FollowerDirection(gameState);
			gridDirection = GridDirections.Average(firstDirection, secondDirection);
		}
		else if (tile.unit.HasFollower())
		{
			gridDirection = tile.unit.FollowerDirection(gameState);
		}
		else if (tile.unit.HasLeader())
		{
			gridDirection = tile.unit.LeaderDirection(gameState);
		}
		gameState.GameLogicData.TryGetData(tile.unit.type, out var data);
		bool flag = data.IsVehicle();
		TileData tileData = null;
		for (int i = 0; i < GridDirections.COUNT; i++)
		{
			int num = (i + 1) / 2 * ((i % 2 == 0) ? 1 : (-1));
			GridDirection direction = (GridDirection)((int)(gridDirection + num + GridDirections.COUNT) % GridDirections.COUNT);
			TileData tile2 = gameState.Map.GetTile(tile.coordinates + direction.ToCoordinates());
			if (tile2 == null || tile2.unit != null)
			{
				continue;
			}
			if (tile.IsWater && !tile2.IsWater && flag && tileData == null)
			{
				tileData = tile2;
				continue;
			}
			List<WorldCoordinates> path = gameState.GetPath(tile.coordinates, tile2.coordinates, 1, tile.unit);
			if (path != null)
			{
				gameState.ActionStack.Add(new MoveAction(tile.unit.owner, tile.unit.id, path, MoveAction.MoveReason.Push));
				return true;
			}
		}
		if (tileData != null)
		{
			List<WorldCoordinates> path2 = gameState.GetPath(tile.coordinates, tileData.coordinates, 1, tile.unit);
			if (path2 != null)
			{
				gameState.ActionStack.Add(new MoveAction(tile.unit.owner, tile.unit.id, path2, MoveAction.MoveReason.Push));
				return true;
			}
		}
		return false;
	}

	public static bool TryPushUnit43(GameState gameState, byte playerId, TileData tile)
	{
		GridDirection gridDirection = ((!tile.unit.HasFollower()) ? ((playerId == tile.unit.owner) ? tile.unit.direction : tile.unit.direction.Opposite()) : GridDirection.SW);
		gameState.GameLogicData.TryGetData(tile.unit.type, out var data);
		bool flag = data.IsVehicle();
		TileData tileData = null;
		for (int i = 0; i < GridDirections.COUNT; i++)
		{
			int num = (i + 1) / 2 * ((i % 2 == 0) ? 1 : (-1));
			GridDirection direction = (GridDirection)((int)(gridDirection + num + GridDirections.COUNT) % GridDirections.COUNT);
			TileData tile2 = gameState.Map.GetTile(tile.coordinates + direction.ToCoordinates());
			if (tile2 == null || tile2.unit != null)
			{
				continue;
			}
			if (tile.IsWater && !tile2.IsWater && flag && tileData == null)
			{
				tileData = tile2;
				continue;
			}
			List<WorldCoordinates> path = gameState.GetPath(tile.coordinates, tile2.coordinates, 1, tile.unit);
			if (path != null)
			{
				gameState.ActionStack.Add(new MoveAction(tile.unit.owner, tile.unit.id, path, MoveAction.MoveReason.Push));
				return true;
			}
		}
		if (tileData != null)
		{
			List<WorldCoordinates> path2 = gameState.GetPath(tile.coordinates, tileData.coordinates, 1, tile.unit);
			if (path2 != null)
			{
				gameState.ActionStack.Add(new MoveAction(tile.unit.owner, tile.unit.id, path2, MoveAction.MoveReason.Push));
				return true;
			}
		}
		return false;
	}

	public static bool TryPushUnit42(GameState gameState, byte playerId, TileData tile)
	{
		GridDirection gridDirection = ((playerId == tile.unit.owner) ? tile.unit.direction : tile.unit.direction.Opposite());
		gameState.GameLogicData.TryGetData(tile.unit.type, out var data);
		bool flag = data.IsVehicle();
		TileData tileData = null;
		for (int i = 0; i < GridDirections.COUNT; i++)
		{
			int num = (i + 1) / 2 * ((i % 2 == 0) ? 1 : (-1));
			GridDirection direction = (GridDirection)((int)(gridDirection + num + GridDirections.COUNT) % GridDirections.COUNT);
			TileData tile2 = gameState.Map.GetTile(tile.coordinates + direction.ToCoordinates());
			if (tile2 == null || tile2.unit != null)
			{
				continue;
			}
			if (tile.IsWater && !tile2.IsWater && flag && tileData == null)
			{
				tileData = tile2;
				continue;
			}
			List<WorldCoordinates> path = gameState.GetPath(tile.coordinates, tile2.coordinates, 1, tile.unit);
			if (path != null)
			{
				gameState.ActionStack.Add(new MoveAction(tile.unit.owner, tile.unit.id, path, MoveAction.MoveReason.Push));
				return true;
			}
		}
		if (tileData != null)
		{
			List<WorldCoordinates> path2 = gameState.GetPath(tile.coordinates, tileData.coordinates, 1, tile.unit);
			if (path2 != null)
			{
				gameState.ActionStack.Add(new MoveAction(tile.unit.owner, tile.unit.id, path2, MoveAction.MoveReason.Push));
				return true;
			}
		}
		return false;
	}

	public static bool TryPushUnit26(GameState gameState, byte playerId, TileData tile)
	{
		GridDirection gridDirection = ((playerId == tile.unit.owner) ? tile.unit.direction : tile.unit.direction.Opposite());
		gameState.GameLogicData.TryGetData(tile.unit.type, out var data);
		bool flag = data.IsVehicle();
		TileData tileData = null;
		for (int i = 0; i < GridDirections.COUNT; i++)
		{
			int num = (i + 1) / 2 * ((i % 2 == 0) ? 1 : (-1));
			GridDirection direction = (GridDirection)((int)(gridDirection + num + GridDirections.COUNT) % GridDirections.COUNT);
			TileData tile2 = gameState.Map.GetTile(tile.coordinates + direction.ToCoordinates());
			if (tile2 == null || tile2.unit != null)
			{
				continue;
			}
			if (tile.IsWater && !tile2.IsWater && flag && tileData == null)
			{
				tileData = tile2;
				continue;
			}
			List<WorldCoordinates> path = gameState.GetPath(tile.coordinates, tile2.coordinates, 1, tile.unit);
			if (path != null)
			{
				gameState.ActionStack.Add(new MoveAction(playerId, tile.unit.id, path, MoveAction.MoveReason.Push));
				return true;
			}
		}
		if (tileData != null)
		{
			List<WorldCoordinates> path2 = gameState.GetPath(tile.coordinates, tileData.coordinates, 1, tile.unit);
			if (path2 != null)
			{
				gameState.ActionStack.Add(new MoveAction(playerId, tile.unit.id, path2, MoveAction.MoveReason.Push));
				return true;
			}
		}
		return false;
	}

	public static bool TryPushUnit19(GameState gameState, byte playerId, TileData tile)
	{
		GridDirection gridDirection = ((playerId == tile.unit.owner) ? tile.unit.direction : tile.unit.direction.Opposite());
		for (int i = 0; i < GridDirections.COUNT; i++)
		{
			int num = (i + 1) / 2 * ((i % 2 == 0) ? 1 : (-1));
			GridDirection direction = (GridDirection)((int)(gridDirection + num + GridDirections.COUNT) % GridDirections.COUNT);
			TileData tile2 = gameState.Map.GetTile(tile.coordinates + direction.ToCoordinates());
			if (tile2 != null && tile2.unit == null)
			{
				List<WorldCoordinates> path = gameState.GetPath(tile.coordinates, tile2.coordinates, 1, tile.unit);
				if (path != null)
				{
					gameState.ActionStack.Add(new MoveAction(playerId, tile.unit.id, path, MoveAction.MoveReason.Push));
					return true;
				}
			}
		}
		return false;
	}

	public static bool TryPushUnit17(GameState gameState, byte playerId, TileData tile)
	{
		GridDirection gridDirection = ((playerId == tile.unit.owner) ? tile.unit.direction : tile.unit.direction.Opposite());
		for (int i = 0; i < GridDirections.COUNT; i++)
		{
			int num = (i + 1) / 2 * ((i % 2 == 0) ? 1 : (-1));
			GridDirection direction = (GridDirection)((int)(gridDirection + num + GridDirections.COUNT) % GridDirections.COUNT);
			TileData tile2 = gameState.Map.GetTile(tile.coordinates + direction.ToCoordinates());
			if (tile2 != null && tile2.unit == null)
			{
				List<WorldCoordinates> path = gameState.GetPath(tile.coordinates, tile2.coordinates, 1, tile.unit);
				if (path != null)
				{
					gameState.ActionStack.Add(new MoveAction(playerId, tile.unit.id, path));
					return true;
				}
			}
		}
		return false;
	}

	public static bool TryPushUnit8(GameState gameState, byte playerId, TileData tile)
	{
		foreach (TileData tileNeighbor in gameState.Map.GetTileNeighbors(tile.coordinates))
		{
			if (tileNeighbor != null && tileNeighbor.unit == null)
			{
				List<WorldCoordinates> path = gameState.GetPath(tile.coordinates, tileNeighbor.coordinates, 1, tile.unit);
				if (path != null)
				{
					gameState.ActionStack.Add(new MoveAction(playerId, tile.unit.id, path));
					return true;
				}
			}
		}
		return false;
	}

	public static UnitState TrainUnitScored(GameState gameState, PlayerState playerState, TileData tile, UnitData unitData)
	{
		AddScore(gameState, playerState, (int)ScoreSheet.GetUnitScore(unitData));
		return TrainUnit(gameState, playerState, tile, unitData);
	}

	public static UnitState TrainUnit(GameState gameState, PlayerState playerState, TileData tile, UnitData unitData)
	{
		WorldCoordinates home = tile.coordinates;
		if (gameState.Version > 21 && !tile.HasImprovement(ImprovementData.Type.City) && tile.rulingCityCoordinates != WorldCoordinates.NULL_COORDINATES)
		{
			home = tile.rulingCityCoordinates;
		}
		UnitState unitState = UnitState.Create(gameState, playerState.Id, (ushort)gameState.CurrentTurn, unitData, tile.coordinates, home);
		unitState.id = gameState.GetNextUnitId();
		unitState.moved = true;
		unitState.attacked = true;
		if (unitData.HasDynamicStyle())
		{
			unitState.style = (short)tile.climate;
			unitState.skinType = tile.skinType;
		}
		if (unitData.IsVehicle())
		{
			gameState.GameLogicData.TryGetData(UnitData.Type.Warrior, out var data);
			unitState.health = (unitState.passengerUnit = TrainUnit(gameState, playerState, tile, data)).health;
			tile.SetUnit(unitState);
			CheckStepOnPoison(tile, unitState, gameState);
		}
		else
		{
			tile.SetUnit(unitState);
			CheckStepOnPoison(tile, unitState, gameState);
		}
		Log.Verbose("Player {0} created a {1} with id {2} @{3}, score {4}", new object[5]
		{
			playerState.Id,
			unitData.type,
			unitState.id,
			tile.coordinates,
			(uint)ScoreSheet.GetUnitScore(unitData)
		});
		return unitState;
	}

	public static void PerformAttack(GameState gameState, byte playerId, WorldCoordinates origin, WorldCoordinates target, int damage)
	{
		if (gameState.Version <= 16)
		{
			PerformAttackV16(gameState, playerId, origin, target, damage);
		}
		else if (gameState.Version <= 21)
		{
			PerformAttackV21(gameState, playerId, origin, target, damage);
		}
		else if (gameState.Version <= 22)
		{
			PerformAttackV22(gameState, playerId, origin, target, damage);
		}
		else if (gameState.Version <= 45)
		{
			PerformAttackV45(gameState, playerId, origin, target, damage);
		}
		else if (gameState.Version <= 50)
		{
			PerformAttackV50(gameState, playerId, origin, target, damage);
		}
		else
		{
			PerformAttackDefault(gameState, playerId, origin, target, damage);
		}
	}

	private static void PerformAttackDefault(GameState gameState, byte playerId, WorldCoordinates origin, WorldCoordinates target, int damage)
	{
		TileData tile = gameState.Map.GetTile(origin);
		TileData tile2 = gameState.Map.GetTile(target);
		UnitState unit = tile.unit;
		UnitState unit2 = tile2.unit;
		unit.SetUnitDirection(origin, target);
		unit2.SetUnitDirection(target, origin);
		unit2.health -= (ushort)Math.Min(damage, unit2.health);
		unit.RemoveEffect(UnitEffect.Boosted);
		unit2.RemoveEffect(UnitEffect.Boosted);
		byte playerId2 = ((origin == target) ? playerId : unit.owner);
		gameState.TryGetPlayer(playerId2, out var playerState);
		if (unit2.health == 0)
		{
			if (unit2.owner != byte.MaxValue)
			{
				playerState.kills++;
				if (origin != WorldCoordinates.NULL_COORDINATES)
				{
					gameState.GameLogicData.TryGetData(unit.type, out var data);
					if (!data.IsVehicle() && !data.hidden)
					{
						unit.xp++;
					}
				}
				EnableTask(gameState, playerState, TaskData.Type.Killer);
				if (gameState.TryGetTask(playerState, TaskData.Type.Killer, out var task) && task.Bump(gameState))
				{
					gameState.CheckTask(playerState, task);
				}
			}
			gameState.TryGetPlayer(unit2.owner, out var playerState2);
			playerState2.casualities++;
			gameState.ActionStack.Add(new KillUnitAction(playerId, target));
		}
		else if (unit.HasAbility(UnitAbility.Type.Poison, gameState))
		{
			gameState.ActionStack.Add(new PoisonUnitAction(playerId, origin, target));
		}
	}

	private static void PerformAttackV50(GameState gameState, byte playerId, WorldCoordinates origin, WorldCoordinates target, int damage)
	{
		TileData tile = gameState.Map.GetTile(origin);
		TileData tile2 = gameState.Map.GetTile(target);
		UnitState unit = tile.unit;
		UnitState unit2 = tile2.unit;
		unit.SetUnitDirection(origin, target);
		unit2.SetUnitDirection(target, origin);
		unit2.health -= (ushort)Math.Min(damage, unit2.health);
		unit.RemoveEffect(UnitEffect.Boosted);
		byte playerId2 = ((origin == target) ? playerId : unit.owner);
		gameState.TryGetPlayer(playerId2, out var playerState);
		if (unit2.health == 0)
		{
			if (unit2.owner != byte.MaxValue)
			{
				playerState.kills++;
				if (origin != WorldCoordinates.NULL_COORDINATES)
				{
					gameState.GameLogicData.TryGetData(unit.type, out var data);
					if (!data.IsVehicle() && !data.hidden)
					{
						unit.xp++;
					}
				}
				EnableTask(gameState, playerState, TaskData.Type.Killer);
				if (gameState.TryGetTask(playerState, TaskData.Type.Killer, out var task) && task.Bump(gameState))
				{
					gameState.CheckTask(playerState, task);
				}
			}
			gameState.TryGetPlayer(unit2.owner, out var playerState2);
			playerState2.casualities++;
			gameState.ActionStack.Add(new KillUnitAction(playerId, target));
		}
		else if (unit.HasAbility(UnitAbility.Type.Poison, gameState))
		{
			gameState.ActionStack.Add(new PoisonUnitAction(playerId, origin, target));
		}
	}

	private static void PerformAttackV16(GameState gameState, byte playerId, WorldCoordinates origin, WorldCoordinates target, int damage)
	{
		TileData tile = gameState.Map.GetTile(origin);
		TileData tile2 = gameState.Map.GetTile(target);
		UnitState unit = tile.unit;
		UnitState unit2 = tile2.unit;
		unit.SetUnitDirection(origin, target);
		unit2.SetUnitDirection(target, origin);
		unit2.health -= (ushort)Math.Min(damage, unit2.health);
		gameState.TryGetPlayer(unit.owner, out var playerState);
		if (unit2.health != 0)
		{
			return;
		}
		gameState.TryGetPlayer(unit2.owner, out var playerState2);
		playerState.kills++;
		playerState2.casualities++;
		if (origin != WorldCoordinates.NULL_COORDINATES)
		{
			gameState.GameLogicData.TryGetData(unit.type, out var data);
			if (!data.IsVehicle() && !data.hidden)
			{
				unit.xp++;
			}
		}
		gameState.ActionStack.Add(new KillUnitAction(playerId, target));
		EnableTask(gameState, playerState, TaskData.Type.Killer);
		if (gameState.TryGetTask(playerState, TaskData.Type.Killer, out var task) && task.Bump(gameState))
		{
			gameState.CheckTask(playerState, task);
		}
	}

	private static void PerformAttackV21(GameState gameState, byte playerId, WorldCoordinates origin, WorldCoordinates target, int damage)
	{
		PerformAttackV45(gameState, playerId, origin, target, damage);
	}

	private static void PerformAttackV22(GameState gameState, byte playerId, WorldCoordinates origin, WorldCoordinates target, int damage)
	{
		TileData tile = gameState.Map.GetTile(origin);
		TileData tile2 = gameState.Map.GetTile(target);
		UnitState unit = tile.unit;
		UnitState unit2 = tile2.unit;
		unit.SetUnitDirection(origin, target);
		unit2.SetUnitDirection(target, origin);
		unit2.health -= (ushort)Math.Min(damage, unit2.health);
		gameState.TryGetPlayer(unit.owner, out var playerState);
		if (unit2.health != 0)
		{
			return;
		}
		if (unit2.owner != byte.MaxValue)
		{
			playerState.kills++;
			if (origin != WorldCoordinates.NULL_COORDINATES)
			{
				gameState.GameLogicData.TryGetData(unit.type, out var data);
				if (data.IsVehicle())
				{
					unit.passengerUnit.xp++;
				}
				else if (!data.hidden)
				{
					unit.xp++;
				}
			}
			EnableTask(gameState, playerState, TaskData.Type.Killer);
			if (gameState.TryGetTask(playerState, TaskData.Type.Killer, out var task) && task.Bump(gameState))
			{
				gameState.CheckTask(playerState, task);
			}
		}
		gameState.TryGetPlayer(unit2.owner, out var playerState2);
		playerState2.casualities++;
		gameState.ActionStack.Add(new KillUnitAction(playerId, target));
	}

	private static void PerformAttackV45(GameState gameState, byte playerId, WorldCoordinates origin, WorldCoordinates target, int damage)
	{
		TileData tile = gameState.Map.GetTile(origin);
		TileData tile2 = gameState.Map.GetTile(target);
		UnitState unit = tile.unit;
		UnitState unit2 = tile2.unit;
		unit.SetUnitDirection(origin, target);
		unit2.SetUnitDirection(target, origin);
		unit2.health -= (ushort)Math.Min(damage, unit2.health);
		unit.RemoveEffect(UnitEffect.Boosted);
		gameState.TryGetPlayer(unit.owner, out var playerState);
		if (unit2.health == 0)
		{
			if (unit2.owner != byte.MaxValue)
			{
				playerState.kills++;
				if (origin != WorldCoordinates.NULL_COORDINATES)
				{
					gameState.GameLogicData.TryGetData(unit.type, out var data);
					if (!data.IsVehicle() && !data.hidden)
					{
						unit.xp++;
					}
				}
				EnableTask(gameState, playerState, TaskData.Type.Killer);
				if (gameState.TryGetTask(playerState, TaskData.Type.Killer, out var task) && task.Bump(gameState))
				{
					gameState.CheckTask(playerState, task);
				}
			}
			gameState.TryGetPlayer(unit2.owner, out var playerState2);
			playerState2.casualities++;
			gameState.ActionStack.Add(new KillUnitAction(playerId, target));
		}
		else if (unit.HasAbility(UnitAbility.Type.Poison, gameState))
		{
			gameState.ActionStack.Add(new PoisonUnitAction(playerId, origin, target));
		}
	}

	public static void KillUnit(GameState gameState, TileData tile)
	{
		Log.Verbose("[felix] Kill unit {0}", new object[1] { tile.unit.type });
		tile.unit.health = 0;
		byte owner = tile.unit.owner;
		gameState.GameLogicData.TryGetData(tile.unit.type, out var data);
		if (tile.unit.HasFollower() && gameState.TryGetUnit(tile.unit.follower, out var unit))
		{
			if (gameState.Version >= 41)
			{
				unit.moved = tile.unit.moved;
				unit.attacked = tile.unit.attacked;
			}
			gameState.ActionStack.Add(new UpgradeAction(tile.unit.owner, UnitData.Type.Centipede, unit.coordinates, 0));
		}
		if (tile.unit.HasLeader() && gameState.TryGetUnit(tile.unit.leader, out var unit2))
		{
			unit2.follower = 0u;
		}
		if (tile.unit.HasEffect(UnitEffect.Poisoned))
		{
			SpawnPoisonResource(gameState, tile);
		}
		tile.unit = null;
		if (gameState.Version >= 21)
		{
			gameState.ActionStack.Add(new DecreaseScoreAction(owner, (int)ScoreSheet.GetUnitScore(data)));
		}
	}

	public static void SpawnPoisonResource(GameState gameState, TileData tile)
	{
		if (tile.improvement == null)
		{
			byte owner = tile.owner;
			if (owner == 0)
			{
				owner = tile.unit.owner;
			}
			if (tile.IsWater)
			{
				gameState.GameLogicData.TryGetData(ImprovementData.Type.Algae, out var _);
				gameState.ActionStack.Add(new BuildAction(owner, ImprovementData.Type.Algae, tile.coordinates, deductCost: false));
			}
			else
			{
				gameState.ActionStack.Add(new CreateResourceAction(owner, ResourceData.Type.Spores, tile.coordinates));
			}
		}
	}

	public static void ExploreFromTile(GameState gameState, PlayerState playerState, TileData tile, int sightRange, bool shouldUseActions)
	{
		int radius = ((tile.altitude > 1) ? 2 : sightRange);
		TileData[] areaSorted = gameState.Map.GetAreaSorted(tile.coordinates, radius, allowDiagonal: true);
		if (areaSorted != null)
		{
			for (int num = areaSorted.Length - 1; num >= 0; num--)
			{
				ExploreTile(gameState, playerState, areaSorted[num], shouldUseActions);
			}
		}
		if (!shouldUseActions)
		{
			CheckTribeConnection(gameState, playerState, areaSorted);
		}
	}

	private static void ExploreTile(GameState gameState, PlayerState playerState, TileData tile, bool shouldUseActions)
	{
		if (tile.GetExplored(playerState.Id))
		{
			return;
		}
		if (shouldUseActions)
		{
			gameState.ActionStack.Add(new ExploreAction(playerState.Id, tile.coordinates));
		}
		else if (!tile.GetExplored(playerState.Id))
		{
			AddScore(gameState, playerState, ScoreSheet.exploreValue);
			tile.SetExplored(playerState.Id, explored: true);
			if (gameState.TryGetTask(playerState, TaskData.Type.Explorer, out var task) && task.Bump(gameState))
			{
				gameState.CheckTask(playerState, task);
			}
		}
	}

	public static void FreezeTile(GameState gameState, PlayerState playerState, TileData tile)
	{
		if (!tile.IsFreezable(gameState, playerState))
		{
			return;
		}
		if (tile.IsWater)
		{
			tile.terrain = TerrainData.Type.Ice;
			if (gameState.Version < 44)
			{
				CheckIceBankLevels(gameState);
			}
		}
		tile.climate = playerState.GetTribeClimate(gameState);
		if (gameState.Version >= 44)
		{
			CheckIceBankLevels(gameState);
		}
	}

	public static List<TileData> GetCityArea(GameState gameState, TileData cityTile)
	{
		List<TileData> list = new List<TileData>();
		List<TileData> area = gameState.Map.GetArea(cityTile.coordinates, cityTile.improvement.borderSize, allowDiagonal: true);
		if (area == null || area.Count == 0)
		{
			return list;
		}
		for (int i = 0; i < area.Count; i++)
		{
			TileData tileData = area[i];
			if (tileData.rulingCityCoordinates == WorldCoordinates.NULL_COORDINATES || tileData.rulingCityCoordinates == cityTile.coordinates)
			{
				list.Add(tileData);
			}
		}
		return list;
	}

	public static List<TileData> GetCityAreaSorted(GameState gameState, TileData cityTile)
	{
		List<TileData> list = new List<TileData>();
		TileData[] areaSorted = gameState.Map.GetAreaSorted(cityTile.coordinates, cityTile.improvement.borderSize, allowDiagonal: true);
		if (areaSorted == null || areaSorted.Length == 0)
		{
			return list;
		}
		foreach (TileData tileData in areaSorted)
		{
			if (tileData.rulingCityCoordinates == WorldCoordinates.NULL_COORDINATES || tileData.rulingCityCoordinates == cityTile.coordinates)
			{
				list.Add(tileData);
			}
		}
		return list;
	}

	public static void AddPopulationForTiles(GameState state, List<TileData> tiles, byte playerId, WorldCoordinates rulingCityCoordinates)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			TileData tileData = tiles[i];
			if (tileData.improvement != null)
			{
				int num = tileData.improvement.CalculateImprovementPopulationAtLevel(state, tileData.improvement.level);
				for (int j = 0; j < num; j++)
				{
					state.ActionStack.Add(new IncreasePopulationAction(playerId, tileData.coordinates, rulingCityCoordinates, 60));
				}
			}
		}
	}

	public static void RuleArea(GameState gameState, PlayerState playerState, TileData cityTile, bool shouldUseActions)
	{
		List<TileData> area = gameState.Map.GetArea(cityTile.coordinates, cityTile.improvement.borderSize, allowDiagonal: true);
		if (area == null)
		{
			return;
		}
		for (int i = 0; i < area.Count; i++)
		{
			TileData tileData = area[i];
			if (tileData.rulingCityCoordinates == WorldCoordinates.NULL_COORDINATES || tileData.rulingCityCoordinates == cityTile.coordinates)
			{
				bool num = tileData.owner == playerState.Id;
				PlayerState playerState2;
				bool flag = gameState.TryGetPlayer(tileData.owner, out playerState2);
				tileData.owner = playerState.Id;
				tileData.rulingCityCoordinates = cityTile.coordinates;
				if (tileData.HasImprovement(ImprovementData.Type.City))
				{
					playerState.cities++;
					if (flag)
					{
						playerState2.cities--;
					}
					if (flag && shouldUseActions && !playerState2.IsAlive(gameState, gameState.Settings.rules.PlayerDeathCondition))
					{
						gameState.ActionStack.Add(new WipePlayerAction(playerState.Id, playerState2.Id));
					}
				}
				if (!num)
				{
					int num2 = ScoreSheet.tileValue;
					if (tileData.improvement != null)
					{
						num2 += gameState.CalculateImprovementScore(tileData);
					}
					if (shouldUseActions)
					{
						gameState.ActionStack.Add(new IncreaseScoreAction(playerState.Id, num2, tileData.coordinates, 100));
						if (flag && gameState.Version >= 21)
						{
							gameState.ActionStack.Add(new DecreaseScoreAction(playerState2.Id, num2));
						}
					}
					else
					{
						AddScore(gameState, playerState, num2);
						if (flag && gameState.Version >= 21)
						{
							RemoveScore(playerState2, num2);
						}
					}
				}
			}
			ExploreTile(gameState, playerState, tileData, shouldUseActions);
			CheckSurroundingArea(gameState, playerState.Id, tileData);
		}
	}

	public static void CheckSurroundingArea(GameState gameState, byte playerId, TileData tile)
	{
		List<TileData> area = gameState.Map.GetArea(tile.coordinates, 1, allowDiagonal: true);
		if (area == null || area.Count == 0)
		{
			return;
		}
		for (int i = 0; i < area.Count; i++)
		{
			TileData tileData = area[i];
			if (tileData != null && tileData.improvement != null && gameState.GameLogicData.TryGetData(tileData.improvement.type, out var data) && data.adjacencyImprovements.Count > 0)
			{
				UpdateImprovementLevel(gameState, playerId, tileData);
			}
		}
	}

	public static int UpdateImprovementLevel(GameState gameState, byte playerId, TileData tile)
	{
		int num = CalculateImprovementLevel(gameState, tile);
		if (tile.improvement == null)
		{
			return num;
		}
		if (tile.improvement.level < num)
		{
			int num2 = num - tile.improvement.level;
			for (int i = 0; i < num2; i++)
			{
				gameState.ActionStack.Add(new ImprovementLevelUpAction(playerId, tile.coordinates));
			}
		}
		else if (tile.improvement.level > num)
		{
			int num3 = tile.improvement.level - num;
			for (int j = 0; j < num3; j++)
			{
				gameState.ActionStack.Add(new ImprovementLevelDownAction(playerId, tile.coordinates, 1));
			}
		}
		return num;
	}

	public static int CalculateImprovementLevel(GameState gameState, TileData tile)
	{
		if (tile.improvement == null)
		{
			return 1;
		}
		if (tile.improvement.type == ImprovementData.Type.City)
		{
			return tile.improvement.level;
		}
		if (!gameState.GameLogicData.TryGetData(tile.improvement.type, out var data))
		{
			throw new Exception("Congratulations");
		}
		if (tile.improvement.type == ImprovementData.Type.IceBank)
		{
			return GetIceBankLevel(gameState);
		}
		if (data.maxLevel == 0 && tile.improvement.type != ImprovementData.Type.IceBank)
		{
			return tile.improvement.level;
		}
		if (data.adjacencyImprovements == null && data.adjacencyImprovements.Count == 0)
		{
			return tile.improvement.level;
		}
		if (data.HasAbility(ImprovementAbility.Type.Patina))
		{
			int num = tile.improvement.GetAge(gameState) / data.growthRate;
			if (gameState.Version < 40)
			{
				num++;
			}
			return Math.Min(num, data.maxLevel);
		}
		if (data.adjacencyImprovements.Count > 0)
		{
			int num2 = 0;
			List<TileData> area = gameState.Map.GetArea(tile.coordinates, 1, allowDiagonal: true, includeCenter: false);
			if (area == null || area.Count == 0)
			{
				return num2;
			}
			for (int i = 0; i < area.Count; i++)
			{
				TileData tileData = area[i];
				if (tileData == null || tileData.owner != tile.owner)
				{
					continue;
				}
				foreach (AdjacencyImprovements adjacencyImprovement in data.adjacencyImprovements)
				{
					if (gameState.Version < 27)
					{
						if (adjacencyImprovement.improvement != null && adjacencyImprovement.improvement.type != ImprovementData.Type.None && tileData.HasImprovement(adjacencyImprovement.improvement.type))
						{
							num2++;
						}
						else if (adjacencyImprovement.resources != null && adjacencyImprovement.resources.type != ResourceData.Type.None && tileData.HasResource(adjacencyImprovement.resources.type))
						{
							num2++;
						}
					}
					else if (adjacencyImprovement.improvement != null && adjacencyImprovement.improvement.type != ImprovementData.Type.None && tileData.HasImprovement(adjacencyImprovement.improvement.type))
					{
						num2++;
					}
					else if (adjacencyImprovement.resources != null && adjacencyImprovement.resources.type != ResourceData.Type.None && tileData.HasResource(adjacencyImprovement.resources.type) && tileData.improvement == null)
					{
						num2++;
					}
				}
			}
			return num2;
		}
		return tile.improvement.level;
	}

	public static void CalculateEmbassyLevel(GameState gameState, PlayerState playerState, PlayerState otherPlayer)
	{
		DiplomacyRelation relation = otherPlayer.GetRelation(playerState.Id);
		if (playerState.HasActiveEmbassyWith(otherPlayer, gameState))
		{
			relation.EmbassyLevel = 1;
		}
	}

	public static void LevelUpCity(GameState gameState, PlayerState playerState, TileData cityTile)
	{
		cityTile.improvement.LevelUp();
		gameState.ActionStack.Add(new IncreaseScoreAction(playerState.Id, cityTile.CalculateLevelUpScore(), cityTile.coordinates));
		EnableTask(gameState, playerState, TaskData.Type.Metropolis);
		if (ActionManager.USE_COMMAND_TRIGGER)
		{
			CommandTrigger commandTrigger = new CommandTrigger
			{
				playerId = playerState.Id,
				type = CommandTriggerType.CityLevelUp,
				coordinates = cityTile.coordinates
			};
			gameState.AddPendingCommandTrigger(commandTrigger);
		}
	}

	public static void EnableTask(GameState gameState, PlayerState playerState, TaskData.Type type)
	{
		for (int i = 0; i < playerState.tasks.Count; i++)
		{
			if (playerState.tasks[i].GetTaskType() == type)
			{
				return;
			}
		}
		gameState.ActionStack.Add(new EnableTaskAction(playerState.Id, type));
	}

	public static void UpdateCityConnections(GameState gameState, PlayerState playerState, List<TileData> cityTiles, List<TileData> connectedCities)
	{
		cityTiles.Clear();
		connectedCities.Clear();
		gameState.Map.GetPlayerCityTiles(playerState.Id, cityTiles);
		gameState.FindConnectedCities(playerState.Id, cityTiles, connectedCities);
		PerformConnectedCityChanges(gameState, playerState.Id, cityTiles, connectedCities);
	}

	public static void PerformConnectedCityChanges(GameState gameState, byte playerId, List<TileData> cityTiles, List<TileData> connectedCities)
	{
		foreach (TileData cityTile in cityTiles)
		{
			bool flag = connectedCities.Contains(cityTile);
			if (!flag && cityTile.improvement.connectedToCapitalOfPlayer != 0)
			{
				gameState.ActionStack.Add(new DisconnectCityAction(playerId, cityTile.coordinates));
			}
			else if (flag && cityTile.improvement.connectedToCapitalOfPlayer == 0)
			{
				gameState.ActionStack.Add(new ConnectCityAction(playerId, cityTile.coordinates));
			}
			else if (flag && cityTile.improvement.connectedToCapitalOfPlayer != playerId)
			{
				gameState.ActionStack.Add(new ChangeCityConnectionAction(playerId, cityTile.coordinates));
			}
		}
	}

	public static void CheckTribeConnection(GameState gameState, PlayerState playerState, TileData[] tiles)
	{
		if (gameState.Version <= 29)
		{
			CheckTribeConnectionV29(gameState, playerState, tiles);
		}
		else if (gameState.Version < 80)
		{
			CheckTribeConnectionV30(gameState, playerState, tiles);
		}
		else
		{
			CheckTribeConnectionDefault(gameState, playerState, tiles);
		}
	}

	public static void CheckTribeConnectionDefault(GameState gameState, PlayerState playerState, TileData[] tiles)
	{
		if (playerState.Id == byte.MaxValue || playerState.knownPlayers.Count >= gameState.PlayerCount)
		{
			return;
		}
		foreach (TileData tileData in tiles)
		{
			if (tileData.GetExplored(playerState.Id))
			{
				UnitState unit = tileData.GetUnit(gameState, playerState.Id);
				bool flag = unit != null && unit.owner != playerState.Id && !playerState.KnowsPlayer(unit.owner);
				bool num = tileData.HasImprovement(ImprovementData.Type.City) && tileData.owner != 0 && tileData.owner != playerState.Id && !playerState.KnowsPlayer(tileData.owner);
				if (flag)
				{
					gameState.ActionStack.Add(new MeetAction(playerState.Id, unit.owner, tileData.coordinates));
				}
				if (num && (!flag || unit.owner != tileData.owner))
				{
					gameState.ActionStack.Add(new MeetAction(playerState.Id, tileData.owner, tileData.coordinates));
				}
			}
		}
	}

	public static void CheckTribeConnectionV29(GameState gameState, PlayerState playerState, TileData[] tiles)
	{
		if (playerState.Id == byte.MaxValue || playerState.knownPlayers.Count >= gameState.PlayerCount)
		{
			return;
		}
		foreach (TileData tileData in tiles)
		{
			if (tileData.GetExplored(playerState.Id) && tileData.unit != null && tileData.unit.owner != playerState.Id && !playerState.KnowsPlayer(tileData.unit.owner))
			{
				gameState.ActionStack.Add(new MeetAction(playerState.Id, tileData.unit.owner, tileData.coordinates));
			}
		}
	}

	public static void CheckTribeConnectionV30(GameState gameState, PlayerState playerState, TileData[] tiles)
	{
		if (playerState.Id == byte.MaxValue || playerState.knownPlayers.Count >= gameState.PlayerCount)
		{
			return;
		}
		foreach (TileData tileData in tiles)
		{
			if (tileData.GetExplored(playerState.Id))
			{
				UnitState unit = tileData.GetUnit(gameState, playerState.Id);
				bool flag = unit != null && unit.owner != playerState.Id && !playerState.KnowsPlayer(unit.owner);
				bool num = tileData.HasImprovement(ImprovementData.Type.City) && tileData.owner != 0 && tileData.owner != playerState.Id && !playerState.KnowsPlayer(tileData.owner);
				if (flag)
				{
					gameState.ActionStack.Add(new MeetAction(playerState.Id, unit.owner, tileData.coordinates));
				}
				if (num && (!flag || unit.owner != tileData.owner))
				{
					gameState.ActionStack.Add(new MeetAction(playerState.Id, tileData.owner, tileData.coordinates));
				}
			}
		}
	}

	public static int GetIceBankLevel(GameState gameState)
	{
		int num = gameState.CountIceTiles();
		gameState.GameLogicData.TryGetData(ImprovementData.Type.IceBank, out var data);
		return (int)Math.Min(Math.Floor((float)num / 20f), data.maxLevel);
	}

	public static void CheckIceBankLevels(GameState gameState)
	{
		int num = 0;
		List<TileData> list = new List<TileData>();
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (tileData.IsFrozen(gameState))
			{
				num++;
			}
			if (tileData.HasImprovement(ImprovementData.Type.IceBank))
			{
				list.Add(tileData);
			}
		}
		if (list.Count <= 0 || !gameState.GameLogicData.TryGetData(ImprovementData.Type.IceBank, out var data))
		{
			return;
		}
		int num2 = (int)Math.Min(Math.Floor((float)num / 20f), data.maxLevel);
		foreach (TileData item in list)
		{
			if (item.improvement.level < num2)
			{
				int num3 = num2 - item.improvement.level;
				for (int j = 0; j < num3; j++)
				{
					gameState.ActionStack.Add(new ImprovementLevelUpAction(item.owner, item.coordinates));
				}
			}
			else if (item.improvement.level > num2)
			{
				int num4 = item.improvement.level - num2;
				for (int k = 0; k < num4; k++)
				{
					gameState.ActionStack.Add(new ImprovementLevelDownAction(item.owner, item.coordinates, 1));
				}
			}
		}
	}

	public static void TryRevealCapital(GameState gameState, PlayerState playerState, PlayerState otherPlayerState)
	{
		if (!playerState.HasAbility(PlayerAbility.Type.CapitalVision, gameState))
		{
			return;
		}
		TileData tile = gameState.Map.GetTile(otherPlayerState.startTile);
		if ((gameState.Version > 80 || tile.owner == otherPlayerState.Id) && !tile.GetExplored(playerState.Id))
		{
			if (gameState.Version <= 80)
			{
				gameState.ActionStack.Add(new RevealCapitalAction(playerState.Id, tile.coordinates));
				gameState.ActionStack.Add(new ExploreAction(playerState.Id, tile.coordinates));
			}
			else
			{
				gameState.ActionStack.Add(new ExploreAction(playerState.Id, tile.coordinates));
				gameState.ActionStack.Add(new RevealCapitalAction(playerState.Id, tile.coordinates));
			}
		}
	}

	public static void TryRevealAllCapitals(GameState gameState, PlayerState playerState)
	{
		if (!playerState.HasAbility(PlayerAbility.Type.CapitalVision, gameState))
		{
			return;
		}
		foreach (PlayerState playerState2 in gameState.PlayerStates)
		{
			if (playerState2.Id != byte.MaxValue && playerState.KnowsPlayer(playerState2.Id))
			{
				TryRevealCapital(gameState, playerState, playerState2);
			}
		}
	}

	public static void CheckStepOnPoison(TileData targetTile, UnitState unit, GameState gameState)
	{
		if (targetTile.improvement != null && gameState.GameLogicData.TryGetData(targetTile.improvement.type, out var data) && data != null && data.HasAbility(ImprovementAbility.Type.Poison) && gameState.TryGetPlayer(unit.owner, out var playerState) && !playerState.HasTribeAbility(TribeAbility.Type.PoisonResist, gameState) && !unit.HasAbility(UnitAbility.Type.Fly, gameState))
		{
			gameState.ActionStack.Add(new PoisonUnitAction(unit.owner, targetTile.coordinates, targetTile.coordinates));
			if (gameState.Version < 83)
			{
				gameState.ActionStack.Add(new AttackAction(unit.owner, targetTile.coordinates, targetTile.coordinates, 20, shouldMoveToTarget: false, AttackAction.AnimationType.None, 0));
			}
		}
	}
}
