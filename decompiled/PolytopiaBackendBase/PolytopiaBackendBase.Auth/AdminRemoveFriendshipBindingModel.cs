using System;

namespace PolytopiaBackendBase.Auth;

public class AdminRemoveFriendshipBindingModel
{
	public Guid UserId { get; set; }

	public Guid FriendId { get; set; }
}
