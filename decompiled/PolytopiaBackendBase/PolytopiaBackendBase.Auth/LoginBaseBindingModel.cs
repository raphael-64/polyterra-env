namespace PolytopiaBackendBase.Auth;

public abstract class LoginBaseBindingModel
{
	public int? GameVersion { get; set; }

	public string DeviceId { get; set; }

	public string BundleId { get; set; }

	public string SemanticVersion { get; set; }
}
