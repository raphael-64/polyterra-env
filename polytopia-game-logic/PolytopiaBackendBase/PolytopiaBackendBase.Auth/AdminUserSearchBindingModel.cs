using System;

namespace PolytopiaBackendBase.Auth;

public class AdminUserSearchBindingModel
{
	public Guid UserId { get; set; }

	public string DisplayName { get; set; }
}
