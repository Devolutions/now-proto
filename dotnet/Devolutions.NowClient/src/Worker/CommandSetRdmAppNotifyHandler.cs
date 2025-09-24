namespace Devolutions.NowClient.Worker
{
    internal class CommandSetRdmAppNotifyHandler : IClientCommand
    {
        private readonly RdmAppNotifyHandler? _handler;

        public CommandSetRdmAppNotifyHandler(RdmAppNotifyHandler? handler)
        {
            _handler = handler;
        }

        async Task IClientCommand.Execute(WorkerCtx ctx)
        {
            ctx.RdmAppNotifyHandler = _handler;
            await Task.CompletedTask;
        }
    }
}