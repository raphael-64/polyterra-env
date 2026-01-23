using System.Collections.Generic;

namespace PolytopiaBackendBase;

public class ServerResponseList<T> where T : IServerResponseData
{
	public bool Success { get; set; }

	public ErrorCode ErrorCode { get; set; }

	public string ErrorMessage { get; set; }

	public string InnerMessage { get; set; }

	public List<T> Data { get; set; }

	public ServerResponseList()
	{
	}

	public ServerResponseList(List<T> data)
	{
		Data = data;
		Success = true;
	}

	public ServerResponseList(ErrorCode errorCode, string errorMessage, string innerMessage = null)
	{
		ErrorCode = errorCode;
		ErrorMessage = errorMessage;
		InnerMessage = innerMessage;
		Success = false;
	}
}
