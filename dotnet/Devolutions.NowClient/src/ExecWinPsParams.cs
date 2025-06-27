using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient
{
    /// <summary>
    /// Powershell 5 (PowerShell.exe) remote execution session parameters.
    /// </summary>
    /// <param name="command">PowerShell command/script to execute.</param>
    public class ExecWinPsParams(string filename) : AExecParams
    {
        /// <summary>
        /// Set the working directory for the command/script.
        /// </summary>
        public ExecWinPsParams Directory(string directory)
        {
            _directory = directory;
            return this;
        }

        /// <summary>
        /// Set the configuration name for the PowerShell session. (-ConfigurationName)
        /// </summary>
        public ExecWinPsParams ConfigurationName(string configurationName)
        {
            _configurationName = configurationName;
            return this;
        }

        /// <summary>
        /// Set the execution policy for the PowerShell session. (-ExecutionPolicy)
        /// </summary>
        public ExecWinPsParams ExecutionPolicy(string executionPolicy)
        {
            _executionPolicy = executionPolicy;
            return this;
        }

        /// <summary>
        /// Set the apartment state for the PowerShell session. (-Sta/-Mta)
        /// </summary>
        public ExecWinPsParams ApartmentState(NowMsgExecWinPs.ApartmentStateKind apartmentState)
        {
            _apartmentState = apartmentState;
            return this;
        }

        /// <summary>
        /// Disable the PowerShell logo display. (-NoLogo)
        /// </summary>
        public ExecWinPsParams NoLogo(bool enable = true)
        {
            _noLogo = enable;
            return this;
        }


        /// <summary>
        /// Do not close the PowerShell session after the command/script execution. (-NoExit)
        /// </summary>
        public ExecWinPsParams NoExit(bool enable = true)
        {
            _noExit = enable;
            return this;
        }

        /// <summary>
        /// Do not load the PowerShell profile. (-NoProfile)
        /// </summary>
        public ExecWinPsParams NoProfile(bool enable = true)
        {
            _noProfile = enable;
            return this;
        }

        /// <summary>
        /// Run the PowerShell session in non-interactive mode. (-NonInteractive)
        /// </summary>
        public ExecWinPsParams NonInteractive(bool enable = true)
        {
            _nonInteractive = enable;
            return this;
        }

        /// <summary>
        /// Enables or disables the use of pipes for standard input, output, and error streams.
        /// When enabled, the process's standard streams are redirected through pipes.
        /// </summary>
        private ExecWinPsParams IoRedirection(bool enable = true)
        {
            _ioRedirection = enable;
            return this;
        }

        internal NowMsgExecWinPs ToNowMessage(uint sessionId)
        {
            var builder = new NowMsgExecWinPs.Builder(sessionId, filename);

            if (_directory != null)
            {
                builder.Directory(_directory);
            }

            if (_configurationName != null)
            {
                builder.ConfigurationName(_configurationName);
            }

            if (_executionPolicy != null)
            {
                builder.ExecutionPolicy(_executionPolicy);
            }

            if (_apartmentState != null)
            {
                builder.ApartmentState(_apartmentState.Value);
            }

            if (_noLogo)
            {
                builder.SetNoLogo();
            }

            if (_noExit)
            {
                builder.SetNoExit();
            }

            if (_noProfile)
            {
                builder.SetNoProfile();
            }

            if (_nonInteractive)
            {
                builder.SetNonInteractive();
            }

            if (_ioRedirection)
            {
                builder.EnableIoRedirection();
            }

            return builder.Build();
        }


        private string? _configurationName = null;
        private string? _executionPolicy = null;
        private string? _directory = null;
        private NowMsgExecWinPs.ApartmentStateKind? _apartmentState = null;
        private bool _noLogo = false;
        private bool _noExit = false;
        private bool _noProfile = false;
        private bool _nonInteractive = false;
        private bool _ioRedirection = false;
    }
}