using Devolutions.NowProto.Types;

namespace Devolutions.NowProto.Messages
{
    /// <summary>
    /// The NOW_EXEC_RUN_MSG message is used to send a run request. This request type maps to starting
    /// a program by using the “Run” menu on operating systems (the Start Menu on Windows, the Dock on
    /// macOS etc.). The execution of programs started with NOW_EXEC_RUN_MSG is not followed and does
    /// not send back the output.
    ///
    /// NOW_PROTO: NOW_EXEC_RUN_MSG
    /// </summary>
    public class NowMsgExecRun(uint sessionId, string command) : INowSerialize, INowDeserialize<NowMsgExecRun>
    {
        // -- INowMessage --

        static byte INowMessage.TypeMessageClass => NowMessage.ClassExec;
        static byte INowMessage.TypeMessageKind => 0x10; // NOW-PROTO: NOW_EXEC_RUN_MSG_ID

        byte INowMessage.MessageClass => NowMessage.ClassExec;
        byte INowMessage.MessageKind => 0x10;

        // -- INowSerialize --

        ushort INowSerialize.Flags => 0;
        uint INowSerialize.BodySize => FixedPartSize + NowVarStr.LengthOf(Command);

        void INowSerialize.SerializeBody(NowWriteCursor cursor)
        {
            cursor.EnsureEnoughBytes(FixedPartSize);
            cursor.WriteUint32Le(SessionId);
            cursor.WriteVarStr(Command);
        }

        // -- INowDeserialize --

        static NowMsgExecRun INowDeserialize<NowMsgExecRun>.Deserialize(
            ushort flags,
            NowReadCursor cursor
        )
        {
            cursor.EnsureEnoughBytes(FixedPartSize);
            var sessionId = cursor.ReadUInt32Le();
            var command = cursor.ReadVarStr();

            return new NowMsgExecRun(sessionId, command);
        }

        // -- impl --

        private const uint FixedPartSize = 4; // u32 SessionId

        public uint SessionId { get; } = sessionId;
        public string Command { get; } = command;
    }
}