using System;
using System.Collections.Generic;
using Polytopia.Data;
using PolytopiaBackendBase.Game;

public class AI
{
	public enum MissionType
	{
		None,
		Capture
	}

	public struct Mission
	{
		public MissionType type;

		public TileData tile;

		public float score;
	}

	public struct UnitTarget
	{
		public UnitState unit;

		public float score;
	}

	public struct ScoredCommand
	{
		public CommandBase command;

		public float score;
	}

	public struct UnitStats
	{
		public float defence;

		public float attack;

		public float movement;

		public float range;

		public Dictionary<UnitAbility.Type, float> abilities;

		public override string ToString()
		{
			return string.Format("Unit stats defence {0} attack {1} movement {2} range {3}", defence.ToString("F2"), attack.ToString("F2"), movement.ToString("F2"), range.ToString("F2"));
		}
	}

	public struct PlayerMapData
	{
		public List<UnitState> units;

		public List<TileData> empireTiles;

		public List<TileData> cityTiles;

		public PlayerMapData(GameState gameState, PlayerState player)
		{
			units = new List<UnitState>();
			gameState.Map.GetPlayerUnits(player.Id, units);
			empireTiles = new List<TileData>();
			gameState.Map.GetPlayerEmpireTiles(player.Id, empireTiles);
			cityTiles = new List<TileData>();
			gameState.Map.GetPlayerCityTiles(player.Id, cityTiles);
		}
	}

	private const float LATE_GAME_PROGRESS_LIMIT = 0.8f;

	private const float BREAK_PEACE_LIMIT = 0f;

	private const int HIGH_AGGRESSION = 5000;

	private static bool enableLog = true;

	public static bool isEnabled = true;

	private static Random random;

	public static Random GetRandom()
	{
		if (random == null)
		{
			random = new Random();
		}
		return random;
	}

	public static CommandBase GetMove(GameState gameState, PlayerState player, CommandType specificCommand = CommandType.None)
	{
		if (!isEnabled || gameState.CurrentState == GameState.State.FinalTurn || player.handicap == GameSettings.HandicapFromDifficulty(GameSettings.Difficulties.Frozen))
		{
			return EndCommand(gameState, player);
		}
		CreateMissionsIfNeeded(gameState, player);
		PlayerMapData mapData = new PlayerMapData(gameState, player);
		if (enableLog)
		{
			Log.Verbose("Getting move for player {0} with unit count {1}, cities {2}, empire {3}", new object[4]
			{
				player.Id,
				mapData.units.Count,
				mapData.cityTiles.Count,
				mapData.empireTiles.Count
			});
		}
		for (int i = 0; i < mapData.units.Count; i++)
		{
			UnitState unitState = mapData.units[i];
			gameState.GameLogicData.TryGetData(unitState.type, out var data);
			if ((data.range > 1 || data.HasAbility(UnitAbility.Type.Surprise)) && unitState.CanPerformAnyAction(gameState))
			{
				CommandBase unitMove = GetUnitMove(gameState, mapData, unitState, player);
				if (unitMove != null)
				{
					return unitMove;
				}
			}
		}
		for (int j = 0; j < mapData.units.Count; j++)
		{
			UnitState unitState2 = mapData.units[j];
			if (unitState2.CanPerformAnyAction(gameState))
			{
				CommandBase unitMove = GetUnitMove(gameState, mapData, unitState2, player);
				if (unitMove != null)
				{
					return unitMove;
				}
			}
		}
		CommandBase tileCommands = GetTileCommands(gameState, mapData, player, specificCommand);
		if (tileCommands != null)
		{
			return tileCommands;
		}
		return EndCommand(gameState, player);
	}

	public static CommandBase EndCommand(GameState gameState, PlayerState player)
	{
		if (gameState.CurrentState == GameState.State.FinalTurn)
		{
			return new EndMatchCommand(player.Id);
		}
		return new EndTurnCommand(player.Id);
	}

	public static void CollectMilitaryStats(GameState gameState, Dictionary<byte, MilitaryStats> playerMilitaryStats, TileData tileData)
	{
		if (tileData.unit != null)
		{
			gameState.GameLogicData.TryGetData(tileData.unit.type, out var data);
			if (!playerMilitaryStats.TryGetValue(tileData.unit.owner, out var value))
			{
				value = new MilitaryStats();
				playerMilitaryStats[tileData.unit.owner] = value;
			}
			value.Defense += (float)tileData.unit.GetDefence(gameState) * 0.01f;
			value.Attack += (float)tileData.unit.GetAttack(gameState) * 0.01f;
			value.Movement += tileData.unit.GetMovement(gameState);
			value.Range += (data.GetRange() - 1) * 2;
		}
	}

	public static void MakeMissions(GameState gameState, PlayerState player)
	{
		player.aiState.Clear();
		if (player.Id == byte.MaxValue)
		{
			return;
		}
		Log.Verbose("[flx]Make missions for {0}", new object[1] { player.tribe });
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (!tileData.GetExplored(player.Id))
			{
				continue;
			}
			if (tileData.terrain == TerrainData.Type.Forest)
			{
				num++;
			}
			if (tileData.terrain == TerrainData.Type.Mountain)
			{
				num++;
			}
			num2++;
			if (tileData.CanBeAccessedByPlayer(gameState, player))
			{
				int num3 = 0;
				List<TileData> tileNeighbors = gameState.Map.GetTileNeighbors(tileData.coordinates);
				for (int j = 0; j < tileNeighbors.Count; j++)
				{
					if (!tileNeighbors[j].GetExplored(player.Id))
					{
						num3++;
					}
				}
				if (num3 > 2 && player.aiState.missions.Count < 5)
				{
					player.aiState.missions.Add(new Mission
					{
						tile = tileData,
						score = num3 * 5
					});
				}
			}
			if (tileData.HasImprovement(ImprovementData.Type.Ruin))
			{
				player.aiState.missions.Add(new Mission
				{
					tile = tileData,
					score = 30f
				});
			}
			else if (tileData.HasImprovement(ImprovementData.Type.City))
			{
				if (tileData.owner == 0)
				{
					if (tileData.unit == null)
					{
						player.aiState.missions.Add(new Mission
						{
							type = MissionType.Capture,
							tile = tileData,
							score = 50f
						});
					}
				}
				else if (tileData.owner != player.Id && !player.HasPeaceWith(tileData.owner))
				{
					if (player.opinions.GetOpinion(tileData.owner) < 0f)
					{
						player.aiState.enemyCities++;
						player.aiState.enemyPopulation += Math.Min(tileData.improvement.population, (short)5);
						Log.Verbose("[flx] add {0} population? opinion {1}", new object[2]
						{
							tileData.improvement.population,
							player.opinions.GetOpinion(tileData.owner)
						});
					}
					if (tileData.unit != null && tileData.unit.owner != player.Id)
					{
						player.aiState.missions.Add(new Mission
						{
							type = MissionType.Capture,
							tile = tileData,
							score = 0.005f * (float)player.GetAggression(tileData.owner, gameState)
						});
					}
					else
					{
						player.aiState.missions.Add(new Mission
						{
							type = MissionType.Capture,
							tile = tileData,
							score = 0.06f * (float)player.GetAggression(tileData.owner, gameState)
						});
					}
				}
			}
			if (tileData.unit == null)
			{
				continue;
			}
			gameState.GameLogicData.TryGetData(tileData.unit.type, out var data);
			CollectMilitaryStats(gameState, player.aiState.playerMilitaryStats, tileData);
			gameState.TryGetPlayer(tileData.unit.owner, out var _);
			if (tileData.unit.owner == player.Id)
			{
				continue;
			}
			float num4 = 0f;
			if (tileData.HasImprovement(ImprovementData.Type.City))
			{
				if (tileData.owner != player.Id)
				{
					num4 = ((tileData.owner != 0) ? (num4 + 15f) : (num4 + 20f));
				}
				else
				{
					num4 += 100f;
					player.aiState.missions.Add(new Mission
					{
						tile = tileData,
						score = 200f
					});
				}
			}
			else if (tileData.HasImprovement(ImprovementData.Type.Ruin))
			{
				num4 += 20f;
			}
			float num5 = 0f;
			int num6 = data.GetRange();
			if (tileData.unit.HasAbility(UnitAbility.Type.Dash, gameState))
			{
				num6 += tileData.unit.GetMovement(gameState);
			}
			List<WorldCoordinates> attackOptions = tileData.unit.GetAttackOptions(gameState, num6);
			for (int k = 0; k < attackOptions.Count; k++)
			{
				TileData tile = gameState.Map.GetTile(attackOptions[k]);
				if (tile.unit != null && tile.unit.owner == player.Id)
				{
					float num7 = RateBattle(gameState, tileData.unit, tile);
					if (num7 > num5)
					{
						num5 = num7;
					}
				}
			}
			num4 += num5 / 5f;
			List<WorldCoordinates> movementOptions = tileData.unit.GetMovementOptions(gameState, tileData.unit.GetMovement(gameState));
			for (int l = 0; l < movementOptions.Count; l++)
			{
				TileData tile2 = gameState.Map.GetTile(movementOptions[l]);
				if (tile2.HasImprovement(ImprovementData.Type.City))
				{
					if (tile2.owner == player.Id)
					{
						num4 += 30f;
					}
					else if (tile2.owner == 0)
					{
						num4 += 10f;
					}
				}
			}
			if (num4 > 0f)
			{
				player.aiState.hitList.Add(new UnitTarget
				{
					unit = tileData.unit,
					score = num4
				});
			}
		}
		player.aiState.roughTerrain = (float)num / (float)num2;
		Log.Verbose("[felix] Adjust military needs for {0}", new object[1] { player.tribe });
		MilitaryStats militaryStats = null;
		foreach (KeyValuePair<byte, MilitaryStats> playerMilitaryStat in player.aiState.playerMilitaryStats)
		{
			if (playerMilitaryStat.Key == player.Id)
			{
				militaryStats = playerMilitaryStat.Value;
				continue;
			}
			gameState.TryGetPlayer(playerMilitaryStat.Key, out var playerState2);
			Log.Verbose("[felix] Opponent {0} military is :{1}", new object[2] { playerState2.tribe, playerMilitaryStat.Value });
			player.aiState.militaryNeeds.Movement += Math.Max(0f, playerMilitaryStat.Value.Range);
			player.aiState.militaryNeeds.Range += Math.Max(0f, playerMilitaryStat.Value.Defense);
			player.aiState.militaryNeeds.Attack += Math.Max(0f, playerMilitaryStat.Value.Defense);
			player.aiState.militaryNeeds.Defense += Math.Max(0f, playerMilitaryStat.Value.Movement) * 0.5f;
			player.aiState.militaryNeeds.Defense += Math.Max(0f, playerMilitaryStat.Value.Attack) * 0.5f;
		}
		if (militaryStats != null)
		{
			player.aiState.militaryNeeds.Attack -= militaryStats.Attack;
			player.aiState.militaryNeeds.Defense -= militaryStats.Defense;
			player.aiState.militaryNeeds.Movement -= militaryStats.Movement;
			player.aiState.militaryNeeds.Range -= militaryStats.Range;
		}
		player.aiState.militaryNeeds.Attack = Math.Max(0f, player.aiState.militaryNeeds.Attack);
		player.aiState.militaryNeeds.Defense = Math.Max(0f, player.aiState.militaryNeeds.Defense);
		player.aiState.militaryNeeds.Movement = Math.Max(0f, player.aiState.militaryNeeds.Movement);
		player.aiState.militaryNeeds.Range = Math.Max(0f, player.aiState.militaryNeeds.Range);
		float total = player.aiState.militaryNeeds.GetTotal();
		if (total > 0f)
		{
			player.aiState.militaryNeeds.Attack = player.aiState.militaryNeeds.Attack / total;
			player.aiState.militaryNeeds.Defense = player.aiState.militaryNeeds.Defense / total;
			player.aiState.militaryNeeds.Movement = player.aiState.militaryNeeds.Movement / total;
			player.aiState.militaryNeeds.Range = player.aiState.militaryNeeds.Range / total;
		}
		for (int m = 0; m < player.aiState.missions.Count; m++)
		{
			Mission value = player.aiState.missions[m];
			value.score += StupidFactor(player);
			player.aiState.missions[m] = value;
			if (enableLog)
			{
				Log.Verbose("Mission {0} {1}", new object[2]
				{
					value.tile.coordinates.ToString(),
					value.score
				});
			}
		}
		for (int n = 0; n < player.aiState.hitList.Count; n++)
		{
			UnitTarget value2 = player.aiState.hitList[n];
			value2.score += StupidFactor(player);
			player.aiState.hitList[n] = value2;
			if (enableLog)
			{
				Log.Verbose("UnitTarget {0} {1}", new object[2]
				{
					value2.unit.coordinates.ToString(),
					value2.score
				});
			}
		}
		player.aiState.missions.Sort((Mission a, Mission b) => b.score.CompareTo(a.score));
		player.aiState.hitList.Sort((UnitTarget a, UnitTarget b) => b.score.CompareTo(a.score));
	}

	public static CommandBase GetUnitMove(GameState gameState, PlayerMapData mapData, UnitState unit, PlayerState player)
	{
		if (unit == null)
		{
			return null;
		}
		if (!gameState.GameLogicData.TryGetData(unit.type, out var data))
		{
			return null;
		}
		TileData tile = gameState.Map.GetTile(unit.coordinates);
		float currentSituationScore = RatePosition(gameState, mapData, player, unit, tile);
		Log.Verbose("Get the best move for {0} at {1}", new object[2] { data.type, tile.terrain });
		List<ScoredCommand> list = new List<ScoredCommand>();
		if (unit.CanAttack())
		{
			Log.Verbose("   check attack options:", Array.Empty<object>());
			AddAttackOptions(gameState, mapData, player, unit, data, currentSituationScore, list);
		}
		if (unit.CanMove())
		{
			Log.Verbose("   check move options:", Array.Empty<object>());
			AddUnitMoveOptions(gameState, mapData, player, unit, data, tile, currentSituationScore, list);
		}
		AddUnitActionOptions(gameState, mapData, player, unit, data, tile, currentSituationScore, list);
		return PickBestPossibleCommand(gameState, list, player);
	}

	private static void AddUnitActionOptions(GameState gameState, PlayerMapData mapData, PlayerState player, UnitState unit, UnitData unitData, TileData tile, float currentSituationScore, List<ScoredCommand> commands)
	{
		foreach (CommandBase unitAction in CommandUtils.GetUnitActions(gameState, player, tile))
		{
			if (unitAction.GetCommandType() == CommandType.Capture)
			{
				commands.Add(new ScoredCommand
				{
					command = unitAction,
					score = 200 + player.GetAggression(tile.owner, gameState) / 2
				});
			}
			if (unitAction.GetCommandType() == CommandType.ExamineRuins)
			{
				commands.Add(new ScoredCommand
				{
					command = unitAction,
					score = 500f
				});
			}
			if (unitAction.GetCommandType() == CommandType.Recover)
			{
				commands.Add(new ScoredCommand
				{
					command = unitAction,
					score = currentSituationScore + (float)(int)ActionUtils.GetHealAmount(gameState, tile) * 0.1f
				});
			}
			if (unitAction.GetCommandType() == CommandType.HealOthers)
			{
				float num = 0f;
				foreach (TileData healOption in tile.GetHealOptions(unit.owner, gameState))
				{
					num += (float)(int)ActionUtils.GetHealAmount(gameState, healOption) * 0.1f;
				}
				if (num > 0f)
				{
					commands.Add(new ScoredCommand
					{
						command = unitAction,
						score = num * 50f
					});
				}
			}
			if (unitAction.GetCommandType() == CommandType.Promote)
			{
				commands.Add(new ScoredCommand
				{
					command = unitAction,
					score = 5000f
				});
			}
			if (unitAction.GetCommandType() == CommandType.FreezeArea)
			{
				List<TileData> area = gameState.Map.GetArea(unit.coordinates, 1, allowDiagonal: true);
				int num2 = 0;
				foreach (TileData item in area)
				{
					if (item.IsFreezable(gameState, player))
					{
						num2++;
					}
				}
				commands.Add(new ScoredCommand
				{
					command = unitAction,
					score = num2 * 50
				});
			}
			if (unitAction.GetCommandType() == CommandType.BreakIce && player.tribe != TribeData.Type.Polaris)
			{
				List<TileData> area2 = gameState.Map.GetArea(unit.coordinates, 1, allowDiagonal: true, includeCenter: false);
				int num3 = 0;
				for (int i = 0; i < area2.Count; i++)
				{
					if (area2[i].CanBreakIce())
					{
						num3++;
					}
				}
				commands.Add(new ScoredCommand
				{
					command = unitAction,
					score = num3 * 50
				});
			}
			if (unitAction.GetCommandType() == CommandType.Upgrade)
			{
				UnitStats desiredUnitStats = GetDesiredUnitStats(gameState, mapData, player, tile, 1f);
				foreach (UnitData item2 in gameState.GameLogicData.GetUnlockedUpgradesForUnit(player, gameState, unitData))
				{
					float buildUnitScore = GetBuildUnitScore(gameState, mapData, player, tile, item2, desiredUnitStats);
					commands.Add(new ScoredCommand
					{
						command = unitAction,
						score = buildUnitScore
					});
				}
			}
			if (unitAction.GetCommandType() == CommandType.Explode)
			{
				List<TileData> area3 = gameState.Map.GetArea(unit.coordinates, 1, allowDiagonal: true, includeCenter: false);
				float num4 = 0f;
				foreach (TileData item3 in area3)
				{
					if (item3.unit != null && item3.unit.owner != unit.owner)
					{
						float num5 = BattleHelpers.GetBattleResults(gameState, unit, item3.unit).attackDamage;
						gameState.GameLogicData.TryGetData(item3.unit.type, out var data);
						float num6 = num5 / (float)item3.unit.GetMaxHealth(gameState) * (float)data.cost;
						num4 += num6;
						Log.Verbose("\texplo +{0}", new object[1] { num6 });
					}
				}
				if (num4 > 0f)
				{
					float num7 = (float)(int)unit.health / (float)unit.GetMaxHealth(gameState) * (float)unitData.cost;
					num4 -= num7;
					Log.Verbose("\treduce explo -{0}", new object[1] { num7 });
					Log.Verbose("Total explo = {0}", new object[1] { num4 });
					if (num4 > 0f)
					{
						commands.Add(new ScoredCommand
						{
							command = unitAction,
							score = num4 * 100f
						});
					}
				}
			}
			if (unitAction.GetCommandType() == CommandType.Boost)
			{
				int count = unit.GetBoostOptions(gameState).Count;
				if (count > 0)
				{
					commands.Add(new ScoredCommand
					{
						command = unitAction,
						score = count * 50
					});
				}
			}
		}
	}

	private static void AddUnitMoveOptions(GameState gameState, PlayerMapData mapData, PlayerState player, UnitState unit, UnitData unitData, TileData tile, float currentSituationScore, List<ScoredCommand> commands)
	{
		commands.Add(new ScoredCommand
		{
			command = new StayCommand(player.Id, unit.coordinates),
			score = currentSituationScore
		});
		foreach (WorldCoordinates movementOption in unit.GetMovementOptions(gameState, unit.GetMovement(gameState)))
		{
			TileData tile2 = gameState.Map.GetTile(movementOption);
			bool canAttack = unit.HasAbility(UnitAbility.Type.Dash, gameState) && unit.CanAttack();
			float num = RatePosition(gameState, mapData, player, unit, tile2, canAttack);
			if (unitData.HasAbility(UnitAbility.Type.Carry) && tile.IsWater && !tile2.IsWater)
			{
				num -= (float)unitData.cost * 10f;
			}
			num += 5f;
			commands.Add(new ScoredCommand
			{
				score = num,
				command = new MoveCommand(player.Id, unit, movementOption)
			});
		}
	}

	private static void AddAttackOptions(GameState gameState, PlayerMapData mapData, PlayerState player, UnitState unit, UnitData unitData, float currentSituationScore, List<ScoredCommand> commands)
	{
		gameState.TryGetPlayer(unit.owner, out var playerState);
		List<WorldCoordinates> attackOptions = unit.GetAttackOptions(gameState, unitData.GetRange());
		if (attackOptions.Count > 0)
		{
			Log.Verbose("-Unit can attack {0} targets for {1} {2}", new object[3] { attackOptions.Count, playerState.tribe, unit.type });
		}
		foreach (WorldCoordinates item in attackOptions)
		{
			TileData tile = gameState.Map.GetTile(item);
			float num = RateBattle(gameState, unit, tile);
			Log.Verbose("\t\t-Attack basic battle score {0}", new object[1] { num });
			if (tile.unit == null)
			{
				if (!unit.HasAbility(UnitAbility.Type.Infiltrate, gameState) || !tile.HasOpponentCity(player.Id))
				{
					Log.Verbose("\t-AI no defending unit found", Array.Empty<object>());
					continue;
				}
				num += num * (float)player.GetAggression(tile.owner, gameState) * 0.001f;
			}
			else
			{
				for (int i = 0; i < player.aiState.hitList.Count; i++)
				{
					UnitTarget unitTarget = player.aiState.hitList[i];
					if (tile.unit == unitTarget.unit)
					{
						num += unitTarget.score;
						Log.Verbose("\t\t-AI hit list +{0}", new object[1] { unitTarget.score });
					}
				}
				if (BattleHelpers.GetBattleResults(gameState, unit, tile.unit).shouldMoveToDefeatedEnemyTile)
				{
					float num2 = RatePosition(gameState, mapData, player, unit, tile) - currentSituationScore;
					num2 *= 0.3f;
					Log.Verbose("\t\t-AI movement diff if preceed {0}", new object[1] { num2 });
					num += num2;
				}
				if (num > 0f)
				{
					Log.Verbose("\t\t-AI aggression modifier +({0}*{1})", new object[2]
					{
						(float)player.GetAggression(tile.unit.owner, gameState) * 0.001f,
						num
					});
					num += num * (float)player.GetAggression(tile.unit.owner, gameState) * 0.001f;
				}
				else
				{
					Log.Verbose("\t\t-Not a good idea, score: {0}", new object[1] { num });
				}
			}
			_ = 0f;
			Log.Verbose("-----", Array.Empty<object>());
			if (num > 0f)
			{
				commands.Add(new ScoredCommand
				{
					score = num,
					command = new AttackCommand(player.Id, unit, tile.coordinates)
				});
			}
		}
	}

	private static void CreateMissionsIfNeeded(GameState gameState, PlayerState player)
	{
		bool flag = player.aiState.missionsForTurn != gameState.CurrentTurn;
		player.aiState.missionsForTurn = gameState.CurrentTurn;
		if (player.aiState.missions == null || flag)
		{
			MakeMissions(gameState, player);
		}
	}

	public static CommandBase GetTileCommands(GameState gameState, PlayerMapData mapData, PlayerState player, CommandType specificCommand)
	{
		Dictionary<TechData.Type, int> neededTech = new Dictionary<TechData.Type, int>();
		CreateMissionsIfNeeded(gameState, player);
		List<ScoredCommand> list = new List<ScoredCommand>();
		if (gameState.GameLogicData.IsUnlocked(ImprovementData.Type.Road, player))
		{
			AddPossibleRoadBuildingCommands(gameState, player, mapData.cityTiles, list);
		}
		else
		{
			AddTechNeed(neededTech, TechData.Type.Roads, 2);
		}
		CheckForTechNeeds(gameState, player, mapData.empireTiles, neededTech);
		AddPossibleResearchCommands(gameState, player, neededTech, list);
		float num = (float)(player.aiState.hitList.Count + player.aiState.missions.Count - mapData.units.Count) / 3f;
		if (num < 0f)
		{
			num *= 2f;
		}
		if (enableLog)
		{
			Log.Verbose("Unitneed {0}", new object[1] { num });
		}
		int numberOfFrozenTilesOwnedByPlayer = gameState.GetNumberOfFrozenTilesOwnedByPlayer(player.Id);
		AddPossibleTrainUnitCommands(gameState, mapData, player, list, num);
		AddPossibleImprovementCommands(gameState, mapData, player, list, numberOfFrozenTilesOwnedByPlayer, num);
		AddPossibleDiplomacyCommands(gameState, player, list);
		if (specificCommand != CommandType.None)
		{
			list = list.FindAll((ScoredCommand x) => x.command.GetCommandType() == specificCommand);
		}
		return PickBestPossibleCommand(gameState, list, player);
	}

	private static CommandBase PickBestPossibleCommand(GameState gameState, List<ScoredCommand> possibleCommands, PlayerState player)
	{
		if (possibleCommands.Count > 0)
		{
			for (int i = 0; i < possibleCommands.Count; i++)
			{
				ScoredCommand value = possibleCommands[i];
				float num = StupidFactor(player);
				value.score += num;
				possibleCommands[i] = value;
				if (enableLog)
				{
					Log.Verbose("PossibleCommand {0} score {1} (≈{2})", new object[3]
					{
						value.command.ToString(),
						value.score - num,
						num
					});
				}
			}
			possibleCommands.Sort((ScoredCommand a, ScoredCommand b) => b.score.CompareTo(a.score));
			CommandBase command = possibleCommands[0].command;
			if (command != null && command.IsValid(gameState))
			{
				if (enableLog)
				{
					Log.Verbose("Picking best command {0}", new object[1] { command.ToString() });
				}
				return command;
			}
		}
		return null;
	}

	private static void AddPossibleTrainUnitCommands(GameState gameState, PlayerMapData mapData, PlayerState player, List<ScoredCommand> possibleCommands, float unitNeed)
	{
		Log.Verbose("[flx] {0} evaluate units to train", new object[1] { player });
		TileData[] tiles = gameState.Map.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (!tileData.GetExplored(player.Id))
			{
				continue;
			}
			List<TrainCommand> trainableUnits = CommandUtils.GetTrainableUnits(gameState, player, tileData);
			if (trainableUnits.Count == 0)
			{
				continue;
			}
			bool num = tileData.owner == player.Id && IsCityThreatened(gameState, tileData);
			float num2 = 0f;
			UnitStats desiredUnitStats = GetDesiredUnitStats(gameState, mapData, player, tileData, unitNeed);
			if (num)
			{
				num2 += 300f;
				desiredUnitStats.defence += 3f;
			}
			Log.Verbose("[flx]Desired stats {0}", new object[1] { desiredUnitStats });
			foreach (TrainCommand item in trainableUnits)
			{
				gameState.GameLogicData.TryGetData(item.Type, out var data);
				float num3 = num2 + GetBuildUnitScore(gameState, mapData, player, tileData, data, desiredUnitStats);
				if (num3 > 0f)
				{
					if (enableLog)
					{
						Log.Verbose("[flx] Adding unit of type {0} with cost {1} at {2} with score {3}", new object[4] { data.type, data.cost, tileData.coordinates, num3 });
					}
					possibleCommands.Add(new ScoredCommand
					{
						command = item,
						score = num3
					});
				}
			}
		}
	}

	private static float GetWinAdvantage(GameState gameState, PlayerState player, byte opponentId)
	{
		gameState.TryGetPlayer(opponentId, out var playerState);
		switch (gameState.Settings.RulesGameMode)
		{
		case GameMode.Domination:
			return (float)playerState.cities / Math.Max(0.1f, player.cities);
		case GameMode.Might:
			return (float)playerState.CountCapitals(gameState) / Math.Max(0.1f, player.CountCapitals(gameState));
		case GameMode.Perfection:
		case GameMode.Glory:
		case GameMode.Tutorial:
			return (float)playerState.score / (float)player.score;
		case GameMode.Sandbox:
			return 1f;
		default:
			throw new Exception("Not implemented");
		}
	}

	public static float GetGameProgress(GameState gameState, PlayerState winningPlayer)
	{
		switch (gameState.Settings.RulesGameMode)
		{
		case GameMode.Domination:
		{
			float val = (float)winningPlayer.cities / Math.Max(0.1f, MapDataExtensions.CountCities(gameState));
			float val2 = (float)gameState.CurrentTurn / (float)gameState.Settings.rules.TurnLimit;
			return Math.Max(val, val2);
		}
		case GameMode.Might:
			return (float)winningPlayer.cities / Math.Max(0.1f, MapDataExtensions.CountCities(gameState));
		case GameMode.Glory:
			return (float)winningPlayer.score / (float)gameState.Settings.rules.ScoreLimit;
		case GameMode.Perfection:
			return (float)gameState.CurrentTurn / (float)gameState.Settings.rules.TurnLimit;
		case GameMode.Sandbox:
			return 0f;
		case GameMode.Tutorial:
			return (float)winningPlayer.score / (float)gameState.Settings.rules.ScoreLimit;
		default:
			throw new Exception("Not implemented");
		}
	}

	private static void AddPossibleDiplomacyCommands(GameState gameState, PlayerState player, List<ScoredCommand> possibleCommands)
	{
		List<CommandBase> diplomacyCommandsForOpponents = CommandUtils.GetDiplomacyCommandsForOpponents(gameState, player, player.knownPlayers);
		if (diplomacyCommandsForOpponents.Count == 0)
		{
			return;
		}
		PlayerState winningPlayer = gameState.GetPlayersSortedByRank()[0];
		GetGameProgress(gameState, winningPlayer);
		foreach (CommandBase item in diplomacyCommandsForOpponents)
		{
			if (item is EstablishEmbassyCommand establishEmbassyCommand)
			{
				float embassyScore = GetEmbassyScore(gameState, player, establishEmbassyCommand.OpponentId);
				if (!(embassyScore <= 0f))
				{
					possibleCommands.Add(new ScoredCommand
					{
						command = item,
						score = embassyScore
					});
				}
			}
			else if (item is UpgradeEmbassyCommand upgradeEmbassyCommand)
			{
				float embassyScore2 = GetEmbassyScore(gameState, player, upgradeEmbassyCommand.OpponentId);
				if (!(embassyScore2 <= 0f))
				{
					possibleCommands.Add(new ScoredCommand
					{
						command = item,
						score = embassyScore2
					});
				}
			}
			else if (item is PeaceTreatyCommand peaceTreatyCommand)
			{
				if ((double)player.opinions.GetOpinion(peaceTreatyCommand.OpponentId) * GetRandom().NextDouble() > (double)OpinionManager.LoveLimit)
				{
					possibleCommands.Add(new ScoredCommand
					{
						command = item,
						score = 1000f
					});
				}
			}
			else if (item is BreakPeaceCommand breakPeaceCommand && player.opinions.GetOpinion(breakPeaceCommand.OpponentId) < 0f)
			{
				int num = gameState.Map.CountPlayerUnitsWithinOpponentBorders(player.Id, breakPeaceCommand.OpponentId);
				float num2 = (0f - player.opinions.GetOpinion(breakPeaceCommand.OpponentId)) * 500f - (float)(num * 200);
				if (num2 > 0f)
				{
					possibleCommands.Add(new ScoredCommand
					{
						command = item,
						score = num2
					});
				}
			}
		}
	}

	private static float GetEmbassyScore(GameState gameState, PlayerState player, byte opponentId)
	{
		int currency = player.Currency;
		int embassyIncome = gameState.GameLogicData.DiplomacyData.embassyIncome;
		float num = 1f + (float)gameState.GameLogicData.DiplomacyData.embassyCost / (Math.Max(currency, 0.1f) / 2f);
		float num2 = (float)(embassyIncome * 10) / num;
		float num3 = player.GetAggression(opponentId, gameState);
		float num4 = (player.HasPeaceWith(opponentId) ? (embassyIncome * 5) : 0);
		gameState.TryGetPlayer(opponentId, out var playerState);
		if (num3 > 5000f || player.HasWarWith(playerState, gameState))
		{
			return 0f;
		}
		return num2 - num3 * 0.001f + num4;
	}

	private static void AddPossibleImprovementCommands(GameState gameState, PlayerMapData mapData, PlayerState player, List<ScoredCommand> possibleCommands, int frozenTileCount, float unitNeed)
	{
		foreach (TileData empireTile in mapData.empireTiles)
		{
			if (empireTile.improvement != null)
			{
				continue;
			}
			foreach (BuildCommand buildableImprovement in CommandUtils.GetBuildableImprovements(gameState, player, empireTile))
			{
				gameState.GameLogicData.TryGetData(buildableImprovement.Type, out var data);
				float num = 0f;
				int population = data.rewards.GetPopulation();
				num += (float)(population * 10);
				num += (float)(data.rewards.GetCurrency() - 2);
				num += (float)(data.work * 20);
				UnitData unit = data.creates.GetUnit();
				if (unit != null)
				{
					UnitStats desiredUnitStats = GetDesiredUnitStats(gameState, mapData, player, empireTile, unitNeed);
					num += GetBuildUnitScore(gameState, mapData, player, empireTile, unit, desiredUnitStats) / 2f;
				}
				if (data.adjacencyImprovements != null && data.adjacencyImprovements.Count > 0)
				{
					int num2 = gameState.Map.GetMultiplierImprovementsForImprovementOnTile(data, empireTile);
					if (data.adjacencyImprovements.Contains(ImprovementData.Type.PolarisClimate))
					{
						num2 += frozenTileCount / 20;
						foreach (TileData tileNeighbor in gameState.Map.GetTileNeighbors(empireTile.coordinates))
						{
							if (tileNeighbor.unit != null && tileNeighbor.unit.owner != player.Id)
							{
								num -= 30f;
							}
						}
					}
					num *= (float)(num2 * 2);
				}
				if (data.HasAbility(ImprovementAbility.Type.Patina))
				{
					population = data.growthRewards.GetPopulation();
					num += (float)(population * 100);
				}
				_ = empireTile.rulingCityCoordinates;
				TileData tile = gameState.Map.GetTile(empireTile.rulingCityCoordinates);
				ImprovementState improvement = tile.improvement;
				if (improvement != null && improvement.type == ImprovementData.Type.City && improvement.xp + population > improvement.level)
				{
					num += 100f;
				}
				if (data.IsRouteOpener())
				{
					if (!tile.IsConnected)
					{
						num += 30f;
						if (tile.coordinates == player.startTile)
						{
							num += 50f;
						}
					}
					foreach (TileData item in gameState.Map.GetArea(tile.coordinates, improvement.borderSize, allowDiagonal: true, includeCenter: false))
					{
						if (item.improvement != null && gameState.GameLogicData.TryGetData(item.improvement.type, out var data2) && data2.IsRouteOpener())
						{
							num -= 10f;
						}
					}
				}
				if (gameState.Settings.RulesGameMode == GameMode.Perfection)
				{
					int score = data.rewards.GetScore();
					num += (float)score / 6f;
				}
				if (data.type != ImprovementData.Type.Road && empireTile.resource != null && !gameState.GameLogicData.IsResourceRequiredByImprovement(empireTile.resource.type, data))
				{
					num *= 0.5f;
				}
				if (data.HasAbility(ImprovementAbility.Type.Consumed))
				{
					num *= 2f;
				}
				if (data.HasAbility(ImprovementAbility.Type.Bridge) && !gameState.GameLogicData.GetUnlockedMovements(player).Contains(empireTile.terrain))
				{
					foreach (TileData item2 in gameState.Map.GetArea(empireTile.coordinates, 1, allowDiagonal: true, includeCenter: false))
					{
						ImprovementData data3;
						bool flag = item2.improvement != null && gameState.GameLogicData.TryGetData(item2.improvement.type, out data3) && data3.HasAbility(ImprovementAbility.Type.Bridge);
						if (gameState.GameLogicData.GetUnlockedMovements(player).Contains(item2.terrain) || flag)
						{
							num += 10f;
							if (item2.owner != player.Id)
							{
								num += 10f;
							}
						}
					}
				}
				if (data.HasAbility(ImprovementAbility.Type.Network))
				{
					List<TileData> area = gameState.Map.GetArea(empireTile.coordinates, data.range, allowDiagonal: true, includeCenter: false);
					float num3 = 0f;
					float num4 = 0.1f;
					foreach (TileData item3 in area)
					{
						if (item3.improvement != null && item3.improvement.type == ImprovementData.Type.City && item3.owner == player.Id)
						{
							num3 += 1f;
							if (!item3.IsConnected)
							{
								num3 += 0.5f;
							}
							else
							{
								num4 = 1f;
							}
						}
					}
					num += num3 * num3 * num4 * 5f;
					Log.Verbose("Network score {0} ({1})", new object[2]
					{
						num3 * num3 * num4 * 5f,
						num3
					});
				}
				int cost = data.cost;
				int currency = player.Currency;
				float num5 = 1f + (float)cost / (Math.Max(currency, 0.1f) / 2f);
				if (enableLog)
				{
					Log.Verbose("build score before cheapoh {0} after {1} cheapoh {2}", new object[3]
					{
						num,
						num / num5,
						num5
					});
				}
				num /= num5;
				if (num > 1f)
				{
					CommandBase command = new BuildCommand(player.Id, data.type, empireTile.coordinates);
					possibleCommands.Add(new ScoredCommand
					{
						command = command,
						score = num
					});
				}
			}
		}
	}

	private static void AddPossibleResearchCommands(GameState gameState, PlayerState player, Dictionary<TechData.Type, int> neededTech, List<ScoredCommand> possibleCommands)
	{
		List<TechData> unlockableTech = gameState.GameLogicData.GetUnlockableTech(player);
		for (int i = 0; i < unlockableTech.Count; i++)
		{
			TechData techData = unlockableTech[i];
			if (!player.CanAfford(gameState, techData))
			{
				continue;
			}
			float num = 0f;
			int value = 0;
			if (neededTech.TryGetValue(techData.type, out value))
			{
				num += (float)(value * 3);
			}
			if (techData.unitUnlocks != null)
			{
				for (int j = 0; j < techData.unitUnlocks.Count; j++)
				{
					UnitData unitData = techData.unitUnlocks[j];
					num += (float)(unitData.attack / 10 + unitData.defence / 10 + unitData.movement + unitData.range);
				}
			}
			if (techData.abilityUnlocks != null)
			{
				num += (float)(5 * techData.abilityUnlocks.Count);
			}
			float num2 = gameState.GameLogicData.GetTechPrice(techData, player, gameState);
			int currency = player.Currency;
			float num3 = 1f + num2 / (Math.Max(currency, 0.1f) * 0.5f);
			num += 30f / num3;
			CommandBase command = new ResearchCommand(player.Id, techData.type);
			possibleCommands.Add(new ScoredCommand
			{
				command = command,
				score = num
			});
		}
	}

	private static void CheckForTechNeeds(GameState gameState, PlayerState player, List<TileData> playerEmpire, Dictionary<TechData.Type, int> neededTech)
	{
		for (int i = 0; i < playerEmpire.Count; i++)
		{
			TileData tileData = playerEmpire[i];
			ResourceState resource = tileData.GetResource(gameState, player.Id);
			if (resource != null)
			{
				ImprovementData improvementForResource = gameState.GameLogicData.GetImprovementForResource(resource.type);
				if (improvementForResource != null)
				{
					TechData techThatUnlocks = gameState.GameLogicData.GetTechThatUnlocks(improvementForResource, player.GetTribeData(gameState));
					if (!player.HasTech(techThatUnlocks.type))
					{
						if (improvementForResource.rewards.GetCurrency() > 0)
						{
							int num = improvementForResource.rewards.CalculateRewardScore();
							AddTechNeed(neededTech, techThatUnlocks.type, num + 1);
						}
						else
						{
							AddTechNeed(neededTech, techThatUnlocks.type);
						}
					}
				}
			}
			if (!tileData.CanBeAccessedByPlayer(gameState, player))
			{
				TechData techThatUnlocks2 = gameState.GameLogicData.GetTechThatUnlocks(tileData.terrain);
				AddTechNeed(neededTech, techThatUnlocks2.type);
			}
		}
	}

	private static void AddTechNeed(Dictionary<TechData.Type, int> neededTech, TechData.Type techType, int addedNeedScore = 1)
	{
		if (neededTech.TryGetValue(techType, out var value))
		{
			neededTech[techType] = value + addedNeedScore;
		}
		else
		{
			neededTech[techType] = addedNeedScore;
		}
	}

	private static void AddPossibleRoadBuildingCommands(GameState gameState, PlayerState player, List<TileData> cityTiles, List<ScoredCommand> possibleCommands)
	{
		TileData tile = gameState.Map.GetTile(player.startTile);
		for (int i = 0; i < cityTiles.Count; i++)
		{
			TileData tileData = cityTiles[i];
			if (tileData.IsConnected)
			{
				continue;
			}
			gameState.GameLogicData.TryGetData(TerrainData.Type.Forest, out var data);
			gameState.GameLogicData.TryGetData(TerrainData.Type.Field, out var data2);
			PathFinderSettings settings = PathFinderSettings.CreateDefault(player, new TerrainData[2] { data, data2 }, gameState.Version, gameState);
			List<WorldCoordinates> route = GetRoute(gameState, tile, tileData, settings);
			if (route == null)
			{
				continue;
			}
			for (int j = 0; j < route.Count; j++)
			{
				if (gameState.Map.GetTile(route[j]).HasRoad)
				{
					route.RemoveAt(j--);
				}
			}
			float score = (float)gameState.GetCityPotential(tileData, player) / (float)((route.Count == 0) ? (-1) : route.Count);
			for (int k = 0; k < route.Count; k++)
			{
				TileData tile2 = gameState.Map.GetTile(route[k]);
				List<ImprovementData> unlockedImprovements = gameState.GameLogicData.GetUnlockedImprovements(player);
				int num = 0;
				while (unlockedImprovements != null && num < unlockedImprovements.Count)
				{
					if (unlockedImprovements[num].type == ImprovementData.Type.Road)
					{
						ScoredCommand item = new ScoredCommand
						{
							command = new BuildCommand(player.Id, ImprovementData.Type.Road, tile2.coordinates),
							score = score
						};
						possibleCommands.Add(item);
						break;
					}
					num++;
				}
			}
		}
	}

	public static CityReward ChooseCityReward(GameState gameState, TileData tile, CityReward[] rewards)
	{
		uint hash = gameState.RandomHash.GetHash(tile.coordinates.X, tile.coordinates.Y, gameState.LastProcessedCommand);
		Log.Verbose("{0} AI pick city reward {1}", new object[2] { gameState.LastProcessedCommand, hash });
		if (Array.IndexOf(rewards, CityReward.BorderGrowth) != -1)
		{
			return CityReward.BorderGrowth;
		}
		int num = Array.IndexOf(rewards, CityReward.SuperUnit);
		if (num != -1 && hash > 1431655765)
		{
			return CityReward.SuperUnit;
		}
		if (num == 0)
		{
			return rewards[1];
		}
		return rewards[(hash > int.MaxValue) ? 1u : 0u];
	}

	public static CityReward ChooseInfiltrationReward(GameState gameState, TileData tile, CityReward[] rewards)
	{
		uint hash = gameState.RandomHash.GetHash(tile.coordinates.X, tile.coordinates.Y, gameState.LastProcessedCommand);
		Log.Verbose("{0} AI pick infiltration reward {1}", new object[2] { gameState.LastProcessedCommand, hash });
		return rewards[(hash > int.MaxValue) ? 1u : 0u];
	}

	public static bool ShouldAcceptPeace(GameState gameState, byte playerId, byte opponentId)
	{
		gameState.TryGetPlayer(playerId, out var playerState);
		return playerState.opinions.GetOpinion(opponentId) > OpinionManager.LoveLimit;
	}

	public static float RateBattle(GameState gameState, UnitState attackingUnit, TileData defendingTile)
	{
		if (attackingUnit.HasAbility(UnitAbility.Type.Infiltrate, gameState) && defendingTile.HasOpponentCity(attackingUnit.owner))
		{
			return 1000f;
		}
		UnitState unit = defendingTile.unit;
		if (unit == null)
		{
			return 0f;
		}
		BattleResults battleResults = BattleHelpers.GetBattleResults(gameState, attackingUnit, unit);
		if (!gameState.GameLogicData.TryGetData(unit.type, out var data))
		{
			return 0f;
		}
		if (!gameState.GameLogicData.TryGetData(attackingUnit.type, out var data2))
		{
			return 0f;
		}
		float num = battleResults.attackDamage * data.cost;
		int cost = data2.cost;
		if (battleResults.retaliationDamage > 0)
		{
			num -= (float)(battleResults.retaliationDamage * cost);
		}
		num *= 0.05f;
		bool flag = battleResults.attackDamage >= unit.health;
		if (flag)
		{
			num += 10f;
			if (data2.HasAbility(UnitAbility.Type.Eat))
			{
				num += 5f;
			}
			if (data2.HasAbility(UnitAbility.Type.Poison) && !unit.HasEffect(UnitEffect.Poisoned))
			{
				num -= 5f;
			}
			if (data2.HasAbility(UnitAbility.Type.Persist))
			{
				num += 5f;
			}
		}
		else if (data.HasAbility(UnitAbility.Type.Poison) && !attackingUnit.HasEffect(UnitEffect.Poisoned))
		{
			num -= 5f;
		}
		if (battleResults.retaliationDamage >= data2.health)
		{
			num -= 2f;
		}
		TileData tile = gameState.Map.GetTile(attackingUnit.coordinates);
		if (!flag && tile != null && tile.HasImprovement(ImprovementData.Type.City))
		{
			num -= 2f;
		}
		if (attackingUnit.HasAbility(UnitAbility.Type.Convert, gameState))
		{
			num = (float)data.cost * 20f;
		}
		if (!attackingUnit.HasAbility(UnitAbility.Type.Convert, gameState) && unit.HasFollower())
		{
			num -= 50f;
		}
		if (data2.HasAbility(UnitAbility.Type.Poison) && !unit.HasEffect(UnitEffect.Poisoned))
		{
			num += (float)unit.GetDefence(gameState);
		}
		gameState.TryGetPlayer(attackingUnit.owner, out var playerState);
		if (unit.HasEffect(UnitEffect.Poisoned) && playerState != null && playerState.KnowsResource(ResourceData.Type.Spores, gameState))
		{
			num += 2f;
			if (flag && (tile.owner == attackingUnit.owner || tile.IsWater))
			{
				num += 5f;
			}
		}
		if (!attackingUnit.HasAbility(UnitAbility.Type.Poison, gameState) && !attackingUnit.HasAbility(UnitAbility.Type.Freeze, gameState) && battleResults.attackDamage <= 0)
		{
			num -= 10f;
		}
		return num;
	}

	public static float StupidFactor(PlayerState player)
	{
		return ((float)GetRandom().NextDouble() - 0.5f) * (float)player.GetStupidity() * 50f;
	}

	public static List<WorldCoordinates> GetRoute(GameState gameState, TileData fromTile, TileData toTile, PathFinderSettings settings)
	{
		return gameState.Map.GetPath(fromTile.coordinates, toTile.coordinates, 1000, settings);
	}

	public static UnitStats GetDesiredUnitStats(GameState gameState, PlayerMapData mapData, PlayerState player, TileData tileData, float unitNeed)
	{
		UnitStats result = new UnitStats
		{
			defence = unitNeed,
			attack = unitNeed,
			range = unitNeed,
			movement = unitNeed
		};
		int num = 5;
		result.range += player.aiState.roughTerrain * (float)num;
		int num2 = 0;
		int num3 = 10;
		foreach (Mission mission in player.aiState.missions)
		{
			int num4 = MapDataExtensions.ChebyshevDistance(mission.tile.coordinates, tileData.coordinates);
			num2 += num4;
			if (num4 < num3)
			{
				num3 = num4;
			}
		}
		result.movement += (float)num3 * 0.5f;
		int num5 = 10;
		result.movement += player.aiState.militaryNeeds.Movement * (float)num5;
		result.range += player.aiState.militaryNeeds.Range * (float)num5;
		result.attack += player.aiState.militaryNeeds.Attack * (float)num5;
		result.defence += player.aiState.militaryNeeds.Defense * (float)num5;
		int num6 = 0;
		result.abilities = new Dictionary<UnitAbility.Type, float>();
		if (gameState.GameLogicData.TryGetData(player.tribe, out var data) && data.HasUnitAbility(UnitAbility.Type.Freeze, gameState))
		{
			int num7 = 0;
			foreach (TileData item in gameState.Map.GetArea(tileData.coordinates, 3, allowDiagonal: true))
			{
				if (item.GetExplored(player.Id))
				{
					if ((item.climate != player.GetTribeClimate(gameState) && item.owner == 0) || item.IsWater)
					{
						num7 += 2;
					}
					if (item.unit != null && item.unit.owner == player.Id && item.unit.HasAbility(UnitAbility.Type.FreezeArea, gameState))
					{
						num7 -= 80;
					}
					if (item.terrain == TerrainData.Type.Ice)
					{
						num6++;
					}
				}
			}
			result.abilities[UnitAbility.Type.FreezeArea] = num7;
			result.abilities[UnitAbility.Type.Skate] = num6 * 5;
		}
		int num8 = 0;
		foreach (UnitState unit in mapData.units)
		{
			if (unit.HasAbility(UnitAbility.Type.Infiltrate, gameState))
			{
				num8++;
			}
		}
		result.abilities[UnitAbility.Type.Infiltrate] = (float)player.aiState.enemyPopulation * 1f / ((float)num8 + 1f);
		Log.Verbose("[flx]Need for infiltrators:" + player.aiState.enemyPopulation + "/" + (num8 + 1) + " = " + result.abilities[UnitAbility.Type.Infiltrate], Array.Empty<object>());
		return result;
	}

	private static float GetBuildUnitScore(GameState gameState, PlayerMapData mapData, PlayerState player, TileData tile, UnitData unit, UnitStats desiredUnitStats)
	{
		float num = (float)unit.attack * 0.1f * desiredUnitStats.attack;
		num += (float)unit.defence * 0.1f * desiredUnitStats.defence;
		num += (float)unit.movement * desiredUnitStats.movement;
		num += (float)unit.range * desiredUnitStats.range;
		if (desiredUnitStats.abilities != null)
		{
			foreach (KeyValuePair<UnitAbility.Type, float> ability in desiredUnitStats.abilities)
			{
				if (unit.HasAbility(ability.Key))
				{
					num += ability.Value;
				}
			}
		}
		UnitState unitState = UnitState.Create(gameState, player.Id, (ushort)gameState.CurrentTurn, unit, tile.coordinates, tile.coordinates);
		float num2 = RatePosition(gameState, mapData, player, unitState, tile) * 0.5f;
		num += num2;
		float num3 = 1f + (float)unit.cost / (Math.Max(player.Currency, 1f) * 0.8f);
		return num / num3;
	}

	private static float RatePosition(GameState gameState, PlayerMapData mapData, PlayerState player, UnitState unitState, TileData tileData, bool canAttack = false)
	{
		if (tileData.owner != 0 && player.HasPeaceWith(tileData.owner) && player.opinions.GetOpinion(tileData.owner) < 0f)
		{
			return -1000f;
		}
		gameState.GameLogicData.TryGetData(unitState.type, out var data);
		float num = 0f;
		if (canAttack)
		{
			float num2 = 0f;
			int range = data.GetRange();
			foreach (WorldCoordinates item in UnitDataExtensions.GetAttackOptionsAtPosition(gameState, player.Id, tileData.coordinates, range))
			{
				float num3 = 0f;
				TileData tile = gameState.Map.GetTile(item);
				if (tile.unit != null)
				{
					num3 = RateBattle(gameState, unitState, tile) * (float)player.GetAggression(tileData.owner, gameState) * 0.001f;
				}
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
			if (num2 > 0f)
			{
				num += num2;
			}
		}
		float num4 = 1f;
		if (canAttack)
		{
			num4 -= 0.3f;
			if (unitState.HasAbility(UnitAbility.Type.Escape, gameState))
			{
				num4 -= 0.6f;
			}
		}
		float num5 = 0f;
		int num6 = 0;
		bool flag = unitState.HasAbility(UnitAbility.Type.AutoFreeze, gameState) || unitState.HasAbility(UnitAbility.Type.FreezeArea, gameState);
		int num7 = 0;
		foreach (TileData item2 in gameState.Map.GetArea(tileData.coordinates, 1, allowDiagonal: true, includeCenter: false))
		{
			if (!item2.GetExplored(player.Id))
			{
				num += 2f;
				continue;
			}
			UnitState unit = item2.GetUnit(gameState, player.Id);
			if (unit == null)
			{
				continue;
			}
			if (unit.owner == player.Id)
			{
				if (unit.IsDetectingHiddenUnits(gameState))
				{
					num += 20f;
				}
				continue;
			}
			num6++;
			float num8 = 0f - RateBattle(gameState, unit, tileData);
			num5 += num8 * num4;
			if (unit.HasAbility(UnitAbility.Type.FreezeArea, gameState) && !unitState.HasAbility(UnitAbility.Type.Dash, gameState) && !unitState.HasEffect(UnitEffect.Invisible))
			{
				num -= 30f;
			}
			if (flag && item2.IsFreezable(gameState, player))
			{
				num7++;
			}
		}
		num += (float)(num7 * 5);
		if (unitState.HasEffect(UnitEffect.Invisible))
		{
			num -= (float)(num6 * 3);
		}
		else if (num6 > 0)
		{
			num += num5 / (float)num6;
		}
		foreach (Mission mission in player.aiState.missions)
		{
			if ((!data.HasAbility(UnitAbility.Type.Disloyal) || mission.tile.owner == tileData.owner) && (mission.type != MissionType.Capture || unitState.CanCapture(gameState, mission.tile, includeNextTurn: true, allowOnlyOnTile: false)))
			{
				float num9 = mission.score / (float)Math.Max(1, MapDataExtensions.ManhattanDistance(tileData.coordinates, mission.tile.coordinates));
				num += num9;
				if (mission.tile == tileData)
				{
					num += mission.score;
				}
			}
		}
		int num10;
		int num11;
		if (tileData.owner != 0 && tileData.owner != player.Id)
		{
			num10 = ((!player.HasPeaceWith(tileData.owner)) ? 1 : 0);
			if (num10 != 0)
			{
				num11 = player.GetAggression(tileData.owner, gameState);
				goto IL_035d;
			}
		}
		else
		{
			num10 = 0;
		}
		num11 = 0;
		goto IL_035d;
		IL_035d:
		int num12 = num11;
		if (tileData.HasImprovement(ImprovementData.Type.City))
		{
			if (tileData.owner != player.Id)
			{
				num = ((!unitState.CanCapture(gameState, tileData, includeNextTurn: true, allowOnlyOnTile: false) && CanAllyCapture(gameState, mapData, tileData)) ? (num - 50f) : (num + (float)num12 * 0.1f));
			}
			else if (IsCityThreatened(gameState, tileData))
			{
				int num13 = (ThreatenedUnguardedCityCount(gameState, mapData.cityTiles) + 1) * 2;
				bool num14 = CommandValidation.CanCitySupportUnit(gameState, tileData.coordinates) && num13 < player.Currency;
				float num15 = (float)(unitState.GetDefence(gameState) / unitState.GetDefenceBonus(gameState) - 20) + ((float)(unitState.health / unitState.GetMaxHealth(gameState)) - 5f);
				num += num15 * 0.2f;
				if (!num14)
				{
					num += 300f;
				}
				else
				{
					float num16 = (float)mapData.units.Count / (float)mapData.cityTiles.Count;
					Math.Min(num16, 5f);
					float num17 = (0f - (float)(player.Currency / num13)) * 10f;
					num += num16 + num17;
				}
			}
			else
			{
				num -= 10f;
			}
		}
		if (tileData.improvement != null && gameState.GameLogicData.TryGetData(tileData.improvement.type, out var data2) && data2.HasAbility(ImprovementAbility.Type.Poison) && !player.HasTribeAbility(TribeAbility.Type.PoisonResist, gameState))
		{
			num -= 20f;
			if (!unitState.HasEffect(UnitEffect.Poisoned))
			{
				num -= 50f;
			}
			if (unitState.health <= 20)
			{
				num -= 100f;
			}
		}
		if (num10 != 0 && num12 < 0)
		{
			num += num * (float)(num12 / 1000);
		}
		return num + StupidFactor(player);
	}

	private static bool CanAllyCapture(GameState gameState, PlayerMapData mapData, TileData tileData)
	{
		foreach (UnitState unit in mapData.units)
		{
			if (MapDataExtensions.ChebyshevDistance(unit.coordinates, tileData.coordinates) < 2 && unit.CanMove() && unit.CanCapture(gameState, tileData, includeNextTurn: true, allowOnlyOnTile: false))
			{
				return true;
			}
		}
		return false;
	}

	private static int ThreatenedUnguardedCityCount(GameState gameState, List<TileData> cityTiles)
	{
		int num = 0;
		foreach (TileData cityTile in cityTiles)
		{
			if (cityTile.unit == null && IsCityThreatened(gameState, cityTile))
			{
				num++;
			}
		}
		return num;
	}

	private static bool IsCityThreatened(GameState gameState, TileData tileData)
	{
		if (!tileData.HasImprovement(ImprovementData.Type.City))
		{
			return false;
		}
		int owner = tileData.owner;
		foreach (TileData item in gameState.Map.GetArea(tileData.coordinates, 3, allowDiagonal: true, includeCenter: false))
		{
			if (item.unit != null && item.unit.owner != owner)
			{
				int num = MapDataExtensions.ChebyshevDistance(tileData.coordinates, item.coordinates);
				if (item.unit.GetMovement(gameState) >= num)
				{
					return true;
				}
			}
		}
		return false;
	}
}
