// Simple CLI test app for NowProto DVC

using System.CommandLine;

using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient.Cli;

static class Program
{
    static async Task<int> Main(params string[] args)
    {
        var rootCommand = new RootCommand("CLI test app for NowProto DVC");

        var pipeNameOption = new Option<string>("--pipe")
        {
            Description = "DVC transport pipe name",
            Required = true
        };
        var timeoutSeconds = new Option<uint?>("--timeout")
        {
            Description = "DVC transport timeout in seconds",
            DefaultValueFactory = (_) => null
        };


        rootCommand.Add(pipeNameOption);
        rootCommand.Add(timeoutSeconds);

        var parsed = rootCommand.Parse(args);

        var timeoutParsed = parsed.GetValue(timeoutSeconds);
        TimeSpan? timeout = timeoutParsed.HasValue ? TimeSpan.FromSeconds(timeoutParsed.Value) : null;
        var pipeNameValue = parsed.GetValue<string>(pipeNameOption)!;

        Console.WriteLine("Connecting to the NowProto DVC...");
        var transport = await NowClientPipeTransport.Connect(
            pipeNameValue,
            timeout
        );

        Console.WriteLine($"Pipe transport has been connected via {pipeNameValue}");

        var client = await NowClient.Connect(transport);

        Console.WriteLine($"Negotiated NowProto client version: {client.Capabilities.Version}");


        bool repeat;
        do
        {
            repeat = true;

            Console.Write("Operation (msg/run/logoff/lock/exit): ");
            var operation = Console.ReadLine()?.Trim().ToLowerInvariant() ?? string.Empty;

            switch (operation)
            {
                case "run":
                    Console.Write("ShellExecute command: ");
                    var command = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (string.IsNullOrEmpty(command))
                    {
                        Console.WriteLine("Command cannot be empty.");
                        continue;
                    }

                    var runParams = new ExecRunParams(command);
                    await client.ExecRun(runParams);
                    Console.WriteLine("OK");
                    break;
                case "msg":
                    Console.Write("Message box text: ");
                    var message = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("Message cannot be empty.");
                        continue;
                    }
                    var messageBoxParams = new MessageBoxParams(message)
                        .Style(NowMsgSessionMessageBoxReq.MessageBoxStyle.YesNo)
                        .Title("DVC test");

                    await client.SessionMessageBoxNoResponse(messageBoxParams);
                    Console.WriteLine("OK");
                    break;
                case "logoff":
                    await client.SessionLogoff();
                    Console.WriteLine("OK");
                    repeat = false;
                    break;
                case "lock":
                    await client.SessionLock();
                    Console.WriteLine("OK");
                    break;
                case "exit":
                    Console.WriteLine("Exiting...");
                    repeat = false;
                    break;
                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        } while (repeat);

        Console.WriteLine("Disconnecting from the NowProto DVC...");

        return 0;
    }
}