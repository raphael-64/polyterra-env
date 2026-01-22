using System;
using System.Threading.Tasks;

namespace PolytopiaBackendBase;

public static class ServerResponseHelpers
{
	public static Action<Exception> OnError;

	public static async Task<ServerResponse<T>> ClientErrorHandledResponse<T>(ServerResponseDelegate<T> serverResponseDelegate) where T : IServerResponseData, new()
	{
		try
		{
			return await serverResponseDelegate();
		}
		catch (Exception obj)
		{
			ServerResponse<T> result = new ServerResponse<T>(ErrorCode.ConnectionError, "Connection to server failed");
			OnError?.Invoke(obj);
			return result;
		}
	}

	public static async Task<ServerResponseList<T>> ClientErrorHandledResponse<T>(ServerResponseListDelegate<T> serverResponseListDelegate) where T : IServerResponseData, new()
	{
		try
		{
			return await serverResponseListDelegate();
		}
		catch (Exception obj)
		{
			ServerResponseList<T> result = new ServerResponseList<T>(ErrorCode.ConnectionError, "Connection to server failed");
			OnError?.Invoke(obj);
			return result;
		}
	}

	public static async Task<ServerResponse<T>> ClientHttpErrorHandledResponse<T>(DataHttpResponseDelegate<T> responseDelegate) where T : IServerResponseData, new()
	{
		try
		{
			DataHttpResponse<ServerResponse<T>> dataHttpResponse = await responseDelegate();
			if (dataHttpResponse.Message.IsSuccessStatusCode && dataHttpResponse.Data != null)
			{
				return dataHttpResponse.Data;
			}
			return new ServerResponse<T>((ErrorCode)dataHttpResponse.Message.StatusCode, dataHttpResponse.Message.ReasonPhrase ?? "Connection to server failed");
		}
		catch (Exception obj)
		{
			ServerResponse<T> result = new ServerResponse<T>(ErrorCode.ConnectionError, "Connection to server failed");
			OnError?.Invoke(obj);
			return result;
		}
	}

	public static async Task<ServerResponseList<T>> ClientHttpErrorHandledResponse<T>(DataHttpResponseListDelegate<T> responseListDelegate) where T : IServerResponseData, new()
	{
		try
		{
			DataHttpResponse<ServerResponseList<T>> dataHttpResponse = await responseListDelegate();
			if (dataHttpResponse.Message.IsSuccessStatusCode && dataHttpResponse.Data != null)
			{
				return dataHttpResponse.Data;
			}
			return new ServerResponseList<T>((ErrorCode)dataHttpResponse.Message.StatusCode, dataHttpResponse.Message.ReasonPhrase ?? "Connection to server failed");
		}
		catch (Exception obj)
		{
			ServerResponseList<T> result = new ServerResponseList<T>(ErrorCode.ConnectionError, "Connection to server failed");
			OnError?.Invoke(obj);
			return result;
		}
	}
}
