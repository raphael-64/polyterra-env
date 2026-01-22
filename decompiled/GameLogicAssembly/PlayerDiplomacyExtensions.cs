using System.Collections.Generic;

public static class PlayerDiplomacyExtensions
{
	public static void SetLastAttack(this PlayerState player, byte opponentID, int turn, GameState gameState)
	{
		Log.Verbose("opinion {0} attacked {1}", new object[2] { player.Id, opponentID });
		DiplomacyRelation relation = player.GetRelation(opponentID);
		relation.PreviousAttackTurn = relation.LastAttackTurn;
		relation.LastAttackTurn = turn;
		gameState.TryGetPlayer(opponentID, out var playerState);
		playerState.GetRelation(player.Id).PreviousAttackTurn = turn;
		gameState.ActionStack.Add(new DestroyEmbassyAction(player.Id, opponentID));
		gameState.ActionStack.Add(new DestroyEmbassyAction(opponentID, player.Id));
	}

	public static int GetLastAttack(this PlayerState player, byte opponentID)
	{
		return player.GetRelation(opponentID).LastAttackTurn;
	}

	public static List<byte> GetEmbassiesInCapitalOf(this PlayerState player, GameState gameState)
	{
		List<byte> list = new List<byte>(gameState.PlayerStates.Count);
		foreach (PlayerState playerState in gameState.PlayerStates)
		{
			if (playerState.HasEmbassyWith(player))
			{
				list.Add(playerState.Id);
			}
		}
		return list;
	}

	public static int GetAllyCount(this PlayerState player, GameState gameState)
	{
		int num = 0;
		foreach (PlayerState playerState in gameState.PlayerStates)
		{
			if (playerState.Id != player.Id && playerState.Id != byte.MaxValue && player.HasPeaceWith(playerState.Id) && player.IsAlive(gameState))
			{
				num++;
			}
		}
		return num;
	}

	public static bool HasEmbassyWith(this PlayerState player, PlayerState opponent)
	{
		return opponent.GetRelation(player.Id).EmbassyLevel > 0;
	}

	public static bool HasActiveEmbassyWith(this PlayerState player, PlayerState opponent, GameState gameState)
	{
		if (opponent.GetRelation(player.Id).EmbassyLevel > 0)
		{
			return !player.HasWarWith(opponent, gameState);
		}
		return false;
	}

	public static int GetEmbassyLevel(this PlayerState player, PlayerState opponent)
	{
		return opponent.GetRelation(player.Id).EmbassyLevel;
	}

	public static int GetTotalIncomeFromEmbassiesAndDividend(this PlayerState playerState, PlayerState otherPlayer, GameState gameState)
	{
		return 0 + playerState.GetIncomeFromEmbassy(otherPlayer, gameState) + otherPlayer.GetIncomeFromEmbassy(playerState, gameState);
	}

	public static int GetIncomeFromEmbassy(this PlayerState playerState, PlayerState otherPlayer, GameState gameState)
	{
		int num = ((!playerState.HasPeaceWith(otherPlayer.Id)) ? 1 : 2);
		int num2 = 0;
		if (otherPlayer.HasActiveEmbassyWith(playerState, gameState))
		{
			num2 += gameState.GameLogicData.DiplomacyData.embassyIncome * otherPlayer.GetEmbassyLevel(playerState) * num;
		}
		return num2;
	}

	private static bool DidPlayerAttackOpponentWithinTurn(PlayerState player, PlayerState opponent, GameState gameState)
	{
		int num = (int)gameState.CurrentTurn - player.GetLastAttack(opponent.Id);
		if (num >= 1)
		{
			if (num == 1)
			{
				return !player.HasMadeCurrentTurn(gameState);
			}
			return false;
		}
		return true;
	}

	public static bool HasWarWith(this PlayerState player, PlayerState opponent, GameState gameState)
	{
		if (player.HasPeaceWith(opponent.Id))
		{
			return false;
		}
		if (!DidPlayerAttackOpponentWithinTurn(player, opponent, gameState))
		{
			return DidPlayerAttackOpponentWithinTurn(opponent, player, gameState);
		}
		return true;
	}

	public static bool HasPeaceWith(this PlayerState player, byte opponentID)
	{
		return player.GetRelation(opponentID).State == DiplomacyRelationState.Peace;
	}

	public static bool HasBrokenPeaceWith(this PlayerState player, byte opponentID)
	{
		return player.GetRelation(opponentID).State == DiplomacyRelationState.BrokenPeace;
	}

	private static int FindMessage(this PlayerState player, DiplomacyMessageType messageType, byte senderId)
	{
		for (int i = 0; i < player.messages.Count; i++)
		{
			DiplomacyMessage diplomacyMessage = player.messages[i];
			if (diplomacyMessage.Sender == senderId && diplomacyMessage.Type == messageType)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool TryRemoveMessage(this PlayerState player, DiplomacyMessageType messageType, byte senderId)
	{
		int num = player.FindMessage(messageType, senderId);
		if (num != -1)
		{
			player.messages.RemoveAt(num);
			return true;
		}
		return false;
	}

	public static bool HasMessage(this PlayerState player, DiplomacyMessageType messageType, byte senderId)
	{
		return player.FindMessage(messageType, senderId) != -1;
	}

	public static void SendMessage(this PlayerState recipientPlayer, DiplomacyMessageType messageType, byte senderId)
	{
		recipientPlayer.messages.Add(new DiplomacyMessage
		{
			Type = messageType,
			Sender = senderId
		});
	}

	public static DiplomacyRelation GetRelation(this PlayerState player, byte opponentID)
	{
		if (!player.relations.TryGetValue(opponentID, out var value))
		{
			value = new DiplomacyRelation();
			player.relations[opponentID] = value;
		}
		return value;
	}
}
