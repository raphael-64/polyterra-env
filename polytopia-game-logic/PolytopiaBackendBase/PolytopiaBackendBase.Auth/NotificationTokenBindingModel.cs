using PolytopiaBackendBase.Common;

namespace PolytopiaBackendBase.Auth;

public class NotificationTokenBindingModel
{
	public string Token { get; set; }

	public NotificationBackendType NotificationBackend { get; set; }
}
