using System;

namespace PolytopiaBackendBase.Auth;

public class PolytopiaAdminToken : IServerResponseData
{
	public string JwtToken { get; set; }

	public DateTime? ExpiresAt { get; set; }
}
