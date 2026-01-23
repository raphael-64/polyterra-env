namespace PolytopiaBackendBase;

public class ServerResponse<T> where T : IServerResponseData
{
	public bool Success { get; set; }

	public ErrorCode ErrorCode { get; set; }

	public string ErrorMessage { get; set; }

	public string InnerMessage { get; set; }

	public T Data { get; set; }

	public ServerResponse()
	{
	}

	public ServerResponse(T data)
	{
		Data = data;
		Success = true;
	}

	public ServerResponse(ErrorCode errorCode, string errorMessage, string innerMessage = null)
	{
		ErrorCode = errorCode;
		ErrorMessage = errorMessage;
		InnerMessage = InnerMessage;
		Success = false;
	}
}
