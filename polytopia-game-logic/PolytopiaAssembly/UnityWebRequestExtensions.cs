using UnityEngine.Networking;

public static class UnityWebRequestExtensions
{
	public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation operation)
	{
		return new UnityWebRequestAwaiter(operation);
	}
}
