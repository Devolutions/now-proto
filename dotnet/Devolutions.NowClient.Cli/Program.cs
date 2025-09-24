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

            Console.Write("Operation (msg/run/pwsh/logoff/lock/rdm-run/rdm-version/exit): ");
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
                case "pwsh":
                    Console.Write("PowerShell command: ");
                    var psCommand = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (string.IsNullOrEmpty(psCommand))
                    {
                        Console.WriteLine("PowerShell command cannot be empty.");
                        continue;
                    }

                    await ExecutePowerShellCommand(client, psCommand);
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
                case "rdm-run":
                    await ExecuteRdmRunCommand(client);
                    break;
                case "rdm-version":
                    await ExecuteRdmVersionCommand(client);
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

    /// <summary>
    /// Executes a PowerShell command with IO redirection and prints output to console.
    /// </summary>
    private static async Task ExecutePowerShellCommand(NowClient client, string command)
    {
        const int NoOutputLimit = 1024 * 100; // 100KB
        const int DisplayProgressInterval = 1024 * 1024; // 1MB

        var completionSource = new TaskCompletionSource<int>();
        var stdoutSize = 0;
        var lastStdoutPrintSize = 0;
        var stderrSize = 0;
        var lastStderrPrintSize = 0;
        using var stdoutBuffer = new MemoryStream();
        using var stderrBuffer = new MemoryStream();

        var startTime = DateTime.UtcNow;

        // Create PowerShell execution parameters with IO redirection.
        var psParams = new ExecPwshParams(command)
            .IoRedirection(true)
            .NonInteractive(true)
            .NoLogo(true);

        // Set stdout handler - accumulate bytes and show progress.
        psParams.OnStdout = (sessionId, data, isLast) =>
        {
            if (data.Count > 0)
            {
                if (stdoutSize < NoOutputLimit)
                {
                    stdoutBuffer.Write(data.Array!, data.Offset, data.Count);
                }
                stdoutSize += data.Count;

                if ((stdoutSize - lastStdoutPrintSize) > DisplayProgressInterval)
                {
                    lastStdoutPrintSize = stdoutSize;
                    Console.WriteLine($"[STDOUT] Received {stdoutSize} bytes so far...");
                }
            }
        };

        // Set stderr handler - accumulate bytes and show progress.
        psParams.OnStderr = (sessionId, data, isLast) =>
        {
            if (data.Count > 0)
            {
                if (stderrSize < NoOutputLimit)
                {
                    stderrBuffer.Write(data.Array!, data.Offset, data.Count);
                }
                stderrSize += data.Count;

                // Show progress every 100KB.
                if (stderrSize - lastStderrPrintSize > DisplayProgressInterval)
                {
                    lastStderrPrintSize = stderrSize;
                    Console.WriteLine($"[STDERR] Received {stderrSize} bytes so far...");
                }
            }
        };

        try
        {
            Console.WriteLine($"Executing PowerShell command: {command}");

            var session = await client.ExecPwsh(psParams);

            // Wait for the session to complete.
            var exitCode = await session.GetResult();

            // Convert accumulated output to UTF8 and print.
            if (stdoutBuffer.Length > 0 && stdoutSize <= NoOutputLimit)
            {
                Console.WriteLine($"\n--- STDOUT Output ({stdoutBuffer.Length} bytes) ---");
                try
                {
                    var stdoutBytes = stdoutBuffer.ToArray();
                    var stdoutText = System.Text.Encoding.UTF8.GetString(stdoutBytes);
                    Console.Write(stdoutText);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting stdout to UTF8: {ex.Message}");
                    var stdoutBytes = stdoutBuffer.ToArray();
                    Console.WriteLine($"Raw bytes (first 100): {string.Join(" ", stdoutBytes.Take(100).Select(b => b.ToString("X2")))}");
                }
            }
            else
            {
                Console.WriteLine($"\n--- STDOUT Output skipped (total size {stdoutSize} bytes) ---");
            }

            if (stderrBuffer.Length > 0 && stderrSize <= 1024 * 10)
            {
                Console.WriteLine($"\n--- STDERR Output ({stderrBuffer.Length} bytes) ---");
                try
                {
                    var stderrBytes = stderrBuffer.ToArray();
                    var stderrText = System.Text.Encoding.UTF8.GetString(stderrBytes);
                    Console.Error.Write(stderrText);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error converting stderr to UTF8: {ex.Message}");
                    var stderrBytes = stderrBuffer.ToArray();
                    Console.Error.WriteLine($"Raw bytes (first 100): {string.Join(" ", stderrBytes.Take(100).Select(b => b.ToString("X2")))}");
                }
            }
            else
            {
                Console.WriteLine($"\n--- STDERR Output skipped (total size {stderrSize} bytes) ---");
            }

            Console.WriteLine($"\nExit code: {exitCode}");
            var executionTime = DateTime.UtcNow - startTime;
            Console.WriteLine($"Execution time: {executionTime.TotalSeconds:F2} seconds");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error executing PowerShell command: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Executes the RDM run command - starts an RDM application.
    /// </summary>
    private static async Task ExecuteRdmRunCommand(NowClient client)
    {
        try
        {
            // Check if RDM is available
            Console.WriteLine("Checking RDM availability...");
            var isAvailable = await client.IsRdmAppAvailable();
            if (!isAvailable)
            {
                Console.WriteLine("RDM is not available on this system.");
                return;
            }

            // Start RDM application
            Console.WriteLine("Starting RDM application...");
            var startParams = new RdmStartParams();

            await client.RdmStart(startParams);
            Console.WriteLine("RDM application started successfully.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error executing RDM run command: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes the RDM version command - displays RDM version information.
    /// </summary>
    private static async Task ExecuteRdmVersionCommand(NowClient client)
    {
        try
        {
            // Check if RDM is available
            var isAvailable = await client.IsRdmAppAvailable();
            if (!isAvailable)
            {
                Console.WriteLine("RDM is not available on this system.");
                return;
            }

            Console.WriteLine("RDM is available.");

            // Get and display version information
            var rdmVersion = await client.GetRdmVersion();
            if (rdmVersion != null)
            {
                Console.WriteLine($"RDM Version: {rdmVersion.Version}");
                if (!string.IsNullOrEmpty(rdmVersion.Extra))
                {
                    Console.WriteLine($"Extra information: {rdmVersion.Extra}");
                }
            }
            else
            {
                Console.WriteLine("Could not retrieve RDM version information.");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error executing RDM version command: {ex.Message}");
        }
    }
}