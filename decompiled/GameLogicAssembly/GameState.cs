using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polytopia.Data;
using PolytopiaBackendBase.Game;

public class GameState : IBinarySerializable
{
	public enum State : byte
	{
		Unknown,
		Lobby,
		Started,
		FinalTurn,
		Ended
	}

	public class RemovedPlayerSort : IComparer<PlayerState>
	{
		public int Compare(PlayerState a, PlayerState b)
		{
			int value = ((a.resignedAtCommandIndex >= 0) ? a.resignedAtCommandIndex : a.wipedAtCommand);
			return ((b.resignedAtCommandIndex >= 0) ? b.resignedAtCommandIndex : b.wipedAtCommand).CompareTo(value);
		}
	}

	public List<CommandTrigger> pendingCommandTriggers;

	private XXHash randomHash;

	public int Version { get; set; }

	public int Seed { get; set; }

	public uint CurrentTurn { get; set; }

	public byte CurrentPlayerIndex { get; set; }

	public uint CurrentUnitId { get; private set; }

	public State CurrentState { get; set; }

	public GameSettings Settings { get; set; }

	public MapData Map { get; set; }

	public List<PlayerState> PlayerStates { get; set; }

	public ushort LastProcessedCommand { get; set; }

	public List<CommandBase> CommandStack { get; set; }

	public List<ActionBase> ActionStack { get; set; }

	public XXHash RandomHash
	{
		get
		{
			if (randomHash == null)
			{
				randomHash = new XXHash(Seed);
			}
			return randomHash;
		}
	}

	public int PlayerCount => PlayerStates.Count - 1;

	public byte CurrentPlayer => PlayerStates[CurrentPlayerIndex].Id;

	public GameLogicData GameLogicData => PolytopiaDataManager.GetGameLogicData(VersionManager.GetGameLogicDataVersionFromGameVersion(Version));

	public bool WaitForCommand
	{
		get
		{
			if (ActionStack == null || ActionStack.Count == 0)
			{
				return false;
			}
			return ActionStack[ActionStack.Count - 1].HoldForCommand;
		}
	}

	public GameState()
	{
		CommandStack = new List<CommandBase>();
		ActionStack = new List<ActionBase>();
		Random random = new Random();
		Seed = random.Next(int.MinValue, int.MaxValue);
	}

	public void ClearWaitAction()
	{
		if (WaitForCommand)
		{
			Log.Info("Clear wait action: {0}", new object[1] { ActionStack[ActionStack.Count - 1].GetActionType() });
			ActionStack.RemoveAt(ActionStack.Count - 1);
		}
	}

	public bool TryGetPlayer(byte playerId, out PlayerState playerState)
	{
		playerState = null;
		for (int i = 0; i < PlayerStates.Count; i++)
		{
			if (PlayerStates[i].Id == playerId)
			{
				playerState = PlayerStates[i];
				return true;
			}
		}
		return false;
	}

	public int IndexOfPlayer(PlayerState playerState)
	{
		for (int i = 0; i < PlayerStates.Count; i++)
		{
			if (PlayerStates[i].Id == playerState.Id)
			{
				return i;
			}
		}
		return -1;
	}

	public bool TryGetPlayer(Guid accountId, out PlayerState playerState)
	{
		playerState = null;
		for (int i = 0; i < PlayerStates.Count; i++)
		{
			if (PlayerStates[i].AccountId == accountId)
			{
				playerState = PlayerStates[i];
				return true;
			}
		}
		return false;
	}

	public bool TryGetUnit(uint unitId, out UnitState unit)
	{
		unit = null;
		for (int i = 0; i < Map.Tiles.Length; i++)
		{
			TileData tileData = Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.id == unitId)
			{
				unit = tileData.unit;
				return true;
			}
		}
		return false;
	}

	public List<PlayerState> GetPlayersSortedByRankForMultiplayerResults()
	{
		return GetPlayersSortedByRankInternal(shouldIgnoreResigns: false);
	}

	public List<PlayerState> GetPlayersSortedByRank()
	{
		bool shouldIgnoreResigns = true;
		return GetPlayersSortedByRankInternal(shouldIgnoreResigns);
	}

	private List<PlayerState> GetPlayersSortedByRankInternal(bool shouldIgnoreResigns)
	{
		List<PlayerState> list = new List<PlayerState>(PlayerStates);
		if (Settings.RulesGameMode == GameMode.Might)
		{
			return GetPlayersSortedByCapitals(shouldIgnoreResigns);
		}
		if (Settings.RulesGameMode == GameMode.Domination)
		{
			return GetPlayersSortedByCities(shouldIgnoreResigns);
		}
		return GetPlayersSortedByScore(shouldIgnoreResigns);
	}

	private List<PlayerState> GetPlayersSortedByInternal(Func<List<PlayerState>, List<PlayerState>> remainingPlayersSortFunction, bool shouldIgnoreResigns)
	{
		List<PlayerState> list = new List<PlayerState>();
		List<PlayerState> list2 = new List<PlayerState>();
		PlayerState item = null;
		foreach (PlayerState playerState in PlayerStates)
		{
			if (playerState.Id == byte.MaxValue)
			{
				item = playerState;
			}
			else if ((shouldIgnoreResigns || playerState.resignedAtCommandIndex == -1) && playerState.wipedAtCommand == -1)
			{
				list.Add(playerState);
			}
			else
			{
				list2.Add(playerState);
			}
		}
		list = remainingPlayersSortFunction(list);
		list2.Sort(new RemovedPlayerSort());
		List<PlayerState> list3 = list;
		list3.AddRange(list2);
		list3.Add(item);
		return list3;
	}

	private List<PlayerState> GetPlayersSortedByCapitals(bool shouldIgnoreResigns)
	{
		return GetPlayersSortedByInternal((List<PlayerState> remainingPlayers) => (from player in remainingPlayers
			orderby player.CountCapitals(this) descending, player.GetScore() descending
			select player).ToList(), shouldIgnoreResigns);
	}

	private List<PlayerState> GetPlayersSortedByCities(bool shouldIgnoreResigns)
	{
		return GetPlayersSortedByInternal((List<PlayerState> remainingPlayers) => (from player in remainingPlayers
			orderby player.CountCities(this) descending, player.GetScore() descending
			select player).ToList(), shouldIgnoreResigns);
	}

	private List<PlayerState> GetPlayersSortedByScore(bool shouldIgnoreResigns)
	{
		return GetPlayersSortedByInternal((List<PlayerState> remainingPlayers) => remainingPlayers.OrderByDescending((PlayerState player) => player.GetScore()).ToList(), shouldIgnoreResigns);
	}

	public int GetLivingHumanCount()
	{
		int num = 0;
		for (int i = 0; i < PlayerStates.Count; i++)
		{
			PlayerState playerState = PlayerStates[i];
			if (playerState.IsAlive(this, Settings.rules.PlayerDeathCondition) && !playerState.AutoPlay)
			{
				num++;
			}
		}
		return num;
	}

	public bool TryGetWinner(out PlayerState winner)
	{
		if (Version < 70)
		{
			return TryGetWinnerV69(out winner);
		}
		if (Version < 91)
		{
			return TryGetWinnerV90(out winner);
		}
		return TryGetWinnerDefault(out winner);
	}

	private bool TryGetWinnerDefault(out PlayerState winner)
	{
		List<PlayerState> playersSortedByRank = GetPlayersSortedByRank();
		winner = playersSortedByRank[0];
		int num = GameStateUtils.CountRealAlivePlayers(this);
		if (num <= 1 && (Settings.GameType == GameType.Competitive || Settings.GameType == GameType.Multiplayer))
		{
			return true;
		}
		if (num == 0)
		{
			return true;
		}
		if (Settings.rules.WinByExtermination && GameStateUtils.CountAlivePlayers(this) == 1)
		{
			return true;
		}
		if (Settings.rules.TurnLimit > 0)
		{
			return CurrentTurn >= Settings.rules.TurnLimit;
		}
		if (Settings.rules.WinByCapital && winner.CountCapitals(this) == PlayerCount)
		{
			return true;
		}
		if (Settings.rules.ScoreLimit > 0 && winner.score >= Settings.rules.ScoreLimit)
		{
			return true;
		}
		return false;
	}

	private bool TryGetWinnerV90(out PlayerState winner)
	{
		List<PlayerState> playersSortedByRank = GetPlayersSortedByRank();
		winner = playersSortedByRank[0];
		int num = GameStateUtils.CountRealAlivePlayers(this);
		if (num <= 1 && (Settings.GameType == GameType.Competitive || Settings.GameType == GameType.Multiplayer) && Settings.BaseGameMode != GameMode.Glory)
		{
			return true;
		}
		if (num == 0)
		{
			return true;
		}
		if (Settings.rules.WinByExtermination && GameStateUtils.CountAlivePlayers(this) == 1)
		{
			return true;
		}
		if (Settings.rules.TurnLimit > 0)
		{
			return CurrentTurn >= Settings.rules.TurnLimit;
		}
		if (Settings.rules.WinByCapital && winner.CountCapitals(this) == PlayerCount)
		{
			return true;
		}
		if (Settings.rules.ScoreLimit > 0 && winner.score >= Settings.rules.ScoreLimit)
		{
			return true;
		}
		return false;
	}

	private bool TryGetWinnerV69(out PlayerState winner)
	{
		int num = 0;
		int num2 = 0;
		uint num3 = 0u;
		PlayerState playerState = null;
		PlayerState playerState2 = null;
		for (int i = 0; i < PlayerStates.Count; i++)
		{
			PlayerState playerState3 = PlayerStates[i];
			if (playerState3.IsAlive(this, Settings.rules.PlayerDeathCondition))
			{
				if (playerState3.Id != byte.MaxValue)
				{
					num++;
				}
				if (!playerState3.AutoPlay)
				{
					num2++;
				}
				uint score = playerState3.GetScore();
				if (score > num3)
				{
					num3 = score;
					playerState2 = playerState3;
				}
			}
			if (playerState3.Id == CurrentPlayer)
			{
				playerState = playerState3;
			}
		}
		winner = playerState;
		if (num2 == 0)
		{
			return true;
		}
		if (Settings.rules.WinByExtermination && num == 1)
		{
			return true;
		}
		if (Settings.rules.TurnLimit > 0)
		{
			winner = playerState2;
			return CurrentTurn >= Settings.rules.TurnLimit;
		}
		if (Settings.rules.WinByCapital && playerState.CountCapitals(this) == PlayerCount)
		{
			return true;
		}
		if (Settings.rules.ScoreLimit > 0 && playerState.score >= Settings.rules.ScoreLimit)
		{
			return true;
		}
		return false;
	}

	public void EndPlayerTurn(bool newTurn = false)
	{
		CurrentPlayerIndex++;
		if (CurrentPlayerIndex >= PlayerStates.Count)
		{
			CurrentPlayerIndex = 0;
			newTurn = true;
		}
		if (!PlayerStates[CurrentPlayerIndex].IsAlive(this))
		{
			EndPlayerTurn(newTurn);
		}
		else if (newTurn)
		{
			CurrentTurn++;
		}
	}

	public bool TryGetNextHumanPlayerIndex(byte currentPlayerIndex, out byte nextPlayerIndex, int iterations = -1)
	{
		nextPlayerIndex = 0;
		switch (iterations)
		{
		case 0:
			return false;
		case -1:
			iterations = PlayerStates.Count;
			break;
		}
		if (currentPlayerIndex >= PlayerStates.Count)
		{
			currentPlayerIndex = 0;
		}
		Log.Verbose("- checking player: {0} (Id: {1})", new object[2]
		{
			currentPlayerIndex,
			PlayerStates[currentPlayerIndex].Id
		});
		if (PlayerStates[currentPlayerIndex].AutoPlay || !PlayerStates[currentPlayerIndex].IsAlive(this))
		{
			return TryGetNextHumanPlayerIndex(++currentPlayerIndex, out nextPlayerIndex, --iterations);
		}
		nextPlayerIndex = currentPlayerIndex;
		return true;
	}

	public byte GetNextPlayerIndex(byte currentPlayer)
	{
		byte b = ++currentPlayer;
		if (b >= PlayerStates.Count)
		{
			b = 0;
		}
		if (!PlayerStates[b].IsAlive(this))
		{
			return GetNextPlayerIndex(b);
		}
		return b;
	}

	public PlayerState GetFirstHumanPlayer()
	{
		foreach (PlayerState playerState in PlayerStates)
		{
			if (!playerState.AutoPlay)
			{
				return playerState;
			}
		}
		return null;
	}

	public uint GetNextUnitId()
	{
		if (Version < 40)
		{
			return CurrentUnitId++;
		}
		return ++CurrentUnitId;
	}

	public bool TryGetTask(PlayerState playerState, TaskData.Type type, out TaskBase task)
	{
		if (playerState.tasks != null && playerState.tasks.Count > 0)
		{
			for (int i = 0; i < playerState.tasks.Count; i++)
			{
				if (playerState.tasks[i].GetTaskType() == type)
				{
					task = playerState.tasks[i];
					return true;
				}
			}
		}
		task = null;
		return false;
	}

	public void CheckTasks(PlayerState playerState)
	{
		if (playerState.tasks == null || playerState.tasks.Count == 0)
		{
			return;
		}
		for (int i = 0; i < playerState.tasks.Count; i++)
		{
			if (playerState.tasks[i].IsTaskCompleted(this, playerState))
			{
				ActionStack.Add(new TaskCompletedAction(playerState.Id, playerState.tasks[i].GetTaskType()));
			}
		}
	}

	public void CheckTask(PlayerState playerState, TaskData.Type type)
	{
		if (playerState.tasks == null || playerState.tasks.Count == 0)
		{
			return;
		}
		for (int i = 0; i < playerState.tasks.Count; i++)
		{
			if (playerState.tasks[i].GetTaskType() == type)
			{
				CheckTask(playerState, playerState.tasks[i]);
			}
		}
	}

	public void CheckTask(PlayerState playerState, TaskBase task)
	{
		if (task != null && task.IsTaskCompleted(this, playerState))
		{
			ActionStack.Add(new TaskCompletedAction(playerState.Id, task.GetTaskType()));
		}
	}

	public void AddPendingCommandTrigger(CommandTrigger commandTrigger)
	{
		if (pendingCommandTriggers == null)
		{
			pendingCommandTriggers = new List<CommandTrigger>();
		}
		Log.Verbose("Adding command trigger: {0} for player {1} @ {2}", new object[3] { commandTrigger.type, commandTrigger.playerId, commandTrigger.coordinates });
		pendingCommandTriggers.Add(commandTrigger);
	}

	public void PopPendingCommandTrigger(byte playerId, CommandTriggerType triggerType)
	{
		if (pendingCommandTriggers != null && pendingCommandTriggers.Count > 0)
		{
			for (int num = pendingCommandTriggers.Count - 1; num >= 0; num--)
			{
				CommandTrigger commandTrigger = pendingCommandTriggers[num];
				if (commandTrigger.playerId == playerId && commandTrigger.type == triggerType)
				{
					Log.Verbose("Popping command trigger: {0} for player {1} @ {2}", new object[3] { commandTrigger.type, commandTrigger.playerId, commandTrigger.coordinates });
					pendingCommandTriggers.RemoveAt(num);
					return;
				}
			}
		}
		throw new Exception("Failed to pop command trigger for player " + playerId + " with type " + triggerType);
	}

	public List<CommandTrigger> PopAllPendingCommandTriggers()
	{
		List<CommandTrigger> result = pendingCommandTriggers;
		pendingCommandTriggers = new List<CommandTrigger>();
		return result;
	}

	public bool TryGetPendingCommandTrigger(byte playerId, out CommandTrigger trigger)
	{
		if (pendingCommandTriggers != null && pendingCommandTriggers.Count > 0)
		{
			for (int num = pendingCommandTriggers.Count - 1; num >= 0; num--)
			{
				if (pendingCommandTriggers[num].playerId == playerId)
				{
					trigger = pendingCommandTriggers[num];
					return true;
				}
			}
		}
		trigger = default(CommandTrigger);
		return false;
	}

	public byte[] GetCommandStack()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
		{
			binaryWriter.Write(Version);
			binaryWriter.Write(CommandStack.Count);
			for (int i = 0; i < CommandStack.Count; i++)
			{
				SerializeCommand(CommandStack[i], binaryWriter, Version);
			}
		}
		return memoryStream.ToArray();
	}

	public void LoadCommandStack(byte[] stackData)
	{
		using (MemoryStream input = new MemoryStream(stackData))
		{
			using BinaryReader binaryReader = new BinaryReader(input);
			int version = binaryReader.ReadInt32();
			int num = binaryReader.ReadInt32();
			CommandStack = new List<CommandBase>(num);
			for (int i = 0; i < num; i++)
			{
				CommandBase item = DeserializeCommand(binaryReader, version);
				CommandStack.Add(item);
			}
		}
		LastProcessedCommand = (ushort)CommandStack.Count;
	}

	public void DebugPrintActions()
	{
		Log.Verbose("actions in stack {0}", new object[1] { ActionStack.Count });
		foreach (ActionBase item in ActionStack)
		{
			Log.Verbose("action {0}", new object[1] { item.ToString() });
		}
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		writer.Write(Version);
		writer.Write(LastProcessedCommand);
		writer.Write(CurrentTurn);
		writer.Write(CurrentPlayerIndex);
		writer.Write(CurrentUnitId);
		writer.Write((byte)CurrentState);
		writer.Write(Seed);
		Settings.Serialize(writer, version);
		Map.Serialize(writer, version);
		writer.Write((ushort)((PlayerStates != null) ? ((uint)PlayerStates.Count) : 0u));
		if (PlayerStates != null)
		{
			for (int i = 0; i < PlayerStates.Count; i++)
			{
				PlayerStates[i].Serialize(writer, version);
			}
		}
		writer.Write((ushort)((pendingCommandTriggers != null) ? ((uint)pendingCommandTriggers.Count) : 0u));
		if (pendingCommandTriggers != null)
		{
			for (int j = 0; j < pendingCommandTriggers.Count; j++)
			{
				pendingCommandTriggers[j].Serialize(writer, version);
			}
		}
		ushort num = (ushort)((CommandStack != null) ? ((uint)CommandStack.Count) : 0u);
		writer.Write(num);
		for (int k = 0; k < num; k++)
		{
			SerializeCommand(CommandStack[k], writer, version);
		}
		ushort num2 = (ushort)((ActionStack != null) ? ((uint)ActionStack.Count) : 0u);
		writer.Write(num2);
		for (int l = 0; l < num2; l++)
		{
			SerializeAction(ActionStack[l], writer, version);
		}
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		Version = reader.ReadInt32();
		LastProcessedCommand = reader.ReadUInt16();
		CurrentTurn = reader.ReadUInt32();
		CurrentPlayerIndex = reader.ReadByte();
		CurrentUnitId = reader.ReadUInt32();
		CurrentState = (State)reader.ReadByte();
		Seed = reader.ReadInt32();
		Settings = new GameSettings();
		Settings.Deserialize(reader, version);
		if (Map != null)
		{
			Map.Deserialize(reader, version);
		}
		else
		{
			Map = new MapData();
			Map.Deserialize(reader, version);
		}
		ushort num = reader.ReadUInt16();
		if (PlayerStates == null || PlayerStates.Count < num)
		{
			PlayerStates = new List<PlayerState>(num);
		}
		for (int i = 0; i < num; i++)
		{
			if (i < PlayerStates.Count)
			{
				PlayerStates[i].Deserialize(reader, version);
				continue;
			}
			PlayerState playerState = new PlayerState();
			playerState.Deserialize(reader, version);
			PlayerStates.Add(playerState);
		}
		int num2 = reader.ReadUInt16();
		if (pendingCommandTriggers == null)
		{
			pendingCommandTriggers = new List<CommandTrigger>(num2);
		}
		else
		{
			pendingCommandTriggers.Clear();
		}
		for (int j = 0; j < num2; j++)
		{
			CommandTrigger item = default(CommandTrigger);
			item.Deserialize(reader, version);
			pendingCommandTriggers.Add(item);
		}
		ushort num3 = reader.ReadUInt16();
		if (CommandStack == null)
		{
			CommandStack = new List<CommandBase>(num3);
		}
		else
		{
			CommandStack.Clear();
		}
		for (int k = 0; k < num3; k++)
		{
			CommandStack.Add(DeserializeCommand(reader, version));
		}
		ushort num4 = reader.ReadUInt16();
		if (ActionStack == null)
		{
			ActionStack = new List<ActionBase>(num4);
		}
		else
		{
			ActionStack.Clear();
		}
		for (int l = 0; l < num4; l++)
		{
			ActionStack.Add(DeserializeAction(reader, version));
		}
	}

	public static CommandBase GetCommand(CommandType type)
	{
		return type switch
		{
			CommandType.Build => new BuildCommand(), 
			CommandType.Attack => new AttackCommand(), 
			CommandType.Recover => new RecoverCommand(), 
			CommandType.HealOthers => new HealOthersCommand(), 
			CommandType.Train => new TrainCommand(), 
			CommandType.Move => new MoveCommand(), 
			CommandType.Capture => new CaptureCommand(), 
			CommandType.Research => new ResearchCommand(), 
			CommandType.Destroy => new DestroyCommand(), 
			CommandType.Disband => new DisbandCommand(), 
			CommandType.CityReward => new CityRewardCommand(), 
			CommandType.Promote => new PromoteCommand(), 
			CommandType.ExamineRuins => new ExamineRuinsCommand(), 
			CommandType.EndTurn => new EndTurnCommand(), 
			CommandType.Upgrade => new UpgradeCommand(), 
			CommandType.FreezeArea => new FreezeAreaCommand(), 
			CommandType.BreakIce => new BreakIceCommand(), 
			CommandType.StartMatch => new StartMatchCommand(), 
			CommandType.Stay => new StayCommand(), 
			CommandType.EndMatch => new EndMatchCommand(), 
			CommandType.Harvest => new HarvestCommand(), 
			CommandType.Explode => new ExplodeCommand(), 
			CommandType.Boost => new BoostCommand(), 
			CommandType.Decompose => new DecomposeCommand(), 
			CommandType.PeaceTreaty => new PeaceTreatyCommand(), 
			CommandType.PeaceRequestResponse => new PeaceRequestResponseCommand(), 
			CommandType.BreakPeace => new BreakPeaceCommand(), 
			CommandType.EstablishEmbassy => new EstablishEmbassyCommand(), 
			CommandType.Clone => new CloneCommand(), 
			CommandType.UpgradeEmbassy => new UpgradeEmbassyCommand(), 
			CommandType.Hide => new HideCommand(), 
			CommandType.InfiltrateReward => new InfiltrateRewardCommand(), 
			CommandType.Resign => new ResignCommand(), 
			_ => throw new Exception("Command type " + type.ToString() + " is not implemented"), 
		};
	}

	public static ActionBase GetAction(ActionType type)
	{
		return type switch
		{
			ActionType.Build => new BuildAction(), 
			ActionType.Attack => new AttackAction(), 
			ActionType.Recover => new RecoverAction(), 
			ActionType.HealOthers => new HealOthersAction(), 
			ActionType.Train => new TrainAction(), 
			ActionType.Move => new MoveAction(), 
			ActionType.RuleArea => new RuleAreaAction(), 
			ActionType.Research => new ResearchAction(), 
			ActionType.DestroyImprovement => new DestroyImprovementAction(), 
			ActionType.DisbandUnit => new DisbandUnitAction(), 
			ActionType.CityReward => new CityRewardAction(), 
			ActionType.Meet => new MeetAction(), 
			ActionType.Promote => new PromoteAction(), 
			ActionType.ExamineRuins => new ExamineRuinsAction(), 
			ActionType.EndTurn => new EndTurnAction(), 
			ActionType.Upgrade => new UpgradeAction(), 
			ActionType.FreezeArea => new FreezeAreaAction(), 
			ActionType.BreakIce => new BreakIceAction(), 
			ActionType.BuildRoad => new BuildRoadAction(), 
			ActionType.CaptureCity => new CaptureCityAction(), 
			ActionType.CityLevelUp => new CityLevelUpAction(), 
			ActionType.UpdateRoutes => new UpdateRoutesAction(), 
			ActionType.KillUnit => new KillUnitAction(), 
			ActionType.ModifyProduction => new ModifyProductionAction(), 
			ActionType.Explore => new ExploreAction(), 
			ActionType.IncreasePopulation => new IncreasePopulationAction(), 
			ActionType.IncreaseScore => new IncreaseScoreAction(), 
			ActionType.IncreaseCurrency => new IncreaseCurrencyAction(), 
			ActionType.StartTurn => new StartTurnAction(), 
			ActionType.ScoutMove => new ScoutMoveAction(), 
			ActionType.UpdateTransportConnections => new UpdateTransportConnectionAction(), 
			ActionType.DecreasePopulation => new DecreasePopulationAction(), 
			ActionType.CityRewardPopup => new CityRewardPopupAction(), 
			ActionType.Embark => new EmbarkAction(), 
			ActionType.Disembark => new DisembarkAction(), 
			ActionType.GameOver => new GameOverAction(), 
			ActionType.Heal => new HealAction(), 
			ActionType.StartMatch => new StartMatchAction(), 
			ActionType.ImprovementLevelUp => new ImprovementLevelUpAction(), 
			ActionType.ImprovementLevelDown => new ImprovementLevelDownAction(), 
			ActionType.Convert => new ConvertAction(), 
			ActionType.FreezeUnit => new FreezeUnitAction(), 
			ActionType.EnableTask => new EnableTaskAction(), 
			ActionType.TaskCompleted => new TaskCompletedAction(), 
			ActionType.FreezeTile => new FreezeTileAction(), 
			ActionType.ClimateChange => new ClimateChangeAction(), 
			ActionType.Reselect => new ReselectAction(), 
			ActionType.WipePlayer => new WipePlayerAction(), 
			ActionType.CreateResource => new CreateResourceAction(), 
			ActionType.DestroyResource => new DestroyResourceAction(), 
			ActionType.ModifyScore => new ModifyScoreAction(), 
			ActionType.PassPlayer => new PassPlayerAction(), 
			ActionType.ConnectCity => new ConnectCityAction(), 
			ActionType.DisconnectCity => new DisconnectCityAction(), 
			ActionType.ChangeCityConnection => new ChangeCityConnectionAction(), 
			ActionType.BreakIceArea => new BreakIceAreaAction(), 
			ActionType.ExpandCity => new ExpandCityAction(), 
			ActionType.DecreaseScore => new DecreaseScoreAction(), 
			ActionType.Explode => new ExplodeUnitAction(), 
			ActionType.EndMatch => new EndMatchAction(), 
			ActionType.WipePlayerEnd => new WipePlayerEndAction(), 
			ActionType.EndCommand => new EndCommandAction(), 
			ActionType.Boost => new BoostAction(), 
			ActionType.BoostOthers => new BoostOthersAction(), 
			ActionType.Eat => new EatAction(), 
			ActionType.Poison => new PoisonUnitAction(), 
			ActionType.HarvestImprovement => new HarvestImprovementAction(), 
			ActionType.Decompose => new DecomposeAction(), 
			ActionType.ReceiveDiplomacyMessage => new ReceiveDiplomacyMessageAction(), 
			ActionType.PeaceRequestResponse => new PeaceRequestResponseAction(), 
			ActionType.PeaceTreaty => new PeaceTreatyAction(), 
			ActionType.BreakPeace => new BreakPeaceAction(), 
			ActionType.EstablishEmbassy => new EstablishEmbassyAction(), 
			ActionType.RevealCapital => new RevealCapitalAction(), 
			ActionType.UpgradeEmbassy => new UpgradeEmbassyAction(), 
			ActionType.DestroyEmbassy => new DestroyEmbassyAction(), 
			ActionType.Hide => new HideAction(), 
			ActionType.Reveal => new RevealAction(), 
			ActionType.Infiltrate => new InfiltrateAction(), 
			ActionType.InfiltrationReward => new InfiltrationRewardAction(), 
			ActionType.Resign => new ResignAction(), 
			_ => throw new Exception("Action type " + type.ToString() + " is not implemented"), 
		};
	}

	public static void SerializeCommand(CommandBase command, BinaryWriter writer, int version)
	{
		writer.Write((ushort)command.GetCommandType());
		command.Serialize(writer, version);
	}

	public static CommandBase DeserializeCommand(BinaryReader reader, int version)
	{
		CommandBase command = GetCommand((CommandType)reader.ReadUInt16());
		command.Deserialize(reader, version);
		return command;
	}

	public static void SerializeAction(ActionBase action, BinaryWriter writer, int version)
	{
		writer.Write((ushort)action.GetActionType());
		action.Serialize(writer, version);
	}

	public static ActionBase DeserializeAction(BinaryReader reader, int version)
	{
		ActionBase action = GetAction((ActionType)reader.ReadUInt16());
		action.Deserialize(reader, version);
		return action;
	}
}
