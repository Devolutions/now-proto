using Devolutions.NowProto;

namespace Devolutions.NowClient
{
    /// <summary>
    /// Transport wrapper with NOW-proto message
    /// serialization/deserialization capabilities.
    /// Includes defragmentation logic to handle partial messages and multiple in-frame messages.
    /// </summary>
    internal class NowChannelTransport(INowTransport transport)
    {
        private readonly NowMessageBuffer _messageBuffer = new();

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
            // Keep reading from transport until we have at least one complete message.
            while (!_messageBuffer.HasCompleteMessage)
            {
                var frame = await transport.Read();
                _messageBuffer.AddData(frame);
            }

            return _messageBuffer.GetNextMessage()!.Value;
        }

        private const int DefaultBufferSize = 1024 * 64; // 64KB Buffer
        private readonly byte[] _writeBuffer = new byte[DefaultBufferSize];
    }
}