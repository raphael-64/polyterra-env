using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Polytopia.Data;
using Polytopia.IO;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;

public class HotseatClient : ClientBase
{
	private const int STARTING_CURRENCY = 5;

	private GameState lastTurnGameState;

	private ushort[] lastSeenCommands;

	private byte currentLocalPlayerIndex;

	private bool hasInitializedSaveData;

	public override string LOG_PREFIX => "<color=#e27a34>[HC]</color>";

	protected override GameState RecapGameState => initialGameState;

	public override void Reset()
	{
		lastTurnGameState = null;
		lastSeenCommands = null;
		hasInitializedSaveData = false;
		base.Reset();
	}

	public override void ResetGameState(StateUpdateReason reason)
	{
		_ = gameId;
		Log.Info("{0} Resetting GameState to latest, reason: {1}...", new object[2] { LOG_PREFIX, reason });
		string hotseatFilePath = Paths.GetHotseatFilePath(gameId.ToString());
		if (!DiskSerializationHelpers.FromDisk<HotseatGameData>(hotseatFilePath, out var result, out var _))
		{
			Log.Error("{0} Failed to reset state for {1}", new object[2] { LOG_PREFIX, hotseatFilePath });
			PopupManager.ShowErrorPopup(Localization.Get("misc.resetfailed"), GameManager.ReturnToMenu);
		}
		else
		{
			UpdateGameState(result.currentGameState, reason);
		}
	}

	public override void Connect(Uri endpoint)
	{
		Log.Verbose("{0} Connecting to {1}", new object[2]
		{
			LOG_PREFIX,
			endpoint.ToString()
		});
		OnConnected?.Invoke();
	}

	public override void Disconnect()
	{
		Log.Verbose("{0} Disconnected from local server", new object[1] { LOG_PREFIX });
		if (base.CurrentGameId.HasValue)
		{
			SaveSession(base.CurrentGameId.Value);
		}
		Reset();
		OnDisconnected?.Invoke();
	}

	public override Task<bool> OpenSession(Guid gameId)
	{
		Log.Verbose("{0} Opening session: {1}...", new object[2] { LOG_PREFIX, gameId });
		Reset();
		base.gameId = gameId;
		HotseatGameData result;
		int version;
		bool flag = DiskSerializationHelpers.FromDisk<HotseatGameData>(Paths.GetHotseatFilePath(gameId.ToString()), out result, out version);
		if (!VersionManager.IsGameVersionSupported(version))
		{
			PopupManager.ShowErrorPopup(Localization.GetErrorMessage(ErrorCode.UnsupportedOpenVersion));
			return Task.FromResult(result: false);
		}
		if (!flag)
		{
			PopupManager.ShowErrorPopup(Localization.GetErrorMessage(ErrorCode.GameStateDeserializationFailed));
			return Task.FromResult(result: false);
		}
		initialGameState = result.initialGameState;
		currentGameState = result.currentGameState;
		lastTurnGameState = result.lastTurnGameState;
		lastSeenCommands = result.lastSeenCommands;
		currentLocalPlayerIndex = currentGameState.CurrentPlayerIndex;
		hasInitializedSaveData = true;
		Log.Info("{0} Session opened, version: {1}", new object[2] { LOG_PREFIX, version });
		UpdateGameStateImmediate(currentGameState, StateUpdateReason.GameJoined);
		PrepareSession();
		return Task.FromResult(result: true);
	}

	public void SyncCurrentLocalPlayer()
	{
		bool flag = currentLocalPlayerIndex != GameState.CurrentPlayerIndex;
		currentLocalPlayerIndex = GameState.CurrentPlayerIndex;
		UIManager instance = UIManager.Instance;
		if (instance != null && instance.type == UIManager.Type.Ingame && (HotSeatOverlay.IsActive || flag))
		{
			PassPlayerReaction.ShowOverlay(GetCurrentLocalPlayer(), instant: true);
		}
	}

	public override Task<CreateSessionResult> CreateSession(GameSettings settings, List<PlayerState> players)
	{
		Reset();
		gameId = Guid.NewGuid();
		Log.Verbose("{0} Creating session with key: {1}", new object[2]
		{
			LOG_PREFIX,
			gameId.ToString()
		});
		GameState gameState = new GameState
		{
			Version = VersionManager.GameVersion,
			Settings = settings,
			PlayerStates = new List<PlayerState>()
		};
		for (int i = 0; i < settings.Players.Length; i++)
		{
			PlayerData playerData = settings.Players[i];
			if (playerData.type != PlayerData.Type.Bot)
			{
				PlayerState playerState = new PlayerState
				{
					Id = (byte)(i + 1),
					AccountId = playerData.profile.id,
					AutoPlay = (playerData.type == PlayerData.Type.Bot),
					UserName = playerData.GetName(),
					tribe = playerData.tribe,
					tribeMix = playerData.tribeMix,
					hasChosenTribe = true,
					skinType = playerData.skinType
				};
				gameState.PlayerStates.Add(playerState);
				Log.Verbose("Created player: {0}", new object[1] { playerState });
			}
			else
			{
				GameStateUtils.AddAIOpponent(gameState, GameStateUtils.GetRandomPickableTribe(gameState), GameSettings.HandicapFromDifficulty(playerData.botDifficulty), playerData.GetName());
			}
		}
		GameStateUtils.SetPlayerColors(gameState);
		GameStateUtils.AddNaturePlayer(gameState);
		Log.Verbose("{0} Creating world...", new object[1] { LOG_PREFIX });
		ushort num = (ushort)Math.Max(settings.MapSize, MapDataExtensions.GetMinimumMapSize(gameState.PlayerCount));
		gameState.Map = new MapData(num, num);
		MapGeneratorSettings mapGeneratorSettings = settings.GetMapGeneratorSettings();
		new MapGenerator().Generate(gameState, mapGeneratorSettings);
		Log.Verbose("{0} Creating initial state for {1} players...", new object[2] { LOG_PREFIX, gameState.PlayerCount });
		lastSeenCommands = new ushort[gameState.PlayerStates.Count];
		foreach (PlayerState playerState2 in gameState.PlayerStates)
		{
			foreach (PlayerState playerState3 in gameState.PlayerStates)
			{
				playerState2.aggressions[playerState3.Id] = 0;
			}
			if (playerState2.Id != byte.MaxValue)
			{
				playerState2.Currency = 5;
				if (gameState.GameLogicData.TryGetData(playerState2.tribe, out var data) && gameState.GameLogicData.TryGetData(data.startingUnit.type, out var data2))
				{
					TileData tile = gameState.Map.GetTile(playerState2.startTile);
					UnitState unitState = ActionUtils.TrainUnitScored(gameState, playerState2, tile, data2);
					unitState.attacked = false;
					unitState.moved = false;
				}
			}
		}
		byte[] data3 = SerializationHelpers.ToByteArray(gameState, gameState.Version);
		SerializationHelpers.FromByteArray<GameState>(data3, out initialGameState);
		SerializationHelpers.FromByteArray<GameState>(data3, out lastTurnGameState);
		currentLocalPlayerIndex = gameState.CurrentPlayerIndex;
		Log.Verbose("{0} Session created successfully", new object[1] { LOG_PREFIX });
		gameState.CommandStack.Add(new StartMatchCommand(1));
		UpdateGameStateImmediate(gameState, StateUpdateReason.GameCreated);
		hasInitializedSaveData = true;
		SaveSession(gameId);
		PrepareSession();
		AnalyticsHelpers.SendGameStartEvent(gameId, settings, GetCurrentLocalPlayer()?.tribe);
		return Task.FromResult(CreateSessionResult.Success);
	}

	public override void SaveSession(Guid gameId, bool showSaveErrorPopup = false)
	{
		if (base.ActionManager != null && hasInitializedSaveData && !isSessionEnded && base.ActionManager.LastSeenCommand >= GameState.LastProcessedCommand)
		{
			Log.Verbose("{0} Saving session: {1}...", new object[2] { LOG_PREFIX, gameId });
			if (!ActionManagerUtils.PerformAllQueuedActions(base.ActionManager.GameState))
			{
				Log.Info("Failed to bring state up to date, abort save", Array.Empty<object>());
			}
			else
			{
				SaveHotSeatGameDate(showSaveErrorPopup);
			}
		}
	}

	private void SaveHotSeatGameDate(bool showSaveErrorPopup)
	{
		string hotseatFilePath = Paths.GetHotseatFilePath(gameId.ToString());
		if (DiskSerializationHelpers.ToDisk(new HotseatGameData
		{
			initialGameState = initialGameState,
			currentGameState = currentGameState,
			lastTurnGameState = lastTurnGameState,
			lastSeenCommands = lastSeenCommands
		}, hotseatFilePath, currentGameState.Version, out var exception))
		{
			Log.Verbose("{0} Saved game data to: {1}", new object[2] { LOG_PREFIX, hotseatFilePath });
		}
		else if (DiskSerializationHelpers.IsDiskFullException(exception) && showSaveErrorPopup)
		{
			TriggerSaveToDiskFailedPopup(delegate
			{
				SaveHotSeatGameDate(showSaveErrorPopup);
			});
		}
	}

	public override Guid[] GetSessions(long playerId)
	{
		string saveDirectoryPath = Paths.GetSaveDirectoryPath("Hotseat");
		if (PolytopiaDirectory.Exists(saveDirectoryPath))
		{
			string[] files = PolytopiaDirectory.GetFiles(saveDirectoryPath, "*.state");
			if (files != null && files.Length != 0)
			{
				List<Guid> list = new List<Guid>(files.Length);
				for (int i = 0; i < files.Length; i++)
				{
					if (Guid.TryParse(Path.GetFileNameWithoutExtension(files[i]), out var result))
					{
						list.Add(result);
					}
				}
				return list.ToArray();
			}
		}
		return null;
	}

	protected override void PrepareSession()
	{
		Log.Verbose("{0} Session ready, position {1}/{2} (Hash: {3})", new object[4]
		{
			LOG_PREFIX,
			base.ActionManager?.LastSeenCommand,
			GameState.LastProcessedCommand,
			GameState.GetHashCode()
		});
		base.ActionManager.Pause();
		if (lastSeenCommands != null && lastSeenCommands.Length != 0)
		{
			_ = lastSeenCommands[currentLocalPlayerIndex];
		}
		SessionOpened();
	}

	public override void EndSession()
	{
		if (base.CurrentGameId.HasValue)
		{
			Log.Verbose("{0} Marking session as ended: {1}", new object[2]
			{
				LOG_PREFIX,
				base.CurrentGameId.Value
			});
			string hotseatFilePath = Paths.GetHotseatFilePath(base.CurrentGameId.Value.ToString());
			PolytopiaFileInfo polytopiaFileInfo = new PolytopiaFileInfo(hotseatFilePath);
			if (polytopiaFileInfo.Exists)
			{
				polytopiaFileInfo.Delete();
				polytopiaFileInfo.Refresh();
			}
			else
			{
				Log.Warning("Couldn't find pass and play game to delete at " + hotseatFilePath, Array.Empty<object>());
			}
			isSessionEnded = true;
		}
	}

	protected override void OnFinishedProcessing()
	{
		base.OnFinishedProcessing();
	}

	public override bool IsPlayerLocal(byte playerId)
	{
		if (base.ActionManager.GameState.TryGetPlayer(playerId, out var playerState) && playerState.AutoPlay)
		{
			return false;
		}
		return true;
	}

	public override PlayerState GetCurrentLocalPlayer()
	{
		if (GameState == null)
		{
			return null;
		}
		int num = currentLocalPlayerIndex;
		int num2 = GameState.PlayerStates.Count;
		int num3 = -1;
		int num4 = -1;
		while (num2 > 0)
		{
			PlayerState playerState = GameState.PlayerStates[num];
			if (!playerState.AutoPlay && playerState.IsAlive(GameState))
			{
				return playerState;
			}
			if (!playerState.AutoPlay && !playerState.IsAlive(GameState))
			{
				ushort lastSeenCommandForPlayerIndex = GetLastSeenCommandForPlayerIndex(num);
				if (lastSeenCommandForPlayerIndex >= num3)
				{
					num3 = lastSeenCommandForPlayerIndex;
					num4 = num;
				}
			}
			num++;
			if (num >= GameState.PlayerStates.Count)
			{
				num = 0;
			}
			num2--;
		}
		if (num4 >= 0)
		{
			return GameState.PlayerStates[num4];
		}
		return null;
	}

	public override Task<bool> PickTribe(TribeData.Type tribeType, List<TribeData.Type> disabledTribes = null, SkinType skinType = SkinType.Default)
	{
		throw new NotImplementedException();
	}

	public override Task SendCommand(CommandBase command)
	{
		ReceiveCommand(command);
		return Task.CompletedTask;
	}

	private Task<bool> ReceiveCommand(CommandBase command)
	{
		if (command.GetCommandType() == CommandType.EndTurn && command.IsValid(currentGameState))
		{
			currentLocalPlayerIndex = currentGameState.GetNextPlayerIndex(currentLocalPlayerIndex);
			Log.Verbose("Player {0} ended their turn, next player: {1}", new object[2] { currentGameState.CurrentPlayerIndex, currentLocalPlayerIndex });
		}
		if (base.ActionManager.ExecuteCommand(command, out var error))
		{
			return Task.FromResult(result: true);
		}
		Log.Error("Receive command failed with {0}", new object[1] { error });
		return Task.FromResult(result: false);
	}

	public override Task<bool> ReceiveCommand(List<CommandBase> commands)
	{
		return Task.FromResult(base.ActionManager.ExecuteCommands(commands));
	}

	public override ushort GetLastSeenCommand()
	{
		return GetLastSeenCommandForPlayerIndex(GameState.CurrentPlayerIndex);
	}

	public ushort GetLastSeenCommandForPlayerIndex(int playerIndex)
	{
		if (lastSeenCommands != null && lastSeenCommands.Length != 0)
		{
			return lastSeenCommands[playerIndex];
		}
		return 0;
	}

	public override void SetLastSeenCommand(ushort commandIndex)
	{
		Log.Verbose("Set last seen command for player {0} to: {1}", new object[2] { currentGameState.CurrentPlayer, commandIndex });
		if (lastSeenCommands != null && lastSeenCommands.Length != 0)
		{
			lastSeenCommands[GameState.CurrentPlayerIndex] = commandIndex;
		}
	}
}
