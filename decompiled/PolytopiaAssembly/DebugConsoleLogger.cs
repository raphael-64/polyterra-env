using System;
using Microsoft.Extensions.Logging;

public class DebugConsoleLogger : ILogger
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

	public DebugConsoleLogger(string categoryName = null)
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
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected I4, but got Unknown
		string text = $"[{categoryName}], {state}, {exception}".Replace("{", "{{").Replace("}", "}}");
		switch ((int)logLevel)
		{
		case 0:
		case 1:
			Log.Verbose(text, Array.Empty<object>());
			break;
		case 2:
			Log.Info(text, Array.Empty<object>());
			break;
		case 3:
			Log.Warning(text, Array.Empty<object>());
			break;
		case 4:
			Log.Error(text, Array.Empty<object>());
			break;
		case 5:
			Log.Error(text, Array.Empty<object>());
			Log.Exception(exception);
			break;
		case 6:
			break;
		}
	}
}
