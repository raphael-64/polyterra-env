using System;
using PolytopiaBackendBase.Game;

namespace PolytopiaBackendBase;

public class SubscribeToGameBindingModel
{
	public Guid GameId { get; set; }

	public SubscriptionType SubscriptionType { get; set; }
}
