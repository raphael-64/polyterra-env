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
    int commandCount = 0;
    int errorCount = 0;

    Console.Error.WriteLine("Polyterra Environment Server started");
    Console.Error.WriteLine("Ready to accept JSON commands on stdin");

    while (true)
    {
        string line = null;
        try
        {
            line = Console.ReadLine();

            if (string.IsNullOrEmpty(line))
            {
                Console.Error.WriteLine($"Server received EOF after {commandCount} commands ({errorCount} errors)");
                break;
            }

            commandCount++;

            // Process command
            string response = bridge.ProcessCommand(line);

            // Write response to stdout
            Console.WriteLine(response);
            Console.Out.Flush();
        }
        catch (Exception ex)
        {
            errorCount++;
            // Log the error with context but DON'T crash - return error response instead
            Console.Error.WriteLine($"[ERROR #{errorCount}] Command #{commandCount}: {ex.Message}");
            Console.Error.WriteLine($"  Input: {(line?.Length > 200 ? line.Substring(0, 200) + "..." : line)}");
            Console.Error.WriteLine($"  Stack: {ex.StackTrace}");

            // Return error response to Python instead of crashing
            var errorResponse = new System.Text.Json.Nodes.JsonObject
            {
                ["error"] = ex.Message,
                ["error_type"] = ex.GetType().Name,
                ["command_count"] = commandCount
            };
            Console.WriteLine(errorResponse.ToJsonString());
            Console.Out.Flush();
            // CONTINUE instead of break - don't crash the server!
        }
    }

    Console.Error.WriteLine($"Environment Server shutting down after {commandCount} commands ({errorCount} errors)");
}
