namespace Devolutions.NowClient.Worker;

internal class CommandSetRdmSessionNotifyHandler : IClientCommand
{
    private readonly RdmSessionNotifyHandler? _handler;

    public CommandSetRdmSessionNotifyHandler(RdmSessionNotifyHandler? handler)
    {
        _handler = handler;
    }

    async Task IClientCommand.Execute(WorkerCtx ctx)
    {
        ctx.RdmSessionNotifyHandler = _handler;
        await Task.CompletedTask;
    }
}