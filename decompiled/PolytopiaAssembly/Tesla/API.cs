using System;
using System.Runtime.InteropServices;

namespace Tesla;

internal static class API
{
	public struct TeslaArcade_RequestDefaultGamerInfo_CallbackInfo
	{
		public int resultCode;

		public IntPtr userData;

		public IntPtr gamerInfo;
	}

	public struct TeslaArcade_RequestDefaultGamerInfo_Options
	{
		[MarshalAs(UnmanagedType.I1)]
		public bool showSignInUI;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void TeslaArcade_RequestDefaultGamerInfo_Callback([In][Out] ref TeslaArcade_RequestDefaultGamerInfo_CallbackInfo info);

	public struct TeslaArcade_RequestGamerInfos_ByUUID_Options
	{
		public IntPtr uuids;

		public ulong count;
	}

	public struct TeslaArcade_RequestGamerInfos_ByNicknames_Options
	{
		public IntPtr nicknames;

		public ulong count;
	}

	public struct TeslaArcade_RequestGamerInfos_CallbackInfo
	{
		public int resultCode;

		public IntPtr userData;

		public IntPtr gamersArray;

		public ulong gamersArrayCount;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void TeslaArcade_RequestGamerInfos_Callback([In][Out] TeslaArcade_RequestGamerInfos_CallbackInfo info);

	public struct TeslaArcade_Initialize_Options
	{
		public uint version;

		[MarshalAs(UnmanagedType.I1)]
		public bool spawnTickThread;

		public IntPtr reserved;
	}

	public enum TeslaArcade_Backend
	{
		TeslaArcade_Backend_Development,
		TeslaArcade_Backend_Production,
		TeslaArcade_Backend_Manufacturing
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void AudioStateDelegate(IntPtr appClient, AudioState value, IntPtr userData);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void DeepLinkDelegate(IntPtr appClient, IntPtr userData, IntPtr value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void AppStateDelegate(IntPtr appClient, AppState value, IntPtr userData);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void VirtualKeyboardVisibilityDelegate(IntPtr clientPtr, [MarshalAs(UnmanagedType.I1)] bool visible, IntPtr userData);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void VirtualKeyboardTextStartedDelegate(IntPtr clientPtr, IntPtr sessionID, IntPtr text, int cursorPos, IntPtr userData);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void VirtualKeyboardTextUpdatedDelegate(IntPtr clientPtr, IntPtr sessionID, IntPtr text, int cursorPos, IntPtr userData);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void VirtualKeyboardTextSubmittedDelegate(IntPtr clientPtr, IntPtr sessionID, IntPtr text, IntPtr userData);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void VirtualKeyboardTextCanceledDelegate(IntPtr clientPtr, IntPtr sessionID, IntPtr userData);

	private const string arcadeLib = "libTeslaArcade.so";

	private const string appsLib = "libTeslaApps.so";

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern uint TeslaArcade_GetMaxSupportedVersion();

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern int TeslaArcade_RequestDefaultGamerInfo([In][Out] ref TeslaArcade_RequestDefaultGamerInfo_Options options, IntPtr userData, TeslaArcade_RequestDefaultGamerInfo_Callback callback);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern IntPtr TeslaArcade_GetGamerInfos_Interface();

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern int TeslaArcade_RequestGamerInfos_ByUUIDs([In][Out] ref TeslaArcade_RequestGamerInfos_ByUUID_Options options, IntPtr userData, TeslaArcade_RequestGamerInfos_Callback callback);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern int TeslaArcade_RequestGamerInfos_ByNicknames([In][Out] ref TeslaArcade_RequestGamerInfos_ByNicknames_Options options, IntPtr userData, TeslaArcade_RequestGamerInfos_Callback callback);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern IntPtr TeslaArcade_ResultCode_AsString(int r);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern int TeslaArcade_Initialize([In][Out] ref TeslaArcade_Initialize_Options options);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern bool TeslaArcade_Tick();

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern int TeslaArcade_GetBackendType([In][Out] ref TeslaArcade_Backend backend);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern void TeslaArcade_Shutdown();

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern IntPtr TeslaArcade_GamerInfo_GetNickname(IntPtr gamer);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern IntPtr TeslaArcade_GamerInfo_GetUUID(IntPtr gamer);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern IntPtr TeslaArcade_GamerInfo_GetVUID(IntPtr gamer);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern IntPtr TeslaArcade_GamerInfo_GetAuthToken(IntPtr gamer);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool TeslaArcade_GamerInfo_IsOnline(IntPtr gamer);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool TeslaArcade_GamerInfo_IsLocal(IntPtr gamer);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern ulong TeslaArcade_GamerInfo_GetAuthTokenExpireTime(IntPtr gamer);

	[DllImport("libTeslaArcade.so", CharSet = CharSet.Ansi)]
	public static extern IntPtr TeslaArcade_GamerInfo_GetNotificationToken(IntPtr gamer);

	[DllImport("libTeslaDevices.so")]
	public static extern IntPtr TeslaDevices_Create(string deviceNamePrefix, uint wantedDevices);

	[DllImport("libTeslaDevices.so")]
	public static extern void TeslaDevices_Destroy(IntPtr devices);

	[DllImport("libTeslaDevices.so")]
	public static extern IntPtr TeslaDevices_GetTouchScreen(IntPtr devices);

	[DllImport("libTeslaDevices.so")]
	public static extern IntPtr TeslaDevices_GetSteeringWheel(IntPtr devices);

	[DllImport("libTeslaDevices.so")]
	public static extern IntPtr TeslaDevices_GetLeftScrollWheel(IntPtr devices);

	[DllImport("libTeslaDevices.so")]
	public static extern IntPtr TeslaDevices_GetRightScrollWheel(IntPtr devices);

	[DllImport("libTeslaDevices.so")]
	public static extern uint TeslaDeviceDriverTouch_UpdateEvents(IntPtr driver, IntPtr buf);

	[DllImport("libTeslaDevices.so")]
	public static extern void TeslaDeviceDriverTouch_SetSize(IntPtr driver, int width, int height);

	[DllImport("libTeslaDevices.so")]
	public static extern uint TeslaDeviceDriverSteeringWheel_UpdateEvents(IntPtr driver, IntPtr buf);

	[DllImport("libTeslaDevices.so")]
	public static extern uint TeslaDeviceDriverScrollWheel_UpdateEvents(IntPtr driver, IntPtr buf);

	[DllImport("libTeslaDevices.so")]
	public static extern IntPtr TeslaTouchDeviceEventBuffer_Create();

	[DllImport("libTeslaDevices.so")]
	public static extern void TeslaTouchDeviceEventBuffer_Destroy(IntPtr buf);

	[DllImport("libTeslaDevices.so")]
	public static extern uint TeslaTouchDeviceEventBuffer_GetEventCount(IntPtr evbuf);

	[DllImport("libTeslaDevices.so")]
	public static extern IntPtr TeslaTouchDeviceEventBuffer_GetEventData(IntPtr evbuf);

	[DllImport("libTeslaDevices.so")]
	public static extern uint TeslaTouchDeviceEventBuffer_GetEventSize();

	[DllImport("libTeslaDevices.so")]
	public static extern IntPtr TeslaSteeringDeviceEventBuffer_Create();

	[DllImport("libTeslaDevices.so")]
	public static extern void TeslaSteeringDeviceEventBuffer_Destroy(IntPtr buf);

	[DllImport("libTeslaDevices.so")]
	public static extern uint TeslaSteeringDeviceEventBuffer_GetEventCount(IntPtr evbuf);

	[DllImport("libTeslaDevices.so")]
	public static extern IntPtr TeslaSteeringDeviceEventBuffer_GetEventData(IntPtr evbuf);

	[DllImport("libTeslaDevices.so")]
	public static extern uint TeslaSteeringDeviceEventBuffer_GetEventSize();

	[DllImport("libTeslaDevices.so")]
	public static extern IntPtr TeslaScrollDeviceEventBuffer_Create();

	[DllImport("libTeslaDevices.so")]
	public static extern void TeslaScrollDeviceEventBuffer_Destroy(IntPtr buf);

	[DllImport("libTeslaDevices.so")]
	public static extern uint TeslaScrollDeviceEventBuffer_GetEventCount(IntPtr evbuf);

	[DllImport("libTeslaDevices.so")]
	public static extern IntPtr TeslaScrollDeviceEventBuffer_GetEventData(IntPtr evbuf);

	[DllImport("libTeslaDevices.so")]
	public static extern uint TeslaScrollDeviceEventBuffer_GetEventSize();

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern IntPtr Tesla_AppsStatusToString(int status);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern int TeslaAppClient_Create(string appID, IntPtr appOut);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern int TeslaAppClient_GetStatus(IntPtr appClient);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern void TeslaAppClient_Free(IntPtr appClient);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern AudioState Tesla_GetAudioState(IntPtr appClient);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern int Tesla_SetAudioStateHandler(IntPtr appClient, AudioStateDelegate value, IntPtr userData);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern IntPtr Tesla_GetDeepLink(IntPtr appClient);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern int Tesla_SetDeepLinkHandler(IntPtr appClient, DeepLinkDelegate value, IntPtr userData);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern AppState Tesla_GetAppState(IntPtr appClient);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern int Tesla_SetAppStateHandler(IntPtr appClient, AppStateDelegate value, IntPtr userData);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern int Tesla_WebBrowser_ShowAndNavigateTo(IntPtr appClient, string url);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern int Tesla_VirtualKeyboard_Dismiss(IntPtr clientPtr, IntPtr sessionID);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern int Tesla_VirtualKeyboard_BeginSimpleTextEntry(IntPtr clientPtr, string text, int caretPos, IntPtr sessionID);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool Tesla_VirtualKeyboard_IsVisible(IntPtr clientPtr);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern int Tesla_VirtualKeyboard_SetVisibilityHandler(IntPtr clientPtr, VirtualKeyboardVisibilityDelegate value, IntPtr userData);

	[DllImport("libTeslaApps.so", CharSet = CharSet.Ansi)]
	public static extern int Tesla_VirtualKeyboard_SetSimpleTextHandlers(IntPtr clientPtr, VirtualKeyboardTextStartedDelegate startedDelegate, VirtualKeyboardTextUpdatedDelegate updatedDelegate, VirtualKeyboardTextSubmittedDelegate submittedDelegate, VirtualKeyboardTextCanceledDelegate canceledDelegate, IntPtr userData);
}
