using System.Collections.Generic;
using Polytopia.Data;

public static class UnitDataExtensions
{
	public static int GetAttack(this UnitData unitData)
	{
		return unitData.attack * 10;
	}

	public static int GetAttack(this UnitState unitState, GameState gameState)
	{
		gameState.GameLogicData.TryGetData(unitState.type, out var data);
		return data.GetAttack() + (unitState.HasEffect(UnitEffect.Boosted) ? 50 : 0);
	}

	public static int GetDefence(this UnitState unit, GameState state)
	{
		if (state.GameLogicData.TryGetData(unit.type, out var data))
		{
			return data.defence * unit.GetDefenceBonus(state);
		}
		return 0;
	}

	public static void SetUnitDirection(this UnitState unit, WorldCoordinates origin, WorldCoordinates target)
	{
		GridDirection? direction = WorldCoordinates.GetDirection(origin, target);
		if (direction.HasValue && unit != null)
		{
			unit.direction = direction.Value;
			switch (direction)
			{
			case GridDirection.SW:
			case GridDirection.W:
			case GridDirection.NW:
			case GridDirection.N:
				unit.flipped = true;
				break;
			default:
				unit.flipped = false;
				break;
			}
		}
	}

	public static int GetRange(this UnitData unitData)
	{
		return unitData.range;
	}

	public static int GetRange(this UnitState unitState, GameState gameState)
	{
		gameState.GameLogicData.TryGetData(unitState.type, out var data);
		return data.GetRange();
	}

	public static int GetMovement(this UnitData unitData)
	{
		return unitData.movement;
	}

	public static int GetMovement(this UnitState unitState, GameState gameState)
	{
		gameState.GameLogicData.TryGetData(unitState.type, out var data);
		return data.GetMovement() + (unitState.HasEffect(UnitEffect.Boosted) ? 1 : 0);
	}

	public static int GetSightRange(this UnitData unitData)
	{
		if (unitData.HasAbility(UnitAbility.Type.Scout))
		{
			return 2;
		}
		return 1;
	}

	public static uint GetAge(this UnitState unit, GameState state)
	{
		return state.CurrentTurn - unit.createdTurn;
	}

	public static int GetMaxHealth(this UnitState unitState, GameState state)
	{
		if (unitState.passengerUnit != null && state.GameLogicData.TryGetData(unitState.passengerUnit.type, out var data))
		{
			return (ushort)data.health + unitState.passengerUnit.promotionLevel * 50;
		}
		if (state.GameLogicData.TryGetData(unitState.type, out var data2))
		{
			return (ushort)data2.health + unitState.promotionLevel * 50;
		}
		return 0;
	}

	public static int GetDefenceBonus(this UnitState unit, GameState gameState)
	{
		int result = 10;
		if (unit.HasEffect(UnitEffect.Poisoned))
		{
			if (gameState.Version < 50)
			{
				return 8;
			}
			return 7;
		}
		TileData tile = gameState.Map.GetTile(unit.coordinates);
		if (tile == null)
		{
			return result;
		}
		if (unit.owner != 0 && gameState.TryGetPlayer(unit.owner, out var playerState) && playerState.GetDefenceBonus(tile.terrain, gameState) > 1)
		{
			result = 15;
		}
		if (tile.improvement != null && gameState.GameLogicData.TryGetData(tile.improvement.type, out var data))
		{
			if (tile.HasImprovement(ImprovementData.Type.City) && tile.owner == unit.owner && unit.HasAbility(UnitAbility.Type.Fortify, gameState))
			{
				result = 15;
				if (tile.improvement.HasReward(CityReward.CityWall))
				{
					result = 40;
				}
			}
			if (data.HasAbility(ImprovementAbility.Type.Defend))
			{
				result = 40;
			}
		}
		return result;
	}

	public static bool HasAbility(this UnitState unit, UnitAbility.Type ability, GameState state)
	{
		return unit.HasAbility(ability, state, WorldCoordinates.NULL_COORDINATES);
	}

	public static bool HasAbility(this UnitState unit, UnitAbility.Type ability, GameState state, WorldCoordinates targetCoordinates)
	{
		bool result = false;
		bool flag = false;
		if (state.GameLogicData.TryGetData(unit.type, out var data) && data.unitAbilities != null && data.unitAbilities.Count > 0)
		{
			foreach (UnitAbility.Type unitAbility in data.unitAbilities)
			{
				if (unitAbility == UnitAbility.Type.Skate)
				{
					flag = true;
				}
				if (unitAbility == ability)
				{
					result = true;
				}
			}
		}
		if (state.Version < 14)
		{
			return result;
		}
		if (flag)
		{
			if (ability == UnitAbility.Type.Scout || (uint)(ability - 18) <= 1u || ability == UnitAbility.Type.Skate)
			{
				return result;
			}
			if (targetCoordinates == WorldCoordinates.NULL_COORDINATES)
			{
				targetCoordinates = unit.coordinates;
			}
			if (state.Map.GetTile(targetCoordinates).terrain != TerrainData.Type.Ice)
			{
				result = false;
			}
		}
		return result;
	}

	public static bool HasAbility(this UnitData unit, UnitAbility.Type ability)
	{
		if (unit.unitAbilities != null && unit.unitAbilities.Count > 0)
		{
			foreach (UnitAbility.Type unitAbility in unit.unitAbilities)
			{
				if (unitAbility == ability)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsAquatic(this UnitData unitData)
	{
		if (unitData.HasAbility(UnitAbility.Type.Swim))
		{
			return true;
		}
		if (unitData.movementTerrain != null && unitData.movementTerrain.Count > 0)
		{
			foreach (TerrainData item in unitData.movementTerrain)
			{
				if (item.type == TerrainData.Type.Water)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsVehicle(this UnitData unitData)
	{
		return unitData.HasAbility(UnitAbility.Type.Carry);
	}

	public static bool HasDynamicStyle(this UnitData unitData)
	{
		return HasDynamicStyle(unitData.type);
	}

	public static bool HasDynamicStyle(this UnitState unitState)
	{
		return HasDynamicStyle(unitState.type);
	}

	private static bool HasDynamicStyle(UnitData.Type type)
	{
		if (type == UnitData.Type.Polytaur)
		{
			return true;
		}
		return false;
	}

	public static List<WorldCoordinates> GetMovementOptions(this UnitState unit, GameState gameState, int range)
	{
		return gameState.GetMoveOptions(unit.coordinates, range, unit);
	}

	public static bool CanMoveTo(this UnitState unitState, GameState gameState, WorldCoordinates coordinates)
	{
		if (!unitState.CanMove())
		{
			return false;
		}
		return unitState.GetMovementOptions(gameState, unitState.GetMovement(gameState)).Contains(coordinates);
	}

	public static List<WorldCoordinates> GetAttackOptions(this UnitState unit, GameState state, int range, bool ignoreDiplomacyRelation = false)
	{
		if (unit.HasLeader())
		{
			return new List<WorldCoordinates>();
		}
		if (state.GameLogicData.TryGetData(unit.type, out var data) && data.GetAttack() == 0 && !unit.HasAbility(UnitAbility.Type.Convert, state) && !unit.HasAbility(UnitAbility.Type.Infiltrate, state))
		{
			return new List<WorldCoordinates>();
		}
		return GetAttackOptionsAtPosition(state, unit.owner, unit.coordinates, range, includeHiddenTiles: false, null, ignoreDiplomacyRelation);
	}

	public static List<WorldCoordinates> GetAttackOptionsAtPosition(GameState gameState, byte playerId, WorldCoordinates position, int range, bool includeHiddenTiles = false, UnitState customUnitState = null, bool ignoreDiplomacyRelation = false)
	{
		List<WorldCoordinates> list = new List<WorldCoordinates>();
		UnitState unitState = customUnitState ?? gameState.Map.GetTile(position).unit;
		List<TileData> area = gameState.Map.GetArea(position, range, allowDiagonal: true, includeCenter: false);
		if (area != null && area.Count > 0)
		{
			list = new List<WorldCoordinates>();
			for (int i = 0; i < area.Count; i++)
			{
				TileData tileData = area[i];
				if (tileData == null)
				{
					continue;
				}
				gameState.TryGetPlayer(playerId, out var playerState);
				if (unitState != null && unitState.HasAbility(UnitAbility.Type.Infiltrate, gameState))
				{
					if (gameState.Version < 83)
					{
						if (tileData.HasImprovement(ImprovementData.Type.City) && tileData.owner != 0 && tileData.owner != unitState.owner && !playerState.HasPeaceWith(tileData.owner) && !tileData.IsBeingCaptured(gameState) && !HasDiplomacyRelation(tileData.owner))
						{
							list.Add(tileData.coordinates);
						}
					}
					else if (tileData.HasImprovement(ImprovementData.Type.City) && tileData.owner != 0 && tileData.owner != unitState.owner && !playerState.HasPeaceWith(tileData.owner) && !tileData.IsBeingCaptured(gameState) && !tileData.improvement.HasEffect(ImprovementEffect.robbed) && !HasDiplomacyRelation(tileData.owner))
					{
						list.Add(tileData.coordinates);
					}
				}
				else
				{
					UnitState unit = tileData.GetUnit(gameState, playerId, includeHiddenTiles);
					if (unit != null && (tileData.GetExplored(playerId) || includeHiddenTiles) && unit.owner != playerId && !HasDiplomacyRelation(unit.owner))
					{
						list.Add(tileData.coordinates);
					}
				}
				bool HasDiplomacyRelation(byte ownerID)
				{
					if (!ignoreDiplomacyRelation)
					{
						if (!playerState.HasPeaceWith(ownerID))
						{
							return playerState.HasBrokenPeaceWith(ownerID);
						}
						return true;
					}
					return false;
				}
			}
		}
		return list;
	}

	public static List<TileData> GetBoostOptions(this UnitState unitState, GameState state)
	{
		List<TileData> area = state.Map.GetArea(unitState.coordinates, 1, allowDiagonal: true, includeCenter: false);
		for (int i = 0; i < area.Count; i++)
		{
			TileData tileData = area[i];
			if (tileData.unit == null || tileData.unit.owner != unitState.owner || tileData.unit.HasEffect(UnitEffect.Boosted) || tileData.unit.HasLeader())
			{
				area.RemoveAt(i--);
			}
		}
		return area;
	}

	public static IEnumerable<TerrainData> GetAllowedTerrain(this UnitState unit, GameState state)
	{
		List<TerrainData> list = new List<TerrainData>();
		if (unit != null && state.GameLogicData.TryGetData(unit.type, out var data))
		{
			if (state.Version >= 9 && data.HasAbility(UnitAbility.Type.Fly))
			{
				foreach (KeyValuePair<TerrainData.Type, TerrainData> allTerrainDatum in state.GameLogicData.AllTerrainData)
				{
					if (allTerrainDatum.Key != TerrainData.Type.None)
					{
						list.Add(allTerrainDatum.Value);
					}
				}
				return list;
			}
			state.TryGetPlayer(unit.owner, out var playerState);
			List<TerrainData> unlockedMovements = state.GameLogicData.GetUnlockedMovements(playerState);
			if (data.movementTerrain != null && data.movementTerrain.Count > 0)
			{
				foreach (TerrainData item in data.movementTerrain)
				{
					if (data.HasAbility(UnitAbility.Type.Navigate) || (unlockedMovements != null && unlockedMovements.Contains(item)))
					{
						list.Add(item);
					}
				}
			}
			else if (unlockedMovements != null)
			{
				list.AddRange(unlockedMovements);
			}
			if (data.HasAbility(UnitAbility.Type.Swim) && state.GameLogicData.TryGetData(TerrainData.Type.Water, out var data2) && !list.Contains(data2))
			{
				list.Add(data2);
			}
		}
		return list;
	}

	public static bool CanPerformAnyAction(this UnitState unitState, GameState gameState)
	{
		if (unitState.moved && unitState.attacked && !unitState.CanBePromoted(gameState))
		{
			return false;
		}
		if (unitState.CanPerformAnyAbility(gameState))
		{
			return true;
		}
		if (!gameState.GameLogicData.TryGetData(unitState.type, out var data))
		{
			return false;
		}
		List<WorldCoordinates> attackOptions = unitState.GetAttackOptions(gameState, data.GetRange());
		if (unitState.moved && attackOptions.Count == 0)
		{
			return false;
		}
		return true;
	}

	private static bool CanPerformAnyAbility(this UnitState unitState, GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(unitState.coordinates);
		if (unitState.CanRecover(gameState))
		{
			return true;
		}
		if (unitState.CanHealOthers(gameState) && tile.GetHealOptions(unitState.owner, gameState).Count > 0)
		{
			return true;
		}
		if (unitState.CanBePromoted(gameState))
		{
			return true;
		}
		if (unitState.CanFreezeArea(gameState))
		{
			return true;
		}
		if (unitState.CanBreakIce(gameState))
		{
			return true;
		}
		if (gameState.TryGetPlayer(unitState.owner, out var playerState) && gameState.GameLogicData.IsUnlocked(PlayerAbility.Type.Disband, playerState) && !unitState.moved && !unitState.attacked)
		{
			return true;
		}
		if (!unitState.HasAbility(UnitAbility.Type.Grow, gameState) && gameState.GameLogicData.TryGetData(unitState.type, out var data))
		{
			foreach (UnitData item in gameState.GameLogicData.GetUnlockedUpgradesForUnit(playerState, gameState, data))
			{
				if (playerState.CanAfford(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CanCapture(this UnitState unitState, GameState gameState, TileData tile, bool includeNextTurn = false, bool allowOnlyOnTile = true)
	{
		if (unitState.owner == byte.MaxValue)
		{
			return false;
		}
		if (!includeNextTurn && (!unitState.CanMove() || !unitState.CanAttack()))
		{
			return false;
		}
		if (allowOnlyOnTile && tile.coordinates != unitState.coordinates)
		{
			return false;
		}
		if (gameState.Version < 27)
		{
			if (unitState.HasAbility(UnitAbility.Type.Fly, gameState))
			{
				return false;
			}
			if (unitState.type == UnitData.Type.Mooni && tile.owner != 0)
			{
				return false;
			}
		}
		if (unitState.HasLeader())
		{
			return false;
		}
		if (gameState.TryGetPlayer(unitState.owner, out var playerState) && playerState.HasPeaceWith(tile.owner))
		{
			return false;
		}
		if (tile.improvement != null)
		{
			if (tile.owner != unitState.owner)
			{
				return tile.improvement.type == ImprovementData.Type.City;
			}
			return false;
		}
		return false;
	}

	public static bool CanOccupy(this UnitState unitState, GameState gameState, TileData tile)
	{
		if (tile == null)
		{
			return false;
		}
		if (tile.improvement == null)
		{
			return false;
		}
		if (tile.improvement.type != ImprovementData.Type.City)
		{
			return false;
		}
		if (tile.coordinates != unitState.coordinates)
		{
			return false;
		}
		if (tile.owner == unitState.owner)
		{
			return false;
		}
		return true;
	}

	public static bool CanExamineRuins(this UnitState unitState, GameState state, TileData tile)
	{
		if (!unitState.CanMove() || !unitState.CanAttack())
		{
			return false;
		}
		if (tile.coordinates != unitState.coordinates)
		{
			return false;
		}
		if (unitState.HasLeader())
		{
			return false;
		}
		if (state.Version >= 27 && unitState.owner == byte.MaxValue)
		{
			return false;
		}
		return tile.HasImprovement(ImprovementData.Type.Ruin);
	}

	public static bool CanRecover(this UnitState unitState, GameState state)
	{
		if (unitState.HasLeader())
		{
			return false;
		}
		if (state.Version <= 42)
		{
			if (unitState.health < unitState.GetMaxHealth(state) && unitState.CanMove())
			{
				return unitState.CanAttack();
			}
			return false;
		}
		if ((unitState.health < unitState.GetMaxHealth(state) || unitState.HasEffect(UnitEffect.Poisoned)) && unitState.CanMove())
		{
			return unitState.CanAttack();
		}
		return false;
	}

	public static bool IsDamaged(this UnitState unitState, GameState state)
	{
		return unitState.health < unitState.GetMaxHealth(state);
	}

	public static bool CanHealOthers(this UnitState unitState, GameState state)
	{
		if (unitState.HasAbility(UnitAbility.Type.Heal, state) && unitState.CanMove())
		{
			return unitState.CanAttack();
		}
		return false;
	}

	public static bool CanFreezeArea(this UnitState unitState, GameState gameState, int size = 1)
	{
		return unitState.CanFreezeArea(gameState, unitState.coordinates, size);
	}

	public static bool CanFreezeArea(this UnitState unitState, GameState gameState, WorldCoordinates coordinates, int size = 1)
	{
		if (!unitState.moved && !unitState.attacked && unitState.HasAbility(UnitAbility.Type.FreezeArea, gameState) && gameState.TryGetPlayer(unitState.owner, out var playerState))
		{
			List<TileData> area = gameState.Map.GetArea(coordinates, size, allowDiagonal: true, includeCenter: false);
			for (int i = 0; i < area.Count; i++)
			{
				TileData tileData = area[i];
				if (tileData != null && tileData.IsFreezable(gameState, playerState))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CanBreakIce(this UnitState unitState, GameState gameState)
	{
		if (unitState.HasLeader())
		{
			return false;
		}
		if (!unitState.moved && !unitState.attacked && !unitState.HasAbility(UnitAbility.Type.FreezeArea, gameState))
		{
			List<TileData> area = gameState.Map.GetArea(unitState.coordinates, 1, allowDiagonal: true, includeCenter: false);
			for (int i = 0; i < area.Count; i++)
			{
				if (area[i].CanBreakIce())
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CanBePromoted(this UnitState unitState, GameState gameState)
	{
		if (unitState.passengerUnit != null)
		{
			unitState = unitState.passengerUnit;
		}
		gameState.GameLogicData.TryGetData(unitState.type, out var data);
		if (data.hidden)
		{
			return false;
		}
		if (data.promotionLimit <= 0)
		{
			return false;
		}
		if (unitState.xp >= data.promotionLimit)
		{
			return unitState.promotionLevel < 1;
		}
		return false;
	}

	public static bool CanDisband(this UnitState unitState, GameState gameState, PlayerState playerState)
	{
		if (unitState.attacked)
		{
			return false;
		}
		if (unitState.moved)
		{
			return false;
		}
		if (!gameState.GameLogicData.IsUnlocked(PlayerAbility.Type.Disband, playerState))
		{
			return false;
		}
		if (unitState.HasLeader())
		{
			return false;
		}
		if (unitState.HasAbility(UnitAbility.Type.Disloyal, gameState))
		{
			return false;
		}
		return true;
	}
}
