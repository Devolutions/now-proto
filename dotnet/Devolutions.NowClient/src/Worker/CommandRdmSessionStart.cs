using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient.Worker
{
    internal class CommandRdmSessionStart : IClientCommand
    {
        private readonly NowMsgRdmSessionStart _message;
        private readonly RdmSession _session;

        public CommandRdmSessionStart(NowMsgRdmSessionStart message, RdmSession session)
        {
            _message = message;
            _session = session;
        }

        public async Task Execute(WorkerCtx ctx)
        {
            await ctx.NowChannel.WriteMessage(_message);

            // Register the session for tracking
            ctx.RegisterRdmSession(_session);
        }
    }
}