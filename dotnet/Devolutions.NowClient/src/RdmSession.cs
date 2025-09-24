using System.Threading.Channels;

using Devolutions.NowClient.Worker;
using Devolutions.NowProto.Messages;

namespace Devolutions.NowClient
{
    /// <summary>
    /// Represents the state of an RDM session.
    /// </summary>
    public enum RdmSessionState
    {
        /// <summary>
        /// Session is active and can receive commands.
        /// </summary>
        Active,

        /// <summary>
        /// Session has been closed and cannot receive further commands.
        /// </summary>
        Closed
    }

    /// <summary>
    /// Represents an active RDM session and provides methods to control it.
    /// </summary>
    public class RdmSession
    {
        private readonly Guid _sessionId;
        private readonly ChannelWriter<IClientCommand> _commandWriter;
        private RdmSessionState _state = RdmSessionState.Active;
        private readonly object _stateLock = new object();

        /// <summary>
        /// Callback for session notifications. Set during session creation and cannot be changed.
        /// </summary>
        private readonly RdmSessionNotifyHandler? _notifyHandler;

        internal RdmSession(Guid sessionId, ChannelWriter<IClientCommand> commandWriter, RdmSessionNotifyHandler? notifyHandler = null)
        {
            _sessionId = sessionId;
            _commandWriter = commandWriter;
            _notifyHandler = notifyHandler;
        }

        /// <summary>
        /// Gets the unique session identifier.
        /// </summary>
        public Guid SessionId => _sessionId;

        /// <summary>
        /// Gets the current state of the session.
        /// </summary>
        public RdmSessionState State
        {
            get
            {
                lock (_stateLock)
                {
                    return _state;
                }
            }
        }

        /// <summary>
        /// Focuses the RDM session (brings it to the foreground).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the session is closed.</exception>
        public async Task Focus()
        {
            ThrowIfClosed();

            var message = new NowMsgRdmSessionAction(NowRdmSessionAction.Focus, _sessionId);
            var command = new CommandRdmSessionAction(message);
            await _commandWriter.WriteAsync(command);
        }

        /// <summary>
        /// Closes the RDM session.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the session is already closed.</exception>
        public async Task Close()
        {
            ThrowIfClosed();

            var message = new NowMsgRdmSessionAction(NowRdmSessionAction.Close, _sessionId);
            var command = new CommandRdmSessionAction(message);
            await _commandWriter.WriteAsync(command);

            // Mark session as closed
            lock (_stateLock)
            {
                _state = RdmSessionState.Closed;
            }
        }

        /// <summary>
        /// Internal method to mark the session as closed (called when receiving session notifications).
        /// </summary>
        internal void MarkAsClosed()
        {
            lock (_stateLock)
            {
                _state = RdmSessionState.Closed;
            }
        }

        /// <summary>
        /// Internal method to handle session notifications.
        /// </summary>
        internal void HandleNotification(NowRdmSessionNotifyKind notifyKind, string logData)
        {
            if (notifyKind == NowRdmSessionNotifyKind.Close)
            {
                MarkAsClosed();
            }

            _notifyHandler?.Invoke(_sessionId, notifyKind, logData);
        }

        private void ThrowIfClosed()
        {
            lock (_stateLock)
            {
                if (_state == RdmSessionState.Closed)
                {
                    throw new InvalidOperationException("Cannot perform operation on a closed RDM session.");
                }
            }
        }
    }
}