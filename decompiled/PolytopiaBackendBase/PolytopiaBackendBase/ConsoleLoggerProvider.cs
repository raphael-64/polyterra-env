using System;
using Microsoft.Extensions.Logging;

namespace PolytopiaBackendBase;

public class ConsoleLoggerProvider : ILoggerProvider, IDisposable
{
	public class ConsoleLog : ILogger
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

		public ConsoleLog(string categoryName)
		{
			this.categoryName = categoryName;
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
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			switch ((int)logLevel)
			{
			case 0:
			case 1:
			case 2:
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine($"[{categoryName}], {eventId}, {state}, {exception}");
				break;
			case 3:
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"[{categoryName}], {eventId}, {state}, {exception}");
				break;
			case 4:
			case 5:
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"[{categoryName}], {eventId}, {state}, {exception}");
				break;
			case 6:
				break;
			}
		}
	}

	public ILogger CreateLogger(string categoryName)
	{
		return (ILogger)(object)new ConsoleLog(categoryName);
	}

	public void Dispose()
	{
	}
}
