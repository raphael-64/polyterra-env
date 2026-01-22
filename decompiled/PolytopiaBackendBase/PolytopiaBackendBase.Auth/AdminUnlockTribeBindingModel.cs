using System;

namespace PolytopiaBackendBase.Auth;

public class AdminUnlockTribeBindingModel
{
	public Guid UserId { get; set; }

	public int TribeId { get; set; }
}
