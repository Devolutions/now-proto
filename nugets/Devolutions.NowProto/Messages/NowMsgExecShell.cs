using Devolutions.NowProto.Types;

namespace Devolutions.NowProto.Messages
{
    /// <summary>
    /// The NOW_EXEC_SHELL_MSG message is used to execute a remote shell command.
    ///
    /// NOW-PROTO: NOW_EXEC_SHELL_MSG
    /// </summary>
    public class NowMsgExecShell : INowSerialize, INowDeserialize<NowMsgExecShell>
    {
        // -- INowMessage --

        public static byte TypeMessageClass => NowMessage.ClassExec;
        public static byte TypeMessageKind => 0x12; // NOW-PROTO: NOW_EXEC_SHELL_MSG_ID

        byte INowMessage.MessageClass => NowMessage.ClassExec;
        byte INowMessage.MessageKind => 0x12;

        // -- INowSerialize --

        ushort INowSerialize.Flags => (ushort)(
            (Shell != null ? MsgFlags.ShellSet : 0)
            | (Directory != null ? MsgFlags.DirectorySet : 0)
        );

        uint INowSerialize.BodySize =>
            FixedPartSize
            + NowVarStr.LengthOf(Filename)
            + NowVarStr.LengthOf(Shell ?? "")
            + NowVarStr.LengthOf(Directory ?? "");

        void INowSerialize.SerializeBody(NowWriteCursor cursor)
        {
            cursor.EnsureEnoughBytes(FixedPartSize);
            cursor.WriteUint32Le(SessionId);
            cursor.WriteVarStr(Filename);
            cursor.WriteVarStr(Shell ?? "");
            cursor.WriteVarStr(Directory ?? "");
        }

        // -- INowDeserialize --
        static NowMsgExecShell INowDeserialize<NowMsgExecShell>.Deserialize(ushort flags, NowReadCursor cursor)
        {
            cursor.EnsureEnoughBytes(FixedPartSize);
            var msgFlags = (MsgFlags)flags;
            var sessionId = cursor.ReadUInt32Le();
            var filename = cursor.ReadVarStr();
            var parameters = cursor.ReadVarStr();
            var directory = cursor.ReadVarStr();

            return new NowMsgExecShell(
                sessionId,
                filename,
                msgFlags.HasFlag(MsgFlags.ShellSet) ? parameters : null,
                msgFlags.HasFlag(MsgFlags.DirectorySet) ? directory : null
            );
        }

        // -- impl --

        [Flags]
        private enum MsgFlags : ushort
        {
            /// <summary>
            /// Set if parameters shell contains non-default value.
            ///
            /// NOW-PROTO: NOW_EXEC_FLAG_SHELL_SHELL_SET
            /// </summary>
            ShellSet = 0x0001,

            /// <summary>
            /// Set if directory field contains non-default value.
            ///
            /// NOW-PROTO: NOW_EXEC_FLAG_SHELL_DIRECTORY_SET
            /// </summary>
            DirectorySet = 0x0002,
        }

        private const uint FixedPartSize = 4; // u32 SessionId

        public class Builder(uint sessionId, string filename)
        {
            public Builder Shell(string shell)
            {
                _shell = shell;
                return this;
            }

            public Builder Directory(string directory)
            {
                _directory = directory;
                return this;
            }

            public NowMsgExecShell Build()
            {
                return new NowMsgExecShell(_sessionId, _filename, _shell, _directory);
            }

            private readonly uint _sessionId = sessionId;
            private readonly string _filename = filename;
            private string? _shell = null;
            private string? _directory = null;
        }

        internal NowMsgExecShell(uint sessionId, string filename, string? shell, string? directory)
        {
            SessionId = sessionId;
            Filename = filename;
            Shell = shell;
            Directory = directory;
        }

        public uint SessionId { get; }
        public string Filename { get; }
        public string? Shell { get; }
        public string? Directory { get; }
    }
}