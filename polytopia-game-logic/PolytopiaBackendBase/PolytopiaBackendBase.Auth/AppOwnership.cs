namespace PolytopiaBackendBase.Auth;

public class AppOwnership
{
	public bool OwnsApp { get; set; }

	public bool Permanent { get; set; }

	public string TimeStamp { get; set; }

	public ulong OwnerSteamId { get; set; }

	public bool SiteLicence { get; set; }
}
