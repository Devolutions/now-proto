using System.Diagnostics;
using System.Threading.Channels;

using Devolutions.NowClient.Worker;
using Devolutions.NowProto.Capabilities;
using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient
{
    /// <summary>
    /// NOW-proto remote execution channel client.
    /// </summary>
    public class NowClient
    {
        /// <summary>
        /// Performs connection and negotiation sequence to NOW-proto server
        /// over the provided transport (e.g. DVC-based channel) and returns
        /// a client object ready to accept commands.
        /// </summary>
        public static async Task<NowClient> Connect(INowTransport transportImpl)
        {
            var channel = new NowChannelTransport(transportImpl);

            // Support all capabilities by default on client side.
            var clientCapabilities = new NowMsgChannelCapset.Builder()
                .HeartbeatInterval(TimeSpan.FromSeconds(60))
                .SystemCapset(NowCapabilitySystem.All)
                .SessionCapset(NowCapabilitySession.All)
                .ExecCapset(NowCapabilityExec.All)
                .Build();

            await channel.WriteMessage(clientCapabilities);

            // Wait for downgraded capabilities from server or throw timeout error.
            var capabilities = await channel.ReadMessage<NowMsgChannelCapset>()
                .WaitAsync(TimeSpan.FromSeconds(TimeoutConnectSeconds));

            // Negotiation successful
            Debug.WriteLine($"NOW channel negotiation complete");

            // client -> worker communication channel.
            // Reverse communication (e.g) worker -> client is done through passed handler objects.
            var clientChannel = Channel.CreateBounded<IClientCommand>(IoChannelCapacity);

            var ctx = new WorkerCtx
            {
                NowChannel = channel,
                Capabilities = capabilities,
                LastHeartbeat = DateTime.Now,
                Commands = clientChannel.Reader,
                HeartbeatInterval = capabilities.HeartbeatInterval,
            };

            var workerTask = Task.Run(() =>
            {
                try
                {
                    return WorkerCtx.Run(ctx);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"NowClient worker exception: {e}");
                    throw;
                }
            });

            return new NowClient(capabilities, workerTask, clientChannel.Writer);
        }

        /// <summary>
        /// Send system shutdown command to the NOW-proto server.
        /// </summary>
        public async Task SystemShutdown(SystemShutdownParams shutdownParams)
        {
            ThrowIfWorkerTerminated();

            if (!Capabilities.SystemCapset.HasFlag(NowCapabilitySystem.Shutdown))
            {
                ThrowCapabilitiesError("Shutdown");
            }

            await _commandWriter.WriteAsync(new CommandSystemShutdown(shutdownParams.NowMessage));
        }

        /// <summary>
        /// Send session lock command to the NOW-proto server.
        /// </summary>
        public async Task SessionLock()
        {
            ThrowIfWorkerTerminated();

            if (!Capabilities.SessionCapset.HasFlag(NowCapabilitySession.Lock))
            {
                ThrowCapabilitiesError("Session lock");
            }

            await _commandWriter.WriteAsync(new CommandSessionLock());
        }

        /// <summary>
        /// Send session logoff command to the NOW-proto server.
        /// </summary>
        public async Task SessionLogoff()
        {
            ThrowIfWorkerTerminated();

            if (!Capabilities.SessionCapset.HasFlag(NowCapabilitySession.Logoff))
            {
                ThrowCapabilitiesError("Session logoff");
            }

            await _commandWriter.WriteAsync(new CommandSessionLogoff());
        }

        /// <summary>
        /// Send message box request to the NOW-proto server.
        /// This request will return response handler which could be
        /// used to wait for the response result.
        /// </summary>
        public async Task<MessageBoxResponse> SessionMessageBox(MessageBoxParams msgBoxParams)
        {
            ThrowIfWorkerTerminated();

            if (!Capabilities.SessionCapset.HasFlag(NowCapabilitySession.Msgbox))
            {
                ThrowCapabilitiesError("Message box");
            }

            var requestId = _nextMessageBoxId++;
            var message = msgBoxParams.ToNowMessage(requestId, true);
            var handler = new MessageBoxResponse(requestId);
            var command = new CommandSessionMsgBox(message, handler);

            await _commandWriter.WriteAsync(command);

            return handler;
        }

        /// <summary>
        /// Send message box request to the NOW-proto server.
        /// This request is fire-and-forget and does not wait for the response.
        /// </summary>
        public async Task SessionMessageBoxNoResponse(MessageBoxParams msgBoxParams)
        {
            ThrowIfWorkerTerminated();

            if (!Capabilities.SessionCapset.HasFlag(NowCapabilitySession.Msgbox))
            {
                ThrowCapabilitiesError("Message box");
            }

            var requestId = _nextMessageBoxId++;
            var message = msgBoxParams.ToNowMessage(requestId, false);
            var command = new CommandSessionMsgBox(message, null);

            await _commandWriter.WriteAsync(command);
        }

        /// <summary>
        /// Start a new simple remote execution session.
        /// (see <see cref="ExecRunParams"/> for more details).
        /// </summary>
        public async Task ExecRun(ExecRunParams execParams)
        {
            ThrowIfWorkerTerminated();

            if (!Capabilities.ExecCapset.HasFlag(NowCapabilityExec.Run))
            {
                ThrowCapabilitiesError("Run execution style");
            }

            var sessionId = _nextExecSessionId++;
            var message = execParams.ToNowMessage(sessionId);
            var command = new CommandExecRun(message);

            await _commandWriter.WriteAsync(command);
        }

        /// <summary>
        /// Start a new process remote execution session.
        /// See <see cref="ExecProcessParams"/> for more details.
        /// </summary>
        public async Task<ExecSession> ExecProcess(ExecProcessParams execParams)
        {
            ThrowIfWorkerTerminated();

            if (!Capabilities.ExecCapset.HasFlag(NowCapabilityExec.Process))
            {
                ThrowCapabilitiesError("Process execution style");
            }

            var sessionId = _nextExecSessionId++;
            var message = execParams.ToNowMessage(sessionId);
            var execSession = execParams.ToExecSession(sessionId, _commandWriter);
            var command = new CommandExecProcess(message, execSession);

            await _commandWriter.WriteAsync(command);

            return execSession;
        }

        /// <summary>
        /// Start a new shell remote execution session.
        /// See <see cref="ExecShellParams"/> for more details.
        /// </summary>
        public async Task<ExecSession> ExecShell(ExecShellParams execParams)
        {
            ThrowIfWorkerTerminated();

            if (!Capabilities.ExecCapset.HasFlag(NowCapabilityExec.Shell))
            {
                ThrowCapabilitiesError("Shell execution style");
            }

            var sessionId = _nextExecSessionId++;
            var message = execParams.ToNowMessage(sessionId);
            var execSession = execParams.ToExecSession(sessionId, _commandWriter);
            var command = new CommandExecShell(message, execSession);

            await _commandWriter.WriteAsync(command);

            return execSession;
        }

        /// <summary>
        /// Start a new batch remote execution session.
        /// See <see cref="ExecBatchParams"/> for more details.
        /// </summary>
        public async Task<ExecSession> ExecBatch(ExecBatchParams execParams)
        {
            ThrowIfWorkerTerminated();

            if (!Capabilities.ExecCapset.HasFlag(NowCapabilityExec.Batch))
            {
                ThrowCapabilitiesError("Batch execution style");
            }

            var sessionId = _nextExecSessionId++;
            var message = execParams.ToNowMessage(sessionId);
            var execSession = execParams.ToExecSession(sessionId, _commandWriter);
            var command = new CommandExecBatch(message, execSession);

            await _commandWriter.WriteAsync(command);

            return execSession;
        }

        /// <summary>
        /// Start a new PowerShell remote execution session.
        /// See <see cref="ExecWinPsParams"/> for more details.
        /// </summary>
        public async Task<ExecSession> ExecWinPs(ExecWinPsParams execParams)
        {
            ThrowIfWorkerTerminated();

            if (!Capabilities.ExecCapset.HasFlag(NowCapabilityExec.WinPs))
            {
                ThrowCapabilitiesError("Windows PowerShell execution style");
            }

            var sessionId = _nextExecSessionId++;
            var message = execParams.ToNowMessage(sessionId);
            var execSession = execParams.ToExecSession(sessionId, _commandWriter);
            var command = new CommandExecWinPs(message, execSession);

            await _commandWriter.WriteAsync(command);

            return execSession;
        }

        /// <summary>
        /// Start a new PowerShell Core remote execution session.
        /// See <see cref="ExecPwshParams"/> for more details.
        /// </summary>
        public async Task<ExecSession> ExecPwsh(ExecPwshParams execParams)
        {
            ThrowIfWorkerTerminated();

            if (!Capabilities.ExecCapset.HasFlag(NowCapabilityExec.Pwsh))
            {
                ThrowCapabilitiesError("Pwsh execution style");
            }

            var sessionId = _nextExecSessionId++;
            var message = execParams.ToNowMessage(sessionId);
            var execSession = execParams.ToExecSession(sessionId, _commandWriter);
            var command = new CommandExecPwsh(message, execSession);

            await _commandWriter.WriteAsync(command);

            return execSession;
        }

        /// <summary>
        /// Gracefully terminate and close the NOW-proto communication channel.
        /// </summary>
        public async Task ForceTermiate()
        {
            await _commandWriter.WriteAsync(new CommandChannelClose());
        }

        private static void ThrowCapabilitiesError(string capability)
        {
            throw new NowClientException($"{capability} is not supported by server.");
        }

        private void ThrowIfWorkerTerminated()
        {
            if (_runnerTask.IsCompleted)
            {
                throw new NowClientException("NOW-proto worker has been terminated.");
            }
        }

        private NowClient(
            NowMsgChannelCapset capabilities,
            Task runnerTask,
            ChannelWriter<IClientCommand> commandWriter
        )
        {
            this.Capabilities = capabilities;
            this._runnerTask = runnerTask;
            this._commandWriter = commandWriter;
        }

        private const int TimeoutConnectSeconds = 10;
        private const int IoChannelCapacity = 1024;

        /// <summary>
        /// Check if the NOW-proto channel has been terminated.
        /// </summary>
        public bool IsTerminated => _runnerTask.IsCompleted;

        public NowMsgChannelCapset Capabilities { get; }

        private uint _nextMessageBoxId = 0;
        private uint _nextExecSessionId = 0;

        private readonly Task _runnerTask;
        private readonly ChannelWriter<IClientCommand> _commandWriter;
    }
}