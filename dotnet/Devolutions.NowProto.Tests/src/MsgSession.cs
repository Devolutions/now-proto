namespace Devolutions.NowProto.Tests
{
    public class MsgSession
    {
        [Fact]
        public void SessionLock()
        {
            var msg = new NowMsgSessionLock();

            var encoded = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x12, 0x01, 0x00, 0x00, };

            _ = NowTest.MessageRoundtrip(msg, encoded);
        }

        [Fact]
        public void SessionLogoff()
        {
            var msg = new NowMsgSessionLogoff();

            var encoded = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x12, 0x02, 0x00, 0x00, };

            _ = NowTest.MessageRoundtrip(msg, encoded);
        }

        [Fact]
        public void MsgBoxReq()
        {
            var msg = new NowMsgSessionMessageBoxReq.Builder(0x76543210, "hello")
                .WithResponse()
                .Style(NowMsgSessionMessageBoxReq.MessageBoxStyle.AbortRetryIgnore)
                .Title("world")
                .Timeout(TimeSpan.FromSeconds(3))
                .Build();

            var encoded = new byte[]
            {
                0x1A, 0x00, 0x00, 0x00, 0x12, 0x03, 0x0F, 0x00, 0x10, 0x32, 0x54, 0x76,
                0x02, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x05, 0x77, 0x6F, 0x72,
                0x6C, 0x64, 0x00, 0x05, 0x68, 0x65, 0x6C, 0x6C, 0x6F, 0x00
            };

            var decoded = NowTest.MessageRoundtrip(msg, encoded);

            Assert.Equal(msg.RequestId, decoded.RequestId);
            Assert.Equal(msg.Message, decoded.Message);
            Assert.True(decoded.WaitForResponse);
            Assert.Equal(msg.Style, decoded.Style);
            Assert.Equal(msg.Title, decoded.Title);
            Assert.Equal(msg.Timeout, decoded.Timeout);
        }

        [Fact]
        public void MsgBoxReqSimple()
        {
            var msg = new NowMsgSessionMessageBoxReq
                .Builder(0x76543210, "hello")
                .Build();



            var encoded = new byte[]
            {
                0x15, 0x00, 0x00, 0x00, 0x12, 0x03, 0x00, 0x00, 0x10, 0x32, 0x54, 0x76,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x68,
                0x65, 0x6C, 0x6C, 0x6F, 0x00
            };

            var decoded = NowTest.MessageRoundtrip(msg, encoded);

            Assert.Equal(msg.RequestId, decoded.RequestId);
            Assert.Equal(msg.Message, decoded.Message);
            Assert.False(decoded.WaitForResponse);
            Assert.Null(decoded.Style);
            Assert.Null(decoded.Title);
            Assert.Null(decoded.Timeout);
        }

        [Fact]
        public void MsgBoxRsp()
        {
            var msg = NowMsgSessionMessageBoxRsp.Success(
                0x01234567,
                NowMsgSessionMessageBoxRsp.MessageBoxResponse.Retry
            );

            var encoded = new byte[]
            {
                0x12, 0x00, 0x00, 0x00, 0x12, 0x04, 0x00, 0x00, 0x67, 0x45, 0x23, 0x01,
                0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00
            };

            var decoded = NowTest.MessageRoundtrip(msg, encoded);

            Assert.Equal(msg.RequestId, decoded.RequestId);
            Assert.True(msg.IsSuccess);
            Assert.Equal(NowMsgSessionMessageBoxRsp.MessageBoxResponse.Retry, msg.GetResponseOrThrow());
        }

        [Fact]
        public void MsgBoxRspError()
        {
            var msg = NowMsgSessionMessageBoxRsp.Error(
                0x01234567,
                new NowProtocolException(NowProtocolErrorCode.NotImplemented)
            );

            var encoded = new byte[]
            {
                0x12, 0x00, 0x00, 0x00, 0x12, 0x04, 0x00, 0x00, 0x67, 0x45, 0x23, 0x01,
                0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x07, 0x00, 0x00, 0x00,
                0x00, 0x00
            };

            var decoded = NowTest.MessageRoundtrip(msg, encoded);

            Assert.Equal(msg.RequestId, decoded.RequestId);
            Assert.False(msg.IsSuccess);
            Assert.Throws<NowProtocolException>(() => msg.GetResponseOrThrow());
        }

        [Fact]
        public void SetKbdLayoutSpecific()
        {
            var msg = NowMsgSessionSetKbdLayout.Specific("00000409");

            var encoded = new byte[]
            {
                0x0A, 0x00, 0x00, 0x00, 0x12, 0x05, 0x00, 0x00, 0x08, 0x30, 0x30, 0x30,
                0x30, 0x30, 0x34, 0x30, 0x39, 0x00
            };

            var decoded = NowTest.MessageRoundtrip(msg, encoded);

            Assert.Equal(msg.LayoutOption, decoded.LayoutOption);
            Assert.Equal(msg.Layout, decoded.Layout);

            Assert.Equal(NowMsgSessionSetKbdLayout.SetKbdLayoutOption.Specific, msg.LayoutOption);
            Assert.Equal("00000409", msg.Layout);
        }

        [Fact]
        public void SetKbdLayoutNext()
        {
            var msg = NowMsgSessionSetKbdLayout.Next();

            var encoded = new byte[]
            {
                0x02, 0x00, 0x00, 0x00, 0x12, 0x05, 0x01, 0x00, 0x00, 0x00
            };

            var decoded = NowTest.MessageRoundtrip(msg, encoded);

            Assert.Equal(msg.LayoutOption, decoded.LayoutOption);
            Assert.Equal(msg.Layout, decoded.Layout);

            Assert.Equal(NowMsgSessionSetKbdLayout.SetKbdLayoutOption.Next, msg.LayoutOption);
            Assert.Null(msg.Layout);
        }

        [Fact]
        public void SetKbdLayoutPrev()
        {
            var msg = NowMsgSessionSetKbdLayout.Prev();

            var encoded = new byte[]
            {
                0x02, 0x00, 0x00, 0x00, 0x12, 0x05, 0x02, 0x00, 0x00, 0x00
            };

            var decoded = NowTest.MessageRoundtrip(msg, encoded);

            Assert.Equal(msg.LayoutOption, decoded.LayoutOption);
            Assert.Equal(msg.Layout, decoded.Layout);

            Assert.Equal(NowMsgSessionSetKbdLayout.SetKbdLayoutOption.Prev, msg.LayoutOption);
            Assert.Null(msg.Layout);
        }
    }
}