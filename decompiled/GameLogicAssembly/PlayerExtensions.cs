using System;
using System.Collections.Generic;
using Polytopia.Data;

public static class PlayerExtensions
{
	public static bool KnowsPlayer(this PlayerState player, byte playerId)
	{
		if (playerId == byte.MaxValue || player.Id == playerId)
		{
			return true;
		}
		return player.knownPlayers.Contains(playerId);
	}

	public static TribeData.Type GetVisualTribeType(this PlayerState player)
	{
		if (player.tribeMix == TribeData.Type.None)
		{
			return player.tribe;
		}
		return player.tribeMix;
	}

	public static int GetTribeStyle(this PlayerState player, GameState gameState)
	{
		if (player == null || gameState == null)
		{
			return 0;
		}
		TribeData.Type visualTribeType = player.GetVisualTribeType();
		if (gameState.GameLogicData.TryGetData(visualTribeType, out var data))
		{
			return data.style;
		}
		return 0;
	}

	public static int GetTribeClimate(this PlayerState player, GameState gameState)
	{
		if (gameState.GameLogicData.TryGetData(player.tribe, out var data))
		{
			return data.climate;
		}
		return 0;
	}

	public static TribeData GetTribeData(this PlayerState player, GameState gameState)
	{
		if (gameState.GameLogicData.TryGetData(player.tribe, out var data))
		{
			return data;
		}
		return null;
	}

	public static int GetPlayerColorInt(this PlayerState playerState, GameState gameState)
	{
		if (playerState.colorOverride >= 0)
		{
			return playerState.colorOverride;
		}
		if (gameState.GameLogicData.TryGetData(playerState.tribeMix, out var data))
		{
			return data.color;
		}
		if (gameState.GameLogicData.TryGetData(playerState.tribe, out var data2))
		{
			return data2.color;
		}
		return 0;
	}

	public static int CountCities(this PlayerState player, GameState state)
	{
		int num = 0;
		TileData[] tiles = state.Map.Tiles;
		int num2 = tiles.Length;
		for (int i = 0; i < num2; i++)
		{
			TileData tileData = tiles[i];
			if (tileData.improvement != null && tileData.improvement.type == ImprovementData.Type.City && tileData.owner == player.Id)
			{
				num++;
			}
		}
		return num;
	}

	public static WorldCoordinates GetCurrentCapitalCoordinates(this PlayerState playerState, GameState state)
	{
		if (playerState.OwnsTheirCapital(state))
		{
			return playerState.startTile;
		}
		List<TileData> list = new List<TileData>();
		state.Map.GetPlayerCityTiles(playerState.Id, list);
		if (list.Count > 0)
		{
			return list[0].coordinates;
		}
		return WorldCoordinates.NULL_COORDINATES;
	}

	public static List<TileData> GetCityTiles(this PlayerState playerState, GameState gameState)
	{
		List<TileData> list = new List<TileData>();
		gameState.Map.GetPlayerCityTiles(playerState.Id, list);
		return list;
	}

	public static bool OwnsTheirCapital(this PlayerState playerState, GameState gameState)
	{
		return gameState.Map.GetTile(playerState.startTile).owner == playerState.Id;
	}

	public static int CountCities(this PlayerState player, GameState state, out List<WorldCoordinates> cityCoordinates)
	{
		cityCoordinates = new List<WorldCoordinates>();
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.owner == player.Id && tileData.improvement != null && tileData.improvement.type == ImprovementData.Type.City)
			{
				cityCoordinates.Add(tileData.coordinates);
			}
		}
		return cityCoordinates.Count;
	}

	public static int CountCapitals(this PlayerState player, GameState gameState)
	{
		int num = 0;
		for (int i = 0; i < gameState.PlayerStates.Count; i++)
		{
			PlayerState playerState = gameState.PlayerStates[i];
			if (playerState != null && playerState.Id != byte.MaxValue && gameState.Map.GetTile(playerState.startTile).owner == player.Id)
			{
				num++;
			}
		}
		return num;
	}

	public static int CountPopulation(this PlayerState player, GameState state)
	{
		List<TileData> list = new List<TileData>();
		state.Map.GetPlayerCityTiles(player.Id, list);
		short num = 0;
		foreach (TileData item in list)
		{
			num += item.improvement.population;
		}
		return num;
	}

	public static int CountUnits(this PlayerState player, GameState gameState)
	{
		int num = 0;
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner == player.Id)
			{
				num++;
			}
		}
		return num;
	}

	public static int CountUnits(this PlayerState player, GameState gameState, out List<UnitState> units)
	{
		units = new List<UnitState>();
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner == player.Id)
			{
				units.Add(tileData.unit);
			}
		}
		return units.Count;
	}

	public static bool AnyUnitCanPerformAction(this PlayerState player, GameState gameState)
	{
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner == player.Id && tileData.unit.CanPerformAnyAction(gameState))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetDefenceBonus(this PlayerState player, TerrainData.Type terrain, GameState gameState)
	{
		return gameState.GameLogicData.GetUnlockedDefenceBonus(player, terrain);
	}

	public static bool KnowsResource(this PlayerState playerState, ResourceData.Type resourceType, GameState gameState)
	{
		ImprovementData improvementForResource = gameState.GameLogicData.GetImprovementForResource(resourceType);
		if (improvementForResource != null)
		{
			if (!gameState.GameLogicData.IsUnlocked(improvementForResource.type, playerState))
			{
				return gameState.GameLogicData.IsUnlockable(improvementForResource.type, playerState);
			}
			return true;
		}
		return false;
	}

	public static bool HasAbility(this PlayerState player, PlayerAbility.Type ability, GameState gameState)
	{
		foreach (PlayerAbility.Type unlockedAbility in gameState.GameLogicData.GetUnlockedAbilities(player))
		{
			if (unlockedAbility == ability)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasTech(this PlayerState player, TechData.Type tech)
	{
		if (player.availableTech != null)
		{
			return player.availableTech.Contains(tech);
		}
		return false;
	}

	public static bool HasTribeAbility(this PlayerState player, TribeAbility.Type ability, GameState gameState)
	{
		if (player == null || player.Id == 0)
		{
			return false;
		}
		if (gameState.GameLogicData.TryGetData(player.tribe, out var data))
		{
			return data.HasAbility(ability);
		}
		return false;
	}

	public static string GetAggressionsDebugString(this PlayerState playerState)
	{
		string text = "Aggressions: ";
		foreach (KeyValuePair<byte, int> aggression in playerState.aggressions)
		{
			text = text + aggression.Key + ": " + aggression.Value + ", ";
		}
		return text;
	}

	public static void SetAgression(this PlayerState playerState, byte otherPlayerId, int aggression)
	{
		if (playerState.Id != otherPlayerId && otherPlayerId != 0)
		{
			playerState.aggressions[otherPlayerId] = aggression;
		}
	}

	public static int GetOrCreateAggression(this PlayerState playerState, byte otherPlayerId)
	{
		if (playerState.aggressions.TryGetValue(otherPlayerId, out var value))
		{
			return value;
		}
		playerState.aggressions[otherPlayerId] = 0;
		return 0;
	}

	public static void RemoveNonagression(this PlayerState playerState, byte otherPlayerId)
	{
		if (playerState.GetOrCreateAggression(otherPlayerId) < 1000)
		{
			playerState.aggressions[otherPlayerId] = 1000;
		}
	}

	public static int GetAggression(this PlayerState player, byte otherPlayerId, GameState gameState)
	{
		if (gameState.Version < 60)
		{
			if (otherPlayerId != 0 && otherPlayerId != player.Id)
			{
				return player.GetOrCreateAggression(otherPlayerId);
			}
			return 1000;
		}
		if (!player.opinions.IsInitiated)
		{
			player.opinions.UpdateOpinions(gameState, player);
		}
		return (int)(player.opinions.GetOpinion(otherPlayerId) * -1000f);
	}

	public static void ModifyAggression(this PlayerState playerState, byte otherPlayerId, int aggressionModifier)
	{
		if (playerState.Id != otherPlayerId)
		{
			int orCreateAggression = playerState.GetOrCreateAggression(otherPlayerId);
			playerState.aggressions[otherPlayerId] = orCreateAggression + aggressionModifier;
		}
	}

	public static void NormalizeAggression(this PlayerState playerState, byte otherPlayerId)
	{
		int orCreateAggression = playerState.GetOrCreateAggression(otherPlayerId);
		orCreateAggression += (1000 - orCreateAggression) / 7;
		playerState.SetAgression(otherPlayerId, orCreateAggression);
	}

	public static void ReactToAttack(PlayerState attacker, PlayerState defender, TileData tile, GameState state)
	{
		defender.RemoveNonagression(attacker.Id);
		defender.ModifyAggression(attacker.Id, 1000);
		if (attacker.GetAggression(defender.Id, state) > 1000)
		{
			attacker.ModifyAggression(defender.Id, -200);
		}
		foreach (PlayerState playerState in state.PlayerStates)
		{
			if (playerState.Id != attacker.Id && tile.GetExplored(playerState.Id))
			{
				int aggressionModifier = -(playerState.GetAggression(defender.Id, state) - 1000) / 5;
				playerState.ModifyAggression(attacker.Id, aggressionModifier);
			}
		}
		foreach (PlayerState playerState2 in state.PlayerStates)
		{
			if (tile.GetExplored(playerState2.Id))
			{
				playerState2.ModifyAggression(attacker.Id, 1000 - playerState2.GetAggression(defender.Id, state) / 2);
			}
		}
		attacker.SetLastAttack(defender.Id, (int)state.CurrentTurn, state);
	}

	public static uint GetScore(this PlayerState player)
	{
		return player.score;
	}

	public static bool IsAlive(this PlayerState player, GameState gameState)
	{
		return player.IsAlive(gameState, gameState.Settings.rules.PlayerDeathCondition);
	}

	public static bool IsAlive(this PlayerState player, GameState gameState, GameRules.DeathCondition condition)
	{
		if (player.Id == byte.MaxValue)
		{
			return true;
		}
		switch (condition)
		{
		case GameRules.DeathCondition.Cities:
			return player.CountCities(gameState) > 0;
		case GameRules.DeathCondition.Units:
			if (player.CountCities(gameState) > 0)
			{
				return player.CountUnits(gameState) > 0;
			}
			return false;
		default:
			return true;
		}
	}

	public static ScoreDetails GetScore(this PlayerState player, GameState state)
	{
		byte id = player.Id;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		int num13 = state.Map.Tiles.Length;
		for (int i = 0; i < num13; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.owner == id)
			{
				num5++;
				if (tileData.improvement != null)
				{
					if (tileData.improvement.type == ImprovementData.Type.City)
					{
						int num14 = state.CalculateImprovementScore(tileData);
						num2 += num14;
						num4++;
					}
					else
					{
						int num15 = state.CalculateImprovementScore(tileData);
						num += num15;
						if (tileData.improvement.IsTemple())
						{
							num9++;
						}
						else if (tileData.improvement.IsMonument())
						{
							num10++;
						}
					}
				}
			}
			if (tileData.GetExplored(id))
			{
				num6++;
			}
			if (tileData.unit != null && tileData.unit.owner == id && state.GameLogicData.TryGetData(tileData.unit.type, out var data))
			{
				int num16 = (int)ScoreSheet.GetUnitScore(data);
				num8 += num16;
				num7++;
			}
		}
		List<TechData> unlockedTech = state.GameLogicData.GetUnlockedTech(player);
		if (state.GameLogicData.TryGetData(player.tribe, out var data2))
		{
			num11 = state.GameLogicData.GetAllTechForTribe(data2).Count;
		}
		if (unlockedTech != null)
		{
			num12 = unlockedTech.Count;
			foreach (TechData item in unlockedTech)
			{
				int num17 = (int)ScoreSheet.GetTechScore(item);
				num3 += num17;
			}
		}
		int num18 = num5 * ScoreSheet.tileValue;
		num18 += num6 * ScoreSheet.exploreValue;
		num18 += num8;
		ScoreDetails scoreDetails = new ScoreDetails();
		scoreDetails.territoryScore = num18;
		scoreDetails.territoryInfoParams = new object[2] { num7, num5 };
		scoreDetails.cultureScore = num;
		scoreDetails.cultureInfoParams = new object[2] { num10, num9 };
		scoreDetails.cityScore = num2;
		scoreDetails.cityInfoParams = new object[1] { num4 };
		scoreDetails.techScore = num3;
		scoreDetails.techInfoParams = new object[2] { num12, num11 };
		int num19 = num18 + num + num2 + num3;
		scoreDetails.difficultyBonusMultiplier = ScoreSheet.GetDifficultyBonusMultiplier(state.Settings.Difficulty, state.PlayerCount - 1);
		scoreDetails.difficultyBonus = (int)Math.Round((float)num19 * scoreDetails.difficultyBonusMultiplier);
		scoreDetails.totalScore = num19;
		return scoreDetails;
	}

	public static PlayerRating GetRating(this PlayerState player, GameState state, bool playerIsWinner = false)
	{
		PlayerRating playerRating = new PlayerRating();
		int opponentCount = state.Settings.OpponentCount;
		playerRating.battleScore = (int)Math.Round((double)(player.kills + 1) / (double)(player.casualities + player.kills + 1) * 100.0);
		playerRating.wipeScore = (int)Math.Round((double)player.wipeOuts / (double)opponentCount * 100.0);
		playerRating.timeGoal = (int)Math.Round((double)opponentCount * 10.0);
		int num = Math.Max((int)state.CurrentTurn, 1);
		playerRating.timeScore = (int)Math.Round(Math.Min(100.0, (double)playerRating.timeGoal / (double)num * 100.0));
		if (!playerIsWinner)
		{
			playerRating.timeScore = 0;
		}
		playerRating.difficultyScore = (int)Math.Round(state.GetDifficultyScore());
		playerRating.averageRating = (int)Math.Round((double)(playerRating.battleScore + playerRating.wipeScore + playerRating.timeScore + playerRating.difficultyScore) / 4.0);
		return playerRating;
	}

	public static bool HasMadeCurrentTurn(this PlayerState playerState, GameState gameState)
	{
		return gameState.IndexOfPlayer(playerState) < gameState.CurrentPlayerIndex;
	}
}
