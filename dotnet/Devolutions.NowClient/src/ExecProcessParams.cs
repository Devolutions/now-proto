using System.Runtime.InteropServices;

using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient
{
    /// <summary>
    /// Plain process execution session parameters (e.g. CreateProcessW on Windows hosts).
    /// </summary>
    /// <param name="filename"></param>
    public class ExecProcessParams(string filename) : AExecParams
    {
        /// <summary>
        /// Set the command line parameters for the process.
        /// </summary>
        public ExecProcessParams Parameters(string parameters)
        {
            _parameters = parameters;
            return this;
        }

        /// <summary>
        /// Set the working directory for the process.
        /// </summary>
        public ExecProcessParams Directory(string directory)
        {
            _directory = directory;
            return this;
        }

        /// <summary>
        /// Enable stdio (stdout, stderr, stdin) redirection.
        /// </summary>
        public ExecProcessParams IoRedirection()
        {
            _ioRedirection = true;
            return this;
        }

        internal NowMsgExecProcess ToNowMessage(uint sessionId)
        {
            var builder = new NowMsgExecProcess.Builder(sessionId, filename);

            if (_parameters != null)
            {
                builder.Parameters(_parameters);
            }

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


        private bool _ioRedirection = false;
        private string? _parameters = null;
        private string? _directory = null;
    }
}