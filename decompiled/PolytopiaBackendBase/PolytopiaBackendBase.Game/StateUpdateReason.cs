namespace PolytopiaBackendBase.Game;

public enum StateUpdateReason
{
	Unknown = 0,
	MetadataUpdated = 1,
	GameCreated = 2,
	InvitationResponse = 3,
	ValidPickTribe = 4,
	InvalidPickTribe = 5,
	ValidStartGame = 6,
	InvalidStartGame = 7,
	PlayerResigned = 8,
	PlayerKicked = 9,
	ValidCommand = 10,
	InvalidCommand = 11,
	SetParticipationStateToDone = 12,
	GameJoined = 13,
	ReplayFinished = 15,
	PlayerRemindedToPlay = 16,
	StateReset = 17,
	Reconnect = 18,
	LobbyGameStarted = 19,
	GameEnded = 20
}
