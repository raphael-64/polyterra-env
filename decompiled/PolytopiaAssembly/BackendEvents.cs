using System;
using System.Collections.Generic;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

public static class BackendEvents
{
	public delegate void OnBackendConnectionChangedEvent(ConnectionStatus status);

	public delegate void OnReceivedGameSummaryEvent(GameSummaryViewModel summary, StateUpdateReason reason);

	public delegate void OnReceivedGameDeletionEvent(Guid id);

	public delegate void OnReceivedMatchmakingGameUpdateEvent(long gameId, MatchmakingUpdateReason reason);

	public delegate void OnReceivedPlayerResignationEvent(PlayerResignedViewModel playerResignedViewModel);

	public delegate void OnReceivedPlayerSkippedEvent(PlayerSkippedViewModel playerskippedViewModel);

	public delegate void OnReceivedGameStateEvent(Guid gameId, StateUpdateReason reason);

	public delegate void OnReceivedLobbyInvitationEvent(LobbyGameViewModel lobby);

	public delegate void OnReceivedLobbyUpdateEvent(LobbyGameViewModel lobbyGameViewModel);

	public delegate void OnLobbiesUpdatedEvent();

	public delegate void OnRefreshFriendsEvent(List<PolytopiaFriendViewModel> friends);

	public delegate void OnRefreshUserEvent(PolytopiaUserViewModel user);

	public delegate void OnReceivedReminderEvent(Guid gameId);

	public delegate void OnGameSummariesUpdatedEvent();

	public delegate void OnPlayersStatusesUpdatedEvent();

	public delegate void OnAccountLinkedEvent(bool success);

	public static event OnBackendConnectionChangedEvent OnBackendConnectionChanged;

	public static event OnReceivedGameSummaryEvent OnReceivedGameSummary;

	public static event OnReceivedGameDeletionEvent OnReceivedGameDeletion;

	public static event OnReceivedMatchmakingGameUpdateEvent OnReceivedMatchmakingGameUpdate;

	public static event OnReceivedPlayerResignationEvent OnReceivedPlayerResignation;

	public static event OnReceivedPlayerSkippedEvent OnReceivedPlayerSkipped;

	public static event OnReceivedGameStateEvent OnReceivedGameState;

	public static event OnReceivedLobbyInvitationEvent OnReceivedLobbyInvitation;

	public static event OnReceivedLobbyUpdateEvent OnReceivedLobbyUpdate;

	public static event OnLobbiesUpdatedEvent OnLobbiesUpdated;

	public static event OnRefreshFriendsEvent OnRefreshFriends;

	public static event OnRefreshUserEvent OnRefreshUser;

	public static event OnReceivedReminderEvent OnReceivedReminder;

	public static event OnGameSummariesUpdatedEvent OnGameSummariesUpdated;

	public static event OnPlayersStatusesUpdatedEvent OnPlayersStatusesUpdated;

	public static event OnAccountLinkedEvent OnAccountLinked;

	public static void BackendConnectionChanged(ConnectionStatus status)
	{
		BackendEvents.OnBackendConnectionChanged?.Invoke(status);
	}

	public static void ReceivedGameSummary(GameSummaryViewModel summary, StateUpdateReason reason)
	{
		BackendEvents.OnReceivedGameSummary?.Invoke(summary, reason);
	}

	public static void ReceivedGameDeletion(Guid id)
	{
		BackendEvents.OnReceivedGameDeletion?.Invoke(id);
	}

	public static void ReceivedMatchmakingGameUpdate(long gameId, MatchmakingUpdateReason reason)
	{
		BackendEvents.OnReceivedMatchmakingGameUpdate?.Invoke(gameId, reason);
	}

	public static void ReceivedPlayerResignation(PlayerResignedViewModel playerResignedViewModel)
	{
		BackendEvents.OnReceivedPlayerResignation?.Invoke(playerResignedViewModel);
	}

	public static void ReceivedPlayerSkipped(PlayerSkippedViewModel playerskippedViewModel)
	{
		BackendEvents.OnReceivedPlayerSkipped?.Invoke(playerskippedViewModel);
	}

	public static void ReceivedGameState(Guid gameId, StateUpdateReason reason)
	{
		BackendEvents.OnReceivedGameState?.Invoke(gameId, reason);
	}

	public static void ReceivedLobbyInvitation(LobbyGameViewModel lobby)
	{
		BackendEvents.OnReceivedLobbyInvitation?.Invoke(lobby);
	}

	public static void ReceivedLobbyUpdate(LobbyGameViewModel lobbyGameViewModel)
	{
		BackendEvents.OnReceivedLobbyUpdate?.Invoke(lobbyGameViewModel);
	}

	public static void LobbiesUpdated()
	{
		BackendEvents.OnLobbiesUpdated?.Invoke();
	}

	public static void RefreshFriends(List<PolytopiaFriendViewModel> friends)
	{
		BackendEvents.OnRefreshFriends?.Invoke(friends);
	}

	public static void RefreshUser(PolytopiaUserViewModel user)
	{
		BackendEvents.OnRefreshUser?.Invoke(user);
	}

	public static void ReceivedReminder(Guid gameId)
	{
		BackendEvents.OnReceivedReminder?.Invoke(gameId);
	}

	public static void GameSummariesUpdated()
	{
		BackendEvents.OnGameSummariesUpdated?.Invoke();
	}

	public static void PlayersStatusesUpdated()
	{
		BackendEvents.OnPlayersStatusesUpdated?.Invoke();
	}

	public static void AccountLinked(bool success)
	{
		BackendEvents.OnAccountLinked?.Invoke(success);
	}
}
