using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient
{
    /// <summary>
    /// Delegate for handling RDM application notifications.
    /// </summary>
    /// <param name="appState">The RDM application state.</param>
    /// <param name="reasonCode">The reason code for the state change.</param>
    /// <param name="notifyData">Additional notification data.</param>
    public delegate void RdmAppNotifyHandler(NowRdmAppState appState, NowRdmReason reasonCode, string notifyData);

    /// <summary>
    /// Delegate for handling RDM session notifications.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="sessionNotify">The type of session notification.</param>
    /// <param name="logData">Session log data.</param>
    public delegate void RdmSessionNotifyHandler(Guid sessionId, NowRdmSessionNotifyKind sessionNotify, string logData);

    /// <summary>
    /// RDM version information.
    /// </summary>
    public class RdmVersion
    {
        /// <summary>
        /// Gets the RDM version string (e.g., "2025.1.0.0").
        /// </summary>
        public string Version { get; internal set; } = "";

        /// <summary>
        /// Gets extra version information.
        /// </summary>
        public string? Extra { get; internal set; }

        internal RdmVersion(string version, string? extra)
        {
            Version = version;
            Extra = extra;
        }
    }

    /// <summary>
    /// Contains information about RDM capabilities and version.
    /// </summary>
    internal class RdmCapabilityInfo
    {
        /// <summary>
        /// Gets a value indicating whether RDM application is available.
        /// </summary>
        public bool IsAppAvailable { get; internal set; }

        /// <summary>
        /// Gets the RDM version string (e.g., "2025.1.0.0").
        /// </summary>
        public string RdmVersion { get; internal set; } = "";

        /// <summary>
        /// Gets extra version information.
        /// </summary>
        public string VersionExtra { get; internal set; } = "";

        /// <summary>
        /// Gets the server timestamp from the capabilities exchange.
        /// </summary>
        public DateTimeOffset ServerTimestamp { get; internal set; }
    }

    /// <summary>
    /// Parameters for starting RDM application.
    /// </summary>
    public class RdmStartParams
    {
        /// <summary>
        /// Gets or sets the launch flags for the RDM application.
        /// </summary>
        public NowRdmLaunchFlags LaunchFlags { get; set; } = NowRdmLaunchFlags.None;

        /// <summary>
        /// Gets or sets the timeout for RDM to launch and become ready.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(45);
    }

    /// <summary>
    /// Parameters for starting an RDM session.
    /// </summary>
    public class RdmSessionStartParams
    {
        /// <summary>
        /// Gets or sets the unique session identifier. If not provided, a new GUID will be generated.
        /// </summary>
        public Guid SessionId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the connection identifier. Reserved for future use.
        /// </summary>
        public Guid ConnectionId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the serialized RDM XML connection object.
        /// </summary>
        public string ConnectionData { get; set; } = "";

        /// <summary>
        /// Gets or sets the callback to handle session notifications.
        /// </summary>
        public RdmSessionNotifyHandler? NotifyHandler { get; set; }
    }
}