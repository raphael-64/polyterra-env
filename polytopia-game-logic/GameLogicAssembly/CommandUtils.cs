using System;
using System.Collections.Generic;
using Polytopia.Data;

public static class CommandUtils
{
	public static List<TrainCommand> GetTrainableUnits(GameState gameState, PlayerState player, TileData tile, bool includeUnavailable = false)
	{
		if (tile.owner != 0 && tile.owner != player.Id && !player.HasPeaceWith(tile.owner))
		{
			return GetTrainableAgents(gameState, player, tile, includeUnavailable);
		}
		List<TrainCommand> list = new List<TrainCommand>();
		if (player.Id != gameState.CurrentPlayer)
		{
			return list;
		}
		if (tile.owner != player.Id)
		{
			return list;
		}
		if (tile.unit != null)
		{
			return list;
		}
		if (tile.improvement == null || tile.improvement.type != ImprovementData.Type.City)
		{
			return list;
		}
		foreach (UnitData unlockedUnit in gameState.GameLogicData.GetUnlockedUnits(player, gameState, shouldIncludeHidden: false))
		{
			if (!unlockedUnit.HasAbility(UnitAbility.Type.Agent) && CommandValidation.HasUnitTerrain(gameState, tile.coordinates, unlockedUnit))
			{
				TrainCommand trainCommand = new TrainCommand(player.Id, unlockedUnit.type, tile.coordinates);
				if (!player.blockTrainUnits && (includeUnavailable || trainCommand.IsValid(gameState)))
				{
					list.Add(trainCommand);
				}
			}
		}
		return list;
	}

	public static List<TrainCommand> GetTrainableAgents(GameState gameState, PlayerState player, TileData tile, bool includeUnavailable = false)
	{
		List<TrainCommand> list = new List<TrainCommand>();
		if (player.Id != gameState.CurrentPlayer)
		{
			return list;
		}
		if (tile.unit != null)
		{
			return list;
		}
		if (tile.HasImprovement(ImprovementData.Type.City))
		{
			return list;
		}
		if (tile.owner == player.Id || tile.owner == 0)
		{
			return list;
		}
		foreach (UnitData unlockedUnit in gameState.GameLogicData.GetUnlockedUnits(player, gameState, shouldIncludeHidden: false))
		{
			if (unlockedUnit.HasAbility(UnitAbility.Type.Agent) && tile.CanBeAccessedByPlayer(gameState, player) && CommandValidation.HasUnitTerrain(gameState, tile.coordinates, unlockedUnit))
			{
				TrainCommand trainCommand = new TrainCommand(player.Id, unlockedUnit.type, tile.coordinates);
				if (includeUnavailable || trainCommand.IsValid(gameState, out var _))
				{
					list.Add(trainCommand);
				}
			}
		}
		return list;
	}

	public static List<CommandBase> GetBuildableImprovements(GameState gameState, PlayerState player, TileData tile, bool includeUnavailable = false)
	{
		List<CommandBase> list = new List<CommandBase>();
		if (player.Id != gameState.CurrentPlayer)
		{
			return list;
		}
		foreach (ImprovementData unlockedImprovement in gameState.GameLogicData.GetUnlockedImprovements(player))
		{
			if (gameState.GameLogicData.CanBuild(gameState, tile, player, unlockedImprovement))
			{
				CommandBase commandBase = new BuildCommand(player.Id, unlockedImprovement.type, tile.coordinates);
				if (includeUnavailable || commandBase.IsValid(gameState))
				{
					list.Add(commandBase);
				}
			}
		}
		return list;
	}

	public static List<CommandBase> GetImprovementAbilities(GameState gameState, PlayerState player, TileData tile, bool includeUnavailable = false)
	{
		List<CommandBase> list = new List<CommandBase>();
		if (player.Id != gameState.CurrentPlayer)
		{
			return list;
		}
		if (tile.improvement != null && tile.improvement.type != ImprovementData.Type.City)
		{
			if (tile.CanDestroy(gameState, player) && player.HasAbility(PlayerAbility.Type.Destroy, gameState))
			{
				list.Add(new DestroyCommand(player.Id, tile.coordinates));
			}
			if (gameState.GameLogicData.TryGetData(tile.improvement.type, out var data) && data.HasAbility(ImprovementAbility.Type.Harvest))
			{
				list.Add(new HarvestCommand(player.Id, tile.coordinates));
			}
			if (tile.CanDestroy(gameState, player) && !tile.improvement.HasEffect(ImprovementEffect.decomposing) && player.HasAbility(PlayerAbility.Type.Decompose, gameState))
			{
				list.Add(new DecomposeCommand(player.Id, tile.coordinates));
			}
		}
		return list;
	}

	public static List<CommandBase> GetUnitActions(GameState gameState, PlayerState player, TileData tile, bool includeUnavailable = false)
	{
		List<CommandBase> list = new List<CommandBase>();
		UnitState unit = tile.unit;
		if (unit == null)
		{
			return list;
		}
		if (unit.owner != player.Id)
		{
			return list;
		}
		gameState.GameLogicData.TryGetData(unit.type, out var data);
		if (unit.CanCapture(gameState, tile))
		{
			AddCommand(gameState, list, new CaptureCommand(player.Id, unit.id, unit.coordinates), includeUnavailable);
		}
		if (unit.CanExamineRuins(gameState, tile))
		{
			AddCommand(gameState, list, new ExamineRuinsCommand(player.Id, unit.coordinates), includeUnavailable);
		}
		if (unit.CanRecover(gameState))
		{
			AddCommand(gameState, list, new RecoverCommand(player.Id, unit.coordinates), includeUnavailable);
		}
		if (unit.CanHealOthers(gameState) && tile.GetHealOptions(unit.owner, gameState).Count > 0)
		{
			AddCommand(gameState, list, new HealOthersCommand(player.Id, unit.coordinates), includeUnavailable);
		}
		if (unit.CanBePromoted(gameState))
		{
			AddCommand(gameState, list, new PromoteCommand(player.Id, unit.coordinates), includeUnavailable);
		}
		if (unit.CanFreezeArea(gameState))
		{
			AddCommand(gameState, list, new FreezeAreaCommand(player.Id, unit.coordinates), includeUnavailable);
		}
		if (unit.CanBreakIce(gameState))
		{
			List<TileData> area = gameState.Map.GetArea(unit.coordinates, 1, allowDiagonal: true, includeCenter: false);
			int num = 0;
			for (int i = 0; i < area.Count; i++)
			{
				if (area[i].CanBreakIce())
				{
					num++;
				}
			}
			AddCommand(gameState, list, new BreakIceCommand(player.Id, unit.coordinates), includeUnavailable);
		}
		if (!unit.HasAbility(UnitAbility.Type.Grow, gameState))
		{
			foreach (UnitData item in gameState.GameLogicData.GetUnlockedUpgradesForUnit(player, gameState, data))
			{
				AddCommand(gameState, list, new UpgradeCommand(player.Id, item.type, unit.coordinates), includeUnavailable);
			}
		}
		if (unit != null && !unit.attacked && !tile.unit.moved && unit.HasAbility(UnitAbility.Type.Clone, gameState))
		{
			Log.Verbose("Add clone command", Array.Empty<object>());
			list.Add(new CloneCommand(player.Id, tile.unit.type, tile.unit.coordinates));
		}
		if (!unit.attacked && player.Id == gameState.CurrentPlayer)
		{
			if (unit.CanDisband(gameState, player))
			{
				AddCommand(gameState, list, new DisbandCommand(player.Id, unit.coordinates), includeUnavailable);
			}
			if (unit.HasAbility(UnitAbility.Type.Boost, gameState))
			{
				AddCommand(gameState, list, new BoostCommand(player.Id, unit.coordinates), includeUnavailable);
			}
			if (unit.HasAbility(UnitAbility.Type.Explode, gameState))
			{
				AddCommand(gameState, list, new ExplodeCommand(player.Id, unit.coordinates), includeUnavailable);
			}
		}
		return list;
	}

	public static List<CommandBase> GetDiplomacyCommandsForOpponents(GameState gameState, PlayerState player, List<byte> opponentIds, bool includeUnavailable = false)
	{
		List<byte> list = new List<byte>(opponentIds);
		List<CommandBase> list2 = new List<CommandBase>(opponentIds.Count * 2);
		for (int i = 0; i < list.Count; i++)
		{
			byte b = list[i];
			if (!player.KnowsPlayer(b))
			{
				continue;
			}
			gameState.TryGetPlayer(b, out var playerState);
			if (playerState.IsAlive(gameState))
			{
				List<TileData> cityTiles = playerState.GetCityTiles(gameState);
				if (cityTiles.Count > 0)
				{
					GetDiplomacyCommandsForOpponentsInternal(list, list2, gameState, cityTiles[0], player, includeUnavailable, b);
				}
			}
		}
		return list2;
	}

	private static int GetDiplomacyCommandsForOpponentsInternal(List<byte> players, List<CommandBase> possibleCommands, GameState gameState, TileData tile, PlayerState player, bool includeUnavailable, byte opponentId)
	{
		List<CommandBase> list = new List<CommandBase>(3);
		byte item = AddDiplomacyCommands(list, gameState, tile, player, includeUnavailable, opponentId);
		int num = players.IndexOf(item);
		if (num != -1)
		{
			players.RemoveAt(num);
			possibleCommands.AddRange(list);
		}
		return num;
	}

	private static byte AddDiplomacyCommands(List<CommandBase> possibleCommands, GameState gameState, TileData tile, PlayerState player, bool includeUnavailable, byte opponentId)
	{
		if (gameState.Version < 60)
		{
			return opponentId;
		}
		gameState.TryGetPlayer(opponentId, out var playerState);
		AddPeaceCommands(possibleCommands, gameState, tile, player, playerState, includeUnavailable);
		if ((includeUnavailable || (playerState.OwnsTheirCapital(gameState) && gameState.GameLogicData.IsUnlocked(PlayerAbility.Type.Embassy, player))) && (player.GetEmbassyLevel(playerState) == 0 || includeUnavailable))
		{
			AddCommand(gameState, possibleCommands, new EstablishEmbassyCommand(player.Id, opponentId, tile.coordinates), includeUnavailable);
		}
		return opponentId;
	}

	private static void AddPeaceCommands(List<CommandBase> possibleCommands, GameState gameState, TileData tile, PlayerState player, PlayerState opponent, bool includeUnavailable)
	{
		bool flag = player.HasPeaceWith(opponent.Id);
		if (gameState.GameLogicData.IsUnlocked(PlayerAbility.Type.PeaceTreaty, player) && !flag)
		{
			AddCommand(gameState, possibleCommands, new PeaceTreatyCommand(player.Id, opponent.Id, tile.coordinates), includeUnavailable);
		}
		else if (flag)
		{
			AddCommand(gameState, possibleCommands, new BreakPeaceCommand(player.Id, opponent.Id, tile.coordinates), includeUnavailable);
		}
		else if (includeUnavailable)
		{
			AddCommand(gameState, possibleCommands, new PeaceTreatyCommand(player.Id, opponent.Id, tile.coordinates), includeUnavailable);
		}
	}

	private static void AddCommand(GameState gameState, List<CommandBase> possibleCommands, CommandBase command, bool includeUnavailable)
	{
		if (includeUnavailable || command.IsValid(gameState))
		{
			possibleCommands.Add(command);
		}
	}
}
