using System;
using System.Collections.Generic;
using System.Linq;
using Polytopia.Data;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;

public static class GameStateUtils
{
	public static Random utilityRng = new Random();

	public static TribeData.Type GetRandomPickableTribe(GameState gameState)
	{
		return GetRandomPickableTribe(gameState.Version, gameState.Settings, gameState.PlayerStates);
	}

	public static TribeData.Type GetRandomPickableTribe(int version, GameSettings settings, List<PlayerState> players)
	{
		return GetRandomPickableTribe(GameLogicDataUtils.GetPickableTribes(PolytopiaDataManager.GetGameLogicData(VersionManager.GetGameLogicDataVersionFromGameVersion(version)).GetAllTribes(), settings, players));
	}

	public static TribeData.Type GetRandomPickableTribe(ProbabilityTable tribes)
	{
		float randomValue = utilityRng.Value();
		int num = tribes.GetValueForRandomValue(randomValue);
		if (num == -1)
		{
			num = 7;
		}
		return (TribeData.Type)num;
	}

	public static List<TribeData> GetTribes(int gameVersion)
	{
		return PolytopiaDataManager.GetGameLogicData(VersionManager.GetGameLogicDataVersionFromGameVersion(gameVersion)).GetAllTribes();
	}

	public static int GetMaxPlayers(int gameVersion)
	{
		return GetTribes(gameVersion).Count;
	}

	public static int GetMaxOpponents(int gameVersion)
	{
		return GetMaxPlayers(gameVersion) - 1;
	}

	public static TimeSpan GetTotalTimeIncrement(this GameState gameState, PlayerState playerState)
	{
		return gameState.GetBaseTimeIncrement(playerState) + gameState.GetTimeBonusForPlayer(playerState);
	}

	public static TimeSpan GetBaseTimeIncrement(this GameState gameState, PlayerState playerState)
	{
		return TimeSpan.FromSeconds(gameState.Settings.BaseTimeSeconds);
	}

	public static TimeSpan GetTimeBonusForPlayer(this GameState gameState, PlayerState playerState)
	{
		List<TileData> list = new List<TileData>();
		gameState.Map.GetPlayerCityTiles(playerState.Id, list);
		int num = 0;
		if (gameState.Version < 94)
		{
			for (int i = 0; i < list.Count; i++)
			{
				TileData tileData = list[i];
				num += gameState.Map.GetCityUnitCount(tileData.coordinates);
			}
		}
		else
		{
			List<UnitState> list2 = new List<UnitState>();
			gameState.Map.GetPlayerUnits(playerState.Id, list2);
			num = list2.Count;
		}
		return TimeSpan.FromSeconds((float)list.Count * gameState.Settings.TimeBonusPerCity + (float)num * gameState.Settings.TimeBonusPerPopulation);
	}

	public static void SetPlayerNames(GameState gameState)
	{
		foreach (PlayerState playerState in gameState.PlayerStates)
		{
			if (string.IsNullOrEmpty(playerState.UserName))
			{
				gameState.GameLogicData.TryGetData(playerState.tribe, out var data);
				playerState.UserName = PolyLanguage.MakeWord(data, 0f, playerState.GetPlayerColorInt(gameState) + 1);
			}
		}
	}

	public static void SetPlayerColors(GameState gameState)
	{
		List<TribeData> allTribes = gameState.GameLogicData.GetAllTribes();
		List<int> list = new List<int>();
		List<PlayerState> list2 = new List<PlayerState>();
		foreach (PlayerState playerState in gameState.PlayerStates)
		{
			int playerColorInt = playerState.GetPlayerColorInt(gameState);
			if (list.Contains(playerColorInt))
			{
				list2.Add(playerState);
			}
			else
			{
				list.Add(playerColorInt);
			}
		}
		if (list2.Count > 0)
		{
			List<int> list3 = new List<int>();
			foreach (TribeData item in allTribes)
			{
				if (!list.Contains(item.color))
				{
					list3.Add(item.color);
				}
			}
			foreach (PlayerState item2 in list2)
			{
				list3.Remove(item2.colorOverride = ((list3.Count <= 0) ? ((int)utilityRng.Value() * 16777215) : list3[utilityRng.Next(0, list3.Count)]));
			}
		}
		SetPlayerNames(gameState);
	}

	public static int GetPlayerColor(GameState gameState, PlayerState player, List<PlayerState> players)
	{
		int result = player.GetPlayerColorInt(gameState);
		bool flag = false;
		List<int> list = new List<int>();
		foreach (PlayerState player2 in players)
		{
			if (player2.Id != player.Id && player2.hasChosenTribe && player2.tribe == player.tribe)
			{
				list.Add(player2.GetPlayerColorInt(gameState));
				flag = true;
			}
		}
		if (flag)
		{
			List<TribeData> allTribes = gameState.GameLogicData.GetAllTribes();
			List<int> list2 = new List<int>();
			foreach (TribeData item in allTribes)
			{
				if (!list.Contains(item.color))
				{
					list2.Add(item.color);
				}
			}
			result = ((list2.Count <= 0) ? ((int)utilityRng.Value() * 16777215) : list2[utilityRng.Next(0, list2.Count)]);
		}
		return result;
	}

	public static void AddNaturePlayer(GameState gameState)
	{
		if (!gameState.TryGetPlayer(byte.MaxValue, out var _))
		{
			PlayerState playerState2 = new PlayerState
			{
				Id = byte.MaxValue,
				UserName = "Nature",
				AccountId = Guid.Empty,
				AutoPlay = true,
				startTile = WorldCoordinates.NULL_COORDINATES,
				hasChosenTribe = true,
				tribe = TribeData.Type.Nature
			};
			gameState.PlayerStates.Add(playerState2);
			Log.Verbose("Created player: {0}", new object[1] { playerState2 });
		}
	}

	public static void AddAIOpponent(GameState gameState, TribeData.Type tribe, int handicap, string name = null)
	{
		int num = gameState.PlayerStates[gameState.PlayerStates.Count - 1].Id + 1;
		PlayerState playerState = new PlayerState
		{
			Id = (byte)num,
			AccountId = Guid.Empty,
			AutoPlay = true,
			startTile = WorldCoordinates.NULL_COORDINATES,
			hasChosenTribe = (tribe != TribeData.Type.None),
			tribe = tribe,
			skinType = gameState.Settings.GetSelectedSkin(tribe),
			handicap = handicap
		};
		gameState.PlayerStates.Add(playerState);
		Log.Verbose("Created player: {0}", new object[1] { playerState });
	}

	public static int GetLastPerformedCommandByPlayer(this GameState gameState, byte playerId)
	{
		if (gameState.CommandStack == null || gameState.CommandStack.Count == 0)
		{
			return 0;
		}
		for (int num = gameState.CommandStack.Count - 1; num >= 0; num--)
		{
			if (gameState.CommandStack[num].PlayerId == playerId)
			{
				return num;
			}
		}
		return 0;
	}

	public static void SetPlayerTribe(GameState gameState, int tribeType, List<int> disabledTribes, int skinType, Guid userId, Guid? gameOwner)
	{
		PlayerState playerState = null;
		foreach (PlayerState playerState2 in gameState.PlayerStates)
		{
			if (playerState2.AccountId == userId)
			{
				playerState = playerState2;
				break;
			}
		}
		if (playerState == null)
		{
			throw new ServerException(ErrorCode.PlayerNotFound, $"Failed to pick tribe. Could not find player {userId}");
		}
		if (playerState.hasChosenTribe)
		{
			throw new ServerException(ErrorCode.PickTribeFailed, $"User has already picked tribe: {userId}");
		}
		if (!gameState.GameLogicData.TryGetData((TribeData.Type)tribeType, out var data))
		{
			throw new ServerException(ErrorCode.PickTribeFailed, "Invalid tribe");
		}
		Guid value = userId;
		Guid? currentPlayerAccountId = GetCurrentPlayerAccountId(gameState);
		if (value != currentPlayerAccountId)
		{
			throw new ServerException(ErrorCode.PickTribeFailed, "Player picked out of order");
		}
		if (gameOwner.HasValue)
		{
			value = userId;
			currentPlayerAccountId = gameOwner;
			if (currentPlayerAccountId.HasValue && value == currentPlayerAccountId.GetValueOrDefault() && disabledTribes != null)
			{
				List<TribeData.Type> list = new List<TribeData.Type>(disabledTribes.Count);
				foreach (int disabledTribe in disabledTribes)
				{
					list.Add((TribeData.Type)disabledTribe);
				}
				gameState.Settings.disabledTribes = list;
			}
		}
		AssignTribe(gameState, playerState, data, (SkinType)skinType);
	}

	public static void AssignTribe(GameState gameState, PlayerState playerState, TribeData tribeData, SkinType skinType = SkinType.Default)
	{
		TribeData.Type type = tribeData?.type ?? GetRandomPickableTribe(gameState);
		if (playerState.Id == byte.MaxValue)
		{
			type = TribeData.Type.Nature;
		}
		else
		{
			playerState.Currency = 5;
		}
		playerState.tribe = type;
		playerState.hasChosenTribe = true;
		playerState.skinType = skinType;
		if (playerState.Id == gameState.CurrentPlayer)
		{
			StepNextPlayer(gameState);
		}
		Log.Verbose("Set tribe of player {0}(idx: {1}) to : {2}", new object[3] { playerState.Id, gameState.CurrentPlayerIndex, type });
		bool flag = false;
		List<int> list = new List<int>();
		foreach (PlayerState playerState3 in gameState.PlayerStates)
		{
			playerState.aggressions[playerState3.Id] = 0;
			list.Add(playerState3.GetPlayerColorInt(gameState));
			if (playerState3.AccountId != playerState.AccountId && playerState3.hasChosenTribe && playerState3.tribe == type)
			{
				flag = true;
			}
		}
		if (flag)
		{
			List<TribeData> allTribes = gameState.GameLogicData.GetAllTribes();
			List<int> list2 = new List<int>();
			foreach (TribeData item in allTribes)
			{
				if (!list.Contains(item.color))
				{
					list2.Add(item.color);
				}
			}
			if (list2.Count > 0)
			{
				playerState.colorOverride = list2[new Random().Next(0, list2.Count)];
			}
			else
			{
				playerState.colorOverride = (int)new Random().Value() * 16777215;
			}
		}
		PlayerState playerState2 = gameState.PlayerStates[gameState.CurrentPlayerIndex];
		Log.Verbose("Next player to pick: {0}", new object[1] { playerState2.UserName });
		if (playerState2.AutoPlay && !playerState2.hasChosenTribe)
		{
			AssignTribe(gameState, playerState2, null);
		}
	}

	public static void StepNextPlayer(GameState gameState)
	{
		gameState.CurrentPlayerIndex++;
		WrapCurrentPlayerIndexIgnoringNature(gameState);
	}

	public static void WrapCurrentPlayerIndexIgnoringNature(GameState gameState)
	{
		gameState.CurrentPlayerIndex = GetNextPlayerIndex(gameState);
	}

	private static byte GetNextPlayerIndex(GameState gameState, byte? currentPlayerIndex = null)
	{
		int num = NumPlayersExcludingNature(gameState);
		if (!currentPlayerIndex.HasValue)
		{
			currentPlayerIndex = gameState.CurrentPlayerIndex;
		}
		return Convert.ToByte((currentPlayerIndex + num) % num);
	}

	private static int NumPlayersExcludingNature(GameState gameState)
	{
		int num = ((gameState.PlayerStates != null) ? gameState.PlayerStates.Count : 0);
		if (num == 0)
		{
			return 0;
		}
		if (gameState.PlayerStates[gameState.PlayerStates.Count - 1].Id != byte.MaxValue)
		{
			return num;
		}
		return num - 1;
	}

	public static Guid? GetFirstHumanPlayerAccountId(GameState gameState)
	{
		return gameState.GetFirstHumanPlayer()?.AccountId;
	}

	public static Guid? GetCurrentPlayerAccountId(GameState gameState)
	{
		gameState.TryGetPlayer(gameState.CurrentPlayer, out var playerState);
		if (playerState.Id == byte.MaxValue || playerState.AutoPlay)
		{
			return null;
		}
		return playerState?.AccountId ?? ((Guid?)null);
	}

	public static bool IsReadyToStart(GameState gameState)
	{
		return gameState.PlayerStates.TrueForAll((PlayerState state) => state.hasChosenTribe);
	}

	public static void SortBotsLast(List<PlayerState> players)
	{
		players.Sort(delegate(PlayerState a, PlayerState b)
		{
			bool flag = !a.AccountId.HasValue || a.AccountId == Guid.Empty;
			bool value = !b.AccountId.HasValue || b.AccountId == Guid.Empty;
			return flag.CompareTo(value);
		});
	}

	public static int CountRealAlivePlayers(GameState gameState, Guid? excludePlayer = null)
	{
		int num = 0;
		foreach (PlayerState playerState in gameState.PlayerStates)
		{
			if (playerState.Id != byte.MaxValue && !playerState.AutoPlay && (!excludePlayer.HasValue || !(playerState.AccountId == excludePlayer)) && playerState.IsAlive(gameState))
			{
				num++;
			}
		}
		return num;
	}

	public static int CountAlivePlayers(GameState gameState)
	{
		int num = 0;
		foreach (PlayerState playerState in gameState.PlayerStates)
		{
			if (playerState.Id != byte.MaxValue && playerState.IsAlive(gameState))
			{
				num++;
			}
		}
		return num;
	}

	public static bool SecondLastPlayerResigned(GameState gameState)
	{
		if (CountRealAlivePlayers(gameState) > 1)
		{
			return false;
		}
		PlayerState playerState = null;
		PlayerState playerState2 = null;
		foreach (PlayerState playerState3 in gameState.PlayerStates)
		{
			if (playerState3.resignedAtCommandIndex > -1 && (playerState == null || playerState3.resignedAtCommandIndex > playerState.resignedAtCommandIndex))
			{
				playerState = playerState3;
			}
			if (playerState3.wipedAtCommand > -1 && (playerState2 == null || playerState3.wipedAtCommand > playerState2.wipedAtCommand))
			{
				playerState2 = playerState3;
			}
		}
		if (playerState == null)
		{
			return false;
		}
		if (playerState2 == null)
		{
			return true;
		}
		if (playerState.resignedAtCommandIndex > playerState2.wipedAtCommand)
		{
			return true;
		}
		return false;
	}

	public static void GenerateMap(GameState gameState)
	{
		gameState.Map = new MapData((ushort)gameState.Settings.MapSize, (ushort)gameState.Settings.MapSize);
		MapGeneratorSettings mapGeneratorSettings = gameState.Settings.GetMapGeneratorSettings();
		new MapGenerator().Generate(gameState, mapGeneratorSettings);
		foreach (PlayerState playerState in gameState.PlayerStates)
		{
			if (gameState.GameLogicData.TryGetData(playerState.tribe, out var data) && gameState.GameLogicData.TryGetData(data.startingUnit.type, out var data2))
			{
				TileData tile = gameState.Map.GetTile(playerState.startTile);
				UnitState unitState = ActionUtils.TrainUnitScored(gameState, playerState, tile, data2);
				unitState.attacked = false;
				unitState.moved = false;
			}
		}
	}

	public static CommandResult PerformCommandsWithAutoSelectCommandTrigger(GameState gameState, List<CommandBase> commands, out List<CommandBase> executedCommands, out List<CommandResultEvent> events)
	{
		events = new List<CommandResultEvent>();
		executedCommands = new List<CommandBase>();
		try
		{
			if (!CommandTriggerUtils.TryGetTriggerCommand(gameState, out var _))
			{
				return PerformCommands(gameState, commands, out executedCommands, out events);
			}
			int hashCode = gameState.GetHashCode();
			bool previouslyReadyToStart = IsReadyToStart(gameState);
			CommandBase command2;
			while (gameState.CurrentState != GameState.State.Ended && CommandTriggerUtils.TryGetTriggerCommand(gameState, out command2))
			{
				if (!ExecuteCommands(gameState, new List<CommandBase> { command2 }, out var executedCommands2, out var events2, out var error))
				{
					return CommandResult.UnsuccessfulResult(new ServerException(ErrorCode.InvalidUserCommand, $"Could not perform command in game with version {gameState.Version}, error {error}"));
				}
				events.AddRange(events2);
				executedCommands.AddRange(executedCommands2);
			}
			if (!ExecuteCommands(gameState, commands, out var executedCommands3, out var events3, out var error2))
			{
				return CommandResult.UnsuccessfulResult(new ServerException(ErrorCode.InvalidUserCommand, $"Could not perform command in game with version {gameState.Version}, error {error2}"));
			}
			events.AddRange(events3);
			executedCommands.AddRange(executedCommands3);
			return CreateCommandResult(gameState, hashCode, events, executedCommands, null, previouslyReadyToStart);
		}
		catch (Exception arg)
		{
			return CommandResult.UnsuccessfulResult(new ServerException(ErrorCode.InvalidUserCommand, $"Encountered unexpected exception in game with version {gameState.Version}, exception: {arg}"));
		}
	}

	public static CommandResult PerformCommands(GameState gameState, List<CommandBase> commands, out List<CommandBase> executedCommands, out List<CommandResultEvent> events)
	{
		events = new List<CommandResultEvent>();
		executedCommands = new List<CommandBase>();
		try
		{
			int hashCode = gameState.GetHashCode();
			string error;
			bool num = ExecuteCommands(gameState, commands, out executedCommands, out events, out error);
			bool previouslyReadyToStart = IsReadyToStart(gameState);
			if (!num)
			{
				return CommandResult.UnsuccessfulResult(new ServerException(ErrorCode.InvalidUserCommand, $"Could not perform command in game with version {gameState.Version}, error {error}"));
			}
			return CreateCommandResult(gameState, hashCode, events, executedCommands, null, previouslyReadyToStart);
		}
		catch (Exception arg)
		{
			return CommandResult.UnsuccessfulResult(new ServerException(ErrorCode.InvalidUserCommand, $"Encountered unexpected exception in game with version {gameState.Version}, exception: {arg}"));
		}
	}

	private static bool ExecuteCommands(GameState gameState, List<CommandBase> commands, out List<CommandBase> executedCommands, out List<CommandResultEvent> events, out string error)
	{
		executedCommands = new List<CommandBase>();
		events = new List<CommandResultEvent>();
		error = null;
		byte currentPlayer = gameState.CurrentPlayer;
		try
		{
			ActionManager actionManager = new ActionManager(gameState);
			foreach (CommandBase command2 in commands)
			{
				GameState.State currentState = gameState.CurrentState;
				uint currentTurn = gameState.CurrentTurn;
				if (actionManager.ExecuteCommand(command2, out error))
				{
					executedCommands.Add(command2);
					if (RegisterCommandResultEvent(gameState, currentState, currentTurn, command2, out var commandResultEvent))
					{
						events.Add(commandResultEvent);
					}
					continue;
				}
				return false;
			}
			PlayerState playerState;
			while (gameState.TryGetPlayer(gameState.CurrentPlayer, out playerState) && playerState.AutoPlay && gameState.CurrentState != GameState.State.Ended)
			{
				if (!CommandTriggerUtils.TryGetTriggerCommand(gameState, out var command))
				{
					command = AI.GetMove(gameState, playerState);
				}
				GameState.State currentState2 = gameState.CurrentState;
				uint currentTurn2 = gameState.CurrentTurn;
				if (actionManager.ExecuteCommand(command, out error))
				{
					executedCommands.Add(command);
					if (RegisterCommandResultEvent(gameState, currentState2, currentTurn2, command, out var commandResultEvent2, playerState.Id == currentPlayer))
					{
						events.Add(commandResultEvent2);
					}
					continue;
				}
				throw new ServerException(ErrorCode.AICommandFailed, $"AI Failed to perform command: {command.ToString()} with error {error})");
			}
		}
		catch (Exception ex)
		{
			error = ex.ToString();
			Console.WriteLine(ex);
			executedCommands = new List<CommandBase>();
			return false;
		}
		return true;
	}

	public static bool RegisterCommandResultEvent(GameState gameState, GameState.State previousSessionState, uint previousTurn, CommandBase command, out CommandResultEvent commandResultEvent, bool allowBotRunPlayerEvents = false)
	{
		commandResultEvent = CommandResultEvent.Unknown;
		if (previousSessionState != GameState.State.Ended && gameState.CurrentState == GameState.State.Ended)
		{
			commandResultEvent = CommandResultEvent.GameEnded;
			return true;
		}
		if (command.GetCommandType() == CommandType.EndTurn)
		{
			if (gameState.PlayerStates.Find((PlayerState player) => player.Id == command.PlayerId).AutoPlay && !allowBotRunPlayerEvents)
			{
				return false;
			}
			commandResultEvent = CommandResultEvent.PlayerTurnEnded;
			return true;
		}
		if (previousTurn != gameState.CurrentTurn)
		{
			commandResultEvent = CommandResultEvent.WholeTurnEnded;
			return true;
		}
		return false;
	}

	public static CommandResult CreateCommandResult(GameState gameState, int previousHash, List<CommandResultEvent> events, List<CommandBase> executedCommands = null, byte[] stateBeforeAi = null, bool previouslyReadyToStart = false)
	{
		byte[] array = SerializationHelpers.ToByteArray(gameState, gameState.Version);
		return new CommandResult
		{
			Success = true,
			PreviousGameStateHash = previousHash,
			GameStateHash = gameState.GetHashCode(),
			CurrentGameStateData = array,
			GameStateDataBeforeAi = (stateBeforeAi ?? array),
			ExecutedCommands = executedCommands?.Select((CommandBase x) => new PolytopiaCommandViewModel(CommandBase.ToByteArray(x, gameState.Version))).ToList(),
			CurrentUserId = GetCurrentPlayerAccountId(gameState),
			FirstHumanPlayerId = GetFirstHumanPlayerAccountId(gameState),
			CurrentTurnNumber = (int)gameState.CurrentTurn,
			PreviouslyReadyToStart = previouslyReadyToStart,
			ReadyToStart = IsReadyToStart(gameState),
			Events = events
		};
	}
}
