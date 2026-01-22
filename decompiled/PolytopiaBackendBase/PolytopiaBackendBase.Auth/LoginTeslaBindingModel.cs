namespace PolytopiaBackendBase.Auth;

public class LoginTeslaBindingModel : LoginBaseBindingModel
{
	public string AuthToken { get; set; }

	public string UserName { get; set; }

	public string UserId { get; set; }

	public TeslaEnvironment Environment { get; set; }
}
