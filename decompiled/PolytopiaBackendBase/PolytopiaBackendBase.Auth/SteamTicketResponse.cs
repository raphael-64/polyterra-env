namespace PolytopiaBackendBase.Auth;

public class SteamTicketResponse
{
	public SteamTicketResponseData Params { get; set; }

	public SteamTicketResponseError Error { get; set; }
}
