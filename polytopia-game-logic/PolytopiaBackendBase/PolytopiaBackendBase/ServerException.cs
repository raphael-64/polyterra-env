using System;

namespace PolytopiaBackendBase;

public class ServerException : Exception
{
	public ErrorCode ErrorCode;

	public ServerException(ErrorCode errorCode, string message)
		: base(message)
	{
		ErrorCode = errorCode;
	}
}
