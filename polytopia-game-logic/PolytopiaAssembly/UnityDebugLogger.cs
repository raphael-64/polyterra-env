using System;
using Microsoft.Extensions.Logging;
using UnityEngine;

public class UnityDebugLogger : ILogger
{
	private struct Scope<TState> : IDisposable
	{
		public TState State { get; }

		public Guid Id { get; }

		public Scope(TState state, Guid id)
		{
			State = state;
			Id = id;
		}

		public void Dispose()
		{
		}
	}

	private string categoryName;

	public UnityDebugLogger(string categoryName = null)
	{
		this.categoryName = categoryName ?? "";
	}

	public IDisposable BeginScope<TState>(TState state)
	{
		return new Scope<TState>(state, Guid.NewGuid());
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return true;
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected I4, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		switch ((int)logLevel)
		{
		case 0:
		case 1:
		case 2:
			Debug.Log((object)$"[{categoryName}], {eventId}, {state}");
			break;
		case 3:
			Debug.LogWarning((object)$"[{categoryName}], {eventId}, {state}, {exception}");
			break;
		case 4:
		case 5:
			Debug.LogError((object)$"[{categoryName}], {eventId}, {state}, {exception}");
			break;
		case 6:
			break;
		}
	}
}
