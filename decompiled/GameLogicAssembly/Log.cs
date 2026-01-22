using System;

public static class Log
{
    public static void Error(string message, params object[] args)
    {
        Console.Error.WriteLine("[ERROR] " + string.Format(message, args));
    }

    public static void Warning(string message, params object[] args)
    {
        Console.Error.WriteLine("[WARN] " + string.Format(message, args));
    }

    public static void Info(string message, params object[] args)
    {
        Console.Error.WriteLine("[INFO] " + string.Format(message, args));
    }

    public static void Debug(string message, params object[] args)
    {
        // Suppress debug logs for now
    }

    public static void Verbose(string message, params object[] args)
    {
        // Suppress verbose logs for now
    }

    public static void Learn(string message, params object[] args)
    {
        // Suppress learn logs for now
    }

    public static void Spam(string message, params object[] args)
    {
        // Suppress spam logs for now
    }
}
