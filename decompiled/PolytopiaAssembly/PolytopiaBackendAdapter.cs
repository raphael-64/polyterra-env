using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polytopia.IO;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using UnityEngine;

public class PolytopiaBackendAdapter : BackendAdapter
{
	private const string BUNDLE_ID_ALPHA = "com.midjiwan.polytopia.alpha";

	public static readonly PolytopiaBackendAdapter Instance = new PolytopiaBackendAdapter().UseLoggerProvider<DebugConsoleLoggerProvider>().ConfigureSignalRLogLevel((LogLevel)Config.signalRLogLevel.IntValue).UseBackendUri(new Uri(Config.backendUri.Value))
		.UseHttpClient<BackendHttpClient>()
		.ConfigureLZ4Compression(delegate(LZ4Options options)
		{
			options.UseLZ4Compression = true;
		})
		.UseAs<PolytopiaBackendAdapter>();

	private bool isLoggingIn;

	private string tokenLocalPath => Path.Combine(Paths.GetUserDirectory(), "jwtToken");

	public bool IsUserTriggeredLogin { get; set; }

	public bool IsUserTriggeredLogout { get; set; }

	public bool HasSocialLogin
	{
		get
		{
			return PlayerPrefsUtils.GetBoolValue("has_social_login", defaultValue: false);
		}
		set
		{
			PlayerPrefsUtils.SetBoolValue("has_social_login", value);
			PlayerPrefsUtils.Save();
		}
	}

	public async void LoginFireAndForget(bool connect = true, bool silent = false)
	{
		await Login(connect, silent);
	}

	public override async Task Login(bool connect = true, bool silent = false)
	{
		Log("Logging in", (LogLevel)1);
		if (isLoggingIn)
		{
			Log("Already loggin in", (LogLevel)1);
			return;
		}
		await new WaitForUpdate();
		isLoggingIn = true;
		ConnectionStatus = ((ConnectionStatus == ConnectionStatus.None) ? ConnectionStatus.Connecting : ConnectionStatus.Reconnecting);
		BackendEvents.BackendConnectionChanged(ConnectionStatus);
		if (!base.IsAuthenticated)
		{
			try
			{
				Log("LoginPlatform", (LogLevel)1);
				if (await LoginPlatform() == null)
				{
					throw new Exception("Token not received");
				}
			}
			catch (Exception arg)
			{
				Log($"Platform login failed: {arg}", (LogLevel)3);
			}
		}
		if (!base.IsAuthenticated || !HasSocialLogin)
		{
			ConnectionStatus = ConnectionStatus.ConnectionFailed;
			BackendEvents.BackendConnectionChanged(ConnectionStatus);
			isLoggingIn = false;
			return;
		}
		if (SystemManager.Platform == SystemManager.Platforms.PC)
		{
			await GameManager.RefreshSteamNotificationStatus();
		}
		if (connect && base.IsAuthenticated && !base.IsConnected && GameManager.IsMultiplayerEnabled)
		{
			try
			{
				await Connect();
				Log("Connected to backend", (LogLevel)1);
			}
			catch (Exception arg2)
			{
				Log($"Server connection failed: {arg2}", (LogLevel)3);
			}
		}
		if (!base.IsConnected)
		{
			ConnectionStatus = ConnectionStatus.ConnectionFailed;
			BackendEvents.BackendConnectionChanged(ConnectionStatus);
			isLoggingIn = false;
			return;
		}
		if (base.ClientUserData != null && base.ClientUserData.User != null)
		{
			AnalyticsManager.SetCrashMetaData("user_id", base.ClientUserData.User.PolytopiaId.ToString());
			AvatarState result = null;
			int version = 0;
			if (base.ClientUserData.User.AvatarStateData == null || !SerializationHelpers.FromByteArray<AvatarState>(base.ClientUserData.User.AvatarStateData, out result, out version) || version < VersionManager.AvatarVersion)
			{
				Log($"Avatar state {result}, version {version}, missing or not up to date", (LogLevel)1);
				AvatarState serializable = ((result == null || version < 9) ? AvatarExtensions.CreateRandomState(VersionManager.GameVersion, Random.Range(0, int.MaxValue)) : result);
				base.ClientUserData.User.AvatarStateData = SerializationHelpers.ToByteArray(serializable, VersionManager.AvatarVersion);
				await UpdateAvatar(new AvatarBindingModel
				{
					AvatarStateData = base.ClientUserData.User.AvatarStateData
				});
			}
		}
		ServerResponse<PlayersStatusesResponse> serverResponse = await GetFriendsStatuses();
		if (serverResponse.Success)
		{
			AccountManager.UpdatePlayersStatuses(serverResponse.Data.Statuses);
		}
		ConnectionStatus = ((ConnectionStatus == ConnectionStatus.None) ? ConnectionStatus.Connected : ConnectionStatus.Reconnected);
		BackendEvents.BackendConnectionChanged(ConnectionStatus);
		isLoggingIn = false;
	}

	public static Platform GetCurrentPlatform()
	{
		return Platform.Steam;
	}

	private async Task<ServerResponse<PolytopiaToken>> LoginPlatform()
	{
		return await LoginPlatformSteam();
	}

	public async Task LogoutPlatform()
	{
		await Task.FromResult(result: true);
	}

	private async Task<ServerResponse<PolytopiaToken>> LoginPlatformTesla()
	{
		return await TeslaArcadePlatform.LoginAsync();
	}

	private async Task<ServerResponse<PolytopiaToken>> LoginPlatformSteam()
	{
		if (Config.fakePlayerId.Value != null)
		{
			HasSocialLogin = true;
			if (Config.loginFakeDebugLegacy.IntValue == 1)
			{
				return await LoginDebugLegacyUser(new LoginFakeBindingModel
				{
					GameVersion = VersionManager.GameVersion,
					DeviceId = SystemInfo.deviceUniqueIdentifier,
					UserName = Config.fakePlayerId.Value,
					BundleId = Application.identifier,
					SemanticVersion = VersionManager.SemanticVersion.ToString(),
					LegacyIdClaim = VersionMigration.GetFloxLegacyID()
				});
			}
			return await LoginFake(new LoginFakeBindingModel
			{
				GameVersion = VersionManager.GameVersion,
				DeviceId = SystemInfo.deviceUniqueIdentifier,
				UserName = Config.fakePlayerId.Value
			});
		}
		SteamAuthTicket steamAuthTicket = await FacepunchHelpers.CreateSteamTicket((uint)Config.steamAppId.IntValue);
		HasSocialLogin = steamAuthTicket != null;
		BackendEvents.BackendConnectionChanged(ConnectionStatus.SocialConnected);
		if (steamAuthTicket == null)
		{
			Log("Steam ticket not found", (LogLevel)4);
			return null;
		}
		return await LoginSteam(new SteamLoginBindingModel
		{
			GameVersion = VersionManager.GameVersion,
			DeviceId = SystemInfo.deviceUniqueIdentifier,
			SteamAuthTicket = steamAuthTicket
		});
	}

	protected override async Task CacheUserProfileData(PolytopiaUserViewModel polytopiaUserViewModel)
	{
		await CacheManager.CacheUserProfileData(polytopiaUserViewModel);
	}

	protected override async Task StoreJwtToken(string jwtToken)
	{
		using StreamWriter writer = new StreamWriter(PolytopiaFile.Open(tokenLocalPath, FileMode.Create));
		Log("Storing jwtToken: " + tokenLocalPath, (LogLevel)1);
		await writer.WriteAsync(jwtToken);
	}

	protected override async Task<string> LoadJwtToken()
	{
		if (PolytopiaFile.Exists(tokenLocalPath))
		{
			using (StreamReader reader = new StreamReader(File.Open(tokenLocalPath, FileMode.Open)))
			{
				Log("Loading jwtToken: " + tokenLocalPath, (LogLevel)1);
				return await reader.ReadToEndAsync();
			}
		}
		return null;
	}

	public async void SetParticipationFailedParseFireAndForget(SetParticipationHasFailedParseBindingModel model)
	{
		await SetParticipationFailedParse(model);
	}

	public override async Task OnNotify(string message)
	{
		await Task.FromResult(result: true);
	}

	public override async Task OnCommand(CommandArrayViewModel model)
	{
		await new WaitForUpdate();
		if (GameManager.Client.CurrentGameId != model.GameId)
		{
			Log($"Received command for other game ({model.GameId}), ignoring.", (LogLevel)1);
			return;
		}
		List<CommandBase> list = new List<CommandBase>();
		foreach (PolytopiaCommandViewModel command2 in model.Commands)
		{
			CommandBase.FromByteArray(command2.SerializedData, out var command, out var _);
			list.Add(command);
		}
		await GameManager.Client.ReceiveCommand(list);
	}

	public override async Task OnGameStateUpdated(GameStateViewModel model, StateUpdateReason reason)
	{
		await new WaitForUpdate();
		if (GameManager.Client.CurrentGameId != model.GameId)
		{
			Log($"Received state for other game ({model.GameId}), ignoring.", (LogLevel)1);
			return;
		}
		Log($"Received state from backend ({reason.ToString()}). GameId: {model.GameId}, hash: {model.SerializedGameState.GetHashCode()}", (LogLevel)1);
		GameManager.Client.UpdateGameState(model.SerializedGameState, reason);
		if (model.ExecutedCommands != null)
		{
			List<CommandBase> list = new List<CommandBase>();
			foreach (PolytopiaCommandViewModel executedCommand in model.ExecutedCommands)
			{
				CommandBase.FromByteArray(executedCommand.SerializedData, out var command, out var _);
				Log($"Received command from backend: {command.ToString()} (player {command.PlayerId})", (LogLevel)1);
				list.Add(command);
			}
			await GameManager.Client.ReceiveCommand(list);
		}
		BackendEvents.ReceivedGameState(model.GameId, reason);
	}

	public override async Task OnGameSummaryUpdated(GameSummaryViewModel gameSummaryViewModel, StateUpdateReason reason)
	{
		await new WaitForUpdate();
		Log($"Received game summary from backend ({reason}). GameId: {gameSummaryViewModel.GameId}", (LogLevel)1);
		if (reason == StateUpdateReason.InvitationResponse)
		{
			ParticipatorViewModel participatorViewModel = null;
			foreach (ParticipatorViewModel participator in gameSummaryViewModel.Participators)
			{
				if (participator.UserId == AccountManager.PlayerAccountId)
				{
					participatorViewModel = participator;
					break;
				}
			}
			if (participatorViewModel != null && participatorViewModel.InvitationState == PlayerInvitationState.Declined)
			{
				await Instance.UnsubscribeToGame(new UnsubscribeToGameBindingModel
				{
					GameId = gameSummaryViewModel.GameId,
					SubscriptionType = SubscriptionType.GameSummary
				});
				GameManager.GetRemoteGameDataManager().RemoveGameSummary(gameSummaryViewModel.GameId);
				return;
			}
		}
		BackendEvents.ReceivedGameSummary(gameSummaryViewModel, reason);
	}

	public override async Task OnGameDeleted(Guid gameId)
	{
		await new WaitForUpdate();
		Log($"Game deleted in backend. GameId: {gameId}", (LogLevel)1);
		BackendEvents.ReceivedGameDeletion(gameId);
	}

	public override async Task OnMatchmakingGameUpdated(long gameId, MatchmakingUpdateReason reason)
	{
		await new WaitForUpdate();
		Log($"Matchmaking Game {gameId} updated because of {reason}", (LogLevel)1);
		BackendEvents.ReceivedMatchmakingGameUpdate(gameId, reason);
	}

	public override async Task OnPlayerResigned(PlayerResignedViewModel playerResignedViewModel)
	{
		await new WaitForUpdate();
		Log(string.Format("User {0} {1} from game with GameId: {2}", playerResignedViewModel.Resignee, playerResignedViewModel.Kicked ? "got kicked" : "resigned", playerResignedViewModel.GameSummary.GameId), (LogLevel)2);
		BackendEvents.ReceivedPlayerResignation(playerResignedViewModel);
	}

	public override async Task OnGameReadyToStart(Guid gameId)
	{
		await new WaitForUpdate();
		Log($"Got game ready to start: {gameId}", (LogLevel)1);
		if (UIManager.Instance.type == UIManager.Type.StartMenu && !PopupManager.IsPopupShowing<GameInfoPopup>())
		{
			ServerResponse<GameSummaryViewModel> serverResponse = await GetGameSummaryViewModelByIdAsync(gameId);
			if (!serverResponse.Success)
			{
				NetworkUtils.ShowLoaderError(Localization.Get("misc.pushfailed"));
				return;
			}
			GameInfoPopup gameInfoPopup = PopupManager.GetGameInfoPopup();
			gameInfoPopup.SetData(serverResponse.Data);
			gameInfoPopup.Show();
		}
	}

	public override async Task OnInvitation(Guid gameId)
	{
		SubscribeToGame(new SubscribeToGameBindingModel
		{
			GameId = gameId,
			SubscriptionType = SubscriptionType.GameSummary
		}).WrapErrors();
		await new WaitForUpdate();
		Log($"Got game invitation: {gameId}", (LogLevel)1);
		if (UIManager.Instance.type != UIManager.Type.StartMenu || PopupManager.IsPopupShowing<GameInfoPopup>())
		{
			return;
		}
		ServerResponse<GameSummaryViewModel> serverResponse = await GetGameSummaryViewModelByIdAsync(gameId);
		if (!serverResponse.Success)
		{
			NetworkUtils.ShowLoaderError(Localization.Get("misc.pushfailed"));
			return;
		}
		GameManager.GetRemoteGameDataManager().AddOrUpdateGameSummary(serverResponse.Data);
		GameInfoPopup gameInfoPopup = PopupManager.GetGameInfoPopup();
		gameInfoPopup.SetData(serverResponse.Data);
		gameInfoPopup.Show();
		if (UIManager.Instance.CurrentScreen == UIConstants.Screens.MultiplayerScreen)
		{
			((MultiplayerScreen)UIManager.Instance.GetScreen(UIConstants.Screens.MultiplayerScreen)).Reload();
		}
	}

	public override async Task OnFriendsUpdated(List<PolytopiaFriendViewModel> friends)
	{
		await new WaitForUpdate();
		Log($"Friends updated: {friends}", (LogLevel)1);
		AccountManager.UpdateFriends(friends);
	}

	public override async Task OnPlayerStatusUpdated(Guid playerId, PlayerStatus status)
	{
		await new WaitForUpdate();
		Log($"Player status updated: {playerId} {status.PlayerOnlineStatus}", (LogLevel)1);
		AccountManager.UpdatePlayerStatus(playerId.ToString(), status);
	}

	public override async Task OnUserUpdated(PolytopiaUserViewModel user)
	{
		await new WaitForUpdate();
		Log($"User updated (pub-sub): {user?.PolytopiaId}", (LogLevel)1);
		if (AccountManager.PlayerAccountId == user.PolytopiaId)
		{
			await CacheManager.CacheUserProfileData(user);
			base.ClientUserData.User = user;
		}
		else
		{
			AccountManager.UpdateFriend(user);
		}
		BackendEvents.RefreshUser(user);
	}

	public override async Task OnFriendRequestReceived(Guid friendUserId)
	{
		await new WaitForUpdate();
		Log($"Friend request received from user {friendUserId}", (LogLevel)1);
		if ((Object)(object)UIManager.Instance != (Object)null && UIManager.Instance.type == UIManager.Type.StartMenu)
		{
			PlayerData playerData = await AccountManager.GetFriendPlayerDataWithId(friendUserId);
			PopupManager.GetFriendInfoPopup(playerData.profile.id, playerData.GetName(), playerData.state, playerData.profile.avatarState, playerData).Show();
		}
		else
		{
			NotificationManager.Alert(Localization.Get("friendlist.requestrecieved"), 2f);
		}
	}

	public override async Task OnFriendRequestAccepted(Guid friendUserId)
	{
		await new WaitForUpdate();
		Log($"User {friendUserId} has accepted your friend request.", (LogLevel)1);
		NotificationManager.Alert(Localization.Get("friendlist.requestaccepted"), 2f);
	}

	public override async Task OnActionableGamesUpdated(int count)
	{
		await new WaitForUpdate();
		Log($"ActionableGamesCountUpdated: {count}", (LogLevel)1);
		GameManager.ActionableGamesCount = count;
		GameEvents.RefreshBadges();
	}

	public override async Task OnReconnecting(Exception exception)
	{
		await new WaitForUpdate();
		Log($"Lost connection, reconnecting. {exception}", (LogLevel)1);
		ConnectionStatus = ConnectionStatus.Reconnecting;
		BackendEvents.BackendConnectionChanged(ConnectionStatus.Reconnecting);
	}

	protected override async Task OnReconnected(string arg)
	{
		await new WaitForUpdate();
		await base.OnReconnected(arg);
		Log("Reconnected", (LogLevel)1);
		Guid? guid = (GameManager.Client as RemoteClient)?.CurrentGameId;
		if (UIManager.Instance.type == UIManager.Type.Ingame && guid.HasValue && guid != Guid.Empty)
		{
			await SubscribeToGame(new SubscribeToGameBindingModel
			{
				GameId = guid.Value
			});
		}
		Log("Reconnected.", (LogLevel)1);
		ConnectionStatus = ConnectionStatus.Connected;
		BackendEvents.BackendConnectionChanged((ConnectionStatus == ConnectionStatus.None) ? ConnectionStatus.Connected : ConnectionStatus.Reconnected);
	}

	public override async Task OnClosed(Exception exception)
	{
		await new WaitForUpdate();
		Log($"Lost connection, reconnection timeout expired. {exception}", (LogLevel)1);
		if (!IsUserTriggeredLogout)
		{
			NetworkUtils.ShowLoaderError(Localization.Get("backend.connecting.failed"));
		}
		IsUserTriggeredLogout = false;
		ConnectionStatus = ConnectionStatus.Disconnected;
		BackendEvents.BackendConnectionChanged(ConnectionStatus.Disconnected);
	}

	public override async Task OnPlayerSkipped(PlayerSkippedViewModel playerSkippedViewModel)
	{
		await new WaitForUpdate();
		Log($"User {playerSkippedViewModel.SkippedUserId} got skipped ({playerSkippedViewModel.SkipCount}/{playerSkippedViewModel.MaxSkipCount}) from game with GameId: {playerSkippedViewModel.GameId}", (LogLevel)2);
		if (playerSkippedViewModel.SkipCount < playerSkippedViewModel.MaxSkipCount)
		{
			BackendEvents.ReceivedPlayerSkipped(playerSkippedViewModel);
		}
	}

	public override async Task OnLobbyInvitation(LobbyGameViewModel lobby)
	{
		Log($"OnLobbyInvitation: {lobby.Id}", (LogLevel)1);
		await new WaitForUpdate();
		GameManager.GetLobbyManager().AddOrUpdateLobby(lobby);
		if (UIManager.Instance.type == UIManager.Type.StartMenu && !PopupManager.IsPopupShowing<LobbyPopup>() && !PopupManager.IsPopupShowing<GameInfoPopup>())
		{
			LobbyPopup lobbyPopup = PopupManager.GetLobbyPopup();
			lobbyPopup.SetData(lobby);
			lobbyPopup.Show();
			if (UIManager.Instance.CurrentScreen == UIConstants.Screens.MultiplayerScreen)
			{
				((MultiplayerScreen)UIManager.Instance.GetScreen(UIConstants.Screens.MultiplayerScreen)).Reload();
			}
		}
		BackendEvents.ReceivedLobbyInvitation(lobby);
	}

	public override async Task OnLobbyUpdated(LobbyGameViewModel lobby)
	{
		await new WaitForUpdate();
		Log($"OnLobbyUpdated: {lobby.Id}, reason: {lobby.UpdatedReason}", (LogLevel)1);
		ParticipatorViewModel localParticipator = LobbyManager.GetLocalParticipator(lobby);
		bool flag = lobby.UpdatedReason == LobbyUpdatedReason.Deleted;
		if (lobby.UpdatedReason == LobbyUpdatedReason.PlayersKicked && localParticipator.InvitationState == PlayerInvitationState.Done)
		{
			flag = true;
		}
		if (flag && lobby.OwnerId != AccountManager.PlayerAccountId)
		{
			UIButtonBase.ButtonAction callback = null;
			string header = Localization.Get("onlineview.lobby.kicked.title");
			string description = Localization.Get("onlineview.lobby.kicked", lobby.Name);
			if (lobby.UpdatedReason == LobbyUpdatedReason.Deleted)
			{
				header = Localization.Get("onlineview.lobby.closed.title");
				description = Localization.Get("onlineview.lobby.closed", lobby.Name);
			}
			if (UIManager.Instance.CurrentScreen == UIConstants.Screens.TribeSelector)
			{
				TribeSelectorScreen tribeSelector = UIManager.Instance.GetCurrentScreen() as TribeSelectorScreen;
				if (tribeSelector.GetLobbyId() == lobby.Id)
				{
					tribeSelector.onCancel = null;
					callback = delegate
					{
						tribeSelector.OnBack();
					};
				}
			}
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = header;
			basicPopup.Description = description;
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, callback)
			};
			basicPopup.Show();
		}
		if (GameManager.GetLobbyManager().TryGetCachedLobby(lobby.Id, out var lobbyGameViewModel) && localParticipator != null && localParticipator.InvitationState == PlayerInvitationState.Accepted && lobbyGameViewModel.OwnerId != AccountManager.PlayerAccountId && lobby.OwnerId == AccountManager.PlayerAccountId)
		{
			NotificationManager.Notify(Localization.Get("onlineview.lobby.newowner", lobby.Name));
		}
		BackendEvents.ReceivedLobbyUpdate(lobby);
	}

	public override Task OnTournamentUpdated(TournamentViewModel tournament, TournamentUpdateReason reason)
	{
		Log($"OnTournamentUpdated, tournamentId {tournament.Id}, reason: {reason}", (LogLevel)1);
		if (reason == TournamentUpdateReason.CheckinOpen && tournament.PersonalViewModel.HasSignedUp)
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("onlineview.tournament.notification.header");
			basicPopup.Description = Localization.Get("onlineview.tournament.notification.confirmation", tournament.Name);
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			basicPopup.Show();
		}
		return Task.CompletedTask;
	}
}
