namespace PolytopiaBackendBase.Notifications;

public class NotificationMessage
{
	public string Body { get; set; }

	public string LocalizationKey { get; set; }

	public string[] LocalizationVariables { get; set; }

	public string Title { get; set; }

	public string Url { get; set; }

	public int? Badge { get; set; }
}
