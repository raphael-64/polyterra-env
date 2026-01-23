using PolytopiaBackendBase.Common;

namespace PolytopiaBackendBase.Auth;

public class ClientGameVersionViewModel
{
	public Platform Platform { get; set; }

	public string DeviceId { get; set; }

	public int GameVersion { get; set; }
}
