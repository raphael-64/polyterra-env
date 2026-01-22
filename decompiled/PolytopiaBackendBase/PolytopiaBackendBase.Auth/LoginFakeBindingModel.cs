namespace PolytopiaBackendBase.Auth;

public class LoginFakeBindingModel : LoginLegacyPlatformBindingModel
{
	public string UserName { get; set; }

	public string ProposedUserId { get; set; }
}
