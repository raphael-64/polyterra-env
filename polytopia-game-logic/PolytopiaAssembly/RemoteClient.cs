using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polytopia.Data;
using Polytopia.IO;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;

public class RemoteClient : ClientBase
{
	public const int SERVER_MESSAGE_DELAY = 1000;

	private bool hasInitializedSaveData;

	private bool isWaitingForServer;

	private ushort lastSeenCommand;

	private uint resets;

	public override string LOG_PREFIX => "<color=#34e0e2>[RC]</color>";

	public override bool HasLocalAI => false;

	public override bool IsWaitingForCommand
	{
		get
		{
			if (!isWaitingForServer)
			{
				if (base.ActionManager != null && base.ActionManager.IsProcessing)
				{
					return !base.ActionManager.IsWaitingForCommandTrigger(GetCurrentLocalPlayer().Id);
				}
				return false;
			}
			return true;
		}
	}

	public override bool IsSpectating
	{
		get
		{
			if (GameManager.GetRemoteGameDataManager().TryGetLocalParticipator(gameId, out var participatorViewModel))
			{
				return participatorViewModel.InvitationState == PlayerInvitationState.Resigned;
			}
			PlayerState currentLocalPlayer = GetCurrentLocalPlayer();
			if (currentLocalPlayer != null)
			{
				if (!currentLocalPlayer.AutoPlay)
				{
					return !currentLocalPlayer.IsAlive(GameState);
				}
				return true;
			}
			return false;
		}
	}

	public override void Reset()
	{
		lastSeenCommand = 0;
		base.Reset();
	}

	public override async void ResetGameState(StateUpdateReason reason)
	{
		Log.Info("{0} Resetting GameState to latest, reason: {1}...", new object[2] { LOG_PREFIX, reason });
		Guid gameId = base.CurrentGameId.Value;
		ServerResponse<GameViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetGameViewModelByIdAsync(base.CurrentGameId.Value);
		if (GameState == null || gameId != base.CurrentGameId.Value)
		{
			Log.Warning("Client has been nulled, aborting reset", Array.Empty<object>());
		}
		else if (!serverResponse.Success)
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("misc.error.title");
			basicPopup.Description = Localization.Get("misc.gameoutofsync");
			basicPopup.buttonData = new PopupBase.PopupButtonData[2]
			{
				new PopupBase.PopupButtonData("buttons.exit", PopupBase.PopupButtonData.States.None, delegate
				{
					GameManager.ReturnToMenu();
				}),
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					ResetGameState(reason);
				})
			};
			basicPopup.Show();
		}
		else
		{
			resets++;
			GameViewModel data = serverResponse.Data;
			UpdateGameState(data.CurrentGameStateData ?? data.InitialGameStateData, reason);
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
		Log.Verbose("{0} Disconnected from remote server", new object[1] { LOG_PREFIX });
		if (base.CurrentGameId.HasValue && base.CurrentGameId.Value != Guid.Empty)
		{
			PolytopiaBackendAdapter.Instance.UnsubscribeToGame(new UnsubscribeToGameBindingModel
			{
				GameId = base.CurrentGameId.Value
			});
			SaveSession(base.CurrentGameId.Value);
		}
		Reset();
		OnDisconnected?.Invoke();
	}

	public override async Task<bool> OpenSession(Guid gameId)
	{
		Log.Verbose("{0} Opening session: {1}...", new object[2] { LOG_PREFIX, gameId });
		Reset();
		isReady = false;
		try
		{
			ServerResponse<GameViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.JoinGame(gameId);
			if (!serverResponse.Success)
			{
				PopupManager.ShowErrorPopup(Localization.GetErrorMessage(serverResponse));
				return false;
			}
			GameViewModel data = serverResponse.Data;
			base.gameId = gameId;
			AnalyticsManager.SetCrashMetaData("game_id", base.gameId.ToString());
			GameState result;
			int version;
			bool flag = SerializationHelpers.FromByteArray<GameState>(data.InitialGameStateData, out result, out version);
			if (!VersionManager.IsGameVersionSupported(version))
			{
				PopupManager.ShowErrorPopup(Localization.GetErrorMessage(ErrorCode.UnsupportedOpenVersion));
				return false;
			}
			if (!flag)
			{
				PopupManager.ShowErrorPopup(Localization.GetErrorMessage(ErrorCode.GameStateDeserializationFailed));
				return false;
			}
			initialGameState = result;
			hasInitializedSaveData = false;
			Log.Info("{0} Session opened, version: {1}", new object[2] { LOG_PREFIX, version });
			UpdateGameStateImmediate(data.CurrentGameStateData ?? data.InitialGameStateData, StateUpdateReason.GameJoined);
			PrepareSession();
			return true;
		}
		catch (Exception ex)
		{
			Log.Error(ex.Message, Array.Empty<object>());
			GameManager.GetAnalyticsManager().SendEvent("GameLoadFailure", new Dictionary<string, object>
			{
				{
					"error",
					ex.ToString()
				},
				{ "gameId", gameId },
				{
					"version",
					VersionManager.SemanticVersion.ToString()
				}
			});
			PopupManager.ShowErrorPopup(Localization.Get("misc.errorloadinggame"));
			return false;
		}
	}

	public override async Task<CreateSessionResult> CreateSession(GameSettings settings, List<PlayerState> players)
	{
		Log.Verbose("{0} Creating session...", new object[1] { LOG_PREFIX });
		Reset();
		isReady = false;
		try
		{
			ServerResponse<GameViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.CreateGame(new CreateGameBindingModel
			{
				UseLowestGameVersionAmongPlayers = true,
				Version = VersionManager.GameVersion,
				Players = settings.Players.Select((PlayerData player) => new PlayerBindingModel
				{
					UserId = ((player.type != PlayerData.Type.Bot) ? new Guid?(player.profile.id) : ((Guid?)null)),
					AutoPlay = (player.type == PlayerData.Type.Bot),
					PlayerName = player.GetName(),
					Handicap = ((player.type != PlayerData.Type.Bot) ? 1 : GameSettings.HandicapFromDifficulty(player.botDifficulty))
				}).ToList(),
				GameSettingsData = SerializationHelpers.ToByteArray(settings, VersionManager.GameVersion)
			});
			if (!serverResponse.Success)
			{
				PopupManager.ShowErrorPopup(Localization.GetErrorMessage(serverResponse));
				return CreateSessionResult.FailedCreate;
			}
			GameViewModel data = serverResponse.Data;
			gameId = data.Id;
			AnalyticsManager.SetCrashMetaData("game_id", gameId.ToString());
			GameState result;
			int version;
			bool flag = SerializationHelpers.FromByteArray<GameState>(data.InitialGameStateData, out result, out version);
			if (!VersionManager.IsGameVersionSupported(version))
			{
				PopupManager.ShowErrorPopup(Localization.GetErrorMessage(ErrorCode.UnsupportedOpenVersion));
				return CreateSessionResult.FailedOpen;
			}
			if (!flag)
			{
				PopupManager.ShowErrorPopup(Localization.GetErrorMessage(ErrorCode.GameStateDeserializationFailed));
				return CreateSessionResult.FailedOpen;
			}
			initialGameState = result;
			UpdateGameStateImmediate(data.CurrentGameStateData ?? data.InitialGameStateData, StateUpdateReason.GameCreated);
			await PolytopiaBackendAdapter.Instance.SubscribeToGame(new SubscribeToGameBindingModel
			{
				GameId = data.Id
			});
			Log.Verbose("{0} Session created: {1}...", new object[2] { LOG_PREFIX, gameId });
			return CreateSessionResult.Success;
		}
		catch (Exception ex)
		{
			Log.Error(ex.Message, Array.Empty<object>());
			GameManager.GetAnalyticsManager().SendEvent("GameCreationFailure", new Dictionary<string, object>
			{
				{
					"error",
					ex.ToString()
				},
				{
					"version",
					VersionManager.SemanticVersion.ToString()
				}
			});
			PopupManager.ShowErrorPopup(Localization.Get("misc.errorloadinggame"));
			return CreateSessionResult.FailedCreate;
		}
	}

	public override void SaveSession(Guid gameId, bool showSaveErrorPopup = false)
	{
		if (hasInitializedSaveData && base.ActionManager != null)
		{
			Log.Verbose("{0} Saving session: {1}...", new object[2] { LOG_PREFIX, gameId });
			SaveRemoteGameData(showSaveErrorPopup);
		}
	}

	private void SaveRemoteGameData(bool showSaveErrorPopup)
	{
		string multiplayerFilePath = Paths.GetMultiplayerFilePath(gameId.ToString());
		if (DiskSerializationHelpers.ToDisk(new RemoteGameData
		{
			lastSeenCommand = base.ActionManager.LastSeenCommand
		}, multiplayerFilePath, currentGameState.Version, out var exception))
		{
			Log.Verbose("{0} Saved game data to: {1}", new object[2] { LOG_PREFIX, multiplayerFilePath });
		}
		else if (DiskSerializationHelpers.IsDiskFullException(exception) && showSaveErrorPopup)
		{
			TriggerSaveToDiskFailedPopup(delegate
			{
				SaveRemoteGameData(showSaveErrorPopup);
			});
		}
	}

	public override Guid[] GetSessions(long playerId)
	{
		return null;
	}

	protected override void PrepareSession()
	{
		Log.Verbose("{0} Loading local data for game: {1}", new object[2]
		{
			LOG_PREFIX,
			gameId.ToString()
		});
		string multiplayerFilePath = Paths.GetMultiplayerFilePath(gameId.ToString());
		ushort num = (ushort)currentGameState.GetLastPerformedCommandByPlayer(GetCurrentLocalPlayer().Id);
		if (PolytopiaFile.Exists(multiplayerFilePath) && DiskSerializationHelpers.FromDisk<RemoteGameData>(multiplayerFilePath, out var result))
		{
			lastSeenCommand = ((result.lastSeenCommand > GameState.LastProcessedCommand) ? GameState.LastProcessedCommand : result.lastSeenCommand);
			if (lastSeenCommand < num)
			{
				lastSeenCommand = num;
			}
			base.ActionManager.LastSeenCommand = lastSeenCommand;
			hasInitializedSaveData = true;
			if (lastSeenCommand < currentGameState.LastProcessedCommand)
			{
				Log.Verbose("{0} Found local data, last seen command is {1} (server has {2} new commands)", new object[3]
				{
					LOG_PREFIX,
					lastSeenCommand,
					currentGameState.LastProcessedCommand - lastSeenCommand
				});
			}
			else
			{
				Log.Verbose("{0} Found local data, we are up to date", new object[1] { LOG_PREFIX });
			}
		}
		else
		{
			hasInitializedSaveData = true;
			Log.Verbose("{0} No local data found", new object[1] { LOG_PREFIX });
			lastSeenCommand = num;
		}
		Log.Verbose("{0} Session ready, position {1}/{2} (Hash: {3})", new object[4]
		{
			LOG_PREFIX,
			lastSeenCommand,
			GameState.LastProcessedCommand,
			GameState.GetHashCode()
		});
		base.ActionManager.Pause();
		RewindToCommand(lastSeenCommand, delegate
		{
			SessionOpened();
		});
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
			SetParticipationDoneBindingModel participationDone = new SetParticipationDoneBindingModel
			{
				GameId = base.CurrentGameId.Value
			};
			PolytopiaBackendAdapter.Instance.SetParticipationDone(participationDone);
			isSessionEnded = true;
		}
	}

	public override bool IsPlayerLocal(byte playerId)
	{
		return playerId == GetCurrentLocalPlayer().Id;
	}

	public override PlayerState GetCurrentLocalPlayer()
	{
		if (GameState?.PlayerStates == null)
		{
			return null;
		}
		for (int i = 0; i < GameState.PlayerStates.Count; i++)
		{
			PlayerState playerState = GameState.PlayerStates[i];
			if (playerState.Id != byte.MaxValue && playerState.AccountId == AccountManager.PlayerAccountId)
			{
				return playerState;
			}
		}
		return null;
	}

	public override async Task<bool> PickTribe(TribeData.Type tribeType, List<TribeData.Type> disabledTribes = null, SkinType skinType = SkinType.Default)
	{
		Log.Verbose("{0} Picking tribe: {1}", new object[2] { LOG_PREFIX, tribeType });
		if (!base.CurrentGameId.HasValue)
		{
			Log.Verbose("Tried to pick tribe but no open session GameId was found ", Array.Empty<object>());
			return false;
		}
		List<int> list = null;
		if (disabledTribes != null)
		{
			list = new List<int>(disabledTribes.Count);
			foreach (TribeData.Type disabledTribe in disabledTribes)
			{
				list.Add((int)disabledTribe);
			}
		}
		PickTribeBindingModel model = new PickTribeBindingModel
		{
			TribeType = (int)tribeType,
			GameId = gameId,
			DisabledTribes = list,
			SkinType = (int)skinType
		};
		try
		{
			ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.PickTribe(model);
			if (!serverResponse.Success)
			{
				PopupManager.ShowErrorPopup(Localization.GetErrorMessage(serverResponse));
				return false;
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex.Message, Array.Empty<object>());
			return false;
		}
		return true;
	}

	public override async Task SendCommand(CommandBase command)
	{
		if (!base.CurrentGameId.HasValue)
		{
			Log.Verbose("Tried to perform and send command but no GameId was set", Array.Empty<object>());
			return;
		}
		if (!ClientActionManager.CanExecuteCommand(command, GameState))
		{
			Log.Error("Tried to send invalid command", Array.Empty<object>());
			return;
		}
		uint currentResetId = resets;
		if (command.NeedServerConfirmation())
		{
			if (await SendCommandToServer(command) && currentResetId == resets)
			{
				base.ActionManager.ExecuteCommands(new List<CommandBase> { command });
			}
			return;
		}
		base.ActionManager.ExecuteCommands(new List<CommandBase> { command });
		if (GameManager.debugSkipNextSend)
		{
			Log.Verbose("skipped send", Array.Empty<object>());
			GameManager.debugSkipNextSend = false;
		}
		else
		{
			await SendCommandToServer(command);
		}
	}

	private async Task<bool> SendCommandToServer(CommandBase command)
	{
		NetworkUtils.ShowLoader(Localization.Get("onlineview.waiting.server"), 1000);
		isWaitingForServer = true;
		GameEvents.WaitingForServer(isWaiting: true);
		Guid? gameId = base.CurrentGameId;
		ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.SendCommand(new SendCommandBindingModel
		{
			Command = new PolytopiaCommandViewModel(CommandBase.ToByteArray(command, GameState.Version)),
			GameId = gameId.Value
		});
		isWaitingForServer = false;
		GameEvents.WaitingForServer(isWaiting: false);
		NetworkUtils.HideLoader();
		if (GameManager.Client != this || gameId != base.CurrentGameId || UIManager.Instance.type != UIManager.Type.Ingame)
		{
			return false;
		}
		if (!serverResponse.Success)
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("misc.error.title");
			basicPopup.Description = Localization.Get("misc.failedcommand");
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, async delegate
				{
					ResetGameState(StateUpdateReason.InvalidCommand);
				})
			};
			basicPopup.Show();
			return false;
		}
		return true;
	}

	public override Task<bool> ReceiveCommand(List<CommandBase> commands)
	{
		if (commands == null || commands.Count == 0)
		{
			return Task.FromResult(result: false);
		}
		if (!base.ActionManager.ExecuteCommands(commands))
		{
			Log.Warning("Could not perform command received from backend.", Array.Empty<object>());
			ResetGameState(StateUpdateReason.InvalidCommand);
			return Task.FromResult(result: false);
		}
		return Task.FromResult(result: true);
	}

	public override ushort GetLastSeenCommand()
	{
		return lastSeenCommand;
	}

	public override void SetLastSeenCommand(ushort commandIndex)
	{
		lastSeenCommand = commandIndex;
	}

	protected override void OnFinishedProcessing()
	{
		base.OnFinishedProcessing();
	}

	protected override void OnStartedReplay()
	{
	}

	public override async Task<ServerResponse<ResponseViewModel>> ResignCurrentGameAsync()
	{
		if (!base.CurrentGameId.HasValue)
		{
			return null;
		}
		NetworkUtils.ShowLoader();
		ServerResponse<ResponseViewModel> result = await PolytopiaBackendAdapter.Instance.Resign(new ResignBindingModel
		{
			GameId = base.CurrentGameId.Value
		});
		NetworkUtils.HideLoader();
		return result;
	}
}
