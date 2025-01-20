namespace Devolutions.NowProto.Capabilities
{
    /// <summary>
    /// NOW-PROTO: NOW_CHANNEL_CAPSET_MSG execCapset field.
    /// </summary>
    [Flags]
    public enum NowCapabilityExec : ushort
    {
        /// <summary>
        /// Empty capability set.
        /// </summary>
        None = 0x0000,
        /// <summary>
        /// Generic "Run" execution style.
        ///
        /// NOW-PROTO: NOW_CAP_EXEC_STYLE_RUN
        /// </summary>
        Run = 0x0001,
        /// <summary>
        /// CreateProcess() execution style.
        ///
        /// NOW-PROTO: NOW_CAP_EXEC_STYLE_PROCESS
        /// </summary>
        Process = 0x0002,
        /// <summary>
        /// System shell (.sh) execution style.
        ///
        /// NOW-PROTO: NOW_CAP_EXEC_STYLE_SHELL
        /// </summary>
        Shell = 0x0004,
        /// <summary>
        /// Windows batch file (.bat) execution style.
        ///
        /// NOW-PROTO: NOW_CAP_EXEC_STYLE_BATCH
        /// </summary>
        Batch = 0x0008,
        /// <summary>
        /// Windows PowerShell (.ps1) execution style.
        ///
        /// NOW-PROTO: NOW_CAP_EXEC_STYLE_WINPS
        /// </summary>
        WinPs = 0x0010,
        /// <summary>
        /// PowerShell 7 (.ps1) execution style.
        ///
        /// NOW-PROTO: NOW_CAP_EXEC_STYLE_PWSH
        /// </summary>
        Pwsh = 0x0020,
    }
}