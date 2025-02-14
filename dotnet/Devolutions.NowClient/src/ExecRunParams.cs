using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient
{
    /// <summary>
    /// OS-specific simple "fire-and-forget" command execution parameters.
    /// Note that session state/result is not available for this type of execution.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    public class ExecRunParams(string command) : AExecParams
    {
        internal NowMsgExecRun ToNowMessage(uint sessionId)
        {
            return new NowMsgExecRun(sessionId, command);
        }
    }
}