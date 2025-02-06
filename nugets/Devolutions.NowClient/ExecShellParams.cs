using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient
{
    /// <summary>
    /// Shell (e.g. sh, bash, etc.) remote execution session parameters.
    /// </summary>
    /// <param name="command">Command/script to execute.</param>
    public class ExecShellParams(string command) : AExecParams
    {
        /// <summary>
        /// Set the shell to use for the command/script execution.
        /// Uses default system shell if not set.
        /// </summary>
        public ExecShellParams Shell(string shell)
        {
            _shell = shell;
            return this;
        }

        /// <summary>
        /// Set the working directory for the command/script.
        /// </summary>
        public ExecShellParams Directory(string directory)
        {
            _directory = directory;
            return this;
        }

        internal NowMsgExecShell ToNowMessage(uint sessionId)
        {
            var builder = new NowMsgExecShell.Builder(sessionId, command);

            if (_shell != null)
            {
                builder.Shell(_shell);
            }

            if (_directory != null)
            {
                builder.Directory(_directory);
            }

            return builder.Build();
        }

        private string? _shell = null;
        private string? _directory = null;
    }
}