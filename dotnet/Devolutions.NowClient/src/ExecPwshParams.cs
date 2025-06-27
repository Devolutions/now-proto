using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient
{
    /// <summary>
    /// Powershell 7 and higher (pwsh) remote execution session parameters.
    /// </summary>
    /// <param name="command">PowerShell command/script to execute.</param>
    public class ExecPwshParams(string command) : AExecParams
    {
        /// <summary>
        /// Set the working directory for the command/script.
        /// </summary>
        public ExecPwshParams Directory(string directory)
        {
            _directory = directory;
            return this;
        }

        /// <summary>
        /// Set the configuration name for the PowerShell session. (-ConfigurationName)
        /// </summary>
        public ExecPwshParams ConfigurationName(string configurationName)
        {
            _configurationName = configurationName;
            return this;
        }

        /// <summary>
        /// Set the execution policy for the PowerShell session. (-ExecutionPolicy)
        /// </summary>
        public ExecPwshParams ExecutionPolicy(string executionPolicy)
        {
            _executionPolicy = executionPolicy;
            return this;
        }

        /// <summary>
        /// Set the apartment state for the PowerShell session. (-Sta/-Mta)
        /// </summary>
        public ExecPwshParams ApartmentState(NowMsgExecPwsh.ApartmentStateKind apartmentState)
        {
            _apartmentState = apartmentState;
            return this;
        }

        /// <summary>
        /// Disable the PowerShell logo display. (-NoLogo)
        /// </summary>
        public ExecPwshParams NoLogo()
        {
            _noLogo = true;
            return this;
        }

        /// <summary>
        /// Do not close the PowerShell session after the command/script execution. (-NoExit)
        /// </summary>
        public ExecPwshParams NoExit()
        {
            _noExit = true;
            return this;
        }

        /// <summary>
        /// Do not load the PowerShell profile. (-NoProfile)
        /// </summary>
        public ExecPwshParams NoProfile()
        {
            _noProfile = true;
            return this;
        }

        /// <summary>
        /// Run the PowerShell session in non-interactive mode. (-NonInteractive)
        /// </summary>
        public ExecPwshParams NonInteractive()
        {
            _nonInteractive = true;
            return this;
        }

        /// <summary>
        /// Enable stdio (stdout, stderr, stdin) redirection.
        /// </summary>
        private ExecPwshParams IoRedirection()
        {
            _ioRedirection = true;
            return this;
        }

        internal NowMsgExecPwsh ToNowMessage(uint sessionId)
        {
            var builder = new NowMsgExecPwsh.Builder(sessionId, command);

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
                builder.IoRedirection();
            }

            return builder.Build();
        }


        private string? _configurationName = null;
        private string? _executionPolicy = null;
        private string? _directory = null;
        private NowMsgExecPwsh.ApartmentStateKind? _apartmentState = null;
        private bool _noLogo = false;
        private bool _noExit = false;
        private bool _noProfile = false;
        private bool _nonInteractive = false;
        private bool _ioRedirection = false;
    }
}