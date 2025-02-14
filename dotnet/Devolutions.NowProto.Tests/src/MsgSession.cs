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

        /*
    let msg = NowSessionMsgBoxRspMsg::new_error(
               0x01234567,
               NowStatusError::from(NowStatusErrorKind::Now(NowProtoError::NotImplemented))
                   .with_message("err")
                   .unwrap(),
           )
           .unwrap();

           let decoded = now_msg_roundtrip(
               msg,
               expect!["[15, 00, 00, 00, 12, 04, 00, 00, 67, 45, 23, 01, 00, 00, 00, 00, 03, 00, 01, 00, 07, 00, 00, 00, 03, 65, 72, 72, 00]"],
           );

           let actual = match decoded {
               NowMessage::Session(NowSessionMessage::MsgBoxRsp(msg)) => msg,
               _ => panic!("Expected NowSessionMsgBoxRspMsg"),
           };

           assert_eq!(actual.request_id(), 0x01234567);
           assert_eq!(
               actual.to_result().unwrap_err(),
               NowStatusError::from(NowStatusErrorKind::Now(NowProtoError::NotImplemented))
                   .with_message("err")
                   .unwrap()
           );

                    /*
                    [TestMethod]
                    public void MsgLockRoundtrip()
                    {
                        var msg = new NowMsgSessionLock();

                        var actualEncoded = new byte[(msg as INowSerialize).Size];
                        {
                            var cursor = new NowWriteCursor(actualEncoded);
                            (msg as INowSerialize).Serialize(cursor);
                        }

                        var expectedEncoded = new byte[]
                        {
                            0x00, 0x00, 0x00, 0x00, 0x12, 0x01, 0x00, 0x00,
                        };

                        CollectionAssert.AreEqual(expectedEncoded, actualEncoded);
                    }

                    [TestMethod]
                    public void MsgLogoff()
                    {
                        var msg = new NowMsgSessionLogoff();

                        var actualEncoded = new byte[(msg as INowSerialize).Size];
                        {
                            var cursor = new NowWriteCursor(actualEncoded);
                            (msg as INowSerialize).Serialize(cursor);
                        }

                        var expectedEncoded = new byte[]
                        {
                            0x00, 0x00, 0x00, 0x00, 0x12, 0x02, 0x00, 0x00,
                        };

                        CollectionAssert.AreEqual(expectedEncoded, actualEncoded);
                    }

                    [TestMethod]
                    public void MsgMessageBoxReq()
                    {
                        var msg = new NowMsgSessionMessageBoxReq(0x76543210, "hello")
                        {
                            WaitForResponse = true,
                            Style = NowMsgSessionMessageBoxReq.MessageBoxStyle.AbortRetryIgnore,
                            Title = "world",
                            Timeout = 3,
                        };

                        var actualEncoded = new byte[(msg as INowSerialize).Size];
                        {
                            var cursor = new NowWriteCursor(actualEncoded);
                            (msg as INowSerialize).Serialize(cursor);
                        }

                        var expectedEncoded = new byte[]
                        {
                            0x1A, 0x00, 0x00, 0x00, 0x12, 0x03, 0x0F, 0x00,
                            0x10, 0x32, 0x54, 0x76, 0x02, 0x00, 0x00, 0x00,
                            0x03, 0x00, 0x00, 0x00, 0x05, 0x77, 0x6F, 0x72,
                            0x6C, 0x64, 0x00, 0x05, 0x68, 0x65, 0x6C, 0x6C,
                            0x6F, 0x00,
                        };

                        CollectionAssert.AreEqual(expectedEncoded, actualEncoded);
                    }

                    [TestMethod]
                    public void MsgMessageBoxRsp()
                    {
                        var encoded = new byte[]
                        {
                            0x08, 0x00, 0x00, 0x00, 0x12, 0x04, 0x00, 0x00,
                            0x67, 0x45, 0x23, 0x01, 0x04, 0x00, 0x00, 0x00,
                        };

                        var msg = NowMessage
                            .Read(new NowReadCursor(encoded))
                            .Deserialize<NowMsgSessionMessageBoxRsp>();

                        Assert.AreEqual((uint)0x01234567, msg.RequestId);
                        Assert.AreEqual(NowMsgSessionMessageBoxRsp.MessageBoxResponse.Retry, msg.Response);
                    }
                    */
    }
}