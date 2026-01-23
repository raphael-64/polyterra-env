using System;

namespace PolytopiaBackendBase.Game.BindingModels;

public class SubscribeToGameParticipantsStatusesBindingModel
{
	public bool Subscribe { get; set; }

	public Guid GameId { get; set; }

	public Guid[] ParticipantsToSubscribe { get; set; }

	public Guid[] Friends { get; set; }
}
