using Devolutions.NowProto.Types;

namespace Devolutions.NowProto.Messages
{
    /// <summary>
    /// The NOW_EXEC_BATCH_MSG message is used to execute a remote batch command.
    ///
    /// NOW-PROTO: NOW_EXEC_BATCH_MSG
    /// </summary>
    public class NowMsgExecBatch : INowSerialize, INowDeserialize<NowMsgExecBatch>
    {
        // -- INowMessage --

        public static byte TypeMessageClass => NowMessage.ClassExec;
        public static byte TypeMessageKind => 0x13; // NOW-PROTO: NOW_EXEC_BATCH_MSG_ID

        byte INowMessage.MessageClass => NowMessage.ClassExec;
        byte INowMessage.MessageKind => 0x13;

        // -- INowSerialize --

        ushort INowSerialize.Flags => (ushort)(
            (Directory != null ? MsgFlags.DirectorySet : 0) |
            (IoRedirection ? MsgFlags.IoRedirection : 0)
        );
        uint INowSerialize.BodySize =>
            FixedPartSize
            + NowVarStr.LengthOf(Filename)
            + NowVarStr.LengthOf(Directory ?? "");

        void INowSerialize.SerializeBody(NowWriteCursor cursor)
        {
            cursor.EnsureEnoughBytes(FixedPartSize);
            cursor.WriteUint32Le(SessionId);
            cursor.WriteVarStr(Filename);
            cursor.WriteVarStr(Directory ?? "");
        }

        // -- INowDeserialize --
        static NowMsgExecBatch INowDeserialize<NowMsgExecBatch>.Deserialize(ushort flags, NowReadCursor cursor)
        {
            cursor.EnsureEnoughBytes(FixedPartSize);
            var msgFlags = (MsgFlags)flags;
            var sessionId = cursor.ReadUInt32Le();
            var filename = cursor.ReadVarStr();
            var directory = cursor.ReadVarStr();

            return new NowMsgExecBatch(
                sessionId,
                filename,
                msgFlags.HasFlag(MsgFlags.DirectorySet) ? directory : null,
                msgFlags.HasFlag(MsgFlags.IoRedirection)
            );
        }

        // -- impl --

        [Flags]
        private enum MsgFlags : ushort
        {
            /// <summary>
            /// Set if directory field contains non-default value.
            ///
            /// NOW-PROTO: NOW_EXEC_FLAG_BATCH_DIRECTORY_SET
            /// </summary>
            DirectorySet = 0x0001,

            /// <summary>
            /// Enable stdio (stdout, stderr, stdin) redirection.
            ///
            /// NOW-PROTO: NOW_EXEC_FLAG_BATCH_IO_REDIRECTION
            /// </summary>
            IoRedirection = 0x1000,
        }

        private const uint FixedPartSize = 4; // u32 SessionId

        public class Builder(uint sessionId, string filename)
        {
            public Builder Directory(string directory)
            {
                _directory = directory;
                return this;
            }
            public Builder IoRedirection()
            {
                _ioRedirection = true;
                return this;
            }

            public NowMsgExecBatch Build()
            {
                return new NowMsgExecBatch(_sessionId, _filename, _directory, _ioRedirection);
            }

            private readonly uint _sessionId = sessionId;
            private readonly string _filename = filename;
            private string? _directory = null;
            private bool _ioRedirection = false;
        }

        internal NowMsgExecBatch(uint sessionId, string filename, string? directory, bool ioRedirection)
        {
            SessionId = sessionId;
            Filename = filename;
            Directory = directory;
            IoRedirection = ioRedirection;
        }

        public uint SessionId { get; }
        public string Filename { get; }
        public string? Directory { get; }
        public bool IoRedirection { get; }
    }
}