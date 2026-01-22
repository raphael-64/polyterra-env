using PolytopiaBackendBase;

public class StartViewModel : IServerResponseData
{
	public int UnseenNewsItemCount { get; set; }

	public int ActionableGamesCount { get; set; }
}
