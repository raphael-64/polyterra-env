namespace PolytopiaBackendBase.Game;

public enum LobbyUpdatedReason
{
	Unknown,
	Created,
	Get,
	UpdatedSettings,
	ActivatedLinkInvitations,
	PlayerRespondedToInvitation,
	PlayerChangedTribe,
	PlayerLeftDueToDisconnect,
	Deleted,
	PlayerLeftByRequest,
	PlayersInvited,
	PlayersKicked
}
