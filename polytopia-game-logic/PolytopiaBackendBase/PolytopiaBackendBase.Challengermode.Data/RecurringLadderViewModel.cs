namespace PolytopiaBackendBase.Challengermode.Data;

public class RecurringLadderViewModel : IServerResponseData
{
	public LadderViewModel Current { get; set; }

	public LadderViewModel Upcoming { get; set; }
}
