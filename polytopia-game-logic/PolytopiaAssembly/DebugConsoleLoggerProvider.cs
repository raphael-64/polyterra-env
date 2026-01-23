using System;
using Microsoft.Extensions.Logging;

public class DebugConsoleLoggerProvider : ILoggerProvider, IDisposable
{
	public ILogger CreateLogger(string categoryName)
	{
		return (ILogger)(object)new DebugConsoleLogger(categoryName);
	}

	public void Dispose()
	{
	}
}
