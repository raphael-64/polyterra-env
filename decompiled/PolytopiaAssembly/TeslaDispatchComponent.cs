using System;
using System.Collections.Concurrent;
using UnityEngine;

internal class TeslaDispatchComponent : MonoBehaviour
{
	private static ConcurrentQueue<Action> _taskQueue = new ConcurrentQueue<Action>();

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void Initialize()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Object.DontDestroyOnLoad((Object)(object)new GameObject("TeslaDispatcher").AddComponent<TeslaDispatchComponent>());
	}

	public static void Enqueue(Action action)
	{
		_taskQueue.Enqueue(action);
	}

	private void Update()
	{
		Action result;
		while (_taskQueue.TryDequeue(out result))
		{
			result();
		}
	}
}
