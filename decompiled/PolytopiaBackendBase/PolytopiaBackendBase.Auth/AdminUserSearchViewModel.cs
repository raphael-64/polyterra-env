using System.Collections.Generic;

namespace PolytopiaBackendBase.Auth;

public class AdminUserSearchViewModel : IServerResponseData
{
	public List<UserInAdminPanelViewModel> FoundUsers { get; set; }
}
