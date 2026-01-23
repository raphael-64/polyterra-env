using System;
using System.Collections.Generic;
using Polytopia.Data;
using PolytopiaBackendBase.Game;

public class OpinionManager
{
	public enum Type
	{
		CommonRelation,
		DifferentRelation,
		Peaceful,
		Aggression,
		Embassy,
		Threatening,
		Intrusive,
		Winning,
		Brave,
		Dislike,
		Like,
		Weak,
		Powerful
	}

	public static float LoveLimit = 2f;

	public static float HateLimit = -2f;

	public static int Longsight = 4;

	public Dictionary<byte, OpinionState> Opinions = new Dictionary<byte, OpinionState>();

	public List<TileData> tiles = new List<TileData>();

	public Dictionary<byte, MilitaryStats> knownPlayerMilitaryStats = new Dictionary<byte, MilitaryStats>();

	private PlayerState player;

	public bool IsInitiated;

	private List<PlayerState> playerRanked;

	public void UpdateOpinions(GameState gameState, PlayerState _player)
	{
		player = _player;
		playerRanked = gameState.GetPlayersSortedByRank();
		tiles.Clear();
		knownPlayerMilitaryStats.Clear();
		TileData[] array = gameState.Map.Tiles;
		foreach (TileData tileData in array)
		{
			AI.CollectMilitaryStats(gameState, knownPlayerMilitaryStats, tileData);
			if (!tileData.GetExplored(player.Id) || !tileData.HasImprovement(ImprovementData.Type.City) || tileData.owner != player.Id)
			{
				continue;
			}
			foreach (TileData item in gameState.Map.GetArea(tileData.coordinates, tileData.improvement.borderSize, allowDiagonal: true))
			{
				if (item.owner != player.Id)
				{
					continue;
				}
				foreach (TileData item2 in gameState.Map.GetArea(item.coordinates, 1, allowDiagonal: true))
				{
					if (!tiles.Contains(item2))
					{
						tiles.Add(item2);
					}
				}
			}
		}
		float num = 0f;
		float num2 = 0f;
		foreach (PlayerState playerState2 in gameState.PlayerStates)
		{
			if (playerState2.IsAlive(gameState) && playerState2.Id != player.Id)
			{
				UpdateOpinion(gameState, player, playerState2);
				if (player.HasPeaceWith(playerState2.Id))
				{
					num += 1f;
				}
				num2 += 1f;
			}
		}
		num /= Math.Max(1f, num2);
		OpinionState opinionState = null;
		foreach (KeyValuePair<byte, OpinionState> opinion in Opinions)
		{
			gameState.TryGetPlayer(opinion.Key, out var playerState);
			if (!player.HasPeaceWith(playerState.Id))
			{
				opinion.Value.AddOpinion(num * -1f, Type.Dislike);
			}
			if (opinionState == null || opinion.Value.total < opinionState.total)
			{
				opinionState = opinion.Value;
			}
		}
		opinionState?.AddOpinion(-0.5f, Type.Dislike);
		IsInitiated = true;
	}

	private void UpdateOpinion(GameState gameState, PlayerState player, PlayerState opponent)
	{
		if (player == opponent || player.Id == byte.MaxValue || opponent.Id == byte.MaxValue || (player.GetRelation(opponent.Id).FirstMeet < 0 && opponent.GetRelation(player.Id).LastAttackTurn < 0))
		{
			return;
		}
		OpinionState opinionState = new OpinionState();
		if (!opponent.IsAlive(gameState))
		{
			return;
		}
		List<PlayerState> playersSortedByRank = gameState.GetPlayersSortedByRank();
		PlayerState playerState = playersSortedByRank[0];
		float num = 0f;
		if (gameState.Settings.RulesGameMode == GameMode.Might)
		{
			num += (float)opponent.CountCapitals(gameState) / (float)playersSortedByRank.Count;
		}
		if (gameState.Settings.RulesGameMode == GameMode.Glory)
		{
			num += (float)(opponent.score / gameState.Settings.rules.ScoreLimit);
		}
		if (gameState.Settings.RulesGameMode == GameMode.Perfection)
		{
			num += (float)gameState.CurrentTurn / (float)gameState.Settings.rules.TurnLimit;
		}
		if (gameState.Settings.RulesGameMode == GameMode.Domination)
		{
			num += (float)playerState.CountCapitals(gameState) / (float)gameState.PlayerCount;
		}
		if (playerState != opponent && opponent.HasWarWith(playerState, gameState) && playerState != player)
		{
			opinionState.AddOpinion(num * num * 1f, Type.Brave);
		}
		if (playerRanked.Contains(opponent))
		{
			float num2 = playerRanked.IndexOf(opponent) + 1;
			float ammount = num * num * 4f / (num2 * num2);
			opinionState.AddOpinion(ammount, Type.Winning);
		}
		float num3 = 0f;
		foreach (PlayerState playerState2 in gameState.PlayerStates)
		{
			if (playerState2 == player || playerState2 == opponent)
			{
				continue;
			}
			if (player.HasWarWith(playerState2, gameState))
			{
				if (opponent.HasWarWith(playerState2, gameState))
				{
					num3 += 2f;
				}
				if (opponent.HasPeaceWith(playerState2.Id))
				{
					num3 -= 1f;
				}
			}
			if (player.HasPeaceWith(playerState2.Id))
			{
				if (opponent.HasWarWith(playerState2, gameState))
				{
					num3 -= 2f;
				}
				if (opponent.HasPeaceWith(playerState2.Id))
				{
					num3 += 1f;
				}
			}
		}
		if (player.GetRelation(opponent.Id).LastPeaceBrokenTurn > 0)
		{
			float num4 = (float)(gameState.CurrentTurn - player.GetRelation(opponent.Id).LastPeaceBrokenTurn) + 1f;
			float num5 = (float)Longsight / num4;
			num3 -= num5;
		}
		num3 = Math.Min(num3, 2f);
		if (num3 > 0f)
		{
			opinionState.AddOpinion(num3, Type.CommonRelation);
		}
		if (num3 < 0f)
		{
			opinionState.AddOpinion(0f - num3, Type.DifferentRelation);
		}
		opponent.HasMadeCurrentTurn(gameState);
		int num6 = opponent.GetLastAttack(player.Id);
		if (num6 < player.GetRelation(opponent.Id).FirstMeet)
		{
			num6 = player.GetRelation(opponent.Id).FirstMeet - Longsight;
		}
		float num7 = (float)((int)gameState.CurrentTurn - num6 - Longsight) / (float)Longsight;
		if (num7 > 0f)
		{
			opinionState.AddOpinion(Math.Min(num7, 2f) * 0.5f, Type.Peaceful);
		}
		else
		{
			opinionState.AddOpinion(num7 * -2f, Type.Aggression);
		}
		if (player.HasEmbassyWith(opponent))
		{
			opinionState.AddOpinion(1f, Type.Embassy);
		}
		if (opponent.HasEmbassyWith(player))
		{
			opinionState.AddOpinion(1f, Type.Embassy);
		}
		foreach (TileData tile in tiles)
		{
			UnitState unit = tile.GetUnit(gameState, player.Id);
			if (unit != null && unit.owner == opponent.Id)
			{
				if (tile.owner == player.Id)
				{
					if (player.HasPeaceWith(opponent.Id))
					{
						opinionState.AddOpinion(0.2f, Type.Threatening);
					}
					else
					{
						opinionState.AddOpinion(1f, Type.Threatening);
					}
					if (tile.HasImprovement(ImprovementData.Type.City))
					{
						opinionState.AddOpinion(2f, Type.Threatening);
					}
				}
				else if (!player.HasPeaceWith(opponent.Id))
				{
					opinionState.AddOpinion(0.5f, Type.Threatening);
				}
			}
			if (tile.owner == opponent.Id)
			{
				opinionState.AddOpinion(0.1f, Type.Intrusive);
			}
		}
		if (!opponent.AutoPlay)
		{
			int num8 = 2 - player.handicap;
			if (num8 > 0)
			{
				opinionState.AddOpinion((float)num8 * 2f, Type.Like);
			}
		}
		float militaryAdvantage = GetMilitaryAdvantage(knownPlayerMilitaryStats, player.Id, opponent.Id);
		if (militaryAdvantage < 1f)
		{
			opinionState.AddOpinion((militaryAdvantage - 1f) * -1f, Type.Powerful);
		}
		else if (militaryAdvantage > 1f)
		{
			opinionState.AddOpinion((militaryAdvantage - 1f) * 1f, Type.Weak);
		}
		if (gameState.Map.GetTile(player.startTile).owner == opponent.Id)
		{
			opinionState.AddOpinion(2f, Type.Intrusive);
		}
		if (!Opinions.ContainsKey(opponent.Id))
		{
			Opinions[opponent.Id] = new OpinionState();
		}
		Opinions[opponent.Id].Clear();
		Opinions[opponent.Id].AddOpinion(opinionState.GetOpinion(Type.Brave), Type.Brave);
		Opinions[opponent.Id].AddOpinion(opinionState.GetOpinion(Type.Like), Type.Like);
		Opinions[opponent.Id].AddOpinion(opinionState.GetOpinion(Type.Embassy), Type.Embassy);
		Opinions[opponent.Id].AddOpinion(opinionState.GetOpinion(Type.Peaceful), Type.Peaceful);
		Opinions[opponent.Id].AddOpinion(opinionState.GetOpinion(Type.CommonRelation), Type.CommonRelation);
		Opinions[opponent.Id].AddOpinion(opinionState.GetOpinion(Type.Powerful), Type.Powerful);
		Opinions[opponent.Id].AddOpinion(opinionState.GetOpinion(Type.Threatening) * -1f, Type.Threatening);
		Opinions[opponent.Id].AddOpinion(opinionState.GetOpinion(Type.Aggression) * -1f, Type.Aggression);
		Opinions[opponent.Id].AddOpinion(opinionState.GetOpinion(Type.DifferentRelation) * -1f, Type.DifferentRelation);
		Opinions[opponent.Id].AddOpinion(opinionState.GetOpinion(Type.Weak) * -1f, Type.Weak);
		Opinions[opponent.Id].AddOpinion(opinionState.GetOpinion(Type.Winning) * -1f, Type.Winning);
		Opinions[opponent.Id].AddOpinion(opinionState.GetOpinion(Type.Intrusive) * -1f, Type.Intrusive);
	}

	public float GetMilitaryAdvantage(Dictionary<byte, MilitaryStats> playerMilitaryStats, byte playerId, byte otherPlayerId)
	{
		float num = 0f;
		if (playerMilitaryStats.TryGetValue(playerId, out var value))
		{
			num = value.GetWeightedTotal();
		}
		float num2 = 0f;
		if (playerMilitaryStats.TryGetValue(otherPlayerId, out var value2))
		{
			num2 = value2.GetWeightedTotal();
		}
		float result = Math.Min(2f, num / Math.Max(1f, num2));
		if (num == 0f && num2 == 0f)
		{
			result = 1f;
		}
		return result;
	}

	public float GetOpinion(byte opponent)
	{
		if (player == null)
		{
			return 0f;
		}
		if (Opinions == null)
		{
			return 0f;
		}
		if (!Opinions.ContainsKey(opponent))
		{
			return 0f;
		}
		if (Opinions.TryGetValue(opponent, out var value))
		{
			return value.total;
		}
		return 0f;
	}

	public List<KeyValuePair<Type, float>> GetReasons(byte opponent)
	{
		List<KeyValuePair<Type, float>> list = new List<KeyValuePair<Type, float>>();
		if (Opinions == null)
		{
			return list;
		}
		if (!Opinions.ContainsKey(opponent))
		{
			return list;
		}
		foreach (KeyValuePair<Type, float> reason in Opinions[opponent].reasons)
		{
			if (Math.Abs(reason.Value) > 0.1f)
			{
				list.Add(reason);
			}
		}
		list.Sort((KeyValuePair<Type, float> a, KeyValuePair<Type, float> b) => Math.Abs(b.Value).CompareTo(Math.Abs(a.Value)));
		return list;
	}
}
