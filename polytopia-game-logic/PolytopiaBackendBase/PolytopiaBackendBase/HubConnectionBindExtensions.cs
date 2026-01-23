using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace PolytopiaBackendBase;

public static class HubConnectionBindExtensions
{
	public static IDisposable BindMethod<T>(this HubConnection connection, Func<T, Task> boundMethod)
	{
		return HubConnectionExtensions.On<T>(connection, boundMethod.Method.Name, boundMethod);
	}

	public static IDisposable BindMethod<T, T1>(this HubConnection connection, Func<T, T1, Task> boundMethod)
	{
		return HubConnectionExtensions.On<T, T1>(connection, boundMethod.Method.Name, boundMethod);
	}
}
