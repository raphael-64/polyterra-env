using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Common;

namespace PolytopiaBackendBase.Game;

public interface IGameClient
{
	Task OnNotify(string message);

	Task OnCommand(CommandArrayViewModel model);

	Task OnGameStateUpdated(GameStateViewModel model, StateUpdateReason pushReason);

	Task OnGameSummaryUpdated(GameSummaryViewModel model, StateUpdateReason pushReason);

	Task OnGameDeleted(Guid gameId);

	Task OnMatchmakingGameUpdated(long gameId, MatchmakingUpdateReason pushReason);

	Task OnPlayerResigned(PlayerResignedViewModel playerResignedViewModel);

	Task OnPlayerSkipped(PlayerSkippedViewModel playerSkippedViewModel);

	Task OnInvitation(Guid gameId);

	Task OnLobbyInvitation(LobbyGameViewModel lobby);

	Task OnGameReadyToStart(Guid gameId);

	Task OnFriendsUpdated(List<PolytopiaFriendViewModel> friends);

	Task OnPlayerStatusUpdated(Guid playerId, PlayerStatus status);

	Task OnLobbyUpdated(LobbyGameViewModel lobby);

	Task OnUserUpdated(PolytopiaUserViewModel user);

	Task OnFriendRequestReceived(Guid friendUserId);

	Task OnFriendRequestAccepted(Guid friendUserId);

	Task OnActionableGamesUpdated(int count);

	Task OnTournamentUpdated(TournamentViewModel tournament, TournamentUpdateReason reason);
}
