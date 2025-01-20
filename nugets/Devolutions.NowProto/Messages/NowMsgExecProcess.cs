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

        static byte INowMessage.TypeMessageClass => NowMessage.ClassExec;
        static byte INowMessage.TypeMessageKind => 0x11; // NOW-PROTO: NOW_EXEC_PROCESS_MSG_ID

        byte INowMessage.MessageClass => NowMessage.ClassExec;
        byte INowMessage.MessageKind => 0x11;

        // -- INowSerialize --

        ushort INowSerialize.Flags => (ushort)(
            (Parameters != null ? MsgFlags.ParametersSet : 0)
            | (Directory != null ? MsgFlags.DirectorySet : 0)
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
                msgFlags.HasFlag(MsgFlags.DirectorySet) ? directory : null
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

            public NowMsgExecProcess Build()
            {
                return new NowMsgExecProcess(_sessionId, _filename, _parameters, _directory);
            }

            private readonly uint _sessionId = sessionId;
            private readonly string _filename = filename;
            private string? _parameters = null;
            private string? _directory = null;
        }

        internal NowMsgExecProcess(uint sessionId, string filename, string? parameters, string? directory)
        {
            SessionId = sessionId;
            Filename = filename;
            Parameters = parameters;
            Directory = directory;
        }

        public uint SessionId { get; }
        public string Filename { get; }
        public string? Parameters { get; }
        public string? Directory { get; }
    }
}