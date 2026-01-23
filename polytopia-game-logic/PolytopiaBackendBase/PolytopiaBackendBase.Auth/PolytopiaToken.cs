using System;
using Newtonsoft.Json;

namespace PolytopiaBackendBase.Auth;

public class PolytopiaToken : IServerResponseData
{
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public PolytopiaUserViewModel User { get; set; }

	public string JwtToken { get; set; }

	public DateTime? ExpiresAt { get; set; }

	public override string ToString()
	{
		return $"Polytopia User {User.PolytopiaId}: \n" + "  SteamId: " + User.SteamId + "\n  Token: " + JwtToken + "\n" + $"\tExpires at: {ExpiresAt:u}";
	}
}
