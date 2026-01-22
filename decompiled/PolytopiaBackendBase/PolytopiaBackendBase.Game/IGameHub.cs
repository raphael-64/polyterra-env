using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game.BindingModels;

namespace PolytopiaBackendBase.Game;

public interface IGameHub
{
	Task<ServerResponse<GameViewModel>> CreateGame(CreateGameBindingModel model);

	Task<ServerResponse<MatchmakingSubmissionViewModel>> SubmitMatchmakingRequest(SubmitMatchmakingBindingModel model);

	Task<ServerResponse<ResponseViewModel>> RespondToInvitation(RespondToInvitationBindingModel model);

	Task<ServerResponse<GameViewModel>> GetGameViewModelByIdAsync(Guid gameId);

	Task<ServerResponse<GameViewModel>> GetGameSpectateViewModelByIdAsync(Guid gameId);

	Task<ServerResponse<GameSummaryViewModel>> GetGameSummaryViewModelByIdAsync(Guid gameId);

	Task<ServerResponseList<GameViewModel>> GetGamesByOwner();

	Task<ServerResponseList<GameViewModel>> GetGamesByParticipation();

	Task<ServerResponseList<GameSummaryViewModel>> GetGameSummariesByOwner();

	Task<ServerResponseList<GameSummaryViewModel>> GetGameSummariesByParticipation();

	Task<ServerResponse<GameListingViewModel>> GetGameListings();

	Task<ServerResponse<GameListingViewModel>> GetGameListingsV2();

	Task<ServerResponse<GameListingViewModel>> GetGameListingsV3();

	Task<ServerResponse<MatchmakingGameSummaryViewModel>> GetMatchmakingGameById(long gameId);

	Task<ServerResponse<ResponseViewModel>> TrackFinishedGame(FinishedGameBindingModel model);

	Task<ServerResponse<ResponseViewModel>> SubscribeToGame(SubscribeToGameBindingModel model);

	Task<ServerResponse<ResponseViewModel>> SubscribeToGameSummaries(SubscribeToGameSummariesBindingModel model);

	Task<ServerResponse<ResponseViewModel>> SubscribeToParticipatingGameSummaries();

	Task<ServerResponse<ResponseViewModel>> SubscribeToFriends();

	Task<ServerResponse<ResponseViewModel>> UnsubscribeToGame(UnsubscribeToGameBindingModel model);

	Task<ServerResponse<ResponseViewModel>> UnsubscribeToParticipatingGameSummaries();

	Task<ServerResponse<GameViewModel>> JoinGame(Guid gameId);

	Task<ServerResponse<GameViewModel>> SpectateGame(Guid gameId);

	Task<ServerResponse<ResponseViewModel>> SendCommand(SendCommandBindingModel model);

	Task<ServerResponse<ResponseViewModel>> PickTribe(PickTribeBindingModel model);

	Task<ServerResponse<ResponseViewModel>> StartGame(StartGameBindingModel model);

	Task<ServerResponse<ResponseViewModel>> UploadHighscores(UploadHighscoresBindingModel model);

	Task<ServerResponseList<HighscoreViewModel>> GetHighscores(TribeHighscoresBindingModel model);

	Task<ServerResponse<ResponseViewModel>> UploadTribeRating(UploadTribeRatingBindingModel model);

	Task<ServerResponse<TribeRatingsViewModel>> GetTribeRatings();

	Task<ServerResponse<ResponseViewModel>> ClearTribeRatings();

	Task<ServerResponse<ResponseViewModel>> UploadNumSingleplayerGames(UploadNumSingleplayerGamesBindingModel model);

	Task<ServerResponse<ResponseViewModel>> Resign(ResignBindingModel model);

	Task<ServerResponse<ResponseViewModel>> SaveGame(SaveGameBindingModel model);

	Task<ServerResponseList<GameSummaryViewModel>> GetSavedGames();

	Task<ServerResponse<ResponseViewModel>> LeaveMatchmakingGame(long matchmakingGameId);

	Task<ServerResponse<ResponseViewModel>> LeaveAllMatchmakingGames();

	Task<ServerResponse<ResponseViewModel>> Kick(KickBindingModel model);

	Task<ServerResponse<ResponseViewModel>> SkipTurn(SkipTurnBindingModel model);

	Task<ServerResponse<ResponseViewModel>> RemindPlayer(RemindPlayerBindingModel model);

	Task<ServerResponse<ResponseViewModel>> UpdateAvatar(AvatarBindingModel model);

	Task<ServerResponse<ResponseViewModel>> SetParticipationDone(SetParticipationDoneBindingModel model);

	Task<ServerResponse<ResponseViewModel>> SetParticipationFailedParse(SetParticipationHasFailedParseBindingModel model);

	Task<ServerResponseList<PolytopiaUserViewModel>> GetUserViewModelsByIds(List<Guid> userIds);

	Task<ServerResponseList<PolytopiaFriendViewModel>> GetFriends();

	Task<ServerResponse<PlayersStatusesResponse>> GetFriendsStatuses();

	Task<ServerResponse<PlayersStatusesResponse>> SubscribeToGameParticipantsStatuses(SubscribeToGameParticipantsStatusesBindingModel model);

	Task<ServerResponse<ResponseViewModel>> SendFriendRequest(FriendRequestBindingModel model);

	Task<ServerResponse<ResponseViewModel>> AcceptFriendRequest(FriendRequestBindingModel model);

	Task<ServerResponse<ResponseViewModel>> RemoveFriend(FriendRequestBindingModel model);

	Task<ServerResponseList<PolytopiaFriendViewModel>> SearchUsers(SearchUsersBindingModel model);

	Task<ServerResponseList<GameSummaryViewModel>> GetRecentGames(RecentGamesBindingModel model);

	Task<ServerResponse<LobbyGameViewModel>> CreateLobby(CreateLobbyBindingModel model);

	Task<ServerResponse<BoolResponseViewModel>> DeleteLobby(DeleteLobbyBindingModel model);

	Task<ServerResponse<BoolResponseViewModel>> ModifyPlayersInLobby(ModifyPlayersInLobbyBindingModel model);

	Task<ServerResponse<BoolResponseViewModel>> SubscribeToLobby(SubscribeToLobbyBindingModel model);

	Task<ServerResponse<LobbyGameViewModel>> GetLobby(GetLobbyBindingModel model);

	Task<ServerResponse<LobbyGameViewModel>> RespondToLobbyInvitation(RespondToLobbyInvitation model);

	Task<ServerResponse<LobbyGameViewModel>> ActivateLobbyLinkInvitations(ActivateLobbyLinkInvitationsBindingModel model);

	Task<ServerResponse<BoolResponseViewModel>> ChangeTribeInLobby(ChangeTribeInLobbyModel model);

	Task<ServerResponse<GuidResponseViewModel>> GetInLobbyId();

	Task<ServerResponse<GetLobbyInvitationsViewModel>> GetLobbiesInvitations();

	Task<ServerResponse<BoolResponseViewModel>> UpdateLobbySettings(UpdateLobbySettingsBindingModel model);

	Task<ServerResponse<LobbyGameViewModel>> StartLobbyGame(StartLobbyBindingModel model);

	Task<ServerResponse<BoolResponseViewModel>> LeaveLobby(Guid lobbyId);

	[Obsolete("This method is obsolete since 2021-01-18, Game version 30, semantic version 2.0.37 is the next version as to this date. Call SetNotificationToken instead.", true)]
	Task<ServerResponse<ResponseViewModel>> SetFCMToken(string token);

	Task<ServerResponse<ResponseViewModel>> SetNotificationToken(NotificationTokenBindingModel model);

	Task<ServerResponse<TournamentViewModel>> GetTournament(Guid id);

	Task<ServerResponse<TournamentListViewModel>> GetTournamentList(GetTournamentListBindingModel model);

	Task<ServerResponse<RecurringLadderViewModel>> GetRecurringLadder();

	Task<ServerResponse<LadderViewModel>> GetLadderDetails(Guid ladderId);

	Task<ServerResponse<LadderPlacementsViewModel>> GetLadderPlacements(Guid ladderId);

	Task<ServerResponse<LadderMatchesViewModel>> GetLadderMatches(GetLadderMatchesBindingModel bindingModel);

	Task<ServerResponse<ChallengermodeConnectionStatus>> GetChallengermodeConnectionStatus();
}
