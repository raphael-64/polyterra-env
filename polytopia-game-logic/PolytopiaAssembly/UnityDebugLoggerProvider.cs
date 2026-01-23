using System;
using Microsoft.Extensions.Logging;

public class UnityDebugLoggerProvider : ILoggerProvider, IDisposable
{
	public ILogger CreateLogger(string categoryName)
	{
		return (ILogger)(object)new UnityDebugLogger(categoryName);
	}

	public void Dispose()
	{
	}
}
