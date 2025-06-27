using Devolutions.NowProto.Types;

namespace Devolutions.NowProto.Messages
{
    /// <summary>
    /// The NOW_EXEC_PROCESS_MSG message is used to send a Windows CreateProcess() request.
    ///
    /// NOW-PROTO: NOW_EXEC_PROCESS_MSG
    /// </summary>
    public class NowMsgExecProcess : INowSerialize, INowDeserialize<NowMsgExecProcess>
    {
        // -- INowMessage --

        public static byte TypeMessageClass => NowMessage.ClassExec;
        public static byte TypeMessageKind => 0x11; // NOW-PROTO: NOW_EXEC_PROCESS_MSG_ID

        byte INowMessage.MessageClass => NowMessage.ClassExec;
        byte INowMessage.MessageKind => 0x11;

        // -- INowSerialize --

        ushort INowSerialize.Flags => (ushort)(
            (Parameters != null ? MsgFlags.ParametersSet : 0)
            | (Directory != null ? MsgFlags.DirectorySet : 0)
            | (IoRedirection ? MsgFlags.IoRedirection : 0)
        );
        uint INowSerialize.BodySize =>
            FixedPartSize
            + NowVarStr.LengthOf(Filename)
            + NowVarStr.LengthOf(Parameters ?? "")
            + NowVarStr.LengthOf(Directory ?? "");

        void INowSerialize.SerializeBody(NowWriteCursor cursor)
        {
            cursor.EnsureEnoughBytes(FixedPartSize);
            cursor.WriteUint32Le(SessionId);
            cursor.WriteVarStr(Filename);
            cursor.WriteVarStr(Parameters ?? "");
            cursor.WriteVarStr(Directory ?? "");
        }

        // -- INowDeserialize --
        static NowMsgExecProcess INowDeserialize<NowMsgExecProcess>.Deserialize(ushort flags, NowReadCursor cursor)
        {
            cursor.EnsureEnoughBytes(FixedPartSize);
            var msgFlags = (MsgFlags)flags;
            var sessionId = cursor.ReadUInt32Le();
            var filename = cursor.ReadVarStr();
            var parameters = cursor.ReadVarStr();
            var directory = cursor.ReadVarStr();

            return new NowMsgExecProcess(
                sessionId,
                filename,
                msgFlags.HasFlag(MsgFlags.ParametersSet) ? parameters : null,
                msgFlags.HasFlag(MsgFlags.DirectorySet) ? directory : null,
                msgFlags.HasFlag(MsgFlags.IoRedirection)
            );
        }

        // -- impl --

        [Flags]
        private enum MsgFlags : ushort
        {
            /// <summary>
            /// Set if parameters field contains non-default value.
            ///
            /// NOW-PROTO: NOW_EXEC_FLAG_PROCESS_PARAMETERS_SET
            /// </summary>
            ParametersSet = 0x0001,

            /// <summary>
            /// Set if directory field contains non-default value.
            ///
            /// NOW-PROTO: NOW_EXEC_FLAG_PROCESS_DIRECTORY_SET
            /// </summary>
            DirectorySet = 0x0002,

            /// <summary>
            /// Enable stdio (stdout, stderr, stdin) redirection.
            ///
            /// NOW-PROTO: NOW_EXEC_FLAG_PROCESS_IO_REDIRECTION
            /// </summary>
            IoRedirection = 0x1000,
        }

        private const uint FixedPartSize = 4; // u32 SessionId

        public class Builder(uint sessionId, string filename)
        {
            public Builder Parameters(string parameters)
            {
                _parameters = parameters;
                return this;
            }

            public Builder Directory(string directory)
            {
                _directory = directory;
                return this;
            }

            public Builder EnableIoRedirection()
            {
                _ioRedirection = true;
                return this;
            }

            public NowMsgExecProcess Build()
            {
                return new NowMsgExecProcess(_sessionId, _filename, _parameters, _directory, _ioRedirection);
            }

            private readonly uint _sessionId = sessionId;
            private readonly string _filename = filename;
            private string? _parameters = null;
            private string? _directory = null;
            private bool _ioRedirection = false;
        }

        internal NowMsgExecProcess(uint sessionId, string filename, string? parameters, string? directory, bool ioRedirection)
        {
            SessionId = sessionId;
            Filename = filename;
            Parameters = parameters;
            Directory = directory;
            IoRedirection = ioRedirection;
        }

        public uint SessionId { get; }
        public string Filename { get; }
        public string? Parameters { get; }
        public string? Directory { get; }
        public bool IoRedirection { get; }
    }
}