using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polytopia.Data;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;

public class ReplayClient : ClientBase
{
	public int currentViewingPlayer;

	public bool doShowAllPlayers;

	public bool doAutoSwitchPlayers = true;

	private ushort lastSeenCommand;

	private byte lastViewingPlayer;

	public override string LOG_PREFIX => "<color=#63d863>[REPLAY]</color>";

	public override bool IsReplay => true;

	public override bool IsSpectating => IsReplay;

	public override void Connect(Uri endpoint)
	{
	}

	public override void Disconnect()
	{
	}

	public override Task<CreateSessionResult> CreateSession(GameSettings settings, List<PlayerState> players)
	{
		return null;
	}

	public override Guid[] GetSessions(long playerId)
	{
		return null;
	}

	public override void SaveSession(Guid gameId, bool showSaveErrorPopup = false)
	{
	}

	public override void EndSession()
	{
		Log.Info("EndSession", Array.Empty<object>());
	}

	public override async Task<bool> OpenSession(Guid gameId)
	{
		Log.Verbose("{0} Opening session: {1}...", new object[2] { LOG_PREFIX, gameId });
		Reset();
		isReady = false;
		try
		{
			ServerResponse<GameViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.SpectateGame(gameId);
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
			Log.Info("{0} Session opened, version: {1}", new object[2] { LOG_PREFIX, version });
			UpdateGameStateImmediate(data.CurrentGameStateData ?? data.InitialGameStateData, StateUpdateReason.GameJoined);
			PrepareSession();
			return true;
		}
		catch (Exception ex)
		{
			Log.Error(ex.Message, Array.Empty<object>());
			GameManager.GetAnalyticsManager().SendEvent("ReplayLoadFailure", new Dictionary<string, object>
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

	protected override void PrepareSession()
	{
		Log.Verbose("{0} Session ready (Hash: {1})", new object[2]
		{
			LOG_PREFIX,
			GameState.GetHashCode()
		});
		currentViewingPlayer = 1;
		lastViewingPlayer = 1;
		lastSeenCommand = 1;
		base.ActionManager.Pause();
		RewindToCommand(lastSeenCommand, delegate
		{
			SessionOpened();
		});
	}

	public override PlayerState GetCurrentLocalPlayer()
	{
		if (GameState?.PlayerStates == null)
		{
			return null;
		}
		if (doAutoSwitchPlayers)
		{
			for (int i = 0; i < GameState.PlayerStates.Count; i++)
			{
				PlayerState playerState = GameState.PlayerStates[i];
				if (GameState.CurrentPlayer == byte.MaxValue && playerState.Id == lastViewingPlayer)
				{
					return playerState;
				}
				if (playerState.Id == GameState.CurrentPlayer)
				{
					lastViewingPlayer = playerState.Id;
					return playerState;
				}
			}
		}
		else
		{
			for (int j = 0; j < GameState.PlayerStates.Count; j++)
			{
				PlayerState playerState2 = GameState.PlayerStates[j];
				if (playerState2.Id == currentViewingPlayer)
				{
					lastViewingPlayer = playerState2.Id;
					return playerState2;
				}
			}
		}
		return null;
	}

	public override bool IsPlayerLocal(byte playerId)
	{
		return playerId == GetCurrentLocalPlayer().Id;
	}

	protected override void OnFinishedProcessing()
	{
		base.OnFinishedProcessing();
	}

	public override ushort GetLastSeenCommand()
	{
		return lastSeenCommand;
	}

	public bool GetAutoSwitchPlayers()
	{
		return doAutoSwitchPlayers;
	}

	public override void SetLastSeenCommand(ushort commandIndex)
	{
		lastSeenCommand = commandIndex;
	}

	public override Task<bool> PickTribe(TribeData.Type tribeType, List<TribeData.Type> disabledTribes = null, SkinType skinType = SkinType.Default)
	{
		return null;
	}

	public override Task<bool> ReceiveCommand(List<CommandBase> commands)
	{
		return null;
	}

	public override Task SendCommand(CommandBase command)
	{
		return null;
	}
}
