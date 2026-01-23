using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public class UnityWebRequestAwaiter : INotifyCompletion
{
	private readonly UnityWebRequestAsyncOperation operation;

	public bool IsCompleted => ((AsyncOperation)operation).isDone;

	public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation operation)
	{
		this.operation = operation;
	}

	public void OnCompleted(Action continuation)
	{
		((AsyncOperation)operation).completed += delegate
		{
			continuation();
		};
	}

	public void GetResult()
	{
	}
}
