using System;

namespace Tesla;

public class Gamer
{
	public string nickname { get; private set; }

	public string uuid { get; private set; }

	public string vuid { get; private set; }

	public string authToken { get; private set; }

	public bool isOnline { get; private set; }

	public bool isLocal { get; private set; }

	public string notificationToken { get; private set; }

	public DateTime authTokenExpireTime { get; private set; }

	public Gamer(IntPtr gamer)
	{
		if (gamer == IntPtr.Zero)
		{
			throw new NullReferenceException("Gamer must have a non-zero handle");
		}
		nickname = NativeString.PointerToString(API.TeslaArcade_GamerInfo_GetNickname(gamer));
		uuid = NativeString.PointerToString(API.TeslaArcade_GamerInfo_GetUUID(gamer));
		vuid = NativeString.PointerToString(API.TeslaArcade_GamerInfo_GetVUID(gamer));
		authToken = NativeString.PointerToString(API.TeslaArcade_GamerInfo_GetAuthToken(gamer));
		isOnline = API.TeslaArcade_GamerInfo_IsOnline(gamer);
		isLocal = API.TeslaArcade_GamerInfo_IsLocal(gamer);
		authTokenExpireTime = DateTimeOffset.FromUnixTimeSeconds((long)API.TeslaArcade_GamerInfo_GetAuthTokenExpireTime(gamer)).UtcDateTime;
		notificationToken = NativeString.PointerToString(API.TeslaArcade_GamerInfo_GetNotificationToken(gamer));
	}
}
