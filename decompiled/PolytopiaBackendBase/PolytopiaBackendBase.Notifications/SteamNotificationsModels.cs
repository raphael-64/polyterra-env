using System.Collections.Generic;

namespace PolytopiaBackendBase.Notifications;

public static class SteamNotificationsModels
{
	public enum NotificationType
	{
		PickTribe,
		Invited,
		RandomMatchReady,
		ReadyToStart,
		Skipped,
		Kicked,
		Ended,
		YourTurn,
		NotYourTurn
	}

	public class MessageSection
	{
		public string Token { get; set; }

		public List<KeyValue> Variables { get; set; }
	}

	public class UserSection
	{
		public string SteamId { get; set; }

		public string State { get; set; }

		public MessageSection Title { get; set; }

		public MessageSection Message { get; set; }
	}

	public class KeyValue
	{
		public string Key { get; set; }

		public string Value { get; set; }

		public KeyValue(string key, string value)
		{
			Key = key;
			Value = value;
		}
	}

	public class RequestSteamNotificationsBindingModel
	{
		public string SteamId { get; set; }
	}

	public class RequestSteamNotificationsResponse : IServerResponseData
	{
		public bool Was_Created { get; set; }

		public bool Allow_Notifications { get; set; }
	}

	public class CreateSession
	{
		public string AppId { get; set; }

		public string Context { get; set; }

		public MessageSection Title { get; set; }

		public List<UserSection> Users { get; set; }
	}

	public class CreateSessionResponse
	{
		public Dictionary<string, string> Response { get; set; }
	}

	public class UpdateSession
	{
		public string AppId { get; set; }

		public string SessionId { get; set; }

		public MessageSection Title { get; set; }

		public List<UserSection> Users { get; set; }
	}

	public class DeleteSession
	{
		public string AppId { get; set; }

		public string SessionId { get; set; }
	}

	public class GetNotificationState
	{
		public string AppId { get; set; }

		public List<GetNotificationStateSession> Sessions { get; set; }
	}

	public class GetNotificationStateSession
	{
		public string SessionId { get; set; }

		public int Include_all_user_messages { get; set; }
	}

	public class GetNotificationStateResponse
	{
		public GetNotificationStateInnerResponse Response { get; set; }
	}

	public class GetNotificationStateInnerResponse
	{
		public GetNotificationStateResponseSession[] Sessions { get; set; }
	}

	public class GetNotificationStateResponseSession
	{
		public string AppId { get; set; }

		public string Context { get; set; }

		public string SessionId { get; set; }

		public MessageSection Title { get; set; }

		public ulong Time_created { get; set; }

		public ulong Time_updated { get; set; }

		public List<UserSection> User_status { get; set; }
	}
}
