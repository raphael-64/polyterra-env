using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;

namespace Tesla;

public class Arcade : NativeWrapper
{
	public static uint maxSupportedVersion => API.TeslaArcade_GetMaxSupportedVersion();

	public bool isDevelopmentBackend { get; private set; }

	private static void ThrowOnError(int result)
	{
		if (result != 0)
		{
			throw new InvalidOperationException(NativeString.PointerToString(API.TeslaArcade_ResultCode_AsString(result)));
		}
	}

	public Arcade(uint requestedVersion)
	{
		API.TeslaArcade_Initialize_Options options = new API.TeslaArcade_Initialize_Options
		{
			version = requestedVersion,
			spawnTickThread = true
		};
		ThrowOnError(API.TeslaArcade_Initialize(ref options));
		API.TeslaArcade_Backend backend = API.TeslaArcade_Backend.TeslaArcade_Backend_Development;
		ThrowOnError(API.TeslaArcade_GetBackendType(ref backend));
		if (backend == API.TeslaArcade_Backend.TeslaArcade_Backend_Development)
		{
			isDevelopmentBackend = true;
		}
		else
		{
			isDevelopmentBackend = false;
		}
	}

	protected override void ReleaseNativeResources()
	{
		API.TeslaArcade_Shutdown();
	}

	public override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public async Task<Gamer[]> RequestGamersByNicknames(string[] gamerNicknames)
	{
		await Task.Run(delegate
		{
			throw new NotImplementedException();
		});
		return null;
	}

	public async Task<Gamer[]> RequestGamersByUUIDs(string[] uuids)
	{
		await Task.Run(delegate
		{
			throw new NotImplementedException();
		});
		return null;
	}

	[MonoPInvokeCallback(typeof(API.TeslaArcade_RequestDefaultGamerInfo_Callback))]
	private static void RequestDefaultGamerInfo_Callback([In][Out] ref API.TeslaArcade_RequestDefaultGamerInfo_CallbackInfo info)
	{
		CallBackLUT<API.TeslaArcade_RequestDefaultGamerInfo_CallbackInfo>.Set(info.userData, info);
	}

	public async Task<Gamer> RequestDefaultGamer(bool showSignInUI)
	{
		API.TeslaArcade_RequestDefaultGamerInfo_Options options = new API.TeslaArcade_RequestDefaultGamerInfo_Options
		{
			showSignInUI = showSignInUI
		};
		AsyncResult<API.TeslaArcade_RequestDefaultGamerInfo_CallbackInfo> r = new AsyncResult<API.TeslaArcade_RequestDefaultGamerInfo_CallbackInfo>();
		IntPtr userData = CallBackLUT<API.TeslaArcade_RequestDefaultGamerInfo_CallbackInfo>.Add(r);
		ThrowOnError(API.TeslaArcade_RequestDefaultGamerInfo(ref options, userData, RequestDefaultGamerInfo_Callback));
		await r.WaitAsync();
		ThrowOnError(r.result.resultCode);
		return new Gamer(r.result.gamerInfo);
	}
}
