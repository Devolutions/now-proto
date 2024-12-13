using Devolutions.NowProto;

namespace Devolutions.NowClient
{
    /// <summary>
    /// Transport wrapper with NOW-proto message
    /// serialization/deserialization capabilities.
    /// </summary>
    internal class NowChannelTransport(INowTransport transport)
    {
        public async Task WriteMessage(INowSerialize message)
        {
            var cursor = new NowWriteCursor(_writeBuffer);
            message.Serialize(cursor);

            var bytesFilled = checked((int)cursor.BytesFilled);
            await transport.Write(_writeBuffer[0..bytesFilled]);
        }

        public async Task<T> ReadMessage<T>() where T : INowDeserialize<T>
        {
            var message = await ReadMessageAny();
            return message.Deserialize<T>();
        }

        public async Task<NowMessage.NowMessageView> ReadMessageAny()
        {
            var frame = await transport.Read();
            var cursor = new NowReadCursor(frame);

            return NowMessage.Read(cursor);
        }

        private const int DefaultBufferSize = 1024 * 64; // 64KB Buffer
        private readonly byte[] _writeBuffer = new byte[DefaultBufferSize];
    }
}