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
        /// Enables or disables the use of pipes for standard input, output, and error streams.
        /// When enabled, the process's standard streams are redirected through pipes.
        /// </summary>
        public ExecBatchParams IoRedirection(bool enable = true)
        {
            _ioRedirection = enable;
            return this;
        }

        /// <summary>
        /// Enables detached mode: the batch is started without tracking execution or sending back output.
        /// </summary>
        public ExecBatchParams Detached(bool enable = true)
        {
            _detached = enable;
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
                builder.EnableIoRedirection();
            }

            if (_detached)
            {
                builder.EnableDetached();
            }

            return builder.Build();
        }

        private string? _directory = null;
        private bool _ioRedirection = false;
        private bool _detached = false;
    }
}