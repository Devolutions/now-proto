using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient
{
    /// <summary>
    /// Batch (Windows CMD) remote execution session parameters.
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

        /// <summary>
        /// Enable stdio (stdout, stderr, stdin) redirection.
        /// </summary>
        public ExecBatchParams IoRedirection()
        {
            _ioRedirection = true;
            return this;
        }

        internal NowMsgExecBatch ToNowMessage(uint sessionId)
        {
            var builder = new NowMsgExecBatch.Builder(sessionId, command);

            if (_directory != null)
            {
                builder.Directory(_directory);
            }

            if (_ioRedirection)
            {
                builder.IoRedirection();
            }

            return builder.Build();
        }

        private string? _directory = null;
        private bool _ioRedirection = false;
    }
}