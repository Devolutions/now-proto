using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient
{
    /// <summary>
    /// Batch(Windows CMD) remote execution session parameters.
    /// </summary>
    /// <param name="command">Command/script to execute.</param>
    public class ExecBatchParams(string command) : AExecParams
    {
        /// <summary>
        /// Set the working directory for the command/script.
        /// </summary>
        public ExecBatchParams Directory(string directory)
        {
            _directory = directory;
            return this;
        }

        internal NowMsgExecBatch ToNowMessage(uint sessionId)
        {
            var builder = new NowMsgExecBatch.Builder(sessionId, command);

            if (_directory != null)
            {
                builder.Directory(_directory);
            }

            return builder.Build();
        }

        private string? _directory = null;
    }
}