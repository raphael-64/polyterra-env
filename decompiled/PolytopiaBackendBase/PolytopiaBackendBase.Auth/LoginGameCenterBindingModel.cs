namespace PolytopiaBackendBase.Auth;

public class LoginGameCenterBindingModel : LoginLegacyPlatformBindingModel
{
	public string GameCenterId { get; set; }

	public string TeamPlayerId { get; set; }

	public string Username { get; set; }
}
