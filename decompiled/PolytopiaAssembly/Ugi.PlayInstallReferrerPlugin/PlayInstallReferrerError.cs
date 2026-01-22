using System;

namespace Ugi.PlayInstallReferrerPlugin;

public class PlayInstallReferrerError
{
	public int ResponseCode { get; }

	public Exception Exception { get; }

	internal PlayInstallReferrerError(int responseCode, Exception exception)
	{
		ResponseCode = responseCode;
		Exception = exception;
	}
}
