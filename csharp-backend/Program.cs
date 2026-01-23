// Check if running as environment server
if (args.Length > 0 && args[0] == "--env-server")
{
    RunEnvServer();
    return 0;
}

// Default: show usage
Console.WriteLine("Polyterra Backend Server");
Console.WriteLine();
Console.WriteLine("Usage:");
Console.WriteLine("  dotnet run -- --env-server    Start RL environment server");
Console.WriteLine();
Console.WriteLine("The environment server accepts JSON commands via stdin/stdout");
Console.WriteLine("and is used by the Python PettingZoo environment.");

return 0;

static void RunEnvServer()
{
    var bridge = new PolyterraEnvBridge(maxTurns: 100);

    Console.Error.WriteLine("Polyterra Environment Server started");
    Console.Error.WriteLine("Ready to accept JSON commands on stdin");

    while (true)
    {
        try
        {
            string line = Console.ReadLine();

            if (string.IsNullOrEmpty(line))
            {
                break;
            }

            // Process command
            string response = bridge.ProcessCommand(line);

            // Write response to stdout
            Console.WriteLine(response);
            Console.Out.Flush();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            break;
        }
    }

    Console.Error.WriteLine("Environment Server shutting down");
}
