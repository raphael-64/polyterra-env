using System;

namespace PolytopiaBackendBase.Auth;

public class AdminUnlockSkinBindingModel
{
	public Guid UserId { get; set; }

	public int SkinId { get; set; }
}
