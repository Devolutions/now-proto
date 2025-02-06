﻿using System.Diagnostics;
using System.Threading.Channels;

using Devolutions.NowProto;
using Devolutions.NowProto.Exceptions;
using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient.Worker
{
    /// <summary>
    /// Background worker logic for NowClient.
    /// </summary>
    internal class WorkerCtx
    {
        public static async Task Run(WorkerCtx ctx)
        {
            Task<IClientCommand>? clientReadTask = null;
            Task<NowMessage.NowMessageView>? serverReadTask = null;
            Task? heartbeatCheckTask = null;

            var tasks = new List<Task>();

            // Main async IO loop. (akin to tokio's `select!`)
            while (!ctx.ExitRequested)
            {
                tasks.Clear();

                // Check if task was completed on the previous loop iteration.
                // and re-add it to the list of tasks to be awaited.
                if (clientReadTask == null)
                {
                    clientReadTask = ctx.Commands.ReadAsync().AsTask();
                }

                if (serverReadTask == null)
                {
                    serverReadTask = ctx.NowChannel.ReadMessageAny();
                }

                // Skip task of heartbeat interval was not negotiated.
                if (heartbeatCheckTask == null && ctx.HeartbeatInterval != null)
                {
                    heartbeatCheckTask = Task.Delay(ctx.HeartbeatInterval.Value * 2);
                    tasks.Add(heartbeatCheckTask);
                }

                tasks.Add(clientReadTask);
                tasks.Add(serverReadTask);

                var completedTask = await Task.WhenAny(tasks);

                if (completedTask == clientReadTask)
                {
                    var command = await clientReadTask;
                    clientReadTask = null;
                    // See concrete commands implementations in
                    // Devolutions.NowClient.Worker namespace.
                    await command.Execute(ctx);
                }
                else if (completedTask == serverReadTask)
                {
                    var message = await serverReadTask;
                    serverReadTask = null;

                    switch (message.MessageClass)
                    {
                        case NowMessage.ClassChannel:
                            HandleChannelMessage(message, ctx);
                            break;
                        case NowMessage.ClassSystem:
                            // System messages are not expected to have responses(yet).
                            Debug.WriteLine($"Unhandled system message kind={message.MessageKind}");
                            break;
                        case NowMessage.ClassSession:
                            HandleSessionMessage(message, ctx);
                            break;
                        case NowMessage.ClassExec:
                            HandleExecMessage(message, ctx);
                            break;
                        default:
                            Debug.WriteLine($"Unhandled message class={message.MessageClass}");
                            break;
                    }
                }
                else if (completedTask == heartbeatCheckTask)
                {
                    var heartbeatLeeway = TimeSpan.FromSeconds(5);

                    heartbeatCheckTask = null;
                    if (!((DateTime.Now - ctx.LastHeartbeat) > (ctx.HeartbeatInterval + heartbeatLeeway)))
                    {
                        continue;
                    }

                    Debug.WriteLine("Heartbeat timeout triggered");

                    // Channel is considered dead; No attempt to send any messages should be made.
                    ctx.ExitRequested = true;
                }
            }
        }

        private static void HandleChannelMessage(NowMessage.NowMessageView message, WorkerCtx ctx)
        {
            var kind = (ChannelMessageKind)message.MessageKind;

            switch (kind)
            {
                case ChannelMessageKind.Heartbeat:
                    ctx.LastHeartbeat = DateTime.Now;
                    break;
                case ChannelMessageKind.Close:
                    var decoded = message.Deserialize<NowMsgChannelClose>();
                    ctx.ExitRequested = true;
                    try
                    {
                        decoded.ThrowIfError();
                    }
                    catch (NowException e)
                    {
                        Debug.WriteLine($"Channel close error: {e.Message}");
                    }

                    break;
                default:
                    Debug.WriteLine($"Unhandled channel message kind={message.MessageKind}");
                    break;
            }
        }

        private static void HandleSessionMessage(NowMessage.NowMessageView message, WorkerCtx ctx)
        {
            var kind = (SessionMessageKind)message.MessageKind;

            Debug.WriteLine($"Received session message");

            switch (kind)
            {
                case SessionMessageKind.MsgBoxRsp:
                    var decoded = message.Deserialize<NowMsgSessionMessageBoxRsp>();

                    if (ctx.MessageBoxHandlers.TryGetValue(decoded.RequestId, out IMessageBoxRspHandler? handler))
                    {
                        handler.HandleMessageBoxRsp(decoded);
                        ctx.MessageBoxHandlers.Remove(decoded.RequestId);
                    }
                    else
                    {
                        Debug.WriteLine($"Received unexpected message box response with requestId={decoded.RequestId}");
                    }

                    break;
                default:
                    Debug.WriteLine($"Unhandled session message kind: {message.MessageKind}");
                    break;
            }
        }

        private static void HandleExecMessage(NowMessage.NowMessageView message, WorkerCtx ctx)
        {
            var kind = (ExecMessageKind)message.MessageKind;

            switch (kind)
            {
                case ExecMessageKind.CancelRsp:
                    {
                        var decoded = message.Deserialize<NowMsgExecCancelRsp>();

                        if (ctx.ExecSessionHandlers.TryGetValue(decoded.SessionId, out IExecSessionHandler? handler))
                        {
                            handler.HandleCancelRsp(decoded);
                            // Unregister session if the cancel was successful.
                            if (decoded.IsSuccess)
                            {
                                ctx.ExecSessionHandlers.Remove(decoded.SessionId);
                            }
                        }
                        else
                        {
                            Debug.WriteLine(
                                $"Received unexpected exec cancel response with sessionId={decoded.SessionId}");
                        }

                        break;
                    }
                case ExecMessageKind.Data:
                    {
                        var decoded = message.Deserialize<NowMsgExecData>();
                        if (ctx.ExecSessionHandlers.TryGetValue(decoded.SessionId, out IExecSessionHandler? handler))
                        {
                            handler.HandleOutput(decoded);
                        }
                        else
                        {
                            Debug.WriteLine($"Received unexpected exec data with sessionId={decoded.SessionId}");
                        }

                        break;
                    }
                case ExecMessageKind.Started:
                    {
                        var decoded = message.Deserialize<NowMsgExecStarted>();
                        if (ctx.ExecSessionHandlers.TryGetValue(decoded.SessionId, out IExecSessionHandler? handler))
                        {
                            handler.HandleStarted();
                        }
                        else
                        {
                            Debug.WriteLine($"Received unexpected exec started with sessionId={decoded.SessionId}");
                        }

                        break;
                    }
                case ExecMessageKind.Result:
                    {
                        var decoded = message.Deserialize<NowMsgExecResult>();
                        if (ctx.ExecSessionHandlers.TryGetValue(decoded.SessionId, out IExecSessionHandler? handler))
                        {
                            handler.HandleResult(decoded);
                            // Unregister session after receiving the result.
                            ctx.ExecSessionHandlers.Remove(decoded.SessionId);
                        }
                        else
                        {
                            Debug.WriteLine($"Received unexpected exec result with sessionId={decoded.SessionId}");
                        }

                        break;
                    }
                default:
                    Debug.WriteLine($"Unhandled exec message kind: {message.MessageKind}");
                    break;
            }
        }

        // -- IO --
        public required NowChannelTransport NowChannel;
        public required ChannelReader<IClientCommand> Commands;

        // -- State --
        public required TimeSpan? HeartbeatInterval;
        public required DateTime LastHeartbeat;
        public required NowMsgChannelCapset Capabilities;
        public bool ExitRequested = false;

        public Dictionary<uint, IMessageBoxRspHandler> MessageBoxHandlers = [];
        public Dictionary<uint, IExecSessionHandler> ExecSessionHandlers = [];
    }
}