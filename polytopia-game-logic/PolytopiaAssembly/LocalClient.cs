using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Polytopia.Data;
using Polytopia.IO;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;

public class LocalClient : ClientBase
{
	private const int LOCAL_PLAYER_ID = 1;

	private const int STARTING_CURRENCY = 5;

	private const int STARTING_TUTORIAL_CURRENCY = 0;

	private ushort lastSeenCommand;

	private bool hasInitializedSaveData;

	private bool isUploadingHighscore;

	private bool hasUploadedHighScore;

	public override string LOG_PREFIX => "<color=#63d863>[LC]</color>";

	public override void Reset()
	{
		hasInitializedSaveData = false;
		lastSeenCommand = 0;
		isUploadingHighscore = false;
		hasUploadedHighScore = false;
		base.Reset();
	}

	public override void ResetGameState(StateUpdateReason reason)
	{
		_ = gameId;
		Log.Info("{0} Resetting GameState to latest, reason: {1}...", new object[2] { LOG_PREFIX, reason });
		string singleplayerFilePath = Paths.GetSingleplayerFilePath(gameId.ToString());
		if (!DiskSerializationHelpers.FromDisk<LocalGameData>(singleplayerFilePath, out var result, out var _))
		{
			Log.Error("{0} Failed to reset state for {1}", new object[2] { LOG_PREFIX, singleplayerFilePath });
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
		LocalGameData result;
		int version;
		bool flag = DiskSerializationHelpers.FromDisk<LocalGameData>(Paths.GetSingleplayerFilePath(gameId.ToString()), out result, out version);
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
		lastSeenCommand = result.lastSeenCommand;
		hasInitializedSaveData = true;
		Log.Info("{0} Session opened, version: {1}", new object[2] { LOG_PREFIX, version });
		UpdateGameStateImmediate(currentGameState, StateUpdateReason.GameJoined);
		PrepareSession();
		return Task.FromResult(result: true);
	}

	public override Task<CreateSessionResult> CreateSession(GameSettings settings, List<PlayerState> players)
	{
		ClearPreviousSession();
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
					AccountId = Guid.Empty,
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
		foreach (PlayerState playerState2 in gameState.PlayerStates)
		{
			foreach (PlayerState playerState3 in gameState.PlayerStates)
			{
				playerState2.aggressions[playerState3.Id] = 0;
			}
			if (playerState2.Id != byte.MaxValue)
			{
				playerState2.Currency = 5;
				if (gameState.Settings.BaseGameMode == GameMode.Tutorial)
				{
					playerState2.Currency = 0;
				}
				if (gameState.Settings.BaseGameMode != GameMode.Tutorial && gameState.GameLogicData.TryGetData(playerState2.tribe, out var data) && gameState.GameLogicData.TryGetData(data.startingUnit.type, out var data2))
				{
					TileData tile = gameState.Map.GetTile(playerState2.startTile);
					UnitState unitState = ActionUtils.TrainUnitScored(gameState, playerState2, tile, data2);
					unitState.attacked = false;
					unitState.moved = false;
				}
			}
		}
		SerializationHelpers.FromByteArray<GameState>(SerializationHelpers.ToByteArray(gameState, gameState.Version), out initialGameState);
		Log.Verbose("{0} Session created successfully", new object[1] { LOG_PREFIX });
		gameState.CommandStack.Add(new StartMatchCommand(1));
		hasInitializedSaveData = true;
		UpdateGameStateImmediate(gameState, StateUpdateReason.GameCreated);
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
				SaveLocalGameData(showSaveErrorPopup);
			}
		}
	}

	private void SaveLocalGameData(bool showSaveErrorPopup)
	{
		string singleplayerFilePath = Paths.GetSingleplayerFilePath(gameId.ToString());
		if (DiskSerializationHelpers.ToDisk(new LocalGameData
		{
			initialGameState = initialGameState,
			currentGameState = currentGameState,
			lastSeenCommand = base.ActionManager.LastSeenCommand
		}, singleplayerFilePath, currentGameState.Version, out var exception))
		{
			Log.Verbose("{0} Saved game data to: {1}", new object[2] { LOG_PREFIX, singleplayerFilePath });
		}
		else if (DiskSerializationHelpers.IsDiskFullException(exception) && showSaveErrorPopup)
		{
			TriggerSaveToDiskFailedPopup(delegate
			{
				SaveLocalGameData(showSaveErrorPopup);
			});
		}
	}

	public override Guid[] GetSessions(long playerId)
	{
		string saveDirectoryPath = Paths.GetSaveDirectoryPath("Singleplayer");
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

	private void ClearPreviousSession()
	{
		Log.Verbose("{0} Clearing previous session...", new object[1] { LOG_PREFIX });
		string saveDirectoryPath = Paths.GetSaveDirectoryPath("Singleplayer");
		if (!PolytopiaDirectory.Exists(saveDirectoryPath))
		{
			return;
		}
		string[] files = PolytopiaDirectory.GetFiles(saveDirectoryPath, "*.state");
		if (files != null && files.Length != 0)
		{
			for (int i = 0; i < files.Length; i++)
			{
				PolytopiaFile.Delete(files[i]);
			}
		}
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
		SessionOpened();
	}

	public override void EndSession()
	{
		if (!base.CurrentGameId.HasValue)
		{
			return;
		}
		Log.Verbose("{0} Marking session as ended: {1}", new object[2]
		{
			LOG_PREFIX,
			base.CurrentGameId.Value
		});
		string singleplayerFilePath = Paths.GetSingleplayerFilePath(base.CurrentGameId.Value.ToString());
		if (PolytopiaFile.Exists(singleplayerFilePath))
		{
			string replayFilePath = Paths.GetReplayFilePath(base.CurrentGameId.Value.ToString());
			if (PolytopiaFile.Exists(replayFilePath))
			{
				PolytopiaFile.Delete(replayFilePath);
			}
			PolytopiaFile.Move(singleplayerFilePath, replayFilePath);
		}
		isSessionEnded = true;
	}

	public override bool IsPlayerLocal(byte playerId)
	{
		return playerId == 1;
	}

	public override PlayerState GetCurrentLocalPlayer()
	{
		if (GameState != null && GameState.TryGetPlayer(1, out var playerState))
		{
			return playerState;
		}
		return null;
	}

	public override Task<bool> PickTribe(TribeData.Type tribeType, List<TribeData.Type> disabledTribes = null, SkinType skinType = SkinType.Default)
	{
		throw new NotImplementedException();
	}

	public override async Task UploadHighscore()
	{
		if (currentGameState == null || !GameModeUtils.HighscoreEnabled(currentGameState.Settings.BaseGameMode) || isUploadingHighscore || hasUploadedHighScore)
		{
			return;
		}
		Log.Verbose("Uploading highscore...", Array.Empty<object>());
		isUploadingHighscore = true;
		try
		{
			NetworkUtils.ShowLoader(Localization.Get("highscore.uploading"));
			if (!PolytopiaBackendAdapter.Instance.IsAuthenticated)
			{
				await GameManager.GetLoginManager().LoginAsync();
			}
			if (!GameManager.GetVersioningInfoHolder().IsHighscoreEnabled(out var message))
			{
				NetworkUtils.ShowLoaderError(Localization.Get("highscore.uploading.failed"));
				PopupManager.ShowErrorPopup(message);
				return;
			}
			ServerResponse<ResponseViewModel> serverResponse = ((!PolytopiaBackendAdapter.Instance.IsConnected) ? (await PolytopiaBackendAdapter.Instance.UploadHighscoresHttp(new UploadHighscoresBindingModel
			{
				InitialGameStateData = SerializationHelpers.ToByteArray(initialGameState, initialGameState.Version),
				CurrentGameStateData = SerializationHelpers.ToByteArray(currentGameState, currentGameState.Version)
			})) : (await PolytopiaBackendAdapter.Instance.UploadHighscores(new UploadHighscoresBindingModel
			{
				InitialGameStateData = SerializationHelpers.ToByteArray(initialGameState, initialGameState.Version),
				CurrentGameStateData = SerializationHelpers.ToByteArray(currentGameState, currentGameState.Version)
			})));
			isUploadingHighscore = false;
			if (serverResponse.Success)
			{
				hasUploadedHighScore = true;
				NetworkUtils.ShowLoader(Localization.Get("highscore.uploading.successful"), 0, 2f);
			}
			else
			{
				ShowRetryUploadHighscorePopup();
			}
		}
		catch
		{
			isUploadingHighscore = false;
			ShowRetryUploadHighscorePopup();
		}
	}

	private void ShowRetryUploadHighscorePopup()
	{
		NetworkUtils.ShowLoaderError(Localization.Get("highscore.uploading.failed"));
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("highscore.error.title");
		basicPopup.Description = Localization.Get("highscore.error");
		basicPopup.buttonData = new PopupBase.PopupButtonData[2]
		{
			new PopupBase.PopupButtonData("buttons.back"),
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, async delegate
			{
				await UploadHighscore();
			})
		};
		basicPopup.Show();
	}

	public override Task SendCommand(CommandBase command)
	{
		ReceiveCommand(command);
		return Task.CompletedTask;
	}

	private Task<bool> ReceiveCommand(CommandBase command)
	{
		string error;
		return Task.FromResult(base.ActionManager.ExecuteCommand(command, out error));
	}

	public override Task<bool> ReceiveCommand(List<CommandBase> commands)
	{
		return Task.FromResult(base.ActionManager.ExecuteCommands(commands));
	}

	public override ushort GetLastSeenCommand()
	{
		return lastSeenCommand;
	}

	public override void SetLastSeenCommand(ushort commandIndex)
	{
		lastSeenCommand = commandIndex;
	}
}
