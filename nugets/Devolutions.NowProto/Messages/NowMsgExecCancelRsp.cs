using Devolutions.NowProto.Exceptions;
using Devolutions.NowProto.Types;

namespace Devolutions.NowProto.Messages
{
    /// <summary>
    /// The NOW_EXEC_RESULT_MSG message is used to return the result of an execution request.
    ///
    /// NOW_PROTO: NOW_EXEC_RESULT_MSG
    /// </summary>
    public class NowMsgExecCancelRsp : INowSerialize, INowDeserialize<NowMsgExecCancelRsp>
    {
        // -- INowMessage --



        static byte INowMessage.TypeMessageClass => NowMessage.ClassExec;
        static byte INowMessage.TypeMessageKind => 0x03; // NOW-PROTO: NOW_EXEC_RESULT_MSG_ID

        byte INowMessage.MessageClass => NowMessage.ClassExec;
        byte INowMessage.MessageKind => 0x03;

        // -- INowSerialize --
        ushort INowSerialize.Flags => 0;
        uint INowSerialize.BodySize => FixedPartSize + _status.Size;

        void INowSerialize.SerializeBody(NowWriteCursor cursor)
        {
            cursor.EnsureEnoughBytes(FixedPartSize);
            cursor.WriteUint32Le(RequestId);
            _status.Serialize(cursor);
        }

        // -- INowDeserialize --

        static NowMsgExecCancelRsp INowDeserialize<NowMsgExecCancelRsp>.Deserialize(
            ushort flags,
            NowReadCursor cursor
        )
        {
            cursor.EnsureEnoughBytes(FixedPartSize);
            var sessionId = cursor.ReadUInt32Le();
            var status = NowStatus.Deserialize(cursor);

            return new NowMsgExecCancelRsp(sessionId, status);
        }

        // -- impl --

        private const uint FixedPartSize = 4; // u32 requestId

        internal NowMsgExecCancelRsp(uint requestId, NowStatus status)
        {
            RequestId = requestId;
            _status = status;
        }

        public static NowMsgExecCancelRsp Success(uint requestId)
        {
            return new NowMsgExecCancelRsp(requestId, NowStatus.Success());
        }

        public static NowMsgExecCancelRsp Error(uint requestId, NowStatusException exception)
        {
            return new NowMsgExecCancelRsp(
                requestId,
                NowStatus.Error(exception)
            );
        }

        public bool IsSuccess => _status.IsSuccess;

        public void ThrowIfError()
        {
            _status.ThrowIfError();
        }

        public uint RequestId { get; private init; }

        private readonly NowStatus _status;
    }
}